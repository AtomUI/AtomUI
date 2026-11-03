using System.Reactive.Disposables;
using System.Windows.Input;
using AtomUI.Controls.Primitives;
using AtomUI.Data;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Media;
using Avalonia.Metadata;

namespace AtomUI.Controls.Commons;

public abstract class AbstractFloatButtonHost : TemplatedControl, IMotionAwareControl
{
    #region 公共属性定义
    public static readonly StyledProperty<FloatButtonPlacement> PlacementProperty =
        AbstractFloatButton.PlacementProperty.AddOwner<AbstractFloatButtonHost>();
    
    public static readonly StyledProperty<double> FloatOffsetXProperty =
        AbstractFloatButton.FloatOffsetXProperty.AddOwner<AbstractFloatButtonHost>();
    
    public static readonly StyledProperty<double> FloatOffsetYProperty =
        AbstractFloatButton.FloatOffsetYProperty.AddOwner<AbstractFloatButtonHost>();
    
    public static readonly StyledProperty<PathIcon?> IconProperty =
        AbstractFloatButton.IconProperty.AddOwner<AbstractFloatButtonHost>();
    
    public static readonly StyledProperty<string?> TooltipProperty =
        AbstractFloatButton.TooltipProperty.AddOwner<AbstractFloatButtonHost>();
    
    public static readonly StyledProperty<Color?> TooltipColorProperty =
        AbstractFloatButton.TooltipColorProperty.AddOwner<AbstractFloatButtonHost>();
    
    public static readonly StyledProperty<object?> DescriptionProperty =
        AvaloniaProperty.Register<AbstractFloatButtonHost, object?>(nameof(Description));
    
    public static readonly StyledProperty<IDataTemplate?> DescriptionTemplateProperty =
        AvaloniaProperty.Register<AbstractFloatButtonHost, IDataTemplate?>(nameof(DescriptionTemplate));
    
    public static readonly StyledProperty<FloatButtonType> ButtonTypeProperty =
        AbstractFloatButton.ButtonTypeProperty.AddOwner<AbstractFloatButtonHost>();
    
    public static readonly StyledProperty<FloatButtonShape> ShapeProperty =
        AbstractFloatButton.ShapeProperty.AddOwner<AbstractFloatButtonHost>();
    
    public static readonly StyledProperty<Uri?> HrefProperty =
        AbstractFloatButton.HrefProperty.AddOwner<AbstractFloatButtonHost>();

    public static readonly StyledProperty<ICommand?> CommandProperty =
        Avalonia.Controls.Button.CommandProperty.AddOwner<AbstractFloatButtonHost>();

    public static readonly StyledProperty<object?> CommandParameterProperty =
        Avalonia.Controls.Button.CommandParameterProperty.AddOwner<AbstractFloatButtonHost>();
    
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        AbstractFloatButton.IsMotionEnabledProperty.AddOwner<AbstractFloatButtonHost>();
    
    public static readonly StyledProperty<bool> IsBadgeEnabledProperty =
        AbstractFloatButton.IsBadgeEnabledProperty.AddOwner<AbstractFloatButtonHost>();
    
    public static readonly StyledProperty<bool> IsDotBadgeProperty =
        AbstractFloatButton.IsDotBadgeProperty.AddOwner<AbstractFloatButtonHost>();
    
    public static readonly StyledProperty<int> BadgeCountProperty =
        AbstractFloatButton.BadgeCountProperty.AddOwner<AbstractFloatButtonHost>();
    
    public static readonly StyledProperty<string?> BadgeColorProperty =
        AbstractFloatButton.BadgeColorProperty.AddOwner<AbstractFloatButtonHost>();
    
    public static readonly StyledProperty<Point> BadgeOffsetProperty =
        AbstractFloatButton.BadgeOffsetProperty.AddOwner<AbstractFloatButtonHost>();

    public static readonly StyledProperty<int> BadgeOverflowCountProperty =
        AbstractFloatButton.BadgeOverflowCountProperty.AddOwner<AbstractFloatButtonHost>();
    
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
    
