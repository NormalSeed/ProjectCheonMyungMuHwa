using System;


public class GameCurrencyController
{
    private readonly CurrencyModel _model;

    public GameCurrencyController(CurrencyModel model)
    {
        _model = model ?? throw new ArgumentNullException(nameof(model));
    }

    /// <summary>
    /// 기본 재화들을 등록. 기본값은 0A.
    /// </summary>
    public void RegisterDefaults(
        BigCurrency? initJewel = null,
        BigCurrency? initGold = null,
        BigCurrency? initSoul = null,
        BigCurrency? initSpiritStone = null,
        BigCurrency? initSummonTicket = null)
    {
        Register(CurrencyIds.Jewel, initJewel ?? new BigCurrency(0, 0));
        Register(CurrencyIds.Gold, initGold ?? new BigCurrency(0, 0));
        Register(CurrencyIds.Soul, initSoul ?? new BigCurrency(0, 0));
        Register(CurrencyIds.SpiritStone, initSpiritStone ?? new BigCurrency(0, 0));
        Register(CurrencyIds.SummonTicket, initSummonTicket ?? new BigCurrency(0, 0));
    }

    public void Register(CurrencyId id, BigCurrency initial)
    {
        if (!_model.Has(id))
            _model.Set(id, initial);
    }

    public BigCurrency Get(CurrencyId id) => _model.Get(id);

    public void Set(CurrencyId id, BigCurrency amount)
    {
        _model.Set(id, amount);
    }

    public void Add(CurrencyId id, BigCurrency delta)
    {
        if (delta.Value <= 0) return;
        _model.Add(id, delta);
    }

    public bool CanSpend(CurrencyId id, BigCurrency cost) => _model.Get(id) >= cost;

    public bool Spend(CurrencyId id, BigCurrency cost)
    {
        if (cost.Value <= 0) return true;
        return _model.TrySpend(id, cost);
    }

    /// <summary>
    /// 퍼센트로 증가. 예: 5% 증가 = +5% = *1.05
    /// </summary>
    /// <param name="id"></param>
    /// <param name="percentPoints"></param>
    public void IncreaseByPercent(CurrencyId id, double percentPoints)
    {
        var cur = _model.Get(id);
        var next = cur * (1.0 + percentPoints / 100.0);
        _model.Set(id, next);
    }

    /// <summary>
    /// 포인트 단위로 증가. 예: 50 bps = +0.50% = *1.005
    /// </summary>
    /// <param name="id"></param>
    /// <param name="bps"></param>
    public void IncreaseByBps(CurrencyId id, int bps)
    {
        var cur = _model.Get(id);
        var next = cur * (1.0 + bps / 10000.0);
        _model.Set(id, next);
    }

    /// <summary>
    /// 배율로 증가. 예: 2배 = *2.0
    /// </summary>
    /// <param name="id"></param>
    /// <param name="rate"></param>
    public void Multiply(CurrencyId id, double rate)
    {
        var cur = _model.Get(id);
        var next = cur * rate;
        _model.Set(id, next);
    }

    public void ResetAll() => _model.Clear();
}

