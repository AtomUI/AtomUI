using System.ComponentModel;
using System.Globalization;

namespace AtomUI.Desktop.Controls;

[TypeConverter(typeof(RowJustifyConverter))]
public enum RowJustify
{
    Start,
    Center,
    End,
    SpaceBetween,
    SpaceAround,
    SpaceEvenly
}

[TypeConverter(typeof(RowAlignConverter))]
public enum RowAlign
{
    Top,
    Middle,
    Bottom,
    Stretch
}

public class RowJustifyConverter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
    {
        return sourceType == typeof(string);
    }

    public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
    {
        return destinationType == typeof(string);
    }

    public override object ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
    {
        if (value is string text)
        {
            var normalized = text.Trim().ToLowerInvariant();
            return normalized switch
            {
                "start" or "flex-start" => RowJustify.Start,
                "center" => RowJustify.Center,
                "end" or "flex-end" => RowJustify.End,
                "space-between" => RowJustify.SpaceBetween,
                "space-around" => RowJustify.SpaceAround,
                "space-evenly" => RowJustify.SpaceEvenly,
                _ => Enum.Parse(typeof(RowJustify), text, ignoreCase: true)
            };
        }

        throw new NotSupportedException($"Cannot convert value '{value}' to RowJustify.");
    }

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

public class RowAlignConverter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
    {
        return sourceType == typeof(string);
    }

    public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
    {
        return destinationType == typeof(string);
    }

    public override object ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
    {
        if (value is string text)
        {
            var normalized = text.Trim().ToLowerInvariant();
            return normalized switch
            {
                "top" => RowAlign.Top,
                "middle" or "center" => RowAlign.Middle,
                "bottom" => RowAlign.Bottom,
                "stretch" => RowAlign.Stretch,
                _ => Enum.Parse(typeof(RowAlign), text, ignoreCase: true)
            };
        }

        throw new NotSupportedException($"Cannot convert value '{value}' to RowAlign.");
    }

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
