using System.Collections.Generic;
using UnityEngine;
using VContainer;

public class EquipmentItemList : MonoBehaviour
{
    [Inject] private EquipmentService equipmentService;
    [Inject] private EquipmentManager equipmentManager;

    [Header("Pool")]
    [SerializeField] GachaCardPoolManager cardPoolManager;

    public void ShowEquipmentListByTemplateID(string templateID)
    {
        cardPoolManager.ReturnAll(); // 기존 카드 초기화

        var filtered = equipmentManager.allEquipments.FindAll(e => e.templateID == templateID);

        Debug.Log($"[ShowEquipmentListByTemplateID] templateID: {templateID}, 장비 수: {filtered.Count}");

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
                Debug.LogWarning($"[ShowEquipmentListByTemplateID] 카드에 EquipmentCardDisplay가 없습니다 - {equip.templateID}");
            }

            card.transform.SetAsLastSibling();
            card.SetActive(true);
        }
    }

}
