using AtomUI.Controls;
using Avalonia;
using Avalonia.Controls;

namespace AtomUI.Desktop.Controls;

public class Row : Panel
{
    private const int GridColumns = 24;

    public static readonly StyledProperty<GridGutter> GutterProperty =
        AvaloniaProperty.Register<Row, GridGutter>(nameof(Gutter), new GridGutter());

    public static readonly StyledProperty<RowJustify> JustifyProperty =
        AvaloniaProperty.Register<Row, RowJustify>(nameof(Justify), RowJustify.Start);

    public static readonly StyledProperty<RowAlign> AlignProperty =
        AvaloniaProperty.Register<Row, RowAlign>(nameof(Align), RowAlign.Stretch);

    public static readonly StyledProperty<bool> WrapProperty =
        AvaloniaProperty.Register<Row, bool>(nameof(Wrap), true);

    public GridGutter Gutter
    {
        get => GetValue(GutterProperty);
        set => SetValue(GutterProperty, value);
    }

    public RowJustify Justify
    {
        get => GetValue(JustifyProperty);
        set => SetValue(JustifyProperty, value);
    }

    public RowAlign Align
    {
        get => GetValue(AlignProperty);
        set => SetValue(AlignProperty, value);
    }

    public bool Wrap
    {
        get => GetValue(WrapProperty);
        set => SetValue(WrapProperty, value);
    }

    private MediaBreakPoint? _breakPoint;
    private IMediaBreakAwareControl? _mediaOwner;

    static Row()
    {
        AffectsMeasure<Row>(GutterProperty, JustifyProperty, AlignProperty, WrapProperty);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if (TopLevel.GetTopLevel(this) is IMediaBreakAwareControl mediaOwner)
        {
            _mediaOwner = mediaOwner;
            _breakPoint = mediaOwner.MediaBreakPoint;
            mediaOwner.MediaBreakPointChanged += HandleMediaBreakChanged;
        }
    }

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
        _breakPoint = args.MediaBreakPoint;
        InvalidateMeasure();
        InvalidateArrange();
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var result = BuildLayout(availableSize, measureChildren: true);
        return result.DesiredSize;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var result = BuildLayout(finalSize, measureChildren: false);
        var y = 0.0;

        foreach (var line in result.Lines)
        {
            var gapCount = Math.Max(0, line.Items.Count - 1);
            var remaining = result.WidthInfinite ? 0 : Math.Max(0, result.ContainerWidth - line.TotalWidth);
            var (startX, gap) = CalculateJustify(Justify, remaining, gapCount, line.Items.Count);
            var x = startX;

            foreach (var item in line.Items)
            {
                x += item.OffsetWidth;
                var childWidth = item.FinalWidth;
                var childHeight = Align == RowAlign.Stretch ? line.Height : item.Child.DesiredSize.Height;
                var yOffset = CalculateCrossAxisOffset(Align, line.Height, childHeight);
                var shift = result.WidthInfinite ? 0 : result.ColumnWidth * (item.Layout.Push - item.Layout.Pull);
                var paddedWidth = Math.Max(0, childWidth - result.HorizontalPadding * 2);
                var childX = x + shift + result.HorizontalPadding;
                item.Child.Arrange(new Rect(childX, y + yOffset, paddedWidth, Math.Max(0, childHeight)));
                x += childWidth + gap;
            }

            y += line.Height + result.VerticalGap;
        }

        return finalSize;
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
        result.ColumnWidth = result.WidthInfinite ? 0 : result.ContainerWidth / GridColumns;

        var orderedChildren = GetOrderedChildren(breakPoint);
        var currentLine = new RowLine();
        var lineWidth = 0.0;

        foreach (var info in orderedChildren)
        {
            var child = info.Child;
            var layout = info.Layout;
            var baseWidth = MeasureChild(child, layout, availableSize, result, measureChildren);
            var offsetWidth = result.WidthInfinite ? 0 : result.ColumnWidth * layout.Offset;
            var estimatedWidth = offsetWidth + baseWidth;

            if (Wrap && !result.WidthInfinite && currentLine.Items.Count > 0 &&
                lineWidth + estimatedWidth > result.ContainerWidth)
            {
                FinalizeLine(currentLine);
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
            FinalizeLine(currentLine);
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
        if (layout.Span > 0 && !result.WidthInfinite)
        {
            return result.ColumnWidth * layout.Span;
        }

        return result.WidthInfinite ? double.PositiveInfinity : result.ContainerWidth;
    }

    private static double GetBaseWidth(Control child, GridColLayout layout, LayoutResult result)
    {
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

    private static void FinalizeLine(RowLine line)
    {
        line.TotalOffset = line.Items.Sum(item => item.OffsetWidth);
        foreach (var item in line.Items)
        {
            item.FinalWidth = item.BaseWidth;
        }

        line.TotalWidth = line.TotalOffset + line.Items.Sum(item => item.FinalWidth);
    }

    private List<RowChildInfo> GetOrderedChildren(MediaBreakPoint breakPoint)
    {
        var children = new List<RowChildInfo>(Children.Count);
        for (var i = 0; i < Children.Count; i++)
        {
            if (Children[i] is not Control child || !child.IsVisible)
            {
                continue;
            }

            var layout = child is Col col
                ? col.ResolveLayout(breakPoint)
                : new GridColLayout(0, 0, 0, 0, 0);

            children.Add(new RowChildInfo(child, layout, i));
        }

        return children
            .OrderBy(info => info.Layout.Order)
            .ThenBy(info => info.Index)
            .ToList();
    }

    private MediaBreakPoint GetBreakPoint()
    {
        if (_breakPoint.HasValue)
        {
            return _breakPoint.Value;
        }

        if (TopLevel.GetTopLevel(this) is IMediaBreakAwareControl mediaOwner)
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
    }
}
