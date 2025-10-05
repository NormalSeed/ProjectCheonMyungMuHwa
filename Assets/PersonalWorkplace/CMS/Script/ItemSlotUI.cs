using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.AddressableAssets;

public class ItemSlotUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TMP_Text amountText;  // 아이템 수량 표시
    [SerializeField] private Image itemImage;      // 아이콘 표시

    public void SetSlot(CurrencyType type, string amount, Sprite icon = null)
    {
        if (amountText != null)
            amountText.text = $"x{amount}";
        if (icon != null && itemImage != null)
        {
            switch (type)
            {
                case CurrencyType.Gold: LoadIcon("GoldImage"); break;
                case CurrencyType.Soul: LoadIcon("SoulImage"); break;
                case CurrencyType.SpiritStone: LoadIcon("SpiritImage"); break;
                case CurrencyType.SummonTicket: LoadIcon("HeroTicketImage"); break;
                case CurrencyType.EquipmentSummonTicket: LoadIcon("EquipTicketImage"); break;
            }
        }
    }
    private void LoadIcon(string key)
    {
        AsyncOperationHandle<Sprite>? _loadedHandle = Addressables.LoadAssetAsync<Sprite>(key);
        _loadedHandle.Value.Completed += handle =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                itemImage.sprite = handle.Result;
            }
            else
            {
                Debug.LogWarning($"[ItemSlot] Failed to load sprite: {key}");
            }
        };
    }

}