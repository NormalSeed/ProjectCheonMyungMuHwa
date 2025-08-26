using System;
using System.Collections.Generic;

public class CurrencyModel : ICurrencyModel
{
    private readonly Dictionary<CurrencyType, BigCurrency> _balances = new();

    public event Action<CurrencyType, BigCurrency> OnChanged;

    public bool Has(CurrencyType id)
    {
        return _balances.ContainsKey(id);
    }

    public BigCurrency Get(CurrencyType id)
    {
        return _balances.TryGetValue(id, out var value) ? value : new BigCurrency(0, 0);
    }

    public void Set(CurrencyType id, BigCurrency value)
    {
        _balances[id] = value;
        OnChanged?.Invoke(id, value);
    }

    public void Add(CurrencyType id, BigCurrency delta)
    {
        var current = Get(id);
        var updated = current + delta;
        _balances[id] = updated;
        OnChanged?.Invoke(id, updated);
    }

    public bool TrySpend(CurrencyType id, BigCurrency cost)
    {
        if (!Has(id)) return false;

        var current = _balances[id];
        if (current < cost) return false;

        var updated = current - cost;
        _balances[id] = updated;
        OnChanged?.Invoke(id, updated);
        return true;
    }

    public void Clear()
    {
        _balances.Clear();
    }

    public CurrencySaveData ToSaveData()
    {
        var list = new List<CurrencySaveEntry>(_balances.Count);
        foreach (var kv in _balances) {
            list.Add(new CurrencySaveEntry {
                id = kv.Key.ToString(),
                value = kv.Value.Value,
                tier = kv.Value.Tier
            });
        }

        return new CurrencySaveData { entries = list };
    }

    public void FromSaveData(CurrencySaveData data)
    {
        _balances.Clear();
        if (data.entries == null) return;

        foreach (var entry in data.entries) {
            if (!Enum.TryParse(entry.id, out CurrencyType id)) {
                UnityEngine.Debug.LogWarning($"[CurrencyModel] Unknown CurrencyId: {entry.id}");
                continue;
            }
            var value = new BigCurrency(entry.value, entry.tier);
            Set(id, value);
        }
    }
}
