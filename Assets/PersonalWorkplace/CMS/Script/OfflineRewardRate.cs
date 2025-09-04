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

    public Dictionary<CurrencyType, int> CalculateRewards(TimeSpan offlineTime, int clearedStage)
    {
        Dictionary<CurrencyType, int> rewards = new();

        double seconds = Math.Min(offlineTime.TotalSeconds, 8 * 3600);
        foreach (var rate in rewardRates)
        {
            double amount = rate.baseRatePerSecond * seconds;
            amount *= (1 + (clearedStage * 0.05));
            amount *= rate.rarityMultiplier;

            rewards[rate.currencyType] = (int)Math.Floor(amount);
        }

        return rewards;
    }
}
