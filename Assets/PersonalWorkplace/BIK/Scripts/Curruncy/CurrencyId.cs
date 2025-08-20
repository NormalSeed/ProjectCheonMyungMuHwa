using System;
using System.Collections.Generic;


public readonly struct CurrencyId : IEquatable<CurrencyId>
{
    public string Key { get; }

    public CurrencyId(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("CurrencyId key cannot be null or empty.", nameof(key));
        Key = key.Trim();
    }

    public override string ToString() => Key;

    public bool Equals(CurrencyId other) =>
        string.Equals(Key, other.Key, StringComparison.Ordinal);

    public override bool Equals(object obj) =>
        obj is CurrencyId other && Equals(other);

    public override int GetHashCode() =>
        StringComparer.Ordinal.GetHashCode(Key);

    public static implicit operator CurrencyId(string key) => new CurrencyId(key);
}

public static class CurrencyIds
{
    public static readonly CurrencyId Jewel = "Jewel";                 // 용옥(다이아)
    public static readonly CurrencyId Gold = "Gold";                   // 금화
    public static readonly CurrencyId Soul = "Soul";                   // 혼백(경험치)
    public static readonly CurrencyId SpiritStone = "SpiritStone";     // 영석(보물 강화)
    public static readonly CurrencyId SummonTicket = "SummonTicket";   // 등용패(뽑기권)

    public static IEnumerable<CurrencyId> All {
        get {
            yield return Jewel;
            yield return Gold;
            yield return Soul;
            yield return SpiritStone;
            yield return SummonTicket;
        }
    }
}