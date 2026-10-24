using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace AtomUI.Desktop.Controls;

internal static class RangeTimePickerThemeBindings
{
    public static readonly IMultiValueConverter OptionalGeometry = new GeometryOverrideConverter();

    private sealed class GeometryOverrideConverter : IMultiValueConverter
    {
        public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            if (values.Count != 3)
            {
                return AvaloniaProperty.UnsetValue;
            }
            if (values[1] is Thickness && values[2] is Thickness margin)
            {
                return margin;
            }
            if (values[1] is double && values[2] is double scalar)
            {
                return scalar;
            }
            return values[0] ?? AvaloniaProperty.UnsetValue;
        }
    }
}
