using System.Globalization;
using AtomUI.Utils;

namespace AtomUI;

/// <summary>
/// The unit of a <see cref="Dimension" /> value: a percentage of a reference size or a number of pixels.
/// </summary>
public enum DimensionUnitType
{
    /// <summary>
    /// The value is a percentage of the reference size given to <see cref="Dimension.Resolve(double)" />.
    /// </summary>
    Percentage = 0,
    /// <summary>
    /// The value is a length in pixels, which <see cref="Dimension.Resolve(double)" /> returns unchanged.
    /// </summary>
    Pixel = 1,
}

/// <summary>
/// A length that is either a number of pixels or a percentage of a reference size supplied when it is resolved.
/// </summary>
/// <remarks>
///     <para>
///         Arithmetic and ordering operators that take two <see cref="Dimension" /> values, and also
///         <see cref="Min" />, <see cref="Max" /> and <see cref="Clamp" />, require the same unit and throw
///         <see cref="InvalidOperationException" /> otherwise. For different units <c>==</c> returns
///         <see langword="false" /> and <c>!=</c> returns <see langword="true" />. A <c>double</c> operand is
///         taken in the unit of the <see cref="Dimension" /> operand, and the result keeps that unit.
///     </para>
///     <para>
///         Every result is built with <see cref="Dimension(double, DimensionUnitType)" />, so an operation on
///         percentages throws <see cref="ArgumentException" /> when its result falls outside 0 to 100. Division and
///         modulo throw <see cref="DivideByZeroException" /> when the divisor is zero.
///     </para>
///     <para>
///         The <c>default</c> value of this struct is 0 percent, not 0 pixels, because
///         <see cref="DimensionUnitType.Percentage" /> is the zero value of the unit. <c>new Dimension()</c> gives
///         0 pixels.
///     </para>
/// </remarks>
public struct Dimension : IEquatable<Dimension>
{
    /// <summary>
    /// The unit in which <see cref="Value" /> is expressed.
    /// </summary>
    public DimensionUnitType UnitType => _type;

    /// <summary>
    /// <see langword="true" /> when the value is a percentage of the size passed to <see cref="Resolve(double)" />.
    /// </summary>
    public bool IsPercentage => _type == DimensionUnitType.Percentage;

    /// <summary>
    /// <see langword="true" /> when the unit is <see cref="DimensionUnitType.Pixel" />.
    /// </summary>
    public bool IsAbsolute => _type == DimensionUnitType.Pixel;

    /// <summary>
    /// The numeric amount in the unit given by <see cref="UnitType" />. A percentage is always from 0 to 100.
    /// A pixel length can be any number, including a negative one, NaN or infinity.
    /// </summary>
    public double Value => _value;

    private readonly DimensionUnitType _type = DimensionUnitType.Pixel;
    private readonly double _value = 0;

    public Dimension()
    {
    }

    /// <summary>
    /// Initializes a length of <paramref name="value" /> pixels.
    /// </summary>
    public Dimension(double value)
        : this(value, DimensionUnitType.Pixel)
    {
    }

    /// <summary>
    /// Initializes a length of <paramref name="value" /> in the unit <paramref name="type" />.
    /// </summary>
    /// <param name="value">
    /// The numeric amount. A percentage must be finite and from 0 to 100. A pixel length is stored as given.
    /// </param>
    /// <param name="type">
    /// The unit of the length.
    /// </param>
    /// <exception cref="ArgumentException">
    /// <paramref name="type" /> is <see cref="DimensionUnitType.Percentage" /> and <paramref name="value" /> is NaN,
    /// infinite or outside 0 to 100.
    /// </exception>
    public Dimension(double value, DimensionUnitType type)
    {
        if (type == DimensionUnitType.Percentage &&
            (double.IsNaN(value) || double.IsInfinity(value) || value < 0 || value > 100))
        {
            throw new ArgumentException("Invalid value for Percentage unit, value must in [0, 100]", nameof(value));
        }

        _type  = type;
        _value = value;
    }

    /// <summary>
    /// Converts the length to pixels: a percentage is scaled against <paramref name="referenceSize" />, a pixel
    /// length is returned unchanged.
    /// </summary>
    /// <param name="referenceSize">
    /// The full size, in pixels, that 100 percent stands for.
    /// </param>
    public double Resolve(double referenceSize)
    {
        return _type switch
        {
            DimensionUnitType.Percentage => _value * referenceSize / 100.0,
            DimensionUnitType.Pixel => _value,
            _ => _value
        };
    }

