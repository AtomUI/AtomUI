using System.ComponentModel;
using System.Globalization;

namespace AtomUI.Controls;

/// <summary>
/// The number of the 24 columns of a row that a grid column occupies, for each breakpoint.
/// </summary>
/// <remarks>
/// It is the value of <see cref="Col.Span" />. A span of <c>0</c> hides the column at that breakpoint.
/// </remarks>
[TypeConverter(typeof(GridColSpanInfoConverter))]
public readonly record struct GridColSpanInfo
{
    /// <summary>
    /// The span for the <see cref="MediaBreakPoint.ExtraSmall" /> breakpoint (<c>xs</c>).
    /// </summary>
    public int ExtraSmall { get; init; }
    /// <summary>
    /// The span for the <see cref="MediaBreakPoint.Small" /> breakpoint (<c>sm</c>).
    /// </summary>
    public int Small { get; init; }
    /// <summary>
    /// The span for the <see cref="MediaBreakPoint.Medium" /> breakpoint (<c>md</c>).
    /// </summary>
    public int Medium { get; init; }
    /// <summary>
    /// The span for the <see cref="MediaBreakPoint.Large" /> breakpoint (<c>lg</c>).
    /// </summary>
    public int Large { get; init; }
    /// <summary>
    /// The span for the <see cref="MediaBreakPoint.ExtraLarge" /> breakpoint (<c>xl</c>).
    /// </summary>
    public int ExtraLarge { get; init; }
    /// <summary>
    /// The span for the <see cref="MediaBreakPoint.ExtraExtraLarge" /> breakpoint (<c>xxl</c>).
    /// </summary>
    public int ExtraExtraLarge { get; init; }
    /// <summary>
    /// The span for the <see cref="MediaBreakPoint.ExtraExtraExtraLarge" /> breakpoint (<c>xxxl</c>).
    /// </summary>
    public int ExtraExtraExtraLarge { get; init; }

    /// <summary>
    /// Initializes a span that is the same for every breakpoint.
    /// </summary>
    /// <param name="span">
    /// The number of columns the column occupies at every breakpoint. It must be between 0 and 24.
    /// </param>
    /// <exception cref="FormatException">
    /// <paramref name="span" /> is not between 0 and 24.
    /// </exception>
    public GridColSpanInfo(int span)
    {
        ValidateSpan(span);
        ExtraSmall = span;
        Small = span;
        Medium = span;
        Large = span;
        ExtraLarge = span;
        ExtraExtraLarge = span;
        ExtraExtraExtraLarge = span;
    }

    /// <summary>
    /// Initializes a span for the breakpoints up to the extra-extra-large one. The extra-extra-extra-large
    /// breakpoint gets the span of the extra-extra-large breakpoint.
    /// </summary>
    /// <param name="extraSmall">
    /// The span for the extra-small breakpoint.
    /// </param>
    /// <param name="small">
    /// The span for the small breakpoint.
    /// </param>
    /// <param name="medium">
    /// The span for the medium breakpoint.
    /// </param>
    /// <param name="large">
    /// The span for the large breakpoint.
    /// </param>
    /// <param name="extraLarge">
    /// The span for the extra-large breakpoint.
    /// </param>
    /// <param name="extraExtraLarge">
    /// The span for the extra-extra-large breakpoint and for the extra-extra-extra-large breakpoint.
    /// </param>
    /// <exception cref="FormatException">
    /// A span is not between 0 and 24.
    /// </exception>
    public GridColSpanInfo(int extraSmall, int small, int medium, int large, int extraLarge, int extraExtraLarge)
        : this(extraSmall, small, medium, large, extraLarge, extraExtraLarge, extraExtraLarge)
    {
    }

    /// <summary>
    /// Initializes a span for every breakpoint.
    /// </summary>
    /// <param name="extraSmall">
    /// The span for the extra-small breakpoint.
    /// </param>
    /// <param name="small">
    /// The span for the small breakpoint.
    /// </param>
    /// <param name="medium">
    /// The span for the medium breakpoint.
    /// </param>
    /// <param name="large">
    /// The span for the large breakpoint.
    /// </param>
    /// <param name="extraLarge">
    /// The span for the extra-large breakpoint.
    /// </param>
    /// <param name="extraExtraLarge">
    /// The span for the extra-extra-large breakpoint.
    /// </param>
    /// <param name="extraExtraExtraLarge">
    /// The span for the extra-extra-extra-large breakpoint.
    /// </param>
    /// <exception cref="FormatException">
    /// A span is not between 0 and 24.
    /// </exception>
    public GridColSpanInfo(int extraSmall,
                           int small,
                           int medium,
                           int large,
                           int extraLarge,
                           int extraExtraLarge,
                           int extraExtraExtraLarge)
    {
        ValidateSpan(extraSmall);
        ValidateSpan(small);
        ValidateSpan(medium);
        ValidateSpan(large);
        ValidateSpan(extraLarge);
        ValidateSpan(extraExtraLarge);
        ValidateSpan(extraExtraExtraLarge);

        ExtraSmall = extraSmall;
        Small = small;
        Medium = medium;
        Large = large;
        ExtraLarge = extraLarge;
        ExtraExtraLarge = extraExtraLarge;
        ExtraExtraExtraLarge = extraExtraExtraLarge;
    }

    /// <summary>
    /// Parses a span that is the same for every breakpoint, or a span for each breakpoint that the input names.
    /// </summary>
    /// <param name="input">
    /// Either a single number such as <c>6</c>, or comma-separated segments of the form <c>breakpoint:value</c>
    /// such as <c>xs: 24, sm: 12</c>. The breakpoints are <c>xs</c>, <c>sm</c>, <c>md</c>, <c>lg</c>, <c>xl</c>,
    /// <c>xxl</c> and <c>xxxl</c>, ignoring case.
    /// </param>
    /// <returns>
    /// For a single number, that span for every breakpoint. Otherwise every breakpoint gets the number of the
    /// nearest named breakpoint at or below it, or <c>0</c> when there is none, which hides the column at that
    /// breakpoint.
    /// </returns>
    /// <exception cref="FormatException">
    /// A number is not between 0 and 24, or <see cref="ResponsiveInt.Parse" /> rejects the input.
    /// </exception>
    public static GridColSpanInfo Parse(string input)
    {
        if (int.TryParse(input.AsSpan().Trim(), out var singleSpan))
        {
            ValidateSpan(singleSpan);
            return new GridColSpanInfo(singleSpan);
        }

        var responsive = ResponsiveInt.Parse(input);
        return new GridColSpanInfo(
            responsive.Resolve(MediaBreakPoint.ExtraSmall, 0),
            responsive.Resolve(MediaBreakPoint.Small, 0),
            responsive.Resolve(MediaBreakPoint.Medium, 0),
            responsive.Resolve(MediaBreakPoint.Large, 0),
            responsive.Resolve(MediaBreakPoint.ExtraLarge, 0),
            responsive.Resolve(MediaBreakPoint.ExtraExtraLarge, 0),
            responsive.Resolve(MediaBreakPoint.ExtraExtraExtraLarge, 0));
    }

    /// <summary>
    /// Gets the span for a breakpoint.
    /// </summary>
    /// <param name="breakPoint">
    /// The breakpoint to read.
    /// </param>
    /// <returns>
    /// The span that was set for <paramref name="breakPoint" />.
    /// </returns>
    public int GetValue(MediaBreakPoint breakPoint)
    {
        return breakPoint switch
        {
            MediaBreakPoint.ExtraSmall => ExtraSmall,
            MediaBreakPoint.Small => Small,
            MediaBreakPoint.Medium => Medium,
            MediaBreakPoint.Large => Large,
            MediaBreakPoint.ExtraLarge => ExtraLarge,
            MediaBreakPoint.ExtraExtraLarge => ExtraExtraLarge,
            _ => ExtraExtraExtraLarge
        };
    }

    private static void ValidateSpan(int span)
    {
        if (span < 0 || span > 24)
        {
            throw new FormatException($"Span must be between 0 and 24, got {span}.");
        }
    }

    /// <summary>
    /// Creates a span that is the same for every breakpoint.
    /// </summary>
    public static implicit operator GridColSpanInfo(int span) => new(span);
}

