using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CurrencyConfig", menuName = "Configs/CurrencyConfig")]
public class CurrencyConfig : ScriptableObject
{
    [Serializable]
    public class Entry
    {
        public CurrencyType type;
        public int itemId;
    }

    [SerializeField] private List<Entry> _entries = new();

    private Dictionary<CurrencyType, int> _map;

    private void OnEnable()
    {
        _map = new Dictionary<CurrencyType, int>();
        foreach (var e in _entries)
            _map[e.type] = e.itemId;
    }

    public int GetItemId(CurrencyType type)
    {
        return _map.TryGetValue(type, out var id) ? id : -1;
    }
}
