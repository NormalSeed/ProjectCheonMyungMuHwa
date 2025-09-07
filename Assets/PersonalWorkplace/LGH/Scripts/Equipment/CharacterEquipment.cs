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

    public string charID;
    public EquipClass equipClass;
    public Dictionary<EquipmentType, EquipmentSlot> slots = new();

    private void OnEnable()
    {
        // 장착 상태 초기화
        equipmentService.RegisterCharacter(this);
    }

    
}
