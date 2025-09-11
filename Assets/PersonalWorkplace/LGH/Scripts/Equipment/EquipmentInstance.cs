using UnityEngine;

public enum EquipClass { 검호 = 1, 호법 = 2, 도사 = 3, 살수 = 4 }
public enum EquipmentType { Weapon, Armor, Gloves, Boots }
public enum RarityType { Normal = 1, Rare = 2, Epic = 3, Unique = 4 }

public enum StatType { Attack, ExtAtk, InnAtk, Defense, CritDamage, CritRate, Health, BDamage, NDamage, SkillDamage }

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
