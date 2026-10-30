using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Transformation;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

internal enum FeedbackStackMode
{
    Message,
    Notification
}

/// <summary>
/// Steady-state allocation-free layout primitive shared by transient feedback hosts.
/// Children are ordered from oldest to newest.
/// </summary>
internal sealed class FeedbackStackPanel : Panel
{
    private const double CollapsedClipOverflow = 48;
    private static readonly ITransform FullTransform = BuildTransform(1, 0);

    public static readonly AttachedProperty<double> StackClipProgressProperty =
        AvaloniaProperty.RegisterAttached<FeedbackStackPanel, Control, double>("StackClipProgress");

    private static readonly AttachedProperty<bool> StackClipIsBottomProperty =
        AvaloniaProperty.RegisterAttached<FeedbackStackPanel, Control, bool>("StackClipIsBottom");

    public static readonly StyledProperty<FeedbackStackMode> StackModeProperty =
        AvaloniaProperty.Register<FeedbackStackPanel, FeedbackStackMode>(nameof(StackMode));

    public static readonly StyledProperty<NotificationPosition> PositionProperty =
        AvaloniaProperty.Register<FeedbackStackPanel, NotificationPosition>(
            nameof(Position), NotificationPosition.TopCenter);

    public static readonly StyledProperty<bool> IsStackEnabledProperty =
        AvaloniaProperty.Register<FeedbackStackPanel, bool>(nameof(IsStackEnabled));

    public static readonly StyledProperty<int> StackThresholdProperty =
        AvaloniaProperty.Register<FeedbackStackPanel, int>(nameof(StackThreshold), 3);

    public static readonly StyledProperty<bool> IsStackExpandedProperty =
        AvaloniaProperty.Register<FeedbackStackPanel, bool>(nameof(IsStackExpanded));

    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        AvaloniaProperty.Register<FeedbackStackPanel, bool>(nameof(IsMotionEnabled), true);

    public static readonly StyledProperty<double> ExpandedGapProperty =
        AvaloniaProperty.Register<FeedbackStackPanel, double>(nameof(ExpandedGap), 16);

    public static readonly StyledProperty<double> CollapsedOffsetProperty =
        AvaloniaProperty.Register<FeedbackStackPanel, double>(nameof(CollapsedOffset), 8);

    private bool _isCollapsed;
    private bool _hasArrangedProjection;
    private bool _wasArrangedCollapsed;
    private bool _hasActiveTransitionSnapshots;
    private Size _collapsedCardSize;
    private FeedbackStackPresenter? _owner;
    private readonly ConditionalWeakTable<Control, CachedProjection> _projections = new();

    public FeedbackStackMode StackMode
    {
        get => GetValue(StackModeProperty);
        set => SetValue(StackModeProperty, value);
    }

    public NotificationPosition Position
    {
        get => GetValue(PositionProperty);
        set => SetValue(PositionProperty, value);
    }

    public bool IsStackEnabled
    {
        get => GetValue(IsStackEnabledProperty);
        set => SetValue(IsStackEnabledProperty, value);
    }

    public int StackThreshold
    {
        get => GetValue(StackThresholdProperty);
        set => SetValue(StackThresholdProperty, value);
    }

    public bool IsStackExpanded
    {
        get => GetValue(IsStackExpandedProperty);
        set => SetValue(IsStackExpandedProperty, value);
    }

    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    public double ExpandedGap
    {
        get => GetValue(ExpandedGapProperty);
        set => SetValue(ExpandedGapProperty, value);
    }

    public double CollapsedOffset
    {
        get => GetValue(CollapsedOffsetProperty);
        set => SetValue(CollapsedOffsetProperty, value);
    }

    public bool IsCollapsed => _isCollapsed;

    public Size CollapsedCardSize => _collapsedCardSize;

