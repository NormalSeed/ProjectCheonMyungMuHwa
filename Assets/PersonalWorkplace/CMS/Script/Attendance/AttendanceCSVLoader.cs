using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

public class AttendanceCSVLoader : MonoBehaviour
{
    [SerializeField] private TextAsset csvFile;
    private TableManager _tableManager;

    [Inject]
    public void Construct(TableManager tableManager)
    {
        Debug.Log("[AttendanceCSVLoader] Construct 호출됨, TableManager 주입 완료");
        _tableManager = tableManager;
    }

    // 출석 CSV 컬럼, Item.csv 의 Item_Image 매핑
    private static readonly Dictionary<string, string> CsvToItemImageMap = new()
{
    { "Gold_Per_Hour", "금화" },
    { "Spirit_Per_Hour", "혼백" },
    { "Crystal", "용옥" },
    { "RareBox", "희귀상자" },
    { "Gacha_Hero", "등용패" },
    { "Gacha_Equipment", "초대장" },
    { "GrindingStone", "연마석" },
    { "Gacha_Legend_Hero", "전설_영웅_랜덤_뽑기권" },
    { "Gacha_Unique_Hero", "특급_영웅_랜덤_뽑기권" },
};

    public List<AttendanceReward> LoadRewards()
    {
        var rewards = new List<AttendanceReward>();

        if (csvFile == null)
        {
            Debug.LogError("[AttendanceCSVLoader] csvFile 이 비어있음");
            return new List<AttendanceReward>();
        }

        if (_tableManager == null)
        {
            Debug.LogError("[AttendanceCSVLoader] _tableManager 가 null! VContainer 주입 안됨");
            return rewards;
        }

        var itemTable = _tableManager.GetTable<TItem>(TableType.Item);
        if (itemTable == null)
        {
            Debug.LogError("[AttendanceCSVLoader] itemTable 불러오기 실패");
            return new List<AttendanceReward>();
        }

        string[] lines = csvFile.text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length < 3)
        {
            Debug.LogError("[AttendanceCSVLoader] CSV 라인 부족");
            return rewards;
        }

        // 2번째 줄 헤더
        string[] headers = lines[1].Split(new[] { '\t', ',' }, StringSplitOptions.RemoveEmptyEntries);

        for (int i = 2; i < lines.Length; i++)
        {
            string[] cols = lines[i].Split(new[] { '\t', ',' }, StringSplitOptions.None);
            if (cols.Length < 2) continue;

            // Day 숫자 아니면 스킵
            if (!int.TryParse(cols[0].Trim(), out int day))
            {
                Debug.Log($"[AttendanceCSVLoader] 스킵됨: {cols[0]} (i={i})");
                continue;
            }

            var dayReward = new AttendanceReward { day = day, rewards = new List<RewardItemInfo>() };
            Debug.Log($"[AttendanceCSVLoader] >>> {day}일차 파싱 시작");

            for (int col = 1; col < headers.Length; col++)
            {
                if (col >= cols.Length) continue;

                string rawValue = cols[col].Replace(",", "").Trim();
                if (!float.TryParse(rawValue, out float value)) continue;
                if (value <= 0) continue;

                string header = headers[col].Trim();
                Debug.Log($"[AttendanceCSVLoader] Day {day}, header={header}, value={value}");

                if (CsvToItemImageMap.TryGetValue(header, out var itemImage))
                {
                    var itemData = itemTable?.GetItemByImage(itemImage);

                    if (itemData != null)
                    {
                        Debug.Log($"[AttendanceCSVLoader] Day {day} 보상 추가됨: ID={itemData.Id}, Image={itemImage}, 수량={value}");
                        dayReward.rewards.Add(new RewardItemInfo
                        {
                            itemID = itemData.Id,
                            amount = value,
                            amountTier = 0
                        });
                    }
                    else
                    {
                        Debug.LogError($"[Attendance] Item_Image '{itemImage}' 못 찾음 (header={header})");
                    }
                }
                else
                {
                    Debug.LogWarning($"[Attendance] '{header}' 매핑 안됨");
                }
            }

            if (dayReward.rewards.Count > 0)
            {
                rewards.Add(dayReward);
            }
            else
            {
                Debug.LogWarning($"[AttendanceCSVLoader] {day}일차 보상 없음 → 스킵됨");
            }
        }

        Debug.Log($"[AttendanceCSVLoader] 최종 {rewards.Count}일치 보상 로드 완료");
        return rewards;
    }
}

// 보상 아이템 하나에 대한 정보를 담는 클래스
[Serializable]
public class RewardItemInfo
{
    public int itemID;       // TItem 테이블의 아이템 ID
    public double amount;    // 수량 (BigCurrency로 변환될 값)
    public int amountTier;   // 수량의 단위 (0: 없음, 1: A, 2: B...)
}

[Serializable]
public class AttendanceReward
{
    public int day;
    public List<RewardItemInfo> rewards = new();
}

public static class TItemD
{
    public static ItemData GetItemByImage(this TItem table, string imageKey)
    {
        return null;
    }
}