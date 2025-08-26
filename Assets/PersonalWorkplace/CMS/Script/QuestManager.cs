using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using System;
using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    public Dictionary<string, Quest> activeQuests = new Dictionary<string, Quest>();
    private DatabaseReference dbRef;

    [SerializeField] private string questCSV = "QuestData"; // Resources/QuestData.csv

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

    private DateTime NowUtc() =>
        DateTime.UtcNow.AddMilliseconds(serverTimeOffsetMs);

    private void LoadQuests()
    {
        Debug.Log("CSV 퀘스트 불러오기 시작");

        QuestDatabase.LoadFromCSV(questCSV);

        activeQuests.Clear();
        foreach (var baseQuest in QuestDatabase.AllQuests)
            activeQuests[baseQuest.questID] = baseQuest;

        string userId = BackendManager.Auth.CurrentUser.UserId;
        dbRef.Child("users").Child(userId).Child("quests").Child("quests")
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
            Debug.Log($"퀘스트 데이터 존재, childCount={task.Result.ChildrenCount}");

            foreach (var child in task.Result.Children)
            {
                Debug.Log($"Child key={child.Key}, raw={child.GetRawJsonValue()}");

                Quest quest = JsonUtility.FromJson<Quest>(child.GetRawJsonValue());
                if (quest != null)
                {
                    Debug.Log($"불러온 퀘스트: {quest.questID}, {quest.questName}");
                    if (activeQuests.ContainsKey(quest.questID))
                        activeQuests[quest.questID] = quest;
                    else
                        activeQuests.Add(quest.questID, quest);
                }
                else
                {
                    Debug.LogError($"퀘스트 파싱 실패: {child.GetRawJsonValue()}");
                }
            }

            Debug.Log($"서버 퀘스트 로드 성공, {activeQuests.Count}개 퀘스트 적용됨");
            CheckAndResetQuests();
            IsReady = true;
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

                case QuestCategory.Repeatable:
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
    public void ClaimReward(string questID)
    {
        if (activeQuests.TryGetValue(questID, out Quest quest))
        {
            if (quest.isComplete && !quest.isClaimed)
            {
                quest.isClaimed = true;
                quest.lastUpdated = NowUtc();
                Debug.Log($"보상 수령: {quest.rewardID} x {quest.rewardCount}");
                SaveQuests();
                OnQuestsUpdated?.Invoke();
            }
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

    [System.Serializable]
    private class QuestList
    {
        public List<Quest> quests;
        public QuestList(IEnumerable<Quest> quests) =>
            this.quests = new List<Quest>(quests);
    }
}