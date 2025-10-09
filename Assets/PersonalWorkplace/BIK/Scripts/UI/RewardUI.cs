using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RewardUI : UIBase
{
    [SerializeField] private Button _closeButton;
    [SerializeField] private List<ItemSlot> _itemSlots = new();
    [SerializeField] private GameObject _bonusGroup;
    [SerializeField] private TMP_Text _bonusRateText;
    [SerializeField] private CurrencyConfig _currencyConfig;

    private static readonly BigCurrency ZERO = new BigCurrency(0, 0);

    private void Awake()
    {
        _closeButton.onClick.AddListener(PopupManager.Instance.CloseAllPopups);
    }

    private void Start()
    {
        //for (int i = 0; i < _itemSlots.Count; i++) {
        //    _itemSlots[i].SetEmpty();
        //}
    }

    /// <summary>
    /// 보상 표시 및 지급
    /// </summary>
    /// <param name="rewards">아이템 메타</param>
    /// <param name="rewardCounts">각 수량 (BigCurrency: 0티어=무단위, 1티어=A)</param>
    /// <param name="bonus">보너스 유무</param>
    /// <param name="bonusRate">예: 1.5 → "1.50배 획득!"</param>
    public void SetShow(List<ItemData> rewards, List<BigCurrency> rewardCounts, bool bonus = false, double bonusRate = 0.0, int exp = 0)
    {
        if (rewards == null || rewardCounts == null) return;

        // 1) 슬롯 채우기 (표시는 BigCurrency 그대로)
        for (int i = 0; i < _itemSlots.Count; i++) {
            if (i < rewards.Count) {
                var amt = i < rewardCounts.Count ? rewardCounts[i] : ZERO;
                _itemSlots[i].SetItem(rewards[i], amt);
            }
            else {
                _itemSlots[i].SetEmpty();
            }
        }

        // 2) 실제 지급 처리
        int count = Mathf.Min(rewards.Count, rewardCounts.Count);
        for (int i = 0; i < count; i++)
        {
            var item = rewards[i];
            var amount = rewardCounts[i] ?? ZERO;

            if (item.Type == ItemType.Currency)
            {
                var currencyType = _currencyConfig.GetCurrencyType(item.Id);
                // CurrencyManager가 BigCurrency delta를 받는다고 가정
                CurrencyManager.Instance.Add(currencyType, amount);
            }
            else
            {
                // 인벤토리는 int(=A단위 원시수량)로 저장하므로 변환
                int baseUnits = ToBaseUnitsInt(amount);
                if (baseUnits <= 0) continue;

                string key = item.Id.ToString();
                InventoryManager.Instance.Add(key, baseUnits);
            }
        }
        if ( exp > 0 ) PlayerProfileManager.Instance.AddExp(exp);
        

        // 3) 보너스 표기
        _bonusGroup.SetActive(bonus);
        _bonusRateText.text = bonus ? $"{bonusRate:F2}배 획득!" : string.Empty;

        base.SetShow();
    }

    /// <summary>
    /// BigCurrency를 A-단위 원시 정수로 변환(인벤토리 저장용)
    /// </summary>
    private int ToBaseUnitsInt(BigCurrency c)
    {
        if (c == null) return 0;
        double baseAmount = c.Value * Math.Pow(1000.0, c.Tier);
        if (baseAmount <= 0) return 0;
        if (baseAmount > int.MaxValue) return int.MaxValue;
        return (int)Math.Round(baseAmount);
    }
}
