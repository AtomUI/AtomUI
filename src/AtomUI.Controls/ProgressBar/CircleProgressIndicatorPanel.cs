using Avalonia;
using Avalonia.Controls;

namespace AtomUI.Controls.Commons;

internal sealed class CircleProgressIndicatorPanel : Panel
{
    protected override Size MeasureOverride(Size availableSize)
    {
        if (TemplatedParent is AbstractCircleProgress owner)
        {
            // Runtime part dimensions can change through dedicated styles without
            // changing the fixed-size owner. The theme consumes the resulting fit flag.
            owner.UpdateStatusIconVisibility();
        }
        var width = 0d;
        var height = 0d;
        foreach (var child in Children)
        {
            child.Measure(Size.Infinity);
            width = Math.Max(width, child.DesiredSize.Width);
            height = Math.Max(height, child.DesiredSize.Height);
        }
        return new Size(width, height);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        foreach (var child in Children)
        {
            var desired = child.DesiredSize;
            child.Arrange(new Rect(
                (finalSize.Width - desired.Width) / 2,
                (finalSize.Height - desired.Height) / 2,
                desired.Width,
                desired.Height));
        }
        return finalSize;
    }
}
