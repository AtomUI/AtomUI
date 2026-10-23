using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

internal static class OptionButtonThemeConverters
{
    public static readonly IMultiValueConverter OptionalBackground = new OptionalBackgroundConverter();

    private sealed class OptionalBackgroundConverter : IMultiValueConverter
    {
        public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            IBrush result = Brushes.Transparent;
            for (var i = 0; i + 1 < values.Count; i += 2)
            {
                if (values[i] is IBrush or Color)
                {
                    result = values[i + 1] switch
                    {
                        IBrush brush => brush,
                        Color color => new SolidColorBrush(color),
                        _ => result
                    };
                }
            }
            return result;
        }
    }
}
