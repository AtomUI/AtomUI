using Avalonia;
using Avalonia.Controls;

namespace AtomUI.Controls.Commons;

internal sealed class LineProgressPanel : Panel
{
    private Rect _grooveRect;

    protected override Size MeasureOverride(Size availableSize)
    {
        foreach (var child in Children)
        {
            child.Measure(availableSize);
        }

        return default;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        if (TemplatedParent is not AbstractGeneralProgressBar owner)
        {
            foreach (var child in Children)
            {
                child.Arrange(new Rect(finalSize));
            }

            return finalSize;
        }

        var grooveRect = owner.GetLineProgressBarRect(finalSize);
        _grooveRect = grooveRect;
        foreach (var child in Children)
        {
            switch (child.Name)
            {
                case AbstractProgressBar.ProgressRailPart:
                    child.Arrange(grooveRect);
                    break;
                case AbstractProgressBar.ProgressTrackPart:
                    ArrangeFillTrack(child, owner.GetLineTrackRectForRender(grooveRect), owner);
                    break;
                case AbstractProgressBar.ProgressSuccessPart:
                    ArrangeSuccessTrack(owner, child, grooveRect);
                    break;
                case AbstractProgressBar.ProgressIndicatorPart:
                    child.Arrange(owner.GetLineProgressIndicatorRect(finalSize));
                    break;
                default:
                    child.Arrange(new Rect(finalSize));
                    break;
            }
        }

        return finalSize;
    }

    /// <summary>
    /// Arranges the fill part at the current indicator ratio inside the groove rectangle of the last arrange.
    /// It does not invalidate the measure or the arrange of the panel or of its host, so call it when only the
    /// fill ratio changed. Does nothing when the arrange of the panel is already invalid; the arrange that
    /// follows reads the latest ratio.
    /// </summary>
    internal void SyncTrackToIndicatorRatio()
    {
        if (!IsArrangeValid || TemplatedParent is not AbstractGeneralProgressBar owner)
        {
            return;
        }

        foreach (var child in Children)
        {
            if (child.Name == AbstractProgressBar.ProgressTrackPart)
            {
                ArrangeFillTrack(child, owner.GetLineTrackRectForRender(_grooveRect), owner);
            }
        }
    }

    // A fill shorter than the minimum visible length is arranged as an empty rectangle instead of toggling
    // IsVisible: a visibility change invalidates the measure of the panel, which costs a transition that
    // starts from 0 one extra layout pass.
    private static void ArrangeFillTrack(
        Control track,
        Rect trackRect,
        AbstractGeneralProgressBar owner)
    {
        track.Arrange(owner.IsLineTrackVisible(trackRect)
            ? trackRect
            : new Rect(trackRect.Position, new Size()));
    }

    private static void ArrangeSuccessTrack(
        AbstractGeneralProgressBar owner,
        Control track,
        Rect grooveRect)
    {
        if (owner.IsIndeterminate || double.IsNaN(owner.SuccessThreshold))
        {
            track.IsVisible = false;
            track.Arrange(default);
            return;
        }

        var threshold = Math.Clamp(owner.SuccessThreshold, owner.Minimum, owner.Maximum);
        var trackRect = owner.GetLineTrackRect(grooveRect, threshold);
        track.IsVisible = owner.IsLineTrackVisible(trackRect);
        track.Arrange(trackRect);
    }
}
