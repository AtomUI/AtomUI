using Avalonia;
using Avalonia.Controls;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Arranges Breadcrumb item containers and their sibling separators in one horizontal flow:
/// container 0, separator 0, container 1, separator 1, ..., container N - 1.
/// Item containers stay in <see cref="Panel.Children"/> so the item generator keeps full
/// ownership of that collection; separator visuals are appended to the panel's visual
/// children so they render without polluting the generator's index mapping.
/// </summary>
internal class BreadcrumbItemsPanel : Panel
{
    private readonly List<Control> _separators = new();

    internal void SyncSeparators(IReadOnlyList<Control> separators)
    {
        var count = Math.Min(Math.Max(0, Children.Count - 1), separators.Count);
        if (_separators.Count == count &&
            _separators.SequenceEqual(separators.Take(count), ReferenceEqualityComparer.Instance))
        {
            return;
        }

        var desired = new HashSet<Control>(separators.Take(count), ReferenceEqualityComparer.Instance);
        for (var i = VisualChildren.Count - 1; i >= 0; i--)
        {
            if (VisualChildren[i] is Control control &&
                _separators.Contains(control) &&
                !desired.Contains(control))
            {
                VisualChildren.RemoveAt(i);
            }
        }

        _separators.Clear();
        for (var i = 0; i < count; i++)
        {
            var separator = separators[i];
            if (!VisualChildren.Contains(separator))
            {
                VisualChildren.Add(separator);
            }

            _separators.Add(separator);
        }

        InvalidateMeasure();
    }

    internal void ClearSeparatorVisuals()
    {
        for (var i = VisualChildren.Count - 1; i >= 0; i--)
        {
            if (VisualChildren[i] is Control control && _separators.Contains(control))
            {
                VisualChildren.RemoveAt(i);
            }
        }

        _separators.Clear();
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var desired = new Size();
        for (var i = 0; i < Children.Count; i++)
        {
            MeasureFlowChild(Children[i], availableSize, ref desired);
            if (i < Children.Count - 1 && i < _separators.Count)
            {
                MeasureFlowChild(_separators[i], availableSize, ref desired);
            }
        }

        return desired;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var offset = 0.0;
        for (var i = 0; i < Children.Count; i++)
        {
            offset = ArrangeFlowChild(Children[i], offset, finalSize);
            if (i < Children.Count - 1 && i < _separators.Count)
            {
                offset = ArrangeFlowChild(_separators[i], offset, finalSize);
            }
        }

        return finalSize;
    }

    private static void MeasureFlowChild(Control child, Size availableSize, ref Size desired)
    {
        child.Measure(availableSize);
        var childDesired = child.DesiredSize;
        // Layoutable.MeasureCore already includes Margin in DesiredSize. Add the
        // child's slot once; adding Margin again doubles semantic separator spacing.
        desired = new Size(
            desired.Width + childDesired.Width,
            Math.Max(desired.Height, childDesired.Height));
    }

    private static double ArrangeFlowChild(Control child, double offset, Size finalSize)
    {
        // Pass a layout slot that includes the child's DesiredSize (and therefore
        // its Margin). Layoutable.ArrangeCore applies Margin and cross-axis
        // alignment exactly once and produces the content Bounds inside that slot.
        var slotWidth = child.DesiredSize.Width;
        child.Arrange(new Rect(offset, 0, slotWidth, finalSize.Height));
        return offset + slotWidth;
    }
}
