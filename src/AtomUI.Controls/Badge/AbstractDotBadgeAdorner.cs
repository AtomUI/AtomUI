using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Media;

namespace AtomUI.Controls.Commons;

[TemplatePart("PART_IndicatorSurface", typeof(Control))]
internal abstract class AbstractDotBadgeAdorner : TemplatedControl
{
    #region 公共属性定义

    public static readonly StyledProperty<IBrush?> BadgeDotColorProperty =
        AvaloniaProperty.Register<AbstractDotBadgeAdorner, IBrush?>(
            nameof(BadgeDotColor));

    public static readonly DirectProperty<AbstractDotBadgeAdorner, DotBadgeStatus?> StatusProperty =
        AvaloniaProperty.RegisterDirect<AbstractDotBadgeAdorner, DotBadgeStatus?>(
            nameof(Status),
            o => o.Status,
            (o, v) => o.Status = v);
    
    public static readonly DirectProperty<AbstractDotBadgeAdorner, string?> TextProperty =
        AvaloniaProperty.RegisterDirect<AbstractDotBadgeAdorner, string?>(
            nameof(Text),
            o => o.Text,
            (o, v) => o.Text = v);
    
    public static readonly DirectProperty<AbstractDotBadgeAdorner, bool> IsAdornerModeProperty =
        AvaloniaProperty.RegisterDirect<AbstractDotBadgeAdorner, bool>(
            nameof(IsAdornerMode),
            o => o.IsAdornerMode,
            (o, v) => o.IsAdornerMode = v);
    
    public static readonly StyledProperty<Point> OffsetProperty =
        AvaloniaProperty.Register<AbstractDotBadgeAdorner, Point>(
            nameof(Offset));

    public IBrush? BadgeDotColor
    {
        get => GetValue(BadgeDotColorProperty);
        set => SetValue(BadgeDotColorProperty, value);
    }

    private DotBadgeStatus? _status;

    public DotBadgeStatus? Status
    {
        get => _status;
        set => SetAndRaise(StatusProperty, ref _status, value);
    }

    private string? _text;

    public string? Text
    {
        get => _text;
        set => SetAndRaise(TextProperty, ref _text, value);
    }

    private bool _isAdornerMode;

    public bool IsAdornerMode
    {
        get => _isAdornerMode;
        set => SetAndRaise(IsAdornerModeProperty, ref _isAdornerMode, value);
    }

    public Point Offset
    {
        get => GetValue(OffsetProperty);
        set => SetValue(OffsetProperty, value);
    }

    #endregion
   
    #region 内部属性定义

    internal static readonly StyledProperty<TimeSpan> MotionDurationProperty =
        MotionAwareControlProperty.MotionDurationProperty.AddOwner<AbstractDotBadgeAdorner>();
    
    internal static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<AbstractDotBadgeAdorner>();
    
    internal TimeSpan MotionDuration
    {
        get => GetValue(MotionDurationProperty);
        set => SetValue(MotionDurationProperty, value);
    }
    
    internal bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }
    
    #endregion

    private Control? _indicator;
    private readonly BadgeMotionSession _motion;

    protected AbstractDotBadgeAdorner()
    {
        _motion = new BadgeMotionSession(this);
    }

    static AbstractDotBadgeAdorner()
    {
        AffectsMeasure<AbstractDotBadgeAdorner>(TextProperty, IsAdornerModeProperty);
    }
    
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _indicator = e.NameScope.Find<Control>("PART_IndicatorSurface");
        _motion.SetTarget(_indicator);
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

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IsMotionEnabledProperty || change.Property == MotionDurationProperty)
        {
            _motion.UpdateConfiguration(IsMotionEnabled, MotionDuration);
        }
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var size = base.ArrangeOverride(finalSize);
        if (IsAdornerMode && _indicator is not null)
        {
            var offsetX = Offset.X;
            var offsetY = Offset.Y;
            var dotSize = _indicator.Bounds.Size;
            offsetX += dotSize.Width / 3;
            offsetY -= dotSize.Height / 3;
            _indicator.Arrange(new Rect(new Point(offsetX, offsetY), dotSize));
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

    internal void ShowWithoutMotion()
    {
        _motion.Show(false, MotionDuration, null);
    }

    internal void DetachFromTargetAsync(AdornerLayer? adornerLayer,
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