    [DependsOn(nameof(DescriptionTemplate))]
    public object? Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }
    
    public IDataTemplate? DescriptionTemplate
    {
        get => GetValue(DescriptionTemplateProperty);
        set => SetValue(DescriptionTemplateProperty, value);
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

    public ICommand? Command
    {
        get => GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public object? CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
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

    private protected AbstractFloatButton? FloatButton;
    private protected CompositeDisposable? Disposables;
    private ScopeAwareOverlayLayer? _overlayLayer;
    
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        PrepareScopedOverlayLayer();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        CleanupScopedOverlayLayer();
    }

    private void PrepareScopedOverlayLayer()
    {
        _overlayLayer = ScopeAwareOverlayLayer.GetLayer(this);
        Disposables?.Dispose();
        Disposables =   new CompositeDisposable();
        FloatButton ??= CreateFloatButton(Disposables);
        if (_overlayLayer is { } layer)
        {
            LogicalChildren.Add(FloatButton);
            layer.Children.Add(FloatButton);
        }
    }

    private void CleanupScopedOverlayLayer()
    {
        if (FloatButton != null)
        {
            _overlayLayer?.Children.Remove(FloatButton);
            LogicalChildren.Remove(FloatButton);
            Disposables?.Dispose();
            Disposables = null;
            FloatButton = null;
        }

        _overlayLayer = null;
    }

    protected abstract AbstractFloatButton CreateFloatButton(CompositeDisposable disposables);

    protected void BindFloatButtonProperties(AbstractFloatButton floatButton, CompositeDisposable disposables)
    {
        // This button is created at runtime in an external overlay, outside the host's ControlTemplate.
        disposables.Add(BindUtils.RelayBind(this, IconProperty, floatButton, AbstractFloatButton.IconProperty));
        disposables.Add(BindUtils.RelayBind(this, TooltipProperty, floatButton, AbstractFloatButton.TooltipProperty));
        disposables.Add(BindUtils.RelayBind(this, TooltipColorProperty, floatButton, AbstractFloatButton.TooltipColorProperty));
        disposables.Add(BindUtils.RelayBind(this, ButtonTypeProperty, floatButton, AbstractFloatButton.ButtonTypeProperty));
        disposables.Add(BindUtils.RelayBind(this, ShapeProperty, floatButton, AbstractFloatButton.ShapeProperty));
        disposables.Add(BindUtils.RelayBind(this, HrefProperty, floatButton, AbstractFloatButton.HrefProperty));
        disposables.Add(BindUtils.RelayBind(this, CommandProperty, floatButton, Avalonia.Controls.Button.CommandProperty));
        disposables.Add(BindUtils.RelayBind(this, CommandParameterProperty, floatButton, Avalonia.Controls.Button.CommandParameterProperty));
        disposables.Add(BindUtils.RelayBind(this, DataContextProperty, floatButton, DataContextProperty));
        disposables.Add(BindUtils.RelayBind(this, IsMotionEnabledProperty, floatButton, AbstractFloatButton.IsMotionEnabledProperty));
        disposables.Add(BindUtils.RelayBind(this, PlacementProperty, floatButton, AbstractFloatButton.PlacementProperty));
        disposables.Add(BindUtils.RelayBind(this, FloatOffsetXProperty, floatButton, AbstractFloatButton.FloatOffsetXProperty));
        disposables.Add(BindUtils.RelayBind(this, FloatOffsetYProperty, floatButton, AbstractFloatButton.FloatOffsetYProperty));
        disposables.Add(BindUtils.RelayBind(this, DescriptionProperty, floatButton, ContentControl.ContentProperty));
        disposables.Add(BindUtils.RelayBind(this, DescriptionTemplateProperty, floatButton, ContentControl.ContentTemplateProperty));
        disposables.Add(BindUtils.RelayBind(this, IsBadgeEnabledProperty, floatButton, AbstractFloatButton.IsBadgeEnabledProperty));
        disposables.Add(BindUtils.RelayBind(this, IsDotBadgeProperty, floatButton, AbstractFloatButton.IsDotBadgeProperty));
        disposables.Add(BindUtils.RelayBind(this, BadgeCountProperty, floatButton, AbstractFloatButton.BadgeCountProperty));
        disposables.Add(BindUtils.RelayBind(this, BadgeColorProperty, floatButton, AbstractFloatButton.BadgeColorProperty));
        disposables.Add(BindUtils.RelayBind(this, BadgeOffsetProperty, floatButton, AbstractFloatButton.BadgeOffsetProperty));
        disposables.Add(BindUtils.RelayBind(this, BadgeOverflowCountProperty, floatButton, AbstractFloatButton.BadgeOverflowCountProperty));
    }
}
