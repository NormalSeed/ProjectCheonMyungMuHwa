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

    private void Start()
    {
        controller = GetComponent<PlayerController>();
        charID = controller.model.modelSO.CharID;
        equipClass = GetEquipClass();
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
}
