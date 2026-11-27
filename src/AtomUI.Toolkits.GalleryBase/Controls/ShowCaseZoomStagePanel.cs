using Avalonia;
using Avalonia.Controls;

namespace AtomUI.Toolkits.GalleryBase.Controls;

/// <summary>
/// Lays out the single child of the zoom stage. <see cref="ShowCaseZoomContentLayout.Natural" /> measures
/// the child with the width of the stage and centers it at its desired size;
/// <see cref="ShowCaseZoomContentLayout.Stretch" /> measures it with the finite slot of the stage and gives it
/// that slot to arrange in. The child's own Width, Height, Min/Max and Margin are still applied by the child's
/// own arrange. The panel never stretches the child or scales it with a RenderTransform: filling the stage is
/// a reflow of the content, not an external transform.
/// </summary>
public class ShowCaseZoomStagePanel : Panel
{
    /// <summary>
    /// Identifies the <see cref="ZoomContentLayout" /> property.
    /// </summary>
    public static readonly StyledProperty<ShowCaseZoomContentLayout> ZoomContentLayoutProperty =
        ShowCaseItem.ZoomContentLayoutProperty.AddOwner<ShowCaseZoomStagePanel>();

    /// <summary>
    /// Chooses how the single child of the panel fills the zoom stage. With
    /// <see cref="ShowCaseZoomContentLayout.Natural" /> the child keeps its desired size and is centered; with
    /// <see cref="ShowCaseZoomContentLayout.Stretch" /> it gets the whole slot of the stage. The zoom overlay
    /// template forwards the <see cref="ShowCaseItem.ZoomContentLayout" /> value of the showcased item to it.
    /// </summary>
    public ShowCaseZoomContentLayout ZoomContentLayout
    {
        get => GetValue(ZoomContentLayoutProperty);
        set => SetValue(ZoomContentLayoutProperty, value);
    }

    static ShowCaseZoomStagePanel()
    {
        AffectsMeasure<ShowCaseZoomStagePanel>(ZoomContentLayoutProperty);
    }

    /// <summary>
    /// Measures the child according to <see cref="ZoomContentLayout" />. It returns an empty size when there is no
    /// child or when an axis of <paramref name="availableSize" /> is not positive.
    /// </summary>
    /// <param name="availableSize">
    /// The slot that the stage offers to the panel.
    /// </param>
    /// <returns>
    /// The offered size on each finite axis and the desired size of the child on each infinite axis.
    /// </returns>
    protected override Size MeasureOverride(Size availableSize)
    {
        if (Children.Count == 0 || availableSize.Width <= 0 || availableSize.Height <= 0)
        {
            return new Size();
        }

        MeasureContent(availableSize);
        var desired = Children[0].DesiredSize;
        return new Size(
            double.IsFinite(availableSize.Width) ? availableSize.Width : desired.Width,
            double.IsFinite(availableSize.Height) ? availableSize.Height : desired.Height);
    }

    /// <summary>
    /// Measures the child again with the final slot and arranges it centered.
    /// <see cref="ShowCaseZoomContentLayout.Stretch" /> gives the child the whole slot; every other layout gives it
    /// its desired size clipped to the slot. Nothing is arranged when there is no child or when an axis of
    /// <paramref name="finalSize" /> is not positive.
    /// </summary>
    /// <param name="finalSize">
    /// The slot that the stage has given to the panel.
    /// </param>
    /// <returns>
    /// <paramref name="finalSize" />, unchanged.
    /// </returns>
    protected override Size ArrangeOverride(Size finalSize)
    {
        if (Children.Count > 0 && finalSize.Width > 0 && finalSize.Height > 0)
        {
            var child = Children[0];
            MeasureContent(finalSize);
            var desired = child.DesiredSize;

            // The layout policy is an explicit, observable contract. It is not inferred from the origin of the
            // ZoomContentLayout value or from the type of the child.
            var width = ZoomContentLayout == ShowCaseZoomContentLayout.Stretch
                ? finalSize.Width : Math.Min(desired.Width, finalSize.Width);
            var height = ZoomContentLayout == ShowCaseZoomContentLayout.Stretch
                ? finalSize.Height : Math.Min(desired.Height, finalSize.Height);
            var x      = (finalSize.Width - width) / 2;
            var y      = (finalSize.Height - height) / 2;

            child.Arrange(new Rect(x, y, width, height));
        }

        return finalSize;
    }

    private void MeasureContent(Size availableSize)
    {
        Children[0].Measure(ZoomContentLayout == ShowCaseZoomContentLayout.Stretch
            ? availableSize
            : new Size(availableSize.Width, double.PositiveInfinity));
    }
}
