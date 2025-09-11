using UnityEngine;
using UnityEngine.UI;

public class InventoryEquipButton : MonoBehaviour
{
    [SerializeField] Button button;

    private GameObject panel;
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
        panel.SetActive(true);
    }
    #endregion

    #region Public 
    public void Init(GameObject input, EquipmentInstance equip)
    {
        panel = input;
        equipmentInstance = equip;
    }
    #endregion
}
