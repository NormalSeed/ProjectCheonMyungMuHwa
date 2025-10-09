using System.Collections;
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
    [SerializeField] ItemSlot[] slots;

    [Header("Buttons")]
    public Button adButton;
    public Button rewardButton;

    [Header("KMS Add")]
    public AdDataSO adData;

    private double rewardGold;
    private double adMultiplier = 1.3f;

    private Dictionary<CurrencyType, BigCurrency> pendingRewards;
    private int Exp;

    private void Awake()
    {
        Instance = this;
        slots = rewardSlotsParent.GetComponentsInChildren<ItemSlot>(true);
        offlineRewardPanel.SetActive(false);
    }

    // BackendManager에서 불러온 데이터로 UI 초기화
    public void ShowReward(Dictionary<CurrencyType, BigCurrency> rewards, int time, int exp)
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
        timeText.text = $"{time} 분";

        Debug.Log("[OfflineRewardUI] 보상 적용 시작");
        Exp = exp;
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

        int i = 0;
        foreach (var reward in rewards)
        {
            if (i >= slots.Length) break;

            // BigCurrency를 문자열로 표시
            slots[i].gameObject.SetActive(true);
            ItemData item = null;
            switch (reward.Key)
            {
                case CurrencyType.Gold:
                    item = new ItemData(11002, "", "", "GoldImage", true, ItemType.Currency); break;
                case CurrencyType.Soul:
                    item = new ItemData(11003, "", "", "SoulImage", true, ItemType.Currency); break;
                case CurrencyType.SpiritStone:
                    item = new ItemData(11004, "", "", "SpiritImage", true, ItemType.Currency); break;
                case CurrencyType.SummonTicket:
                    item = new ItemData(11006, "", "", "HeroTicketImage", true, ItemType.Currency); break;
                case CurrencyType.EquipmentSummonTicket:
                    item = new ItemData(11011, "", "", "EquipTicketImage", true, ItemType.Currency); break;
            }
            slots[i].SetItem(item, reward.Value);
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
            PlayerProfileManager.Instance.AddExp(Exp);
            offlineRewardPanel.SetActive(false);
        });

        // 광고 보상 버튼
        adButton.onClick.RemoveAllListeners();
        //adButton.onClick.AddListener(() =>
        //{
        //    foreach (var reward in rewards)
        //    {
        //        CurrencyManager.Instance.Add(reward.Key, reward.Value);
        //    }
        //    offlineRewardPanel.SetActive(false);
        //});
        adButton.onClick.AddListener(() =>
        {
            adData.ShowRewardAD(() =>
            {
                StartCoroutine(AdRoutine(rewards));
            });

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

    private IEnumerator AdRoutine(Dictionary<CurrencyType, BigCurrency> rewards)
    {
        yield return null;
        RouletteUI ui = PopupManager.Instance.ShowRoulettePopup();
        ui.Rewards = rewards; //리워드 룰렛 ui로 전달
        ui.Exp = Exp;
        this.SetHide();
    }
}
