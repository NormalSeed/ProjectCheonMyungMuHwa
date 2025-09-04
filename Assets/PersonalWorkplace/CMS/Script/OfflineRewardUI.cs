using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OfflineRewardUI : MonoBehaviour
{
    public static OfflineRewardUI Instance;

    [Header("Panel")]
    public GameObject offlineRewardPanel;

    [Header("Texts")]
    public TMP_Text timeText;

    [Header("Slots")]
    public Transform rewardSlotsParent;
    private ItemSlotUI[] slots;

    [Header("Buttons")]
    public Button adButton;
    public Button rewardButton;

    private double rewardGold;
    private double adMultiplier = 1.3f;

    private void Awake()
    {
        Instance = this;
        slots = rewardSlotsParent.GetComponentsInChildren<ItemSlotUI>(true);
        offlineRewardPanel.SetActive(false);
    }

    // BackendManager에서 불러온 데이터로 UI 초기화
    public void ShowReward(Dictionary<CurrencyType, int> rewards)
    {
        offlineRewardPanel.SetActive(true);

        // UI 초기화
        foreach (var slot in slots) slot.gameObject.SetActive(false);

        int i = 0;
        foreach (var reward in rewards)
        {
            if (i >= slots.Length) break;
            slots[i].SetSlot(reward.Key.ToString(), reward.Value);
            slots[i].gameObject.SetActive(true);
            if (reward.Key == CurrencyType.Gold)
                rewardGold = reward.Value;
            i++;
        }

        // 광고 보상 버튼
        adButton.onClick.RemoveAllListeners();
        adButton.onClick.AddListener(() =>
        {
            double extraGold = rewardGold * adMultiplier;
            PlayerDataManager.Instance.AddGold(extraGold);
            BackendManager.Instance.UpdatePlayerData(
                PlayerDataManager.Instance.ClearedStage,
                PlayerDataManager.Instance.Gold
            );
            offlineRewardPanel.SetActive(false);
        });

        // 그냥 보상 버튼
        rewardButton.onClick.RemoveAllListeners();
        rewardButton.onClick.AddListener(() =>
        {
            PlayerDataManager.Instance.AddGold(rewardGold);
            BackendManager.Instance.UpdatePlayerData(
                PlayerDataManager.Instance.ClearedStage,
                PlayerDataManager.Instance.Gold
            );
            offlineRewardPanel.SetActive(false);
        });
    }
}