    private static void ValidateSameUnit(Dimension a, Dimension b)
    {
        if (a._type != b._type)
        {
            throw new InvalidOperationException("Cannot perform operation on dimensions with different units");
        }
    }

    #region 自定义四则运算符

    /// <summary>
    /// Adds the values of <paramref name="a" /> and <paramref name="b" />. Both must share one unit, which the
    /// result keeps.
    /// </summary>
    public static Dimension operator +(Dimension a, Dimension b)
    {
        ValidateSameUnit(a, b);
        return new Dimension(a._value + b._value, a._type);
    }

    /// <summary>
    /// Adds <paramref name="b" /> to the value and keeps the unit.
    /// </summary>
    public static Dimension operator +(Dimension a, double b)
    {
        return new Dimension(a._value + b, a._type);
    }

    /// <summary>
    /// Adds <paramref name="a" /> to the value and keeps the unit.
    /// </summary>
    public static Dimension operator +(double a, Dimension b)
    {
        return new Dimension(a + b._value, b._type);
    }

    /// <summary>
    /// Subtracts the value of <paramref name="b" /> from that of <paramref name="a" />. Both must share one unit,
    /// which the result keeps.
    /// </summary>
    public static Dimension operator -(Dimension a, Dimension b)
    {
        ValidateSameUnit(a, b);
        return new Dimension(a._value - b._value, a._type);
    }

    /// <summary>
    /// Subtracts <paramref name="b" /> from the value and keeps the unit.
    /// </summary>
    public static Dimension operator -(Dimension a, double b)
    {
        return new Dimension(a._value - b, a._type);
    }

    /// <summary>
    /// Subtracts the value from <paramref name="a" /> and keeps the unit.
    /// </summary>
    public static Dimension operator -(double a, Dimension b)
    {
        return new Dimension(a - b._value, b._type);
    }

    /// <summary>
    /// Multiplies the values of <paramref name="a" /> and <paramref name="b" />. Both must share one unit, which
    /// the result keeps.
    /// </summary>
    public static Dimension operator *(Dimension a, Dimension b)
    {
        ValidateSameUnit(a, b);
        return new Dimension(a._value * b._value, a._type);
    }

    /// <summary>
    /// Multiplies the value by <paramref name="b" /> and keeps the unit.
    /// </summary>
    public static Dimension operator *(Dimension a, double b)
    {
        return new Dimension(a._value * b, a._type);
    }

    /// <summary>
    /// Multiplies the value by <paramref name="a" /> and keeps the unit.
    /// </summary>
    public static Dimension operator *(double a, Dimension b)
    {
        return new Dimension(a * b._value, b._type);
    }

    /// <summary>
    /// Divides the value of <paramref name="a" /> by that of <paramref name="b" />, which must not be zero. Both
    /// must share one unit, which the result keeps.
    /// </summary>
    public static Dimension operator /(Dimension a, Dimension b)
    {
        ValidateSameUnit(a, b);
        if (Math.Abs(b._value) < double.Epsilon)
        {
            throw new DivideByZeroException("Division by zero");
        }

        return new Dimension(a._value / b._value, a._type);
    }

    /// <summary>
    /// Divides the value by <paramref name="b" />, which must not be zero, and keeps the unit.
    /// </summary>
    public static Dimension operator /(Dimension a, double b)
    {
        if (Math.Abs(b) < double.Epsilon)
        {
            throw new DivideByZeroException("Division by zero");
        }

        return new Dimension(a._value / b, a._type);
    }

    /// <summary>
    /// Divides <paramref name="a" /> by the value, which must not be zero, and keeps the unit.
    /// </summary>
    public static Dimension operator /(double a, Dimension b)
    {
        if (Math.Abs(b._value) < double.Epsilon)
        {
            throw new DivideByZeroException("Division by zero");
        }

        return new Dimension(a / b._value, b._type);
    }

    /// <summary>
    /// Returns the remainder of <paramref name="a" /> divided by <paramref name="b" />, whose value must not be
    /// zero. Both must share one unit, which the result keeps.
    /// </summary>
    public static Dimension operator %(Dimension a, Dimension b)
    {
        ValidateSameUnit(a, b);
        if (Math.Abs(b._value) < double.Epsilon)
        {
            throw new DivideByZeroException("Modulo by zero");
        }

        return new Dimension(a._value % b._value, a._type);
    }

