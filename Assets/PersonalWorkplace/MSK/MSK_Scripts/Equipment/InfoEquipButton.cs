using UnityEngine;
using UnityEngine.UI;

public class InfoEquipButton : MonoBehaviour
{
    [Header("Button")]
    [SerializeField] private Button Button;

    [Header("EquipDisplay")]
    [SerializeField] private EquipmentCardDisplay EquipmentCardDisplay;
    [SerializeField] private EquipmentType equipType;

    private EquipmentInstance EquipmentInstance;

    private EquipClass equipClass;
    private string templateID;

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
        equipmentList.gameObject.SetActive(true);
        equipmentList.ShowEquipmentListByTemplateID(templateID);
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

    public void EquipmentSettingLoad
        (EquipClass input)
    {
        equipClass = input;
        templateID = $"{(int)equipClass}" + "_" + $"{equipType}";
    }
    public void EquipReset()
    {
        EquipmentInstance = null;
        EquipmentCardDisplay.SetData(EquipmentInstance);
    }
    #endregion
}
