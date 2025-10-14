using Firebase.Database;
using Firebase.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VContainer;

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
    public event Action<Quest> OnQuestProgressChanged;

    // Firebase 서버 시간 오프셋(ms)
    private long serverTimeOffsetMs = 0;

    [Inject] private ICurrencyModel _currencyModel;

    private void Awake()
    {
        Debug.Log("[QuestManager] Awake 호출됨");
        if (Instance == null)
        {
            Instance = this;
            Debug.Log("[QuestManager] Instance 등록 완료");
        }
        else
        {
            Debug.LogWarning("[QuestManager] 중복 인스턴스 발견 → 파괴됨");
            Destroy(gameObject);
        }
    }
    private void Update()
    {
        // 아직 초기화 전이면 아무것도 하지 않음
        if (!IsReady)
        {
            Debug.Log("[QuestManager.Update] 아직 준비 안됨 (IsReady=false)");
            return;
        }

        // --- Playtime 진행도 누적 ---
        playtimeBuffer += Time.deltaTime;
        if (playtimeBuffer >= 1f)
        {
            Debug.Log($"[QuestManager.Update] playtimeBuffer={playtimeBuffer}");
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

    private IEnumerator Start()
    {
        Debug.Log("[QuestManager] Start() 실행 대기 중...");
        yield return new WaitUntil(() => BackendManager.Auth != null);
        Debug.Log("[QuestManager] Auth 준비됨");

        if (BackendManager.Auth.CurrentUser != null)
        {
            Debug.Log("[QuestManager] 이미 로그인된 유저 존재 → OnBackendLoginSuccess()");
            OnBackendLoginSuccess();
        }
        else
        {
            Debug.Log("[QuestManager] 로그인 대기 → OnLoginSuccess 이벤트 등록");
            BackendManager.Instance.OnLoginSuccess += OnBackendLoginSuccess;
        }
    }

    private void OnBackendLoginSuccess()
    {
        Debug.Log("[QuestManager] OnBackendLoginSuccess fired → InitializeAfterLogin()");
        InitializeAfterLogin();
    }
    public void InitializeAfterLogin()
    {
        Debug.Log("[QuestManager] InitializeAfterLogin() 실행 시작");
        IsReady = false;

        if (BackendManager.Database == null)
        {
            Debug.LogError("[QuestManager] BackendManager.Database == null");
            return;
        }

        dbRef = BackendManager.Database.RootReference;
        Debug.Log("[QuestManager] Firebase Database 레퍼런스 연결 성공");

        FetchServerTimeOffset(() =>
        {
            Debug.Log("[QuestManager] 서버 시간 오프셋 적용 완료 → LoadQuests 실행");
            LoadQuests();

            // 로그인 퀘스트
            HandleLoginQuest();
        });
    }

    private void HandleLoginQuest()
    {
        Debug.Log("[QuestManager] HandleLoginQuest() 실행");
        DateTime now = NowUtc().Date;
        foreach (var quest in activeQuests.Values)
        {
            if (quest.questTarget == QuestTargetType.OnLogin && quest.questType == QuestCategory.Daily)
            {
                if (quest.lastUpdated.Date < now)
                {
                    Debug.Log($"[QuestManager] 오늘 첫 로그인 퀘스트 처리: {quest.questName}");
                    quest.ResetProgress();
                    quest.lastUpdated = now;
                    ReportEvent(QuestTargetType.OnLogin, 1);
                    SaveQuests();
                }
                else
                {
                    Debug.Log($"[QuestManager] 오늘 이미 로그인 처리됨: {quest.questName}");
                }
            }
        }
    }
    private void FetchServerTimeOffset(Action onDone)
    {
        Debug.Log("[QuestManager] FetchServerTimeOffset 시작");
        BackendManager.Database.GetReference(".info/serverTimeOffset")
            .GetValueAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    Debug.LogWarning("⚠[QuestManager] 서버 시간 오프셋 불러오기 실패");
                }
                else
                {
                    try
                    {
                        serverTimeOffsetMs = Convert.ToInt64(task.Result.Value);
                        Debug.Log($"[QuestManager] 서버 오프셋: {serverTimeOffsetMs}ms");
                    }
                    catch
                    {
                        Debug.LogWarning("⚠️ [QuestManager] 서버 오프셋 파싱 실패");
                        serverTimeOffsetMs = 0;
                    }
                }
                Debug.Log("[QuestManager] FetchServerTimeOffset 완료 → onDone 호출");
                onDone?.Invoke();
            });
    }

    public DateTime NowUtc() =>
        DateTime.UtcNow.AddMilliseconds(serverTimeOffsetMs);

    private void LoadQuests()
    {
        Debug.Log("[QuestManager] LoadQuests() 시작");

        if (BackendManager.Auth?.CurrentUser == null)
        {
            Debug.LogError("[QuestManager] Auth.CurrentUser가 null → LoadQuests 중단");
            return;
        }

        QuestDatabase.LoadAll();
        Debug.Log("[QuestManager] QuestDatabase.LoadAll() 완료");

        activeQuests.Clear();
        foreach (var quest in QuestDatabase.DailyQuests
            .Concat(QuestDatabase.WeeklyQuests)
            .Concat(QuestDatabase.RepeatQuests)
            .Concat(QuestDatabase.MissionQuests))
        {
            activeQuests[quest.questID] = quest;
        }
        Debug.Log($"[QuestManager] 총 퀘스트 로드 완료: {activeQuests.Count}개");

        string userId = BackendManager.Auth.CurrentUser.UserId;
        dbRef.Child("players").Child(userId).Child("quests")
        .GetValueAsync().ContinueWithOnMainThread(task =>
        {
            Debug.Log("[QuestManager] Firebase 퀘스트 데이터 요청 완료");

            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogWarning("[QuestManager] 서버 진행상황 불러오기 실패, CSV 기준 사용");
                IsReady = true;
                OnQuestsUpdated?.Invoke();
                return;
            }

            if (!task.Result.Exists)
            {
                Debug.Log("[QuestManager] 서버 데이터 없음 → CSV 기준 초기 저장");
                SaveQuests();
                IsReady = true;
                OnQuestsUpdated?.Invoke();
                return;
            }

            var wrapper = JsonUtility.FromJson<SerializationWrapper<QuestProgressData>>(task.Result.GetRawJsonValue());
            var dict = wrapper.ToDictionary();
            Debug.Log($"[QuestManager] 서버에서 받은 진행 데이터 {dict.Count}개 반영 시작");

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

            IsReady = true;
            Debug.Log("[QuestManager] 퀘스트 로드 완료 → IsReady = true");
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

        // 실제 보상 지급
        foreach (var reward in quest.rewards)
        {
            GrantReward(reward);
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
        PopupManager.Instance.ShowMissionClearPanel(quest.questName);
    }

    // 보상 지급 로직
    public void GrantReward(Reward reward)
    {
        switch (reward.rewardType)
        {
            case RewardType.Currency:
                if (reward.currencyType.HasValue)
                {
                    var currencyModel = CurrencyManager.Instance.Model;
                    currencyModel.Add(reward.currencyType.Value, new BigCurrency(reward.rewardCount, 0));
                    Debug.Log($"[보상 지급] {reward.currencyType.Value} +{reward.rewardCount} 지급 완료!");
                }
                else
                {
                    Debug.LogWarning("[보상 지급] CurrencyType이 지정되지 않았습니다. 지급을 건너뜁니다.");
                }
                break;

            case RewardType.Equipment:
                Debug.LogWarning($"[보상 지급] 장비 지급 로직 필요, {reward.rewardID} x{reward.rewardCount}");
                break;

            case RewardType.Item:
                if (!InventoryManager.Instance.IsInitialized)
                {
                    Debug.LogError("[보상 지급] 인벤토리가 초기화되지 않음!");
                    return;
                }

                InventoryManager.Instance.Add(reward.rewardID, reward.rewardCount);
                Debug.Log($"[보상 지급] 아이템 {reward.rewardID} x{reward.rewardCount} 지급 완료! 현재 보유: {InventoryManager.Instance.Get(reward.rewardID)}");
                break;

            default:
                Debug.LogWarning($"알 수 없는 보상 타입: {reward.rewardType}");
                break;
        }
    }

    // Firebase 저장
    private void SaveQuests()
    {
        if (BackendManager.Auth?.CurrentUser == null || dbRef == null)
        {
            Debug.LogWarning("[QuestManager.SaveQuests] Auth/DB 준비 안됨");
            return;
        }

        string userId = BackendManager.Auth.CurrentUser.UserId;
        Dictionary<string, QuestProgressData> saveData = new Dictionary<string, QuestProgressData>();
        foreach (var quest in activeQuests.Values)
            saveData[quest.questID] = new QuestProgressData(quest);

        string json = JsonUtility.ToJson(new SerializationWrapper<QuestProgressData>(saveData));
        dbRef.Child("players").Child(userId).Child("quests")
            .SetRawJsonValueAsync(json);

        Debug.Log($"[QuestManager] 퀘스트 저장 완료 ({saveData.Count}개)");
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
        #if UNITY_EDITOR
        Debug.Log($"[ReportEvent 호출] type={type}, amount={amount}");
        #endif

        var matchedQuests = activeQuests.Values
            .Where(q => q.questTarget == type && q.state == QuestState.InProgress)
            .ToList();

        // 매칭 퀘스트가 없으면 조용히 리턴
        if (matchedQuests.Count == 0)
            return;

        foreach (var quest in matchedQuests)
        {
            int prevProgress = quest.valueProgress; // 이전 값 저장

            quest.valueProgress += amount;

            // 목표 초과 방지
            if (quest.valueProgress >= quest.valueGoal)
            {
                quest.valueProgress = quest.valueGoal;
                CompleteQuest(quest);

                #if UNITY_EDITOR
                Debug.Log($"[ReportEvent] {quest.questName} 완료! ({quest.valueProgress}/{quest.valueGoal})");
                #endif
            }
            else if (quest.valueProgress != prevProgress) // 실제 진행도 변했을 때만 로그
            {
                quest.lastUpdated = NowUtc();
                if (saveImmediately) SaveQuests();

                OnQuestProgressChanged?.Invoke(quest);

                #if UNITY_EDITOR
                Debug.Log($"[ReportEvent] {quest.questName} 진행도 업데이트: {prevProgress} → {quest.valueProgress}/{quest.valueGoal}");
                #endif
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
            if (quest.questType != QuestCategory.Mission)
                continue;

            // 해금 조건 통과 + 잠금 상태일 때만 처리
            if (quest.state == QuestState.Locked && CheckUnlockCondition(quest))
            {
                quest.state = QuestState.InProgress;
                Debug.Log($"[TryUnlockQuests] Mission 해금: {quest.questName}");
                needsUpdate = true;

                // 튜토리얼 실행 연결
                if (TutorialManager.Instance != null)
                {
                    TutorialManager.Instance.StartTutorial(quest.questID);
                    Debug.Log($"[튜토리얼 시작 호출] {quest.questID}");
                }
                else
                {
                    Debug.LogWarning("[튜토리얼] TutorialManager 인스턴스가 존재하지 않습니다.");
                }
            }
        }

        if (needsUpdate)
        {
            OnQuestsUpdated?.Invoke();
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