    static FeedbackStackPanel()
    {
        // A transition can publish its animated value inside the base-value notification.
        // Read the effective value so the outer notification cannot overwrite that frame.
        StackClipProgressProperty.Changed.AddClassHandler<Control>(static (control, _) =>
            UpdateStackClipGeometry(control, control.GetValue(StackClipProgressProperty)));
        StackClipIsBottomProperty.Changed.AddClassHandler<Control>(static (control, _) =>
            UpdateStackClipGeometry(control, control.GetValue(StackClipProgressProperty)));

        AffectsMeasure<FeedbackStackPanel>(
            StackModeProperty,
            PositionProperty,
            IsStackEnabledProperty,
            StackThresholdProperty,
            IsStackExpandedProperty,
            ExpandedGapProperty,
            CollapsedOffsetProperty);
        AffectsArrange<FeedbackStackPanel>(
            StackModeProperty,
            PositionProperty,
            IsStackEnabledProperty,
            StackThresholdProperty,
            IsStackExpandedProperty,
            IsMotionEnabledProperty,
            ExpandedGapProperty,
            CollapsedOffsetProperty);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var activeCount = 0;
        _collapsedCardSize = default;
        for (var i = 0; i < Children.Count; i++)
        {
            var child = Children[i];
            child.Measure(availableSize);
            if (IsActive(child))
            {
                activeCount++;
                // Backplates track the latest card even while the stack is expanded.
                _collapsedCardSize = child.DesiredSize;
            }
        }

        _isCollapsed = IsStackEnabled && !IsStackExpanded && activeCount > Math.Max(1, StackThreshold);
        if (activeCount == 0)
        {
            _owner?.ReportLayoutState(_isCollapsed, _collapsedCardSize, activeCount);
            return default;
        }

        var desiredSize = _isCollapsed
            ? MeasureCollapsed(activeCount)
            : MeasureExpanded(activeCount);
        _owner?.ReportLayoutState(_isCollapsed, _collapsedCardSize, activeCount);
        return desiredSize;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var activeCount = CountActiveChildren();
        _isCollapsed = IsStackEnabled && !IsStackExpanded && activeCount > Math.Max(1, StackThreshold);
        var isCollapseTransition = _hasArrangedProjection &&
                                   !_wasArrangedCollapsed &&
                                   _isCollapsed &&
                                   IsMotionEnabled &&
                                   StackMode == FeedbackStackMode.Notification;

        if (!_isCollapsed || !IsMotionEnabled || StackMode != FeedbackStackMode.Notification)
        {
            ReleaseAllTransitionSnapshots();
        }

        if (_isCollapsed)
        {
            ArrangeCollapsed(finalSize, activeCount, isCollapseTransition);
        }
        else
        {
            ArrangeExpanded(finalSize, activeCount);
        }

        _hasArrangedProjection = true;
        _wasArrangedCollapsed = _isCollapsed;

        return finalSize;
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _owner = this.FindAncestorOfType<FeedbackStackPresenter>();
        _owner?.AttachPanel(this);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        ReleaseAllTransitionSnapshots();
        _hasArrangedProjection = false;
        _wasArrangedCollapsed = false;
        _owner?.DetachPanel(this);
        _owner = null;
        base.OnDetachedFromVisualTree(e);
    }

    private Size MeasureExpanded(int activeCount)
    {
        var width = 0d;
        var height = 0d;
        for (var i = 0; i < Children.Count; i++)
        {
            var child = Children[i];
            if (!IsActive(child))
            {
                continue;
            }

            width = Math.Max(width, child.DesiredSize.Width);
            height += child.DesiredSize.Height;
        }

        height += Math.Max(0, ExpandedGap) * Math.Max(0, activeCount - 1);
        return new Size(width, height);
    }

    private Size MeasureCollapsed(int activeCount)
    {
        var threshold = Math.Max(1, StackThreshold);
        var projectedCount = StackMode == FeedbackStackMode.Message ? 1 : Math.Min(threshold, activeCount);
        var visibleCount = StackMode == FeedbackStackMode.Message ? 1 : Math.Min(3, projectedCount);
        var width = 0d;
        var farEdge = 0d;
        var depth = 0;
        var offset = Math.Max(0, CollapsedOffset);

        for (var i = Children.Count - 1; i >= 0 && depth < projectedCount; i--)
        {
            var child = Children[i];
            if (!IsActive(child))
            {
                continue;
            }

            if (depth < visibleCount)
            {
                width = Math.Max(width, child.DesiredSize.Width);
            }

            farEdge = depth == 0 ? child.DesiredSize.Height : farEdge + offset;
            depth++;
        }

        if (StackMode == FeedbackStackMode.Message)
        {
            farEdge += 2 * offset;
        }

        return new Size(width, farEdge);
    }

