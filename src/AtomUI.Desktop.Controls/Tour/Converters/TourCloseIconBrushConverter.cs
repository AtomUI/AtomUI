using System.Globalization;
using AtomUI.Media;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls.Converters;

internal sealed class TourCloseIconBrushConverter : IMultiValueConverter
{
    private const double MinimumIconContrast = 3;

    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values is not [IBrush preferredBrush, _, _, _])
        {
            return AvaloniaProperty.UnsetValue;
        }

        if (values is not [ISolidColorBrush preferred, ISolidColorBrush button, ISolidColorBrush surface, ISolidColorBrush fallback])
        {
            return preferredBrush;
        }

        var buttonColor = GetColor(button);
        var surfaceColor = GetColor(surface);
        if (buttonColor.A != byte.MaxValue && surfaceColor.A != byte.MaxValue)
        {
            return preferred;
        }

        var background = ColorUtils.OnBackground(buttonColor, surfaceColor);
        var foreground = ColorUtils.OnBackground(GetColor(preferred), background);
        var contrast = ColorUtils.Readability(foreground, background);
        if (contrast >= MinimumIconContrast)
        {
            return preferred;
        }

        var fallbackColor = ColorUtils.OnBackground(GetColor(fallback), background);
        return ColorUtils.Readability(fallbackColor, background) > contrast ? fallback : preferred;
    }

    private static Color GetColor(ISolidColorBrush brush) => brush.Color.SetAlphaF(brush.Color.GetAlphaF() * brush.Opacity);
}
