using AtomUI.Controls.Commons;
using AtomUI.Theme.Resources;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Presenters;
using Avalonia.Layout;
using Avalonia.Media.Transformation;
using Avalonia.Rendering.Composition;
using Avalonia.Styling;
using Avalonia.VisualTree;

namespace AtomUI.Controls;

/// <summary>
/// Extent of the selection indicator of a tab strip along the strip, relative to the selected tab.
/// </summary>
public enum TabIndicatorMode
{
    /// <summary>
    /// The indicator spans the whole selected tab item.
    /// </summary>
    Full,
    /// <summary>
    /// The indicator spans the header of the selected tab item; when the header cannot be found or has no size,
    /// it spans the whole item.
    /// </summary>
    Content,
    /// <summary>
    /// The indicator has the length of <see cref="AbstractTabControl.IndicatorFixedLength" /> and is centered
    /// on the header of the selected tab item; when the header cannot be found or has no size, it is centered
    /// on the whole item.
    /// </summary>
    Fixed
}

internal sealed class TabSelectionIndicator(SelectingItemsControl owner)
{
    private const string ScaleChannel = "Scale";
    private const string TranslationChannel = "Translation";
    private static readonly CubicEaseOut EaseOut = new();

    private Border? _indicator;
    private ScrollViewer? _scroll;
    private ItemsPresenter? _itemsPresenter;
    private Control? _lastSelection;
    private bool _initialized;
    private Size _lastViewport;
    private Size _lastExtent;
    private double _lastCenteredItemStart;
    private double _lastCenteredItemLength;
    private bool _lastCenteringVertical;
    private IndicatorBounds _lastIndicator;
    private bool _hasIndicatorBounds;
    private MotionGate? _motionGate;
    private CompositionVisual? _animatedVisual;
    private Vector3D _originalScale;
    private Vector3D _originalTranslation;
    private CancellationTokenSource? _scrollAnimation;
    private Vector? _scrollTarget;
    private int _lastItemCount;

    internal void Apply(INameScope names, bool isMotionEnabled)
    {
        var container = names.Find<Control>("PART_TabsContainer");
        var scroll = container as ScrollViewer ?? container?.FindDescendantOfType<ScrollViewer>();
        SetParts(names.Find<Border>("PART_SelectedItemIndicator"), scroll,
            names.Find<ItemsPresenter>("PART_ItemsPresenter"), isMotionEnabled);
    }

    internal void SetParts(Border? indicator, ScrollViewer? scroll, ItemsPresenter? itemsPresenter, bool isMotionEnabled)
    {
        Reset();
        _indicator = indicator;
        _scroll = scroll;
        _itemsPresenter = itemsPresenter;
        if (owner.IsAttachedToVisualTree())
        {
            Attach(isMotionEnabled);
        }
    }

    internal void Attach(bool isMotionEnabled)
    {
        if ((_indicator is not null || _scroll is not null) && _motionGate is null)
        {
            _motionGate = new MotionGate(owner, HandleMotionAvailability) { IsMotionEnabled = isMotionEnabled };
        }
    }

    internal void Detach()
    {
        _motionGate?.Dispose();
        _motionGate = null;
        StopIndicatorMotion();
        StopScrolling(false);
        _lastSelection = null;
        _initialized = false;
        _hasIndicatorBounds = false;
    }

    internal void Reset()
    {
        Detach();
        _indicator = null;
        _scroll = null;
        _itemsPresenter = null;
    }

