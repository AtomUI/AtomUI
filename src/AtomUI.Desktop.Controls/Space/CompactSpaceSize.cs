using System.Globalization;
using AtomUI.Utils;
using Avalonia.Utilities;

namespace AtomUI.Desktop.Controls;

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

public struct CompactSpaceSize : IEquatable<CompactSpaceSize>
{
    private readonly CompactSpaceUnitType _type;
    private readonly double _value;

    /// <summary>
    /// Initializes a new instance of the <see cref="CompactSpaceSize"/> struct.
    /// </summary>
    /// <param name="value">The size of the CompactSpaceSize in device independent pixels.</param>
    public CompactSpaceSize(double value)
        : this(value, CompactSpaceUnitType.Pixel)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CompactSpaceSize"/> struct.
    /// </summary>
    /// <param name="value">The size of the CompactSpaceSize.</param>
    /// <param name="type">The unit of the CompactSpaceSize.</param>
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
    /// Gets an instance of <see cref="CompactSpaceSize"/> that indicates that a row or column should
    /// auto-size to fit its content.
    /// </summary>
    public static CompactSpaceSize Auto => new CompactSpaceSize(0, CompactSpaceUnitType.Auto);

    /// <summary>
    /// Gets an instance of <see cref="CompactSpaceSize"/> that indicates that a row or column should
    /// fill its content.
    /// </summary>
    public static CompactSpaceSize Star => new CompactSpaceSize(1, CompactSpaceUnitType.Star);

    /// <summary>
    /// Gets the unit of the <see cref="CompactSpaceSize"/>.
    /// </summary>
    public CompactSpaceUnitType CompactSpaceUnitType => _type;

    /// <summary>
    /// Gets a value that indicates whether the <see cref="CompactSpaceSize"/> has a <see cref="CompactSpaceUnitType"/> of Pixel.
    /// </summary>
    public bool IsAbsolute => _type == CompactSpaceUnitType.Pixel;

    /// <summary>
    /// Gets a value that indicates whether the <see cref="CompactSpaceSize"/> has a <see cref="CompactSpaceUnitType"/> of Auto.
    /// </summary>
    public bool IsAuto => _type == CompactSpaceUnitType.Auto;

    /// <summary>
    /// Gets a value that indicates whether the <see cref="CompactSpaceSize"/> has a <see cref="CompactSpaceUnitType"/> of Star.
    /// </summary>
    public bool IsStar => _type == CompactSpaceUnitType.Star;

    /// <summary>
    /// Gets the length.
    /// </summary>
    public double Value => _value;

    /// <summary>
    /// Compares two CompactSpaceSize structures for equality.
    /// </summary>
    /// <param name="a">The first CompactSpaceSize.</param>
    /// <param name="b">The second CompactSpaceSize.</param>
    /// <returns>True if the structures are equal, otherwise false.</returns>
    public static bool operator ==(CompactSpaceSize a, CompactSpaceSize b)
    {
        return (a.IsAuto && b.IsAuto) || (MathUtilities.AreClose(a._value, b._value) && a._type == b._type);
    }

    /// <summary>
    /// Compares two CompactSpaceSize structures for inequality.
    /// </summary>
    /// <param name="gl1">The first CompactSpaceSize.</param>
    /// <param name="gl2">The first CompactSpaceSize.</param>
    /// <returns>True if the structures are unequal, otherwise false.</returns>
    public static bool operator !=(CompactSpaceSize gl1, CompactSpaceSize gl2)
    {
        return !(gl1 == gl2);
    }

    /// <summary>
    /// Determines whether the <see cref="CompactSpaceSize"/> is equal to the specified object.
    /// </summary>
    /// <param name="o">The object with which to test equality.</param>
    /// <returns>True if the objects are equal, otherwise false.</returns>
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
    /// <param name="gridLength">The structure with which to test equality.</param>
    /// <returns>True if the structures are equal, otherwise false.</returns>
    public bool Equals(CompactSpaceSize gridLength)
    {
        return this == gridLength;
    }

    /// <summary>
    /// Gets a hash code for the CompactSpaceSize.
    /// </summary>
    /// <returns>The hash code.</returns>
    public override int GetHashCode()
    {
        return _value.GetHashCode() ^ _type.GetHashCode();
    }

    /// <summary>
    /// Gets a string representation of the <see cref="CompactSpaceSize"/>.
    /// </summary>
    /// <returns>The string representation.</returns>
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
    /// Parses a string to return a <see cref="CompactSpaceSize"/>.
    /// </summary>
    /// <param name="s">The string.</param>
    /// <returns>The <see cref="CompactSpaceSize"/>.</returns>
    public static CompactSpaceSize Parse(string s)
    {
        s = s.ToUpperInvariant();

        if (s == "AUTO")
        {
            return Auto;
        }
        else if (s.EndsWith("*"))
        {
            var valueString = s.Substring(0, s.Length - 1).Trim();
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
    /// Parses a string to return a collection of <see cref="CompactSpaceSize"/>s.
    /// </summary>
    /// <param name="s">The string.</param>
    /// <returns>The <see cref="CompactSpaceSize"/>.</returns>
    public static IEnumerable<CompactSpaceSize> ParseLengths(string s)
    {
        var result = new List<CompactSpaceSize>();

        using (var tokenizer = new SpanStringTokenizer(s, CultureInfo.InvariantCulture))
        {
            while (tokenizer.TryReadString(out var item))
            {
                result.Add(Parse(item));
            }
        }

        return result;
    }
}