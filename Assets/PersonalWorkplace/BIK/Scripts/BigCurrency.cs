using System;

public sealed class BigCurrency : IComparable<BigCurrency>, IEquatable<BigCurrency>
{
    // 1000A = 1B 규칙
    private const double BaseUnit = 1000.0;

    // 0 <= Value < 1000 (Value==0이면 Tier==0 유지)
    public double Value { get; private set; }
    public int Tier { get; private set; }

    public BigCurrency(double value = 0.0, int tier = 0)
    {
        if (value < 0) throw new ArgumentOutOfRangeException(nameof(value), "Value must be non-negative.");
        if (tier < 0) throw new ArgumentOutOfRangeException(nameof(tier), "Tier must be non-negative.");

        Value = value;
        Tier = tier;
        Normalize();
    }

    /// <summary>
    /// 다른 단위와 합치거나 연산 후, 1 <= Value < 1000 형태(혹은 0)로 정규화.
    /// </summary>
    private void Normalize()
    {
        if (Value <= 0) {
            Value = 0;
            Tier = 0;
            return;
        }

        // 위로 승급
        while (Value >= BaseUnit) {
            Value /= BaseUnit;
            Tier++;
        }

        // 아래로 강등 (뺄셈 후 등)
        while (Value < 1.0 && Tier > 0) {
            Value *= BaseUnit;
            Tier--;
        }

        // 아주 작은 값은 0으로 스냅
        if (Tier == 0 && Value < 1e-9) {
            Value = 0;
        }
    }

    /// <summary>
    /// a를 b의 티어로 스케일해 a' 값을 반환 (a는 변경 안 함)
    /// </summary>
    private static double ScaledValue(BigCurrency a, int targetTier)
    {
        if (a.Value == 0) return 0;
        int diff = a.Tier - targetTier;
        if (diff == 0) return a.Value;

        // a를 targetTier로 환산
        return a.Value * Math.Pow(BaseUnit, diff);
    }

    // --------------------
    // 사칙연산
    // --------------------
    public static BigCurrency operator +(BigCurrency a, BigCurrency b)
    {
        if (a is null || b is null) throw new ArgumentNullException();
        // 더 큰 티어 기준으로 합산
        int t = Math.Max(a.Tier, b.Tier);
        double sum = ScaledValue(a, t) + ScaledValue(b, t);
        return new BigCurrency(sum, t);
    }

    public static BigCurrency operator -(BigCurrency a, BigCurrency b)
    {
        if (a is null || b is null) throw new ArgumentNullException();
        int t = Math.Max(a.Tier, b.Tier);
        double diff = ScaledValue(a, t) - ScaledValue(b, t);
        if (diff <= 0) return new BigCurrency(0, 0);
        return new BigCurrency(diff, t);
    }

    // 스칼라 곱/나눗셈 (배수)
    public static BigCurrency operator *(BigCurrency a, double k)
    {
        if (a is null) throw new ArgumentNullException();
        if (k <= 0) return new BigCurrency(0, 0);
        return new BigCurrency(a.Value * k, a.Tier);
    }

    public static BigCurrency operator /(BigCurrency a, double k)
    {
        if (a is null) throw new ArgumentNullException();
        if (k <= 0) throw new DivideByZeroException();
        return new BigCurrency(a.Value / k, a.Tier);
    }

    // --------------------
    // 비교/동등성
    // --------------------
    public int CompareTo(BigCurrency other)
    {
        if (other is null) return 1;
        if (Tier != other.Tier) return Tier.CompareTo(other.Tier);
        return Value.CompareTo(other.Value);
    }

    public bool Equals(BigCurrency other)
    {
        if (other is null) return false;
        // 같은 실수 비교: 티어 같고 값이 충분히 가까우면 동등
        return Tier == other.Tier && Math.Abs(Value - other.Value) < 1e-9;
    }

    public override bool Equals(object obj) => Equals(obj as BigCurrency);

    public override int GetHashCode() => HashCode.Combine(Tier, Math.Round(Value, 9));

    public static bool operator >(BigCurrency a, BigCurrency b) => a.CompareTo(b) > 0;
    public static bool operator <(BigCurrency a, BigCurrency b) => a.CompareTo(b) < 0;
    public static bool operator >=(BigCurrency a, BigCurrency b) => a.CompareTo(b) >= 0;
    public static bool operator <=(BigCurrency a, BigCurrency b) => a.CompareTo(b) <= 0;
    public static bool operator ==(BigCurrency a, BigCurrency b)
        => ReferenceEquals(a, b) || (a is not null && b is not null && a.Equals(b));
    public static bool operator !=(BigCurrency a, BigCurrency b) => !(a == b);

    // --------------------
    // 표시/유틸
    // --------------------

    /// <summary>
    /// 0->A, 1->B, 3->C,..., 25->Z, 26->AA, 27->AB ...
    /// </summary>
    private static string GetUnit(int tier)
    {
        if (tier < 0) return "";
        // 스프레드시트 열 인덱싱과 유사 (1-based로 바꿔서 나눗셈)
        tier += 1;
        string s = "";
        while (tier > 0) {
            tier--; // 0~25로 맞춘 뒤 문자화
            int r = tier % 26;
            s = (char)('A' + r) + s;
            tier /= 26;
        }
        return s;
    }

    public override string ToString()
    {
        if (Value == 0) return "0A"; // 혹은 "0"
        return $"{Value:F2}{GetUnit(Tier)}";
    }

    // 문자열 파싱(옵션): "12.3AB" -> Value=12.3, Tier=27
    public static bool TryParse(string s, out BigCurrency result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(s)) return false;

        s = s.Trim();
        // 숫자 부분과 문자 부분 분리
        int i = 0;
        while (i < s.Length && (char.IsDigit(s[i]) || s[i] == '.')) i++;
        if (i == 0) return false;

        if (!double.TryParse(s.Substring(0, i), out double v)) return false;
        string unit = s.Substring(i).Trim();

        int tier = 0;
        if (!string.IsNullOrEmpty(unit)) {
            // 단위 문자열을 tier로 변환 (A->0 ... Z->25, AA->26 ...)
            tier = UnitToTier(unit);
            if (tier < 0) return false;
        }

        result = new BigCurrency(v, tier);
        return true;
    }

    private static int UnitToTier(string unit)
    {
        // 대문자만 허용
        for (int k = 0; k < unit.Length; k++)
            if (unit[k] < 'A' || unit[k] > 'Z') return -1;

        // 스프레드시트 역변환: "A"->1, ... "Z"->26, "AA"->27 ...
        int n = 0;
        for (int k = 0; k < unit.Length; k++) {
            n = n * 26 + (unit[k] - 'A' + 1);
        }
        return n - 1; // 0-based로
    }

    // 팩토리: A단위 원시값(예: 1500000A)에서 생성
    public static BigCurrency FromBaseAmount(double baseAmount) // 예: 1500000A -> 1.5B
    {
        if (baseAmount <= 0) return new BigCurrency(0, 0);
        int tier = 0;
        double value = baseAmount;

        while (value >= BaseUnit) {
            value /= BaseUnit;
            tier++;
        }
        return new BigCurrency(value, tier);
    }
}
