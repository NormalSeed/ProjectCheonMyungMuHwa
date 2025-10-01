using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class EquipmentItemList : MonoBehaviour
{
    [Inject] private EquipmentService equipmentService;
    [Inject] private EquipmentManager equipmentManager;

    [Header("Panel")]
    [SerializeField] public EquipmentInfoPanel equipPanel;             // 장비 패널
    [SerializeField] public DecompositionEquipment decompositionPanel; // 분해 패널
    [Header("Pool")]
    [SerializeField] private GachaCardPoolManager cardPoolManager;  // 풀메니저

    [Header("Button")]
    [SerializeField] private Button eixtButton;                     // 나가기 버튼
    [SerializeField] private Button decompositionButton;            // 분해 버튼


    public List<InventoryEquipButton> activeEquipButtons = new();
    public string thisTemplateID;

    #region Unity
    private void OnEnable()
    {
        eixtButton.onClick.AddListener(OnClickExitButton);
        decompositionButton.onClick.AddListener(OnClicKDecompositionButton);
    }

    private void OnDisable()
    {
        eixtButton.onClick.RemoveListener(OnClickExitButton);
        decompositionButton.onClick.RemoveListener(OnClicKDecompositionButton);
        equipPanel.gameObject.SetActive(false);
    }
    #endregion

    #region OnClick
    private void OnClickExitButton()
    {
        this.gameObject.SetActive(false);
        AudioManager.Instance.PlaySound("5. 팝업 닫을 때 사운드");
    }
    private void OnClicKDecompositionButton()
    {
        AudioManager.Instance.PlaySound("6. 팝업 열 때 사운드");
        decompositionPanel.gameObject.SetActive(true);
        decompositionPanel.ShowEquipmentListByEquip();
    }
    #endregion
    public void ShowEquipmentListByTemplateID(string templateID)
    {
        cardPoolManager.ReturnAll(); // 기존 카드 초기화
        activeEquipButtons.Clear();
        thisTemplateID = templateID;
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
