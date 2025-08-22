using System;

public interface ICurrencyModel
{
    event Action<CurrencyId, BigCurrency> OnChanged;

    bool Has(CurrencyId id);
    BigCurrency Get(CurrencyId id);
    void Set(CurrencyId id, BigCurrency value);
    void Add(CurrencyId id, BigCurrency delta);
    bool TrySpend(CurrencyId id, BigCurrency cost);
    void Clear();
}
