using Avalonia;
using Avalonia.Controls;

namespace AtomUI.Toolkits.GalleryBase.Controls;

public class ShowCaseMasonryPanel : Panel
{
    public static readonly StyledProperty<double> MinItemWidthProperty =
        AvaloniaProperty.Register<ShowCaseMasonryPanel, double>(nameof(MinItemWidth), 320);

    public static readonly StyledProperty<int> MaxColumnsProperty =
        AvaloniaProperty.Register<ShowCaseMasonryPanel, int>(nameof(MaxColumns), 2);

    public static readonly StyledProperty<double> ColumnGapProperty =
        AvaloniaProperty.Register<ShowCaseMasonryPanel, double>(nameof(ColumnGap), 16);

    public static readonly StyledProperty<double> RowGapProperty =
        AvaloniaProperty.Register<ShowCaseMasonryPanel, double>(nameof(RowGap), 16);

    private List<Rect> _arrangeRects = new();
    private MasonryLayout? _measuredLayout;
    private double _measuredEffectiveWidth;
    private int _measuredChildCount;
    private bool _hasMeasuredLayout;
    private double _previousAvailableWidth = double.NaN;
    private double? _arrangedEffectiveWidth;
    private readonly Dictionary<Control, int> _stableColumns = new(ReferenceEqualityComparer.Instance);
    private int _stableColumnCount;
    private bool _hasStableAssignments;

    public double MinItemWidth
    {
        get => GetValue(MinItemWidthProperty);
        set => SetValue(MinItemWidthProperty, value);
    }

    public int MaxColumns
    {
        get => GetValue(MaxColumnsProperty);
        set => SetValue(MaxColumnsProperty, value);
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

    static ShowCaseMasonryPanel()
    {
        AffectsMeasure<ShowCaseMasonryPanel>(
            MinItemWidthProperty,
            MaxColumnsProperty,
            ColumnGapProperty,
            RowGapProperty);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        if (_previousAvailableWidth != availableSize.Width)
        {
            _arrangedEffectiveWidth = null;
        }
        _previousAvailableWidth = availableSize.Width;
        // Ancestor layout rounding can give us a different final width even when the
        // measure constraint is unchanged. Reuse that width on the follow-up measure
        // so arrange-time child measurement does not invalidate us on every pass.
        var effectiveWidth = _arrangedEffectiveWidth ?? ResolveAvailableWidth(availableSize.Width);
        var layout = CalculateLayout(effectiveWidth, true);
        _measuredLayout = layout;
        _measuredEffectiveWidth = layout.Width;
        _measuredChildCount = Children.Count;
        _hasMeasuredLayout = true;
        _arrangeRects = layout.Rects;
        return new Size(layout.Width, layout.Height);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var effectiveWidth = ResolveAvailableWidth(finalSize.Width);
        _arrangedEffectiveWidth = effectiveWidth;
        var canReuseMeasuredLayout = _hasMeasuredLayout &&
                                      _measuredChildCount == Children.Count &&
                                      AreClose(_measuredEffectiveWidth, effectiveWidth);
        // A ScrollViewer (and similar hosts) can measure us with an unbounded width and then
        // arrange us at the viewport width. The child DesiredSize then belongs to a different
        // column width; remeasure before arranging so height-dependent Masonry positions do not
        // lag one layout pass behind the actual viewport.
        var layout = canReuseMeasuredLayout
            ? _measuredLayout!.Value
            : CalculateLayout(finalSize.Width, measureChildren: true);
        _hasMeasuredLayout = false;
        _measuredLayout = null;
        _arrangeRects = layout.Rects;

        for (var i = 0; i < Children.Count; i++)
        {
            Children[i].Arrange(_arrangeRects[i]);
        }

        CommitStableAssignments(layout);
        return finalSize;
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _hasMeasuredLayout = false;
        _measuredLayout = null;
        _previousAvailableWidth = double.NaN;
        _arrangedEffectiveWidth = null;
        ClearStableAssignments();
        base.OnDetachedFromVisualTree(e);
    }

    private static bool AreClose(double left, double right)
    {
        return Math.Abs(left - right) < 0.01;
    }

    private MasonryLayout CalculateLayout(double availableWidth, bool measureChildren)
    {
        var width       = ResolveAvailableWidth(availableWidth);
        var columnCount = CalculateColumnCount(width);
        var columnGap   = Math.Max(0, ColumnGap);
        var rowGap      = Math.Max(0, RowGap);
        var columnWidth = columnCount == 1
            ? width
            : Math.Max(0, (width - columnGap * (columnCount - 1)) / columnCount);
        var columnHeights = new double[columnCount];
        var rects         = new List<Rect>(Children.Count);
        var columns       = new List<int>(Children.Count);
        var fullSpans     = new List<bool>(Children.Count);

        foreach (var child in Children)
        {
            if (!child.IsVisible)
            {
                rects.Add(default);
                columns.Add(-1);
                fullSpans.Add(false);
                continue;
            }

            var isFullSpan  = IsFullSpan(child);
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
                var columnIndex = ResolveColumnIndex(child, columnHeights, columnCount);
                var x           = columnIndex * (columnWidth + columnGap);
                var y           = columnHeights[columnIndex] > 0 ? columnHeights[columnIndex] + rowGap : 0;
                rects.Add(new Rect(x, y, columnWidth, childHeight));
                columns.Add(columnIndex);
                fullSpans.Add(false);
                columnHeights[columnIndex] = y + childHeight;
            }
        }

        return new MasonryLayout(width, Max(columnHeights), columnCount, rects, columns, fullSpans);
    }

    private double ResolveAvailableWidth(double availableWidth)
    {
        if (!double.IsInfinity(availableWidth))
        {
            return Math.Max(0, availableWidth);
        }

        var maxColumns   = Math.Max(1, MaxColumns);
        var minItemWidth = Math.Max(1, MinItemWidth);
        return minItemWidth * maxColumns + Math.Max(0, ColumnGap) * (maxColumns - 1);
    }

    private int CalculateColumnCount(double width)
    {
        var maxColumns   = Math.Max(1, MaxColumns);
        var minItemWidth = Math.Max(1, MinItemWidth);
        var columnGap    = Math.Max(0, ColumnGap);
        var columnCount  = (int)Math.Floor((width + columnGap) / (minItemWidth + columnGap));
        return Math.Clamp(columnCount, 1, maxColumns);
    }

    private static bool IsFullSpan(Control child)
    {
        return child is ShowCaseItem { Span: ShowCaseItemSpan.Full } or ShowCaseItem { IsOccupyEntireRow: true };
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
        if (!_hasStableAssignments ||
            _stableColumnCount != columnCount ||
            !_stableColumns.TryGetValue(child, out var previousColumn) ||
            previousColumn < 0 ||
            previousColumn >= columnCount)
        {
            return shortestColumn;
        }

        return previousColumn;
    }

    private void CommitStableAssignments(MasonryLayout layout)
    {
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

        _stableColumnCount    = layout.ColumnCount;
        _hasStableAssignments = true;
    }

    private void ClearStableAssignments()
    {
        _stableColumns.Clear();
        _stableColumnCount    = 0;
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

    private readonly record struct MasonryLayout(
        double Width,
        double Height,
        int ColumnCount,
        List<Rect> Rects,
        List<int> Columns,
        List<bool> FullSpans);
}
