using AtomUI.Animations;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.VisualTree;

namespace AtomUI.Controls.Primitives;

/// <summary>Reports animated content occupancy while arranging its content at its natural size.</summary>
public class ContentViewport : ContentControl, IMotionLayout
{
    public IMotionLayoutLease BeginLayoutMotion(Direction direction, bool expanded)
    {
        if (!Enum.IsDefined(direction))
        {
            throw new ArgumentOutOfRangeException(nameof(direction));
        }
        var horizontal = direction is Direction.Left or Direction.Right;
        var freshOpening = expanded && _layout is null;
        var previous = _layout is { Horizontal: var axis } lease && axis == horizontal ? lease : null;
        // A freshly visible preparation may still have Bounds from its last hidden
        // state. Until its layout is committed, hand off the captured start instead.
        var actual = IsVisible && !freshOpening
            ? previous?.PresentedExtent ?? Extent(Bounds.Size, horizontal)
            : 0;
        var desired = IsVisible && !freshOpening
            ? previous?.DesiredExtent ?? Extent(DesiredSize.Deflate(Margin), horizontal)
            : 0;
        var layout = new LayoutLease(this, direction, expanded, actual, desired);
        _layout = layout;
        CaptureAncestorDesiredSizes();
        InvalidateMotionLayout();
        return layout;
    }

    private LayoutLease? _layout;
    private LayoutLease? _arrangingLayout;
    // Ancestor desired sizes seen at the previous arrange while occupancy is animated or settling.
    private List<(Layoutable Ancestor, Size DesiredSize)>? _ancestorDesiredSizes;

    static ContentViewport()
    {
        ClipToBoundsProperty.OverrideDefaultValue<ContentViewport>(true);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        return _layout?.Measure(availableSize) ?? base.MeasureOverride(availableSize);
    }

