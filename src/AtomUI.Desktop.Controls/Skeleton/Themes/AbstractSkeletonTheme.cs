using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Styling;

namespace AtomUI.Desktop.Controls.Themes;

internal class AbstractSkeletonTheme : ControlTheme
{
    public static readonly IMultiValueConverter LineHeight = new LineHeightConverter();

    private sealed class LineHeightConverter : IMultiValueConverter
    {
        public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            // SkeletonTitle deliberately retains the SkeletonLine StyleKey, so a
            // type selector cannot distinguish the two owners in their shared theme.
            return values.Count == 3 ? values[values[0] is SkeletonTitle ? 1 : 2]!
                : AvaloniaProperty.UnsetValue;
        }
    }
}
