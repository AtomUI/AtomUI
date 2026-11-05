using System.Globalization;
using AtomUI.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

internal static class CompoundInputTypographyBindings
{
    public static readonly IMultiValueConverter LineHeight = new CompoundLineHeightConverter();
    public static readonly IMultiValueConverter PlaceholderForeground = new PlaceholderForegroundConverter();

    private sealed class PlaceholderForegroundConverter : IMultiValueConverter
    {
        public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            // DynamicResource lowers its priority inside a ControlTemplate. The one-input
            // MultiBinding keeps this parent-owned brush at the declared Local priority.
            return values.Count == 1 ? values[0] switch
            {
                IBrush brush => brush,
                Color color => new SolidColorBrush(color),
                _ => BindingOperations.DoNothing
            } : BindingOperations.DoNothing;
        }
    }

    private sealed class CompoundLineHeightConverter : IMultiValueConverter
    {
        public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            if (values.Count != 12 || values[0] is not FontFamily family || values[1] is not double size ||
                values[2] is not FontStyle style || values[3] is not FontWeight weight ||
                values[4] is not FontStretch stretch || values[5] is not CustomizableSizeType sizeType ||
                !double.IsFinite(size) || size <= 0)
            {
                return BindingOperations.DoNothing;
            }
            var group = sizeType switch
            {
                CustomizableSizeType.Small => 1,
                CustomizableSizeType.Large => 2,
                _ => 0
            };
            var offset = 6 + group * 2;
            if (values[offset + 1] is not double themed)
            {
                return BindingOperations.DoNothing;
            }
            // An explicit owner line-height remains authoritative. Only the default
            // grows to contain the actual typeface after font/standard-API changes.
            if (values[offset] is double)
            {
                return themed;
            }
            var metrics = new Typeface(family, style, weight, stretch).GlyphTypeface.Metrics;
            return Math.Max(themed, metrics.LineSpacing * size / metrics.DesignEmHeight);
        }
    }
}
