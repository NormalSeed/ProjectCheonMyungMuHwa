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

        for (int i = 1; i < lines.Length; i++) // 0번은 헤더
        {
            var line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            var cols = line.Split(',');

            int day = int.Parse(cols[0]);
            RewardType rewardType = (RewardType)Enum.Parse(typeof(RewardType), cols[1]);
            string rewardID = cols[2];

            // CurrencyType? (nullable) 처리
            CurrencyType? currencyType = null;
            if (!string.IsNullOrEmpty(cols[3]))
            {
                if (Enum.TryParse(cols[3], out CurrencyType parsed))
                    currencyType = parsed;
                else
                    Debug.LogWarning($"[CSV] CurrencyType 파싱 실패 (line {i + 1}) -> {cols[3]}");
            }

            int count = int.Parse(cols[4]);

            var reward = new Reward
            {
                rewardType = rewardType,
                rewardID = rewardID,
                currencyType = currencyType, // null 또는 값
                rewardCount = count
            };

            if (!rewardsDict.ContainsKey(day))
                rewardsDict[day] = new AttendanceReward { day = day };

            rewardsDict[day].rewards.Add(reward);
        }

        return new List<AttendanceReward>(rewardsDict.Values);
    }
}

[System.Serializable]
public class AttendanceReward
{
    public int day;
    public List<Reward> rewards = new();
}