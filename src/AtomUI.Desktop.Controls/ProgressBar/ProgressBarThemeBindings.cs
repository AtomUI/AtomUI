using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace AtomUI.Desktop.Controls;

internal static class ProgressBarThemeBindings
{
    public static readonly IMultiValueConverter OptionalLineBorderRadius = new LineBorderRadiusConverter();

    private sealed class LineBorderRadiusConverter : IMultiValueConverter
    {
        public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            // Keep the existing template capsule until the previously unwired Own is configured.
            return values.Count == 1 && values[0] is CornerRadius radius ? radius : new CornerRadius(1000);
        }
    }
}
