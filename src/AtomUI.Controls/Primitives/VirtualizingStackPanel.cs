using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Layout;
using Avalonia.VisualTree;

namespace AtomUI.Controls.Primitives;

using AvaloniaVirtualizingStackPanel = Avalonia.Controls.VirtualizingStackPanel;

internal class VirtualizingStackPanel : AvaloniaVirtualizingStackPanel
{
    private (int Index, double Offset)? _pendingAlignment;
    private bool _aligning;

    public void ScrollItemIntoView(int index)
    {
        ScrollIntoView(index);
    }

    /// <summary>Places an item at the viewport's start, leaving the specified leading space.</summary>
    internal void ScrollItemToStart(int index, double offset)
    {
        if (index < 0 || !double.IsFinite(offset))
        {
            return;
        }

        _pendingAlignment = (index, offset);
        if (IsArrangeValid)
        {
            ApplyPendingAlignment();
        }
        else if (this.IsAttachedToVisualTree())
        {
            LayoutUpdated -= HandleLayoutUpdated;
            LayoutUpdated += HandleLayoutUpdated;
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        ClearPendingAlignment();
        base.OnDetachedFromVisualTree(e);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var result = base.ArrangeOverride(finalSize);
        if (_pendingAlignment is not null)
        {
            LayoutUpdated -= HandleLayoutUpdated;
            LayoutUpdated += HandleLayoutUpdated;
        }
        return result;
    }

    private void HandleLayoutUpdated(object? sender, EventArgs e) => ApplyPendingAlignment();

    private void ApplyPendingAlignment()
    {
        if (_aligning || _pendingAlignment is not { } request || TopLevel.GetTopLevel(this) is not { } topLevel ||
            this.FindAncestorOfType<ScrollContentPresenter>() is not { } viewport)
        {
            return;
        }

        ClearPendingAlignment();
        if (request.Index >= Items.Count)
        {
            return;
        }

        _aligning = true;
        try
        {
            // Avalonia realizes the distant item using its own estimated extent and recycler.
            var item = ContainerFromIndex(request.Index);
            if (item is null || !item.IsArrangeValid)
            {
                item = ScrollIntoView(request.Index);
            }

            var scrollHost = viewport.FindAncestorOfType<Avalonia.Controls.ScrollViewer>();
            if (item?.TranslatePoint(default, viewport) is not { } position || scrollHost is null)
            {
                return;
            }

            var horizontal = Orientation == Orientation.Horizontal;
            var current = scrollHost.Offset;
            var delta = (horizontal ? position.X : position.Y) - request.Offset;
            var target = horizontal ? new Vector(current.X + delta, current.Y) : new Vector(current.X, current.Y + delta);
            if (target != current)
            {
                scrollHost.SetCurrentValue(Avalonia.Controls.ScrollViewer.OffsetProperty, target);
                topLevel.UpdateLayout();
            }
        }
        finally
        {
            _aligning = false;
        }
    }

    private void ClearPendingAlignment()
    {
        _pendingAlignment = null;
        LayoutUpdated -= HandleLayoutUpdated;
    }
}