    /// <summary>
    /// Returns the remainder of the value after division by <paramref name="b" />, which must not be zero.
    /// </summary>
    public static Dimension operator %(Dimension a, double b)
    {
        if (Math.Abs(b) < double.Epsilon)
        {
            throw new DivideByZeroException("Modulo by zero");
        }

        return new Dimension(a._value % b, a._type);
    }

    /// <summary>
    /// Returns <paramref name="a" /> unchanged.
    /// </summary>
    public static Dimension operator +(Dimension a) => a;

    /// <summary>
    /// Negates the value and keeps the unit.
    /// </summary>
    public static Dimension operator -(Dimension a)
    {
        return new Dimension(-a._value, a._type);
    }

    #endregion

    /// <summary>
    /// Returns whether both operands have the same unit and values that differ only within the small tolerance of
    /// <c>MathUtils.AreClose</c>.
    /// </summary>
    public static bool operator ==(Dimension a, Dimension b)
    {
        return (MathUtils.AreClose(a._value, b._value) && a._type == b._type);
    }

    /// <summary>
    /// Returns whether the operands are not equal under the <c>==</c> operator.
    /// </summary>
    public static bool operator !=(Dimension gl1, Dimension gl2)
    {
        return !(gl1 == gl2);
    }

    /// <summary>
    /// Returns whether the value of <paramref name="a" /> is less than that of <paramref name="b" />. Both must
    /// share one unit.
    /// </summary>
    public static bool operator <(Dimension a, Dimension b)
    {
        ValidateSameUnit(a, b);
        return a._value < b._value;
    }

    /// <summary>
    /// Returns whether the value of <paramref name="a" /> is greater than that of
    /// <paramref name="b" />. Both must share one unit.
    /// </summary>
    public static bool operator >(Dimension a, Dimension b)
    {
        ValidateSameUnit(a, b);
        return a._value > b._value;
    }

    /// <summary>
    /// Returns whether the value of <paramref name="a" /> is less than or equal to that of
    /// <paramref name="b" />. Both must share one unit.
    /// </summary>
    public static bool operator <=(Dimension a, Dimension b)
    {
        ValidateSameUnit(a, b);
        return a._value <= b._value;
    }

    /// <summary>
    /// Returns whether the value of <paramref name="a" /> is greater than or equal to that of
    /// <paramref name="b" />. Both must share one unit.
    /// </summary>
    public static bool operator >=(Dimension a, Dimension b)
    {
        ValidateSameUnit(a, b);
        return a._value >= b._value;
    }

    /// <summary>
    /// Returns whether <paramref name="o" /> is a <see cref="Dimension" /> equal to this instance under the
    /// <c>==</c> operator.
    /// </summary>
    public override bool Equals(object? o)
    {
        if (o == null)
        {
            return false;
        }

        if (!(o is Dimension))
        {
            return false;
        }

        return this == (Dimension)o;
    }

    /// <summary>
    /// Returns whether <paramref name="gridLength" /> and this instance are equal under the <c>==</c> operator.
    /// </summary>
    public bool Equals(Dimension gridLength)
    {
        return this == gridLength;
    }

    /// <summary>
    /// Combines the exact value and the unit. Instances that the <c>==</c> operator treats as equal can therefore
    /// still get different hash codes.
    /// </summary>
    public override int GetHashCode()
    {
        return HashCode.Combine(_value, _type);
    }

    /// <summary>
    /// Formats the value with the invariant culture followed by its unit, <c>%</c> or <c>px</c>.
    /// </summary>
    public override string ToString()
    {
        var vstr = _value.ToString(CultureInfo.InvariantCulture);
        if (IsPercentage)
        {
            return $"{vstr}%";
        }

        return $"{vstr}px";
    }

    /// <summary>
    /// Parses a length written as a decimal number with an optional unit suffix, such as <c>12</c>,
    /// <c>12.5px</c> or <c>50%</c>.
    /// </summary>
    /// <param name="s">
    /// White space around the text and between the number and the suffix is ignored. The number is an optional
    /// sign followed by digits with an optional decimal point; exponent notation is not supported. A <c>%</c>
    /// suffix selects a percentage; any suffix of letters, such as <c>px</c>, or none selects pixels.
    /// </param>
    /// <exception cref="FormatException">
    /// <paramref name="s" /> is empty or is not in this format.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="s" /> is a percentage outside 0 to 100.
    /// </exception>
    public static Dimension Parse(string s)
    {
        if (TryParse(s.AsSpan(), out var dimension))
        {
            return dimension;
        }

        throw new FormatException($"Invalid width format: '{s.ToUpperInvariant()}'");
    }

