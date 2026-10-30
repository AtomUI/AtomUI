using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Layout;

namespace AtomUI.Controls.Commons;

internal class TimelineSectionPanel : Panel
{
    #region 公共属性定义

    public static readonly StyledProperty<bool> IsOddProperty =
        AvaloniaProperty.Register<TimelineSectionPanel, bool>(nameof(IsOdd));

    public static readonly StyledProperty<TimelineMode> ModeProperty =
        AvaloniaProperty.Register<TimelineSectionPanel, TimelineMode>(nameof(Mode), TimelineMode.Start);

    public static readonly StyledProperty<Orientation> OrientationProperty =
        StackPanel.OrientationProperty.AddOwner<TimelineSectionPanel>();

    public static readonly StyledProperty<bool> IsLabelLayoutProperty =
        AvaloniaProperty.Register<TimelineSectionPanel, bool>(nameof(IsLabelLayout), false);

    public static readonly StyledProperty<double> IndicatorSpacingProperty =
        AvaloniaProperty.Register<TimelineSectionPanel, double>(nameof(IndicatorSpacing));

    public static readonly StyledProperty<Thickness> AxisOverflowProperty =
        AvaloniaProperty.Register<TimelineSectionPanel, Thickness>(nameof(AxisOverflow));

    public bool IsOdd
    {
        get => GetValue(IsOddProperty);
        set => SetValue(IsOddProperty, value);
    }

    public TimelineMode Mode
    {
        get => GetValue(ModeProperty);
        set => SetValue(ModeProperty, value);
    }

    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    public bool IsLabelLayout
    {
        get => GetValue(IsLabelLayoutProperty);
        set => SetValue(IsLabelLayoutProperty, value);
    }

    public double IndicatorSpacing
    {
        get => GetValue(IndicatorSpacingProperty);
        set => SetValue(IndicatorSpacingProperty, value);
    }

    public Thickness AxisOverflow
    {
        get => GetValue(AxisOverflowProperty);
        set => SetValue(AxisOverflowProperty, value);
    }

    #endregion

    static TimelineSectionPanel()
    {
        OrientationProperty.OverrideDefaultValue<TimelineSectionPanel>(Orientation.Vertical);
        AffectsMeasure<TimelineSectionPanel>(
            IsOddProperty,
            ModeProperty,
            OrientationProperty,
            IsLabelLayoutProperty,
            IndicatorSpacingProperty);
        AffectsArrange<TimelineSectionPanel>(AxisOverflowProperty);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        return Orientation == Orientation.Horizontal
            ? MeasureHorizontal(availableSize)
            : MeasureVertical(availableSize);
    }

    private Size MeasureVertical(Size availableSize)
    {
        var indicator        = GetTimelineIndicator();
        var headerPanel      = GetHeaderPanel();
        var contentPresenter = GetContentPresenter();
        indicator.Measure(availableSize);
        var indicatorWidth = indicator.DesiredSize.Width;
        var desiredWidth   = 0d;
        var desiredHeight  = indicator.DesiredSize.Height;
        if (IsLabelLayout || Mode == TimelineMode.Alternate)
        {
            var labelOrContentWidth = double.IsPositiveInfinity(availableSize.Width)
                ? double.PositiveInfinity
                : Math.Max(0, availableSize.Width - indicatorWidth) / 2;
            var labelOrContentSize = new Size(labelOrContentWidth, availableSize.Height);
            headerPanel.Measure(labelOrContentSize);
            contentPresenter.Measure(labelOrContentSize);
            var sideWidth = Math.Max(headerPanel.DesiredSize.Width, contentPresenter.DesiredSize.Width);
            desiredWidth  = sideWidth * 2 + indicatorWidth;
            desiredHeight = Math.Max(
                desiredHeight,
                Math.Max(headerPanel.DesiredSize.Height, contentPresenter.DesiredSize.Height));
        }
        else
        {
            var contentWidth = double.IsPositiveInfinity(availableSize.Width)
                ? double.PositiveInfinity
                : Math.Max(0, availableSize.Width - indicatorWidth);
            var contentSize = new Size(contentWidth, availableSize.Height);
            contentPresenter.Measure(contentSize);
            desiredWidth  = indicatorWidth + contentPresenter.DesiredSize.Width;
            desiredHeight = Math.Max(desiredHeight, contentPresenter.DesiredSize.Height);
        }

        if (!double.IsPositiveInfinity(availableSize.Width))
        {
            desiredWidth = availableSize.Width;
        }

        return new Size(desiredWidth, desiredHeight);
    }

