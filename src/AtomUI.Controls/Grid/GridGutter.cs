using System.ComponentModel;
using System.Globalization;

namespace AtomUI.Controls;

/// <summary>
/// The spacing around the columns of a <see cref="Row" />, set on <see cref="Row.Gutter" />. Each part is a
/// <see cref="GridGutterInfo" />, so the spacing can be one width or one width per breakpoint.
/// </summary>
[TypeConverter(typeof(GridGutterConverter))]
public record GridGutter
{
    /// <summary>
    /// The gutter between adjacent columns. A <see cref="Row" /> puts half of it on both sides of each column.
    /// </summary>
    public GridGutterInfo Horizontal { get; init; }
    /// <summary>
    /// The gutter between adjacent lines of a <see cref="Row" />.
    /// </summary>
    public GridGutterInfo Vertical { get; init; }

    public GridGutter()
        : this(new GridGutterInfo(), new GridGutterInfo())
    {
    }

    /// <summary>
    /// Initializes a gutter from its horizontal and its vertical part.
    /// </summary>
    /// <param name="horizontal">
    /// The gutter between adjacent columns.
    /// </param>
    /// <param name="vertical">
    /// The gutter between adjacent lines.
    /// </param>
    public GridGutter(GridGutterInfo horizontal, GridGutterInfo vertical)
    {
        Horizontal = horizontal;
        Vertical   = vertical;
    }

    /// <summary>
    /// Parses the text of a <see cref="Row.Gutter" /> value.
    /// </summary>
    /// <param name="input">
    /// One of these forms. A number such as <c>16</c> sets the horizontal gutter at every breakpoint and leaves
    /// the vertical gutter at zero. Two numbers such as <c>16, 24</c> set the horizontal and the vertical gutter
    /// at every breakpoint. A breakpoint list such as <c>xs: 8, md: 16</c> sets the horizontal gutter per
    /// breakpoint. Two parts separated by <c>;</c> such as <c>xs: 8, md: 16; 24</c> set the horizontal and the
    /// vertical gutter; each part is read by <see cref="GridGutterInfo.Parse(string)" />. White space around a
    /// value or separator is ignored.
    /// </param>
    /// <returns>
    /// The parsed gutter.
    /// </returns>
    /// <exception cref="FormatException">
    /// The input is empty, does not match one of the forms, has a negative number in the two-number form, or
    /// contains a part that <see cref="GridGutterInfo.Parse(string)" /> rejects.
    /// </exception>
    public static GridGutter Parse(string input)
    {
        var trimmed = input.AsSpan().Trim();
        if (trimmed.Length == 0)
        {
            throw new FormatException("Gutter value cannot be empty.");
        }

        if (trimmed.IndexOf(';') >= 0)
        {
            if (!TrySplitTwoNonEmptySegments(trimmed, ';', out var horizontalSegment, out var verticalSegment))
            {
                throw new FormatException("Gutter must have exactly two segments when using ';' separator.");
            }

            var horizontal = GridGutterInfo.Parse(horizontalSegment.Trim());
            var vertical   = GridGutterInfo.Parse(verticalSegment.Trim());
            return new GridGutter(horizontal, vertical);
        }

        if (trimmed.IndexOf(':') >= 0)
        {
            var horizontal = GridGutterInfo.Parse(trimmed);
            return new GridGutter(horizontal, new GridGutterInfo());
        }

        if (TryParsePair(trimmed, out var horizontalValue, out var verticalValue))
        {
            return new GridGutter(new GridGutterInfo(horizontalValue), new GridGutterInfo(verticalValue));
        }

        if (double.TryParse(trimmed, NumberStyles.Float, CultureInfo.InvariantCulture, out var single))
        {
            return new GridGutter(new GridGutterInfo(single), new GridGutterInfo());
        }

        throw new FormatException($"Invalid gutter format '{input}'.");
    }

    internal (double Horizontal, double Vertical) Resolve(MediaBreakPoint breakPoint)
    {
        return (Horizontal.GetValue(breakPoint), Vertical.GetValue(breakPoint));
    }

    private static bool TryParsePair(ReadOnlySpan<char> input, out double horizontal, out double vertical)
    {
        horizontal = 0;
        vertical   = 0;

        if (!TrySplitTwoNonEmptySegments(input, ',', out var horizontalSegment, out var verticalSegment))
        {
            return false;
        }

        if (!double.TryParse(horizontalSegment.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out horizontal))
        {
            return false;
        }

        if (!double.TryParse(verticalSegment.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out vertical))
        {
            return false;
        }

        if (horizontal < 0 || vertical < 0)
        {
            throw new FormatException("Gutter values must be >= 0.");
        }

        return true;
    }

