using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Media;

namespace AtomUI.Controls.Commons;

[TemplatePart("PART_IndicatorSurface", typeof(Control))]
internal abstract class AbstractCountBadgeAdorner : TemplatedControl
{
    #region 公共属性定义

    public static readonly StyledProperty<IBrush?> BadgeColorProperty =
        AvaloniaProperty.Register<AbstractCountBadgeAdorner, IBrush?>(
            nameof(BadgeColor));

    public static readonly StyledProperty<Point> OffsetProperty =
        AvaloniaProperty.Register<AbstractCountBadgeAdorner, Point>(
            nameof(Offset));

    public static readonly StyledProperty<int> OverflowCountProperty =
        AvaloniaProperty.Register<AbstractCountBadgeAdorner, int>(nameof(OverflowCount));

    public static readonly StyledProperty<CountBadgeSize> SizeProperty =
        AvaloniaProperty.Register<AbstractCountBadgeAdorner, CountBadgeSize>(
            nameof(Size));
    
    public static readonly StyledProperty<TimeSpan> MotionDurationProperty =
        MotionAwareControlProperty.MotionDurationProperty.AddOwner<AbstractCountBadgeAdorner>();

    public IBrush? BadgeColor
    {
        get => GetValue(BadgeColorProperty);
        set => SetValue(BadgeColorProperty, value);
    }

    public Point Offset
    {
        get => GetValue(OffsetProperty);
        set => SetValue(OffsetProperty, value);
    }

    public int OverflowCount
    {
        get => GetValue(OverflowCountProperty);
        set => SetValue(OverflowCountProperty, value);
    }

