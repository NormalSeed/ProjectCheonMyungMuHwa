using System.Collections;
using UnityEngine;
using UnityEngine.UI;


public class QuestUIManager : UIBase
{
    [SerializeField] private GameObject questUIPrefab;
    [SerializeField] private Transform contentParent;

    [Header("탭 버튼")]
    [SerializeField] private Button dailyTabButton;
    [SerializeField] private Button weeklyTabButton;
    [SerializeField] private Button repeatTabButton;

    [Header("탭 색상")]
    [SerializeField] private Color activeColor = new(0.29f, 0.56f, 0.89f);   // 파랑
    [SerializeField] private Color inactiveColor = new(0.87f, 0.87f, 0.87f); // 회색

    private Coroutine waitRoutine;
    private QuestCategory currentCategory = QuestCategory.Daily; // 기본은 일일

    private void Start()
    {
        SetActiveCategory(QuestCategory.Daily); // 시작은 일일 퀘스트
    }

    private void OnEnable()
    {
        waitRoutine = StartCoroutine(WaitAndBind());
    }

    private void OnDisable()
    {
        if (waitRoutine != null) {
            StopCoroutine(waitRoutine);
            waitRoutine = null;
        }

        if (QuestManager.Instance != null)
            QuestManager.Instance.OnQuestsUpdated -= RefreshQuestUI;
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

    /// <summary>
    /// 탭 전환 (UI 버튼에서 호출)
    /// </summary>
    public void SetActiveCategory(QuestCategory category)
    {
        currentCategory = category;
        RefreshQuestUI();
        UpdateTabUI();
    }

    public void OnClickDaily() => SetActiveCategory(QuestCategory.Daily);
    public void OnClickWeekly() => SetActiveCategory(QuestCategory.Weekly);
    public void OnClickRepeat() => SetActiveCategory(QuestCategory.Repeat);

    /// <summary>
    /// 퀘스트 UI 갱신
    /// </summary>
    public void RefreshQuestUI()
    {
        if (QuestManager.Instance == null)
            return;

        // 기존 UI 제거
        foreach (Transform child in contentParent)
            Destroy(child.gameObject);

        // 새로운 UI 생성
        var quests = QuestManager.Instance.GetQuestsByCategory(currentCategory);

        foreach (var quest in quests) {
            var ui = Instantiate(questUIPrefab, contentParent);
            var ctrl = ui.GetComponent<QuestUIController>();
            if (ctrl != null) ctrl.SetData(quest);
        }
    }

    private void UpdateTabUI()
    {
        dailyTabButton.GetComponent<Image>().color =
            currentCategory == QuestCategory.Daily ? activeColor : inactiveColor;

        weeklyTabButton.GetComponent<Image>().color =
            currentCategory == QuestCategory.Weekly ? activeColor : inactiveColor;

        repeatTabButton.GetComponent<Image>().color =
            currentCategory == QuestCategory.Repeat ? activeColor : inactiveColor;
    }
}