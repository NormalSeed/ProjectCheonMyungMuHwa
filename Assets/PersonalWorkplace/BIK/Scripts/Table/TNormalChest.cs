using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;

public class TNormalChest : ITable
{
    private Dictionary<int, NormalChestData> _chests = new();
    public bool IsInitialized { get; private set; }
    public event Action OnLoaded;

    public void Load(string url)
    {
        CoroutineRunner.instance.StartCoroutine(LoadFromSheet(url));
    }

    private IEnumerator LoadFromSheet(string url)
    {
        using var req = UnityWebRequest.Get(url);
        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success) {
            ParseCSV(req.downloadHandler.text);
            IsInitialized = true;
            OnLoaded?.Invoke();
        }
        else {
            Debug.LogError("NormalChestTable load failed: " + req.error);
        }
    }

    private void ParseCSV(string csv)
    {
        var lines = csv.Split('\n');

        // 0~2줄은 헤더/타입 정의이므로 스킵
        for (int i = 3; i < lines.Length; i++) {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;
            var cols = lines[i].Split(',');
            if (cols.Length < 17) continue; // 최소 컬럼 수 체크

            try {
                int stage = int.Parse(cols[0].Trim());
                double gold = double.Parse(cols[1].Trim());
                double soul = double.Parse(cols[2].Trim());
                double spiritStone = double.Parse(cols[3].Trim());
                int goldTicket = int.Parse(cols[4].Trim());
                int soulTicket = int.Parse(cols[5].Trim());
                int spiritStoneTicket = int.Parse(cols[6].Trim());
                int summonTicket = int.Parse(cols[7].Trim());
                int equipSummonTicket = int.Parse(cols[8].Trim());

                float goldProb = ParsePercent(cols[9].Trim());
                float soulProb = ParsePercent(cols[10].Trim());
                float spiritStoneProb = ParsePercent(cols[11].Trim());
                float goldTicketProb = ParsePercent(cols[12].Trim());
                float soulTicketProb = ParsePercent(cols[13].Trim());
                float spiritStoneTicketProb = ParsePercent(cols[14].Trim());
                float summonTicketProb = ParsePercent(cols[15].Trim());
                float equipSummonTicketProb = ParsePercent(cols[16].Trim());

                var data = new NormalChestData(
                    stage, gold, soul, spiritStone,
                    goldTicket, soulTicket, spiritStoneTicket,
                    summonTicket, equipSummonTicket,
                    goldProb, soulProb, spiritStoneProb,
                    goldTicketProb, soulTicketProb,
                    spiritStoneTicketProb, summonTicketProb, equipSummonTicketProb
                );

                _chests[stage] = data;
            }
            catch (Exception e) {
                Debug.LogWarning($"[ParseCSV] Stage {i} line parse error: {e.Message}");
            }
        }
    }

    private float ParsePercent(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return 0f;

        value = value.Replace("%", "").Trim();

        if (float.TryParse(value, out float result)) {
            return result / 100f; // % 값 → 0~1 로 변환
        }
        return 0f;
    }

    public NormalChestData GetData(int stage)
    {
        return _chests.TryGetValue(stage, out var data) ? data : null;
    }

    /// <summary>
    /// 해당 스테이지에서 단일 보상 추출
    /// </summary>
    public KeyValuePair<string, double> GetSingleReward(int stage)
    {
        if (!_chests.TryGetValue(stage, out var data))
            return new KeyValuePair<string, double>("None", 0);

        // 확률 합 계산
        float totalProb =
            data.GoldProbability + data.SoulProbability + data.SpiritStoneProbability +
            data.GoldChallengeTicketProbability + data.SoulChallengeTicketProbability +
            data.SpiritStoneChallengeTicketProbability + data.SummonTicketProbability +
            data.EquipmentSummonTicketProbability;

        if (totalProb <= 0f)
            return new KeyValuePair<string, double>("None", 0);

        float roll = UnityEngine.Random.value * totalProb;
        float cumulative = 0f;

        // 룰렛 방식 확률 체크
        cumulative += data.GoldProbability;
        if (roll <= cumulative) return new KeyValuePair<string, double>("Gold", data.Gold);

        cumulative += data.SoulProbability;
        if (roll <= cumulative) return new KeyValuePair<string, double>("Soul", data.Soul);

        cumulative += data.SpiritStoneProbability;
        if (roll <= cumulative) return new KeyValuePair<string, double>("SpiritStone", data.SpiritStone);

        cumulative += data.GoldChallengeTicketProbability;
        if (roll <= cumulative) return new KeyValuePair<string, double>("GoldChallengeTicket", data.GoldChallengeTicket);

        cumulative += data.SoulChallengeTicketProbability;
        if (roll <= cumulative) return new KeyValuePair<string, double>("SoulChallengeTicket", data.SoulChallengeTicket);

        cumulative += data.SpiritStoneChallengeTicketProbability;
        if (roll <= cumulative) return new KeyValuePair<string, double>("SpiritStoneChallengeTicket", data.SpiritStoneChallengeTicket);

        cumulative += data.SummonTicketProbability;
        if (roll <= cumulative) return new KeyValuePair<string, double>("SummonTicket", data.SummonTicket);

        cumulative += data.EquipmentSummonTicketProbability;
        if (roll <= cumulative) return new KeyValuePair<string, double>("EquipmentSummonTicket", data.EquipmentSummonTicket);

        return new KeyValuePair<string, double>("None", 0);
    }
}


public class NormalChestData
{
    public int Stage { get; }
    public double Gold { get; }
    public double Soul { get; }
    public double SpiritStone { get; }
    public int GoldChallengeTicket { get; }
    public int SoulChallengeTicket { get; }
    public int SpiritStoneChallengeTicket { get; }
    public int SummonTicket { get; }
    public int EquipmentSummonTicket { get; }

    public float GoldProbability { get; }
    public float SoulProbability { get; }
    public float SpiritStoneProbability { get; }
    public float GoldChallengeTicketProbability { get; }
    public float SoulChallengeTicketProbability { get; }
    public float SpiritStoneChallengeTicketProbability { get; }
    public float SummonTicketProbability { get; }
    public float EquipmentSummonTicketProbability { get; }

    public NormalChestData(
        int stage,
        double gold, double soul, double spiritStone,
        int goldChallengeTicket, int soulChallengeTicket, int spiritStoneChallengeTicket,
        int summonTicket, int equipmentSummonTicket,
        float goldProbability, float soulProbability, float spiritStoneProbability,
        float goldChallengeTicketProbability, float soulChallengeTicketProbability,
        float spiritStoneChallengeTicketProbability, float summonTicketProbability, float equipmentSummonTicketProbability
    )
    {
        Stage = stage;
        Gold = gold;
        Soul = soul;
        SpiritStone = spiritStone;
        GoldChallengeTicket = goldChallengeTicket;
        SoulChallengeTicket = soulChallengeTicket;
        SpiritStoneChallengeTicket = spiritStoneChallengeTicket;
        SummonTicket = summonTicket;
        EquipmentSummonTicket = equipmentSummonTicket;

        GoldProbability = goldProbability;
        SoulProbability = soulProbability;
        SpiritStoneProbability = spiritStoneProbability;
        GoldChallengeTicketProbability = goldChallengeTicketProbability;
        SoulChallengeTicketProbability = soulChallengeTicketProbability;
        SpiritStoneChallengeTicketProbability = spiritStoneChallengeTicketProbability;
        SummonTicketProbability = summonTicketProbability;
        EquipmentSummonTicketProbability = equipmentSummonTicketProbability;
    }
}