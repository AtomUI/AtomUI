using System.ComponentModel;
using System.Globalization;
using System.Text;

namespace AtomUI.Controls;

/// <summary>
/// The settings of a <see cref="Col" /> for one breakpoint. Every setting is optional: one that is
/// <see langword="null" /> leaves the setting of the column unchanged.
/// </summary>
/// <remarks>
/// It is the value of the per-breakpoint properties of a <see cref="Col" />, such as <see cref="Col.Xs" />.
/// <see cref="Span" />, <see cref="Offset" />, <see cref="Push" /> and <see cref="Pull" /> are between 0 and 24
/// when they come from <see cref="Parse" />. A value that is set in code is not checked.
/// </remarks>
[TypeConverter(typeof(GridColSizeConverter))]
public record GridColSize
{
    /// <summary>
    /// How the column shares the free width or the overflow of its line, or <see langword="null" /> to leave the
    /// setting of the column unchanged.
    /// </summary>
    public GridColFlex? Flex { get; init; }
    /// <summary>
    /// The number of the 24 columns of the row that the column occupies, or <see langword="null" /> to leave the
    /// setting of the column unchanged. A value of <c>0</c> hides the column at that breakpoint.
    /// </summary>
    public int? Span { get; init; }
    /// <summary>
    /// The number of empty columns to the left of the column, or <see langword="null" /> to leave the setting of
    /// the column unchanged.
    /// </summary>
    public int? Offset { get; init; }
    /// <summary>
    /// The position of the column among the children of the row, or <see langword="null" /> to leave the setting
    /// of the column unchanged. Columns with a lower order come first, and negative values are allowed.
    /// </summary>
    public int? Order { get; init; }
    /// <summary>
    /// The number of columns the column is moved to the right, or <see langword="null" /> to leave the setting of
    /// the column unchanged.
    /// </summary>
    public int? Push { get; init; }
    /// <summary>
    /// The number of columns the column is moved to the left, or <see langword="null" /> to leave the setting of
    /// the column unchanged.
    /// </summary>
    public int? Pull { get; init; }

