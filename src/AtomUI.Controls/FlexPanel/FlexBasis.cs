using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace AtomUI.Controls;

/// <summary>
/// Initial main-axis size of a child in a <see cref="FlexPanel" />, before free space is distributed by the
/// grow and shrink factors: the child's own size, a fixed length or a fraction of the panel's main-axis size.
/// </summary>
public readonly struct FlexBasis : IEquatable<FlexBasis>
{
    /// <summary>
    /// The length in device-independent pixels, or the fraction of the panel's main-axis size when
    /// <see cref="Kind" /> is <see cref="FlexBasisKind.Relative" />. Not used when <see cref="Kind" /> is
    /// <see cref="FlexBasisKind.Auto" />.
    /// </summary>
    public double Value { get; }

    /// <summary>
    /// How <see cref="Value" /> is interpreted.
    /// </summary>
    public FlexBasisKind Kind { get; }

    /// <summary>
    /// Creates a basis of the given kind.
    /// </summary>
    /// <param name="value">
    /// The main-axis length in device-independent pixels, or the fraction of the panel's main-axis size when
    /// <paramref name="kind" /> is <see cref="FlexBasisKind.Relative" />.
    /// </param>
    /// <param name="kind">
    /// How <paramref name="value" /> is interpreted.
    /// </param>
    /// <exception cref="ArgumentException">
    /// <paramref name="value" /> is negative, NaN or infinite, or <paramref name="kind" /> is not a defined
    /// <see cref="FlexBasisKind" /> value.
    /// </exception>
    public FlexBasis(double value, FlexBasisKind kind)
    {
        if (value < 0 || double.IsNaN(value) || double.IsInfinity(value))
        {
            throw new ArgumentException($"Invalid basis value: {value}", nameof(value));
        }
        if (kind < FlexBasisKind.Auto || kind > FlexBasisKind.Relative)
        {
            throw new ArgumentException($"Invalid basis kind: {kind}", nameof(kind));
        }
        Value = value;
        Kind = kind;
    }

    /// <summary>
    /// Creates an <see cref="FlexBasisKind.Absolute" /> basis.
    /// </summary>
    /// <param name="value">
    /// The main-axis length in device-independent pixels.
    /// </param>
    /// <exception cref="ArgumentException">
    /// <paramref name="value" /> is negative, NaN or infinite.
    /// </exception>
    public FlexBasis(double value) : this(value, FlexBasisKind.Absolute) { }

    /// <summary>
    /// The basis that uses the child's own main-axis size.
    /// </summary>
    public static FlexBasis Auto => new(0.0, FlexBasisKind.Auto);

    /// <summary>
    /// <see langword="true" /> when <see cref="Kind" /> is <see cref="FlexBasisKind.Auto" />.
    /// </summary>
    public bool IsAuto => Kind == FlexBasisKind.Auto;

    /// <summary>
    /// <see langword="true" /> when <see cref="Kind" /> is <see cref="FlexBasisKind.Absolute" />.
    /// </summary>
    public bool IsAbsolute => Kind == FlexBasisKind.Absolute;

    /// <summary>
    /// <see langword="true" /> when <see cref="Kind" /> is <see cref="FlexBasisKind.Relative" />.
    /// </summary>
    public bool IsRelative => Kind == FlexBasisKind.Relative;

    /// <summary>
    /// Whether both bases are <see cref="Auto" />, or have the same <see cref="Kind" /> and <see cref="Value" />.
    /// </summary>
    [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
    public bool Equals(FlexBasis other) =>
        (IsAuto && other.IsAuto) || (Value == other.Value && Kind == other.Kind);

    /// <inheritdoc />
    public override bool Equals(object? obj) =>
        obj is FlexBasis other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() =>
        (Value, Kind).GetHashCode();

    /// <summary>
    /// Whether two bases are equal.
    /// </summary>
    public static bool operator ==(FlexBasis left, FlexBasis right) =>
        left.Equals(right);

    /// <summary>
    /// Whether two bases differ.
    /// </summary>
    public static bool operator !=(FlexBasis left, FlexBasis right) =>
        !left.Equals(right);

    /// <summary>
    /// Formats the basis as <c>Auto</c>, as the invariant number of an absolute basis, or as the invariant
    /// percentage of a relative basis.
    /// </summary>
    public override string ToString()
    {
        return Kind switch
        {
            FlexBasisKind.Auto => "Auto",
            FlexBasisKind.Absolute => FormattableString.Invariant($"{Value:G17}"),
            FlexBasisKind.Relative => FormattableString.Invariant($"{Value * 100:G17}%"),
            _ => throw new InvalidOperationException(),
        };
    }

    /// <summary>
    /// Parses <c>Auto</c> (ignoring case), a number followed by <c>%</c> as a relative basis, or a number as an
    /// absolute basis, using the invariant culture.
    /// </summary>
    /// <param name="str">
    /// The text to parse.
    /// </param>
    /// <exception cref="FormatException">
    /// <paramref name="str" /> is not <c>Auto</c> and its number is not valid.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// The parsed number is negative, NaN or infinite.
    /// </exception>
    public static FlexBasis Parse(string str)
    {
        if (str.Equals("AUTO", StringComparison.OrdinalIgnoreCase))
        {
            return Auto;
        }
        if (str.EndsWith("%", StringComparison.Ordinal))
        {
            return new FlexBasis(ParseDouble(str.AsSpan(0, str.Length - 1).TrimEnd()) / 100,
                FlexBasisKind.Relative);
        }
        return new FlexBasis(ParseDouble(str.AsSpan()), FlexBasisKind.Absolute);

        double ParseDouble(ReadOnlySpan<char> s) => double.Parse(s, CultureInfo.InvariantCulture);
    }
}
