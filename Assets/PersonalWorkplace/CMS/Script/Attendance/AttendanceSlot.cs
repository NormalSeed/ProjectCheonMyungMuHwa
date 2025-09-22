using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;

public class AttendanceSlot : MonoBehaviour
{
    public Text dayText;
    public Text rewardText;
    public Button claimButton;
    public Text statusText;

    public void SetData(int day, AttendanceReward reward, bool isToday, bool isClaimed, System.Func<Task> onClaim)
    {
        dayText.text = $"{day}일차";
        rewardText.text = reward != null && reward.rewards.Count > 0
            ? reward.rewards[0].GetDisplayName()
            : "-";

        claimButton.onClick.RemoveAllListeners();

        if (isClaimed)
        {
            statusText.text = "완료";
            claimButton.interactable = false;
        }
        else if (isToday)
        {
            statusText.text = "수령 가능";
            claimButton.interactable = true;
            claimButton.onClick.AddListener(async () => await onClaim());
        }
        else
        {
            statusText.text = "잠김";
            claimButton.interactable = false;
        }
    }
}