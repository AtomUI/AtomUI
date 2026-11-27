using Avalonia;
using Avalonia.Media;

namespace AtomUI.Controls;

/// <summary>
/// Supplies the corner radius, the bounds and the background of the area that a popup surface covers, for the
/// shadow of that surface to take its shape. <c>ShadowsAwareContainer</c> and <c>Popup</c> in
/// AtomUI.Desktop.Controls read only <see cref="IArrowAwareShadowMaskInfoProvider" /> from the popup content
/// and never call these three members. The tree view flyout presenter implements this interface directly;
/// other popup content implements <see cref="IArrowAwareShadowMaskInfoProvider" />.
/// </summary>
internal interface IShadowMaskInfoProvider
{
    CornerRadius GetMaskCornerRadius();
    Rect GetMaskBounds();
    IBrush? GetMaskBackground();
}

/// <summary>
/// A <see cref="IShadowMaskInfoProvider" /> for popup content that shows an arrow, such as a flyout presenter
/// or a tooltip. It adds whether the arrow is visible, its position and its bounds, lets the consumer set the
/// arrow opacity through <c>SetArrowOpacity</c>, and exposes the arrow decorated box of the template. It is
/// implemented by the arrow decorated box in AtomUI.Controls and by the menu flyout presenter, the tooltip and
/// the color picker popup root frame.
/// </summary>
internal interface IArrowAwareShadowMaskInfoProvider : IShadowMaskInfoProvider
{
    bool IsArrowVisible();
    ArrowPosition GetArrowPosition();
    Rect GetArrowIndicatorBounds();
    Rect GetArrowIndicatorLayoutBounds();
    void SetArrowOpacity(double opacity);

    /// <summary>
    /// Returns the arrow decorated box of the template, or <see langword="null" /> while the template of the
    /// provider is not applied yet or the current theme does not provide the
    /// <see cref="AbstractArrowDecoratedBox.ArrowDecoratorPart" /> part. Consumers must then fall back to the
    /// no-arrow state and query again once the part is ready.
    /// </summary>
    AbstractArrowDecoratedBox? GetArrowDecoratedBox();
}