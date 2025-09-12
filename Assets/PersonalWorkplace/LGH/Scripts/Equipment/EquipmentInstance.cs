using UnityEngine;

public enum EquipClass { 검호 = 1, 호법 = 2, 도사 = 3, 살수 = 4 }
public enum EquipmentType { Weapon, Armor, Gloves, Boots }
public enum RarityType { Normal = 1, Rare = 2, Epic = 3, Unique = 4 }

public enum StatType 
{ 
    Attack,      // 공격력
    ExtAtk,      // 외공
    InnAtk,      // 내공
    Defense,     // 방어력
    CritDamage,  //치명타 배율
    CritRate,    //차명타 확률
    Health,      // 채력
    BDamage,     // 보스 데미지
    NDamage,     // 일반 데미지
    SkillDamage  // 스킬 데미지
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

    public float baseStat;          // 기본 장비 능력치
    public float statIncrease;      // 장비 레벨당 성장률
    public float appliedStatValue;  // 최종 장비 능력치

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
    
    public float GetNextLevelStat()
    {
        return baseStat + (level+1) * statIncrease;
    }
}
