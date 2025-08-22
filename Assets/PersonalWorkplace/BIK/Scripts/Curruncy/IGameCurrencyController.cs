public interface IGameCurrencyController
{
    void RegisterDefaults(BigCurrency? initJewel = null,
                          BigCurrency? initGold = null,
                          BigCurrency? initSoul = null,
                          BigCurrency? initSpiritStone = null,
                          BigCurrency? initSummonTicket = null);

    void Register(CurrencyId id, BigCurrency initial);
    BigCurrency Get(CurrencyId id);
    void Set(CurrencyId id, BigCurrency amount);
    void Add(CurrencyId id, BigCurrency delta);
    bool CanSpend(CurrencyId id, BigCurrency cost);
    bool Spend(CurrencyId id, BigCurrency cost);
    void IncreaseByPercent(CurrencyId id, double percentPoints);
    void IncreaseByBps(CurrencyId id, int bps);
    void Multiply(CurrencyId id, double rate);
    void ResetAll();
}