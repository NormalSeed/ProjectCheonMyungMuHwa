using System;

public interface ICurrencyModel
{
    /// <summary>
    /// 재화 변동 시 이벤트
    /// </summary>
    event Action<CurrencyType, BigCurrency> OnChanged;

    bool Has(CurrencyType id);
    BigCurrency Get(CurrencyType id);
    void Set(CurrencyType id, BigCurrency value);
    void Add(CurrencyType id, BigCurrency delta);
    bool TrySpend(CurrencyType id, BigCurrency cost);
    void Clear();
}
