using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

internal static class TimeViewCellThemeBindings
{
    public static readonly IMultiValueConverter HasBrush = new BrushPresenceConverter();

    private sealed class BrushPresenceConverter : IMultiValueConverter
    {
        public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture) =>
            values.Count == 1 && values[0] is IBrush;
    }
}
