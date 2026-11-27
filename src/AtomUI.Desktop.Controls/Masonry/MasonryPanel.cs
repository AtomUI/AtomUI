using AtomUI.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.VisualTree;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Styling;
using System.Collections.Specialized;
namespace AtomUI.Desktop.Controls;

/// <summary>
/// Internal layout engine for <see cref="Masonry" />. Derives from <see cref="Panel" /> and is
/// assembled as the <c>ItemsPanel</c> by the Masonry control theme. Not exposed to developers.
/// </summary>
/// <remarks>
/// The layout properties below mirror those on <see cref="Masonry" /> and are populated via
/// RelativeSource binding in the control theme. This keeps the engine decoupled from the
/// owner control and lets it be assembled purely in XAML.
/// </remarks>
internal class MasonryPanel : Panel
{
    #region 公共属性定义

    public static readonly StyledProperty<int> ColumnCountProperty =
        Masonry.ColumnCountProperty.AddOwner<MasonryPanel>();

    public static readonly StyledProperty<ResponsiveInt?> ColumnInfoProperty =
        Masonry.ColumnInfoProperty.AddOwner<MasonryPanel>();

    public static readonly StyledProperty<double> MinColumnWidthProperty =
        Masonry.MinColumnWidthProperty.AddOwner<MasonryPanel>();

    public static readonly StyledProperty<int> MaxColumnCountProperty =
        Masonry.MaxColumnCountProperty.AddOwner<MasonryPanel>();

    public static readonly StyledProperty<double> ColumnGapProperty =
        Masonry.ColumnGapProperty.AddOwner<MasonryPanel>();

    public static readonly StyledProperty<double> RowGapProperty =
        Masonry.RowGapProperty.AddOwner<MasonryPanel>();

    public static readonly StyledProperty<ResponsiveGutter?> GutterProperty =
        Masonry.GutterProperty.AddOwner<MasonryPanel>();

    public static readonly StyledProperty<MasonryLayoutStrategy> LayoutStrategyProperty =
        Masonry.LayoutStrategyProperty.AddOwner<MasonryPanel>();

    public static readonly StyledProperty<TimeSpan> MotionDurationProperty =
        Masonry.MotionDurationProperty.AddOwner<MasonryPanel>();

    public static readonly StyledProperty<TimeSpan> LeaveMotionDurationProperty =
        Masonry.LeaveMotionDurationProperty.AddOwner<MasonryPanel>();

    public int ColumnCount
    {
        get => GetValue(ColumnCountProperty);
        set => SetValue(ColumnCountProperty, value);
    }

    public ResponsiveInt? ColumnInfo
    {
        get => GetValue(ColumnInfoProperty);
        set => SetValue(ColumnInfoProperty, value);
    }

    public double MinColumnWidth
    {
        get => GetValue(MinColumnWidthProperty);
        set => SetValue(MinColumnWidthProperty, value);
    }

    public int MaxColumnCount
    {
        get => GetValue(MaxColumnCountProperty);
        set => SetValue(MaxColumnCountProperty, value);
    }

    public double ColumnGap
    {
        get => GetValue(ColumnGapProperty);
        set => SetValue(ColumnGapProperty, value);
    }

    public double RowGap
    {
        get => GetValue(RowGapProperty);
        set => SetValue(RowGapProperty, value);
    }

    public ResponsiveGutter? Gutter
    {
        get => GetValue(GutterProperty);
        set => SetValue(GutterProperty, value);
    }

    public MasonryLayoutStrategy LayoutStrategy
    {
        get => GetValue(LayoutStrategyProperty);
        set => SetValue(LayoutStrategyProperty, value);
    }

    public TimeSpan MotionDuration
    {
        get => GetValue(MotionDurationProperty);
        set => SetValue(MotionDurationProperty, value);
    }

    public TimeSpan LeaveMotionDuration
    {
        get => GetValue(LeaveMotionDurationProperty);
        set => SetValue(LeaveMotionDurationProperty, value);
    }

    #endregion

