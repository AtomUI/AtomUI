using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace AtomUI.Desktop.Controls;

internal sealed class UploadThumbnailCornerRadiusConverter : IMultiValueConverter
{
    public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count != 3 || values[0] is not CornerRadius radius ||
            values[1] is not Thickness padding || values[2] is not Thickness borderThickness)
        {
            return default(CornerRadius);
        }

        var inset = padding + borderThickness;
        return new CornerRadius(
            Math.Max(0, radius.TopLeft - Math.Max(inset.Left, inset.Top)),
            Math.Max(0, radius.TopRight - Math.Max(inset.Right, inset.Top)),
            Math.Max(0, radius.BottomRight - Math.Max(inset.Right, inset.Bottom)),
            Math.Max(0, radius.BottomLeft - Math.Max(inset.Left, inset.Bottom)));
    }
}