    protected override void ArrangeCore(Rect finalRect)
    {
        var layout = _layout;
        var previous = _arrangingLayout;
        _arrangingLayout = layout;
        try
        {
            base.ArrangeCore(layout?.Constrain(finalRect) ?? finalRect);
            if (ReferenceEquals(_layout, layout))
            {
                // Equal Bounds do not notify, but this lease still completed its arrangement.
                layout?.CompleteArrange();
            }
        }
        finally
        {
            _arrangingLayout = previous;
        }
        RearrangeResizedAncestors();
        if (_layout is null)
        {
            _ancestorDesiredSizes = null;
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        if (change.Property == BoundsProperty && _arrangingLayout is { } layout && ReferenceEquals(_layout, layout))
        {
            // Bounds is now published; external notification callbacks may capture it.
            layout.CompleteArrange();
        }
        base.OnPropertyChanged(change);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        return _layout?.Arrange(finalSize) ?? base.ArrangeOverride(finalSize);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _layout?.Dispose();
        _ancestorDesiredSizes = null;
    }

    private static double Extent(Size size, bool horizontal) => horizontal ? size.Width : size.Height;

    private void InvalidateMotionLayout()
    {
        // Only this node is invalidated per tick; Avalonia propagates DesiredSize changes
        // upwards and stops at the first ancestor whose size is unchanged.
        InvalidateMeasure();
    }

    private void CaptureAncestorDesiredSizes()
    {
        var sizes = _ancestorDesiredSizes ??= [];
        sizes.Clear();
        for (var parent = this.GetVisualParent(); parent is not null; parent = parent.GetVisualParent())
        {
            if (parent is Layoutable layoutable)
            {
                sizes.Add((layoutable, layoutable.DesiredSize));
            }
        }
    }

    private void RearrangeResizedAncestors()
    {
        if (_ancestorDesiredSizes is not { } sizes)
        {
            return;
        }
        // Avalonia's Measure keeps IsArrangeValid when an ancestor is re-measured under a new
        // constraint, and suppresses ChildDesiredSizeChanged while that ancestor is measuring.
        // An identical arrange rect then skips the ancestor and leaves its children in stale
        // slots. Re-arrange exactly the ancestors whose DesiredSize changed; this is the set
        // Avalonia already invalidates on the normal path, so the dirty region does not grow.
        var index = 0;
        for (var parent = this.GetVisualParent(); parent is not null; parent = parent.GetVisualParent())
        {
            if (parent is not Layoutable layoutable)
            {
                continue;
            }
            var desiredSize = layoutable.DesiredSize;
            if (index < sizes.Count && ReferenceEquals(sizes[index].Ancestor, layoutable))
            {
                if (sizes[index].DesiredSize != desiredSize)
                {
                    sizes[index] = (layoutable, desiredSize);
                    layoutable.InvalidateArrange();
                }
            }
            else
            {
                if (index < sizes.Count)
                {
                    sizes.RemoveRange(index, sizes.Count - index);
                }
                sizes.Add((layoutable, desiredSize));
            }
            index++;
        }
        if (index < sizes.Count)
        {
            sizes.RemoveRange(index, sizes.Count - index);
        }
    }

    private sealed class LayoutLease : IMotionLayoutLease
    {
        private readonly ContentViewport _owner;
        private readonly Direction _direction;
        private readonly bool _expanded;
        private readonly bool _horizontal;
        private readonly ExtentInterpolation _desired;
        private readonly ExtentInterpolation _viewport;
        private Size _contentSize;
        private Size _arrangedContentSize;
        private double _progress;
        private bool _measured;
        private bool _arranged;
        private bool _disposed;

        internal bool Horizontal => _horizontal;
        internal double PresentedExtent => _arranged ? Extent(_owner.Bounds.Size, _horizontal) : _viewport.Start;
        internal double DesiredExtent => _measured
            ? Extent(_owner.DesiredSize.Deflate(_owner.Margin), _horizontal) : _desired.Start;

        internal void CompleteArrange()
        {
            _arranged = true;
        }

        internal LayoutLease(ContentViewport owner, Direction direction, bool expanded, double actual, double desired)
        {
            _owner = owner;
            _direction = direction;
            _expanded = expanded;
            _horizontal = direction is Direction.Left or Direction.Right;
            _desired = new ExtentInterpolation(desired);
            _viewport = new ExtentInterpolation(actual);
        }

        public void SetProgress(double progress)
        {
            if (_disposed || !ReferenceEquals(_owner._layout, this))
            {
                return;
            }
            if (!double.IsFinite(progress))
            {
                throw new ArgumentOutOfRangeException(nameof(progress));
            }
            if (_progress != progress)
            {
                _progress = progress;
                _owner.InvalidateMotionLayout();
            }
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }
            _disposed = true;
            if (ReferenceEquals(_owner._layout, this))
            {
                _owner._layout = null;
                _owner.InvalidateMotionLayout();
            }
        }

        internal Size Measure(Size available)
        {
            var content = _owner.Presenter;
            content?.Measure(available);
            _contentSize = content?.DesiredSize ?? default;
            var target = _expanded ? Extent(_contentSize, _horizontal) : 0;
            var extent = _desired.Interpolate(target, _progress, Extent(_owner.DesiredSize.Deflate(_owner.Margin), _horizontal));
            _measured = true;
            return _horizontal ? _contentSize.WithWidth(extent) : _contentSize.WithHeight(extent);
        }

        internal Rect Constrain(Rect rect)
        {
            var available = rect.Size.Deflate(_owner.Margin);
            var full = Math.Max(Extent(_contentSize, _horizontal), Extent(available, _horizontal));
            _arrangedContentSize = _horizontal ? available.WithWidth(full) : available.WithHeight(full);
            var extent = _viewport.Interpolate(_expanded ? full : 0, _progress, Extent(_owner.Bounds.Size, _horizontal));
            var margin = _horizontal ? _owner.Margin.Left + _owner.Margin.Right : _owner.Margin.Top + _owner.Margin.Bottom;
            var length = Math.Min(Extent(rect.Size, _horizontal), extent + margin);
            var size = _horizontal ? rect.Size.WithWidth(length) : rect.Size.WithHeight(length);
            if (_owner.UseLayoutRounding)
            {
                size = LayoutHelper.RoundLayoutSizeUp(size, LayoutHelper.GetLayoutScale(_owner));
            }
            var origin = _direction switch
            {
                Direction.Left => new Point(rect.Right - size.Width, rect.Y),
                Direction.Top => new Point(rect.X, rect.Bottom - size.Height),
                _ => rect.Position
            };
            return new Rect(origin, size);
        }

        internal Size Arrange(Size finalSize)
        {
            var full = _horizontal ? finalSize.WithWidth(_arrangedContentSize.Width) : finalSize.WithHeight(_arrangedContentSize.Height);
            var origin = _direction switch
            {
                Direction.Left => new Point(finalSize.Width - full.Width, 0),
                Direction.Top => new Point(0, finalSize.Height - full.Height),
                _ => default
            };
            _owner.Presenter?.Arrange(new Rect(origin, full));
            return finalSize;
        }
    }

    private sealed class ExtentInterpolation(double start)
    {
        internal double Start => _start;

        private double _start = start;
        private double? _target;

        internal double Interpolate(double target, double progress, double presented)
        {
            if (_target is { } previous && previous != target && progress < 1)
            {
                _start = (presented - target * progress) / (1 - progress);
            }
            _target = target;
            return Math.Max(0, _start + (target - _start) * progress);
        }
    }
}
