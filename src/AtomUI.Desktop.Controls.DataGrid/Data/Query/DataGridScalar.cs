namespace AtomUI.Desktop.Controls;

public enum DataGridScalarKind : byte
{
    Null,
    Boolean,
    SignedInteger,
    UnsignedInteger,
    Double,
    Decimal,
    String,
    Guid,
    DateOnly,
    TimeOnly,
    DateTimeOffset
}

public readonly struct DataGridScalar : IEquatable<DataGridScalar>, IComparable<DataGridScalar>
{
    private readonly long _signedValue;
    private readonly ulong _unsignedValue;
    private readonly double _doubleValue;
    private readonly decimal _decimalValue;
    private readonly string? _stringValue;
    private readonly Guid _guidValue;

    private DataGridScalar(
        DataGridScalarKind kind,
        long signedValue = 0,
        ulong unsignedValue = 0,
        double doubleValue = 0,
        decimal decimalValue = 0,
        string? stringValue = null,
        Guid guidValue = default)
    {
        Kind = kind;
        _signedValue = signedValue;
        _unsignedValue = unsignedValue;
        _doubleValue = doubleValue;
        _decimalValue = decimalValue;
        _stringValue = stringValue;
        _guidValue = guidValue;
    }

    public DataGridScalarKind Kind { get; }

    public static DataGridScalar Null => default;

    public static DataGridScalar FromBoolean(bool value) =>
        new(DataGridScalarKind.Boolean, signedValue: value ? 1 : 0);

    public static DataGridScalar FromInt64(long value) =>
        new(DataGridScalarKind.SignedInteger, signedValue: value);

    public static DataGridScalar FromUInt64(ulong value) =>
        new(DataGridScalarKind.UnsignedInteger, unsignedValue: value);

    public static DataGridScalar FromDouble(double value)
    {
        if (!double.IsFinite(value))
        {
            throw new ArgumentOutOfRangeException(nameof(value), value, "The value must be finite.");
        }
        return new DataGridScalar(DataGridScalarKind.Double, doubleValue: value == 0d ? 0d : value);
    }

    public static DataGridScalar FromDecimal(decimal value) =>
        new(DataGridScalarKind.Decimal, decimalValue: value);

    public static DataGridScalar FromString(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return new DataGridScalar(DataGridScalarKind.String, stringValue: value);
    }

    public static DataGridScalar FromGuid(Guid value) =>
        new(DataGridScalarKind.Guid, guidValue: value);

    public static DataGridScalar FromDateOnly(DateOnly value) =>
        new(DataGridScalarKind.DateOnly, signedValue: value.DayNumber);

    public static DataGridScalar FromTimeOnly(TimeOnly value) =>
        new(DataGridScalarKind.TimeOnly, signedValue: value.Ticks);

    public static DataGridScalar FromDateTimeOffset(DateTimeOffset value) =>
        new(DataGridScalarKind.DateTimeOffset, signedValue: value.UtcTicks);

    public string GetString()
    {
        if (Kind != DataGridScalarKind.String)
        {
            throw new InvalidOperationException(
                $"A scalar of kind '{Kind}' cannot be read as a string.");
        }
        return _stringValue!;
    }

    /// <summary>
    /// Returns the boxed CLR value carried by the scalar, using UTC for <see cref="DateTimeOffset"/>.
    /// </summary>
    internal object? ToValue() => Kind switch
    {
        DataGridScalarKind.Null => null,
        DataGridScalarKind.Boolean => _signedValue != 0,
        DataGridScalarKind.SignedInteger => _signedValue,
        DataGridScalarKind.UnsignedInteger => _unsignedValue,
        DataGridScalarKind.Double => _doubleValue,
        DataGridScalarKind.Decimal => _decimalValue,
        DataGridScalarKind.String => _stringValue,
        DataGridScalarKind.Guid => _guidValue,
        DataGridScalarKind.DateOnly => DateOnly.FromDayNumber((int)_signedValue),
        DataGridScalarKind.TimeOnly => new TimeOnly(_signedValue),
        DataGridScalarKind.DateTimeOffset => new DateTimeOffset(_signedValue, TimeSpan.Zero),
        _ => throw new InvalidOperationException($"Unknown scalar kind '{Kind}'.")
    };

    internal static bool TryFromValue(object? value, out DataGridScalar scalar)
    {
        switch (value)
        {
            case null:
                scalar = Null;
                return true;
            case bool item:
                scalar = FromBoolean(item);
                return true;
            case sbyte item:
                scalar = FromInt64(item);
                return true;
            case short item:
                scalar = FromInt64(item);
                return true;
            case int item:
                scalar = FromInt64(item);
                return true;
            case long item:
                scalar = FromInt64(item);
                return true;
            case byte item:
                scalar = FromUInt64(item);
                return true;
            case ushort item:
                scalar = FromUInt64(item);
                return true;
            case uint item:
                scalar = FromUInt64(item);
                return true;
            case ulong item:
                scalar = FromUInt64(item);
                return true;
            case float item when float.IsFinite(item):
                scalar = FromDouble(item);
                return true;
            case double item when double.IsFinite(item):
                scalar = FromDouble(item);
                return true;
            case decimal item:
                scalar = FromDecimal(item);
                return true;
            case string item:
                scalar = FromString(item);
                return true;
            case Guid item:
                scalar = FromGuid(item);
                return true;
            case DateOnly item:
                scalar = FromDateOnly(item);
                return true;
            case TimeOnly item:
                scalar = FromTimeOnly(item);
                return true;
            case DateTimeOffset item:
                scalar = FromDateTimeOffset(item);
                return true;
            default:
                scalar = default;
                return false;
        }
    }

    public int CompareTo(DataGridScalar other)
    {
        var kindComparison = Kind.CompareTo(other.Kind);
        if (kindComparison != 0)
        {
            return kindComparison;
        }
        return Kind switch
        {
            DataGridScalarKind.Null => 0,
            DataGridScalarKind.Boolean or
            DataGridScalarKind.SignedInteger or
            DataGridScalarKind.DateOnly or
            DataGridScalarKind.TimeOnly or
            DataGridScalarKind.DateTimeOffset => _signedValue.CompareTo(other._signedValue),
            DataGridScalarKind.UnsignedInteger => _unsignedValue.CompareTo(other._unsignedValue),
            DataGridScalarKind.Double => _doubleValue.CompareTo(other._doubleValue),
            DataGridScalarKind.Decimal => _decimalValue.CompareTo(other._decimalValue),
            DataGridScalarKind.String => string.Compare(
                _stringValue, other._stringValue, StringComparison.Ordinal),
            DataGridScalarKind.Guid => _guidValue.CompareTo(other._guidValue),
            _ => throw new InvalidOperationException($"Unknown scalar kind '{Kind}'.")
        };
    }

    public bool Equals(DataGridScalar other)
    {
        if (Kind != other.Kind)
        {
            return false;
        }
        return Kind switch
        {
            DataGridScalarKind.Null => true,
            DataGridScalarKind.Boolean or
            DataGridScalarKind.SignedInteger or
            DataGridScalarKind.DateOnly or
            DataGridScalarKind.TimeOnly or
            DataGridScalarKind.DateTimeOffset => _signedValue == other._signedValue,
            DataGridScalarKind.UnsignedInteger => _unsignedValue == other._unsignedValue,
            DataGridScalarKind.Double => _doubleValue.Equals(other._doubleValue),
            DataGridScalarKind.Decimal => _decimalValue == other._decimalValue,
            DataGridScalarKind.String => string.Equals(
                _stringValue, other._stringValue, StringComparison.Ordinal),
            DataGridScalarKind.Guid => _guidValue == other._guidValue,
            _ => false
        };
    }

    public override bool Equals(object? obj) => obj is DataGridScalar other && Equals(other);

    public override int GetHashCode() => Kind switch
    {
        DataGridScalarKind.Null => 0,
        DataGridScalarKind.Boolean or
        DataGridScalarKind.SignedInteger or
        DataGridScalarKind.DateOnly or
        DataGridScalarKind.TimeOnly or
        DataGridScalarKind.DateTimeOffset => HashCode.Combine(Kind, _signedValue),
        DataGridScalarKind.UnsignedInteger => HashCode.Combine(Kind, _unsignedValue),
        DataGridScalarKind.Double => HashCode.Combine(Kind, _doubleValue),
        DataGridScalarKind.Decimal => HashCode.Combine(Kind, _decimalValue),
        DataGridScalarKind.String => HashCode.Combine(
            Kind, StringComparer.Ordinal.GetHashCode(_stringValue!)),
        DataGridScalarKind.Guid => HashCode.Combine(Kind, _guidValue),
        _ => 0
    };

    public static bool operator ==(DataGridScalar left, DataGridScalar right) => left.Equals(right);

    public static bool operator !=(DataGridScalar left, DataGridScalar right) => !left.Equals(right);

    public static bool operator <(DataGridScalar left, DataGridScalar right) => left.CompareTo(right) < 0;

    public static bool operator <=(DataGridScalar left, DataGridScalar right) => left.CompareTo(right) <= 0;

    public static bool operator >(DataGridScalar left, DataGridScalar right) => left.CompareTo(right) > 0;

    public static bool operator >=(DataGridScalar left, DataGridScalar right) => left.CompareTo(right) >= 0;

    internal string ToCanonicalString() => Kind switch
    {
        DataGridScalarKind.Null => string.Empty,
        DataGridScalarKind.Boolean => _signedValue == 0 ? "0" : "1",
        DataGridScalarKind.SignedInteger or
        DataGridScalarKind.DateOnly or
        DataGridScalarKind.TimeOnly or
        DataGridScalarKind.DateTimeOffset =>
            _signedValue.ToString(System.Globalization.CultureInfo.InvariantCulture),
        DataGridScalarKind.UnsignedInteger =>
            _unsignedValue.ToString(System.Globalization.CultureInfo.InvariantCulture),
        DataGridScalarKind.Double =>
            _doubleValue.ToString("R", System.Globalization.CultureInfo.InvariantCulture),
        DataGridScalarKind.Decimal =>
            _decimalValue.ToString(System.Globalization.CultureInfo.InvariantCulture),
        DataGridScalarKind.String => _stringValue!,
        DataGridScalarKind.Guid => _guidValue.ToString("D"),
        _ => throw new InvalidOperationException($"Unknown scalar kind '{Kind}'.")
    };
}
