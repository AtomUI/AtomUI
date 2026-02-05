using System.ComponentModel;
using System.Globalization;
using AtomUI.Controls;

namespace AtomUI.Desktop.Controls;

[TypeConverter(typeof(GridGutterConverter))]
public record GridGutter
{
    public GridGutterInfo Horizontal { get; init; }
    public GridGutterInfo Vertical { get; init; }

    public GridGutter()
        : this(new GridGutterInfo(), new GridGutterInfo())
    {
    }

    public GridGutter(GridGutterInfo horizontal, GridGutterInfo vertical)
    {
        Horizontal = horizontal;
        Vertical   = vertical;
    }

    public static GridGutter Parse(string input)
    {
        var trimmed = input.Trim();
        if (trimmed.Length == 0)
        {
            throw new FormatException("Gutter value cannot be empty.");
        }

        if (trimmed.Contains(';', StringComparison.Ordinal))
        {
            var parts = trimmed.Split(';', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2)
            {
                throw new FormatException("Gutter must have exactly two segments when using ';' separator.");
            }

            var horizontal = GridGutterInfo.Parse(parts[0].Trim());
            var vertical   = GridGutterInfo.Parse(parts[1].Trim());
            return new GridGutter(horizontal, vertical);
        }

        if (trimmed.Contains(':', StringComparison.Ordinal))
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

    private static bool TryParsePair(string input, out double horizontal, out double vertical)
    {
        horizontal = 0;
        vertical   = 0;

        var parts = input.Split(',', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2)
        {
            return false;
        }

        if (!double.TryParse(parts[0].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out horizontal))
        {
            return false;
        }

        if (!double.TryParse(parts[1].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out vertical))
        {
            return false;
        }

        if (horizontal < 0 || vertical < 0)
        {
            throw new FormatException("Gutter values must be >= 0.");
        }

        return true;
    }
}

public class GridGutterConverter : TypeConverter
{
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

    public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
    {
        return destinationType == typeof(string);
    }

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