    private void ArrangeExpanded(Size finalSize, int activeCount)
    {
        var isBottom = IsBottomPosition(Position);
        var gap = Math.Max(0, ExpandedGap);
        var edge = isBottom ? finalSize.Height : 0d;
        var useStableAnchor = IsMotionEnabled;
        var arranged = 0;

        for (var step = 0; step < Children.Count; step++)
        {
            var index = Children.Count - 1 - step;
            var child = Children[index];
            if (!IsActive(child))
            {
                ArrangeInactive(child, finalSize, isBottom);
                continue;
            }

            var size = child.DesiredSize;
            var targetY = isBottom ? edge - size.Height : edge;
            var edgeOffset = isBottom ? finalSize.Height - targetY - size.Height : targetY;
            SetProjection(
                child,
                finalSize,
                scale: 1,
                edgeOffset,
                isBottom,
                isVisible: true,
                zIndex: activeCount - arranged,
                useStableAnchor,
                useStackClip: StackMode == FeedbackStackMode.Notification && IsStackEnabled,
                clipBackLayer: false);
            edge = isBottom ? targetY - gap : targetY + size.Height + gap;
            arranged++;
        }
    }

    private void ArrangeCollapsed(Size finalSize, int activeCount, bool isCollapseTransition)
    {
        var isBottom = IsBottomPosition(Position);
        var threshold = Math.Max(1, StackThreshold);
        var visibleCount = StackMode == FeedbackStackMode.Message
            ? 1
            : Math.Min(3, Math.Min(threshold, activeCount));
        var offset = Math.Max(0, CollapsedOffset);
        var gap = Math.Max(0, ExpandedGap);
        var useStableAnchor = IsMotionEnabled;
        var expandedEdge = isBottom ? finalSize.Height : 0d;
        var farEdge = 0d;
        var depth = 0;

        for (var i = Children.Count - 1; i >= 0; i--)
        {
            var child = Children[i];
            if (!IsActive(child))
            {
                ArrangeInactive(child, finalSize, isBottom);
                continue;
            }

            var size = child.DesiredSize;
            var expandedTargetY = isBottom ? expandedEdge - size.Height : expandedEdge;
            expandedEdge = isBottom
                ? expandedTargetY - gap
                : expandedTargetY + size.Height + gap;
            var layerDepth = StackMode == FeedbackStackMode.Message ? 0 : Math.Min(depth, 2);
            var scale = StackMode == FeedbackStackMode.Notification ? 1 - layerDepth * 0.06 : 1;
            var isVisibleLayer = depth < visibleCount;
            double edgeOffset;
            if (StackMode == FeedbackStackMode.Message)
            {
                edgeOffset = !isVisibleLayer && !useStableAnchor
                    ? isBottom
                        ? finalSize.Height - expandedTargetY - size.Height
                        : expandedTargetY
                    : 0;
            }
            else
            {
                edgeOffset = depth == 0 ? 0 : farEdge + offset - size.Height;
                farEdge = edgeOffset + size.Height;
            }

            SetProjection(
                child,
                finalSize,
                scale,
                edgeOffset,
                isBottom,
                isVisibleLayer,
                Math.Max(0, visibleCount - depth),
                useStableAnchor,
                useStackClip: StackMode == FeedbackStackMode.Notification,
                clipBackLayer: StackMode == FeedbackStackMode.Notification && depth > 0,
                captureCollapseSnapshot: isCollapseTransition && isVisibleLayer);
            depth++;
        }
    }

    private int CountActiveChildren()
    {
        var count = 0;
        for (var i = 0; i < Children.Count; i++)
        {
            if (IsActive(Children[i]))
            {
                count++;
            }
        }
        return count;
    }

    private static bool IsActive(Control child)
    {
        return child is not IFeedbackStackItem item || (!item.IsClosing && !item.IsClosed);
    }

