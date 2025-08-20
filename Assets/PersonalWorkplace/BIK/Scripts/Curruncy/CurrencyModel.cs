using System;
using System.Collections.Generic;


public class CurrencyModel
{
    private readonly Dictionary<CurrencyId, BigCurrency> _balances = new();

    /// <summary>보유량 변경 시 알림 (id, newAmount)</summary>
    public event Action<CurrencyId, BigCurrency> OnBalanceChanged;

    public bool Has(CurrencyId id) => _balances.ContainsKey(id);

    public BigCurrency Get(CurrencyId id)
    {
        return _balances.TryGetValue(id, out var v) ? v : new BigCurrency(0, 0);
    }

    internal void Set(CurrencyId id, BigCurrency amount)
    {
        _balances[id] = new BigCurrency(amount.Value, amount.Tier);
        OnBalanceChanged?.Invoke(id, _balances[id]);
    }

    internal void Add(CurrencyId id, BigCurrency delta)
    {
        var cur = Get(id);
        Set(id, cur + delta);
    }

    internal bool TrySpend(CurrencyId id, BigCurrency cost)
    {
        var cur = Get(id);
        if (cur < cost) return false;
        Set(id, cur - cost);
        return true;
    }

    internal void Clear() => _balances.Clear();

    [Serializable]
    public struct Entry { public string id; public double value; public int tier; }

    [Serializable]
    public struct SaveData { public List<Entry> entries; }

    public SaveData ToSaveData()
    {
        var list = new List<Entry>(_balances.Count);
        foreach (var kv in _balances) {
            list.Add(new Entry { id = kv.Key.Key, value = kv.Value.Value, tier = kv.Value.Tier });
        }
        return new SaveData { entries = list };
    }

    public void FromSaveData(SaveData data)
    {
        _balances.Clear();
        if (data.entries == null) return;

        foreach (var e in data.entries) {
            var id = new CurrencyId(e.id);
            Set(id, new BigCurrency(e.value, e.tier));
        }
    }
}
