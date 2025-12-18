using AtomUI.Controls;
using AtomUI.Desktop.Controls.Themes;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Controls.Primitives;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

internal class TimelineIndicator : TemplatedControl
{
    #region 公共属性定义

    public static readonly StyledProperty<PathIcon?> IndicatorIconProperty =
        AvaloniaProperty.Register<TimelineIndicator, PathIcon?>(nameof(IndicatorIcon));

    public static readonly StyledProperty<IBrush?> IndicatorColorProperty =
        AvaloniaProperty.Register<TimelineIndicator, IBrush?>(nameof(IndicatorColor));

    public PathIcon? IndicatorIcon
    {
        get => GetValue(IndicatorIconProperty);
        set => SetValue(IndicatorIconProperty, value);
    }

    public IBrush? IndicatorColor
    {
        get => GetValue(IndicatorColorProperty);
        set => SetValue(IndicatorColorProperty, value);
    }

    #endregion

    #region 内部属性定义

    internal static readonly StyledProperty<double> RelativeLineHeightProperty =
        AvaloniaProperty.Register<TimelineIndicator, double>(nameof(RelativeLineHeight));

    internal static readonly DirectProperty<TimelineIndicator, bool> IsFirstProperty =
        AvaloniaProperty.RegisterDirect<TimelineIndicator, bool>(nameof(IsFirst),
            o => o.IsFirst,
            (o, v) => o.IsFirst = v);

    internal static readonly DirectProperty<TimelineIndicator, bool> IsLastProperty =
        AvaloniaProperty.RegisterDirect<TimelineIndicator, bool>(nameof(IsLast),
            o => o.IsLast,
            (o, v) => o.IsLast = v);

    internal static readonly DirectProperty<TimelineIndicator, double> IndicatorMinHeightProperty =
        AvaloniaProperty.RegisterDirect<TimelineIndicator, double>(nameof(RelativeLineHeight),
            o => o.IndicatorMinHeight,
            (o, v) => o.IndicatorMinHeight = v);
    
    internal static readonly StyledProperty<IBrush?> DefaultIndicatorColorProperty =
        AvaloniaProperty.Register<TimelineIndicator, IBrush?>(nameof(DefaultIndicatorColor));
    
    internal static readonly StyledProperty<double> IndicatorDotBorderWidthProperty =
        AvaloniaProperty.Register<TimelineIndicator, double>(nameof(IndicatorDotBorderWidth));
    
    internal static readonly StyledProperty<IBrush?> IndicatorTailColorProperty =
        AvaloniaProperty.Register<TimelineIndicator, IBrush?>(nameof(IndicatorTailColor));
    
    internal static readonly StyledProperty<double> IndicatorTailWidthProperty =
        AvaloniaProperty.Register<TimelineIndicator, double>(nameof(IndicatorTailWidth));
    
    internal static readonly StyledProperty<double> IndicatorDotSizeProperty =
        AvaloniaProperty.Register<TimelineIndicator, double>(nameof(IndicatorDotSize));
    

    internal static readonly DirectProperty<TimelineIndicator, bool> NextIsPendingProperty =
        AvaloniaProperty.RegisterDirect<TimelineIndicator, bool>(nameof(NextIsPending),
            o => o.NextIsPending,
            (o, v) => o.NextIsPending = v);
    
    internal double RelativeLineHeight
    {
        get => GetValue(RelativeLineHeightProperty);
        set => SetValue(RelativeLineHeightProperty, value);
    }

    internal IBrush? DefaultIndicatorColor
    {
        get => GetValue(DefaultIndicatorColorProperty);
        set => SetValue(DefaultIndicatorColorProperty, value);
    }

    private bool _isFirst;

    internal bool IsFirst
    {
        get => _isFirst;
        set => SetAndRaise(IsFirstProperty, ref _isFirst, value);
    }

    private bool _isLast;

    internal bool IsLast
    {
        get => _isLast;
        set => SetAndRaise(IsLastProperty, ref _isLast, value);
    }

    private double _indicatorMinHeight;

    internal double IndicatorMinHeight
    {
        get => _indicatorMinHeight;
        set => SetAndRaise(IndicatorMinHeightProperty, ref _indicatorMinHeight, value);
    }
    
    public double IndicatorDotBorderWidth
    {
        get => GetValue(IndicatorDotBorderWidthProperty);
        set => SetValue(IndicatorDotBorderWidthProperty, value);
    }
    
