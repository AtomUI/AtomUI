using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace AtomUI.Toolkits.GalleryBase.Controls;

internal sealed class SemanticPartPreviewBreakpointConverter : IMultiValueConverter
{
    public const double OuterBreakpoint = 820;
    public static SemanticPartPreviewBreakpointConverter Instance { get; } = new();

    private SemanticPartPreviewBreakpointConverter()
    {
    }

    public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count != 2 || parameter is not double outerBreakpoint ||
            values[0] is not Thickness margin || values[1] is not Thickness border)
        {
            return AvaloniaProperty.UnsetValue;
        }
        // The host uses outer width; its panel receives width minus these
        // actual insets. Compare both widths in the same coordinate system.
        return outerBreakpoint - margin.Left - margin.Right - border.Left - border.Right;
    }
}
