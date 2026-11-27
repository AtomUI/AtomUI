namespace AtomUI.Desktop.Controls;

internal static class DataGridIdentityValidation
{
    public static string Validate(string value, string parameterName)
    {
        ArgumentNullException.ThrowIfNull(value, parameterName);
        if (value.Length == 0)
        {
            throw new ArgumentException("The identity cannot be empty.", parameterName);
        }
        if (char.IsWhiteSpace(value[0]) || char.IsWhiteSpace(value[^1]))
        {
            throw new ArgumentException("The identity cannot have leading or trailing whitespace.", parameterName);
        }
        foreach (var character in value)
        {
            if (char.IsControl(character))
            {
                throw new ArgumentException("The identity cannot contain control characters.", parameterName);
            }
        }
        return value;
    }
}

/// <summary>
/// Identifies one field of a data grid data source by its field name.
/// The <c>default</c> value is not a valid id: <see cref="IsValid" /> is <see langword="false" /> and
/// <see cref="Value" /> throws.
/// </summary>
public readonly struct DataGridFieldId : IEquatable<DataGridFieldId>
{
    private readonly string? _value;

    /// <summary>
    /// Creates an id from the field name and stores it as given.
    /// </summary>
    /// <param name="value">
    /// The name of the field.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="value" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="value" /> is empty, starts or ends with whitespace, or contains a control character.
    /// </exception>
    public DataGridFieldId(string value)
    {
        _value = DataGridIdentityValidation.Validate(value, nameof(value));
    }

    /// <summary>
    /// Gets the field name.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// This value is the <c>default</c> value, so <see cref="IsValid" /> is <see langword="false" />.
    /// </exception>
    public string Value => _value ?? throw new InvalidOperationException("The field identity is invalid.");

    /// <summary>
    /// Gets whether this value names a field. It is <see langword="false" /> only for the <c>default</c> value:
    /// the constructor throws instead of creating such a value.
    /// </summary>
    public bool IsValid => _value is not null;

    /// <summary>
    /// Returns whether this id and <paramref name="other" /> have the same field name. The comparison is
    /// ordinal and case-sensitive, so names that differ only in case name different fields.
    /// </summary>
    public bool Equals(DataGridFieldId other) =>
        string.Equals(_value, other._value, StringComparison.Ordinal);

    /// <summary>
    /// Returns whether <paramref name="obj" /> is a <see cref="DataGridFieldId" /> with the same field name.
    /// </summary>
    public override bool Equals(object? obj) => obj is DataGridFieldId other && Equals(other);

    /// <summary>
    /// Returns the ordinal hash code of the field name, or 0 for the <c>default</c> value.
    /// </summary>
    public override int GetHashCode() =>
        _value is null ? 0 : StringComparer.Ordinal.GetHashCode(_value);

    /// <summary>
    /// Returns the field name, or an empty string for the <c>default</c> value.
    /// </summary>
    public override string ToString() => _value ?? string.Empty;

    /// <summary>
    /// Returns whether <paramref name="left" /> and <paramref name="right" /> have the same field name.
    /// </summary>
    public static bool operator ==(DataGridFieldId left, DataGridFieldId right) => left.Equals(right);

    /// <summary>
    /// Returns whether <paramref name="left" /> and <paramref name="right" /> differ in their field name.
    /// </summary>
    public static bool operator !=(DataGridFieldId left, DataGridFieldId right) => !left.Equals(right);
}

