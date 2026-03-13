using System.Globalization;
using Avalonia.Controls;
using Avalonia.Data.Converters;

namespace AtomUI.Controls.Converters;

internal class DoubleToGridLengthConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double dvalue)
        {
            return new GridLength(dvalue);
        }
        return value;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}