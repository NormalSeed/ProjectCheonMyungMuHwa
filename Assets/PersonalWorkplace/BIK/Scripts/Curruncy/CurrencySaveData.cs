using System;
using System.Collections.Generic;

[Serializable]
public struct CurrencySaveEntry
{
    public string id;
    public double value;
    public int tier;
}

[Serializable]
public struct CurrencySaveData
{
    public List<CurrencySaveEntry> entries;
}
