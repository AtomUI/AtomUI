using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Rendering;

namespace AtomUI.Desktop.Controls;

internal class TabScrollContentPresenter : ScrollContentPresenter, 
                                           ICustomHitTest
{
    internal static readonly DirectProperty<TabScrollContentPresenter, Dock> TabStripPlacementProperty =
        AvaloniaProperty.RegisterDirect<TabScrollContentPresenter, Dock>(nameof(TabStripPlacement),
            o => o.TabStripPlacement,
            (o, v) => o.TabStripPlacement = v);
    
    private Dock _tabStripPlacement = Dock.Top;

    internal Dock TabStripPlacement
    {
        get => _tabStripPlacement;
        set => SetAndRaise(TabStripPlacementProperty, ref _tabStripPlacement, value);
    }
    
    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        if (Extent.Height > Viewport.Height || Extent.Width > Viewport.Width)
        {
            var scrollable = Child as ILogicalScrollable;
            var isLogical  = scrollable?.IsLogicalScrollEnabled == true;

            var x     = Offset.X;
            var y     = Offset.Y;
            var delta = e.Delta;
            if (TabStripPlacement == Dock.Top || TabStripPlacement == Dock.Bottom)
            {
                // Horizontal tabs prefer horizontal delta, fall back to vertical delta when horizontal is zero
                var dx = delta.X != 0 ? delta.X : delta.Y;
                delta  = new Vector(dx, 0);
            }
            else
            {
                // Vertical tabs prefer vertical delta, fall back to horizontal delta when vertical is zero
                var dy = delta.Y != 0 ? delta.Y : delta.X;
                delta  = new Vector(0, dy);
            }

            if (Extent.Height > Viewport.Height)
            {
                var height = isLogical ? scrollable!.ScrollSize.Height : 50;
                y += -delta.Y * height;
                y =  Math.Max(y, 0);
                y =  Math.Min(y, Extent.Height - Viewport.Height);
            }

            if (Extent.Width > Viewport.Width)
            {
                var width = isLogical ? scrollable!.ScrollSize.Width : 50;
                x += -delta.X * width;
                x =  Math.Max(x, 0);
                x =  Math.Min(x, Extent.Width - Viewport.Width);
            }
            var newOffset = this.SnapOffset(new Vector(x, y), delta, true);

            var offsetChanged = newOffset != Offset;
            SetCurrentValue(OffsetProperty, newOffset);

            e.Handled = !IsScrollChainingEnabled || offsetChanged;
        }
    }

    public bool HitTest(Point point)
    {
        return true;
    }
}
