using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestUIController : UIBase
{
    [Header("UI")]
    public TextMeshProUGUI questNameText;
    public Image progressFill;
    public TextMeshProUGUI progressText;
    public Button claimButton;         // 보상 받기 버튼
    public GameObject completeMark;    // "완료" 표시 오브젝트
    public Image dimOverlay;           // 완료 시 어두워짐 효과

    private Quest currentQuest;

    public void SetData(Quest quest)
    {
        this.currentQuest = quest;

        questNameText.text = currentQuest.questName;
        progressFill.fillAmount = (float)currentQuest.valueProgress / currentQuest.valueGoal;
        progressText.text = $"{currentQuest.valueProgress}/{currentQuest.valueGoal}";

        claimButton.onClick.RemoveAllListeners();
        claimButton.onClick.AddListener(OnClickClaim);

        UpdateStateUI();
    }

    private void UpdateStateUI()
    {
        bool isRewardReady = currentQuest.state == QuestState.RewardReady;
        bool isCompletedOrDisabled =
            currentQuest.state == QuestState.Completed ||
            currentQuest.state == QuestState.Disabled;

        // 버튼 활성화 여부
        claimButton.interactable = isRewardReady;

        // 완료 마크는 "완전히 완료"일 때만 활성화
        if (completeMark != null)
            completeMark.SetActive(isCompletedOrDisabled);

        // 어두워지는 효과도 "완전히 완료"일 때만 적용
        if (dimOverlay != null)
            dimOverlay.gameObject.SetActive(isCompletedOrDisabled);
    }

    private void OnClickClaim()
    {
        if (currentQuest == null) return;
        QuestManager.Instance.ClaimReward(currentQuest);
    }

    public override void RefreshUI()
    {
        if (currentQuest != null &&
            QuestManager.Instance.activeQuests.TryGetValue(currentQuest.questID, out Quest updatedQuest))
        {
            // 상태가 바뀐 경우에만 전체 UI 갱신
            if (currentQuest.state != updatedQuest.state)
            {
                SetData(updatedQuest);
            }
            else
            {
                // 진행도만 갱신
                currentQuest.valueProgress = updatedQuest.valueProgress;
                progressFill.fillAmount = (float)updatedQuest.valueProgress / updatedQuest.valueGoal;
                progressText.text = $"{updatedQuest.valueProgress}/{updatedQuest.valueGoal}";
            }
        }
    }
    private void OnEnable()
    {
        QuestManager.Instance.OnQuestProgressChanged += HandleProgressChanged;
        QuestManager.Instance.OnQuestsUpdated += RefreshUI;
    }

    private void OnDisable()
    {
        QuestManager.Instance.OnQuestProgressChanged -= HandleProgressChanged;
        QuestManager.Instance.OnQuestsUpdated -= RefreshUI;
    }

    private void HandleProgressChanged(Quest quest)
    {
        if (currentQuest != null && currentQuest.questID == quest.questID)
        {
            progressFill.fillAmount = (float)quest.valueProgress / quest.valueGoal;
            progressText.text = $"{quest.valueProgress}/{quest.valueGoal}";
        }
    }
}