/// <summary>
/// Identifies one filter operator of a data grid by its name.
/// The <c>default</c> value is not a valid id: <see cref="IsValid" /> is <see langword="false" /> and
/// <see cref="Value" /> throws.
/// </summary>
public readonly struct DataGridOperatorId : IEquatable<DataGridOperatorId>
{
    private readonly string? _value;

    /// <summary>
    /// Creates an id from the operator name and stores it as given.
    /// </summary>
    /// <param name="value">
    /// The name of the filter operator.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="value" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="value" /> is empty, starts or ends with whitespace, or contains a control character.
    /// </exception>
    public DataGridOperatorId(string value)
    {
        _value = DataGridIdentityValidation.Validate(value, nameof(value));
    }

    /// <summary>
    /// Gets the operator name.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// This value is the <c>default</c> value, so <see cref="IsValid" /> is <see langword="false" />.
    /// </exception>
    public string Value => _value ?? throw new InvalidOperationException("The operator identity is invalid.");

    /// <summary>
    /// Gets whether this value names an operator. It is <see langword="false" /> only for the <c>default</c>
    /// value: the constructor throws instead of creating such a value.
    /// </summary>
    public bool IsValid => _value is not null;

    /// <summary>
    /// Returns whether this id and <paramref name="other" /> have the same operator name. The comparison is
    /// ordinal and case-sensitive, so names that differ only in case name different operators.
    /// </summary>
    public bool Equals(DataGridOperatorId other) =>
        string.Equals(_value, other._value, StringComparison.Ordinal);

    /// <summary>
    /// Returns whether <paramref name="obj" /> is a <see cref="DataGridOperatorId" /> with the same operator name.
    /// </summary>
    public override bool Equals(object? obj) => obj is DataGridOperatorId other && Equals(other);

    /// <summary>
    /// Returns the ordinal hash code of the operator name, or 0 for the <c>default</c> value.
    /// </summary>
    public override int GetHashCode() =>
        _value is null ? 0 : StringComparer.Ordinal.GetHashCode(_value);

    /// <summary>
    /// Returns the operator name, or an empty string for the <c>default</c> value.
    /// </summary>
    public override string ToString() => _value ?? string.Empty;

    /// <summary>
    /// Returns whether <paramref name="left" /> and <paramref name="right" /> have the same operator name.
    /// </summary>
    public static bool operator ==(DataGridOperatorId left, DataGridOperatorId right) => left.Equals(right);

    /// <summary>
    /// Returns whether <paramref name="left" /> and <paramref name="right" /> differ in their operator name.
    /// </summary>
    public static bool operator !=(DataGridOperatorId left, DataGridOperatorId right) => !left.Equals(right);
}

/// <summary>
/// Names the kind of value that a <see cref="DataGridRowKey" /> holds.
/// The order of the members is the order that
/// <see cref="DataGridRowKey.CompareTo(DataGridRowKey)" /> uses for keys of different kinds.
/// </summary>
public enum DataGridRowKeyKind : byte
{
    /// <summary>
    /// The key holds no value. <c>default(DataGridRowKey)</c> has this kind; no factory method creates it.
    /// </summary>
    Invalid,
    /// <summary>
    /// The key holds a text, created by <see cref="DataGridRowKey.FromString" />.
    /// </summary>
    String,
    /// <summary>
    /// The key holds a signed 64-bit integer, created by <see cref="DataGridRowKey.FromInt64" />.
    /// </summary>
    SignedInteger,
    /// <summary>
    /// The key holds an unsigned 64-bit integer, created by <see cref="DataGridRowKey.FromUInt64" />.
    /// </summary>
    UnsignedInteger,
    /// <summary>
    /// The key holds a GUID, created by <see cref="DataGridRowKey.FromGuid" />.
    /// </summary>
    Guid
}

