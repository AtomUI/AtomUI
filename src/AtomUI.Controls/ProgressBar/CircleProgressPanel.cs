using Avalonia;
using Avalonia.Controls;

namespace AtomUI.Controls.Commons;

internal sealed class CircleProgressPanel : Panel
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
        if (TemplatedParent is not AbstractCircleProgress owner)
        {
            foreach (var child in Children)
            {
                child.Arrange(new Rect(finalSize));
            }

            return finalSize;
        }

        var pathRect = owner.GetCircleProgressPathRect(finalSize);
        foreach (var child in Children)
        {
            if (child is CircleProgressIndicatorPanel)
            {
                var height = child.DesiredSize.Height;
                child.Arrange(new Rect(0, (finalSize.Height - height) / 2, finalSize.Width, height));
                continue;
            }
            child.Arrange(child.Name is AbstractProgressBar.ProgressRailPart or
                                      AbstractProgressBar.ProgressTrackPart or
                                      AbstractProgressBar.ProgressSuccessPart
                ? pathRect
                : new Rect(finalSize));
        }

        return finalSize;
    }
}