    /// <summary>
    /// Parses the text form of the settings.
    /// </summary>
    /// <param name="input">
    /// Either a single number for <see cref="Span" />, or comma-separated segments of the form <c>key:value</c>
    /// such as <c>span: 6, order: 2</c>. The keys are <c>flex</c>, <c>span</c>, <c>offset</c>, <c>order</c>,
    /// <c>push</c> and <c>pull</c>, ignoring case. The value of <c>flex</c> has the form that
    /// <see cref="GridColFlex.Parse" /> reads. White space around a key or a value is ignored.
    /// </param>
    /// <returns>
    /// A <see cref="GridColSize" /> in which every setting that the input does not name is
    /// <see langword="null" />.
    /// </returns>
    /// <exception cref="FormatException">
    /// The input is empty, a segment has no colon or an empty key or value, a key is unknown, a value is not a
    /// number, <see cref="GridColFlex.Parse" /> rejects the value of <c>flex</c>, or <see cref="Span" />,
    /// <see cref="Offset" />, <see cref="Push" /> or <see cref="Pull" /> is not between 0 and 24.
    /// </exception>
    public static GridColSize Parse(string input)
    {
        var trimmed = input.AsSpan().Trim();
        if (trimmed.Length == 0)
        {
            throw new FormatException("Grid column size cannot be empty.");
        }

        if (int.TryParse(trimmed, out var singleSpan))
        {
            ValidateColumnValue(singleSpan, "span");
            return new GridColSize { Span = singleSpan };
        }

        var result = new GridColSize();
        while (!trimmed.IsEmpty)
        {
            var commaIndex = trimmed.IndexOf(',');
            var segment    = commaIndex >= 0 ? trimmed[..commaIndex].Trim() : trimmed.Trim();
            trimmed = commaIndex >= 0 ? trimmed[(commaIndex + 1)..] : ReadOnlySpan<char>.Empty;
            if (segment.Length == 0)
            {
                continue;
            }

            var colonIndex = segment.IndexOf(':');
            if (colonIndex < 0)
            {
                throw new FormatException($"Missing ':' in segment '{segment}'.");
            }

            var key = segment[..colonIndex].Trim();
            var value = segment[(colonIndex + 1)..].Trim();
            if (key.Length == 0 || value.Length == 0)
            {
                throw new FormatException($"Invalid segment '{segment}'.");
            }

            if (key.Equals("flex", StringComparison.OrdinalIgnoreCase))
            {
                result = result with { Flex = GridColFlex.Parse(value.ToString()) };
            }
            else if (key.Equals("span", StringComparison.OrdinalIgnoreCase))
            {
                var span = ParseInt(value, "span", allowNegative: false);
                ValidateColumnValue(span, "span");
                result = result with { Span = span };
            }
            else if (key.Equals("offset", StringComparison.OrdinalIgnoreCase))
            {
                var offset = ParseInt(value, "offset", allowNegative: false);
                ValidateColumnValue(offset, "offset");
                result = result with { Offset = offset };
            }
            else if (key.Equals("order", StringComparison.OrdinalIgnoreCase))
            {
                var order = ParseInt(value, "order", allowNegative: true);
                result = result with { Order = order };
            }
            else if (key.Equals("push", StringComparison.OrdinalIgnoreCase))
            {
                var push = ParseInt(value, "push", allowNegative: false);
                ValidateColumnValue(push, "push");
                result = result with { Push = push };
            }
            else if (key.Equals("pull", StringComparison.OrdinalIgnoreCase))
            {
                var pull = ParseInt(value, "pull", allowNegative: false);
                ValidateColumnValue(pull, "pull");
                result = result with { Pull = pull };
            }
            else
            {
                throw new FormatException($"Unknown grid column key '{key}'.");
            }
        }

        return result;
    }

    internal GridColLayout ApplyTo(GridColLayout layout)
    {
        return layout with
        {
            Flex = Flex ?? layout.Flex,
            Span = Span ?? layout.Span,
            IsSpanSpecified = Span.HasValue || layout.IsSpanSpecified,
            Offset = Offset ?? layout.Offset,
            Order = Order ?? layout.Order,
            Push = Push ?? layout.Push,
            Pull = Pull ?? layout.Pull
        };
    }

    private static int ParseInt(ReadOnlySpan<char> input, string name, bool allowNegative)
    {
        if (!int.TryParse(input, out var value))
        {
            throw new FormatException($"Invalid {name} value '{input.ToString()}'.");
        }

        if (!allowNegative && value < 0)
        {
            throw new FormatException($"{name} must be >= 0, got {value}.");
        }

        return value;
    }

    private static void ValidateColumnValue(int value, string name)
    {
        if (value < 0 || value > 24)
        {
            throw new FormatException($"{name} must be between 0 and 24, got {value}.");
        }
    }
}

internal readonly record struct GridColLayout(
    GridColFlex? Flex,
    int Span,
    bool IsSpanSpecified,
    int Offset,
    int Order,
    int Push,
    int Pull)
{
    public bool IsHidden => IsSpanSpecified && Span == 0;
}

