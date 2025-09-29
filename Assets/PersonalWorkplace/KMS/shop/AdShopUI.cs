using System;
using UnityEngine;

public class AdShopUI : MonoBehaviour
{
    [SerializeField] AdShopCard[] cards;
    [SerializeField] AdDataSO adData;


    void OnEnable()
    {
        foreach (AdShopCard card in cards)
        {
            card.OnClick.AddListener(() => adData.ShowRewardAD(() => card.GiveCurrencyAndCloseButton()));
        }
    }
    void OnDisable()
    {
        foreach (AdShopCard card in cards)
        {
            card.OnClick.RemoveAllListeners();
        }
    }
}
