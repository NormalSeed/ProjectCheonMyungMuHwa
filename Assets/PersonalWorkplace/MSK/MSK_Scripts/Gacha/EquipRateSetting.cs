using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EquipRateSetting : MonoBehaviour
{
    [Header("SummonInfo")]
    [SerializeField] private GachaRateInfoUI infoUI;

    [SerializeField] private List<TextMeshProUGUI> rateText;

    [SerializeField] private HeroRarity rarity;

    private void OnEnable()
    {
        Init();
    }

    public void Init()
    {
        UpdateRateText();
    }

    private void UpdateRateText()
    {
        if (infoUI == null || rateText == null || rateText.Count == 0)
        {
            Debug.LogWarning("[EquipRateSetting] infoUI 또는 rateText가 설정되지 않았습니다.");
            return;
        }

        string rateValue = rarity switch
        {
            HeroRarity.Normal => infoUI.normalRate,
            HeroRarity.Rare => infoUI.rareRate,
            HeroRarity.Unique => infoUI.uniqueRate,
            HeroRarity.Legend => infoUI.LegendaryRate,
            _ => "0.00%"
        };

        foreach (var text in rateText)
        {
            text.text = rateValue;
        }
    }
}

