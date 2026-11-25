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
    // 上次 Arrange 的有效宽度，仅在 Measure 输入宽度不变时复用（见 MeasureOverride 注释）。
    private double _previousAvailableWidth = double.NaN;
    private double? _arrangedEffectiveWidth;
    private int[]? _lastColumns;
    private bool[]? _lastFullSpans;
    private bool _hasPublishedLayout;
    private readonly Dictionary<Control, int> _stableColumns = new(ReferenceEqualityComparer.Instance);
    private int _stableColumnCount;
    private bool _hasStableAssignments;
    private MediaBreakPoint? _breakPoint;
    private IMediaBreakAwareControl? _mediaOwner;

    // antd motionEaseOut == cubic-bezier(0.215, 0.61, 0.355, 1) == easeOutCubic == Avalonia
    // CubicEaseOut（与 DrawerContainer 默认动效缓动一致）。用于新 item 入场淡入。
    private static readonly CubicEaseOut MotionEasing = new();

    private readonly Dictionary<Control, Rect> _lastVisualRects = new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<Control, ItemMotionState> _itemMotions = new(ReferenceEqualityComparer.Instance);
    private Masonry? _motionOwner;
    // 最近一次 EffectiveViewportChanged 报告的可见区域（面板坐标）；null 表示尚未得知，按全部可见处理。
    private Rect? _motionViewport;

    // 每次入场淡入运行一个独立状态；正常结束、取消、视口裁剪和 detached 都经同一路径释放预置句柄。
    private sealed class ItemMotionState
    {
        public readonly CancellationTokenSource Cancellation = new();
        public IDisposable? Preset;
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
            // 单个面板级订阅（非逐项）：用于把 item 动效限制在可见区域附近。
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
        // Arrange 宽度可能与 Measure 约束不同（布局取整向上取整、或无限宽度 Measure 后按视口 Arrange）。
        // Arrange 按最终列宽重测子项会改变宽度相关子项的 DesiredSize 并使本面板 Measure 失效；若下一轮仍按
        // 原约束测量，子项尺寸来回翻转，形成永久布局循环。因此输入宽度不变时，按上次排列宽度测量子项；
        // 回报的 DesiredSize 宽度仍按约束解析，避免无限宽度宿主把旧排列宽度当作内容 extent 保留。
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

    // 入场时机来自 Arrange 运行时状态（item 是否首次排列、是否接近可见区域），ControlTheme 无法表达，故在面板代码
    // 驱动；时长值由 ControlTheme 从 token Setter 提供（见 MasonryTheme.axaml）。
    // 重新布局（内容高度、宽度、列数、增删 item 引起的位置变化）不做任何 item 动画，既有 item 直接就位：
    // 拖拽缩放与图片陆续加载会连续重新布局，逐项位置动画会让 item 持续落后于布局位置并占用 UI 线程。
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

        // 首次 Arrange 时可见区域尚未得知；得知后立即结束远离可见区域的入场淡入（直接显示）。
        foreach (var (child, state) in _itemMotions.ToArray())
        {
            if (_lastVisualRects.TryGetValue(child, out var rect) && !IsInMotionRange(rect))
            {
                state.Cancellation.Cancel();
            }
        }
    }

    /// <summary>
    /// 判断视觉矩形是否位于动效范围：最近的有效视口向四周各扩展一个视口尺寸。
    /// 视口未知或非有限时按可见处理；视口为空表示面板不可见，不做动效。
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
            // 预置起始值（动画优先级），避免首帧闪烁。注意释放方式：AvaloniaObject.SetValue 对
            // 非 LocalValue 优先级的 UnsetValue 是静默忽略的，必须 Dispose 返回的句柄才能回落基值。
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
