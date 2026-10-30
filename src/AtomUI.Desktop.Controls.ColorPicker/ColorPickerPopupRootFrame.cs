using AtomUI.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

// popup.root 语义部件宿主：直接作为 Popup 的 Child 承载弹层外沿，并向内容层的
// ArrowDecoratedBox 转发箭头/阴影遮罩契约。Popup 自定义定位与 ShadowsAwareContainer
// 都只从直接 Child 读取 IArrowAwareShadowMaskInfoProvider。
internal sealed class ColorPickerPopupRootFrame : Border, IArrowAwareShadowMaskInfoProvider
{
    private ArrowDecoratedBox? _arrowBox;

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ChildProperty)
        {
            _arrowBox = change.NewValue as ArrowDecoratedBox;
        }
    }

    CornerRadius IShadowMaskInfoProvider.GetMaskCornerRadius()
        => _arrowBox?.GetMaskCornerRadius() ?? default;

    Rect IShadowMaskInfoProvider.GetMaskBounds()
        => _arrowBox?.GetMaskBounds() ?? default;

    IBrush? IShadowMaskInfoProvider.GetMaskBackground()
        => _arrowBox?.GetMaskBackground();

    ArrowPosition IArrowAwareShadowMaskInfoProvider.GetArrowPosition()
        => _arrowBox?.ArrowPosition ?? default;

    bool IArrowAwareShadowMaskInfoProvider.IsArrowVisible()
        => _arrowBox?.IsArrowVisible ?? false;

    void IArrowAwareShadowMaskInfoProvider.SetArrowOpacity(double opacity)
        => ((IArrowAwareShadowMaskInfoProvider?)_arrowBox)?.SetArrowOpacity(opacity);

    Rect IArrowAwareShadowMaskInfoProvider.GetArrowIndicatorBounds()
        => _arrowBox?.ArrowIndicatorBounds ?? default;

    Rect IArrowAwareShadowMaskInfoProvider.GetArrowIndicatorLayoutBounds()
        => _arrowBox?.ArrowIndicatorLayoutBounds ?? default;

    AbstractArrowDecoratedBox IArrowAwareShadowMaskInfoProvider.GetArrowDecoratedBox()
        => _arrowBox!;
}
