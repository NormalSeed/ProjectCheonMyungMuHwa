using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class QuestUIManager : UIBase
{
    [Header("상단 UI")]
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private TextMeshProUGUI timeText;   // 남은 시간 표시

    [Header("퀘스트 리스트")]
    [SerializeField] private GameObject questUIPrefab;
    [SerializeField] private Transform contentParent;

    [Header("탭 버튼")]
    [SerializeField] private Toggle dailyToggle;
    [SerializeField] private Toggle weeklyToggle;
    [SerializeField] private Toggle repeatToggle;

    [Header("기타 버튼")]
    [SerializeField] private Button exitButton;

    private Coroutine waitRoutine;
    private QuestCategory currentCategory = QuestCategory.Daily; // 기본 Daily

    private void Start()
    {
        // 토글 이벤트 등록
        dailyToggle.onValueChanged.AddListener((isOn) => { if (isOn) SetActiveCategory(QuestCategory.Daily); });
        weeklyToggle.onValueChanged.AddListener((isOn) => { if (isOn) SetActiveCategory(QuestCategory.Weekly); });
        repeatToggle.onValueChanged.AddListener((isOn) => { if (isOn) SetActiveCategory(QuestCategory.Repeat); });

        // 닫기 버튼
        exitButton.onClick.AddListener(ClosePanel);

        SetActiveCategory(QuestCategory.Daily);
    }

    private void OnEnable()
    {
        waitRoutine = StartCoroutine(WaitAndBind());
    }

    private void OnDisable()
    {
        if (waitRoutine != null)
        {
            StopCoroutine(waitRoutine);
            waitRoutine = null;
        }

        if (QuestManager.Instance != null)
            QuestManager.Instance.OnQuestsUpdated -= RefreshQuestUI;
    }

    private void ClosePanel()
    {
        gameObject.SetActive(false);
    }

    private IEnumerator WaitAndBind()
    {
        while (QuestManager.Instance == null)
            yield return null;

        while (!QuestManager.Instance.IsReady)
            yield return null;

        QuestManager.Instance.OnQuestsUpdated += RefreshQuestUI;
        RefreshQuestUI();
    }

    private void Update()
    {
        // 남은 시간 갱신
        if (QuestManager.Instance != null)
        {
            string remainTime = QuestManager.Instance.GetRemainingTimeFormatted(currentCategory);
            timeText.text = $"남은시간 {remainTime}";
        }

        UpdateCurrency();
    }

    /// <summary>
    /// 탭 전환
    /// </summary>
    public void SetActiveCategory(QuestCategory category)
    {
        currentCategory = category;
        RefreshQuestUI();
    }

    /// <summary>
    /// 퀘스트 UI 갱신
    /// </summary>
    public void RefreshQuestUI()
    {
        if (QuestManager.Instance == null) return;

        foreach (Transform child in contentParent)
            Destroy(child.gameObject);

        var quests = QuestManager.Instance.GetQuestsByCategory(currentCategory);
        Debug.Log($"[UI] {currentCategory} 퀘스트 {quests.Count}개 로드됨");

        foreach (var quest in quests)
        {
            Debug.Log($"[UI] {currentCategory} 표시: {quest.questID} / {quest.questName}");
            var ui = Instantiate(questUIPrefab, contentParent);
            var ctrl = ui.GetComponent<QuestUIController>();
            if (ctrl != null) ctrl.SetData(quest);
        }
    }

    /// <summary>
    /// 골드 갱신
    /// </summary>
    private void UpdateCurrency()
    {
        int gold = 1000; // TODO: 나중에 PlayerDataManager에서 불러오기
        goldText.text = $"{gold:N0}";
    }
}