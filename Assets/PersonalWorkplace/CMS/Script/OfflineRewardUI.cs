using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OfflineRewardUI : UIBase
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

    private Dictionary<CurrencyType, BigCurrency> pendingRewards;

    private void Awake()
    {
        Instance = this;
        slots = rewardSlotsParent.GetComponentsInChildren<ItemSlotUI>(true);
        offlineRewardPanel.SetActive(false);
    }

    // BackendManager에서 불러온 데이터로 UI 초기화
    public void ShowReward(Dictionary<CurrencyType, BigCurrency> rewards)
    {
        Debug.Log("[OfflineRewardUI] ShowReward 호출됨");

        if (rewards == null || rewards.Count == 0)
        {
            Debug.LogWarning("[OfflineRewardUI] rewards 비어있음, 팝업 표시 안 함");
            return;
        }

        if (CurrencyManager.Instance == null || !CurrencyManager.Instance.IsInitialized)
        {
            Debug.Log("[OfflineRewardUI] CurrencyManager 초기화 안됨, 대기 상태로 보상 저장");
            pendingRewards = rewards;
            CurrencyManager.OnInitialized += OnCurrencyManagerInitialized;
            return;
        }

        Debug.Log("[OfflineRewardUI] 보상 적용 시작");
        ApplyRewardUI(rewards);
    }

    // CurrencyManager 초기화 완료 후 보상 적용
    private void OnCurrencyManagerInitialized()
    {
        Debug.Log("[OfflineRewardUI] CurrencyManager 초기화 완료, 대기중 보상 적용 시도");
        if (pendingRewards != null)
        {
            ApplyRewardUI(pendingRewards);
            pendingRewards = null;
        }
        CurrencyManager.OnInitialized -= OnCurrencyManagerInitialized;
    }
    // 실제 보상 UI 처리
    private void ApplyRewardUI(Dictionary<CurrencyType, BigCurrency> rewards)
    {
        offlineRewardPanel.SetActive(true);

        // 슬롯 초기화
        foreach (var slot in slots)
            slot.gameObject.SetActive(false);

        int i = 0;
        foreach (var reward in rewards)
        {
            if (i >= slots.Length) break;

            // BigCurrency를 문자열로 표시
            slots[i].SetSlot(reward.Key.ToString(), reward.Value.ToString());
            slots[i].gameObject.SetActive(true);
            i++;
        }

        // 일반 보상 버튼
        rewardButton.onClick.RemoveAllListeners();
        rewardButton.onClick.AddListener(() =>
        {
            foreach (var reward in rewards)
            {
                CurrencyManager.Instance.Add(reward.Key, reward.Value);
            }
            offlineRewardPanel.SetActive(false);
        });

        // 광고 보상 버튼
        adButton.onClick.RemoveAllListeners();
        adButton.onClick.AddListener(() =>
        {
            foreach (var reward in rewards)
            {
                CurrencyManager.Instance.Add(reward.Key, reward.Value);
            }
            offlineRewardPanel.SetActive(false);
        });
    }

    public override void SetShow()
    {
        offlineRewardPanel.SetActive(true);
    }

    public override void SetHide()
    {
        offlineRewardPanel.SetActive(false);
    }
}
