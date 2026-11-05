using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace AtomUI.Desktop.Controls;

// Preserve actual historical visuals when an Own is absent; each binding also
// supplies the item's real state, so configured values affect only its variant.
internal static class StepsThemeConverters
{
    public static readonly IMultiValueConverter NavigationSpacing = new ConfiguredScalarConverter(
        static value => value is StepsType.Navigation);
    public static readonly IMultiValueConverter DotThickness = new ConfiguredScalarConverter(
        static value => value is StepsType.Dot or StepsType.OutlineDot);
    public static readonly IMultiValueConverter SmallArrowWidth = new ConfiguredScalarConverter(
        static value => value is SizeType.Small);

    private sealed class ConfiguredScalarConverter(Func<object?, bool> applies) : IMultiValueConverter
    {
        public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            if (values.Count == 3 && applies(values[0]) && values[1] is double configured)
            {
                return configured;
            }
            return values.Count > 2 && values[2] is double fallback ? fallback : AvaloniaProperty.UnsetValue;
        }
    }
}
