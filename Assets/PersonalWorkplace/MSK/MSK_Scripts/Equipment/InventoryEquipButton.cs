using UnityEngine;
using UnityEngine.UI;

public class InventoryEquipButton : MonoBehaviour
{
    [SerializeField] Button button;

    private EquipmentInfoPanel panel;
    private EquipmentInstance equipmentInstance;

    #region Unity
    private void OnEnable()
    {
        button.onClick.AddListener(OnClickEquipButton);
    }
    private void OnDisable()
    {
        button.onClick.RemoveListener(OnClickEquipButton);
    }
    #endregion

    #region OnClick
    private void OnClickEquipButton()
    {
        panel.GetEquipmentInstance(equipmentInstance);
        panel.gameObject.SetActive(true);
        panel.Init();
    }
    #endregion

    #region Public 
    public void Init(EquipmentInfoPanel input, EquipmentInstance equip)
    {
        panel = input;
        equipmentInstance = equip;
    }
    #endregion
}