    private static bool TrySplitTwoNonEmptySegments(ReadOnlySpan<char> input,
                                                    char separator,
                                                    out ReadOnlySpan<char> first,
                                                    out ReadOnlySpan<char> second)
    {
        first  = default;
        second = default;
        var foundCount = 0;

        while (!input.IsEmpty)
        {
            var separatorIndex = input.IndexOf(separator);
            var segment        = separatorIndex >= 0 ? input[..separatorIndex] : input;
            input              = separatorIndex >= 0 ? input[(separatorIndex + 1)..] : ReadOnlySpan<char>.Empty;

            if (segment.IsEmpty)
            {
                continue;
            }

            foundCount++;
            if (foundCount == 1)
            {
                first = segment;
            }
            else if (foundCount == 2)
            {
                second = segment;
            }
            else
            {
                return false;
            }
        }

        return foundCount == 2;
    }
}

/// <summary>
/// Converts strings and numbers to <see cref="GridGutter" /> for XAML, and converts a <see cref="GridGutter" />
/// back to text.
/// </summary>
public class GridGutterConverter : TypeConverter
{
    /// <summary>
    /// Returns whether a value of <paramref name="sourceType" /> can be converted to a <see cref="GridGutter" />,
    /// which is the case for <see cref="string" />, <c>decimal</c>, <c>float</c>, <c>double</c> and the signed and
/// unsigned integer types of 16, 32 and 64 bits.
    /// </summary>
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
    /// Returns whether a <see cref="GridGutter" /> can be converted to <paramref name="destinationType" />, which is
    /// only the case for <see cref="string" />.
    /// </summary>
    public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
    {
        return destinationType == typeof(string);
    }

    /// <summary>
    /// Converts <paramref name="value" /> to a <see cref="GridGutter" />: a <see cref="GridGutter" /> is returned
    /// as it is, a string is parsed with <see cref="GridGutter.Parse(string)" />, and a number becomes the
    /// horizontal gutter at every breakpoint with a vertical gutter of zero.
    /// </summary>
    /// <returns>
    /// The converted gutter.
    /// </returns>
    /// <exception cref="NotSupportedException">
    /// <paramref name="value" /> is <see langword="null" />, or is neither a <see cref="GridGutter" />, a string
    /// nor a number.
    /// </exception>
    /// <exception cref="FormatException">
    /// <paramref name="value" /> is a string that <see cref="GridGutter.Parse(string)" /> rejects.
    /// </exception>
    public override object ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
    {
        if (value == null)
        {
            throw new NotSupportedException("Cannot convert null to GridGutter.");
        }

        if (value is GridGutter gutter)
        {
            return gutter;
        }

        if (value is string str)
        {
            return GridGutter.Parse(str);
        }

        if (value is IConvertible convertible)
        {
            var number = convertible.ToDouble(culture ?? CultureInfo.InvariantCulture);
            return new GridGutter(new GridGutterInfo(number), new GridGutterInfo());
        }

        throw new NotSupportedException($"Cannot convert value '{value}' to GridGutter.");
    }

    /// <summary>
    /// Converts <paramref name="value" /> to text in a form that <see cref="GridGutter.Parse(string)" /> reads:
    /// the horizontal gutter, or the horizontal and the vertical gutter separated by a comma when the vertical
    /// gutter is not zero at every breakpoint.
    /// </summary>
    /// <returns>
    /// The text. It holds only the width of the <see cref="MediaBreakPoint.ExtraSmall" /> breakpoint of each part,
    /// so a part that varies per breakpoint is not preserved. It is an empty string when <paramref name="value" />
    /// is <see langword="null" /> or not a <see cref="GridGutter" />.
    /// </returns>
    /// <exception cref="NotSupportedException">
    /// <paramref name="destinationType" /> is not <see cref="string" />.
    /// </exception>
    public override object ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
    {
        if (destinationType != typeof(string))
        {
            throw new NotSupportedException($"Cannot convert GridGutter to {destinationType}.");
        }

        if (value is GridGutter gutter)
        {
            if (gutter.Vertical == new GridGutterInfo())
            {
                return FormattableString.Invariant($"{gutter.Horizontal.ExtraSmall:G17}");
            }

            return FormattableString.Invariant($"{gutter.Horizontal.ExtraSmall:G17},{gutter.Vertical.ExtraSmall:G17}");
        }

        return string.Empty;
    }
}
