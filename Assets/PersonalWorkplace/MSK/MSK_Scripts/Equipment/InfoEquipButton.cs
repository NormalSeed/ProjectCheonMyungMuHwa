using UnityEngine;
using UnityEngine.UI;

public class InfoEquipButton : MonoBehaviour
{
    [Header("Button")]
    [SerializeField] private Button Button;

    [Header("EquipDisplay")]
    [SerializeField] private EquipmentCardDisplay EquipmentCardDisplay;
    private EquipmentInstance EquipmentInstance;

    [Header("Panel")]
    [SerializeField] private EquipmentItemList equipmentList;

    #region 
    private void OnEnable()
    {
        Button.onClick.AddListener(OnClickEquipment);
    }
    private void OnDisable()
    {
        Button.onClick.RemoveListener(OnClickEquipment);
    }
    #endregion
    
    #region OnClick
    private void OnClickEquipment()
    {
        equipmentList.ShowEquipmentListByTemplateID(EquipmentInstance.templateID);
        equipmentList.gameObject.SetActive(true);
    }
    #endregion

    #region Private

    #endregion

    #region Public
    public void HeroEquipSet(EquipmentInstance instance)
    {
        EquipmentInstance = instance;
        EquipmentCardDisplay.SetData(EquipmentInstance);
    }
    #endregion
}
