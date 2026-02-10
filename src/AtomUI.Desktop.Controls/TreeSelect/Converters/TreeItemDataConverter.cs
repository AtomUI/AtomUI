using System.Globalization;
using Avalonia.Data.Converters;

namespace AtomUI.Desktop.Controls.Converters;

internal class TreeItemDataConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is ITreeItemData data)
        {
            return data.Header;
        }
        return value;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}