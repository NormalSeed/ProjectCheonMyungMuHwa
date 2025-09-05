using TMPro;
using UnityEngine;

public class TootipPanel : UIBase
{
    [SerializeField] private ItemSlot _itemSlot;
    [SerializeField] private TMP_Text _itemName;
    [SerializeField] private TMP_Text _itemExplane;


    public void SetShow(ItemData itemData)
    {
        _itemSlot.SetItem(itemData);
        _itemName.text = itemData.Name;
        _itemExplane.text = itemData.Detail;

        base.SetShow();
    }
}
