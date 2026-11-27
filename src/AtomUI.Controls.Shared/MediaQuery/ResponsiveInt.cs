using System.ComponentModel;
using System.Globalization;

namespace AtomUI.Controls;

/// <summary>
/// An integer that applies to every breakpoint, or that is configured per breakpoint.
/// </summary>
/// <remarks>
/// Values that are configured per breakpoint cascade upwards: a breakpoint uses the value of the largest
/// configured breakpoint that is not larger than it.
/// </remarks>
[TypeConverter(typeof(ResponsiveIntConverter))]
public readonly record struct ResponsiveInt
{
    private readonly ResponsiveValueMap<int> _valueMap;

    /// <summary>
    /// Initializes a value that applies to every breakpoint.
    /// </summary>
    /// <param name="value">
    /// The number to use.
    /// </param>
    public ResponsiveInt(int value)
    {
        _valueMap = new ResponsiveValueMap<int>(value);
    }

    private ResponsiveInt(ResponsiveValueMap<int> valueMap)
    {
        _valueMap = valueMap;
    }

    /// <summary>
    /// Parses an integer that applies to every breakpoint, or a value for each breakpoint that the input names.
    /// </summary>
    /// <param name="input">
    /// Either a single integer such as <c>3</c>, or comma-separated segments of the form
    /// <c>breakpoint:value</c> such as <c>xs: 1, md: 3</c>. The breakpoints are <c>xs</c>, <c>sm</c>, <c>md</c>,
    /// <c>lg</c>, <c>xl</c>, <c>xxl</c> and <c>xxxl</c>, ignoring case. Numbers use the invariant culture.
    /// </param>
    /// <returns>
    /// A value that applies to every breakpoint for a single integer, otherwise a value that is configured per
    /// breakpoint.
    /// </returns>
    /// <exception cref="FormatException">
    /// The input is empty, a segment is empty or lacks a colon, a breakpoint name or value is empty, a breakpoint is
    /// named twice or unknown, or a value is not a valid integer.
    /// </exception>
    public static ResponsiveInt Parse(string input)
    {
        return new ResponsiveInt(ResponsiveValueParser.Parse(input, ParseInt, ParseInt));
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
    public int Resolve(MediaBreakPoint breakPoint, int fallback)
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
    /// <see langword="true" /> when the value is a single integer or a configured breakpoint is not larger than
    /// <paramref name="breakPoint" />; otherwise <see langword="false" />.
    /// </returns>
    public bool TryResolve(MediaBreakPoint breakPoint, out int value)
    {
        return _valueMap.TryResolve(breakPoint, out value);
    }

    /// <summary>
    /// Converts an integer into a value that applies to every breakpoint.
    /// </summary>
    public static implicit operator ResponsiveInt(int value) => new(value);

    private static int ParseInt(ReadOnlySpan<char> input)
    {
        if (!int.TryParse(input, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value))
        {
            throw new FormatException($"Responsive integer value '{input.ToString()}' is not a valid integer.");
        }

        return value;
    }
}

/// <summary>
/// Converts strings and integers to <see cref="ResponsiveInt" /> for XAML.
/// </summary>
public class ResponsiveIntConverter : TypeConverter
{
    /// <summary>
    /// Returns whether a value of the type <paramref name="sourceType" /> can be converted to a
    /// <see cref="ResponsiveInt" />.
    /// </summary>
    /// <param name="context">
    /// Ignored.
    /// </param>
    /// <param name="sourceType">
    /// The type of the value to convert.
    /// </param>
    /// <returns>
    /// <see langword="true" /> for <see cref="string" /> and the 16-, 32- and 64-bit integer types;
    /// otherwise <see langword="false" />.
    /// </returns>
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
    {
        var typeCode = Type.GetTypeCode(sourceType);
        return typeCode switch
        {
            TypeCode.String => true,
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
    /// Converts a string or an integer to a <see cref="ResponsiveInt" />.
    /// </summary>
    /// <param name="context">
    /// Ignored.
    /// </param>
    /// <param name="culture">
    /// The culture that converts a numeric value. <see langword="null" /> uses the invariant culture.
    /// </param>
    /// <param name="value">
    /// A <see cref="ResponsiveInt" />, which is returned as it is, a string in the format of
    /// <see cref="ResponsiveInt.Parse" />, or a value that implements <see cref="IConvertible" />.
    /// </param>
    /// <returns>
    /// The converted value.
    /// </returns>
    /// <exception cref="FormatException">
    /// The string is not well formed.
    /// </exception>
    /// <exception cref="NotSupportedException">
    /// <paramref name="value" /> is of another type.
    /// </exception>
    public override object ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
    {
        if (value is ResponsiveInt responsiveInt)
        {
            return responsiveInt;
        }
        if (value is string text)
        {
            return ResponsiveInt.Parse(text);
        }
        if (value is IConvertible convertible)
        {
            return new ResponsiveInt(convertible.ToInt32(culture ?? CultureInfo.InvariantCulture));
        }

        throw new NotSupportedException($"Cannot convert value '{value}' to ResponsiveInt.");
    }
}
