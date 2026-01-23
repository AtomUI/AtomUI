using System.Globalization;
using Avalonia.Data.Converters;

namespace AtomUI.Desktop.Controls.Converters;

internal class CascaderViewItemExpanderIsVisibleConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count != 2)
        {
            return false;
        }
        var isLeaf    = values[0] as bool?;
        var isLoading = values[1] as bool?;
        return isLeaf == false && isLoading == false;
    }
}