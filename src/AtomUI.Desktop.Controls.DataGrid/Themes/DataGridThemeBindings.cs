using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace AtomUI.Desktop.Controls.Themes;

// Optional Own projections preserve the historical baseline until explicitly configured.
internal static class DataGridThemeBindings
{
    public static readonly IMultiValueConverter OptionalValue = new OptionalValueConverter();

    private sealed class OptionalValueConverter : IMultiValueConverter
    {
        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            return values.Count == 2 && values[1] != AvaloniaProperty.UnsetValue && values[1] is not null
                ? values[1] : values.Count > 0 ? values[0] : AvaloniaProperty.UnsetValue;
        }
    }

}
