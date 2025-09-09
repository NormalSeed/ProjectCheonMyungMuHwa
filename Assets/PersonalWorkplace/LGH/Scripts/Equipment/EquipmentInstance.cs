using System.Collections.Generic;
using UnityEngine;

public enum EquipClass { 검호 = 1, 호법 = 2, 도사 = 3, 살수 = 4 }
public enum EquipmentType { Weapon, Armor, Gloves, Boots }
public enum RarityType { Normal = 1, Rare = 2, Epic = 3, Unique = 4 }

public enum StatType { Attack, ExtAtk, InnAtk, Defense, CritDamage, CritRate, Health, BDamage, NDamage, SkillDamage }

public static class EquipmentStatTable
{
    public static readonly Dictionary<(EquipmentType, RarityType), (float baseStat, float statIncrease)> table =
        new()
        {
            // TODO : 추후 데이터 테이블 참고해서 수정해야 함
            // 무기 : 공격력
            { (EquipmentType.Weapon, RarityType.Normal),    (18f, 2f) },
            { (EquipmentType.Weapon, RarityType.Rare),      (32f, 3f) },
            { (EquipmentType.Weapon, RarityType.Epic),      (46f, 4f) },
            { (EquipmentType.Weapon, RarityType.Unique), (65f, 5f) },
            // 갑옷 : 방어력
            { (EquipmentType.Armor, RarityType.Normal),    (18f, 2f) },
            { (EquipmentType.Armor, RarityType.Rare),      (32f, 3f) },
            { (EquipmentType.Armor, RarityType.Epic),      (46f, 4f) },
            { (EquipmentType.Armor, RarityType.Unique), (65f, 5f) },
            // 장갑 : 치명타 데미지
            { (EquipmentType.Gloves, RarityType.Normal),    (5f, 0.01f) },
            { (EquipmentType.Gloves, RarityType.Rare),      (8f, 0.02f) },
            { (EquipmentType.Gloves, RarityType.Epic),      (12f, 0.05f) },
            { (EquipmentType.Gloves, RarityType.Unique), (20f, 0.1f) },
            // 신발 : 치명타 확률
            { (EquipmentType.Boots, RarityType.Normal),    (1f, 0.01f) },
            { (EquipmentType.Boots, RarityType.Rare),      (2f, 0.02f) },
            { (EquipmentType.Boots, RarityType.Epic),      (4f, 0.04f) },
            { (EquipmentType.Boots, RarityType.Unique), (8f, 0.08f) },
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
    public StatType statType;
    public int level;
    public bool isEquipped;
    // public bool isLocked; // 장비 잠금 기능이 있다면

    public float baseStat;
    public float statIncrease;
    public float appliedStatValue;

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
