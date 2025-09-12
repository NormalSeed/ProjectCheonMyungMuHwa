using System.Collections.Generic;

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
