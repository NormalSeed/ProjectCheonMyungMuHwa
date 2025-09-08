using System.Collections.Generic;
using Unity.Android.Gradle.Manifest;
using UnityEngine;
using VContainer;

public class EquipmentSlot
{
    public EquipmentType slotType;
    public EquipmentInstance equippedItem;
}

public class CharacterEquipment : MonoBehaviour
{
    [Inject]
    private EquipmentService equipmentService;

    private PlayerController controller;

    public string charID;
    public EquipClass equipClass;
    public Dictionary<EquipmentType, EquipmentSlot> slots = new();

    public float increasedExtAttackPoint;
    public float increasedInnAttackPoint;
    public float increasedDefensePoint;
    public float increasedCritRate;
    public float increasedCritDamage;

    private void Start()
    {
        controller = GetComponent<PlayerController>();
        charID = controller.model.modelSO.CharID;
        equipClass = GetEquipClass();

        // 슬롯 4개(Weapon, Armor, Gloves, Boots) 만들어 두기
        foreach (EquipmentType type in System.Enum.GetValues(typeof(EquipmentType)))
        {
            slots[type] = new EquipmentSlot { slotType = type };
        }
    }

    private void OnEnable()
    {
        // 장착 상태 초기화
        equipmentService.RegisterCharacter(this);
    }

    private EquipClass GetEquipClass()
    {
        switch (controller.model.modelSO.Position)
        {
            case 1:
                return EquipClass.Front;
            case 2:
                return EquipClass.Middle;
            case 3:
                return EquipClass.Back;
            default:
                return EquipClass.Front;
        }
    }

    public void InitializeEquippedSlotsFromManager(List<EquipmentInstance> allEquipments)
    {
        foreach (var item in allEquipments)
        {
            if (item.charID != charID || !item.isEquipped)
                continue;

            if (!slots.TryGetValue(item.equipmentType, out var slot))
            {
                Debug.LogWarning($"슬롯 타입 {item.equipmentType}이 존재하지 않습니다.");
                continue;
            }

            equipmentService.EquipToCharacter(item.charID, item);
        }
    }


    public void ApplyStats(EquipmentInstance instance)
    {
        var value = instance.GetStat();

        var stats = controller.model.modelSO;

        switch (instance.statType)
        {
            case StatType.Attack:
                increasedExtAttackPoint = stats.ExtPow * value / 100;
                increasedInnAttackPoint = stats.InnPow * value / 100;
                stats.ExtAtkPoint += increasedExtAttackPoint;
                stats.InnAtkPoint += increasedInnAttackPoint;
                break;
            case StatType.Defense:
                increasedDefensePoint = (float)stats.DefPoint * value / 100;
                stats.DefPoint += increasedDefensePoint;
                break;
            case StatType.CritRate:
                increasedCritRate = value / 100;
                stats.CritRate += increasedCritRate;
                break;
            case StatType.CritDamage:
                increasedCritDamage = value / 100;
                stats.CritDamage += increasedCritDamage;
                break;
            default:
                Debug.LogWarning($"알 수 없는 StatType: {instance.statType}");
                break;
        }
    }
    public void RemoveStats(EquipmentInstance instance)
    {
        var value = instance.GetStat();

        var stats = controller.model.modelSO;

        switch (instance.statType)
        {
            case StatType.Attack:
                stats.ExtAtkPoint -= increasedExtAttackPoint;
                stats.InnAtkPoint -= increasedInnAttackPoint;
                increasedExtAttackPoint = 0;
                increasedInnAttackPoint = 0;
                break;
            case StatType.Defense:
                stats.DefPoint -= increasedDefensePoint;
                increasedDefensePoint = 0;
                break;
            case StatType.CritRate:
                stats.CritRate += increasedCritRate;
                increasedCritRate = 0;
                break;
            case StatType.CritDamage:
                stats.CritDamage += increasedCritDamage;
                increasedCritDamage = 0;
                break;
            default:
                Debug.LogWarning($"알 수 없는 StatType: {instance.statType}");
                break;
        }
    }
}
