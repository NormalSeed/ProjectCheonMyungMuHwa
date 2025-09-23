using System;
using System.Collections.Generic;
using UnityEngine;

public class AttendanceCSVLoader : MonoBehaviour
{
    public TextAsset csvFile; // 인스펙터에 CSV 넣기

    public List<AttendanceReward> LoadRewards()
    {
        var rewardsDict = new Dictionary<int, AttendanceReward>();
        var lines = csvFile.text.Split('\n');

        if (lines.Length < 4)
        {
            Debug.LogError("[CSV] 데이터가 부족합니다. (헤더 3줄 + 최소 1줄 데이터 필요)");
            return new List<AttendanceReward>();
        }

        var headers = lines[1].Trim().Split(',');

        for (int i = 3; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            var cols = line.Split(',');

            // Day (첫 번째 값)
            if (!int.TryParse(cols[0].Trim(), out int day))
            {
                Debug.LogError($"[CSV 파싱 오류] {i + 1}번째 줄 Day 값 변환 실패 -> {cols[0]}");
                continue;
            }

            if (!rewardsDict.ContainsKey(day))
                rewardsDict[day] = new AttendanceReward { day = day };

            // 보상 데이터 (두 번째 열부터 끝까지)
            for (int j = 1; j < cols.Length; j++)
            {
                string rawValue = cols[j].Trim().Replace(",", ""); // "1,000.00" → "1000.00"

                if (float.TryParse(rawValue, out float value) && value > 0f)
                {
                    var (rewardType, currencyType) = GetTypesFromHeader(headers[j].Trim());

                    var reward = new Reward
                    {
                        rewardID = headers[j].Trim(),        // CSV 헤더명
                        rewardType = rewardType,             // Currency / Item / Equipment
                        currencyType = currencyType,         // CurrencyType (Gold, Jewel 등)
                        rewardCount = Mathf.RoundToInt(value) // 소수점 제거 후 int
                    };

                    rewardsDict[day].rewards.Add(reward);
                }
            }
        }

        return new List<AttendanceReward>(rewardsDict.Values);
    }

    private (RewardType rewardType, CurrencyType? currencyType) GetTypesFromHeader(string header)
    {
        switch (header)
        {
            case "Gold_Per_Hour":
                return (RewardType.Currency, CurrencyType.Gold);
            case "Spirit_Per_Hour":
                return (RewardType.Currency, CurrencyType.Soul); 
            case "Crystal":
                return (RewardType.Currency, CurrencyType.Jewel);

            case "RareBox":
            case "GrindingStone":
                return (RewardType.Item, null);

            case "Gacha_Hero":
            case "Gacha_Equipment":
            case "Gacha_Legend_Hero":
            case "Gacha_Unique_Hero":
                return (RewardType.Equipment, null);

            default:
                Debug.LogWarning($"[CSV] 알 수 없는 헤더명: {header}, 기본값 Item 처리");
                return (RewardType.Item, null);
        }
    }
}

[System.Serializable]
public class AttendanceReward
{
    public int day;
    public List<Reward> rewards = new();
}

