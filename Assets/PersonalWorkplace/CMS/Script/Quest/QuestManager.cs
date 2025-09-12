using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    public Dictionary<string, Quest> activeQuests = new Dictionary<string, Quest>();
    private DatabaseReference dbRef;

    [SerializeField] private string questCSV = "QuestData"; // Resources/QuestData.csv

    public Quest SelectedQuest { get; private set; }
    public bool IsReady { get; private set; } = false;

    private float playtimeBuffer = 0f;    // 누적 시간(초)
    private float saveInterval = 30f;     // 30초마다 Firebase 저장
    private float saveTimer = 0f;

    //UI 갱신용 이벤트
    public event Action OnQuestsUpdated;

    // Firebase 서버 시간 오프셋(ms)
    private long serverTimeOffsetMs = 0;

    [Inject] private ICurrencyModel _currencyModel;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    private void Update()
    {
        // 아직 초기화 전이면 아무것도 하지 않음
        if (!IsReady) return;

        // --- Playtime 진행도 누적 ---
        playtimeBuffer += Time.deltaTime;
        if (playtimeBuffer >= 1f)
        {
            int seconds = Mathf.FloorToInt(playtimeBuffer);
            playtimeBuffer -= seconds;

            // 진행도는 바로 업데이트
            ReportEvent(QuestTargetType.Playtime, seconds, saveImmediately: false);
        }

        // --- 주기적 저장 ---
        saveTimer += Time.deltaTime;
        if (saveTimer >= saveInterval)
        {
            saveTimer = 0f;
            SaveQuests();
        }
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

                // 로그인 퀘스트 진행도 보고
                ReportEvent(QuestTargetType.Onlogin);
            });
        }
        else
        {
            Debug.LogError("BackendManager 준비 안됨");
        }
    }
    private void HandleLoginQuest()
    {
        DateTime now = NowUtc();

        // 오늘 날짜 기준으로 이미 카운트했는지 체크
        foreach (var quest in activeQuests.Values)
        {
            if (quest.questTarget == QuestTargetType.Onlogin && quest.questType == QuestCategory.Daily)
            {
                if (quest.lastUpdated.Date < now.Date)
                {
                    // 오늘은 아직 로그인 보상 미처리, 카운트 증가
                    ReportEvent(QuestTargetType.Onlogin);
                    quest.lastUpdated = now; // 오늘 처리한 걸로 갱신
                    SaveQuests();
                }
                else
                {
                    Debug.Log("오늘 이미 로그인 퀘스트 처리됨");
                }
            }
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
        foreach (var quest in QuestDatabase.DailyQuests
            .Concat(QuestDatabase.WeeklyQuests)
            .Concat(QuestDatabase.RepeatQuests)
            .Concat(QuestDatabase.MissionQuests))
        {
            activeQuests[quest.questID] = quest;
        }

        Debug.Log($"[로드 직후] activeQuests = {activeQuests.Count}개");

        string userId = BackendManager.Auth.CurrentUser.UserId;
        dbRef.Child("users").Child(userId).Child("quests")
        .GetValueAsync().ContinueWithOnMainThread(task =>
        {
            Debug.Log("Firebase 응답 도착");

            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogWarning("서버 진행상황 불러오기 실패, CSV 기준으로 사용");
                IsReady = true;
                OnQuestsUpdated?.Invoke();
                return;
            }

            if (!task.Result.Exists)
            {
                Debug.Log("서버 진행상황 없음, CSV 기준 초기 저장");
                SaveQuests(); // 여기서만 서버에 올림
                IsReady = true;
                OnQuestsUpdated?.Invoke();
                return;
            }

            // 서버 진행상황 반영
            var wrapper = JsonUtility.FromJson<SerializationWrapper<QuestProgressData>>(task.Result.GetRawJsonValue());
            var dict = wrapper.ToDictionary();

            foreach (var kvp in dict)
            {
                if (activeQuests.TryGetValue(kvp.Key, out var localQuest))
                {
                    var progress = kvp.Value;
                    localQuest.valueProgress = progress.progress;
                    localQuest.isComplete = progress.isComplete;
                    localQuest.isClaimed = progress.isClaimed;
                    localQuest.lastUpdated = new DateTime(progress.lastUpdated, DateTimeKind.Utc);
                    localQuest.lastWeek = progress.lastWeek;
                }
            }

            // 해금 체크
            TryUnlockQuests();

            CheckAndResetQuests();

            IsReady = true;
            OnQuestsUpdated?.Invoke();
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
        quest.state = QuestState.RewardReady;
        quest.lastUpdated = NowUtc();
        Debug.Log($"퀘스트 완료: {quest.questName}");
        SaveQuests();
        OnQuestsUpdated?.Invoke();
    }

    // 보상 수령
    public void ClaimReward(Quest quest)
    {
        if (quest == null || quest.state != QuestState.RewardReady)
        {
            Debug.LogWarning("보상을 수령할 수 없는 퀘스트입니다.");
            return;
        }

        quest.ClaimReward();

        Debug.Log($"퀘스트 보상 수령 완료: {quest.questName}");

        // 반복 퀘스트가 아니라면, 보상 수령 후 비활성화
        if (quest.questType != QuestCategory.Repeat)
        {
            quest.state = QuestState.Disabled;
        }
        else
        {
            // 반복 퀘스트라면 다시 진행상태로 리셋
            quest.ResetProgress();
        }

        SaveQuests();
        OnQuestsUpdated?.Invoke();
    }

    // 보상 지급 로직
    public void GrantReward(Reward reward)
    {
        var currencyModel = CurrencyManager.Instance.Model;
        currencyModel.Add(reward.currencyType, new BigCurrency(reward.rewardCount, 0));
    }

    // Firebase 저장
    private void SaveQuests()
    {
        if (BackendManager.Auth?.CurrentUser == null || dbRef == null)
        {
            Debug.LogWarning("[QuestManager] SaveQuests 호출 시점에 Auth/DB 준비 안 됨");
            return;
        }

        string userId = BackendManager.Auth.CurrentUser.UserId;

        Dictionary<string, QuestProgressData> saveData = new Dictionary<string, QuestProgressData>();
        foreach (var quest in activeQuests.Values)
            saveData[quest.questID] = new QuestProgressData(quest);

        string json = JsonUtility.ToJson(new SerializationWrapper<QuestProgressData>(saveData));
        dbRef.Child("users").Child(userId).Child("quests").SetRawJsonValueAsync(json);

        Debug.Log($"[QuestManager] 퀘스트 저장 완료: {saveData.Count}개");
    }

    // JSON 직렬화를 위한 래퍼
    [System.Serializable]
    private class SerializationWrapper<T>
    {
        public List<string> keys;
        public List<T> values;

        public SerializationWrapper(Dictionary<string, T> dict)
        {
            keys = new List<string>(dict.Keys);
            values = new List<T>(dict.Values);
        }

        public Dictionary<string, T> ToDictionary()
        {
            var dict = new Dictionary<string, T>();
            for (int i = 0; i < keys.Count; i++)
                dict[keys[i]] = values[i];
            return dict;
        }
    }

    // 주차 계산
    private int GetCurrentWeek(DateTime timeUtc)
    {
        var cal = System.Globalization.CultureInfo.InvariantCulture.Calendar;
        return cal.GetWeekOfYear(timeUtc, System.Globalization.CalendarWeekRule.FirstDay, DayOfWeek.Monday);
    }

    public List<Quest> GetQuestsByCategory(QuestCategory type)
    {
        var list = activeQuests.Values.Where(q => q.questType == type).ToList();
        Debug.Log($"[GetQuestsByCategory] {type}, {list.Count}개 반환");
        return list;
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
    public void ReportEvent(QuestTargetType type, int amount = 1, bool saveImmediately = true)
    {
        var matchedQuests = activeQuests.Values
            .Where(q => q.questTarget == type && q.state == QuestState.InProgress);

        foreach (var quest in matchedQuests)
        {
            quest.valueProgress += amount;
            if (quest.valueProgress >= quest.valueGoal)
            {
                quest.valueProgress = quest.valueGoal;
                CompleteQuest(quest);
            }
            else
            {
                quest.lastUpdated = NowUtc();
                if (saveImmediately) SaveQuests(); // 필요 시만 즉시 저장
                OnQuestsUpdated?.Invoke();
                Debug.Log($"[ReportEvent] {type} → {quest.questName}: {quest.valueProgress}/{quest.valueGoal}");
            }
        }
    }

    [System.Serializable]
    private class QuestList
    {
        public List<Quest> quests;
        public QuestList(IEnumerable<Quest> quests) =>
            this.quests = new List<Quest>(quests);
    }
    // 서버에 올릴 진행상황 전용 데이터
    [System.Serializable]
    private class QuestProgressData
    {
        public int progress;
        public bool isComplete;
        public bool isClaimed;
        public long lastUpdated;
        public int lastWeek;

        public QuestProgressData(Quest quest)
        {
            progress = quest.valueProgress;
            isComplete = quest.isComplete;
            isClaimed = quest.isClaimed;
            lastUpdated = quest.lastUpdated.ToUniversalTime().Ticks;
            lastWeek = quest.lastWeek;
        }
    }
    public List<Quest> GetActiveQuestsForHUD()
    {
        // Mission 퀘스트 우선
        var missionQuests = activeQuests.Values
            .Where(q => q.questType == QuestCategory.Mission
                     && q.state == QuestState.InProgress
                     && !q.isComplete)
            .OrderBy(q => q.requiredStage)
            .ToList();

        if (missionQuests.Count > 0)
            return missionQuests.Take(1).ToList();

        // Mission 퀘스트 없으면 반복 퀘스트 표시
        var repeatQuests = activeQuests.Values
            .Where(q => q.questType == QuestCategory.Repeat
                     && q.state == QuestState.InProgress
                     && !q.isComplete)
            .OrderBy(q => q.questID)
            .ToList();

        if (repeatQuests.Count > 0)
            return repeatQuests.Take(1).ToList();

        // 아무것도 없으면 빈 리스트 반환
        return new List<Quest>();
    }

    private int GetQuestPriority(QuestCategory category)
    {
        return category switch
        {
            QuestCategory.Mission => 0,  // 최우선
            QuestCategory.Daily => 1,
            QuestCategory.Weekly => 2,
            QuestCategory.Repeat => 3,
            _ => 99
        };
    }
    public bool CheckUnlockCondition(Quest quest)
    {
        if (quest.questType != QuestCategory.Mission) return true;
        if (quest.state == QuestState.Disabled) return false;

        if (quest.requiredStage > 0 && PlayerDataManager.Instance.ClearedStage < quest.requiredStage)
            return false;

        return true;
    }
    public void NotifyQuestsUpdated()
    {
        OnQuestsUpdated?.Invoke();
    }
    public void TryUnlockQuests()
    {
        bool needsUpdate = false;
        foreach (var quest in activeQuests.Values)
        {
            if (quest.questType != QuestCategory.Mission) continue;

            if (quest.state == QuestState.Locked && CheckUnlockCondition(quest))
            {
                quest.state = QuestState.InProgress;
                Debug.Log($"[TryUnlockQuests] Mission 해금: {quest.questName}");
                needsUpdate = true;
            }

            if (needsUpdate)
            {
                OnQuestsUpdated?.Invoke();
            }
        }
    }
    public Quest GetQuestToDisplayOnHUD()
    {
        // 1순위: 진행 중이거나 보상 수령 가능한 '미션' 퀘스트
        var missionQuest = activeQuests.Values
            .FirstOrDefault(q => q.questType == QuestCategory.Mission &&
                                   (q.state == QuestState.InProgress || q.state == QuestState.RewardReady));
        if (missionQuest != null)
        {
            return missionQuest;
        }

        // 2순위: 보상 수령 가능한 '반복' 퀘스트
        var repeatableRewardReadyQuest = activeQuests.Values
            .FirstOrDefault(q => q.questType == QuestCategory.Repeat && q.state == QuestState.RewardReady);
        if (repeatableRewardReadyQuest != null)
        {
            return repeatableRewardReadyQuest;
        }

        // 3순위: 진행 중인 '반복' 퀘스트
        var repeatableInProgressQuest = activeQuests.Values
            .FirstOrDefault(q => q.questType == QuestCategory.Repeat && q.state == QuestState.InProgress);
        if (repeatableInProgressQuest != null)
        {
            return repeatableInProgressQuest;
        }

        // 4순위: 그 외 표시할 퀘스트가 없다면 null 반환
        return null;
    }
}