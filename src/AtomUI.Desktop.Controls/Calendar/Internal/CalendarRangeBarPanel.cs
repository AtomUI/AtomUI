using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.VisualTree;
using AtomUI.Desktop.Controls.Internal.DateViewer;
using DateViewerControl = AtomUI.Desktop.Controls.DateViewer;

namespace AtomUI.Desktop.Controls.Internal.Calendar;

internal sealed class CalendarRangeBarPanel : Panel
{
    #region 公共属性定义

    public static readonly StyledProperty<DateViewerControl?> LayoutSourceProperty =
        AvaloniaProperty.Register<CalendarRangeBarPanel, DateViewerControl?>(nameof(LayoutSource));
    public DateViewerControl? LayoutSource { get => GetValue(LayoutSourceProperty); set => SetValue(LayoutSourceProperty, value); }

    public static readonly StyledProperty<CalendarMode> ModeProperty =
        AvaloniaProperty.Register<CalendarRangeBarPanel, CalendarMode>(nameof(Mode));

    public static readonly StyledProperty<bool> FullscreenProperty =
        AvaloniaProperty.Register<CalendarRangeBarPanel, bool>(nameof(Fullscreen), true);

    public static readonly StyledProperty<CalendarRangeBarCollection?> RangeBarsProperty =
        AvaloniaProperty.Register<CalendarRangeBarPanel, CalendarRangeBarCollection?>(nameof(RangeBars));

    public static readonly StyledProperty<double> RangeBarHeightProperty =
        AvaloniaProperty.Register<CalendarRangeBarPanel, double>(nameof(RangeBarHeight), double.NaN);

    public static readonly StyledProperty<double> RangeBarTopOffsetProperty =
        AvaloniaProperty.Register<CalendarRangeBarPanel, double>(nameof(RangeBarTopOffset), 32);

    public static readonly StyledProperty<double> RangeBarHorizontalInsetProperty =
        AvaloniaProperty.Register<CalendarRangeBarPanel, double>(nameof(RangeBarHorizontalInset), 4);

    public CalendarMode Mode
    {
        get => GetValue(ModeProperty);
        set => SetValue(ModeProperty, value);
    }

    public bool Fullscreen
    {
        get => GetValue(FullscreenProperty);
        set => SetValue(FullscreenProperty, value);
    }

    public CalendarRangeBarCollection? RangeBars
    {
        get => GetValue(RangeBarsProperty);
        set => SetValue(RangeBarsProperty, value);
    }

    public double RangeBarHeight
    {
        get => GetValue(RangeBarHeightProperty);
        set => SetValue(RangeBarHeightProperty, value);
    }

    public double RangeBarTopOffset
    {
        get => GetValue(RangeBarTopOffsetProperty);
        set => SetValue(RangeBarTopOffsetProperty, value);
    }

    public double RangeBarHorizontalInset
    {
        get => GetValue(RangeBarHorizontalInsetProperty);
        set => SetValue(RangeBarHorizontalInsetProperty, value);
    }

    #endregion

    private const double DefaultRangeBarHeight = 20;
    private const double DefaultRangeBarGap = 2;
    private const double DefaultLabelFontSize = 12;
    private const double DefaultLabelHorizontalPadding = 8;

    private static readonly HashSet<AvaloniaProperty> LayoutTriggers = new()
    {
        ModeProperty,
        FullscreenProperty,
        RangeBarsProperty,
        RangeBarHeightProperty,
        RangeBarTopOffsetProperty,
        RangeBarHorizontalInsetProperty,
        FlowDirectionProperty
    };

    private readonly List<CalendarRangeBarSegment> _segments = new();
    private Size _realizedSize;
    private bool _segmentsDirty = true;
    private DateViewerControl? _subscribedLayoutSource;

    public CalendarRangeBarPanel()
    {
        IsHitTestVisible = false;
        ClipToBounds = true;
    }

