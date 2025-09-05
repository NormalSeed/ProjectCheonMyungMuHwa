using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class ItemSlot : MonoBehaviour
{
    [SerializeField] private Image _itemImage;
    [SerializeField] private TMP_Text _itemAmount;

    private ItemData _itemData;
    private AsyncOperationHandle<Sprite>? _loadedHandle;

    public void SetItem(ItemData itemData, int itemAmount = 1)
    {
        _itemData = itemData;

        // 개수 텍스트
        _itemAmount.text = FormatItemAmount(itemAmount);

        // 아이콘 이미지 로드
        LoadItemImage(_itemData.ImageKey);
    }

    private string FormatItemAmount(int amount)
    {
        if (amount <= 1) {
            return string.Empty;
        }

        return "X" + amount.ToString();
    }

    private void LoadItemImage(string key)
    {
        ReleaseImageHandle(); // 기존 핸들 정리

        if (string.IsNullOrEmpty(key)) {
            Debug.LogWarning("[ItemSlot] ImageKey is empty");
            return;
        }

        _loadedHandle = Addressables.LoadAssetAsync<Sprite>(key);
        _loadedHandle.Value.Completed += handle => {
            if (handle.Status == AsyncOperationStatus.Succeeded) {
                _itemImage.sprite = handle.Result;
            }
            else {
                Debug.LogWarning($"[ItemSlot] Failed to load sprite: {key}");
            }
        };
    }

    private void ReleaseImageHandle()
    {
        if (_loadedHandle.HasValue) {
            Addressables.Release(_loadedHandle.Value);
            _loadedHandle = null;
        }
    }

    private void OnDestroy()
    {
        ReleaseImageHandle();
    }
}
