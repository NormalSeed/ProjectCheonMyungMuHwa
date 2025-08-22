using UnityEngine;

public class TestCurrencyButton : MonoBehaviour
{
    [SerializeField] private CurrencyHud _hud;                     // 씬의 CurrencyHud 참조
    [SerializeField] private CurrencyHud.CurrencyType _type;       // 이 버튼이 늘릴 통화

    public void Add()
    {
        _hud.OnClick_Currency(_type);
    }
}
