using System.Collections.Generic;
using UnityEngine;
using VContainer;

public class EquipmentItemList : MonoBehaviour
{
    [Inject] private EquipmentService equipmentService;
    [Inject] private EquipmentManager equipmentManager;

    [Header("Pool")]
    [SerializeField] GachaCardPoolManager cardPoolManager;

    public void ShowEquipmentListByType(EquipmentType type)
    {
        cardPoolManager.ReturnAll(); // 기존 카드 초기화

        var filtered = equipmentManager.allEquipments.FindAll(e => e.equipmentType == type);

        Debug.Log($"[ShowEquipmentListByType] {type} 장비 {filtered.Count}개 표시");

        foreach (var equip in filtered)
        {
            var card = cardPoolManager.GetCard();
            var display = card.GetComponent<EquipmentCardDisplay>();

            if (display != null)
            {
                display.SetData(equip);
            }
            else
            {
                Debug.LogWarning($"[ShowEquipmentListByType] 카드에 EquipmentCardDisplay가 없습니다 - {equip.templateID}");
            }

            card.transform.SetAsLastSibling();
            card.SetActive(true);
        }
    }
}
