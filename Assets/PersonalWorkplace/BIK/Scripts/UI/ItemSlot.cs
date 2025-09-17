using System;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class ItemSlot : MonoBehaviour
{
    [SerializeField] private Image _itemImage;
    [SerializeField] private TMP_Text _itemAmountText;
    [SerializeField] private Button _button;   // 슬롯 클릭 버튼

    private ItemData _itemData;
    private int _itemAmount;
    private AsyncOperationHandle<Sprite>? _loadedHandle;

    public event Action<ItemSlot> OnClick;

    public ItemData Data => _itemData;
    public int Amount => _itemAmount;

    /// <summary>
    /// 슬롯에 아이템을 세팅
    /// </summary>
    public void SetItem(ItemData itemData, int itemAmount = 1)
    {
        if (itemData == null) {
            SetEmpty();
            return;
        }

        _itemData = itemData;
        _itemAmount = itemAmount;

        // 개수 텍스트
        _itemAmountText.text = FormatItemAmount(itemAmount);
        _itemAmountText.gameObject.SetActive(_itemAmount > 1);

        // 아이콘 이미지 로드
        LoadItemImage(_itemData.ImageKey);
        _itemImage.gameObject.SetActive(true);
    }

    /// <summary>
    /// 슬롯을 비움 (아이템 없음 상태)
    /// </summary>
    public void SetEmpty()
    {
        _itemData = null;
        _itemAmount = 0;

        _itemAmountText.text = string.Empty;
        _itemAmountText.gameObject.SetActive(false);

        _itemImage.sprite = null;
        _itemImage.gameObject.SetActive(false);

        ReleaseImageHandle();
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
            _itemImage.gameObject.SetActive(false);
            return;
        }

        _loadedHandle = Addressables.LoadAssetAsync<Sprite>(key);
        _loadedHandle.Value.Completed += handle => {
            if (handle.Status == AsyncOperationStatus.Succeeded) {
                _itemImage.sprite = handle.Result;
                _itemImage.gameObject.SetActive(true);
            }
            else {
                Debug.LogWarning($"[ItemSlot] Failed to load sprite: {key}");
                _itemImage.gameObject.SetActive(false);
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

    private void Awake()
    {
        if (_button != null)
            _button.onClick.AddListener(() => OnClick?.Invoke(this));
    }

    private void OnDestroy()
    {
        ReleaseImageHandle();
    }
}
