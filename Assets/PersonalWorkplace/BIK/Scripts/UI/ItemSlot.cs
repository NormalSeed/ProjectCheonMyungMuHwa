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
    private BigCurrency _itemAmount;
    private AsyncOperationHandle<Sprite>? _loadedHandle;

    public event Action<ItemSlot> OnClick;

    public ItemData Data => _itemData;
    public BigCurrency Amount => _itemAmount;

    /// <summary>
    /// 슬롯에 아이템을 세팅 (기본 수량 1A)
    /// </summary>
    public void SetItem(ItemData itemData)
    {
        SetItem(itemData, new BigCurrency(1.0, 0));
    }

    /// <summary>
    /// 슬롯에 아이템을 세팅
    /// </summary>
    public void SetItem(ItemData itemData, BigCurrency itemAmount)
    {
        if (itemData == null) {
            SetEmpty();
            return;
        }

        // null 수량이 들어오면 1A로 보정
        if (itemAmount == null) {
            itemAmount = new BigCurrency(1.0, 0);
        }

        _itemData = itemData;
        _itemAmount = itemAmount;

        // 개수 텍스트
        _itemAmountText.text = FormatItemAmount(_itemAmount);
        _itemAmountText.gameObject.SetActive(!IsLessOrEqualOne(_itemAmount));

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
        _itemAmount = new BigCurrency(0.0, 0);

        _itemAmountText.text = string.Empty;
        _itemAmountText.gameObject.SetActive(false);

        _itemImage.sprite = null;
        _itemImage.gameObject.SetActive(false);

        ReleaseImageHandle();
    }

    private bool IsLessOrEqualOne(BigCurrency amount)
    {
        // 1A와 비교
        return amount == null || amount <= new BigCurrency(1.0, 0);
    }

    private string FormatItemAmount(BigCurrency amount)
    {
        if (amount == null || IsLessOrEqualOne(amount)) {
            return string.Empty;
        }

        // 티어 0(무단위)이면 정수만, 접두어 'X'도 붙이지 않음
        if (amount.Tier == 0) {
            // 무단위 값은 그냥 base 수량과 동일
            int whole = (int)Math.Round(amount.Value);
            return whole.ToString();
        }

        // 티어 1 이상(A,B,...)이면 소수점 포함 + 단위, 필요하면 'X' 접두어 유지
        return "X" + amount.ToString(); // 예: X1.25A
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