    internal void Update(Dock placement, TabIndicatorMode mode, double fixedLength, bool animateLength,
        bool centerSelection, bool isMotionEnabled, IndicatorMotionMode motionMode)
    {
        if (_indicator is null && _scroll is null)
        {
            return;
        }

        if (_motionGate is { } gate)
        {
            gate.IsMotionEnabled = isMotionEnabled;
        }

        var selected = owner.SelectedIndex >= 0 ? owner.ContainerFromIndex(owner.SelectedIndex) : null;
        if (selected is null || !selected.IsArrangeValid || selected.Bounds.Width <= 0 || selected.Bounds.Height <= 0)
        {
            if (owner.SelectedIndex < 0)
            {
                _indicator?.SetCurrentValue(Visual.IsVisibleProperty, false);
                StopIndicatorMotion();
                StopScrolling(false);
                _lastSelection = null;
                _hasIndicatorBounds = false;
            }
            return;
        }

        var selectionChanged = _initialized && !ReferenceEquals(selected, _lastSelection);
        var itemsChanged = owner.ItemCount != _lastItemCount;
        var allowMotion = isMotionEnabled && (_motionGate?.ShouldRun ?? false);
        var vertical = placement is Dock.Left or Dock.Right;
        _lastSelection = selected;
        _lastItemCount = owner.ItemCount;
        _initialized = true;
        if (_indicator is { } indicator && indicator.GetVisualParent() is Visual parent &&
            selected.TransformToVisual(parent) is { } itemTransform)
        {
            var bounds = ResolveBounds(selected, new Rect(selected.Bounds.Size).TransformToAABB(itemTransform),
                vertical, mode, fixedLength);
            var previous = _lastIndicator;
            if (!_hasIndicatorBounds || previous != bounds)
            {
                StopIndicatorMotion();
                indicator.SetCurrentValue(Visual.IsVisibleProperty, true);
                indicator.SetCurrentValue(vertical ? Layoutable.HeightProperty : Layoutable.WidthProperty, bounds.Length);
                var builder = new TransformOperations.Builder(1);
                builder.AppendTranslate(vertical ? 0 : bounds.Start, vertical ? bounds.Start : 0);
                indicator.SetCurrentValue(Visual.RenderTransformProperty, builder.Build());
                if (_hasIndicatorBounds && selectionChanged && allowMotion && !itemsChanged &&
                    motionMode != IndicatorMotionMode.None)
                {
                    AnimateIndicator(indicator, previous, bounds, vertical, animateLength, motionMode);
                }
                _lastIndicator = bounds;
                _hasIndicatorBounds = true;
            }
        }

        if (centerSelection && _scroll is { } scroll && _itemsPresenter is { } presenter &&
            selected.TransformToVisual(presenter) is { } scrollTransform)
        {
            var itemBounds = new Rect(selected.Bounds.Size).TransformToAABB(scrollTransform);
            var itemStart = vertical ? itemBounds.Y : itemBounds.X;
            var itemLength = vertical ? itemBounds.Height : itemBounds.Width;
            var viewportChanged = scroll.Viewport != _lastViewport || scroll.Extent != _lastExtent;
            var itemMoved = Math.Abs(itemStart - _lastCenteredItemStart) >= .5 || itemLength != _lastCenteredItemLength ||
                            vertical != _lastCenteringVertical;
            if (selectionChanged || itemsChanged || viewportChanged || itemMoved || _scrollTarget is null)
            {
                _lastViewport = scroll.Viewport;
                _lastExtent = scroll.Extent;
                _lastCenteredItemStart = itemStart;
                _lastCenteredItemLength = itemLength;
                _lastCenteringVertical = vertical;
                var next = GetCenteredOffset(scroll, itemStart, itemLength, vertical);
                Center(scroll, next, selectionChanged && !itemsChanged && !viewportChanged && allowMotion);
            }
        }
        else if (_scrollTarget is not null)
        {
            StopScrolling(false);
        }
    }

    internal static IndicatorBounds ResolveBounds(Control selected, Rect itemBounds, bool vertical, TabIndicatorMode mode, double fixedLength)
    {
        var start = vertical ? itemBounds.Y : itemBounds.X;
        var length = vertical ? itemBounds.Height : itemBounds.Width;
        if (mode == TabIndicatorMode.Full)
        {
            return new IndicatorBounds(start, length);
        }

        var presenter = (selected as AbstractTabItem)?.IndicatorHeader ?? selected.GetVisualDescendants()
            .OfType<Control>().FirstOrDefault(control => control.Name is "PART_HeaderPresenter" or "ContentPresenter");
        var header = (presenter as ContentPresenter)?.Child ?? presenter;
        if (header is not null && header.TransformToVisual(selected) is { } transform &&
            header.Bounds.Width > 0 && header.Bounds.Height > 0)
        {
            // A mirrored header's local origin is its far edge in the item's coordinate system.
            var headerBounds = new Rect(header.Bounds.Size).TransformToAABB(transform);
            start += vertical ? headerBounds.Y : headerBounds.X;
            length = vertical ? headerBounds.Height : headerBounds.Width;
        }

        return mode == TabIndicatorMode.Fixed
            ? new IndicatorBounds(start + (length - fixedLength) / 2, fixedLength)
            : new IndicatorBounds(start, length);
    }

    internal static Vector GetCenteredOffset(ScrollViewer scroll, double position, double length, bool vertical)
    {
        var viewport = vertical ? scroll.Viewport.Height : scroll.Viewport.Width;
        var extent = vertical ? scroll.Extent.Height : scroll.Extent.Width;
        var offset = Math.Clamp(position - (viewport - length) / 2, 0, Math.Max(0, extent - viewport));
        return vertical ? new Vector(scroll.Offset.X, offset) : new Vector(offset, scroll.Offset.Y);
    }

