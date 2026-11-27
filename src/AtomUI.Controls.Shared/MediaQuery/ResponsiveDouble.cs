using System.ComponentModel;
using System.Globalization;

namespace AtomUI.Controls;

/// <summary>
/// A finite number that is zero or positive and applies to every breakpoint, or is configured per breakpoint.
/// </summary>
/// <remarks>
/// Values that are configured per breakpoint cascade upwards: a breakpoint uses the value of the largest
/// configured breakpoint that is not larger than it.
/// </remarks>
[TypeConverter(typeof(ResponsiveDoubleConverter))]
public readonly record struct ResponsiveDouble
{
    private readonly ResponsiveValueMap<double> _valueMap;

    /// <summary>
    /// Initializes a value that applies to every breakpoint.
    /// </summary>
    /// <param name="value">
    /// The number to use. It must be zero or positive, and neither NaN nor infinite.
    /// </param>
    /// <exception cref="FormatException">
    /// <paramref name="value" /> is negative, NaN or infinite.
    /// </exception>
    public ResponsiveDouble(double value)
    {
        Validate(value);
        _valueMap = new ResponsiveValueMap<double>(value);
    }

    private ResponsiveDouble(ResponsiveValueMap<double> valueMap)
    {
        _valueMap = valueMap;
    }

    /// <summary>
    /// Parses a number that applies to every breakpoint, or a value for each breakpoint that the input names.
    /// </summary>
    /// <param name="input">
    /// Either a single number such as <c>16</c>, or comma-separated segments of the form
    /// <c>breakpoint:value</c> such as <c>xs: 8, md: 16</c>. The breakpoints are <c>xs</c>, <c>sm</c>, <c>md</c>,
    /// <c>lg</c>, <c>xl</c>, <c>xxl</c> and <c>xxxl</c>, ignoring case. Numbers use the invariant culture.
    /// </param>
    /// <returns>
    /// A value that applies to every breakpoint for a single number, otherwise a value that is configured per
    /// breakpoint.
    /// </returns>
    /// <exception cref="FormatException">
    /// The input is empty, a segment is empty or lacks a colon, a breakpoint name or value is empty, a breakpoint is
    /// named twice or unknown, or a number is not well formed, negative, NaN or infinite.
    /// </exception>
    public static ResponsiveDouble Parse(string input)
    {
        return new ResponsiveDouble(ResponsiveValueParser.Parse(input, ParseDouble, ParseDouble));
    }

    /// <summary>
    /// Gets the value that applies to <paramref name="breakPoint" />.
    /// </summary>
    /// <param name="breakPoint">
    /// The breakpoint to resolve.
    /// </param>
    /// <param name="fallback">
    /// The value to return when no configured breakpoint is small enough for <paramref name="breakPoint" />.
    /// </param>
    /// <returns>
    /// The single value, the value of the largest configured breakpoint that is not larger than
    /// <paramref name="breakPoint" />, or <paramref name="fallback" /> when there is none.
    /// </returns>
    public double Resolve(MediaBreakPoint breakPoint, double fallback)
    {
        return _valueMap.Resolve(breakPoint, fallback);
    }

    /// <summary>
    /// Determines whether a value is configured for <paramref name="breakPoint" />, without using a fallback.
    /// </summary>
    /// <param name="breakPoint">
    /// The breakpoint to resolve.
    /// </param>
    /// <param name="value">
    /// On success, the value for <paramref name="breakPoint" />; otherwise zero.
    /// </param>
    /// <returns>
    /// <see langword="true" /> when the value is a single number or a configured breakpoint is not larger than
    /// <paramref name="breakPoint" />; otherwise <see langword="false" />.
    /// </returns>
    public bool TryResolve(MediaBreakPoint breakPoint, out double value)
    {
        return _valueMap.TryResolve(breakPoint, out value);
    }

    /// <summary>
    /// Converts a number into a value that applies to every breakpoint.
    /// </summary>
    /// <exception cref="FormatException">
    /// <paramref name="value" /> is negative, NaN or infinite.
    /// </exception>
    public static implicit operator ResponsiveDouble(double value) => new(value);

    private static double ParseDouble(ReadOnlySpan<char> input)
    {
        if (!double.TryParse(input, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
        {
            throw new FormatException($"Responsive number value '{input.ToString()}' is not a valid number.");
        }

        Validate(value);
        return value;
    }

    private static void Validate(double value)
    {
        if (double.IsNaN(value) || double.IsInfinity(value) || value < 0)
        {
            throw new FormatException($"Responsive number value must be >= 0, got {value}.");
        }
    }
}

/// <summary>
/// Converts strings and numbers to <see cref="ResponsiveDouble" /> for XAML.
/// </summary>
public class ResponsiveDoubleConverter : TypeConverter
{
    /// <summary>
    /// Returns whether a value of the type <paramref name="sourceType" /> can be converted to a
    /// <see cref="ResponsiveDouble" />.
    /// </summary>
    /// <param name="context">
    /// Ignored.
    /// </param>
    /// <param name="sourceType">
    /// The type of the value to convert.
    /// </param>
    /// <returns>
    /// <see langword="true" /> for <see cref="string" />, <see cref="decimal" />, <see cref="float" />,
    /// <see cref="double" /> and the 16-, 32- and 64-bit integer types; otherwise <see langword="false" />.
    /// </returns>
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
    {
        var typeCode = Type.GetTypeCode(sourceType);
        return typeCode switch
        {
            TypeCode.String => true,
            TypeCode.Decimal => true,
            TypeCode.Single => true,
            TypeCode.Double => true,
            TypeCode.Int16 => true,
            TypeCode.Int32 => true,
            TypeCode.Int64 => true,
            TypeCode.UInt16 => true,
            TypeCode.UInt32 => true,
            TypeCode.UInt64 => true,
            _ => false
        };
    }

    /// <summary>
    /// Converts a string or a number to a <see cref="ResponsiveDouble" />.
    /// </summary>
    /// <param name="context">
    /// Ignored.
    /// </param>
    /// <param name="culture">
    /// The culture that converts a numeric value. <see langword="null" /> uses the invariant culture.
    /// </param>
    /// <param name="value">
    /// A <see cref="ResponsiveDouble" />, which is returned as it is, a string in the format of
    /// <see cref="ResponsiveDouble.Parse" />, or a value that implements <see cref="IConvertible" />.
    /// </param>
    /// <returns>
    /// The converted value.
    /// </returns>
    /// <exception cref="FormatException">
    /// The string or the number is negative, NaN or infinite, or the string is not well formed.
    /// </exception>
    /// <exception cref="NotSupportedException">
    /// <paramref name="value" /> is of another type.
    /// </exception>
    public override object ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
    {
        if (value is ResponsiveDouble responsiveDouble)
        {
            return responsiveDouble;
        }
        if (value is string text)
        {
            return ResponsiveDouble.Parse(text);
        }
        if (value is IConvertible convertible)
        {
            return new ResponsiveDouble(convertible.ToDouble(culture ?? CultureInfo.InvariantCulture));
        }

        throw new NotSupportedException($"Cannot convert value '{value}' to ResponsiveDouble.");
    }
}
