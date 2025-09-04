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
    public Transform rewardSlotsParent; // ItemSlot들이 들어있는 부모 오브젝트
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

        // UI 표시
        foreach (var slot in slots)
        {
            slot.gameObject.SetActive(false);
        }

        int i = 0;
        foreach (var reward in rewards)
        {
            if (i >= slots.Length) break;
            slots[i].SetSlot(reward.Key.ToString(), reward.Value);
            slots[i].gameObject.SetActive(true);
            i++;
        }

        // 버튼 리스너
        adButton.onClick.RemoveAllListeners();
        adButton.onClick.AddListener(() =>
        {
            var extraGold = rewards[CurrencyType.Gold] * 0.3; // 30% 추가
            PlayerDataManager.Instance.AddGold(extraGold);
            BackendManager.Instance.UpdatePlayerData(
                PlayerDataManager.Instance.ClearedStage,
                PlayerDataManager.Instance.Gold
            );
            offlineRewardPanel.SetActive(false);
        });

        rewardButton.onClick.RemoveAllListeners();
        rewardButton.onClick.AddListener(() =>
        {
            offlineRewardPanel.SetActive(false);
        });
    }

    private void Start()
    {
        adButton.onClick.AddListener(OnAdButton);
        rewardButton.onClick.AddListener(OnRewardButton);
    }

    private void OnAdButton()
    {
        double rewardedGold = rewardGold * adMultiplier;
        PlayerDataManager.Instance.AddGold(rewardGold);

        Debug.Log($"광고 보상 획득: {rewardedGold}");
        offlineRewardPanel.SetActive(false);
    }

    private void OnRewardButton()
    {
        PlayerDataManager.Instance.AddGold(rewardGold);

        Debug.Log($"보상 획득: {rewardGold}");
        offlineRewardPanel.SetActive(false);
    }
}
