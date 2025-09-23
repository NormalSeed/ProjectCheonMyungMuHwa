using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AttendanceSlot : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI dayText;
    public TextMeshProUGUI rewardText;
    public Image icon;
    public GameObject claimedMark;       // "완료" 체크마크
    public Image highlightBorder;        // 7일차 강조 테두리
    public Button adButton;              // 광고 보상 버튼

    private int day;

    public void SetData(AttendanceReward rewardData, int day, bool isToday, bool isClaimed)
    {
        this.day = day;
        dayText.text = $"{day}일차";
        rewardText.text = rewardData != null && rewardData.rewards.Count > 0
            ? rewardData.rewards[0].GetDisplayName()
            : "보상 없음";

        claimedMark.SetActive(isClaimed);

        // 오늘인데 아직 미수령, 강조
        if (isToday && !isClaimed)
            dayText.color = Color.yellow;
        else
            dayText.color = Color.white;
    }

    public void HighlightAsSpecial()
    {
        if (highlightBorder != null)
            highlightBorder.gameObject.SetActive(true);
    }

    public void ResetHighlight()
    {
        if (highlightBorder != null)
            highlightBorder.gameObject.SetActive(false);
    }

    public void SetAdButtonActive(bool active)
    {
        if (adButton != null)
        {
            adButton.gameObject.SetActive(active);
            if (active)
            {
                adButton.onClick.RemoveAllListeners();
                adButton.onClick.AddListener(() =>
                {
                    Debug.Log($"광고 보상 시도: {day}일차");
                    AttendanceManager.Instance.ClaimAdBonus(day);
                });
            }
        }
    }
}