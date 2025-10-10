using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AttendanceSlot : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI dayText;
    // public Image icon; // [수정] 이 변수는 더 이상 사용하지 않습니다.
    [SerializeField] private ItemSlot _rewardItemSlot; // [추가] ItemSlot 참조를 추가합니다.

    public GameObject claimedMark;
    public Button adButton;

    private int day;

    /// <summary>
    /// 출석 슬롯의 데이터를 설정합니다.
    /// </summary>
    /// <param name="rewardData">해당 날짜의 보상 데이터</param>
    /// <param name="day">날짜 (1일차, 2일차...)</param>
    /// <param name="isToday">오늘 날짜인지 여부</param>
    /// <param name="isClaimed">이미 보상을 받았는지 여부</param>
    public void SetData(AttendanceReward rewardData, int day, bool isToday, bool isClaimed, TableManager tableManager)
    {
        this.day = day;
        dayText.text = $"{day}일차";
        claimedMark.SetActive(isClaimed);

        if (rewardData != null && rewardData.rewards.Count > 0)
        {
            RewardItemInfo rewardInfo = rewardData.rewards[0];

            var itemTable = tableManager.GetTable<TItem>(TableType.Item);
            ItemData itemData = itemTable?.GetItem(rewardInfo.itemID);

            if (itemData != null)
            {
                BigCurrency itemAmount = new BigCurrency(rewardInfo.amount, rewardInfo.amountTier);
                _rewardItemSlot.SetItem(itemData, itemAmount);
                _rewardItemSlot.gameObject.SetActive(true);
            }
            else
            {
                Debug.LogWarning($"아이템 ID({rewardInfo.itemID})를 찾을 수 없습니다.");
                _rewardItemSlot.SetEmpty();
                _rewardItemSlot.gameObject.SetActive(false);
            }
        }
        else
        {
            _rewardItemSlot.SetEmpty();
            _rewardItemSlot.gameObject.SetActive(false);
        }

        if (isToday && !isClaimed)
            dayText.color = Color.yellow;
        else
            dayText.color = Color.white;
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
                    // 광고 재생 로직 호출 후 성공 시 ClaimAdBonus 호출
                    // AttendanceManager.Instance.ClaimAdBonus(day);
                });
            }
        }
    }
}
