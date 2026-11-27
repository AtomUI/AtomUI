using System.ComponentModel;
using System.Globalization;

namespace AtomUI.Controls;

/// <summary>
/// A horizontal and a vertical spacing, each of which applies to every breakpoint or is configured per
/// breakpoint.
/// </summary>
/// <remarks>
/// The vertical axis can be left without a value. It then uses the fallback of that axis whenever the gutter is
/// resolved.
/// </remarks>
[TypeConverter(typeof(ResponsiveGutterConverter))]
public readonly record struct ResponsiveGutter
{
    private readonly bool _hasExplicitHorizontal;
    private readonly bool _hasExplicitVertical;

    /// <summary>
    /// Initializes a gutter with both axes set to zero, which replaces the fallback when the gutter is resolved.
    /// </summary>
    public ResponsiveGutter()
        : this(new ResponsiveDouble(0), new ResponsiveDouble(0))
    {
    }

    /// <summary>
    /// Initializes a gutter with a value for both axes.
    /// </summary>
    /// <param name="horizontal">
    /// The spacing between items that lie side by side.
    /// </param>
    /// <param name="vertical">
    /// The spacing between items that lie one above the other.
    /// </param>
    public ResponsiveGutter(ResponsiveDouble horizontal, ResponsiveDouble vertical)
        : this(horizontal, vertical, true, true)
    {
    }

    private ResponsiveGutter(ResponsiveDouble horizontal,
                             ResponsiveDouble vertical,
                             bool hasExplicitHorizontal,
                             bool hasExplicitVertical)
    {
        Horizontal             = horizontal;
        Vertical               = vertical;
        _hasExplicitHorizontal = hasExplicitHorizontal;
        _hasExplicitVertical   = hasExplicitVertical;
    }

    /// <summary>
    /// Gets the spacing between items that lie side by side.
    /// </summary>
    public ResponsiveDouble Horizontal { get; }

    /// <summary>
    /// Gets the spacing between items that lie one above the other.
    /// </summary>
    public ResponsiveDouble Vertical { get; }

    /// <summary>
    /// Parses a gutter in one of four forms: two responsive numbers separated by a semicolon, one per axis; a
    /// responsive number for the horizontal axis only; two plain numbers separated by a comma, one per axis; or one
    /// number for both axes.
    /// </summary>
    /// <param name="input">
    /// The text to parse, for example <c>xs: 8, md: 16; xs: 4, xl: 24</c>, <c>xs: 8, md: 16</c>, <c>16, 24</c> or
    /// <c>16</c>. Each responsive number has a form that <see cref="ResponsiveDouble.Parse" /> accepts.
    /// </param>
    /// <returns>
    /// A gutter with both axes set. For the form that sets the horizontal axis only, the vertical axis has no value.
    /// </returns>
    /// <exception cref="FormatException">
    /// The input is empty, a semicolon does not separate exactly two non-empty parts, or a number is negative or not
    /// well formed.
    /// </exception>
    public static ResponsiveGutter Parse(string input)
    {
        var trimmed = input.AsSpan().Trim();
        if (trimmed.IsEmpty)
        {
            throw new FormatException("Responsive gutter value cannot be empty.");
        }

        if (trimmed.IndexOf(';') >= 0)
        {
            if (!TrySplitTwoNonEmptySegments(trimmed, ';', out var horizontal, out var vertical))
            {
                throw new FormatException("Responsive gutter must have exactly two segments when using ';' separator.");
            }

            return new ResponsiveGutter(ResponsiveDouble.Parse(horizontal.ToString()),
                ResponsiveDouble.Parse(vertical.ToString()));
        }

        if (trimmed.IndexOf(':') >= 0)
        {
            return new ResponsiveGutter(ResponsiveDouble.Parse(trimmed.ToString()), new ResponsiveDouble(0), true, false);
        }

        if (TryParsePair(trimmed, out var horizontalValue, out var verticalValue))
        {
            return new ResponsiveGutter(new ResponsiveDouble(horizontalValue), new ResponsiveDouble(verticalValue));
        }

        var scalar = ResponsiveDouble.Parse(trimmed.ToString());
        return new ResponsiveGutter(scalar, scalar);
    }

    /// <summary>
    /// Gets the spacing that applies to <paramref name="breakPoint" />, for each axis.
    /// </summary>
    /// <returns>
    /// The horizontal and the vertical spacing. An axis that has no value, or whose configured breakpoints are
    /// all larger than <paramref name="breakPoint" />, uses the part of <paramref name="fallback" /> for that
    /// axis.
    /// </returns>
    public (double Horizontal, double Vertical) Resolve(MediaBreakPoint breakPoint, (double Horizontal, double Vertical) fallback)
    {
        return (
            _hasExplicitHorizontal ? Horizontal.Resolve(breakPoint, fallback.Horizontal) : fallback.Horizontal,
            _hasExplicitVertical ? Vertical.Resolve(breakPoint, fallback.Vertical) : fallback.Vertical);
    }

    /// <summary>
    /// Determines whether the gutter has a value for <paramref name="breakPoint" />, without using a fallback.
    /// </summary>
    /// <param name="breakPoint">
    /// The breakpoint to resolve.
    /// </param>
    /// <param name="fallback">
    /// The spacing to use for each axis that has no value for <paramref name="breakPoint" />.
    /// </param>
    /// <param name="value">
    /// On success, the spacing for <paramref name="breakPoint" />, with <paramref name="fallback" /> for each axis
    /// that has none; otherwise <paramref name="fallback" />.
    /// </param>
    /// <returns>
    /// <see langword="true" /> when at least one axis has a value for <paramref name="breakPoint" />;
    /// otherwise <see langword="false" />.
    /// </returns>
    public bool TryResolve(MediaBreakPoint breakPoint,
                           (double Horizontal, double Vertical) fallback,
                           out (double Horizontal, double Vertical) value)
    {
        var horizontal = fallback.Horizontal;
        var vertical   = fallback.Vertical;
        var horizontalHit = _hasExplicitHorizontal && Horizontal.TryResolve(breakPoint, out horizontal);
        var verticalHit   = _hasExplicitVertical && Vertical.TryResolve(breakPoint, out vertical);
        if (!horizontalHit && !verticalHit)
        {
            value = fallback;
            return false;
        }

        value = (
            horizontalHit ? horizontal : fallback.Horizontal,
            verticalHit ? vertical : fallback.Vertical);
        return true;
    }

    private static bool TryParsePair(ReadOnlySpan<char> input, out double horizontal, out double vertical)
    {
        horizontal = 0;
        vertical = 0;

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
            throw new FormatException("Responsive gutter values must be >= 0.");
        }

        return true;
    }

    private static bool TrySplitTwoNonEmptySegments(ReadOnlySpan<char> input,
                                                    char separator,
                                                    out ReadOnlySpan<char> first,
                                                    out ReadOnlySpan<char> second)
    {
        first = default;
        second = default;
        var foundCount = 0;

        while (!input.IsEmpty)
        {
            var separatorIndex = input.IndexOf(separator);
            var segment = separatorIndex >= 0 ? input[..separatorIndex].Trim() : input.Trim();
            input = separatorIndex >= 0 ? input[(separatorIndex + 1)..] : ReadOnlySpan<char>.Empty;

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
/// Converts strings and numbers to <see cref="ResponsiveGutter" /> for XAML.
/// </summary>
public class ResponsiveGutterConverter : TypeConverter
{
    /// <summary>
    /// Returns whether a value of the type <paramref name="sourceType" /> can be converted to a
    /// <see cref="ResponsiveGutter" />.
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
    /// Converts a string or a number to a <see cref="ResponsiveGutter" />.
    /// </summary>
    /// <param name="context">
    /// Ignored.
    /// </param>
    /// <param name="culture">
    /// The culture that converts a numeric value. <see langword="null" /> uses the invariant culture.
    /// </param>
    /// <param name="value">
    /// A <see cref="ResponsiveGutter" />, which is returned as it is, a string in the format of
    /// <see cref="ResponsiveGutter.Parse" />, or a value that implements <see cref="IConvertible" />, which is
    /// used for both axes.
    /// </param>
    /// <returns>
    /// The converted value.
    /// </returns>
    /// <exception cref="FormatException">
    /// The string is not well formed or holds a negative number.
    /// </exception>
    /// <exception cref="NotSupportedException">
    /// <paramref name="value" /> is of another type.
    /// </exception>
    public override object ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
    {
        if (value is ResponsiveGutter gutter)
        {
            return gutter;
        }
        if (value is string text)
        {
            return ResponsiveGutter.Parse(text);
        }
        if (value is IConvertible convertible)
        {
            var scalar = new ResponsiveDouble(convertible.ToDouble(culture ?? CultureInfo.InvariantCulture));
            return new ResponsiveGutter(scalar, scalar);
        }

        throw new NotSupportedException($"Cannot convert value '{value}' to ResponsiveGutter.");
    }
}