    public CountBadgeSize Size
    {
        get => GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    public int Count
    {
        get => GetValue(CountProperty);
        set => SetValue(CountProperty, value);
    }
    
    public TimeSpan MotionDuration
    {
        get => GetValue(MotionDurationProperty);
        set => SetValue(MotionDurationProperty, value);
    }

    #endregion

    #region 内部属性定义

    internal static readonly DirectProperty<AbstractCountBadgeAdorner, bool> IsAdornerModeProperty =
        AvaloniaProperty.RegisterDirect<AbstractCountBadgeAdorner, bool>(
            nameof(IsAdornerMode),
            o => o.IsAdornerMode,
            (o, v) => o.IsAdornerMode = v);

    internal static readonly DirectProperty<AbstractCountBadgeAdorner, BoxShadows> BoxShadowProperty =
        AvaloniaProperty.RegisterDirect<AbstractCountBadgeAdorner, BoxShadows>(
            nameof(BoxShadow),
            o => o.BoxShadow,
            (o, v) => o.BoxShadow = v);

    internal static readonly DirectProperty<AbstractCountBadgeAdorner, string?> CountTextProperty =
        AvaloniaProperty.RegisterDirect<AbstractCountBadgeAdorner, string?>(
            nameof(CountText),
            o => o.CountText,
            (o, v) => o.CountText = v);

    internal static readonly StyledProperty<int> CountProperty =
        AvaloniaProperty.Register<AbstractCountBadgeAdorner, int>(
            nameof(Count));

    internal static readonly StyledProperty<IBrush?> BadgeShadowColorProperty =
        AvaloniaProperty.Register<AbstractCountBadgeAdorner, IBrush?>(
            nameof(BadgeShadowColor));

    internal static readonly StyledProperty<double> BadgeShadowSizeProperty =
        AvaloniaProperty.Register<AbstractCountBadgeAdorner, double>(
            nameof(BadgeShadowSize));

    internal static readonly StyledProperty<bool> IsMotionEnabledProperty
        = MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<AbstractCountBadgeAdorner>();

    private bool _isAdornerMode;

    internal bool IsAdornerMode
    {
        get => _isAdornerMode;
        set => SetAndRaise(IsAdornerModeProperty, ref _isAdornerMode, value);
    }

    private BoxShadows _boxShadow;

    internal BoxShadows BoxShadow
    {
        get => _boxShadow;
        set => SetAndRaise(BoxShadowProperty, ref _boxShadow, value);
    }

    private string? _countText;

    internal string? CountText
    {
        get => _countText;
        set => SetAndRaise(CountTextProperty, ref _countText, value);
    }

    internal IBrush? BadgeShadowColor
    {
        get => GetValue(BadgeShadowColorProperty);
        set => SetValue(BadgeShadowColorProperty, value);
    }

    internal double BadgeShadowSize
    {
        get => GetValue(BadgeShadowSizeProperty);
        set => SetValue(BadgeShadowSizeProperty, value);
    }

    internal bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    #endregion

    private Control? _indicator;
    private readonly BadgeMotionSession _motion;
    private bool _preserveCountTextForHide;

    protected AbstractCountBadgeAdorner()
    {
        _motion = new BadgeMotionSession(this);
    }

    static AbstractCountBadgeAdorner()
    {
        AffectsMeasure<AbstractCountBadgeAdorner>(OverflowCountProperty,
            SizeProperty,
            CountTextProperty,
            IsAdornerModeProperty,
            CornerRadiusProperty);
        AffectsRender<AbstractCountBadgeAdorner>(BadgeColorProperty, OffsetProperty);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _indicator = e.NameScope.Find<Control>("PART_IndicatorSurface");
        _motion.SetTarget(_indicator);
        BuildBoxShadow();
        BuildCountText();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        _motion.TryStart();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _motion.Detach();
    }

    private void BuildBoxShadow()
    {
        if (BadgeShadowColor is not null)
        {
            BoxShadow = new BoxShadows(new BoxShadow
            {
                OffsetX = 0,
                OffsetY = 0,
                Blur    = 0,
                Spread  = BadgeShadowSize,
                Color   = ((ISolidColorBrush)BadgeShadowColor).Color
            });
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IsMotionEnabledProperty || change.Property == MotionDurationProperty)
        {
            _motion.UpdateConfiguration(IsMotionEnabled, MotionDuration);
        }

        if (change.Property == BadgeShadowSizeProperty ||
            change.Property == BadgeShadowColorProperty)
        {
            BuildBoxShadow();
        }

        if (change.Property == CountProperty || change.Property == OverflowCountProperty)
        {
            if (change.Property == CountProperty && _preserveCountTextForHide)
            {
                _preserveCountTextForHide = false;
                return;
            }

            BuildCountText();
        }
    }

    private void BuildCountText()
    {
        CountText = Count > OverflowCount ? $"{OverflowCount}+" : $"{Count}";
    }

    internal void PreserveCountTextForHide()
    {
        _preserveCountTextForHide = true;
    }

    internal void RefreshCountText()
    {
        _preserveCountTextForHide = false;
        BuildCountText();
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var size = base.ArrangeOverride(finalSize);
        if (IsAdornerMode && _indicator is not null)
        {
            var offsetX       = Offset.X;
            var offsetY       = Offset.Y;
            var indicatorSize = _indicator.DesiredSize;
            offsetX += finalSize.Width - indicatorSize.Width / 2;
            offsetY -= indicatorSize.Height / 2;
            _indicator.Arrange(new Rect(new Point(offsetX, offsetY), indicatorSize));
        }

        return size;
    }

    internal void ApplyToTarget(AdornerLayer? adornerLayer, Control adorned, Action? onShowReady = null)
    {
        if (adornerLayer is not null &&
            (!adornerLayer.Children.Contains(this) || !ReferenceEquals(AdornerLayer.GetAdornedElement(this), adorned)))
        {
            DetachFromAdornerLayer(adornerLayer);

            AdornerLayer.SetAdornedElement(this, adorned);
            AdornerLayer.SetIsClipEnabled(this, true);
            ((ISetLogicalParent)this).SetParent(adorned);
            adornerLayer.Children.Add(this);
        }

        _motion.Show(IsMotionEnabled, MotionDuration, onShowReady);
    }

    internal void DetachFromTarget(AdornerLayer? adornerLayer,
                                   bool enableMotion = true,
                                   Action? onDetachCompleted = null)
    {
        _motion.Hide(enableMotion && IsMotionEnabled, MotionDuration, () =>
        {
            DetachFromAdornerLayer(adornerLayer);
            onDetachCompleted?.Invoke();
        });
    }

    private void DetachFromAdornerLayer(AdornerLayer? adornerLayer)
    {
        adornerLayer?.Children.Remove(this);
        if (((ILogical)this).LogicalParent is not null)
        {
            ((ISetLogicalParent)this).SetParent(null);
        }
        AdornerLayer.SetAdornedElement(this, null);
    }
}