/// <summary>
/// Converts strings and numbers to a <see cref="GridColSize" /> for XAML, and a <see cref="GridColSize" /> to a
/// string.
/// </summary>
public class GridColSizeConverter : TypeConverter
{
    /// <summary>
    /// Returns whether a value of the type <paramref name="sourceType" /> can be converted to a
    /// <see cref="GridColSize" />.
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
    /// Returns whether a <see cref="GridColSize" /> can be converted to the type <paramref name="destinationType" />.
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
    /// Converts a string or a number to a <see cref="GridColSize" />.
    /// </summary>
    /// <param name="context">
    /// Ignored.
    /// </param>
    /// <param name="culture">
    /// The culture that converts a numeric value. <see langword="null" /> uses the invariant culture.
    /// </param>
    /// <param name="value">
    /// A <see cref="GridColSize" />, which is returned as it is, a string in the format of
    /// <see cref="GridColSize.Parse" />, or a value that implements <see cref="IConvertible" />, which is used as
    /// <see cref="GridColSize.Span" /> without checking its range.
    /// </param>
    /// <returns>
    /// The converted value.
    /// </returns>
    /// <exception cref="FormatException">
    /// The string is not well formed.
    /// </exception>
    /// <exception cref="NotSupportedException">
    /// <paramref name="value" /> is <see langword="null" /> or of another type.
    /// </exception>
    public override object ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
    {
        if (value == null)
        {
            throw new NotSupportedException("Cannot convert null to GridColSize.");
        }

        if (value is GridColSize size)
        {
            return size;
        }

        if (value is string str)
        {
            return GridColSize.Parse(str);
        }

        if (value is IConvertible convertible)
        {
            var number = convertible.ToInt32(culture ?? CultureInfo.InvariantCulture);
            return new GridColSize { Span = number };
        }

        throw new NotSupportedException($"Cannot convert value '{value}' to GridColSize.");
    }

    /// <summary>
    /// Converts a <see cref="GridColSize" /> to a string.
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
    /// The number of <see cref="GridColSize.Span" /> when only the span has a value. Otherwise comma-separated
    /// segments of the form <c>key:value</c> for the settings that have a value, in the order <c>flex</c>,
    /// <c>span</c>, <c>offset</c>, <c>order</c>, <c>push</c> and <c>pull</c>. An empty string when no setting has a
    /// value or <paramref name="value" /> is not a <see cref="GridColSize" />.
    /// </returns>
    /// <exception cref="NotSupportedException">
    /// <paramref name="destinationType" /> is not <see cref="string" />.
    /// </exception>
    public override object ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
    {
        if (destinationType != typeof(string))
        {
            throw new NotSupportedException($"Cannot convert GridColSize to {destinationType}.");
        }

        if (value is GridColSize size)
        {
            if (size.Flex is null &&
                size.Span.HasValue &&
                size.Offset is null &&
                size.Order is null &&
                size.Push is null &&
                size.Pull is null)
            {
                return size.Span.Value.ToString(CultureInfo.InvariantCulture);
            }

            var builder = new StringBuilder();
            if (size.Flex.HasValue)
            {
                AppendPart(builder, "flex", FormatFlex(size.Flex.Value));
            }
            if (size.Span.HasValue)
            {
                AppendPart(builder, "span", size.Span.Value);
            }
            if (size.Offset.HasValue)
            {
                AppendPart(builder, "offset", size.Offset.Value);
            }
            if (size.Order.HasValue)
            {
                AppendPart(builder, "order", size.Order.Value);
            }
            if (size.Push.HasValue)
            {
                AppendPart(builder, "push", size.Push.Value);
            }
            if (size.Pull.HasValue)
            {
                AppendPart(builder, "pull", size.Pull.Value);
            }
            return builder.ToString();
        }

        return string.Empty;
    }

    private static void AppendPart(StringBuilder builder, string name, int value)
    {
        AppendPart(builder, name, value.ToString(CultureInfo.InvariantCulture));
    }

    private static void AppendPart(StringBuilder builder, string name, string value)
    {
        if (builder.Length > 0)
        {
            builder.Append(',');
        }

        builder.Append(name);
        builder.Append(':');
        builder.Append(value);
    }

    private static string FormatFlex(GridColFlex flex)
    {
        if (flex == GridColFlex.Auto)
        {
            return "auto";
        }

        if (flex == GridColFlex.None)
        {
            return "none";
        }

        if (flex.Basis.HasValue)
        {
            return FormattableString.Invariant($"{flex.Basis.Value:G17}px");
        }

        return FormattableString.Invariant($"{flex.Grow:G17}");
    }
}