    public IBrush? IndicatorTailColor
    {
        get => GetValue(IndicatorTailColorProperty);
        set => SetValue(IndicatorTailColorProperty, value);
    }
    
    public double IndicatorTailWidth
    {
        get => GetValue(IndicatorTailWidthProperty);
        set => SetValue(IndicatorTailWidthProperty, value);
    }
    
    public double IndicatorDotSize
    {
        get => GetValue(IndicatorDotSizeProperty);
        set => SetValue(IndicatorDotSizeProperty, value);
    }

    private bool _nextIsPending;

    internal bool NextIsPending
    {
        get => _nextIsPending;
        set => SetAndRaise(NextIsPendingProperty, ref _nextIsPending, value);
    }
    
    #endregion

    private IconPresenter? _iconPresenter;
    
    static TimelineIndicator()
    {
        AffectsMeasure<TimelineIndicator>(IsFirstProperty, IsLastProperty, IndicatorIconProperty, IndicatorMinHeightProperty);
        AffectsRender<TimelineIndicator>(IndicatorColorProperty, IndicatorDotBorderWidthProperty, IndicatorDotSizeProperty,
            IndicatorTailWidthProperty, IndicatorTailColorProperty);
        TextElement.FontSizeProperty.Changed.AddClassHandler<TimelineIndicator>((indicator, args) =>
        {
            indicator.IndicatorMinHeight = args.GetNewValue<double>() * indicator.RelativeLineHeight;
        });
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        IndicatorColor ??= DefaultIndicatorColor;
        _iconPresenter =   e.NameScope.Find<IconPresenter>(TimelineIndicatorThemeConstants.IconPresenterPart);
    }
    
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == RelativeLineHeightProperty)
        {
            IndicatorMinHeight = RelativeLineHeight * TextElement.GetFontSize(this);
        }
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var size   = base.MeasureOverride(availableSize);
        var height = Math.Max(size.Height, IndicatorMinHeight);
        return new Size(size.Width, height);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        if (_iconPresenter is not null)
        {
            var iconWidth = _iconPresenter.DesiredSize.Width;
            var iconHeight = _iconPresenter.DesiredSize.Height;
            var offsetX   = (finalSize.Width - iconWidth) / 2;
            var offsetY   = (_indicatorMinHeight - iconHeight) / 2;
            _iconPresenter.Arrange(new Rect(offsetX, offsetY, iconWidth, iconHeight));
        }

        return finalSize;
    }

    public override void Render(DrawingContext context)
    {
        var dotBelowLineStartOffsetY = 0d;
        var dotUpLineEndOffsetY      = 0d;
        if (IndicatorIcon == null)
        {
            // 绘制内置的
            var dotPen    = new Pen(IndicatorColor ?? DefaultIndicatorColor, IndicatorDotBorderWidth);
            var dotRadius = IndicatorDotSize / 2;
            var centerX   = (Bounds.Width - IndicatorDotSize) / 2 + dotRadius;
            var centerY   = (IndicatorMinHeight - IndicatorDotSize) / 2 + dotRadius - 1;
            dotBelowLineStartOffsetY = centerY + dotRadius + IndicatorDotBorderWidth / 2;
            dotUpLineEndOffsetY      = centerY - dotRadius - IndicatorDotBorderWidth / 2;
            context.DrawEllipse(null, dotPen, new Point(centerX, centerY), dotRadius, dotRadius);
        }
        else if (_iconPresenter != null)
        {
            dotBelowLineStartOffsetY = _iconPresenter.Bounds.Bottom;
            dotUpLineEndOffsetY      = _iconPresenter.Bounds.Top;
        }

        var lineOffsetX = Bounds.Width / 2;
        var linePen     = new Pen(IndicatorTailColor, IndicatorTailWidth);
        if (!IsLast)
        {
            var dotBelowLineStartPoint = new Point(lineOffsetX, dotBelowLineStartOffsetY);
            var dotBelowLineEndPoint   = new Point(lineOffsetX, Bounds.Bottom);
            context.DrawLine(linePen, dotBelowLineStartPoint, dotBelowLineEndPoint);
        }

        if (!IsFirst)
        {
            var dotUpLineStartPoint = new Point(lineOffsetX, 0);
            var dotUpLineEndPoint   = new Point(lineOffsetX, dotUpLineEndOffsetY);
            context.DrawLine(linePen, dotUpLineStartPoint, dotUpLineEndPoint);
        }
    }
}