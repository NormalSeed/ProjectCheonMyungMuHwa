using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestUIController : UIBase
{
    [Header("UI")]
    public TextMeshProUGUI questNameText;
    public Image progressFill; // Slider 대신 Image로!
    public TextMeshProUGUI progressText;
    public Button claimButton;

    public Transform rewardIconsParent; // RewardIcons 
    public GameObject rewardIconPrefab; // 아이콘 프리팹

    private string questId;

    public void SetData(Quest quest)
    {
        questId = quest.questID;

        questNameText.text = quest.questName;
        progressFill.fillAmount = (float)quest.valueProgress / quest.valueGoal;
        progressText.text = $"{quest.valueProgress}/{quest.valueGoal}";
        claimButton.interactable = quest.isComplete && !quest.isClaimed;

        claimButton.onClick.RemoveAllListeners();
        claimButton.onClick.AddListener(OnClickClaim);
    }

    private void OnClickClaim()
    {
        QuestManager.Instance.ClaimReward(questId);
        RefreshUI();
    }

    public void RefreshUI()
    {
        if (QuestManager.Instance.activeQuests.TryGetValue(questId, out Quest quest))
        {
            SetData(quest);
        }
    }

    public void SetRewards(List<Sprite> rewardSprites)
    {
        // 기존 아이콘 제거
        foreach (Transform child in rewardIconsParent)
            Destroy(child.gameObject);

        // 새로운 아이콘 생성
        foreach (var sprite in rewardSprites)
        {
            var icon = Instantiate(rewardIconPrefab, rewardIconsParent);
            icon.GetComponent<Image>().sprite = sprite;
        }
    }
}