    private void AnimateIndicator(Border indicator, IndicatorBounds previous, IndicatorBounds target, bool vertical,
                                  bool animateLength, IndicatorMotionMode motionMode)
    {
        if (ElementComposition.GetElementVisual(indicator) is not { } visual)
        {
            return;
        }

        var duration = GetDuration();
        if (duration <= TimeSpan.Zero)
        {
            return;
        }

        _animatedVisual = visual;
        _originalScale = visual.Scale;
        _originalTranslation = visual.Translation;
        var delta = previous.Start - target.Start;
        var stretch = motionMode == IndicatorMotionMode.Stretch && delta != 0 && previous.Length > 0 && target.Length > 0;
        var join = stretch ? StretchBounds(previous, target) : default;
        if (delta != 0)
        {
            var animation = visual.Compositor.CreateVector3DKeyFrameAnimation();
            animation.Duration = duration;
            animation.InsertKeyFrame(0, Translate(delta));
            if (stretch)
            {
                animation.InsertKeyFrame(0.5f, Translate(join.Start - target.Start));
            }
            animation.InsertKeyFrame(1, _originalTranslation, EaseOut);
            visual.StartAnimation(TranslationChannel, animation);
        }
        if (stretch || (animateLength && previous.Length > 0 && target.Length > 0 && previous.Length != target.Length))
        {
            var animation = visual.Compositor.CreateVector3DKeyFrameAnimation();
            animation.Duration = duration;
            animation.InsertKeyFrame(0, Scale(previous.Length / target.Length));
            if (stretch)
            {
                animation.InsertKeyFrame(0.5f, Scale(join.Length / target.Length));
            }
            animation.InsertKeyFrame(1, _originalScale, EaseOut);
            visual.StartAnimation(ScaleChannel, animation);
        }

        Vector3D Translate(double offset) => _originalTranslation +
            (vertical ? new Vector3D(0, offset, 0) : new Vector3D(offset, 0, 0));

        Vector3D Scale(double ratio) => vertical
            ? new Vector3D(_originalScale.X, _originalScale.Y * ratio, _originalScale.Z)
            : new Vector3D(_originalScale.X * ratio, _originalScale.Y, _originalScale.Z);
    }

    private static IndicatorBounds StretchBounds(IndicatorBounds previous, IndicatorBounds target)
    {
        var forward = target.Start > previous.Start;
        var start = previous.Start + (target.Start - previous.Start) * (forward ? 0.35 : 0.65);
        var far = previous.Start + previous.Length;
        var end = far + (target.Start + target.Length - far) * (forward ? 0.65 : 0.35);
        return new IndicatorBounds(start, Math.Max(0, end - start));
    }

    private void StopIndicatorMotion()
    {
        if (_animatedVisual is not { } visual)
        {
            return;
        }
        visual.StopAnimation(ScaleChannel);
        visual.StopAnimation(TranslationChannel);
        visual.Scale = _originalScale;
        visual.Translation = _originalTranslation;
        _animatedVisual = null;
    }

    private void Center(ScrollViewer scroll, Vector target, bool animate)
    {
        if (_scrollTarget == target)
        {
            return;
        }

        var origin = scroll.Offset;
        StopScrolling(false);
        _scrollTarget = target;
        if (!animate || origin == target)
        {
            scroll.SetCurrentValue(ScrollViewer.OffsetProperty, target);
            return;
        }

        var duration = GetDuration();
        if (duration <= TimeSpan.Zero)
        {
            scroll.SetCurrentValue(ScrollViewer.OffsetProperty, target);
            return;
        }

        _scrollAnimation = new CancellationTokenSource();
        var animation = new Animation
        {
            Duration = duration,
            Easing = EaseOut,
            FillMode = FillMode.None,
            Children =
            {
                new KeyFrame { Cue = new Cue(0), Setters = { new Setter(ScrollViewer.OffsetProperty, origin) } },
                new KeyFrame { Cue = new Cue(1), Setters = { new Setter(ScrollViewer.OffsetProperty, target) } }
            }
        };
        _ = animation.RunAsync(scroll, _scrollAnimation.Token);
        scroll.SetCurrentValue(ScrollViewer.OffsetProperty, target);
    }

    private void StopScrolling(bool complete)
    {
        var previousTarget = _scrollTarget;
        _scrollTarget = null;
        _scrollAnimation?.Cancel();
        _scrollAnimation?.Dispose();
        _scrollAnimation = null;
        if (complete && _scroll is { } scroll && previousTarget is { } target)
        {
            scroll.SetCurrentValue(ScrollViewer.OffsetProperty, target);
            _scrollTarget = target;
        }
    }

    private void HandleMotionAvailability(bool enabled)
    {
        if (!enabled)
        {
            StopIndicatorMotion();
            StopScrolling(true);
        }
    }

    // Indicator motion runs only while the owner is attached, so its theme context exists.
    private TimeSpan GetDuration() =>
        ControlTokenAccessor.Capture(owner).GetEffectiveGlobal<TimeSpan>(SharedTokenKind.MotionDurationMid);

    internal readonly record struct IndicatorBounds(double Start, double Length);
}