/// <summary>
/// Identifies one row of a data grid data source.
/// A key is created by <see cref="FromString" />, <see cref="FromInt64" />, <see cref="FromUInt64" /> or
/// <see cref="FromGuid" /> and holds one kind of value.
/// The <c>default</c> value is not a valid key: <see cref="IsValid" /> is <see langword="false" />.
/// </summary>
public readonly struct DataGridRowKey : IEquatable<DataGridRowKey>, IComparable<DataGridRowKey>
{
    private readonly string? _stringValue;
    private readonly long _signedValue;
    private readonly ulong _unsignedValue;
    private readonly Guid _guidValue;

    private DataGridRowKey(
        DataGridRowKeyKind kind,
        string? stringValue = null,
        long signedValue = 0,
        ulong unsignedValue = 0,
        Guid guidValue = default)
    {
        Kind = kind;
        _stringValue = stringValue;
        _signedValue = signedValue;
        _unsignedValue = unsignedValue;
        _guidValue = guidValue;
    }

    /// <summary>
    /// Gets the kind of value this key holds. It is <see cref="DataGridRowKeyKind.Invalid" /> only for the
    /// <c>default</c> value.
    /// </summary>
    public DataGridRowKeyKind Kind { get; }

    /// <summary>
    /// Gets whether this key identifies a row. It is <see langword="false" /> only for the <c>default</c> value:
    /// every factory method creates a valid key.
    /// </summary>
    public bool IsValid => Kind != DataGridRowKeyKind.Invalid;

    /// <summary>
    /// Creates a key that holds a text.
    /// </summary>
    /// <param name="value">
    /// The text that identifies the row.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="value" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="value" /> is empty, starts or ends with whitespace, or contains a control character.
    /// </exception>
    public static DataGridRowKey FromString(string value) =>
        new(DataGridRowKeyKind.String, DataGridIdentityValidation.Validate(value, nameof(value)));

    /// <summary>
    /// Creates a key that holds a signed 64-bit integer.
    /// </summary>
    public static DataGridRowKey FromInt64(long value) =>
        new(DataGridRowKeyKind.SignedInteger, signedValue: value);

    /// <summary>
    /// Creates a key that holds an unsigned 64-bit integer.
    /// </summary>
    public static DataGridRowKey FromUInt64(ulong value) =>
        new(DataGridRowKeyKind.UnsignedInteger, unsignedValue: value);

    /// <summary>
    /// Creates a key that holds a GUID.
    /// </summary>
    public static DataGridRowKey FromGuid(Guid value) =>
        new(DataGridRowKeyKind.Guid, guidValue: value);

    /// <summary>
    /// Returns whether this key and <paramref name="other" /> are of the same kind and hold equal values.
    /// Texts are compared ordinally and case-sensitively.
    /// Keys of different kinds are never equal, so <c>FromInt64(1)</c> and <c>FromUInt64(1)</c>
    /// identify different rows.
    /// </summary>
    public bool Equals(DataGridRowKey other)
    {
        if (Kind != other.Kind)
        {
            return false;
        }
        return Kind switch
        {
            DataGridRowKeyKind.Invalid => true,
            DataGridRowKeyKind.String => string.Equals(
                _stringValue, other._stringValue, StringComparison.Ordinal),
            DataGridRowKeyKind.SignedInteger => _signedValue == other._signedValue,
            DataGridRowKeyKind.UnsignedInteger => _unsignedValue == other._unsignedValue,
            DataGridRowKeyKind.Guid => _guidValue == other._guidValue,
            _ => false
        };
    }

    /// <summary>
    /// Returns whether <paramref name="obj" /> is a <see cref="DataGridRowKey" /> that equals this key.
    /// </summary>
    public override bool Equals(object? obj) => obj is DataGridRowKey other && Equals(other);

    /// <summary>
    /// Returns a hash code that combines <see cref="Kind" /> with the value, so that it agrees with equality.
    /// The <c>default</c> value returns 0.
    /// </summary>
    public override int GetHashCode() => Kind switch
    {
        DataGridRowKeyKind.Invalid => 0,
        DataGridRowKeyKind.String => HashCode.Combine(
            Kind, StringComparer.Ordinal.GetHashCode(_stringValue!)),
        DataGridRowKeyKind.SignedInteger => HashCode.Combine(Kind, _signedValue),
        DataGridRowKeyKind.UnsignedInteger => HashCode.Combine(Kind, _unsignedValue),
        DataGridRowKeyKind.Guid => HashCode.Combine(Kind, _guidValue),
        _ => 0
    };

    /// <summary>
    /// Compares this key with <paramref name="other" /> for order.
    /// Keys of different kinds come in the order of the <see cref="DataGridRowKeyKind" /> members: no value,
    /// text, signed integer, unsigned integer, GUID. Keys of one kind are ordered by value, texts ordinally.
    /// </summary>
    /// <returns>
    /// A negative value, zero or a positive value, as this key precedes, equals or follows
    /// <paramref name="other" />.
    /// </returns>
    public int CompareTo(DataGridRowKey other)
    {
        var kind = Kind.CompareTo(other.Kind);
        if (kind != 0)
        {
            return kind;
        }
        return Kind switch
        {
            DataGridRowKeyKind.Invalid => 0,
            DataGridRowKeyKind.String => string.Compare(
                _stringValue, other._stringValue, StringComparison.Ordinal),
            DataGridRowKeyKind.SignedInteger => _signedValue.CompareTo(other._signedValue),
            DataGridRowKeyKind.UnsignedInteger => _unsignedValue.CompareTo(other._unsignedValue),
            DataGridRowKeyKind.Guid => _guidValue.CompareTo(other._guidValue),
            _ => throw new InvalidOperationException($"Unknown row key kind '{Kind}'.")
        };
    }

    /// <summary>
    /// Returns the value as text: the text itself, an integer in the invariant culture, or a GUID in the
    /// hyphenated <c>D</c> format. The <c>default</c> value returns an empty string.
    /// </summary>
    public override string ToString() => Kind switch
    {
        DataGridRowKeyKind.String => _stringValue!,
        DataGridRowKeyKind.SignedInteger => _signedValue.ToString(System.Globalization.CultureInfo.InvariantCulture),
        DataGridRowKeyKind.UnsignedInteger => _unsignedValue.ToString(System.Globalization.CultureInfo.InvariantCulture),
        DataGridRowKeyKind.Guid => _guidValue.ToString("D"),
        _ => string.Empty
    };

    /// <summary>
    /// Returns whether <paramref name="left" /> and <paramref name="right" /> are equal keys.
    /// </summary>
    public static bool operator ==(DataGridRowKey left, DataGridRowKey right) => left.Equals(right);

    /// <summary>
    /// Returns whether <paramref name="left" /> and <paramref name="right" /> are different keys.
    /// </summary>
    public static bool operator !=(DataGridRowKey left, DataGridRowKey right) => !left.Equals(right);
}