/// <summary>
/// Converts strings and numbers to a <see cref="GridColSpanInfo" /> for XAML, and a <see cref="GridColSpanInfo" />
/// to a string.
/// </summary>
public class GridColSpanInfoConverter : TypeConverter
{
    /// <summary>
    /// Returns whether a value of the type <paramref name="sourceType" /> can be converted to a
    /// <see cref="GridColSpanInfo" />.
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
    /// Returns whether a <see cref="GridColSpanInfo" /> can be converted to the type
    /// <paramref name="destinationType" />.
    /// </summary>
    /// <param name="context">
    /// Ignored.
    /// </param>
    /// <param name="destinationType">
    /// The type to convert to.
    /// </param>
    /// <returns>
    /// <see langword="true" /> for <see cref="string" />; otherwise <see langword="false" />.
    /// </returns>
    public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
    {
        return destinationType == typeof(string);
    }

    /// <summary>
    /// Converts a string or a number to a <see cref="GridColSpanInfo" />.
    /// </summary>
    /// <param name="context">
    /// Ignored.
    /// </param>
    /// <param name="culture">
    /// The culture that converts a numeric value. <see langword="null" /> uses the invariant culture.
    /// </param>
    /// <param name="value">
    /// A <see cref="GridColSpanInfo" />, which is returned as it is, a string in the format of
    /// <see cref="GridColSpanInfo.Parse" />, or a value that implements <see cref="IConvertible" />, which is used
    /// as the span of every breakpoint.
    /// </param>
    /// <returns>
    /// The converted value.
    /// </returns>
    /// <exception cref="FormatException">
    /// The string is not well formed, or the number is not between 0 and 24.
    /// </exception>
    /// <exception cref="NotSupportedException">
    /// <paramref name="value" /> is <see langword="null" /> or of another type.
    /// </exception>
    public override object ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
    {
        if (value == null)
        {
            throw new NotSupportedException("Cannot convert null to GridColSpanInfo.");
        }

        if (value is GridColSpanInfo spanInfo)
        {
            return spanInfo;
        }

        if (value is string str)
        {
            return GridColSpanInfo.Parse(str);
        }

        if (value is IConvertible convertible)
        {
            var number = convertible.ToInt32(culture ?? CultureInfo.InvariantCulture);
            return new GridColSpanInfo(number);
        }

        throw new NotSupportedException($"Cannot convert value '{value}' to GridColSpanInfo.");
    }

    /// <summary>
    /// Converts a <see cref="GridColSpanInfo" /> to the span of its extra-small breakpoint, as a string.
    /// </summary>
    /// <param name="context">
    /// Ignored.
    /// </param>
    /// <param name="culture">
    /// Ignored.
    /// </param>
    /// <param name="value">
    /// The value to convert.
    /// </param>
    /// <param name="destinationType">
    /// The type to convert to. Only <see cref="string" /> is supported.
    /// </param>
    /// <returns>
    /// <see cref="GridColSpanInfo.ExtraSmall" /> as a number. The spans of the other breakpoints are not written.
    /// An empty string when <paramref name="value" /> is not a <see cref="GridColSpanInfo" />.
    /// </returns>
    /// <exception cref="NotSupportedException">
    /// <paramref name="destinationType" /> is not <see cref="string" />.
    /// </exception>
    public override object ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
    {
        if (destinationType != typeof(string))
        {
            throw new NotSupportedException($"Cannot convert GridColSpanInfo to {destinationType}.");
        }

        if (value is GridColSpanInfo spanInfo)
        {
            return spanInfo.ExtraSmall.ToString(CultureInfo.InvariantCulture);
        }

        return string.Empty;
    }
}
