namespace AtomUI.Desktop.Controls;

/// <summary>
/// The family of CLR types that a <see cref="DataGridScalar" /> carries. It decides which payload holds the value
/// and how two scalars of that kind are compared. Members are declared in the order that
/// <see cref="DataGridScalar.CompareTo(DataGridScalar)" /> uses to sort scalars of different kinds.
/// </summary>
public enum DataGridScalarKind : byte
{
    /// <summary>
    /// No value. Every scalar of this kind is <see cref="DataGridScalar.Null" />.
    /// </summary>
    Null,
    /// <summary>
    /// A <see cref="bool" />, stored as 0 or 1.
    /// </summary>
    Boolean,
    /// <summary>
    /// A signed integer of any width, widened to 64 bits.
    /// </summary>
    SignedInteger,
    /// <summary>
    /// An unsigned integer of any width, widened to 64 bits.
    /// </summary>
    UnsignedInteger,
    /// <summary>
    /// A finite <see cref="float" /> or <see cref="double" />, stored as <see cref="double" />.
    /// </summary>
    Double,
    /// <summary>
    /// A <see cref="decimal" />.
    /// </summary>
    Decimal,
    /// <summary>
    /// A <see cref="System.String" />, compared ordinally.
    /// </summary>
    String,
    /// <summary>
    /// A <see cref="System.Guid" />.
    /// </summary>
    Guid,
    /// <summary>
    /// A <see cref="System.DateOnly" />, stored as its day number.
    /// </summary>
    DateOnly,
    /// <summary>
    /// A <see cref="System.TimeOnly" />, stored as its ticks.
    /// </summary>
    TimeOnly,
    /// <summary>
    /// A <see cref="System.DateTimeOffset" />, stored as its UTC ticks. The offset is not kept, so two scalars
    /// of this kind are equal when they are the same instant.
    /// </summary>
    DateTimeOffset
}