/// <summary>
/// Identifies one row group of a data grid by its group path text.
/// The <c>default</c> value is not a valid key: <see cref="IsValid" /> is <see langword="false" /> and
/// <see cref="Value" /> throws.
/// </summary>
public readonly struct DataGridGroupKey : IEquatable<DataGridGroupKey>
{
    private readonly string? _value;

    /// <summary>
    /// Creates a key from the group path text and stores it as given.
    /// </summary>
    /// <param name="value">
    /// The text that identifies the group.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="value" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="value" /> is empty, starts or ends with whitespace, or contains a control character.
    /// </exception>
    public DataGridGroupKey(string value)
    {
        _value = DataGridIdentityValidation.Validate(value, nameof(value));
    }

    /// <summary>
    /// Gets the group path text.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// This value is the <c>default</c> value, so <see cref="IsValid" /> is <see langword="false" />.
    /// </exception>
    public string Value => _value ?? throw new InvalidOperationException("The group identity is invalid.");

    /// <summary>
    /// Gets whether this value carries a group path text. It is <see langword="false" /> only for the
    /// <c>default</c> value: the constructor throws instead of creating such a value.
    /// </summary>
    public bool IsValid => _value is not null;

    /// <summary>
    /// Returns whether this key and <paramref name="other" /> have the same text. The comparison is ordinal and
    /// case-sensitive, so texts that differ only in case name different groups.
    /// </summary>
    public bool Equals(DataGridGroupKey other) =>
        string.Equals(_value, other._value, StringComparison.Ordinal);

    /// <summary>
    /// Returns whether <paramref name="obj" /> is a <see cref="DataGridGroupKey" /> with the same text.
    /// </summary>
    public override bool Equals(object? obj) => obj is DataGridGroupKey other && Equals(other);

    /// <summary>
    /// Returns the ordinal hash code of the text, or 0 for the <c>default</c> value.
    /// </summary>
    public override int GetHashCode() =>
        _value is null ? 0 : StringComparer.Ordinal.GetHashCode(_value);

    /// <summary>
    /// Returns the group path text, or an empty string for the <c>default</c> value.
    /// </summary>
    public override string ToString() => _value ?? string.Empty;

    /// <summary>
    /// Returns whether <paramref name="left" /> and <paramref name="right" /> have the same group path text.
    /// </summary>
    public static bool operator ==(DataGridGroupKey left, DataGridGroupKey right) => left.Equals(right);

    /// <summary>
    /// Returns whether <paramref name="left" /> and <paramref name="right" /> differ in their group path text.
    /// </summary>
    public static bool operator !=(DataGridGroupKey left, DataGridGroupKey right) => !left.Equals(right);
}