    private Size MeasureHorizontal(Size availableSize)
    {
        var indicator        = GetTimelineIndicator();
        var headerPanel      = GetHeaderPanel();
        var contentPresenter = GetContentPresenter();
        indicator.Measure(availableSize);

        var childSize    = new Size(availableSize.Width, availableSize.Height);
        var desiredWidth = indicator.DesiredSize.Width;
        double desiredHeight;
        if (Mode == TimelineMode.Alternate)
        {
            headerPanel.Measure(childSize);
            contentPresenter.Measure(childSize);
            var sideExtent  = Math.Max(headerPanel.DesiredSize.Height, contentPresenter.DesiredSize.Height);
            desiredWidth    = Math.Max(
                desiredWidth,
                Math.Max(headerPanel.DesiredSize.Width, contentPresenter.DesiredSize.Width));
            desiredHeight = sideExtent * 2 + indicator.DesiredSize.Height + IndicatorSpacing * 2;
        }
        else if (IsLabelLayout)
        {
            headerPanel.Measure(childSize);
            contentPresenter.Measure(childSize);
            desiredWidth  = Math.Max(
                desiredWidth,
                Math.Max(headerPanel.DesiredSize.Width, contentPresenter.DesiredSize.Width));
            desiredHeight = indicator.DesiredSize.Height;
            if (headerPanel.DesiredSize.Height > 0)
            {
                desiredHeight += IndicatorSpacing + headerPanel.DesiredSize.Height;
            }
            if (contentPresenter.DesiredSize.Height > 0)
            {
                desiredHeight += IndicatorSpacing + contentPresenter.DesiredSize.Height;
            }
        }
        else
        {
            contentPresenter.Measure(childSize);
            desiredWidth  = Math.Max(desiredWidth, contentPresenter.DesiredSize.Width);
            desiredHeight = indicator.DesiredSize.Height + contentPresenter.DesiredSize.Height;
            if (contentPresenter.DesiredSize.Height > 0)
            {
                desiredHeight += IndicatorSpacing;
            }
        }

        if (!double.IsPositiveInfinity(availableSize.Width))
        {
            desiredWidth = availableSize.Width;
        }

        return new Size(desiredWidth, desiredHeight);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        if (Orientation == Orientation.Horizontal)
        {
            ArrangeHorizontal(finalSize);
        }
        else
        {
            ArrangeVertical(finalSize);
        }

        return finalSize;
    }

    private void ArrangeVertical(Size finalSize)
    {
        var indicator        = GetTimelineIndicator();
        var headerPanel      = GetHeaderPanel();
        var contentPresenter = GetContentPresenter();
        var indicatorWidth   = indicator.DesiredSize.Width;
        var effectiveMode    = GetEffectiveMode();
        var indicatorHeight = finalSize.Height + AxisOverflow.Bottom;
        if (IsLabelLayout || Mode == TimelineMode.Alternate)
        {
            var labelOrContentWidth = Math.Max(0, finalSize.Width - indicatorWidth) / 2;
            var leftRect  = new Rect(0, 0, labelOrContentWidth, finalSize.Height);
            var rightRect = new Rect(labelOrContentWidth + indicatorWidth, 0, labelOrContentWidth, finalSize.Height);
            indicator.Arrange(new Rect(labelOrContentWidth, 0, indicatorWidth, indicatorHeight));
            if (effectiveMode == TimelineMode.Start)
            {
                headerPanel.Arrange(leftRect);
                contentPresenter.Arrange(rightRect);
            }
            else
            {
                headerPanel.Arrange(rightRect);
                contentPresenter.Arrange(leftRect);
            }

        }
        else
        {
            var contentWidth = Math.Max(0, finalSize.Width - indicatorWidth);
            if (effectiveMode == TimelineMode.Start)
            {
                indicator.Arrange(new Rect(0, 0, indicatorWidth, indicatorHeight));
                contentPresenter.Arrange(new Rect(indicatorWidth, 0, contentWidth, finalSize.Height));
            }
            else
            {
                indicator.Arrange(new Rect(contentWidth, 0, indicatorWidth, indicatorHeight));
                contentPresenter.Arrange(new Rect(0, 0, contentWidth, finalSize.Height));
            }
        }
    }

