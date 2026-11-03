using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace AtomUI.Controls;

internal static class AvatarThemeConverters
{
    public static readonly IValueConverter TextPadding =
        new FuncValueConverter<double, Thickness>(gap => new Thickness(gap, 0));

    public static readonly IValueConverter CircleRadius =
        new FuncValueConverter<double, CornerRadius>(width => new CornerRadius(width / 2));

    public static readonly IMultiValueConverter CustomIconSize = new CustomIconSizeConverter();

    private sealed class CustomIconSizeConverter : IMultiValueConverter
    {
        public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            if (values.Count != 3 || values[0] is not double size)
            {
                return AvaloniaProperty.UnsetValue;
            }
            return values[1] is not null || values[2] is not null ? size / 2 : 18d;
        }
    }
}
