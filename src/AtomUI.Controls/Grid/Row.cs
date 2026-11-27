using System.Collections.Specialized;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;

namespace AtomUI.Controls;

/// <summary>
/// A panel that lays its children out as columns on a 24-column grid, from left to right and, by default,
/// wrapped onto further lines when they do not fit in the width of the row.
/// </summary>
/// <remarks>
/// A child that is a <see cref="Col" /> is sized, ordered and shifted by the properties of that column, which are
/// read for the current breakpoint. A column whose span is 0 at that breakpoint is left out of the lines. Any
/// other visible child takes a slot as wide as its desired width, with order 0 and without offset, push or pull.
/// While the available width is infinite, spans have no effect and all columns stay on one line.
/// The breakpoint is that of the nearest <c>IMediaBreakAwareControl</c> among the row and its ancestors, or
/// <c>MediaBreakPoint.Large</c> when there is none. A change of the breakpoint makes the row lay its children
/// out again.
/// </remarks>
public class Row : Panel
{
    private const int GridColumns = 24;
    private static readonly Comparison<RowChildInfo> s_compareRowChildInfo = CompareRowChildInfo;

    /// <summary>
    /// Identifies the <see cref="Gutter" /> property.
    /// </summary>
    public static readonly StyledProperty<GridGutter> GutterProperty =
        AvaloniaProperty.Register<Row, GridGutter>(nameof(Gutter), new GridGutter());

    /// <summary>
    /// Identifies the <see cref="Justify" /> property.
    /// </summary>
    public static readonly StyledProperty<RowJustify> JustifyProperty =
        AvaloniaProperty.Register<Row, RowJustify>(nameof(Justify), RowJustify.Start);

    /// <summary>
    /// Identifies the <see cref="JustifyInfo" /> property.
    /// </summary>
    public static readonly StyledProperty<GridRowJustifyInfo?> JustifyInfoProperty =
        AvaloniaProperty.Register<Row, GridRowJustifyInfo?>(nameof(JustifyInfo));

    /// <summary>
    /// Identifies the <see cref="Align" /> property.
    /// </summary>
    public static readonly StyledProperty<RowAlign> AlignProperty =
        AvaloniaProperty.Register<Row, RowAlign>(nameof(Align), RowAlign.Stretch);

    /// <summary>
    /// Identifies the <see cref="AlignInfo" /> property.
    /// </summary>
    public static readonly StyledProperty<GridRowAlignInfo?> AlignInfoProperty =
        AvaloniaProperty.Register<Row, GridRowAlignInfo?>(nameof(AlignInfo));

    /// <summary>
    /// Identifies the <see cref="IsWrapped" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsWrappedProperty =
        AvaloniaProperty.Register<Row, bool>(nameof(IsWrapped), true);

    /// <summary>
    /// The spacing between the columns and between the lines, resolved for the current breakpoint. Half of the
    /// horizontal gutter is put on both sides of every column, so two columns are one full gutter apart and the
    /// content of a line that fills the row stays flush with the edges of the row. The vertical gutter is the gap
    /// between two lines.
    /// </summary>
    /// <remarks>
    /// The default leaves no spacing at any breakpoint.
    /// </remarks>
    public GridGutter Gutter
    {
        get => GetValue(GutterProperty);
        set => SetValue(GutterProperty, value);
    }

    /// <summary>
    /// How a line of columns uses the width it leaves unused: packed at the start, at the end or in the middle, or
    /// spread with gaps of equal width. <see cref="RowJustify.SpaceBetween" /> puts the gaps only between the
    /// columns, <see cref="RowJustify.SpaceAround" /> puts half a gap at both ends, and
    /// <see cref="RowJustify.SpaceEvenly" /> puts the same gap at both ends as between the columns.
    /// </summary>
    /// <remarks>
    /// The default is <see cref="RowJustify.Start" />. The value has no effect while the available width is
    /// infinite. <see cref="JustifyInfo" /> replaces it at the breakpoints that it covers.
    /// </remarks>
    public RowJustify Justify
    {
        get => GetValue(JustifyProperty);
        set => SetValue(JustifyProperty, value);
    }

    /// <summary>
    /// The <see cref="Justify" /> value per breakpoint. A breakpoint uses the value of the largest breakpoint that
    /// is configured and is not larger than it; <see cref="Justify" /> is used when there is none.
    /// </summary>
    /// <remarks>
    /// The default is <see langword="null" />, which uses <see cref="Justify" /> at every breakpoint.
    /// </remarks>
    public GridRowJustifyInfo? JustifyInfo
    {
        get => GetValue(JustifyInfoProperty);
        set => SetValue(JustifyInfoProperty, value);
    }

