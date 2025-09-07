using System.Collections.Generic;
using UnityEngine;

public enum EquipClass { Front, Middle, Back }
public enum EquipmentType { Weapon, Armor, Gloves, Boots }
public enum RarityType { Common = 1, Rare = 2, Epic = 3, Legendary = 4 }

public static class EquipmentStatTable
{
    public static readonly Dictionary<(EquipmentType, RarityType), (float baseStat, float statIncrease)> table =
        new()
        {
            // TODO : 추후 데이터 테이블 참고해서 수정해야 함
            // 무기 : 공격력
            { (EquipmentType.Weapon, RarityType.Common),    (18f, 2f) },
            { (EquipmentType.Weapon, RarityType.Rare),      (32f, 3f) },
            { (EquipmentType.Weapon, RarityType.Epic),      (46f, 4f) },
            { (EquipmentType.Weapon, RarityType.Legendary), (65f, 5f) },
            // 갑옷 : 방어력
            { (EquipmentType.Armor, RarityType.Common),    (15f, 2f) },
            { (EquipmentType.Armor, RarityType.Rare),      (30f, 3f) },
            { (EquipmentType.Armor, RarityType.Epic),      (45f, 4f) },
            { (EquipmentType.Armor, RarityType.Legendary), (60f, 5f) },
            // 장갑 : 치명타 데미지
            { (EquipmentType.Gloves, RarityType.Common),    (5f, 0.01f) },
            { (EquipmentType.Gloves, RarityType.Rare),      (8f, 0.02f) },
            { (EquipmentType.Gloves, RarityType.Epic),      (12f, 0.05f) },
            { (EquipmentType.Gloves, RarityType.Legendary), (20f, 0.1f) },
            // 신발 : 치명타 확률
            { (EquipmentType.Boots, RarityType.Common),    (2.5f, 0.005f) },
            { (EquipmentType.Boots, RarityType.Rare),      (4f, 0.01f) },
            { (EquipmentType.Boots, RarityType.Epic),      (6f, 0.025f) },
            { (EquipmentType.Boots, RarityType.Legendary), (10f, 0.05f) },
        };
}

[System.Serializable]
public class EquipmentInstance
{
    public string instanceID;
    public string templateID;
    public string charID;
    public EquipmentType equipmentType;
    public RarityType rarity;
    public int level;
    public bool isEquipped;
    // public bool isLocked; // 장비 잠금 기능이 있다면

    public float baseStat;
    public float statIncrease;

    [System.NonSerialized]
    public EquipmentSO template;

    public void InitializeStats()
    {
        var key = (equipmentType, rarity);

        if (EquipmentStatTable.table.TryGetValue(key, out var statData))
        {
            baseStat = statData.baseStat;
            statIncrease = statData.statIncrease;
        }
        else
        {
            Debug.LogWarning($"스탯 테이블에 {equipmentType}, {rarity} 조합이 없습니다.");
        }
    }

    public float GetStat()
    {
        return baseStat + level * statIncrease;
    }
}
