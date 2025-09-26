using System;
using UnityEngine;
using UnityEngine.UI;

public class DecompositionButton : MonoBehaviour
{
    [SerializeField] Button selectButton;                               // 선택 버튼
    [SerializeField] public EquipmentCardDisplay equipmentCardDisplay;  // 장비 UI
    [SerializeField] private Image selectImage;                         // 선택 표시 이미지
    [SerializeField] private Outline imageOutLine;                      // 선택 시 아웃라인

    public Action<DecompositionButton, bool> onSelectChanged;           // 선택
    private bool isSelected = false;
    public bool IsSelected => isSelected;

    private void OnEnable()
    {
        selectButton.onClick.AddListener(OnClickSelect);
    }

    private void OnDisable()
    {
        selectButton.onClick.RemoveListener(OnClickSelect);
    }

    private void OnClickSelect()
    {
        isSelected = !isSelected;

        selectImage.gameObject.SetActive(isSelected);
        imageOutLine.enabled = isSelected;

        Debug.Log($"장비 선택 상태 변경: {equipmentCardDisplay.name}, 선택됨: {isSelected}");
        onSelectChanged?.Invoke(this, isSelected);
    }


    public void Init(EquipmentInstance equip)
    {
        if (equipmentCardDisplay != null)
        {
            equipmentCardDisplay.SetData(equip);
        }

        selectImage.gameObject.SetActive(false);
        imageOutLine.enabled = false;
        isSelected = false;
    }

    public EquipmentInstance GetEquipment()
    {
        return equipmentCardDisplay != null ? equipmentCardDisplay.GetEquipment() : null;
    }
}