    private List<Rect> _arrangeRects = new();
    private List<int> _arrangeColumns = new();
    private List<bool> _arrangeFullSpans = new();
    private MasonryLayout? _measuredLayout;
    private double _measuredEffectiveWidth;
    private bool _hasMeasuredLayout;
    // The width passed to the last Measure. When it differs from the current one, the arranged width below is
    // dropped, so that the layout is calculated from the new constraint (see MeasureOverride).
    private double _previousAvailableWidth = double.NaN;
    // The effective width of the last Arrange, reused by Measure while the Measure input width stays unchanged.
    private double? _arrangedEffectiveWidth;
    private int[]? _lastColumns;
    private bool[]? _lastFullSpans;
    private bool _hasPublishedLayout;
    private readonly Dictionary<Control, int> _stableColumns = new(ReferenceEqualityComparer.Instance);
    private int _stableColumnCount;
    private bool _hasStableAssignments;
    private MediaBreakPoint? _breakPoint;
    private IMediaBreakAwareControl? _mediaOwner;

    // antd motionEaseOut == cubic-bezier(0.215, 0.61, 0.355, 1) == easeOutCubic == Avalonia CubicEaseOut, the
    // same easing as the default motion of DrawerContainer. Used for the appear fade of new items.
    private static readonly CubicEaseOut MotionEasing = new();

    private readonly Dictionary<Control, Rect> _lastVisualRects = new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<Control, ItemMotionState> _itemMotions = new(ReferenceEqualityComparer.Instance);
    private Masonry? _motionOwner;
    // The viewport reported by the last EffectiveViewportChanged, in panel coordinates.
    // null means it is not known yet, and everything is then treated as visible.
    private Rect? _motionViewport;

    // Each appear fade runs on its own state. Normal completion, cancellation, viewport culling and detaching all
    // release the preset value handle through the same path.
    private sealed class ItemMotionState
    {
        public readonly CancellationTokenSource Cancellation = new();
        public IDisposable? Preset;
    }

    /// <summary>
    /// A removed child and its last visual rectangle, in panel coordinates.
    /// </summary>
    internal readonly record struct MasonryRemovedItem(Control Container, Rect Rect);

