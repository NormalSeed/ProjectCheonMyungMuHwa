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
        bool isCompleted = currentQuest.state == QuestState.Completed;

        // 버튼 활성화 여부
        claimButton.interactable = isRewardReady;

        // 완료 마크는 "완전히 완료"일 때만 활성화
        if (completeMark != null)
            completeMark.SetActive(isCompleted);

        // 어두워지는 효과도 "완전히 완료"일 때만 적용
        if (dimOverlay != null)
            dimOverlay.gameObject.SetActive(isCompleted);
    }

    private void OnClickClaim()
    {
        if (currentQuest == null) return;
        QuestManager.Instance.ClaimReward(currentQuest);
    }

    public override void RefreshUI()
    {
        if (currentQuest != null && QuestManager.Instance.activeQuests.TryGetValue(currentQuest.questID, out Quest updatedQuest))
        {
            SetData(updatedQuest);
        }
    }
}