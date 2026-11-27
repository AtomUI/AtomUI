using System.ComponentModel;
using System.Globalization;

namespace AtomUI.Controls;

/// <summary>
/// How a <see cref="Row" /> spreads the columns of a line when they leave width unused.
/// </summary>
[TypeConverter(typeof(RowJustifyConverter))]
public enum RowJustify
{
    /// <summary>
    /// Packs the columns at the start of the line and leaves the unused width at its end.
    /// </summary>
    Start,
    /// <summary>
    /// Centers the columns in the line.
    /// </summary>
    Center,
    /// <summary>
    /// Packs the columns at the end of the line and leaves the unused width at its start.
    /// </summary>
    End,
    /// <summary>
    /// Keeps the first column at the start and the last at the end of the line, and spreads the unused width equally
    /// over the gaps between adjacent columns. A line with one column is treated like <see cref="Start" />.
    /// </summary>
    SpaceBetween,
    /// <summary>
    /// Gives every column the same amount of width on both of its sides, so a gap between two columns is twice as
    /// wide as the unused width at the ends of the line.
    /// </summary>
    SpaceAround,
    /// <summary>
    /// Splits the unused width into equally wide gaps, also before the first and after the last column.
    /// </summary>
    SpaceEvenly
}

/// <summary>
/// How a <see cref="Row" /> places the columns of a line in the height of that line.
/// </summary>
[TypeConverter(typeof(RowAlignConverter))]
public enum RowAlign
{
    /// <summary>
    /// Places every column at the top of the line, with the height of its content.
    /// </summary>
    Top,
    /// <summary>
    /// Centers every column in the height of the line, with the height of its content.
    /// </summary>
    Middle,
    /// <summary>
    /// Places every column at the bottom of the line, with the height of its content.
    /// </summary>
    Bottom,
    /// <summary>
    /// Gives every column the full height of its line.
    /// </summary>
    Stretch
}

/// <summary>
/// Converts between <see cref="RowJustify" /> and text. It reads the member names ignoring case, accepts
/// <c>flex-start</c> for <see cref="RowJustify.Start" /> and <c>flex-end</c> for <see cref="RowJustify.End" />,
/// and writes the lowercase names such as <c>space-between</c>.
/// </summary>
public class RowJustifyConverter : TypeConverter
{
    internal static RowJustify Parse(ReadOnlySpan<char> input)
    {
        var normalized = input.Trim();
        if (normalized.Equals("start".AsSpan(), StringComparison.OrdinalIgnoreCase) ||
            normalized.Equals("flex-start".AsSpan(), StringComparison.OrdinalIgnoreCase))
        {
            return RowJustify.Start;
        }
        if (normalized.Equals("center".AsSpan(), StringComparison.OrdinalIgnoreCase))
        {
            return RowJustify.Center;
        }
        if (normalized.Equals("end".AsSpan(), StringComparison.OrdinalIgnoreCase) ||
            normalized.Equals("flex-end".AsSpan(), StringComparison.OrdinalIgnoreCase))
        {
            return RowJustify.End;
        }
        if (normalized.Equals("space-between".AsSpan(), StringComparison.OrdinalIgnoreCase))
        {
            return RowJustify.SpaceBetween;
        }
        if (normalized.Equals("space-around".AsSpan(), StringComparison.OrdinalIgnoreCase))
        {
            return RowJustify.SpaceAround;
        }
        if (normalized.Equals("space-evenly".AsSpan(), StringComparison.OrdinalIgnoreCase))
        {
            return RowJustify.SpaceEvenly;
        }
        return Enum.Parse<RowJustify>(input.ToString(), ignoreCase: true);
    }

    /// <summary>
    /// Returns whether a value of <paramref name="sourceType" /> can be converted to <see cref="RowJustify" />,
    /// which is only the case for <see cref="string" />.
    /// </summary>
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
    {
        return sourceType == typeof(string);
    }

    /// <summary>
    /// Returns whether a <see cref="RowJustify" /> can be converted to <paramref name="destinationType" />, which is
    /// only the case for <see cref="string" />.
    /// </summary>
    public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
    {
        return destinationType == typeof(string);
    }

    /// <summary>
    /// Converts the name in <paramref name="value" /> to a <see cref="RowJustify" />, ignoring case and surrounding
    /// white space.
    /// </summary>
    /// <returns>
    /// The <see cref="RowJustify" /> that the name stands for.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="value" /> names no defined <see cref="RowJustify" /> value, and is not text that
    /// <see cref="Enum" /> parsing accepts, such as a number.
    /// </exception>
    /// <exception cref="NotSupportedException">
    /// <paramref name="value" /> is not a <see cref="string" />.
    /// </exception>
    public override object ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
    {
        if (value is string text)
        {
            return Parse(text.AsSpan());
        }

        throw new NotSupportedException($"Cannot convert value '{value}' to RowJustify.");
    }

