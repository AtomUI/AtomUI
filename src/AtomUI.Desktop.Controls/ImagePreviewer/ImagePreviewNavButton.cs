using Avalonia;
using Avalonia.Controls;

namespace AtomUI.Desktop.Controls;

internal class ImagePreviewNavButton : IconButton
{
    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);
        var size = Math.Max(e.NewSize.Height, e.NewSize.Width);
        SetCurrentValue(CornerRadiusProperty, new CornerRadius(size / 2));
    }
}