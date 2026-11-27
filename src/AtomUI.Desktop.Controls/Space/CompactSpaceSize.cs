using System.Globalization;
using AtomUI.Utils;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// How a <see cref="CompactSpaceSize" /> decides the width or height of an item in a
/// <see cref="CompactSpace" />.
/// </summary>
public enum CompactSpaceUnitType
{
    /// <summary>
    /// The row or column is auto-sized to fit its content.
    /// </summary>
    Auto = 0,

    /// <summary>
    /// The row or column is sized in device independent pixels.
    /// </summary>
    Pixel = 1,

    /// <summary>
    /// The row or column is sized as a weighted proportion of available space.
    /// </summary>
    Star = 2,
}

/// <summary>
/// The width or height of an item in a <see cref="CompactSpace" />: a fixed number of device independent
/// pixels, a weighted share of the remaining length, or the size of the item's content. Set it on an item
/// through the <c>CompactSpace.ItemSize</c> attached property.
/// </summary>
public struct CompactSpaceSize : IEquatable<CompactSpaceSize>
{
    private readonly CompactSpaceUnitType _type;
    private readonly double _value;

    /// <summary>
    /// Initializes a size of a fixed number of device independent pixels.
    /// </summary>
    /// <param name="value">
    /// The number of pixels. It must be 0 or a positive finite number.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="value" /> is negative, <see cref="double.NaN" /> or infinite.
    /// </exception>
    public CompactSpaceSize(double value)
        : this(value, CompactSpaceUnitType.Pixel)
    {
    }

    /// <summary>
    /// Initializes a size from a number and its unit.
    /// </summary>
    /// <param name="value">
    /// The stored number: a length in device independent pixels for <c>Pixel</c>, or a weight for <c>Star</c>. It
    /// must be 0 or a positive finite number.
    /// </param>
    /// <param name="type">
    /// The unit of the size: <c>Auto</c>, <c>Pixel</c> or <c>Star</c>.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="value" /> is negative, <see cref="double.NaN" /> or infinite, or when
    /// <paramref name="type" /> is not one of the three units.
    /// </exception>
    public CompactSpaceSize(double value, CompactSpaceUnitType type)
    {
        if (value < 0 || double.IsNaN(value) || double.IsInfinity(value))
        {
            throw new ArgumentException("Invalid value", nameof(value));
        }

        if (type < CompactSpaceUnitType.Auto || type > CompactSpaceUnitType.Star)
        {
            throw new ArgumentException("Invalid value", nameof(type));
        }

        _type  = type;
        _value = value;
    }

    /// <summary>
    /// Gets an instance of <see cref="CompactSpaceSize" /> that indicates that a row or column should
    /// auto-size to fit its content.
    /// </summary>
    public static CompactSpaceSize Auto => new CompactSpaceSize(0, CompactSpaceUnitType.Auto);

    /// <summary>
    /// Gets a size that gives the item one share of the remaining length, the same as a weight of <c>1*</c>.
    /// </summary>
    public static CompactSpaceSize Star => new CompactSpaceSize(1, CompactSpaceUnitType.Star);

    /// <summary>
    /// Gets the unit of the <see cref="CompactSpaceSize" />.
    /// </summary>
    public CompactSpaceUnitType CompactSpaceUnitType => _type;

    /// <summary>
    /// Gets a value that indicates whether the <see cref="CompactSpaceSize" /> has a <see cref="CompactSpaceUnitType"
    /// /> of Pixel.
    /// </summary>
    public bool IsAbsolute => _type == CompactSpaceUnitType.Pixel;

    /// <summary>
    /// Gets a value that indicates whether the <see cref="CompactSpaceSize" /> has a <see cref="CompactSpaceUnitType"
    /// /> of Auto.
    /// </summary>
    public bool IsAuto => _type == CompactSpaceUnitType.Auto;

    /// <summary>
    /// Gets a value that indicates whether the <see cref="CompactSpaceSize" /> has a <see cref="CompactSpaceUnitType"
    /// /> of Star.
    /// </summary>
    public bool IsStar => _type == CompactSpaceUnitType.Star;

    /// <summary>
    /// The stored number: the length in device independent pixels for <c>Pixel</c>, the weight for <c>Star</c> and
    /// 0 for <c>Auto</c>.
    /// </summary>
    public double Value => _value;