    /// <summary>
    /// Converts a <see cref="RowJustify" /> to its lowercase name, such as <c>space-between</c>.
    /// </summary>
    /// <returns>
    /// The name, or an empty string when <paramref name="value" /> is <see langword="null" /> or not one of the
    /// defined <see cref="RowJustify" /> values.
    /// </returns>
    /// <exception cref="NotSupportedException">
    /// <paramref name="destinationType" /> is not <see cref="string" />.
    /// </exception>
    public override object ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
    {
        if (destinationType != typeof(string))
        {
            throw new NotSupportedException($"Cannot convert RowJustify to {destinationType}.");
        }

        if (value is RowJustify justify)
        {
            return justify switch
            {
                RowJustify.Start => "start",
                RowJustify.Center => "center",
                RowJustify.End => "end",
                RowJustify.SpaceBetween => "space-between",
                RowJustify.SpaceAround => "space-around",
                RowJustify.SpaceEvenly => "space-evenly",
                _ => string.Empty
            };
        }

        return string.Empty;
    }
}

/// <summary>
/// Converts between <see cref="RowAlign" /> and text. It reads the member names ignoring case, accepts
/// <c>center</c> for <see cref="RowAlign.Middle" />, and writes the lowercase names such as <c>middle</c>.
/// </summary>
public class RowAlignConverter : TypeConverter
{
    internal static RowAlign Parse(ReadOnlySpan<char> input)
    {
        var normalized = input.Trim();
        if (normalized.Equals("top".AsSpan(), StringComparison.OrdinalIgnoreCase))
        {
            return RowAlign.Top;
        }
        if (normalized.Equals("middle".AsSpan(), StringComparison.OrdinalIgnoreCase) ||
            normalized.Equals("center".AsSpan(), StringComparison.OrdinalIgnoreCase))
        {
            return RowAlign.Middle;
        }
        if (normalized.Equals("bottom".AsSpan(), StringComparison.OrdinalIgnoreCase))
        {
            return RowAlign.Bottom;
        }
        if (normalized.Equals("stretch".AsSpan(), StringComparison.OrdinalIgnoreCase))
        {
            return RowAlign.Stretch;
        }
        return Enum.Parse<RowAlign>(input.ToString(), ignoreCase: true);
    }

    /// <summary>
    /// Returns whether a value of <paramref name="sourceType" /> can be converted to <see cref="RowAlign" />, which
    /// is only the case for <see cref="string" />.
    /// </summary>
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
    {
        return sourceType == typeof(string);
    }

    /// <summary>
    /// Returns whether a <see cref="RowAlign" /> can be converted to <paramref name="destinationType" />, which is
    /// only the case for <see cref="string" />.
    /// </summary>
    public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
    {
        return destinationType == typeof(string);
    }

    /// <summary>
    /// Converts the name in <paramref name="value" /> to a <see cref="RowAlign" />, ignoring case and surrounding
    /// white space.
    /// </summary>
    /// <returns>
    /// The <see cref="RowAlign" /> that the name stands for.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="value" /> names no defined <see cref="RowAlign" /> value, and is not text that
    /// <see cref="Enum" /> parsing accepts, such as a number.
    /// </exception>
    /// <exception cref="NotSupportedException">
    /// <paramref name="value" /> is not a <see cref="string" />.
    /// </exception>
    public override object ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
    {
        if (value is string text)
        {
            return Parse(text.AsSpan());
        }

        throw new NotSupportedException($"Cannot convert value '{value}' to RowAlign.");
    }

    /// <summary>
    /// Converts a <see cref="RowAlign" /> to its lowercase name, such as <c>middle</c>.
    /// </summary>
    /// <returns>
    /// The name, or an empty string when <paramref name="value" /> is <see langword="null" /> or not one of the
    /// defined <see cref="RowAlign" /> values.
    /// </returns>
    /// <exception cref="NotSupportedException">
    /// <paramref name="destinationType" /> is not <see cref="string" />.
    /// </exception>
    public override object ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
    {
        if (destinationType != typeof(string))
        {
            throw new NotSupportedException($"Cannot convert RowAlign to {destinationType}.");
        }

        if (value is RowAlign align)
        {
            return align switch
            {
                RowAlign.Top => "top",
                RowAlign.Middle => "middle",
                RowAlign.Bottom => "bottom",
                RowAlign.Stretch => "stretch",
                _ => string.Empty
            };
        }

        return string.Empty;
    }
}
