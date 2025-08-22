using TMPro;
using UnityEngine;
using VContainer;

public class CurrencyHud : MonoBehaviour
{
    [SerializeField] private TMP_Text _goldText;
    [SerializeField] private TMP_Text _jewelText;

    [Inject] private IGameCurrencyController _currency;  // 조작용
    [Inject] private ICurrencyModel _model;              // 조회/이벤트용

    private void Start()
    {
        // 최초 한 번 전체 표시
        RefreshAll();

        // 변경 이벤트 구독
        _model.OnChanged += OnCurrencyChanged;
    }

    private void OnDestroy()
    {
        _model.OnChanged -= OnCurrencyChanged;
    }

    private void OnCurrencyChanged(CurrencyId id, BigCurrency amount)
    {
        if (id == CurrencyIds.Gold) {
            _goldText.text = Format(amount);
        }
        else if (id == CurrencyIds.Jewel) {
            _jewelText.text = Format(amount);
        }
        // 필요하면 다른 재화도 추가
    }

    private void RefreshAll()
    {
        _goldText.text = Format(_currency.Get(CurrencyIds.Gold));
        _jewelText.text = Format(_currency.Get(CurrencyIds.Jewel));
    }

    private static string Format(BigCurrency v)
    {
        return v.ToString();
    }

    // 버튼 테스트용
    public void AddGold100()
    {
        _currency.Add(CurrencyIds.Gold, new BigCurrency(100, 0));
    }
}