    private void ArrangeHorizontal(Size finalSize)
    {
        var indicator        = GetTimelineIndicator();
        var headerPanel      = GetHeaderPanel();
        var contentPresenter = GetContentPresenter();
        var indicatorHeight  = indicator.DesiredSize.Height;
        var effectiveMode    = GetEffectiveMode();
        if (Mode == TimelineMode.Alternate)
        {
            var sideExtent    = Math.Max(0, finalSize.Height - indicatorHeight - IndicatorSpacing * 2) / 2;
            var topRect       = new Rect(0, 0, finalSize.Width, sideExtent);
            var indicatorRect = new Rect(
                0,
                sideExtent + IndicatorSpacing,
                finalSize.Width,
                indicatorHeight);
            var bottomRect = new Rect(
                0,
                sideExtent + IndicatorSpacing * 2 + indicatorHeight,
                finalSize.Width,
                sideExtent);
            indicator.Arrange(indicatorRect);
            if (effectiveMode == TimelineMode.Start)
            {
                headerPanel.Arrange(topRect);
                contentPresenter.Arrange(bottomRect);
            }
            else
            {
                contentPresenter.Arrange(topRect);
                headerPanel.Arrange(bottomRect);
            }
        }
        else if (IsLabelLayout)
        {
            var headerHeight  = headerPanel.DesiredSize.Height;
            var contentHeight = contentPresenter.DesiredSize.Height;
            if (effectiveMode == TimelineMode.Start)
            {
                var offsetY = 0d;
                indicator.Arrange(new Rect(0, offsetY, finalSize.Width, indicatorHeight));
                offsetY += indicatorHeight;
                if (headerHeight > 0)
                {
                    offsetY += IndicatorSpacing;
                    headerPanel.Arrange(new Rect(0, offsetY, finalSize.Width, headerHeight));
                    offsetY += headerHeight;
                }
                if (contentHeight > 0)
                {
                    offsetY += IndicatorSpacing;
                    contentPresenter.Arrange(new Rect(0, offsetY, finalSize.Width, contentHeight));
                }
            }
            else
            {
                var offsetY = 0d;
                if (headerHeight > 0)
                {
                    headerPanel.Arrange(new Rect(0, offsetY, finalSize.Width, headerHeight));
                    offsetY += headerHeight + IndicatorSpacing;
                }
                if (contentHeight > 0)
                {
                    contentPresenter.Arrange(new Rect(0, offsetY, finalSize.Width, contentHeight));
                    offsetY += contentHeight + IndicatorSpacing;
                }
                indicator.Arrange(new Rect(0, offsetY, finalSize.Width, indicatorHeight));
            }
        }
        else
        {
            var spacing       = contentPresenter.DesiredSize.Height > 0 ? IndicatorSpacing : 0;
            var contentHeight = Math.Max(0, finalSize.Height - indicatorHeight - spacing);
            if (effectiveMode == TimelineMode.Start)
            {
                indicator.Arrange(new Rect(0, 0, finalSize.Width, indicatorHeight));
                contentPresenter.Arrange(new Rect(
                    0,
                    indicatorHeight + spacing,
                    finalSize.Width,
                    contentHeight));
            }
            else
            {
                contentPresenter.Arrange(new Rect(0, 0, finalSize.Width, contentHeight));
                indicator.Arrange(new Rect(
                    0,
                    contentHeight + spacing,
                    finalSize.Width,
                    indicatorHeight));
            }
        }
    }

    private TimelineMode GetEffectiveMode()
    {
        if (Mode != TimelineMode.Alternate)
        {
            return Mode;
        }

        return IsOdd ? TimelineMode.End : TimelineMode.Start;
    }

    private TimelineIndicator GetTimelineIndicator()
    {
        TimelineIndicator? target = null;
        foreach (var child in LogicalChildren)
        {
            if (child is TimelineIndicator indicator)
            {
                target = indicator;
            }
        }

        Debug.Assert(target != null, "TimelineSectionPanel: TimelineIndicator is null");
        return target;
    }

    private StackPanel GetHeaderPanel()
    {
        StackPanel? target = null;
        foreach (var child in LogicalChildren)
        {
            if (child is StackPanel headerPanel)
            {
                target = headerPanel;
            }
        }

        Debug.Assert(target != null, "TimelineSectionPanel: header StackPanel is null");
        return target;
    }

    private ContentPresenter GetContentPresenter()
    {
        ContentPresenter? target = null;
        foreach (var child in LogicalChildren)
        {
            if (child is ContentPresenter contentPresenter)
            {
                target = contentPresenter;
            }
        }

        Debug.Assert(target != null, "TimelineSectionPanel: ContentPresenter is null");
        return target;
    }
}
