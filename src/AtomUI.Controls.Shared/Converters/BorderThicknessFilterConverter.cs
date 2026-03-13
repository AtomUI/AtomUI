using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace AtomUI.Controls.Converters;

internal class BorderThicknessFilterConverter : IValueConverter
{
    public bool Left { get; set; } = true;

    public bool Top { get; set; } = true;

    public bool Right { get; set; } = true;

    public bool Bottom { get; set; } = true;

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is Thickness thickness)
        {
            return new Thickness(
                Left ? thickness.Left : 0,
                Top ? thickness.Top : 0,
                Right ? thickness.Right : 0,
                Bottom ? thickness.Bottom : 0);
        }
        return new Thickness(0);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}