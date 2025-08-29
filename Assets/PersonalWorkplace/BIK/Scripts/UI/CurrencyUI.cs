using TMPro;
using UnityEngine;

public class CurrencyUI : MonoBehaviour
{
    #region serialize field

    [SerializeField] private CurrencyType _targetCurrency;
    [SerializeField] private TMP_Text _currencyText;

    #endregion // serialize field





    #region private field

    private ICurrencyModel _model;

    #endregion // private field





    #region mono funcs
    private void OnEnable()
    {
        CurrencyManager.OnInitialized += HandleInitialized;

        // 이미 초기화 끝났다면 즉시 실행
        if (CurrencyManager.Instance != null && CurrencyManager.Instance.IsInitialized) {
            HandleInitialized();
        }
    }

    private void OnDisable()
    {
        CurrencyManager.OnInitialized -= HandleInitialized;
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
    }

    #endregion // mono funcs





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

    #endregion // private funcs





    #region test code

    public void OnClick_Currency() // TODO : 삭제하기
    {
        CurrencyManager.Instance.Add(_targetCurrency, new BigCurrency(50, 0));
        //CurrencyManager.Instance.TrySpend(_targetCurrency, new BigCurrency(50, 0));
    }

    #endregion // test code
}
