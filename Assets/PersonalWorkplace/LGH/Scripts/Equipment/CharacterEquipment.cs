using System.Collections.Generic;
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

    public void SetCharID()
    {
        if (controller.model.modelSO == null) return;
        charID = controller.model.modelSO.CharID;
        equipClass = GetEquipClass();
    }

    private EquipClass GetEquipClass()
    {
        switch (controller.model.modelSO.Role)
        {
            case "검호":
                return EquipClass.검호;
            case "호법":
                return EquipClass.호법;
            case "도사":
                return EquipClass.도사;
            case "살수":
                return EquipClass.살수;
            default:
                return EquipClass.검호;
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
        //var value = instance.GetStat();

        //var stats = controller.model.modelSO;

        //switch (instance.statType)
        //{
        //    case StatType.Attack:
        //        increasedExtAttackPoint = stats.ExtPow * value / 100;
        //        increasedInnAttackPoint = stats.InnPow * value / 100;
        //        stats.ExtAtkPoint += increasedExtAttackPoint;
        //        stats.InnAtkPoint += increasedInnAttackPoint;
        //        break;
        //    case StatType.Defense:
        //        increasedDefensePoint = (float)stats.DefPoint * value / 100;
        //        stats.DefPoint += increasedDefensePoint;
        //        break;
        //    case StatType.CritDamage:
        //        increasedCritDamage = value / 100;
        //        stats.CritDamage += increasedCritDamage;
        //        break;
        //    case StatType.CritRate:
        //        increasedCritRate = value / 100;
        //        stats.CritRate += increasedCritRate;
        //        break;
        //    default:
        //        Debug.LogWarning($"알 수 없는 StatType: {instance.statType}");
        //        break;
        //}
        var value = instance.GetStat();
        string originID = $"Equip_{instance.instanceID}"; // 고유 식별자

        if (controller?.model?.modelSO == null)
            return;

        switch (instance.statType)
        {
            case StatType.Attack:
                StatModifierManager.ApplyModifier(charID,
                    new StatModifier(StatType.Attack, value / 100f, ModifierSource.Equipment, originID, true));
                StatModifierManager.ApplyModifier(charID,
                    new StatModifier(StatType.Attack, value / 100f, ModifierSource.Equipment, originID, true));
                break;

            case StatType.Defense:
                StatModifierManager.ApplyModifier(charID,
                    new StatModifier(StatType.Defense, value / 100f, ModifierSource.Equipment, originID, true));
                break;

            case StatType.CritRate:
                StatModifierManager.ApplyModifier(charID,
                    new StatModifier(StatType.CritRate, value / 100f, ModifierSource.Equipment, originID));
                break;

            case StatType.CritDamage:
                StatModifierManager.ApplyModifier(charID,
                    new StatModifier(StatType.CritDamage, value / 100f, ModifierSource.Equipment, originID));
                break;

            default:
                Debug.LogWarning($"알 수 없는 StatType: {instance.statType}");
                break;
        }

        StatModifierManager.ApplyToModel(controller.model);
    }
    public void RemoveStats(EquipmentInstance instance)
    {
        string originID = $"Equip_{instance.instanceID}";

        if (controller?.model?.modelSO == null)
            return;

        StatModifierManager.RemoveModifiersByOrigin(charID, originID);
        StatModifierManager.ApplyToModel(controller.model);
    }
}