    /// <summary>
    /// Compares two CompactSpaceSize structures for equality.
    /// </summary>
    /// <param name="a">
    /// The first CompactSpaceSize.
    /// </param>
    /// <param name="b">
    /// The second CompactSpaceSize.
    /// </param>
    /// <returns>
    /// True if the structures are equal, otherwise false.
    /// </returns>
    public static bool operator ==(CompactSpaceSize a, CompactSpaceSize b)
    {
        return (a.IsAuto && b.IsAuto) || (MathUtils.AreClose(a._value, b._value) && a._type == b._type);
    }

    /// <summary>
    /// Compares two CompactSpaceSize structures for inequality.
    /// </summary>
    /// <param name="gl1">
    /// The first CompactSpaceSize.
    /// </param>
    /// <param name="gl2">
    /// The second CompactSpaceSize.
    /// </param>
    /// <returns>
    /// True if the structures are unequal, otherwise false.
    /// </returns>
    public static bool operator !=(CompactSpaceSize gl1, CompactSpaceSize gl2)
    {
        return !(gl1 == gl2);
    }

    /// <summary>
    /// Determines whether the <see cref="CompactSpaceSize" /> is equal to the specified object.
    /// </summary>
    /// <param name="o">
    /// The object with which to test equality.
    /// </param>
    /// <returns>
    /// True if the objects are equal, otherwise false.
    /// </returns>
    public override bool Equals(object? o)
    {
        if (o == null)
        {
            return false;
        }

        if (!(o is CompactSpaceSize))
        {
            return false;
        }

        return this == (CompactSpaceSize)o;
    }

    /// <summary>
    /// Compares two CompactSpaceSize structures for equality.
    /// </summary>
    /// <param name="gridLength">
    /// The structure with which to test equality.
    /// </param>
    /// <returns>
    /// True if the structures are equal, otherwise false.
    /// </returns>
    public bool Equals(CompactSpaceSize gridLength)
    {
        return this == gridLength;
    }

    /// <summary>
    /// Gets a hash code for the CompactSpaceSize.
    /// </summary>
    /// <returns>
    /// The hash code.
    /// </returns>
    public override int GetHashCode()
    {
        return _value.GetHashCode() ^ _type.GetHashCode();
    }

    /// <summary>
    /// Gets a string representation of the <see cref="CompactSpaceSize" />.
    /// </summary>
    /// <returns>
    /// The string representation.
    /// </returns>
    public override string ToString()
    {
        if (IsAuto)
        {
            return "Auto";
        }

        string s = _value.ToString(CultureInfo.InvariantCulture);
        return IsStar ? s + "*" : s;
    }

    /// <summary>
    /// Parses a size from its text form: <c>Auto</c> in any letter case, a number of device independent pixels,
    /// or a number followed by <c>*</c> as a star weight, where a bare <c>*</c> has the weight 1.
    /// </summary>
    /// <param name="s">
    /// The text to parse. Numbers are read with the invariant culture.
    /// </param>
    /// <returns>
    /// The parsed size.
    /// </returns>
    public static CompactSpaceSize Parse(string s)
    {
        return Parse(s.AsSpan());
    }

    private static CompactSpaceSize Parse(ReadOnlySpan<char> s)
    {
        if (s.Equals("AUTO".AsSpan(), StringComparison.OrdinalIgnoreCase))
        {
            return Auto;
        }
        else if (s.EndsWith("*".AsSpan(), StringComparison.Ordinal))
        {
            var valueString = s.Slice(0, s.Length - 1).Trim();
            var value       = valueString.Length > 0 ? double.Parse(valueString, CultureInfo.InvariantCulture) : 1;
            return new CompactSpaceSize(value, CompactSpaceUnitType.Star);
        }
        else
        {
            var value = double.Parse(s, CultureInfo.InvariantCulture);
            return new CompactSpaceSize(value, CompactSpaceUnitType.Pixel);
        }
    }

    /// <summary>
    /// Parses a string to return a collection of <see cref="CompactSpaceSize" />s.
    /// </summary>
    /// <param name="s">
    /// The string.
    /// </param>
    /// <returns>
    /// The parsed sizes, one for each token of the string.
    /// </returns>
    public static IEnumerable<CompactSpaceSize> ParseLengths(string s)
    {
        var result = new List<CompactSpaceSize>();

        using (var tokenizer = new SpanStringTokenizer(s, CultureInfo.InvariantCulture))
        {
            while (tokenizer.TryReadSpan(out var item))
            {
                result.Add(Parse(item));
            }
        }

        return result;
    }
}
