using System;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;
using VContainer;

public class CurrencyUI : MonoBehaviour
{
    #region serialize field

    [SerializeField] private CurrencyType _targetCurrency;
    [SerializeField] private TMP_Text _currencyText;
    [SerializeField] private Image _currencyImage;
    [SerializeField] private CurrencyConfig _config; // CurrencyType ↔ Item_ID 매핑 SO

    #endregion // serialize field





    #region private field

    private ICurrencyModel _model;
    private AsyncOperationHandle<Sprite>? _loadedHandle;
    [Inject]
    private TableManager _tableManager; // VContainer 주입

    #endregion // private field





    #region public events

    public Action<CurrencyType> OnCurrencyClicked;

    #endregion // public events





    #region DI

    [Inject]
    public void Construct(TableManager tableManager)
    {
        _tableManager = tableManager;
    }

    #endregion // DI





    #region mono funcs

    private void OnEnable()
    {
        CurrencyManager.OnInitialized += HandleInitialized;

        if (CurrencyManager.Instance != null && CurrencyManager.Instance.IsInitialized) {
            HandleInitialized();
        }
    }

    private void OnDisable()
    {
        CurrencyManager.OnInitialized -= HandleInitialized;
    }

    private void Start()
    {
        LoadCurrencyImage(_targetCurrency);
    }

    private void HandleInitialized()
    {
        _model = CurrencyManager.Instance.Model;

        var currentValue = _model.Get(_targetCurrency);
        _currencyText.text = FormatCurrency(currentValue);

        _model.OnChanged += OnCurrencyChanged;
    }

    private void OnDestroy()
    {
        if (_model != null)
            _model.OnChanged -= OnCurrencyChanged;

        ReleaseImageHandle();
    }

    #endregion // mono funcs





    #region public funcs

    public void SetCurrencyType(CurrencyType type)
    {
        _targetCurrency = type;

        if (_model != null) {
            var currentValue = _model.Get(_targetCurrency);
            _currencyText.text = FormatCurrency(currentValue);
        }

        LoadCurrencyImage(_targetCurrency);
    }

    public void OnClick_Currency()
    {
        if (_tableManager == null) {
            Debug.LogWarning("[CurrencyUI] TableManager가 아직 주입되지 않았습니다.");
            return;
        }

        int itemId = _config.GetItemId(_targetCurrency);
        if (itemId == -1) {
            Debug.LogWarning($"[CurrencyUI] {_targetCurrency} → ItemId 매핑 실패");
            return;
        }

        var itemTable = _tableManager.GetTable<TItem>(TableType.Item);
        if (itemTable == null || !itemTable.IsInitialized) {
            Debug.LogWarning("[CurrencyUI] ItemTable이 초기화되지 않았습니다.");
            return;
        }

        var itemData = itemTable.GetItem(itemId);
        if (itemData == null) {
            Debug.LogWarning($"[CurrencyUI] ItemId {itemId}에 해당하는 데이터가 없습니다.");
            return;
        }

        // Tooltip 표시
        PopupManager.Instance.ShowTooltip(itemData);
    }

    public void OnClick_Text()
    {
        OnCurrencyClicked?.Invoke(_targetCurrency);
    }

    #endregion // public funcs





    #region private funcs

    private void OnCurrencyChanged(CurrencyType id, BigCurrency value)
    {
        if (id == _targetCurrency) {
            _currencyText.text = FormatCurrency(value);
        }
    }

    private string FormatCurrency(BigCurrency currency)
    {
        return currency.ToString();
    }

    private void LoadCurrencyImage(CurrencyType type)
    {
        ReleaseImageHandle();

        int itemId = _config.GetItemId(type);
        if (itemId == -1) return;

        var itemTable = _tableManager.GetTable<TItem>(TableType.Item);
        if (itemTable == null || !itemTable.IsInitialized) return;

        var itemData = itemTable.GetItem(itemId);
        if (itemData == null) return;

        string key = itemData.ImageKey;

        _loadedHandle = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<Sprite>(key);
        _loadedHandle.Value.Completed += handle => {
            if (handle.Status == AsyncOperationStatus.Succeeded) {
                _currencyImage.sprite = handle.Result;
            }
            else {
                Debug.LogWarning($"[CurrencyUI] Failed to load sprite: {key}");
            }
        };
    }

    private void ReleaseImageHandle()
    {
        if (_loadedHandle.HasValue) {
            UnityEngine.AddressableAssets.Addressables.Release(_loadedHandle.Value);
            _loadedHandle = null;
        }
    }

    #endregion // private funcs
}
