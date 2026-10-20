using AtomUI.Controls.Primitives;
using AtomUI.Icons.AntDesign;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Media;

namespace AtomUI.Controls.Commons;

using AvaloniaButton = Avalonia.Controls.Button;

[TemplatePart("PART_BadgeLayout", typeof(Canvas))]
[PseudoClasses(ButtonPseudoClass.IconOnly)]
public abstract class AbstractFloatButton : AvaloniaButton, IMotionAwareControl
{
    #region 公共属性定义
    public static readonly StyledProperty<FloatButtonPlacement> PlacementProperty =
        AvaloniaProperty.Register<AbstractFloatButton, FloatButtonPlacement>(nameof(Placement));
    
    public static readonly StyledProperty<double> FloatOffsetXProperty =
        AvaloniaProperty.Register<AbstractFloatButton, double>(nameof(FloatOffsetX));
    
    public static readonly StyledProperty<double> FloatOffsetYProperty =
        AvaloniaProperty.Register<AbstractFloatButton, double>(nameof(FloatOffsetY));
    
    public static readonly StyledProperty<PathIcon?> IconProperty =
        AvaloniaProperty.Register<AbstractFloatButton, PathIcon?>(nameof(Icon));
    
    public static readonly StyledProperty<string?> TooltipProperty =
        AvaloniaProperty.Register<AbstractFloatButton, string?>(nameof(Tooltip));
    
    public static readonly StyledProperty<Color?> TooltipColorProperty =
        AvaloniaProperty.Register<AbstractFloatButton, Color?>(nameof(TooltipColor));
    
    public static readonly StyledProperty<FloatButtonType> ButtonTypeProperty =
        AvaloniaProperty.Register<AbstractFloatButton, FloatButtonType>(nameof(ButtonType), FloatButtonType.Default);
    
    public static readonly StyledProperty<FloatButtonShape> ShapeProperty =
        AvaloniaProperty.Register<AbstractFloatButton, FloatButtonShape>(nameof(Shape), FloatButtonShape.Circle);
    
    public static readonly StyledProperty<Uri?> HrefProperty =
        AvaloniaProperty.Register<AbstractFloatButton, Uri?>(nameof(Href));
    
    public static readonly StyledProperty<BoxShadows> BoxShadowProperty =
        Border.BoxShadowProperty.AddOwner<AbstractFloatButton>();
    
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<AbstractFloatButton>();
    
    public static readonly StyledProperty<bool> IsBadgeEnabledProperty =
        AvaloniaProperty.Register<AbstractFloatButton, bool>(nameof(IsBadgeEnabled));
    
    public static readonly StyledProperty<bool> IsDotBadgeProperty =
        AvaloniaProperty.Register<AbstractFloatButton, bool>(nameof(IsDotBadge));
    
    public static readonly StyledProperty<int> BadgeCountProperty =
        AvaloniaProperty.Register<AbstractFloatButton, int>(nameof(BadgeCount));
    
    public static readonly StyledProperty<string?> BadgeColorProperty =
        AvaloniaProperty.Register<AbstractFloatButton, string?>(
            nameof(BadgeColor));
    
    public static readonly StyledProperty<Point> BadgeOffsetProperty =
        AvaloniaProperty.Register<AbstractFloatButton, Point>(nameof(BadgeOffset));

    public static readonly StyledProperty<int> BadgeOverflowCountProperty =
        AvaloniaProperty.Register<AbstractFloatButton, int>(nameof(BadgeOverflowCount), 99,
            coerce: (o, v) => Math.Max(0, v));
    
    public FloatButtonPlacement Placement
    {
        get => GetValue(PlacementProperty);
        set => SetValue(PlacementProperty, value);
    }
    
    public double FloatOffsetX
    {
        get => GetValue(FloatOffsetXProperty);
        set => SetValue(FloatOffsetXProperty, value);
    }

    public double FloatOffsetY
    {
        get => GetValue(FloatOffsetYProperty);
        set => SetValue(FloatOffsetYProperty, value);
    }
    
