using AtomUI.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.VisualTree;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Data;
using Avalonia.Styling;
using System.Collections.Specialized;
namespace AtomUI.Desktop.Controls;

/// <summary>
/// Internal layout engine for <see cref="Masonry"/>. Derives from <see cref="Panel"/> and is
/// assembled as the <c>ItemsPanel</c> by the Masonry control theme. Not exposed to developers.
/// </summary>
/// <remarks>
/// The layout properties below mirror those on <see cref="Masonry"/> and are populated via
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
    private int[]? _lastColumns;
    private bool[]? _lastFullSpans;
    private bool _hasPublishedLayout;
    private readonly Dictionary<Control, int> _stableColumns = new(ReferenceEqualityComparer.Instance);
    private int _stableColumnCount;
    private bool _hasStableAssignments;
    private MediaBreakPoint? _breakPoint;
    private IMediaBreakAwareControl? _mediaOwner;

    // antd motionEaseOut == cubic-bezier(0.215, 0.61, 0.355, 1) == easeOutCubic == Avalonia
    // CubicEaseOut（与 DrawerContainer 默认动效缓动一致）。
    private static readonly CubicEaseOut MotionEasing = new();

    private readonly Dictionary<Control, Rect> _lastVisualRects = new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<Control, ItemMotionState> _itemMotions = new(ReferenceEqualityComparer.Instance);
    private Masonry? _motionOwner;

    private sealed class ItemMotionState
    {
        public CancellationTokenSource Cancellation = new();
        public bool IsGlide;
    }

    /// <summary>被移除子项及其最后视觉矩形（面板坐标空间）。</summary>
    internal readonly record struct MasonryRemovedItem(Control Container, Rect Rect);

    /// <summary>
    /// 在 base 同步 VisualChildren 之前处理动效语义：
    /// Add 先释放 ghost 托管（否则容器仍有 ghost host 视觉父级，base 的
    /// VisualChildren 插入会抛"already has a visual parent"）；
    /// Remove/Replace 在 base 之后收集被移除子项并上报 Masonry 托管淡出。
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
        // RenderTransform（ITransform?）默认无关键帧插值器（见 MasonryItemTransformAnimator 说明）
        Animation.RegisterCustomAnimator<Avalonia.Media.ITransform?, MasonryInternal.MasonryItemTransformAnimator>();
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
        foreach (var state in _itemMotions.Values)
        {
            state.Cancellation.Cancel();
        }
        _itemMotions.Clear();
        _lastVisualRects.Clear();
        _motionOwner = null;
        _hasMeasuredLayout = false;
        _measuredLayout = null;
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
        InvalidateMeasure();
    }

    internal void InvalidateStableAssignments()
    {
        ClearStableAssignments();
        InvalidateMeasure();
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var layout = CalculateLayout(availableSize.Width, measureChildren: true);
        _measuredLayout = layout;
        _measuredEffectiveWidth = layout.Width;
        _hasMeasuredLayout = true;
        PublishLayout(layout);
        return new Size(layout.Width, layout.Height);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var effectiveWidth = ResolveEffectiveWidth(finalSize.Width);
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

    // 动效偏移来自旧/新 Arrange 矩形（运行时布局状态），ControlTheme 无法表达，故在面板代码驱动；
    // 时长值由 ControlTheme 从 token Setter 提供（见 MasonryTheme.axaml）。
    private void ApplyItemMotion(Control child, Rect visualRect)
    {
        if (_motionOwner is null)
        {
            return;
        }

        if (!_lastVisualRects.TryGetValue(child, out var previousRect))
        {
            // First arrange: appear fade-in only, no glide (antd motionAppear without position transition).
            _lastVisualRects[child] = visualRect;
            StartAppearMotion(child);
            return;
        }

        _lastVisualRects[child] = visualRect;
        var offset = previousRect.TopLeft - visualRect.TopLeft;
        if (offset != default && !_itemMotions.ContainsKey(child))
        {
            // Existing item repositioned: glide only, no fade (antd `&:not(item-fade)` transition).
            StartGlideMotion(child, offset);
        }
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
        IDisposable? preset = null;
        try
        {
            // 预置起始值（动画优先级），避免首帧闪烁。注意释放方式：AvaloniaObject.SetValue 对
            // 非 LocalValue 优先级的 UnsetValue 是静默忽略的，必须 Dispose 返回的句柄才能回落基值。
            preset = child.SetValue(Visual.OpacityProperty, 0d, BindingPriority.Animation);
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
            if (_itemMotions.TryGetValue(child, out var current) && ReferenceEquals(current, state))
            {
                _itemMotions.Remove(child);
            }
            preset?.Dispose();
        }
    }

    private void StartGlideMotion(Control child, Vector offset)
    {
        var duration = MotionDuration;
        if (!child.IsVisible || duration <= TimeSpan.Zero)
        {
            return;
        }

        if (_itemMotions.TryGetValue(child, out var existing))
        {
            if (!existing.IsGlide)
            {
                // 入场淡入激活中的项不做位置过渡（antd `-fade` 项语义）
                return;
            }
            // 滑动中再次变位：先冻结当前视觉平移（在取消前读取），再取消旧动画续滑
            var resumeOffset = ReadCurrentTranslate(child);
            existing.Cancellation.Cancel();
            existing.Cancellation = new CancellationTokenSource();
            _ = RunGlideMotionAsync(child, duration, existing, resumeOffset);
            return;
        }

        var state = new ItemMotionState { IsGlide = true };
        _itemMotions[child] = state;
        _ = RunGlideMotionAsync(child, duration, state, offset);
    }

    private async Task RunGlideMotionAsync(Control child, TimeSpan duration, ItemMotionState state,
        Vector startOffset)
    {
        IDisposable? preset = null;
        try
        {
            // 预置旧位置偏移（动画优先级），布局矩形已是最终位置；结束 Dispose 释放回落用户基值
            if (startOffset != default)
            {
                preset = child.SetValue(Visual.RenderTransformProperty, BuildTranslate(startOffset),
                    BindingPriority.Animation);
            }
            var animation = new Animation
            {
                Duration = duration,
                Easing   = MotionEasing,
                Children =
                {
                    new KeyFrame
                    {
                        Cue = new Cue(0d),
                        Setters = { new Setter(Visual.RenderTransformProperty, BuildTranslate(startOffset)) }
                    },
                    new KeyFrame
                    {
                        Cue = new Cue(1d),
                        Setters = { new Setter(Visual.RenderTransformProperty, BuildTranslate(default)) }
                    },
                }
            };
            await animation.RunAsync(child, state.Cancellation.Token);
        }
        catch (OperationCanceledException) { }
        finally
        {
            if (_itemMotions.TryGetValue(child, out var current) && ReferenceEquals(current, state))
            {
                _itemMotions.Remove(child);
            }
            // 取消续滑（有新动画接管属性）时不释放，避免把新动画刚预置的偏移清掉
            if (!state.Cancellation.IsCancellationRequested)
            {
                preset?.Dispose();
            }
        }
    }

    private static Avalonia.Media.Transformation.TransformOperations BuildTranslate(Vector offset)
    {
        var builder = new Avalonia.Media.Transformation.TransformOperations.Builder(1);
        builder.AppendTranslate(offset.X, offset.Y);
        return builder.Build();
    }

    private static Vector ReadCurrentTranslate(Control child)
    {
        if (child.RenderTransform is Avalonia.Media.ITransform transform)
        {
            var matrix = transform.Value;
            return new Vector(matrix.M31, matrix.M32);
        }
        return default;
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
