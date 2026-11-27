using System.ComponentModel;
using System.Globalization;

namespace AtomUI.Controls;

/// <summary>
/// The <see cref="RowJustify" /> of a <see cref="Row" />, set on <see cref="Row.JustifyInfo" />. It applies to
/// every breakpoint, or it is configured per breakpoint and cascades upwards: a breakpoint uses the value of
/// the largest configured breakpoint that is not larger than it.
/// </summary>
[TypeConverter(typeof(GridRowJustifyInfoConverter))]
public readonly record struct GridRowJustifyInfo
{
    private readonly ResponsiveValueMap<RowJustify> _valueMap;

    /// <summary>
    /// Initializes a value that applies to every breakpoint.
    /// </summary>
    public GridRowJustifyInfo(RowJustify value)
    {
        _valueMap = new ResponsiveValueMap<RowJustify>(value);
    }

    private GridRowJustifyInfo(ResponsiveValueMap<RowJustify> valueMap)
    {
        _valueMap = valueMap;
    }

    /// <summary>
    /// Parses a value that applies to every breakpoint, or a value for each breakpoint that the input names.
    /// </summary>
    /// <param name="input">
    /// Either one name such as <c>center</c>, or comma-separated segments of the form <c>breakpoint:value</c> such
    /// as <c>xs: start, md: center</c>. The breakpoints are <c>xs</c>, <c>sm</c>, <c>md</c>, <c>lg</c>,
    /// <c>xl</c>, <c>xxl</c> and <c>xxxl</c>, ignoring case. Each value is parsed with
    /// <see cref="RowJustifyConverter" />. White space around a value or separator is ignored.
    /// </param>
    /// <returns>
    /// A value that applies to every breakpoint for a single name, otherwise a value that is configured per
    /// breakpoint.
    /// </returns>
    /// <exception cref="FormatException">
    /// The input is empty, a segment is empty or lacks a colon, a breakpoint name or value is empty, or a
    /// breakpoint is named twice or unknown.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// A value is not a name that <see cref="RowJustifyConverter" /> accepts.
    /// </exception>
    public static GridRowJustifyInfo Parse(string input)
    {
        return new GridRowJustifyInfo(ResponsiveValueParser.Parse(input, ParseJustify, ParseJustify));
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
    public RowJustify Resolve(MediaBreakPoint breakPoint, RowJustify fallback)
    {
        return _valueMap.Resolve(breakPoint, fallback);
    }

    /// <summary>
    /// Converts a <see cref="RowJustify" /> into a value that applies to every breakpoint.
    /// </summary>
    public static implicit operator GridRowJustifyInfo(RowJustify value) => new(value);

    private static RowJustify ParseJustify(ReadOnlySpan<char> input)
    {
        return RowJustifyConverter.Parse(input);
    }
}

/// <summary>
/// The <see cref="RowAlign" /> of a <see cref="Row" />, set on <see cref="Row.AlignInfo" />. It applies to
/// every breakpoint, or it is configured per breakpoint and cascades upwards: a breakpoint uses the value of
/// the largest configured breakpoint that is not larger than it.
/// </summary>
[TypeConverter(typeof(GridRowAlignInfoConverter))]
public readonly record struct GridRowAlignInfo
{
    private readonly ResponsiveValueMap<RowAlign> _valueMap;

    /// <summary>
    /// Initializes a value that applies to every breakpoint.
    /// </summary>
    public GridRowAlignInfo(RowAlign value)
    {
        _valueMap = new ResponsiveValueMap<RowAlign>(value);
    }

    private GridRowAlignInfo(ResponsiveValueMap<RowAlign> valueMap)
    {
        _valueMap = valueMap;
    }

    /// <summary>
    /// Parses a value that applies to every breakpoint, or a value for each breakpoint that the input names.
    /// </summary>
    /// <param name="input">
    /// Either one name such as <c>top</c>, or comma-separated segments of the form <c>breakpoint:value</c> such
    /// as <c>xs: top, md: center</c>. The breakpoints are <c>xs</c>, <c>sm</c>, <c>md</c>, <c>lg</c>,
    /// <c>xl</c>, <c>xxl</c> and <c>xxxl</c>, ignoring case. Each value is parsed with
    /// <see cref="RowAlignConverter" />. White space around a value or separator is ignored.
    /// </param>
    /// <returns>
    /// A value that applies to every breakpoint for a single name, otherwise a value that is configured per
    /// breakpoint.
    /// </returns>
    /// <exception cref="FormatException">
    /// The input is empty, a segment is empty or lacks a colon, a breakpoint name or value is empty, or a
    /// breakpoint is named twice or unknown.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// A value is not a name that <see cref="RowAlignConverter" /> accepts.
    /// </exception>
    public static GridRowAlignInfo Parse(string input)
    {
        return new GridRowAlignInfo(ResponsiveValueParser.Parse(input, ParseAlign, ParseAlign));
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
    public RowAlign Resolve(MediaBreakPoint breakPoint, RowAlign fallback)
    {
        return _valueMap.Resolve(breakPoint, fallback);
    }

    /// <summary>
    /// Converts a <see cref="RowAlign" /> into a value that applies to every breakpoint.
    /// </summary>
    public static implicit operator GridRowAlignInfo(RowAlign value) => new(value);

    private static RowAlign ParseAlign(ReadOnlySpan<char> input)
    {
        return RowAlignConverter.Parse(input);
    }
}

/// <summary>
/// Converts strings and <see cref="RowJustify" /> values to <see cref="GridRowJustifyInfo" /> for XAML.
/// </summary>
public class GridRowJustifyInfoConverter : TypeConverter
{
    /// <summary>
    /// Returns whether a value of <paramref name="sourceType" /> can be converted to a
    /// <see cref="GridRowJustifyInfo" />, which is only the case for <see cref="string" /> and
    /// <see cref="RowJustify" />.
    /// </summary>
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
    {
        return sourceType == typeof(string) || sourceType == typeof(RowJustify);
    }

    /// <summary>
    /// Converts <paramref name="value" /> to a <see cref="GridRowJustifyInfo" />: a
    /// <see cref="GridRowJustifyInfo" /> is returned as it is, a <see cref="RowJustify" /> becomes a value that
    /// applies to every breakpoint, and a string is parsed with
    /// <see cref="GridRowJustifyInfo.Parse(string)" />.
    /// </summary>
    /// <exception cref="NotSupportedException">
    /// <paramref name="value" /> is <see langword="null" /> or neither a <see cref="GridRowJustifyInfo" />, a
    /// <see cref="RowJustify" /> nor a string.
    /// </exception>
    /// <exception cref="FormatException">
    /// <paramref name="value" /> is a string that has no readable breakpoint form.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="value" /> is a string that holds a value that is not a name
    /// <see cref="RowJustifyConverter" /> accepts.
    /// </exception>
    public override object ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
    {
        if (value is GridRowJustifyInfo info)
        {
            return info;
        }

        if (value is RowJustify justify)
        {
            return new GridRowJustifyInfo(justify);
        }

        if (value is string text)
        {
            return GridRowJustifyInfo.Parse(text);
        }

        throw new NotSupportedException($"Cannot convert value '{value}' to GridRowJustifyInfo.");
    }
}

/// <summary>
/// Converts strings and <see cref="RowAlign" /> values to <see cref="GridRowAlignInfo" /> for XAML.
/// </summary>
public class GridRowAlignInfoConverter : TypeConverter
{
    /// <summary>
    /// Returns whether a value of <paramref name="sourceType" /> can be converted to a
    /// <see cref="GridRowAlignInfo" />, which is only the case for <see cref="string" /> and
    /// <see cref="RowAlign" />.
    /// </summary>
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
    {
        return sourceType == typeof(string) || sourceType == typeof(RowAlign);
    }

    /// <summary>
    /// Converts <paramref name="value" /> to a <see cref="GridRowAlignInfo" />: a
    /// <see cref="GridRowAlignInfo" /> is returned as it is, a <see cref="RowAlign" /> becomes a value that
    /// applies to every breakpoint, and a string is parsed with
    /// <see cref="GridRowAlignInfo.Parse(string)" />.
    /// </summary>
    /// <exception cref="NotSupportedException">
    /// <paramref name="value" /> is <see langword="null" /> or neither a <see cref="GridRowAlignInfo" />, a
    /// <see cref="RowAlign" /> nor a string.
    /// </exception>
    /// <exception cref="FormatException">
    /// <paramref name="value" /> is a string that has no readable breakpoint form.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="value" /> is a string that holds a value that is not a name
    /// <see cref="RowAlignConverter" /> accepts.
    /// </exception>
    public override object ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
    {
        if (value is GridRowAlignInfo info)
        {
            return info;
        }

        if (value is RowAlign align)
        {
            return new GridRowAlignInfo(align);
        }

        if (value is string text)
        {
            return GridRowAlignInfo.Parse(text);
        }

        throw new NotSupportedException($"Cannot convert value '{value}' to GridRowAlignInfo.");
    }
}
