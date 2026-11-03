using AtomUI.Theme.Resources;
using Avalonia;
using Avalonia.Controls;

namespace AtomUI.Desktop.Controls;

internal class ImagePreviewNavButton : IconButton, IControlThemeGlobalOwnerSource
{
    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);
        var size = Math.Max(e.NewSize.Height, e.NewSize.Width);
        SetCurrentValue(CornerRadiusProperty, new CornerRadius(size / 2));
    }
    #region 实现 IControlThemeGlobalOwnerSource 接口

    Control? IControlThemeGlobalOwnerSource.ThemeGlobalOwner => ImagePreviewThemeOwner.Resolve(this);

    #endregion
}