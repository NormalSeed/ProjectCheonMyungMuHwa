using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class EquipmentItemList : MonoBehaviour
{
    [Inject] private EquipmentService equipmentService;
    [Inject] private EquipmentManager equipmentManager;

    [Header("Panel")]
    [SerializeField] public EquipmentInfoPanel equipPanel;

    [Header("Pool")]
    [SerializeField] private GachaCardPoolManager cardPoolManager;
    [Header("Button")]
    [SerializeField] private Button button;

    public List<InventoryEquipButton> activeEquipButtons = new();

    #region Unity
    private void OnEnable()
    {
        button.onClick.AddListener(OnClickExitButton);
    }

    private void OnDisable()
    {
        button.onClick.RemoveListener(OnClickExitButton);
        equipPanel.gameObject.SetActive(false);
    }
    #endregion

    #region OnClick
    private void OnClickExitButton()
    {
        this.gameObject.SetActive(false);
    }
    #endregion
    public void ShowEquipmentListByTemplateID(string templateID)
    {
        cardPoolManager.ReturnAll(); // 기존 카드 초기화
        activeEquipButtons.Clear();

        var filtered = equipmentManager.allEquipments.FindAll(e => e.templateID == templateID);

        Debug.Log($"[ShowEquipmentListByTemplateID] templateID: {templateID}, 장비 수: {filtered.Count}");
        foreach (var equip in filtered)
        {
            var card = cardPoolManager.GetCard();
            var display = card.GetComponent<EquipmentCardDisplay>();
            var button = card.GetComponent<InventoryEquipButton>();

            if (display != null) display.SetData(equip);
            if (button != null)
            {
                button.Init(equipPanel, equip);
                activeEquipButtons.Add(button);
            }

            card.transform.SetAsLastSibling();
            card.SetActive(true);
        }
    }
}