    /// <summary>
    /// Parses lengths separated by white space or commas, each written in the format that
    /// <see cref="Parse(string)" /> accepts.
    /// </summary>
    public static IEnumerable<Dimension> ParseWidths(string s)
    {
        var result = new List<Dimension>(CountWidthTokens(s.AsSpan()));
        using (var tokenizer = new SpanStringTokenizer(s, CultureInfo.InvariantCulture))
        {
            while (tokenizer.TryReadSpan(out var item))
            {
                result.Add(Parse(item));
            }
        }

        return result;
    }

    private static int CountWidthTokens(ReadOnlySpan<char> s)
    {
        var count   = 0;
        var inToken = false;
        for (var i = 0; i < s.Length; ++i)
        {
            var c = s[i];
            if (char.IsWhiteSpace(c) || c == ',')
            {
                inToken = false;
            }
            else if (!inToken)
            {
                count++;
                inToken = true;
            }
        }

        return count;
    }

    private static Dimension Parse(ReadOnlySpan<char> s)
    {
        if (TryParse(s, out var dimension))
        {
            return dimension;
        }

        throw new FormatException($"Invalid width format: '{s.ToString().ToUpperInvariant()}'");
    }

    private static bool TryParse(ReadOnlySpan<char> s, out Dimension dimension)
    {
        dimension = default;
        var span = s.Trim();
        if (span.IsEmpty)
        {
            return false;
        }

        var index = 0;
        if (span[index] == '+' || span[index] == '-')
        {
            index++;
            if (index == span.Length)
            {
                return false;
            }
        }

        var beforeDecimalDigitCount = 0;
        while (index < span.Length && IsAsciiDigit(span[index]))
        {
            beforeDecimalDigitCount++;
            index++;
        }

        if (index < span.Length && span[index] == '.')
        {
            index++;
            var afterDecimalDigitStart = index;
            while (index < span.Length && IsAsciiDigit(span[index]))
            {
                index++;
            }

            if (index == afterDecimalDigitStart)
            {
                return false;
            }
        }
        else if (beforeDecimalDigitCount == 0)
        {
            return false;
        }

        var valueSpan = span.Slice(0, index);
        while (index < span.Length && char.IsWhiteSpace(span[index]))
        {
            index++;
        }

        var unit = DimensionUnitType.Pixel;
        if (index < span.Length)
        {
            var unitStart = index;
            while (index < span.Length && IsDimensionUnitChar(span[index]))
            {
                index++;
            }

            if (index != span.Length)
            {
                return false;
            }

            var unitSpan = span.Slice(unitStart, index - unitStart);
            if (unitSpan.Length == 1 && unitSpan[0] == '%')
            {
                unit = DimensionUnitType.Percentage;
            }
        }

        var value = double.Parse(valueSpan, CultureInfo.InvariantCulture);
        dimension = new Dimension(value, unit);
        return true;
    }

    private static bool IsAsciiDigit(char value)
    {
        return value >= '0' && value <= '9';
    }

    private static bool IsDimensionUnitChar(char value)
    {
        return value == '%' ||
               value is >= 'a' and <= 'z' ||
               value is >= 'A' and <= 'Z';
    }

    /// <summary>
    /// Returns the smaller of two lengths of the same unit, by value.
    /// </summary>
    public static Dimension Min(Dimension a, Dimension b)
    {
        ValidateSameUnit(a, b);
        return new Dimension(Math.Min(a._value, b._value), a._type);
    }

    /// <summary>
    /// Returns the larger of two lengths of the same unit, by value.
    /// </summary>
    public static Dimension Max(Dimension a, Dimension b)
    {
        ValidateSameUnit(a, b);
        return new Dimension(Math.Max(a._value, b._value), a._type);
    }

    /// <summary>
    /// Returns <paramref name="value" /> limited to the range from <paramref name="min" /> to
    /// <paramref name="max" />. All three must share one unit.
    /// </summary>
    public static Dimension Clamp(Dimension value, Dimension min, Dimension max)
    {
        ValidateSameUnit(value, min);
        ValidateSameUnit(value, max);
        return new Dimension(Math.Clamp(value._value, min._value, max._value), value._type);
    }
}
