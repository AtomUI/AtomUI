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
        var actual = IsVisible && !freshOpening ? Extent(Bounds.Size, horizontal) : 0;
        var desired = IsVisible && !freshOpening ? Extent(DesiredSize.Deflate(Margin), horizontal) : 0;
        var layout = new LayoutLease(this, direction, expanded, actual, desired);
        _layout = layout;
        InvalidateMotionLayout();
        return layout;
    }

    private LayoutLease? _layout;

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
        base.ArrangeCore(_layout?.Constrain(finalRect) ?? finalRect);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        return _layout?.Arrange(finalSize) ?? base.ArrangeOverride(finalSize);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _layout?.Dispose();
    }

    private static double Extent(Size size, bool horizontal) => horizontal ? size.Width : size.Height;

    private void InvalidateMotionLayout()
    {
        InvalidateMeasure();
        // Actual occupancy can change while a width reflow has already published
        // the same DesiredSize. Ancestors must arrange again to replace their
        // cached collapsed slots, even when measurement reports no size change.
        for (var parent = this.GetVisualParent(); parent is not null; parent = parent.GetVisualParent())
        {
            if (parent is Layoutable layoutable)
            {
                layoutable.InvalidateArrange();
            }
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
        private bool _disposed;

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
