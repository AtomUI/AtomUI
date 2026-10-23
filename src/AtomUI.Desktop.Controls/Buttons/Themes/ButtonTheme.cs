using System.Globalization;
using AtomUI.Controls.Converters;
using Avalonia;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Markup.Xaml.MarkupExtensions.CompiledBindings;
using Avalonia.Media;
using Avalonia.Styling;

namespace AtomUI.Desktop.Controls.Themes;

public class ButtonTheme : ControlTheme
{
    public static readonly ButtonIconVisibleConverter IconVisibleConverter = new();
    public static readonly IReadOnlyList<double> DashedStyle = [4, 2];

    public static readonly BindingBase InheritedFontWeightBinding = new CompiledBindingExtension(
        new CompiledBindingPathBuilder().Self()
            .Property(Button.InheritedFontWeightProperty, PropertyInfoAccessorFactory.CreateAvaloniaPropertyAccessor).Build());

    public static readonly IMultiValueConverter FontWeightConverter = new OptionalFontWeightConverter();

    private sealed class OptionalFontWeightConverter : IMultiValueConverter
    {
        public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            var inherited = values.Count > 0 && values[0] is FontWeight weight ? weight : FontWeight.Normal;
            if (values.Count != 3 || values[1] is not double)
            {
                return inherited;
            }
            if (values[2] is FontWeight typed)
            {
                return typed;
            }
            if (values[2] is double configured && double.IsFinite(configured) && configured >= 1 &&
                configured <= 1000 && Math.Truncate(configured) == configured)
            {
                return (FontWeight)(int)configured;
            }
            return new BindingNotification(new InvalidOperationException("Button FontWeight requires an integer from 1 to 1000."),
                BindingErrorType.Error, inherited);
        }
    }
}

internal static class ButtonThemeConverters
{
    public static readonly IMultiValueConverter OptionalDisabledBorder = new OptionalDisabledBorderConverter();

    private sealed class OptionalDisabledBorderConverter : IMultiValueConverter
    {
        public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            var value = values.Count == 3 && values[1] is Color or IBrush ? values[2] : values.FirstOrDefault();
            return value switch
            {
                IBrush brush => brush,
                Color color => new SolidColorBrush(color),
                _ => AvaloniaProperty.UnsetValue
            };
        }
    }
}
