using System.Globalization;
using Avalonia;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace AtomUI.Desktop.Controls.Converters;

internal sealed class SelectTagFramePaddingConverter : IMultiValueConverter
{
    public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values is not [Thickness padding, double tagHeight, double textLineHeight])
        {
            return BindingOperations.DoNothing;
        }

        // The preset padding reserves space for a text line. Let a taller tag use that same space.
        var verticalPadding = padding.Top + padding.Bottom;
        var extraHeight = tagHeight - textLineHeight;
        if (!double.IsFinite(extraHeight) || extraHeight <= 0 || verticalPadding <= 0)
        {
            return padding;
        }

        var scale = Math.Max(0, verticalPadding - extraHeight) / verticalPadding;
        return new Thickness(padding.Left, padding.Top * scale, padding.Right, padding.Bottom * scale);
    }
}
