using Avalonia;
using Avalonia.Data.Converters;

namespace AtomUI.Desktop.Controls;

internal static class UploadShapeConverters
{
    // Circle is a geometric variant; resource updates must not replace its measured radius.
    public static readonly FuncValueConverter<Rect, CornerRadius> CircleCornerRadius =
        new(bounds => new CornerRadius(Math.Max(bounds.Width, bounds.Height)));
}