    /// <summary>
    /// How the columns of a line are placed in the height of that line: at the top, in the middle or at the bottom.
    /// The default, <see cref="RowAlign.Stretch" />, makes every column as high as the tallest column of its line.
    /// </summary>
    /// <remarks>
    /// <see cref="AlignInfo" /> replaces the value at the breakpoints that it covers.
    /// </remarks>
    public RowAlign Align
    {
        get => GetValue(AlignProperty);
        set => SetValue(AlignProperty, value);
    }

    /// <summary>
    /// The <see cref="Align" /> value per breakpoint. A breakpoint uses the value of the largest breakpoint that
    /// is configured and is not larger than it; <see cref="Align" /> is used when there is none.
    /// </summary>
    /// <remarks>
    /// The default is <see langword="null" />, which uses <see cref="Align" /> at every breakpoint.
    /// </remarks>
    public GridRowAlignInfo? AlignInfo
    {
        get => GetValue(AlignInfoProperty);
        set => SetValue(AlignInfoProperty, value);
    }

    /// <summary>
    /// Whether a column that does not fit in the width of its line starts a new line below the previous one, with
    /// the vertical gutter between the two lines. The default is <see langword="true" />. With
    /// <see langword="false" />, all columns stay on the first line even when they do not fit.
    /// </summary>
    /// <remarks>
    /// The value has no effect while the available width is infinite, because all columns stay on one line then.
    /// </remarks>
    public bool IsWrapped
    {
        get => GetValue(IsWrappedProperty);
        set => SetValue(IsWrappedProperty, value);
    }

    private MediaBreakPoint? _breakPoint;
    private IMediaBreakAwareControl? _mediaOwner;
    private readonly List<RowChildInfo> _orderedChildrenCache = new();
    private bool _orderedChildrenDirty = true;
    private LayoutResult? _measureLayoutResult;
    private double _measureLayoutWidth;

    static Row()
    {
        AffectsMeasure<Row>(GutterProperty, JustifyProperty, JustifyInfoProperty, AlignProperty, AlignInfoProperty, IsWrappedProperty);
    }

