using Avalonia;
using Avalonia.Controls;

namespace AtomUI.Controls.Commons;

internal sealed class LineProgressPanel : Panel
{
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
        foreach (var child in Children)
        {
            switch (child.Name)
            {
                case AbstractProgressBar.ProgressRailPart:
                    child.Arrange(grooveRect);
                    break;
                case AbstractProgressBar.ProgressTrackPart:
                    ArrangeTrack(owner, child, grooveRect, owner.Value);
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

    private static void ArrangeTrack(
        AbstractGeneralProgressBar owner,
        Control track,
        Rect grooveRect,
        double value)
    {
        var trackRect = owner.GetLineTrackRect(grooveRect, value);
        track.IsVisible = owner.IsLineTrackVisible(trackRect);
        track.Arrange(trackRect);
    }

    private static void ArrangeSuccessTrack(
        AbstractGeneralProgressBar owner,
        Control track,
        Rect grooveRect)
    {
        if (double.IsNaN(owner.SuccessThreshold))
        {
            track.IsVisible = false;
            track.Arrange(default);
            return;
        }

        var threshold = Math.Clamp(owner.SuccessThreshold, owner.Minimum, owner.Maximum);
        ArrangeTrack(owner, track, grooveRect, threshold);
    }
}