    private void ArrangeInactive(Control child, Size finalSize, bool isBottom)
    {
        ReleaseTransitionSnapshot(child);
        if (child is IFeedbackStackItem { IsClosing: true, IsClosed: false } &&
            _projections.TryGetValue(child, out var projection) &&
            projection.HasLayout)
        {
            var size = child.DesiredSize;
            var anchorY = isBottom ? finalSize.Height - size.Height : 0d;
            child.Arrange(new Rect(
                ResolveX(finalSize.Width, size.Width),
                IsMotionEnabled
                    ? anchorY
                    : anchorY + (isBottom ? -projection.EdgeOffset : projection.EdgeOffset),
                size.Width,
                size.Height));
            SetTransform(
                child,
                projection.Scale,
                IsMotionEnabled ? (isBottom ? -projection.EdgeOffset : projection.EdgeOffset) : 0,
                isBottom,
                projection);
            SetVisibility(child, projection.IsVisible, isHitTestVisible: false);
            child.ZIndex = projection.ZIndex;
            return;
        }

        SetVisibility(child, false);
        child.Clip = null;
        var inactiveProjection = GetProjection(child);
        SetTransform(child, 1, 0, isBottom, inactiveProjection);
    }

    private void SetProjection(
        Control child,
        Size finalSize,
        double scale,
        double edgeOffset,
        bool isBottom,
        bool isVisible,
        int zIndex,
        bool useStableAnchor,
        bool useStackClip,
        bool clipBackLayer,
        bool captureCollapseSnapshot = false)
    {
        var size = child.DesiredSize;
        var projection = GetProjection(child);
        var projectionChanged = projection.HasLayout &&
                                (Math.Abs(projection.Scale - scale) >= 0.0001 ||
                                 Math.Abs(projection.EdgeOffset - edgeOffset) >= 0.0001);
        projection.HasLayout = true;
        projection.Scale = scale;
        projection.EdgeOffset = edgeOffset;
        projection.IsVisible = isVisible;
        projection.ZIndex = zIndex;

        var translateY = useStableAnchor ? (isBottom ? -edgeOffset : edgeOffset) : 0;
        var targetTransform = ResolveTransform(scale, translateY, projection);
        var snapshotItem = captureCollapseSnapshot && projectionChanged
            ? child as IFeedbackStackTransitionSnapshotItem
            : null;
        var snapshotStarted = snapshotItem?.TryBeginStackCollapseSnapshot() == true;

        var anchorY = isBottom ? finalSize.Height - size.Height : 0d;
        child.Arrange(new Rect(
            ResolveX(finalSize.Width, size.Width),
            useStableAnchor ? anchorY : anchorY + (isBottom ? -edgeOffset : edgeOffset),
            size.Width,
            size.Height));
        SetTransform(child, targetTransform, isBottom);
        SetVisibility(child, isVisible);
        SetClip(child, projection, useStackClip, clipBackLayer, isBottom, size, IsMotionEnabled);
        child.ZIndex = zIndex;
        if (snapshotStarted)
        {
            _hasActiveTransitionSnapshots = true;
            snapshotItem!.ArmStackCollapseSnapshot(targetTransform);
        }
    }

    private static void SetVisibility(Control child, bool isVisible, bool? isHitTestVisible = null)
    {
        if (child is IFeedbackStackItem item)
        {
            item.IsStackVisible = isVisible;
        }
        child.Opacity = isVisible ? 1 : 0;
        child.IsHitTestVisible = isHitTestVisible ?? isVisible;
    }

    private CachedProjection GetProjection(Control child)
    {
        return _projections.GetValue(child, static _ => new CachedProjection());
    }

