using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    public Dictionary<string, Quest> activeQuests = new Dictionary<string, Quest>();
    private DatabaseReference dbRef;

    [SerializeField] private string questCSV = "QuestData"; // Resources/QuestData.csv

    public Quest SelectedQuest { get; private set; }
    public bool IsReady { get; private set; } = false;

    //UI 갱신용 이벤트
    public event Action OnQuestsUpdated;

    // Firebase 서버 시간 오프셋(ms)
    private long serverTimeOffsetMs = 0;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    public void InitializeAfterLogin()
    {
        Debug.Log("QuestManager 로그인 이후 초기화 시작");

        if (BackendManager.Auth != null && BackendManager.Database != null)
        {
            dbRef = BackendManager.Database.RootReference;

            FetchServerTimeOffset(() =>
            {
                LoadQuests();
            });
        }
        else
        {
            Debug.LogError("BackendManager 준비 안됨");
        }
    }
    private void FetchServerTimeOffset(Action onDone)
    {
        BackendManager.Database.GetReference(".info/serverTimeOffset")
            .GetValueAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted && task.Result != null && task.Result.Value != null)
                {
                    try
                    {
                        serverTimeOffsetMs = Convert.ToInt64(task.Result.Value);
                    }
                    catch { serverTimeOffsetMs = 0; }
                }
                onDone?.Invoke();
            });
    }

    public DateTime NowUtc() =>
        DateTime.UtcNow.AddMilliseconds(serverTimeOffsetMs);

    private void LoadQuests()
    {
        Debug.Log("CSV 퀘스트 불러오기 시작");

        QuestDatabase.LoadAll();

        activeQuests.Clear();
        foreach (var quest in QuestDatabase.DailyQuests.Concat(QuestDatabase.WeeklyQuests).Concat(QuestDatabase.RepeatQuests))
            activeQuests[quest.questID] = quest;

        string userId = BackendManager.Auth.CurrentUser.UserId;
        dbRef.Child("users").Child(userId).Child("quests")
            .GetValueAsync().ContinueWithOnMainThread(task =>
            {
                Debug.Log("Firebase 응답 도착");

                if (task.IsFaulted || task.IsCanceled)
                {
                    Debug.LogError("서버 퀘스트 로드 실패, CSV 데이터만 사용");
                    IsReady = true;
                    OnQuestsUpdated?.Invoke();
                    return;
                }

                if (task.Result.Exists)
                {
                    Debug.Log($"퀘스트 데이터 존재, {task.Result.GetRawJsonValue()}");

                    QuestList wrapper = JsonUtility.FromJson<QuestList>(task.Result.GetRawJsonValue());
                    if (wrapper != null && wrapper.quests != null)
                    {
                        foreach (var quest in wrapper.quests)
                        {
                            if (activeQuests.ContainsKey(quest.questID))
                                activeQuests[quest.questID] = quest;
                            else
                                activeQuests.Add(quest.questID, quest);

                            Debug.Log($"불러온 퀘스트: {quest.questID}, {quest.questName}");
                        }
                    }

                    Debug.Log($"서버 퀘스트 로드 성공, {activeQuests.Count}개 퀘스트 적용됨");
                    CheckAndResetQuests();
                    IsReady = true;
                    OnQuestsUpdated?.Invoke();
                }
                else
                {
                    Debug.Log("서버 데이터 없음, CSV 데이터 저장");
                    SaveQuests();
                    IsReady = true;
                    OnQuestsUpdated?.Invoke();
                }
            });
    }

    // 퀘스트 자동 리셋 체크 (서버 시간 기준)
    private void CheckAndResetQuests()
    {
        DateTime now = NowUtc();
        int currentWeek = GetCurrentWeek(now);

        foreach (var quest in activeQuests.Values)
        {
            switch (quest.questType)
            {
                case QuestCategory.Daily:
                    // 날짜만 비교
                    if (quest.lastUpdated.Date < now.Date)
                    {
                        quest.ResetProgress();
                        quest.lastUpdated = now;
                        Debug.Log($"일일 퀘스트 리셋: {quest.questName}");
                    }
                    break;

                case QuestCategory.Weekly:
                    if (quest.lastWeek < currentWeek)
                    {
                        quest.ResetProgress();
                        quest.lastWeek = currentWeek;
                        quest.lastUpdated = now;
                        Debug.Log($"주간 퀘스트 리셋: {quest.questName}");
                    }
                    break;

                case QuestCategory.Repeat:
                    break;
            }
        }

        SaveQuests();
        OnQuestsUpdated?.Invoke();
    }

    // 퀘스트 시작
    public void StartQuest(string questId)
    {
        if (activeQuests.TryGetValue(questId, out Quest quest))
        {
            if (quest.valueProgress == 0 && !quest.isComplete)
            {
                Debug.Log($"퀘스트 시작: {quest.questName}");
                quest.lastUpdated = NowUtc();
                SaveQuests();
                OnQuestsUpdated?.Invoke();
            }
        }
    }

    // 진행 업데이트
    public void UpdateQuest(string questId, int amount = 1)
    {
        if (activeQuests.TryGetValue(questId, out Quest quest))
        {
            if (quest.isComplete) return;

            quest.valueProgress += amount;
            if (quest.valueProgress >= quest.valueGoal)
            {
                quest.valueProgress = quest.valueGoal;
                CompleteQuest(quest);
            }
            else
            {
                quest.lastUpdated = NowUtc();
                SaveQuests();
                OnQuestsUpdated?.Invoke();
                Debug.Log($"퀘스트 진행: {quest.questName} {quest.valueProgress}/{quest.valueGoal}");
            }
        }
    }

    // 완료 처리
    private void CompleteQuest(Quest quest)
    {
        quest.isComplete = true;
        quest.lastUpdated = NowUtc();
        Debug.Log($"퀘스트 완료: {quest.questName}");
        SaveQuests();
        OnQuestsUpdated?.Invoke();
    }

    // 보상 수령
    public void ClaimReward(string questId)
    {
        if (!activeQuests.TryGetValue(questId, out Quest quest)) return;
        if (!quest.isComplete || quest.isClaimed) return;

        foreach (var reward in quest.rewards)
        {
            GrantReward(reward);
        }

        quest.isClaimed = true;
        Debug.Log($"퀘스트 보상 수령 완료: {quest.questName}");
    }

    // 보상 지급 로직
    private void GrantReward(Reward reward)
    {
        switch (reward.rewardType)
        {
            case RewardType.Currency:
                // 재화 증가 로직
                break;
            case RewardType.Equipment:
                // 장비 지급 로직
                break;
            case RewardType.Item:
                // 아이템 지급 로직
                break;
        }
    }

    // Firebase 저장
    private void SaveQuests()
    {
        string userId = BackendManager.Auth.CurrentUser.UserId;
        QuestList wrapper = new QuestList(activeQuests.Values);
        string json = JsonUtility.ToJson(wrapper);
        dbRef.Child("users").Child(userId).Child("quests").SetRawJsonValueAsync(json);
    }

    // 주차 계산
    private int GetCurrentWeek(DateTime timeUtc)
    {
        var cal = System.Globalization.CultureInfo.InvariantCulture.Calendar;
        return cal.GetWeekOfYear(timeUtc, System.Globalization.CalendarWeekRule.FirstDay, DayOfWeek.Monday);
    }

    public List<Quest> GetQuestsByCategory(QuestCategory type)
    {
        return activeQuests.Values
            .Where(q => q.questType == type)
            .ToList();
    }
    public string GetRemainingTimeFormatted(QuestCategory category)
    {
        DateTime now = NowUtc();
        TimeSpan remain = TimeSpan.Zero;

        switch (category)
        {
            case QuestCategory.Daily:
                // 오늘 자정까지 남은 시간
                DateTime nextDay = now.Date.AddDays(1);
                remain = nextDay - now;
                break;

            case QuestCategory.Weekly:
                // 이번 주 월요일 기준, 다음 주 월요일 0시까지
                int daysUntilNextMonday = ((int)DayOfWeek.Monday - (int)now.DayOfWeek + 7) % 7;
                if (daysUntilNextMonday == 0) daysUntilNextMonday = 7; // 이번 주가 끝난 경우
                DateTime nextWeek = now.Date.AddDays(daysUntilNextMonday);
                remain = nextWeek - now;
                break;

            case QuestCategory.Repeat:
                remain = TimeSpan.Zero; // 제한 없음
                break;
        }

        if (remain < TimeSpan.Zero) remain = TimeSpan.Zero;

        return $"{remain.Days}일 {remain.Hours:D2}:{remain.Minutes:D2}:{remain.Seconds:D2}";
    }
    public void SetSelectedQuest(Quest quest)
    {
        SelectedQuest = quest;
    }

    [System.Serializable]
    private class QuestList
    {
        public List<Quest> quests;
        public QuestList(IEnumerable<Quest> quests) =>
            this.quests = new List<Quest>(quests);
    }
}