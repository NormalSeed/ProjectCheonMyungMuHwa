using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.InputSystem.LowLevel.InputStateHistory;

public class QuestUIController : UIBase
{
    [Header("UI")]
    public TextMeshProUGUI questNameText;
    public Image progressFill;
    public TextMeshProUGUI progressText;
    public Button claimButton; // 보상 받기 버튼

    private Quest currentQuest; 

    public void SetData(Quest quest)
    {
        this.currentQuest = quest;

        questNameText.text = currentQuest.questName;

        progressFill.fillAmount = (float)currentQuest.valueProgress / currentQuest.valueGoal;
        progressText.text = $"{currentQuest.valueProgress}/{currentQuest.valueGoal}";

        claimButton.onClick.RemoveAllListeners();
        claimButton.onClick.AddListener(OnClickClaim);

        claimButton.interactable = currentQuest.state == QuestState.RewardReady;
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