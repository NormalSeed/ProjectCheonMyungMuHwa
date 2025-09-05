using System.Collections.Generic;
using UnityEngine;

public class CurrencyChanger : MonoBehaviour
{
    #region serialized fields

    [SerializeField] private CurrencyUI _mainCurrencyUI;
    [SerializeField] private List<CurrencyUI> _currencyUIs;

    #endregion // serialized fields


    #region unity funcs

    private void Awake()
    {
        // 각 CurrencyUI 클릭 이벤트 연결
        foreach (var ui in _currencyUIs) {
            ui.OnCurrencyClicked = HandleCurrencyClicked;
        }
    }

    #endregion // unity funcs


    #region private funcs

    private void HandleCurrencyClicked(CurrencyType clickedType)
    {
        // 메인 UI의 타입을 변경
        _mainCurrencyUI.SetCurrencyType(clickedType);

        //test code
        CurrencyManager.Instance.Add(clickedType, new BigCurrency(50, 0));
    }

    #endregion // private funcs
}
