using UnityEngine;

[CreateAssetMenu(fileName = "EquipmentSO", menuName = "Game/EquipmentSO")]
public class EquipmentSO : ScriptableObject
{
    public string templateID;           // 템플릿 ID
    public EquipClass equipClass;       // 장착 가능 클래스(검호, 호법, 도사, 살수)
    public EquipmentType equipmentType; // 장비 타입(Weapon, Armor, Gloves, Boots)
    public StatType statType;           // 장비가 올려줄 능력치
    public Sprite icon;                 // 장비 아이콘 스프라이트
    public string equipmentName;        // 장비 이름
    public string description;          // 장비 설명
}