/// <summary>
/// Identifies one snapshot of a data grid data source, that is one version of its content.
/// The <c>default</c> value is not a valid id: <see cref="IsValid" /> is <see langword="false" /> and
/// <see cref="Value" /> throws.
/// </summary>
public readonly struct DataGridSnapshotId : IEquatable<DataGridSnapshotId>
{
    private readonly string? _value;

    /// <summary>
    /// Creates an id from the snapshot text and stores it as given.
    /// </summary>
    /// <param name="value">
    /// The text that identifies the snapshot version.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="value" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="value" /> is empty, starts or ends with whitespace, or contains a control character.
    /// </exception>
    public DataGridSnapshotId(string value)
    {
        _value = DataGridIdentityValidation.Validate(value, nameof(value));
    }

    /// <summary>
    /// Gets the snapshot text.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// This value is the <c>default</c> value, so <see cref="IsValid" /> is <see langword="false" />.
    /// </exception>
    public string Value => _value ?? throw new InvalidOperationException("The snapshot identity is invalid.");

    /// <summary>
    /// Gets whether this value carries a snapshot text. It is <see langword="false" /> only for the
    /// <c>default</c> value: the constructor throws instead of creating such a value.
    /// </summary>
    public bool IsValid => _value is not null;

    /// <summary>
    /// Returns whether this id and <paramref name="other" /> have the same text. The comparison is ordinal and
    /// case-sensitive, so texts that differ only in case name different snapshots.
    /// </summary>
    public bool Equals(DataGridSnapshotId other) =>
        string.Equals(_value, other._value, StringComparison.Ordinal);

    /// <summary>
    /// Returns whether <paramref name="obj" /> is a <see cref="DataGridSnapshotId" /> with the same text.
    /// </summary>
    public override bool Equals(object? obj) => obj is DataGridSnapshotId other && Equals(other);

    /// <summary>
    /// Returns the ordinal hash code of the text, or 0 for the <c>default</c> value.
    /// </summary>
    public override int GetHashCode() =>
        _value is null ? 0 : StringComparer.Ordinal.GetHashCode(_value);

    /// <summary>
    /// Returns the snapshot text, or an empty string for the <c>default</c> value.
    /// </summary>
    public override string ToString() => _value ?? string.Empty;

    /// <summary>
    /// Returns whether <paramref name="left" /> and <paramref name="right" /> have the same snapshot text.
    /// </summary>
    public static bool operator ==(DataGridSnapshotId left, DataGridSnapshotId right) => left.Equals(right);

    /// <summary>
    /// Returns whether <paramref name="left" /> and <paramref name="right" /> differ in their snapshot text.
    /// </summary>
    public static bool operator !=(DataGridSnapshotId left, DataGridSnapshotId right) => !left.Equals(right);
}