    /// <summary>
    /// Handles the motion semantics around the synchronization of the visual children by the base member.
    /// Add releases the ghost hosting first: a container still hosted by a ghost host already has a visual parent,
    /// and inserting it into the visual children throws "already has a visual parent".
    /// Remove and Replace collect the removed children after the base call and report them to
    /// <see cref="Masonry" /> for the fade-out.
    /// </summary>
    protected override void ChildrenChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        List<MasonryRemovedItem>? removed = null;
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                if (e.NewItems is not null)
                {
                    foreach (Control child in e.NewItems)
                    {
                        _motionOwner?.TryReleaseMotionGhost(child);
                    }
                }
                break;
            case NotifyCollectionChangedAction.Remove:
                base.ChildrenChanged(sender, e);
                if (e.OldItems is not null)
                {
                    foreach (Control child in e.OldItems)
                    {
                        CollectRemovedChild(child, ref removed);
                    }
                }
                NotifyRemovedToOwner(removed);
                return;
            case NotifyCollectionChangedAction.Replace:
                foreach (Control child in e.NewItems!)
                {
                    _motionOwner?.TryReleaseMotionGhost(child);
                }
                base.ChildrenChanged(sender, e);
                if (e.OldItems is not null)
                {
                    foreach (Control child in e.OldItems)
                    {
                        CollectRemovedChild(child, ref removed);
                    }
                }
                NotifyRemovedToOwner(removed);
                return;
        }
        base.ChildrenChanged(sender, e);
    }

    private void CollectRemovedChild(Control child, ref List<MasonryRemovedItem>? removed)
    {
        if (_lastVisualRects.TryGetValue(child, out var rect))
        {
            (removed ??= new List<MasonryRemovedItem>()).Add(new MasonryRemovedItem(child, rect));
            _lastVisualRects.Remove(child);
        }
    }

    private void NotifyRemovedToOwner(List<MasonryRemovedItem>? removed)
    {
        if (removed is { Count: > 0 } && _motionOwner is not null && this.GetVisualRoot() is not null)
        {
            _motionOwner.NotifyItemsRemoved(this, removed);
        }
    }

    static MasonryPanel()
    {
        AffectsMeasure<MasonryPanel>(
            ColumnCountProperty,
            ColumnInfoProperty,
            MinColumnWidthProperty,
            MaxColumnCountProperty,
            ColumnGapProperty,
            RowGapProperty,
            GutterProperty,
            LayoutStrategyProperty);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _motionOwner = this.FindAncestorOfType<Masonry>();
        if (_motionOwner is not null)
        {
            // One panel-level subscription instead of one per item: it limits item motion to
            // the area near the viewport.
            EffectiveViewportChanged += HandleEffectiveViewportChanged;
        }
        if (MediaQueryHost.FindOwner(this) is { } mediaOwner)
        {
            _mediaOwner = mediaOwner;
            _breakPoint = mediaOwner.MediaBreakPoint;
            mediaOwner.MediaBreakPointChanged += HandleMediaBreakChanged;
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == LayoutStrategyProperty)
        {
            ClearStableAssignments();
        }
        if (change.Property == FlowDirectionProperty)
        {
            // Mirror direction changed: re-arrange existing children at their mirrored rects.
            InvalidateArrange();
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        foreach (var state in _itemMotions.Values.ToArray())
        {
            state.Cancellation.Cancel();
        }
        _itemMotions.Clear();
        _lastVisualRects.Clear();
        EffectiveViewportChanged -= HandleEffectiveViewportChanged;
        _motionViewport = null;
        _motionOwner = null;
        _hasMeasuredLayout = false;
        _measuredLayout = null;
        _previousAvailableWidth = double.NaN;
        _arrangedEffectiveWidth = null;
        ClearStableAssignments();
        if (_mediaOwner != null)
        {
            _mediaOwner.MediaBreakPointChanged -= HandleMediaBreakChanged;
            _mediaOwner = null;
        }
    }

    private void HandleMediaBreakChanged(object? sender, MediaBreakPointChangedEventArgs args)
    {
        _breakPoint = args.MediaBreakPoint;
        _hasMeasuredLayout = false;
        _measuredLayout = null;
        _arrangedEffectiveWidth = null;
        InvalidateMeasure();
    }

    internal void InvalidateStableAssignments()
    {
        ClearStableAssignments();
        InvalidateMeasure();
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        // The Arrange width can differ from the Measure constraint: layout rounds sizes up, or a host measures with
        // an infinite width and then arranges at the viewport. Arrange re-measures children at the final column
        // width, which changes the DesiredSize of width-dependent children and invalidates the measure of this
        // panel. If the next pass measured them with the original constraint again, their sizes would flip back
        // and forth in a permanent layout loop. So while the input width is unchanged, children are measured at
        // the last arranged width. The reported DesiredSize width is still resolved from the constraint, so that
        // a host measuring with an infinite width does not keep the old arranged width as the content extent.
        if (!_previousAvailableWidth.Equals(availableSize.Width))
        {
            _arrangedEffectiveWidth = null;
        }
        _previousAvailableWidth = availableSize.Width;

        var layout = CalculateLayout(_arrangedEffectiveWidth ?? availableSize.Width, measureChildren: true);
        _measuredLayout = layout;
        _measuredEffectiveWidth = layout.Width;
        _hasMeasuredLayout = true;
        PublishLayout(layout);
        return new Size(ResolveEffectiveWidth(availableSize.Width), layout.Height);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var effectiveWidth = ResolveEffectiveWidth(finalSize.Width);
        _arrangedEffectiveWidth = effectiveWidth;
        var canReuseMeasuredLayout = _hasMeasuredLayout &&
                                      AreClose(_measuredEffectiveWidth, effectiveWidth);
        // A ScrollViewer (and similar hosts) can measure us with an unbounded width and then
        // arrange us at the viewport width. In that case DesiredSize was produced for a different
        // column width, so arranging it without a fresh child measure leaves height-dependent
        // masonry positions stale and causes visible reflow when content reports its real size.
        var layout = canReuseMeasuredLayout
            ? _measuredLayout!.Value
            : CalculateLayout(finalSize.Width, measureChildren: true);
        _hasMeasuredLayout = false;
        _measuredLayout = null;
        PublishLayout(layout);

        // RTL mirrors the column order (antd `-rtl` + `insetInlineStart` equivalent). The
        // published rects stay in logical (LTR) space; only the arranged visual rect is mirrored.
        var isRtl = FlowDirection == Avalonia.Media.FlowDirection.RightToLeft;
        for (var i = 0; i < Children.Count; i++)
        {
            var visualRect = MirrorForRtl(_arrangeRects[i], finalSize.Width, isRtl);
            ApplyItemMotion(Children[i], visualRect);
            Children[i].Arrange(visualRect);
        }

        CommitStableAssignments(layout);
        MaybeNotifyLayoutChanged();
        return finalSize;
    }

    private static Rect MirrorForRtl(Rect rect, double width, bool isRtl)
    {
        return isRtl ? rect.WithX(width - rect.Right) : rect;
    }

    // When an item animates depends on Arrange-time state (whether this is its first arrange and whether it is
    // near the visible area), which a ControlTheme cannot express, so the panel drives the fade in code. The
    // Masonry control theme supplies the duration from a token setter.
    // A re-layout (positions changed by content height, width, column count or items added or removed) plays no
    // item animation: existing items go straight to their new positions. Drag-resizing and images that load one
    // after another re-layout continuously, and a per-item position animation would keep every item behind its
    // layout position and occupy the UI thread.
    private void ApplyItemMotion(Control child, Rect visualRect)
    {
        if (_motionOwner is null)
        {
            return;
        }

        var isFirstArrange = !_lastVisualRects.ContainsKey(child);
        _lastVisualRects[child] = visualRect;
        if (isFirstArrange && IsInMotionRange(visualRect))
        {
            // First arrange: appear fade-in only (antd motionAppear).
            StartAppearMotion(child);
        }
    }

    private void HandleEffectiveViewportChanged(object? sender, EffectiveViewportChangedEventArgs e)
    {
        _motionViewport = e.EffectiveViewport;
        if (_itemMotions.Count == 0)
        {
            return;
        }

        // The viewport is unknown at the first arrange. Once it is known, the appear fade of any item
        // far from it ends at once and the item is shown.
        foreach (var (child, state) in _itemMotions.ToArray())
        {
            if (_lastVisualRects.TryGetValue(child, out var rect) && !IsInMotionRange(rect))
            {
                state.Cancellation.Cancel();
            }
        }
    }

    /// <summary>
    /// Determines whether a visual rectangle lies in the motion range: the last effective viewport, enlarged by
    /// one viewport width on each side and one viewport height above and below. An unknown or non-finite viewport
    /// counts as visible; an empty viewport means the panel is not visible, so nothing is in range.
    /// </summary>
    private bool IsInMotionRange(Rect visualRect)
    {
        if (_motionViewport is not { } viewport ||
            !double.IsFinite(viewport.X) || !double.IsFinite(viewport.Y) ||
            !double.IsFinite(viewport.Width) || !double.IsFinite(viewport.Height))
        {
            return true;
        }
        if (viewport.Width <= 0 || viewport.Height <= 0)
        {
            return false;
        }
        return viewport.Inflate(new Thickness(viewport.Width, viewport.Height)).Intersects(visualRect);
    }

    private void StartAppearMotion(Control child)
    {
        var duration = MotionDuration;
        if (!child.IsVisible || duration <= TimeSpan.Zero)
        {
            return;
        }

        var state = new ItemMotionState();
        _itemMotions[child] = state;
        _ = RunAppearMotionAsync(child, duration, state);
    }

    private async Task RunAppearMotionAsync(Control child, TimeSpan duration, ItemMotionState state)
    {
        try
        {
            // Presets the start value at animation priority to avoid a flicker in the first frame. How the value is
            // released matters: AvaloniaObject.SetValue silently ignores UnsetValue at any priority other than
            // LocalValue, so only disposing the returned handle lets the property fall back to its base value.
            state.Preset = child.SetValue(Visual.OpacityProperty, 0d, BindingPriority.Animation);
            var animation = new Animation
            {
                Duration = duration,
                Easing   = MotionEasing,
                Children =
                {
                    new KeyFrame { Cue = new Cue(0d), Setters = { new Setter(Visual.OpacityProperty, 0d) } },
                    new KeyFrame { Cue = new Cue(1d), Setters = { new Setter(Visual.OpacityProperty, 1d) } },
                }
            };
            await animation.RunAsync(child, state.Cancellation.Token);
        }
        catch (OperationCanceledException) { }
        finally
        {
            ReleaseItemMotion(child, state);
        }
    }

    private void ReleaseItemMotion(Control child, ItemMotionState state)
    {
        if (_itemMotions.TryGetValue(child, out var current) && ReferenceEquals(current, state))
        {
            _itemMotions.Remove(child);
        }
        state.Preset?.Dispose();
        state.Preset = null;
    }

    private void PublishLayout(MasonryLayout layout)
    {
        _arrangeRects     = layout.Rects;
        _arrangeColumns   = layout.Columns;
        _arrangeFullSpans = layout.FullSpans;
    }

    private double ResolveEffectiveWidth(double availableWidth)
    {
        var breakPoint = GetBreakPoint();
        var (columnGap, _) = ResolveGaps(breakPoint);
        return ResolveAvailableWidth(availableWidth, columnGap);
    }

    private static bool AreClose(double left, double right)
    {
        return Math.Abs(left - right) < 0.01;
    }

    private MasonryLayout CalculateLayout(double availableWidth, bool measureChildren)
    {
        var breakPoint  = GetBreakPoint();
        var (columnGap, rowGap) = ResolveGaps(breakPoint);
        var width       = ResolveAvailableWidth(availableWidth, columnGap);
        var columnCount = CalculateColumnCount(width, columnGap, breakPoint);
        var columnWidth = columnCount == 1
            ? width
            : Math.Max(0, (width - columnGap * (columnCount - 1)) / columnCount);

        var columnHeights = new double[columnCount];
        var rects         = new List<Rect>(Children.Count);
        var columns       = new List<int>(Children.Count);
        var fullSpans     = new List<bool>(Children.Count);

        for (var i = 0; i < Children.Count; i++)
        {
            var child = Children[i];
            if (!child.IsVisible)
            {
                rects.Add(default);
                columns.Add(-1);
                fullSpans.Add(false);
                continue;
            }

            var isFullSpan = child.GetValue(Masonry.SpanProperty) == MasonryItemSpan.Full;
            var targetWidth = isFullSpan ? width : columnWidth;
            if (measureChildren)
            {
                child.Measure(new Size(targetWidth, double.PositiveInfinity));
            }

            var childHeight = child.DesiredSize.Height;
            if (isFullSpan)
            {
                var top = Max(columnHeights);
                var y   = top > 0 ? top + rowGap : 0;
                rects.Add(new Rect(0, y, width, childHeight));
                columns.Add(0);
                fullSpans.Add(true);
                Fill(columnHeights, y + childHeight);
            }
            else
            {
                var explicitColumn = child.GetValue(Masonry.ColumnProperty);
                var columnIndex = explicitColumn.HasValue
                    ? Math.Clamp(explicitColumn.Value, 0, columnCount - 1)
                    : ResolveColumnIndex(child, columnHeights, columnCount);
                var x = columnIndex * (columnWidth + columnGap);
                var y = columnHeights[columnIndex] > 0 ? columnHeights[columnIndex] + rowGap : 0;
                rects.Add(new Rect(x, y, columnWidth, childHeight));
                columns.Add(columnIndex);
                fullSpans.Add(false);
                columnHeights[columnIndex] = y + childHeight;
            }
        }

        return new MasonryLayout(width, Max(columnHeights), columnCount, rects, columns, fullSpans);
    }

    private double ResolveAvailableWidth(double availableWidth, double columnGap)
    {
        if (!double.IsInfinity(availableWidth))
        {
            return Math.Max(0, availableWidth);
        }

        var maxColumns   = Math.Max(1, MaxColumnCount);
        var minColumnWidth = Math.Max(1, NormalizeFinite(MinColumnWidth, 1d));
        return minColumnWidth * maxColumns + columnGap * (maxColumns - 1);
    }

    private int CalculateColumnCount(double width, double columnGap, MediaBreakPoint breakPoint)
    {
        if (ColumnInfo?.TryResolve(breakPoint, out var responsiveColumnCount) == true && responsiveColumnCount > 0)
        {
            return responsiveColumnCount;
        }

        if (ColumnCount > 0)
        {
            return ColumnCount;
        }

        var maxColumns     = Math.Max(1, MaxColumnCount);
        var minColumnWidth = Math.Max(1, NormalizeFinite(MinColumnWidth, 1d));
        var columnCount    = (int)Math.Floor((width + columnGap) / (minColumnWidth + columnGap));
        return Math.Clamp(columnCount, 1, maxColumns);
    }

    private (double ColumnGap, double RowGap) ResolveGaps(MediaBreakPoint breakPoint)
    {
        var fallback = (NormalizeGap(ColumnGap), NormalizeGap(RowGap));
        if (Gutter.HasValue && Gutter.Value.TryResolve(breakPoint, fallback, out var gutter))
        {
            return (NormalizeGap(gutter.Horizontal), NormalizeGap(gutter.Vertical));
        }

        return fallback;
    }

    private MediaBreakPoint GetBreakPoint()
    {
        if (_breakPoint.HasValue)
        {
            return _breakPoint.Value;
        }

        if (MediaQueryHost.FindOwner(this) is { } mediaOwner)
        {
            _breakPoint = mediaOwner.MediaBreakPoint;
            return _breakPoint.Value;
        }

        return MediaBreakPoint.Large;
    }

    private static double NormalizeGap(double value)
    {
        if (double.IsNaN(value) || double.IsInfinity(value) || value < 0)
        {
            return 0;
        }
        return value;
    }

    private static double NormalizeFinite(double value, double fallback)
    {
        if (double.IsNaN(value) || double.IsInfinity(value))
        {
            return fallback;
        }
        return value;
    }

    private static int IndexOfShortestColumn(double[] columnHeights)
    {
        var columnIndex = 0;
        var minHeight   = columnHeights[0];
        for (var i = 1; i < columnHeights.Length; i++)
        {
            if (columnHeights[i] < minHeight)
            {
                columnIndex = i;
                minHeight   = columnHeights[i];
            }
        }
        return columnIndex;
    }

    private int ResolveColumnIndex(Control child, double[] columnHeights, int columnCount)
    {
        var shortestColumn = IndexOfShortestColumn(columnHeights);
        if (LayoutStrategy == MasonryLayoutStrategy.Reflow ||
            !_hasStableAssignments ||
            _stableColumnCount != columnCount ||
            !_stableColumns.TryGetValue(child, out var previousColumn))
        {
            return shortestColumn;
        }

        if (previousColumn < 0 || previousColumn >= columnCount)
        {
            return shortestColumn;
        }

        return previousColumn;
    }

    private void CommitStableAssignments(MasonryLayout layout)
    {
        if (LayoutStrategy != MasonryLayoutStrategy.StableColumns)
        {
            return;
        }

        _stableColumns.Clear();
        for (var i = 0; i < Children.Count && i < layout.Columns.Count; i++)
        {
            if (!Children[i].IsVisible || layout.FullSpans[i])
            {
                continue;
            }

            var column = layout.Columns[i];
            if (column >= 0 && column < layout.ColumnCount)
            {
                _stableColumns[Children[i]] = column;
            }
        }

        _stableColumnCount = layout.ColumnCount;
        _hasStableAssignments = true;
    }

    private void ClearStableAssignments()
    {
        _stableColumns.Clear();
        _stableColumnCount = 0;
        _hasStableAssignments = false;
    }

    private static double Max(double[] values)
    {
        var max = 0d;
        foreach (var value in values)
        {
            max = Math.Max(max, value);
        }
        return max;
    }

    private static void Fill(double[] values, double value)
    {
        for (var i = 0; i < values.Length; i++)
        {
            values[i] = value;
        }
    }

    private void MaybeNotifyLayoutChanged()
    {
        if (_arrangeColumns.Count == 0 && !_hasPublishedLayout)
        {
            return;
        }

        var changed = !_hasPublishedLayout ||
                      _lastColumns is null ||
                      _lastColumns.Length != _arrangeColumns.Count ||
                      _lastFullSpans is null ||
                      _lastFullSpans.Length != _arrangeFullSpans.Count;
        if (!changed)
        {
            for (var i = 0; i < _arrangeColumns.Count; i++)
            {
                if (_arrangeColumns[i] != _lastColumns![i] ||
                    _arrangeFullSpans[i] != _lastFullSpans![i])
                {
                    changed = true;
                    break;
                }
            }
        }

        if (!changed)
        {
            return;
        }

        _lastColumns  = _arrangeColumns.ToArray();
        _lastFullSpans = _arrangeFullSpans.ToArray();
        _hasPublishedLayout = true;

        if (this.FindAncestorOfType<Masonry>() is Masonry owner)
        {
            var items = new List<MasonryItemLayout>(_arrangeColumns.Count);
            for (var i = 0; i < _arrangeColumns.Count && i < Children.Count; i++)
            {
                var col       = _arrangeColumns[i];
                var isFull    = _arrangeFullSpans[i];
                var effective = isFull ? 0 : col;
                items.Add(new MasonryItemLayout(Children[i], i, effective, isFull));
            }
            owner.NotifyLayoutChanged(items);
        }
    }

    private readonly record struct MasonryLayout(
        double Width,
        double Height,
        int ColumnCount,
        List<Rect> Rects,
        List<int> Columns,
        List<bool> FullSpans);
}
