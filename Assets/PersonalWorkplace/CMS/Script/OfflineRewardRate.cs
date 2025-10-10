using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class OfflineRewardRate
{
    public CurrencyType currencyType;
    public double baseRatePerSecond;
    public double rarityMultiplier;
}

public class OfflineRewardSystem : MonoBehaviour
{
    public static OfflineRewardSystem Instance { get; private set; }

    [SerializeField] private List<OfflineRewardRate> rewardRates;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public Dictionary<CurrencyType, BigCurrency> CalculateRewards(TimeSpan offlineTime, int clearedStage)
    {
        var rewards = new Dictionary<CurrencyType, BigCurrency>();

        double seconds = Math.Min(offlineTime.TotalSeconds, 8 * 3600);

        foreach (var rate in rewardRates)
        {
            double raw = rate.baseRatePerSecond * seconds;

            // 스테이지 난이도 보정
            raw *= (1 + clearedStage * 0.05);

            // 희귀도 보정
            raw *= rate.rarityMultiplier;

            // BigCurrency 변환
            rewards[rate.currencyType] = new BigCurrency(raw, 0);
        }

        return rewards;
    }
}