/// <summary>
/// An immutable field value of one of the kinds in <see cref="DataGridScalarKind" />: the form in which a data
/// source reports a value to the DataGrid and in which a filter or an edit passes values back to it.
/// </summary>
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

    /// <summary>
    /// The kind of CLR value this scalar carries, or <see cref="DataGridScalarKind.Null" /> when there is none.
    /// </summary>
    public DataGridScalarKind Kind { get; }

    /// <summary>
    /// The scalar that carries no value, which is also what <c>default(DataGridScalar)</c> yields. Every scalar
    /// of kind <see cref="DataGridScalarKind.Null" /> is equal to it.
    /// </summary>
    public static DataGridScalar Null => default;

    /// <summary>
    /// Creates a scalar of kind <see cref="DataGridScalarKind.Boolean" />.
    /// </summary>
    public static DataGridScalar FromBoolean(bool value) =>
        new(DataGridScalarKind.Boolean, signedValue: value ? 1 : 0);

    /// <summary>
    /// Creates a scalar of kind <see cref="DataGridScalarKind.SignedInteger" />. Values of the narrower signed
    /// integer types share this kind after widening.
    /// </summary>
    public static DataGridScalar FromInt64(long value) =>
        new(DataGridScalarKind.SignedInteger, signedValue: value);

    /// <summary>
    /// Creates a scalar of kind <see cref="DataGridScalarKind.UnsignedInteger" />. Values of the narrower unsigned
    /// integer types share this kind after widening.
    /// </summary>
    public static DataGridScalar FromUInt64(ulong value) =>
        new(DataGridScalarKind.UnsignedInteger, unsignedValue: value);

    /// <summary>
    /// Creates a scalar of kind <see cref="DataGridScalarKind.Double" />, which is also the kind of
    /// <see cref="float" /> values. Negative zero is stored as positive zero.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="value" /> is NaN or infinite.
    /// </exception>
    public static DataGridScalar FromDouble(double value)
    {
        if (!double.IsFinite(value))
        {
            throw new ArgumentOutOfRangeException(nameof(value), value, "The value must be finite.");
        }
        return new DataGridScalar(DataGridScalarKind.Double, doubleValue: value == 0d ? 0d : value);
    }

    /// <summary>
    /// Creates a scalar of kind <see cref="DataGridScalarKind.Decimal" />.
    /// </summary>
    public static DataGridScalar FromDecimal(decimal value) =>
        new(DataGridScalarKind.Decimal, decimalValue: value);

    /// <summary>
    /// Creates a scalar of kind <see cref="DataGridScalarKind.String" /> that carries the given string unchanged.
    /// </summary>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="value" /> is <see langword="null" />.
    /// </exception>
    public static DataGridScalar FromString(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return new DataGridScalar(DataGridScalarKind.String, stringValue: value);
    }

    /// <summary>
    /// Creates a scalar of kind <see cref="DataGridScalarKind.Guid" />.
    /// </summary>
    public static DataGridScalar FromGuid(Guid value) =>
        new(DataGridScalarKind.Guid, guidValue: value);

    /// <summary>
    /// Creates a scalar of kind <see cref="DataGridScalarKind.DateOnly" />.
    /// </summary>
    public static DataGridScalar FromDateOnly(DateOnly value) =>
        new(DataGridScalarKind.DateOnly, signedValue: value.DayNumber);

    /// <summary>
    /// Creates a scalar of kind <see cref="DataGridScalarKind.TimeOnly" />.
    /// </summary>
    public static DataGridScalar FromTimeOnly(TimeOnly value) =>
        new(DataGridScalarKind.TimeOnly, signedValue: value.Ticks);

    /// <summary>
    /// Creates a scalar of kind <see cref="DataGridScalarKind.DateTimeOffset" />.
    /// </summary>
    public static DataGridScalar FromDateTimeOffset(DateTimeOffset value) =>
        new(DataGridScalarKind.DateTimeOffset, signedValue: value.UtcTicks);

    /// <summary>
    /// Returns the string carried by a scalar of kind <see cref="DataGridScalarKind.String" />.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// <see cref="Kind" /> is not <see cref="DataGridScalarKind.String" />.
    /// </exception>
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
    /// Returns the boxed CLR value carried by the scalar, using UTC for <see cref="DateTimeOffset" />, or
    /// <see langword="null" /> when the kind is <see cref="DataGridScalarKind.Null" />.
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

    /// <summary>
    /// Converts a boxed CLR value to a scalar when a kind supports its type.
    /// </summary>
    /// <param name="value">
    /// The value to convert. <see langword="null" /> yields <see cref="Null" />. Narrower integers and
    /// <see cref="float" /> are widened as the <c>From</c> methods of this struct widen them.
    /// </param>
    /// <param name="scalar">
    /// The converted scalar, or <see cref="Null" /> when the conversion fails.
    /// </param>
    /// <returns>
    /// <c>true</c> when the value was converted. <c>false</c> when no kind supports the type of the value, or
    /// when it is a <see cref="float" /> or <see cref="double" /> that is NaN or infinite.
    /// </returns>
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

    /// <summary>
    /// Compares this scalar with <paramref name="other" /> for order. Scalars of different kinds are ordered by
    /// <see cref="Kind" />, which puts <see cref="DataGridScalarKind.Null" /> first. Scalars of the same kind are
    /// ordered by the value they carry, ordinally for strings.
    /// </summary>
    /// <returns>
    /// A negative value when this scalar precedes <paramref name="other" />, zero when the two are equal, and a
    /// positive value otherwise.
    /// </returns>
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

    /// <summary>
    /// Returns whether <paramref name="other" /> carries the same kind and the same value. Scalars of different
    /// kinds are never equal, so the same number given to <see cref="FromInt64(long)" /> and to
    /// <see cref="FromDouble(double)" /> does not compare equal.
    /// </summary>
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

    /// <summary>
    /// Returns whether <paramref name="obj" /> is a <see cref="DataGridScalar" /> that equals this one.
    /// </summary>
    public override bool Equals(object? obj) => obj is DataGridScalar other && Equals(other);

    /// <summary>
    /// Returns a hash code that is the same for any two scalars that
    /// <see cref="Equals(DataGridScalar)" /> reports as equal.
    /// </summary>
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

    /// <summary>
    /// Returns whether <paramref name="left" /> and <paramref name="right" /> carry the same kind and value.
    /// </summary>
    public static bool operator ==(DataGridScalar left, DataGridScalar right) => left.Equals(right);

    /// <summary>
    /// Returns whether <paramref name="left" /> and <paramref name="right" /> differ in kind or in value.
    /// </summary>
    public static bool operator !=(DataGridScalar left, DataGridScalar right) => !left.Equals(right);

    /// <summary>
    /// Returns whether <paramref name="left" /> precedes <paramref name="right" /> in the order of
    /// <see cref="CompareTo(DataGridScalar)" />.
    /// </summary>
    public static bool operator <(DataGridScalar left, DataGridScalar right) => left.CompareTo(right) < 0;

    /// <summary>
    /// Returns whether <paramref name="left" /> precedes or equals <paramref name="right" /> in the order of
    /// <see cref="CompareTo(DataGridScalar)" />.
    /// </summary>
    public static bool operator <=(DataGridScalar left, DataGridScalar right) => left.CompareTo(right) <= 0;

    /// <summary>
    /// Returns whether <paramref name="left" /> follows <paramref name="right" /> in the order of
    /// <see cref="CompareTo(DataGridScalar)" />.
    /// </summary>
    public static bool operator >(DataGridScalar left, DataGridScalar right) => left.CompareTo(right) > 0;

    /// <summary>
    /// Returns whether <paramref name="left" /> follows or equals <paramref name="right" /> in the order of
    /// <see cref="CompareTo(DataGridScalar)" />.
    /// </summary>
    public static bool operator >=(DataGridScalar left, DataGridScalar right) => left.CompareTo(right) >= 0;

    /// <summary>
    /// Formats the carried value as a string that does not depend on the current culture: the stored number for
    /// the numeric and date and time kinds, "0" or "1" for <see cref="DataGridScalarKind.Boolean" />, the "D" form
    /// for <see cref="DataGridScalarKind.Guid" />, the string itself for <see cref="DataGridScalarKind.String" />,
    /// and an empty string for <see cref="DataGridScalarKind.Null" />.
    /// </summary>
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