    public PathIcon? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }
    
    public string? Tooltip
    {
        get => GetValue(TooltipProperty);
        set => SetValue(TooltipProperty, value);
    }
    
    public Color? TooltipColor
    {
        get => GetValue(TooltipColorProperty);
        set => SetValue(TooltipColorProperty, value);
    }
    
    public FloatButtonType ButtonType
    {
        get => GetValue(ButtonTypeProperty);
        set => SetValue(ButtonTypeProperty, value);
    }
    
    public FloatButtonShape Shape
    {
        get => GetValue(ShapeProperty);
        set => SetValue(ShapeProperty, value);
    }
    
    public Uri? Href
    {
        get => GetValue(HrefProperty);
        set => SetValue(HrefProperty, value);
    }
    
    public BoxShadows BoxShadow
    {
        get => GetValue(BoxShadowProperty);
        set => SetValue(BoxShadowProperty, value);
    }
    
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }
    
    public bool IsBadgeEnabled
    {
        get => GetValue(IsBadgeEnabledProperty);
        set => SetValue(IsBadgeEnabledProperty, value);
    }

    public bool IsDotBadge
    {
        get => GetValue(IsDotBadgeProperty);
        set => SetValue(IsDotBadgeProperty, value);
    }
    
    public int BadgeCount
    {
        get => GetValue(BadgeCountProperty);
        set => SetValue(BadgeCountProperty, value);
    }
    
    public string? BadgeColor
    {
        get => GetValue(BadgeColorProperty);
        set => SetValue(BadgeColorProperty, value);
    }
    
    public Point BadgeOffset
    {
        get => GetValue(BadgeOffsetProperty);
        set => SetValue(BadgeOffsetProperty, value);
    }

    public int BadgeOverflowCount
    {
        get => GetValue(BadgeOverflowCountProperty);
        set => SetValue(BadgeOverflowCountProperty, value);
    }
    #endregion

    #region 内部属性定义
    internal static readonly DirectProperty<AbstractFloatButton, bool> IsEmbedModeProperty =
        AvaloniaProperty.RegisterDirect<AbstractFloatButton, bool>(
            nameof(IsEmbedMode),
            o => o.IsEmbedMode,
            (o, v) => o.IsEmbedMode = v);
    
    internal static readonly StyledProperty<IBrush?> BadgeEffectiveColorProperty =
        AvaloniaProperty.Register<AbstractFloatButton, IBrush?>(
            nameof(BadgeEffectiveColor));

    private bool _isEmbedMode;

    internal bool IsEmbedMode
    {
        get => _isEmbedMode;
        set => SetAndRaise(IsEmbedModeProperty, ref _isEmbedMode, value);
    }
    
    internal IBrush? BadgeEffectiveColor
    {
        get => GetValue(BadgeEffectiveColorProperty);
        set => SetValue(BadgeEffectiveColorProperty, value);
    }

    #endregion
    
    private protected ScopeAwareOverlayLayer? _overlayLayer;
    private protected Canvas? _badgeLayout;
    private protected Control? _badge;

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);
        if (Shape == FloatButtonShape.Circle)
        {
            SetValue(CornerRadiusProperty, new CornerRadius(e.NewSize.Height / 2), BindingPriority.Template);
        }

        if (_overlayLayer != null)
        {
            CalculatePosition(this, _overlayLayer.Bounds.Size, Placement, FloatOffsetX, FloatOffsetY);
        }
        CalculateBadgePosition();
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _badgeLayout = e.NameScope.Find<Canvas>("PART_BadgeLayout");
        ConfigureBadge();
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var size       = base.MeasureOverride(availableSize);
        var effectSize = Math.Max(size.Width, size.Height);
        return new Size(effectSize, effectSize);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IconProperty ||
            change.Property == ContentProperty ||
            change.Property == ShapeProperty)
        {
            UpdatePseudoClasses();
        }

        if (change.Property == IsEmbedModeProperty ||
            change.Property == ParentProperty)
        {
            SetupParentLayer(Parent);
            if (_overlayLayer != null)
            {
                CalculatePosition(this, _overlayLayer.Bounds.Size, Placement, FloatOffsetX, FloatOffsetY);
            }
        }
        else if (change.Property == FloatOffsetXProperty ||
                 change.Property == FloatOffsetYProperty ||
                 change.Property == PlacementProperty)
        {
            if (_overlayLayer != null)
            {
                CalculatePosition(this, _overlayLayer.Bounds.Size, Placement, FloatOffsetX, FloatOffsetY);
            }
        }
        else if (change.Property == IsBadgeEnabledProperty ||
                 change.Property == IsDotBadgeProperty)

        {
            ConfigureBadge();
        }
        else if (change.Property == BadgeOffsetProperty)
        {
            CalculateBadgePosition();
        }
        else if (change.Property == BadgeColorProperty)
        {
            SetCurrentValue(BadgeEffectiveColorProperty, BadgeColorUtils.CalculateColor(BadgeColor));
        }
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        ConfigureDefaultIcon();

    }

    protected virtual void UpdatePseudoClasses()
    {
        if (Shape == FloatButtonShape.Square)
        {
            PseudoClasses.Set(ButtonPseudoClass.IconOnly, Icon is not null && Content is null);
        }
        else
        {
            PseudoClasses.Set(ButtonPseudoClass.IconOnly, true);
        }
    }

    protected virtual void ConfigureDefaultIcon()
    {
        if ((Shape == FloatButtonShape.Circle && Icon == null) ||
            (Shape == FloatButtonShape.Square && Content == null && Icon == null))
        {
            SetCurrentValue(IconProperty, new FileTextOutlined());
        }
    }

    private void SetupParentLayer(StyledElement? parent)
    {
        if (_overlayLayer != null)
        {
            _overlayLayer.SizeChanged -= HandleLayerSizeChanged;
        }
        if (!IsEmbedMode)
        {
            if (parent is ScopeAwareOverlayLayer scopeAwareOverlayLayer)
            {
                _overlayLayer                      =  scopeAwareOverlayLayer;
                scopeAwareOverlayLayer.SizeChanged += HandleLayerSizeChanged;
            }
        }
        else
        {
            _overlayLayer = null;
        }
    }

    private void HandleLayerSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        CalculatePosition(this, e.NewSize, Placement, FloatOffsetX, FloatOffsetY);
    }

    private protected abstract void ConfigureBadge();
    private protected virtual void CalculateBadgePosition()
    {
        if (IsBadgeEnabled && _badge != null)
        {
            var targetOffsetX = DesiredSize.Width;
            var targetOffsetY = 0.0d;
            targetOffsetX += BadgeOffset.X;
            targetOffsetY += BadgeOffset.Y;
            Canvas.SetLeft(_badge, targetOffsetX);
            Canvas.SetTop(_badge, targetOffsetY);
        }
    }

    internal static void CalculatePosition(Control floatControl, 
                                           Size layerSize, 
                                           FloatButtonPlacement placement,
                                           double offsetX,
                                           double offsetY)
    {
        var width         = floatControl.DesiredSize.Width;
        var height        = floatControl.DesiredSize.Height;
        var targetOffsetX = 0.0d;
        var targetOffsetY = 0.0d;
        if (placement == FloatButtonPlacement.Left)
        {
            targetOffsetY =  (layerSize.Height - height) / 2;
            targetOffsetX += offsetX;
            targetOffsetY += offsetY;
        }
        else if (placement == FloatButtonPlacement.Right)
        {
            targetOffsetX =  (layerSize.Width - width);
            targetOffsetY =  (layerSize.Height - height) / 2;
            targetOffsetX -= offsetX;
            targetOffsetY += offsetY;
        }
        else if (placement == FloatButtonPlacement.Top)
        {
            targetOffsetX =  (layerSize.Width - width) / 2;
            targetOffsetX += offsetX;
            targetOffsetY += offsetY;
        }
        else if (placement == FloatButtonPlacement.Bottom)
        {
            targetOffsetX =  (layerSize.Width - width) / 2;
            targetOffsetY =  (layerSize.Height - height);
            targetOffsetX += offsetX;
            targetOffsetY -= offsetY;
        }
        else if (placement == FloatButtonPlacement.TopLeft)
        {
            targetOffsetX =  0.0d;
            targetOffsetY =  0.0d;
            targetOffsetX += offsetX;
            targetOffsetY += offsetY;
        }
        else if (placement == FloatButtonPlacement.TopRight)
        {
            targetOffsetX =  (layerSize.Width - width);
            targetOffsetY =  0.0d;
            targetOffsetX -= offsetX;
            targetOffsetY += offsetY;
        }
        else if (placement == FloatButtonPlacement.BottomLeft)
        {
            targetOffsetX =  0.0d;
            targetOffsetY =  (layerSize.Height - height);
            targetOffsetX += offsetX;
            targetOffsetY -= offsetY;
        }
        else if (placement == FloatButtonPlacement.BottomRight)
        {
            targetOffsetX =  (layerSize.Width - width);
            targetOffsetY =  (layerSize.Height - height);
            targetOffsetX -= offsetX;
            targetOffsetY -= offsetY;
        }
        else if (placement == FloatButtonPlacement.Center)
        {
            targetOffsetX =  (layerSize.Width - width) / 2;
            targetOffsetY =  (layerSize.Height - height) / 2;
            targetOffsetX += offsetX;
            targetOffsetY += offsetY;
        }
        Canvas.SetLeft(floatControl, targetOffsetX);
        Canvas.SetTop(floatControl, targetOffsetY);
    }
}
