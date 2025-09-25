using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class EquipmentChange : MonoBehaviour
{
    [Inject] private readonly EquipmentService equipmentService;
    [Inject] private readonly EquipmentManager equipmentManager;

    [Header("EquipmentCardDisplay")]
    [SerializeField] private EquipmentCardDisplay equipmentImage;
    [SerializeField] private EquipmentInfoPanel infoPanel;

    [Header("Button")]
    [SerializeField] private Button submitButton;
    [SerializeField] private Button cancelButton;


    private EquipmentInstance instance;     // 판넬의 장비

    private string thisCharId;              // 새로 장착할 대상 영웅 ID
    private string oldCharID;               // 기존 장착중인 대상 영웅 ID
    private string instanceID;              // 장비 ID


}
