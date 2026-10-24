using AtomUI.Controls.Commons;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

internal static class TimelineIndicatorThemeBindings
{
    // XAML x:Static cannot resolve internal fields on the common primitive.
    // Expose their existing property objects here; bindings stay in ControlTheme.
    public static readonly AvaloniaProperty DotSizeProperty = TimelineIndicator.IndicatorDotSizeProperty;
    public static readonly AvaloniaProperty DotBorderWidthProperty = TimelineIndicator.IndicatorDotBorderWidthProperty;
    public static readonly AvaloniaProperty DefaultColorProperty = TimelineIndicator.DefaultIndicatorColorProperty;

    public static readonly FuncValueConverter<double, Thickness> DotBorderThickness =
        new(value => new Thickness(value));

    public static readonly FuncMultiValueConverter<object, CornerRadius> DotCornerRadius =
        new(values => new CornerRadius(values.OfType<double>().Sum() / 2));

    public static readonly FuncMultiValueConverter<object, IBrush?> IndicatorBrush =
        new(values => values.OfType<IBrush>().FirstOrDefault());
}
