using System.ComponentModel;
using System.Globalization;

namespace AtomUI.Controls;

/// <summary>
/// The flex setting of a grid column: how the column shares the free width or the overflow of its line.
/// </summary>
/// <remarks>
/// The starting width of the column is <see cref="Basis" /> when it has a value, otherwise the width that the
/// column asks for. Nothing is shared while the row is measured with an infinite width.
/// </remarks>
[TypeConverter(typeof(GridColFlexConverter))]
public readonly record struct GridColFlex
{
    /// <summary>
    /// Initializes a flex setting with <see cref="Grow" /> and <see cref="Shrink" /> set to <paramref name="grow" />
    /// and no <see cref="Basis" />.
    /// </summary>
    /// <param name="grow">
    /// The weight of the column for sharing the free width and the overflow of its line. It must be <c>0</c> or a
    /// positive finite number.
    /// </param>
    /// <exception cref="FormatException">
    /// <paramref name="grow" /> is negative, <see cref="double.NaN" /> or infinite.
    /// </exception>
    public GridColFlex(double grow)
        : this(grow, grow, null)
    {
    }

    private GridColFlex(double grow, double shrink, double? basis)
    {
        ValidateFlexValue(grow, nameof(grow));
        ValidateFlexValue(shrink, nameof(shrink));
        if (basis.HasValue)
        {
            ValidateBasis(basis.Value);
        }

        Grow   = grow;
        Shrink = shrink;
        Basis  = basis;
    }

    /// <summary>
    /// The weight that decides how much of the free width of the line is added to the starting width of the
    /// column: the free width multiplied by this value, divided by the sum of the <see cref="Grow" /> values of
    /// the columns of the line.
    /// </summary>
    public double Grow { get; }
    /// <summary>
    /// The weight that decides how much of the overflow of the line is taken from the starting width of the
    /// column: the overflow multiplied by this value and by the starting width, divided by the sum of the same
    /// product over the columns of the line. The width of the column never drops below <c>0</c>.
    /// </summary>
    public double Shrink { get; }
    /// <summary>
    /// The starting width of the column in device-independent pixels, or <see langword="null" /> to use the width
    /// that the column asks for. When this has a value, <see cref="Grow" /> and <see cref="Shrink" /> are
    /// <c>0</c>, so the column keeps this width.
    /// </summary>
    public double? Basis { get; }

    /// <summary>
    /// A flex setting with <see cref="Grow" /> and <see cref="Shrink" /> set to <c>1</c> and no <see cref="Basis" />:
    /// the column takes a share of the free width of its line and gives up a share of its overflow.
    /// </summary>
    public static GridColFlex Auto { get; } = new(1, 1, null);
    /// <summary>
    /// A flex setting with <see cref="Grow" /> and <see cref="Shrink" /> set to <c>0</c> and no <see cref="Basis" />:
    /// the column takes no free width and gives up no width when its line overflows.
    /// </summary>
    public static GridColFlex None { get; } = new(0, 0, null);

    /// <summary>
    /// Parses the text form of a flex setting.
    /// </summary>
    /// <param name="input">
    /// <c>auto</c>, <c>none</c>, a number such as <c>2</c>, or a number with the suffix <c>px</c> such as
    /// <c>100px</c>. Keywords and the suffix ignore case. Outer white space is ignored. Numbers use the invariant
    /// culture.
    /// </param>
    /// <returns>
    /// <see cref="Auto" />, <see cref="None" />, a setting with <see cref="Grow" /> and <see cref="Shrink" /> equal
    /// to the number, or a setting with <see cref="Basis" /> set to the number and with <see cref="Grow" /> and
    /// <see cref="Shrink" /> set to <c>0</c>.
    /// </returns>
    /// <exception cref="FormatException">
    /// The input is none of these forms, or a number is negative, <see cref="double.NaN" /> or infinite.
    /// </exception>
    public static GridColFlex Parse(string input)
    {
        var trimmed = input.AsSpan().Trim();
        if (trimmed.IsEmpty)
        {
            throw new FormatException("Grid column flex value cannot be empty.");
        }

        if (trimmed.Equals("auto".AsSpan(), StringComparison.OrdinalIgnoreCase))
        {
            return Auto;
        }

        if (trimmed.Equals("none".AsSpan(), StringComparison.OrdinalIgnoreCase))
        {
            return None;
        }

        if (trimmed.EndsWith("px".AsSpan(), StringComparison.OrdinalIgnoreCase))
        {
            var valueSpan = trimmed[..^2].Trim();
            if (!double.TryParse(valueSpan, NumberStyles.Float, CultureInfo.InvariantCulture, out var basis))
            {
                throw new FormatException($"Invalid flex basis value '{input}'.");
            }

            return new GridColFlex(0, 0, basis);
        }

        if (double.TryParse(trimmed, NumberStyles.Float, CultureInfo.InvariantCulture, out var grow))
        {
            return new GridColFlex(grow);
        }

        throw new FormatException($"Invalid grid column flex value '{input}'.");
    }

    /// <summary>
    /// Creates a flex setting with <see cref="Grow" /> and <see cref="Shrink" /> set to <paramref name="grow" />.
    /// </summary>
    public static implicit operator GridColFlex(double grow) => new(grow);
    /// <summary>
    /// Creates a flex setting with <see cref="Grow" /> and <see cref="Shrink" /> set to <paramref name="grow" />.
    /// </summary>
    public static implicit operator GridColFlex(int grow) => new(grow);

    private static void ValidateFlexValue(double value, string name)
    {
        if (double.IsNaN(value) || double.IsInfinity(value) || value < 0)
        {
            throw new FormatException($"Flex {name} must be >= 0, got {value}.");
        }
    }

    private static void ValidateBasis(double value)
    {
        if (double.IsNaN(value) || double.IsInfinity(value) || value < 0)
        {
            throw new FormatException($"Flex basis must be >= 0, got {value}.");
        }
    }
}

/// <summary>
/// Converts strings and numbers to a <see cref="GridColFlex" /> for XAML.
/// </summary>
public class GridColFlexConverter : TypeConverter
{
    /// <summary>
    /// Returns whether a value of the type <paramref name="sourceType" /> can be converted to a
    /// <see cref="GridColFlex" />.
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
    /// Converts a string or a number to a <see cref="GridColFlex" />.
    /// </summary>
    /// <param name="context">
    /// Ignored.
    /// </param>
    /// <param name="culture">
    /// The culture that converts a numeric value. <see langword="null" /> uses the invariant culture.
    /// </param>
    /// <param name="value">
    /// A <see cref="GridColFlex" />, which is returned as it is, a string in the format of
    /// <see cref="GridColFlex.Parse" />, or a value that implements <see cref="IConvertible" />, which is used as
    /// <see cref="GridColFlex.Grow" /> and <see cref="GridColFlex.Shrink" />.
    /// </param>
    /// <returns>
    /// The converted value.
    /// </returns>
    /// <exception cref="FormatException">
    /// The string is not well formed, or the number is negative, <see cref="double.NaN" /> or infinite.
    /// </exception>
    /// <exception cref="NotSupportedException">
    /// <paramref name="value" /> is <see langword="null" /> or of another type.
    /// </exception>
    public override object ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
    {
        if (value is GridColFlex flex)
        {
            return flex;
        }

        if (value is string text)
        {
            return GridColFlex.Parse(text);
        }

        if (value is IConvertible convertible)
        {
            return new GridColFlex(convertible.ToDouble(culture ?? CultureInfo.InvariantCulture));
        }

        throw new NotSupportedException($"Cannot convert value '{value}' to GridColFlex.");
    }
}
