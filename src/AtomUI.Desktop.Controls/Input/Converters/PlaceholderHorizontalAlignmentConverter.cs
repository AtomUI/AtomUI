using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Layout;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Horizontal alignment of the placeholder text element: a <see cref="HorizontalAlignment.Stretch" /> value of
/// HorizontalContentAlignment maps to <see cref="HorizontalAlignment.Left" />.
/// The placeholder text always renders from the start of the content alignment. Stretched over the whole content
/// area, the element's bounds, which are the measuring basis of the placeholder semantic part highlight, would be
/// wider than the text itself, unlike the inline placeholder of Ant Design that hugs its text.
/// </summary>
internal class PlaceholderHorizontalAlignmentConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is HorizontalAlignment alignment)
        {
            return alignment == HorizontalAlignment.Stretch
                ? HorizontalAlignment.Left
                : alignment;
        }

        return HorizontalAlignment.Left;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
