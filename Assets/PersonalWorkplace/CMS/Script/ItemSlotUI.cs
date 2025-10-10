using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemSlotUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TMP_Text amountText;  // 아이템 수량 표시
    [SerializeField] private Image itemImage;      // 아이콘 표시

    public void SetSlot(string typeName, string amount, Sprite icon = null)
    {
        if (amountText != null)
            amountText.text = $"x{amount}";
        if (icon != null && itemImage != null)
            itemImage.sprite = icon;
    }
}