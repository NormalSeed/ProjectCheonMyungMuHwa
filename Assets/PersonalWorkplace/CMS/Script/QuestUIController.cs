using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuestUIController : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI questNameText;  // TMP
    public Slider progressBar;
    public Button claimButton;

    private string questId;

    // 퀘스트 UI 데이터 세팅
    public void SetData(Quest quest)
    {
        questId = quest.questID;

        Debug.Log($"[UI 생성됨] {quest.questName} - {quest.valueProgress}/{quest.valueGoal}");

        questNameText.text = $"{quest.questName} ({quest.valueProgress}/{quest.valueGoal})";
        progressBar.maxValue = quest.valueGoal;
        progressBar.value = quest.valueProgress;
        claimButton.interactable = quest.isComplete && !quest.isClaimed;
    }

    // 보상 버튼 클릭
    private void OnClickClaim()
    {
        QuestManager.Instance.ClaimReward(questId);
        RefreshUI();
    }

    // 외부에서 UI 갱신
    public void RefreshUI()
    {
        if (QuestManager.Instance.activeQuests.TryGetValue(questId, out Quest quest))
        {
            UpdateUI(quest);
        }
    }

    // 내부 UI 업데이트 로직
    private void UpdateUI(Quest quest)
    {
        questNameText.text = $"{quest.questName} ({quest.valueProgress}/{quest.valueGoal})";
        progressBar.maxValue = quest.valueGoal;
        progressBar.value = quest.valueProgress;
        claimButton.interactable = quest.isComplete && !quest.isClaimed;
    }
}
