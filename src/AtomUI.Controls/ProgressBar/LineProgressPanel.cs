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
                    ArrangeFillTrack(child, owner.GetLineTrackRectByRatio(grooveRect, owner.IndicatorRatio), owner);
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
    /// 只有填充比例变化时，在上次排布的槽内直接按新比例排布填充部件，不使面板或宿主的测量、排布失效。
    /// 面板排布已失效时什么都不做，随后的排布会读到最新比例。
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
                ArrangeFillTrack(child, owner.GetLineTrackRectByRatio(_grooveRect, owner.IndicatorRatio), owner);
            }
        }
    }

    // 填充不足最小可见长度时排成空矩形而不切换 IsVisible：可见性翻转会使面板测量失效，
    // 从 0 起步的过渡因此多出一次布局
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
        if (double.IsNaN(owner.SuccessThreshold))
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
