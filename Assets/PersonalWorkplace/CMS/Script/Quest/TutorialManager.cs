using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 퀘스트 진행 상태에 따라 튜토리얼 인디케이터(손가락 애니메이션)를 제어하는 매니저
/// </summary>
public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance { get; private set; }

    [Header("인디케이터 프리팹 (손가락 애니메이션)")]
    [SerializeField] private GameObject fingerIndicatorPrefab;

    [Header("인디케이터 부모 오브젝트 (씬 내 고정)")]
    [SerializeField] private Transform indicatorRoot;

    private Dictionary<string, GameObject> indicatorMap = new Dictionary<string, GameObject>();
    private GameObject currentIndicator;

    private readonly List<string> tutorialPriorityOrder = new List<string>
{
    "QT001",
    "QT002",
    "QT003",
    "QT004",
    "QT005",
    "QT006",
    "QT007",
    "QT008",
    "QT009",
    "QT010",
    "QT011"
};

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // QuestManager 이벤트 구독
        QuestManager.Instance.OnQuestsUpdated += OnQuestsUpdated;
        QuestManager.Instance.OnQuestProgressChanged += OnQuestProgressChanged;
    }

    private void OnDestroy()
    {
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestsUpdated -= OnQuestsUpdated;
            QuestManager.Instance.OnQuestProgressChanged -= OnQuestProgressChanged;
        }
    }

    private void OnQuestsUpdated()
    {
        var activeQuests = QuestManager.Instance.activeQuests.Values
            .Where(q => q.state == QuestState.InProgress && q.questType == QuestCategory.Mission)
            .ToList();

        if (activeQuests.Count == 0)
        {
            HideAllIndicators();
            return;
        }

        // 튜토리얼 우선순위 기반 정렬
        Quest nextQuest = null;
        foreach (var questId in tutorialPriorityOrder)
        {
            nextQuest = activeQuests.FirstOrDefault(q => q.questID == questId);
            if (nextQuest != null)
                break;
        }

        if (nextQuest != null)
            ShowIndicatorForQuest(nextQuest);
        else
            HideAllIndicators();
    }


    private void OnQuestProgressChanged(Quest quest)
    {
        if (quest.isComplete)
        {
            HideIndicator(quest.questID);
        }
    }

    private void ShowIndicatorForQuest(Quest quest)
    {
        HideAllIndicators(); // 중복 방지

        if (quest == null || string.IsNullOrEmpty(quest.questID))
            return;

        Transform target = FindTargetTransformByQuestTarget(quest.questTarget);
        if (target == null)
        {
            Debug.LogWarning($"[TutorialManager] '{quest.questTarget}' 대상 UI를 찾을 수 없습니다.");
            return;
        }

        if (!indicatorMap.TryGetValue(quest.questID, out var indicator))
        {
            indicator = Instantiate(fingerIndicatorPrefab, indicatorRoot);
            indicator.name = $"Indicator_{quest.questID}";
            indicatorMap[quest.questID] = indicator;
        }

        indicator.transform.SetParent(target, false);
        indicator.transform.localPosition = Vector3.zero;
        indicator.SetActive(true);
        currentIndicator = indicator;

        Debug.Log($"[TutorialManager] 인디케이터 표시: {quest.questName} ({quest.questTarget})");
    }

    /// <summary>
    /// QuestTargetType 값에 따라 실제 UI 오브젝트 위치를 찾음
    /// </summary>
    private Transform FindTargetTransformByQuestTarget(QuestTargetType targetType)
    {
        // 실제 UI 구조에 맞게 연결해야 하는 부분
        switch (targetType)
        {
            case QuestTargetType.Training:
            case QuestTargetType.ExtPow:
                return GameObject.Find("UI/GrowthTab/ExtPowerButton")?.transform;

            case QuestTargetType.InnPow:
                return GameObject.Find("UI/GrowthTab/InnPowerButton")?.transform;

            case QuestTargetType.Vital:
                return GameObject.Find("UI/GrowthTab/VitalTrainingButton")?.transform;

            case QuestTargetType.Enhance:
                return GameObject.Find("UI/EquipTab/EnhanceButton")?.transform;

            case QuestTargetType.Growth:
                return GameObject.Find("UI/CharacterTab/GrowthButton")?.transform;

            case QuestTargetType.Stage:
                return GameObject.Find("UI/StageTab/EnterStageButton")?.transform;

            default:
                return null;
        }
    }

    private void HideIndicator(string questID)
    {
        if (indicatorMap.TryGetValue(questID, out var indicator))
        {
            indicator.SetActive(false);
            Debug.Log($"[TutorialManager] 인디케이터 숨김: {questID}");
        }
    }

    private void HideAllIndicators()
    {
        foreach (var indicator in indicatorMap.Values)
            indicator.SetActive(false);

        currentIndicator = null;
    }
}