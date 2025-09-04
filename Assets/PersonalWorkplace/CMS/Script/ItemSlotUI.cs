using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemSlotUI : MonoBehaviour
{
    public Image itemImage;
    public TMP_Text amountText;

    // typeName은 나중에 아이템 타입 관리용
    public void SetSlot(string typeName, int amount, Sprite icon = null)
    {
        amountText.text = $"x{amount}";
        if (icon != null)
            itemImage.sprite = icon;
    }
}