using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace AtomUI.Controls.Converters;

public class CornerRadiusFilterConverter : IValueConverter
{
    public bool TopLeft { get; set; } = true;

    public bool TopRight { get; set; } = true;

    public bool BottomLeft { get; set; } = true;

    public bool BottomRight { get; set; } = true;
    
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is CornerRadius cornerRadius)
        {
            return new CornerRadius(
                TopLeft ? cornerRadius.TopLeft : 0,
                TopRight? cornerRadius.TopRight : 0, 
                BottomRight ? cornerRadius.BottomRight : 0, 
                BottomLeft ? cornerRadius.BottomLeft : 0);
        }
        return new CornerRadius(0);
    }
    
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}