    /// <summary>
    /// Called when the row is attached to a visual tree. It calls the base method first, then reads the current
    /// breakpoint from the <c>IMediaBreakAwareControl</c> found for the row and subscribes to its changes, so that
    /// the row lays its children out again when the breakpoint changes.
    /// </summary>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if (MediaQueryHost.FindOwner(this) is { } mediaOwner)
        {
            _mediaOwner = mediaOwner;
            _breakPoint = mediaOwner.MediaBreakPoint;
            mediaOwner.MediaBreakPointChanged += HandleMediaBreakChanged;
        }
    }

    /// <summary>
    /// Called when the row is detached from a visual tree. It calls the base method first, then ends the
    /// subscription to the media breakpoint owner that was found on attach.
    /// </summary>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        if (_mediaOwner != null)
        {
            _mediaOwner.MediaBreakPointChanged -= HandleMediaBreakChanged;
            _mediaOwner = null;
        }
    }

    private void HandleMediaBreakChanged(object? sender, MediaBreakPointChangedEventArgs args)
    {
        _breakPoint           = args.MediaBreakPoint;
        InvalidateOrderedChildrenCache();
        InvalidateMeasure();
    }

    /// <summary>
    /// Called when the children of the row change. It calls the base method first, then drops the cached order,
    /// column layouts and line layout, so that the next layout pass works with the new children.
    /// </summary>
    protected override void ChildrenChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        base.ChildrenChanged(sender, e);
        InvalidateOrderedChildrenCache();
    }

    /// <summary>
    /// Called when the measure of the row is invalidated. It calls the base method first, then drops the cached
    /// order, column layouts and line layout, so that the next measure pass computes them again.
    /// </summary>
    protected override void OnMeasureInvalidated()
    {
        base.OnMeasureInvalidated();
        InvalidateOrderedChildrenCache();
    }

    internal void InvalidateChildLayout()
    {
        InvalidateOrderedChildrenCache();
        InvalidateMeasure();
    }

    private void InvalidateOrderedChildrenCache()
    {
        _orderedChildrenDirty = true;
        _measureLayoutResult  = null;
    }

    /// <summary>
    /// Measures the children and divides them into lines. The result is kept for
    /// <see cref="ArrangeOverride(Size)" /> when the final width equals the measured width.
    /// </summary>
    /// <param name="availableSize">
    /// The size offered to the row. An infinite width puts all columns on one line and ignores their spans.
    /// </param>
    /// <returns>
    /// With a finite width, the offered width and the total height of the lines with the vertical gutters between
    /// them. With an infinite width, the width of the widest line and that height.
    /// </returns>
    protected override Size MeasureOverride(Size availableSize)
    {
        var result = BuildLayout(availableSize, measureChildren: true);
        _measureLayoutResult = result;
        _measureLayoutWidth  = availableSize.Width;
        return result.DesiredSize;
    }

    /// <summary>
    /// Places the lines one below the other and the columns of each line from left to right, with the justify
    /// value, the alignment, and the offset, push and pull of each column applied. A column whose span is 0 at the
    /// current breakpoint is arranged with empty bounds.
    /// </summary>
    /// <param name="finalSize">
    /// The width the lines are laid out in and the height of the row.
    /// </param>
    /// <returns>
    /// Always <paramref name="finalSize" />.
    /// </returns>
    /// <remarks>
    /// The layout of the last measure pass is used when the width of <paramref name="finalSize" /> is close to the
    /// measured width and nothing has been invalidated since. Otherwise the lines are computed again, without
    /// measuring the children again.
    /// </remarks>
    protected override Size ArrangeOverride(Size finalSize)
    {
        var result = GetArrangeLayout(finalSize);
        var y = 0.0;

        foreach (var line in result.Lines)
        {
            var gapCount = Math.Max(0, line.Items.Count - 1);
            var remaining = result.WidthInfinite ? 0 : Math.Max(0, result.LayoutWidth - line.TotalWidth);
            var (justifyOffset, gap) = CalculateJustify(result.Justify, remaining, gapCount, line.Items.Count);
            var x = result.HorizontalOrigin + justifyOffset;

            foreach (var item in line.Items)
            {
                x += item.OffsetWidth;
                var childWidth = item.FinalWidth;
                var childHeight = result.Align == RowAlign.Stretch ? line.Height : item.Child.DesiredSize.Height;
                var yOffset = CalculateCrossAxisOffset(result.Align, line.Height, childHeight);
                var shift = result.WidthInfinite ? 0 : result.ColumnWidth * (item.Layout.Push - item.Layout.Pull);
                var paddedWidth = Math.Max(0, childWidth - result.HorizontalPadding * 2);
                var childX = x + shift + result.HorizontalPadding;
                item.Child.Arrange(new Rect(childX, y + yOffset, paddedWidth, Math.Max(0, childHeight)));
                x += childWidth + gap;
            }

            y += line.Height + result.VerticalGap;
        }

        foreach (var hiddenChild in result.HiddenChildren)
        {
            hiddenChild.Arrange(default);
        }

        return finalSize;
    }

    private LayoutResult GetArrangeLayout(Size finalSize)
    {
        if (!_orderedChildrenDirty &&
            _measureLayoutResult is not null &&
            MathUtils.AreClose(_measureLayoutWidth, finalSize.Width))
        {
            return _measureLayoutResult;
        }

        return BuildLayout(finalSize, measureChildren: false);
    }

    private LayoutResult BuildLayout(Size availableSize, bool measureChildren)
    {
        var result = new LayoutResult();
        var breakPoint = GetBreakPoint();
        var (horizontal, vertical) = Gutter.Resolve(breakPoint);
        result.HorizontalPadding = horizontal / 2.0;
        result.VerticalGap = vertical;
        result.ContainerWidth = availableSize.Width;
        result.WidthInfinite = double.IsInfinity(result.ContainerWidth);
        result.LayoutWidth = result.WidthInfinite ? result.ContainerWidth : Math.Max(0, result.ContainerWidth + horizontal);
        result.HorizontalOrigin = result.WidthInfinite ? 0 : -result.HorizontalPadding;
        result.ColumnWidth = result.WidthInfinite ? 0 : result.LayoutWidth / GridColumns;
        result.Justify = JustifyInfo?.Resolve(breakPoint, Justify) ?? Justify;
        result.Align = AlignInfo?.Resolve(breakPoint, Align) ?? Align;

        var orderedChildren = GetOrderedChildren(breakPoint);
        var currentLine = new RowLine();
        var lineWidth = 0.0;

        foreach (var info in orderedChildren)
        {
            var child = info.Child;
            var layout = info.Layout;

            if (layout.IsHidden)
            {
                if (measureChildren)
                {
                    child.Measure(default);
                }

                result.HiddenChildren.Add(child);
                continue;
            }

            var baseWidth = MeasureChild(child, layout, availableSize, result, measureChildren);
            var offsetWidth = result.WidthInfinite ? 0 : result.ColumnWidth * layout.Offset;
            var estimatedWidth = offsetWidth + baseWidth;

            if (IsWrapped && !result.WidthInfinite && currentLine.Items.Count > 0 &&
                MathUtils.GreaterThan(lineWidth + estimatedWidth, result.LayoutWidth))
            {
                FinalizeLine(currentLine, result, availableSize, measureChildren);
                result.Lines.Add(currentLine);
                currentLine = new RowLine();
                lineWidth = 0;
            }

            currentLine.Items.Add(new RowItem(child, layout, baseWidth, offsetWidth));
            currentLine.Height = Math.Max(currentLine.Height, child.DesiredSize.Height);
            lineWidth += estimatedWidth;
        }

        if (currentLine.Items.Count > 0)
        {
            FinalizeLine(currentLine, result, availableSize, measureChildren);
            result.Lines.Add(currentLine);
        }

        var totalHeight = result.Lines.Sum(l => l.Height);
        if (result.Lines.Count > 1)
        {
            totalHeight += result.VerticalGap * (result.Lines.Count - 1);
        }

        var maxWidth = result.WidthInfinite
            ? (result.Lines.Count > 0 ? result.Lines.Max(l => l.TotalWidth) : 0)
            : result.ContainerWidth;
        result.DesiredSize = new Size(maxWidth, totalHeight);
        return result;
    }

    private double MeasureChild(
        Control child,
        GridColLayout layout,
        Size availableSize,
        LayoutResult result,
        bool measureChildren)
    {
        var measureWidth = GetMeasureWidth(layout, result);
        if (measureChildren)
        {
            var adjustedWidth = AdjustWidth(measureWidth, result.HorizontalPadding);
            child.Measure(new Size(adjustedWidth, availableSize.Height));
        }

        return GetBaseWidth(child, layout, result);
    }

    private static double GetMeasureWidth(GridColLayout layout, LayoutResult result)
    {
        if (layout.Flex?.Basis is { } basis)
        {
            return basis;
        }

        if (layout.Flex.HasValue)
        {
            return result.WidthInfinite ? double.PositiveInfinity : result.LayoutWidth;
        }

        if (layout.Span > 0 && !result.WidthInfinite)
        {
            return result.ColumnWidth * layout.Span;
        }

        return result.WidthInfinite ? double.PositiveInfinity : result.LayoutWidth;
    }

    private static double GetBaseWidth(Control child, GridColLayout layout, LayoutResult result)
    {
        if (layout.Flex?.Basis is { } basis)
        {
            return basis;
        }

        if (layout.Flex.HasValue)
        {
            return child.DesiredSize.Width;
        }

        if (layout.Span > 0 && !result.WidthInfinite)
        {
            return result.ColumnWidth * layout.Span;
        }

        return child.DesiredSize.Width;
    }

    private static double AdjustWidth(double width, double padding)
    {
        if (double.IsInfinity(width))
        {
            return width;
        }

        return Math.Max(0, width - padding * 2);
    }

    private static void FinalizeLine(RowLine line, LayoutResult result, Size availableSize, bool measureChildren)
    {
        line.TotalOffset = line.Items.Sum(item => item.OffsetWidth);
        foreach (var item in line.Items)
        {
            item.FinalWidth = item.BaseWidth;
        }

        line.TotalWidth = line.TotalOffset + line.Items.Sum(item => item.FinalWidth);
        DistributeFlexWidth(line, result);

        if (measureChildren)
        {
            line.Height = 0;
            foreach (var item in line.Items)
            {
                var adjustedWidth = AdjustWidth(item.FinalWidth, result.HorizontalPadding);
                item.Child.Measure(new Size(adjustedWidth, availableSize.Height));
                line.Height = Math.Max(line.Height, item.Child.DesiredSize.Height);
            }
        }
    }

    private static void DistributeFlexWidth(RowLine line, LayoutResult result)
    {
        if (result.WidthInfinite)
        {
            return;
        }

        var remaining = result.LayoutWidth - line.TotalWidth;
        if (remaining > 0)
        {
            var totalGrow = line.Items.Sum(item => item.Layout.Flex?.Grow ?? 0);
            if (totalGrow > 0)
            {
                foreach (var item in line.Items)
                {
                    var grow = item.Layout.Flex?.Grow ?? 0;
                    if (grow > 0)
                    {
                        item.FinalWidth += remaining * grow / totalGrow;
                    }
                }
            }
        }
        else if (remaining < 0)
        {
            var totalShrink = line.Items.Sum(item => (item.Layout.Flex?.Shrink ?? 0) * item.BaseWidth);
            if (totalShrink > 0)
            {
                var overflow = -remaining;
                foreach (var item in line.Items)
                {
                    var shrink = item.Layout.Flex?.Shrink ?? 0;
                    if (shrink > 0)
                    {
                        item.FinalWidth = Math.Max(0, item.FinalWidth - overflow * shrink * item.BaseWidth / totalShrink);
                    }
                }
            }
        }

        line.TotalWidth = line.TotalOffset + line.Items.Sum(item => item.FinalWidth);
    }

    private List<RowChildInfo> GetOrderedChildren(MediaBreakPoint breakPoint)
    {
        if (!_orderedChildrenDirty)
        {
            return _orderedChildrenCache;
        }

        _orderedChildrenCache.Clear();
        for (var i = 0; i < Children.Count; i++)
        {
            if (Children[i] is not Control child || !child.IsVisible)
            {
                continue;
            }

            var layout = child is Col col
                ? col.ResolveLayout(breakPoint)
                : new GridColLayout(null, 0, false, 0, 0, 0, 0);

            _orderedChildrenCache.Add(new RowChildInfo(child, layout, i));
        }

        _orderedChildrenCache.Sort(s_compareRowChildInfo);

        _orderedChildrenDirty = false;
        return _orderedChildrenCache;
    }

    private static int CompareRowChildInfo(RowChildInfo left, RowChildInfo right)
    {
        var orderCmp = left.Layout.Order.CompareTo(right.Layout.Order);
        return orderCmp != 0 ? orderCmp : left.Index.CompareTo(right.Index);
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

    private static (double offset, double gap) CalculateJustify(
        RowJustify justify,
        double remaining,
        int gapCount,
        int itemCount)
    {
        if (itemCount <= 0)
        {
            return (0, 0);
        }

        return justify switch
        {
            RowJustify.Start => (0, 0),
            RowJustify.End => (remaining, 0),
            RowJustify.Center => (remaining / 2, 0),
            RowJustify.SpaceBetween when gapCount > 0 => (0, remaining / gapCount),
            RowJustify.SpaceAround => (remaining / itemCount / 2, remaining / itemCount),
            RowJustify.SpaceEvenly => (remaining / (itemCount + 1), remaining / (itemCount + 1)),
            _ => (0, 0)
        };
    }

    private static double CalculateCrossAxisOffset(RowAlign align, double lineHeight, double childHeight)
    {
        if (align == RowAlign.Middle)
        {
            return (lineHeight - childHeight) / 2;
        }

        if (align == RowAlign.Bottom)
        {
            return lineHeight - childHeight;
        }

        return 0;
    }

    private sealed class RowChildInfo
    {
        public RowChildInfo(Control child, GridColLayout layout, int index)
        {
            Child = child;
            Layout = layout;
            Index = index;
        }

        public Control Child { get; }
        public GridColLayout Layout { get; }
        public int Index { get; }
    }

    private sealed class RowItem
    {
        public RowItem(Control child, GridColLayout layout, double baseWidth, double offsetWidth)
        {
            Child = child;
            Layout = layout;
            BaseWidth = baseWidth;
            OffsetWidth = offsetWidth;
        }

        public Control Child { get; }
        public GridColLayout Layout { get; }
        public double BaseWidth { get; }
        public double OffsetWidth { get; }
        public double FinalWidth { get; set; }
    }

    private sealed class RowLine
    {
        public List<RowItem> Items { get; } = new();
        public double Height { get; set; }
        public double TotalWidth { get; set; }
        public double TotalOffset { get; set; }
    }

    private sealed class LayoutResult
    {
        public List<RowLine> Lines { get; } = new();
        public Size DesiredSize { get; set; }
        public double HorizontalPadding { get; set; }
        public double VerticalGap { get; set; }
        public double ColumnWidth { get; set; }
        public bool WidthInfinite { get; set; }
        public double ContainerWidth { get; set; }
        public double LayoutWidth { get; set; }
        public double HorizontalOrigin { get; set; }
        public RowJustify Justify { get; set; }
        public RowAlign Align { get; set; }
        public List<Control> HiddenChildren { get; } = new();
    }
}
