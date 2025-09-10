using UnityEngine;
using UnityEngine.UI;

public class InfoEquipButton : MonoBehaviour
{
    [Header("Button")]
    [SerializeField] private Button Button;

    [Header("EquipDisplay")]
    [SerializeField] private EquipmentCardDisplay EquipmentCardDisplay;
    private EquipmentInstance EquipmentInstance;

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