    private static void SetClip(
        Control child,
        CachedProjection projection,
        bool useStackClip,
        bool clipBackLayer,
        bool isBottom,
        Size size,
        bool isMotionEnabled)
    {
        if (!useStackClip)
        {
            if (child.Clip is not null)
            {
                child.Clip = null;
            }
            projection.HasClipGeometryState = false;
            if (isMotionEnabled)
            {
                projection.IsClipMotionEnabled = true;
                SetStackClipProgress(child, projection, 0);
            }
            else
            {
                projection.IsClipMotionEnabled = false;
                projection.ClipProgress = 0;
                if (child.GetValue(StackClipProgressProperty) != 0)
                {
                    child.SetValue(StackClipProgressProperty, 0);
                }
            }
            return;
        }

        var targetProgress = clipBackLayer ? 0.5 : 0;
        if (projection.ClipGeometry is null)
        {
            var initialProgress = isMotionEnabled
                ? child.GetValue(StackClipProgressProperty)
                : targetProgress;
            projection.ClipGeometry = new RectangleGeometry(
                CreateStackClipRect(size, isBottom, initialProgress));
            if (!isMotionEnabled)
            {
                projection.ClipProgress = targetProgress;
            }
            projection.ClipSize = size;
            projection.ClipIsBottom = isBottom;
            projection.HasClipGeometryState = true;
        }
        if (!ReferenceEquals(child.Clip, projection.ClipGeometry))
        {
            child.Clip = projection.ClipGeometry;
        }

        if (child.GetValue(StackClipIsBottomProperty) != isBottom)
        {
            child.SetCurrentValue(StackClipIsBottomProperty, isBottom);
        }

        if (!isMotionEnabled)
        {
            projection.IsClipMotionEnabled = false;
            if (!projection.HasClipGeometryState ||
                projection.ClipSize != size ||
                projection.ClipIsBottom != isBottom ||
                Math.Abs(projection.ClipProgress - targetProgress) >= 0.0001)
            {
                projection.ClipGeometry.Rect = CreateStackClipRect(size, isBottom, targetProgress);
            }
            projection.ClipProgress = targetProgress;
            projection.ClipSize = size;
            projection.ClipIsBottom = isBottom;
            projection.HasClipGeometryState = true;
            // Keep the future animation base aligned with the immediate geometry. Otherwise
            // re-enabling motion can replay an old collapsed clip over an expanded card.
            if (child.GetValue(StackClipProgressProperty) != targetProgress)
            {
                child.SetValue(StackClipProgressProperty, targetProgress);
            }
            return;
        }

        if (!projection.IsClipMotionEnabled)
        {
            projection.IsClipMotionEnabled = true;
            projection.ClipProgress = -1;
        }
        if (!projection.HasClipGeometryState ||
            projection.ClipSize != size ||
            projection.ClipIsBottom != isBottom)
        {
            projection.ClipGeometry.Rect = CreateStackClipRect(
                size,
                isBottom,
                child.GetValue(StackClipProgressProperty));
        }
        projection.ClipSize = size;
        projection.ClipIsBottom = isBottom;
        projection.HasClipGeometryState = true;
        SetStackClipProgress(child, projection, targetProgress);
    }

    private static void SetStackClipProgress(Control child, CachedProjection projection, double progress)
    {
        if (Math.Abs(projection.ClipProgress - progress) < 0.0001)
        {
            return;
        }

        projection.ClipProgress = progress;
        // The panel owns this internal target. SetCurrentValue only overrides the current
        // value entry; replacing that entry with a transition can restore the old default.
        child.SetValue(StackClipProgressProperty, progress);
    }

    private static void UpdateStackClipGeometry(Control control, double progress)
    {
        if (control.Clip is not RectangleGeometry geometry)
        {
            return;
        }

        geometry.Rect = CreateStackClipRect(
            control.Bounds.Size,
            control.GetValue(StackClipIsBottomProperty),
            progress);
    }

    private static Rect CreateStackClipRect(Size size, bool isBottom, double progress)
    {
        var interpolation = Math.Clamp(progress, 0, 0.5) * 2;
        var fullTop = -CollapsedClipOverflow;
        var fullBottom = size.Height + CollapsedClipOverflow;
        var collapsedEdge = size.Height / 2;
        var top = isBottom
            ? fullTop
            : fullTop + (collapsedEdge - fullTop) * interpolation;
        var bottom = isBottom
            ? fullBottom + (collapsedEdge - fullBottom) * interpolation
            : fullBottom;
        return new Rect(
            -CollapsedClipOverflow,
            top,
            size.Width + 2 * CollapsedClipOverflow,
            bottom - top);
    }