    internal void InvalidateRangeBars()
    {
        _segmentsDirty = true;
        InvalidateMeasure();
        InvalidateArrange();
        InvalidateVisual();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == LayoutSourceProperty)
        {
            UnsubscribeLayoutSource();
            if (this.IsAttachedToVisualTree())
            {
                SubscribeLayoutSource();
            }

            InvalidateRangeBars();
        }
        if (LayoutTriggers.Contains(change.Property))
        {
            InvalidateRangeBars();
        }
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        SubscribeLayoutSource();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        UnsubscribeLayoutSource();
        base.OnDetachedFromVisualTree(e);
    }

    private void SubscribeLayoutSource()
    {
        UnsubscribeLayoutSource();
        _subscribedLayoutSource = LayoutSource;
        if (_subscribedLayoutSource is not null)
        {
            _subscribedLayoutSource.CellLayoutChanged += OnCellLayoutChanged;
        }
    }

    private void UnsubscribeLayoutSource()
    {
        if (_subscribedLayoutSource is not null)
        {
            _subscribedLayoutSource.CellLayoutChanged -= OnCellLayoutChanged;
        }

        _subscribedLayoutSource = null;
    }

    private void OnCellLayoutChanged(object? sender, EventArgs e) => InvalidateRangeBars();

    protected override Size MeasureOverride(Size availableSize)
    {
        if (HasUsableSize(availableSize))
        {
            EnsureSegments(availableSize);
        }

        MeasureChildren();
        return default;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        EnsureSegments(finalSize);
        MeasureChildren();
        for (var i = 0; i < Children.Count && i < _segments.Count; i++)
        {
            Children[i].Arrange(_segments[i].Bounds);
        }

        return finalSize;
    }

    private static bool HasUsableSize(Size size)
    {
        return double.IsFinite(size.Width) &&
               double.IsFinite(size.Height) &&
               size.Width > 0 &&
               size.Height > 0;
    }

    private void EnsureSegments(Size availableSize)
    {
        if (!_segmentsDirty && _realizedSize == availableSize)
        {
            return;
        }

        _realizedSize = availableSize;
        _segmentsDirty = false;
        BuildSegments(availableSize);
        SyncChildren();
    }

    private void BuildSegments(Size size)
    {
        _segments.Clear();
        if (!CanRender(size) || LayoutSource is not { } source || source.TransformToVisual(this) is not { } transform)
        {
            return;
        }

        var dates = source.CellLayouts.Where(cell => cell.Kind == DateViewerCellType.Date)
            .Select(cell => cell with { Bounds = cell.Bounds.TransformToAABB(transform) })
            .OrderBy(cell => cell.Value).ToArray();
        if (dates.Length == 0)
        {
            return;
        }

        var gridStart = dates[0].Value;
        var gridEnd = dates[^1].Value;
        var laneEnds = new List<DateTime>();
        foreach (var rangeBar in RangeBars!)
        {
            if (!TryNormalizeRange(rangeBar, gridStart, gridEnd, out var rangeStart, out var rangeEnd, out var height))
            {
                continue;
            }

            var lane = AllocateLane(laneEnds, rangeStart, rangeEnd);
            AddRowSegments(rangeBar, rangeStart, rangeEnd, lane, dates, height);
        }
    }

    private bool CanRender(Size size)
    {
        return Mode == CalendarMode.Month &&
               Fullscreen &&
               RangeBars is { Count: > 0 } &&
               size.Width > 0 &&
               size.Height > 0;
    }

    private bool TryNormalizeRange(
        CalendarRangeBar rangeBar,
        DateTime gridStart,
        DateTime gridEnd,
        out DateTime rangeStart,
        out DateTime rangeEnd,
        out double barHeight)
    {
        rangeStart = default;
        rangeEnd = default;
        barHeight = default;

        if (rangeBar.StartDate is null || rangeBar.EndDate is null)
        {
            return false;
        }

        rangeStart = rangeBar.StartDate.Value.Date;
        rangeEnd = rangeBar.EndDate.Value.Date;
        if (rangeEnd < rangeStart)
        {
            (rangeStart, rangeEnd) = (rangeEnd, rangeStart);
        }

        if (rangeStart > gridEnd || rangeEnd < gridStart)
        {
            return false;
        }

        barHeight = GetEffectiveRangeBarHeight(rangeBar);
        return barHeight > 0;
    }

    private double GetEffectiveRangeBarHeight(CalendarRangeBar rangeBar)
    {
        if (double.IsFinite(rangeBar.Height) && rangeBar.Height > 0)
        {
            return rangeBar.Height;
        }

        if (double.IsFinite(RangeBarHeight) && RangeBarHeight > 0)
        {
            return RangeBarHeight;
        }

        return DefaultRangeBarHeight;
    }

    private static int AllocateLane(IList<DateTime> laneEnds, DateTime rangeStart, DateTime rangeEnd)
    {
        for (var lane = 0; lane < laneEnds.Count; lane++)
        {
            if (rangeStart > laneEnds[lane])
            {
                laneEnds[lane] = rangeEnd;
                return lane;
            }
        }

        laneEnds.Add(rangeEnd);
        return laneEnds.Count - 1;
    }

    private void AddRowSegments(CalendarRangeBar rangeBar, DateTime rangeStart, DateTime rangeEnd,
        int lane, IReadOnlyList<DateCellLayout> dates, double requestedHeight)
    {
        foreach (var row in dates.GroupBy(cell => cell.Row))
        {
            var included = row.Where(cell => cell.Value >= rangeStart && cell.Value <= rangeEnd).ToArray();
            if (included.Length == 0)
            {
                continue;
            }

            var left = included.Min(cell => cell.Bounds.Left);
            var right = included.Max(cell => cell.Bounds.Right);
            var top = included.Min(cell => cell.Bounds.Top);
            var bottom = included.Min(cell => cell.Bounds.Bottom);
            var inset = ClampMetric(RangeBarHorizontalInset, 0, included.Min(cell => cell.Bounds.Width) / 2);
            var offset = GetLaneOffset(lane, requestedHeight);
            var width = right - left - inset * 2;
            var height = Math.Min(requestedHeight, Math.Max(0, bottom - top - offset));
            if (width <= 0 || height <= 0)
            {
                continue;
            }

            _segments.Add(new CalendarRangeBarSegment(
                new Rect(left + inset, top + offset, width, height),
                rangeBar.Background ?? Brushes.Transparent,
                BuildCornerRadius(height / 2, included.Any(cell => cell.Value == rangeStart), included.Any(cell => cell.Value == rangeEnd)),
                included.Any(cell => cell.Value == rangeStart) ? rangeBar.Label : null));
        }
    }

    private double GetLaneOffset(int lane, double barHeight)
    {
        var topOffset = Math.Max(0, RangeBarTopOffset);
        return topOffset + lane * (barHeight + DefaultRangeBarGap);
    }

    private CornerRadius BuildCornerRadius(double radius, bool startsRange, bool endsRange)
    {
        var leftRadius = FlowDirection == FlowDirection.RightToLeft ? endsRange : startsRange;
        var rightRadius = FlowDirection == FlowDirection.RightToLeft ? startsRange : endsRange;
        return new CornerRadius(
            leftRadius ? radius : 0,
            rightRadius ? radius : 0,
            rightRadius ? radius : 0,
            leftRadius ? radius : 0);
    }

    private static double ClampMetric(double value, double min, double max)
    {
        if (!double.IsFinite(value))
        {
            return min;
        }

        return Math.Clamp(value, min, max);
    }

    private void SyncChildren()
    {
        while (Children.Count > _segments.Count)
        {
            Children.RemoveAt(Children.Count - 1);
        }

        while (Children.Count < _segments.Count)
        {
            Children.Add(CreateRangeBarElement());
        }

        for (var i = 0; i < _segments.Count; i++)
        {
            ApplySegment((Border)Children[i], _segments[i]);
        }
    }

    private void MeasureChildren()
    {
        for (var i = 0; i < Children.Count && i < _segments.Count; i++)
        {
            Children[i].Measure(_segments[i].Bounds.Size);
        }
    }

    private static Border CreateRangeBarElement()
    {
        return new Border
        {
            ClipToBounds = true,
            IsHitTestVisible = false
        };
    }

    private void ApplySegment(Border element, CalendarRangeBarSegment segment)
    {
        if (!Equals(element.Background, segment.Background))
        {
            element.Background = segment.Background;
        }

        if (element.CornerRadius != segment.CornerRadius)
        {
            element.CornerRadius = segment.CornerRadius;
        }

        if (element.Width != segment.Bounds.Width)
        {
            element.Width = segment.Bounds.Width;
        }

        if (element.Height != segment.Bounds.Height)
        {
            element.Height = segment.Bounds.Height;
        }

        var padding = new Thickness(DefaultLabelHorizontalPadding, 0);
        if (element.Padding != padding)
        {
            element.Padding = padding;
        }

        var text = segment.Label?.ToString();
        if (string.IsNullOrEmpty(text))
        {
            if (element.Child is not null)
            {
                element.Child = null;
            }

            return;
        }

        if (element.Child is TextBlock label)
        {
            if (label.Text != text)
            {
                label.Text = text;
            }

            var textAlignment = FlowDirection == FlowDirection.RightToLeft
                ? TextAlignment.Right
                : TextAlignment.Left;
            if (label.TextAlignment != textAlignment)
            {
                label.TextAlignment = textAlignment;
            }
        }
        else
        {
            element.Child = CreateLabel(text);
        }
    }

    private TextBlock? CreateLabel(object? label)
    {
        var text = label?.ToString();
        if (string.IsNullOrEmpty(text))
        {
            return null;
        }

        return new TextBlock
        {
            Text = text,
            TextTrimming = TextTrimming.CharacterEllipsis,
            Foreground = Brushes.White,
            FontSize = DefaultLabelFontSize,
            VerticalAlignment = VerticalAlignment.Center,
            TextAlignment = FlowDirection == FlowDirection.RightToLeft ? TextAlignment.Right : TextAlignment.Left,
            IsHitTestVisible = false
        };
    }

    private readonly record struct CalendarRangeBarSegment(
        Rect Bounds,
        IBrush Background,
        CornerRadius CornerRadius,
        object? Label);
}
