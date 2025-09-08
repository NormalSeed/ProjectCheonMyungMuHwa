using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestHUD : MonoBehaviour
{
    [SerializeField] private TMP_Text questText;
    [SerializeField] private TMP_Text progressText;
    [SerializeField] private Button claimButton;

    private Quest currentQuest;

    public void ShowQuest(Quest quest)
    {
        currentQuest = quest;

        questText.text = $"{quest.questName}";

        if (quest.isComplete)
        {
            progressText.text = "클리어";
            progressText.color = Color.yellow;
        }
        else
        {
            progressText.text = $"{quest.valueProgress}/{quest.valueGoal}";
            progressText.color = Color.white;
        }

        // 클레임 버튼 활성화 여부
        claimButton.interactable = quest.isComplete && !quest.isClaimed;
    }

    public void OnClickClaim()
    {
        if (currentQuest != null)
        {
            QuestManager.Instance.ClaimReward(currentQuest.questID);
            RefreshHUD();
        }
    }

    private void RefreshHUD()
    {
        var nextQuests = QuestManager.Instance.GetActiveQuestsForHUD();
        if (nextQuests.Any())
            ShowQuest(nextQuests.First());
        else
            gameObject.SetActive(false);
    }
}