using AtomUI.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

// Host of the popup.root semantic part. It is the direct Child of the Popup and carries the outer edge of the
// popup, and it forwards the arrow and shadow mask contract to the ArrowDecoratedBox in the content layer.
// The custom placement of the Popup and ShadowsAwareContainer read IArrowAwareShadowMaskInfoProvider from
// the direct Child only.
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