    private static void SetTransform(
        Control child,
        double scale,
        double translateY,
        bool isBottom,
        CachedProjection projection)
    {
        var transform = ResolveTransform(scale, translateY, projection);
        SetTransform(child, transform, isBottom);
    }

    private static void SetTransform(Control child, ITransform transform, bool isBottom)
    {
        if (!ReferenceEquals(child.RenderTransform, transform))
        {
            child.RenderTransform = transform;
        }
        child.RenderTransformOrigin = new RelativePoint(0.5, isBottom ? 0 : 1, RelativeUnit.Relative);
    }

    private static ITransform ResolveTransform(double scale, double translateY, CachedProjection projection)
    {
        return Math.Abs(scale - 1) < 0.0001 && Math.Abs(translateY) < 0.0001
            ? FullTransform
            : projection.GetOrCreateTransform(scale, translateY);
    }

    private void ReleaseAllTransitionSnapshots()
    {
        if (!_hasActiveTransitionSnapshots)
        {
            return;
        }

        for (var i = 0; i < Children.Count; i++)
        {
            ReleaseTransitionSnapshot(Children[i]);
        }
        _hasActiveTransitionSnapshots = false;
    }

    private static void ReleaseTransitionSnapshot(Control child)
    {
        (child as IFeedbackStackTransitionSnapshotItem)?.ReleaseStackTransitionSnapshot();
    }

    private static ITransform BuildTransform(double scale, double translateY)
    {
        var builder = new TransformOperations.Builder(2);
        builder.AppendScale(scale, scale);
        builder.AppendTranslate(0, translateY);
        return builder.Build();
    }

    private double ResolveX(double finalWidth, double childWidth)
    {
        return Position switch
        {
            NotificationPosition.TopRight or NotificationPosition.BottomRight => finalWidth - childWidth,
            NotificationPosition.TopCenter or NotificationPosition.BottomCenter => (finalWidth - childWidth) / 2,
            _ => 0
        };
    }

    private static bool IsBottomPosition(NotificationPosition position)
    {
        return position is NotificationPosition.BottomLeft or
            NotificationPosition.BottomCenter or
            NotificationPosition.BottomRight;
    }

    private sealed class CachedProjection
    {
        private double _primaryScale;
        private double _primaryTranslateY;
        private ITransform? _primaryTransform;
        private double _secondaryScale;
        private double _secondaryTranslateY;
        private ITransform? _secondaryTransform;

        internal bool HasLayout { get; set; }
        internal double Scale { get; set; }
        internal double EdgeOffset { get; set; }
        internal bool IsVisible { get; set; }
        internal int ZIndex { get; set; }
        internal double ClipProgress { get; set; } = -1;
        internal bool IsClipMotionEnabled { get; set; }
        internal RectangleGeometry? ClipGeometry { get; set; }
        internal Size ClipSize { get; set; }
        internal bool ClipIsBottom { get; set; }
        internal bool HasClipGeometryState { get; set; }

        internal ITransform GetOrCreateTransform(double scale, double translateY)
        {
            if (_primaryTransform is not null && Matches(_primaryScale, _primaryTranslateY, scale, translateY))
            {
                return _primaryTransform;
            }

            if (_secondaryTransform is not null &&
                Matches(_secondaryScale, _secondaryTranslateY, scale, translateY))
            {
                return _secondaryTransform;
            }

            var next = BuildTransform(scale, translateY);
            if (_secondaryTransform is not null)
            {
                _primaryScale = _secondaryScale;
                _primaryTranslateY = _secondaryTranslateY;
                _primaryTransform = _secondaryTransform;
            }
            _secondaryScale = scale;
            _secondaryTranslateY = translateY;
            _secondaryTransform = next;
            return next;
        }

        private static bool Matches(double cachedScale, double cachedTranslateY, double scale, double translateY)
        {
            return Math.Abs(cachedScale - scale) < 0.0001 &&
                   Math.Abs(cachedTranslateY - translateY) < 0.0001;
        }
    }
}
