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

    private string questId;

    public void SetData(Quest quest)
    {
        questId = quest.questID;

        questNameText.text = quest.questName;

        progressFill.fillAmount = (float)quest.valueProgress / quest.valueGoal;
        progressText.text = $"{quest.valueProgress}/{quest.valueGoal}";

        // 보상 받기 버튼 설정
        claimButton.onClick.RemoveAllListeners();
        claimButton.onClick.AddListener(OnClickClaim);

        // 완료 상태에 따라 버튼 활성화
        claimButton.interactable = quest.isComplete && !quest.isClaimed;

    }

    private void OnClickClaim()
    {
        QuestManager.Instance.ClaimReward(questId);
        RefreshUI();
    }

    public override void RefreshUI()
    {
        if (QuestManager.Instance.activeQuests.TryGetValue(questId, out Quest quest))
        {
            SetData(quest);
        }
    }
}