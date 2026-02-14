using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace AtomUI.Desktop.Controls.Converters;

internal class MarginMultiplierConverter : IMultiValueConverter
{
    public bool Left { get; set; } = false;

    public bool Top { get; set; } = false;

    public bool Right { get; set; } = false;

    public bool Bottom { get; set; } = false;

    public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count < 2 || values[0] == AvaloniaProperty.UnsetValue || values[1] == AvaloniaProperty.UnsetValue)
        {
            return new Thickness(0);
        }
        var level  = System.Convert.ToInt32(values[0]);
        var indent = System.Convert.ToDouble(values[1]);
        return new Thickness(
            Left ? indent * level : 0,
            Top ? indent * level : 0,
            Right ? indent * level : 0,
            Bottom ? indent * level : 0);
    }
}