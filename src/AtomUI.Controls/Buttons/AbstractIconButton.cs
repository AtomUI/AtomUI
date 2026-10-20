using System.Reflection;
using AtomUI.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;

namespace AtomUI.Controls.Commons;

using AvaloniaButton = Avalonia.Controls.Button;
using IconControl = Icon;

public abstract class AbstractIconButton : AvaloniaButton, IMotionAwareControl
{
    #region 公共属性定义

    public static readonly StyledProperty<PathIcon?> IconProperty =
        AvaloniaProperty.Register<AbstractIconButton, PathIcon?>(nameof(Icon));

    public static readonly StyledProperty<IconAnimation> LoadingAnimationProperty =
        IconControl.LoadingAnimationProperty.AddOwner<AbstractIconButton>();

    public static readonly StyledProperty<TimeSpan> LoadingAnimationDurationProperty =
        IconControl.LoadingAnimationDurationProperty.AddOwner<AbstractIconButton>();
    
    public static readonly StyledProperty<IBrush?> IconBrushProperty =
        AvaloniaProperty.Register<AbstractIconButton, IBrush?>(
            nameof(IconBrush));

    public static readonly StyledProperty<double> IconWidthProperty =
        AvaloniaProperty.Register<AbstractIconButton, double>(nameof(IconWidth));

    public static readonly StyledProperty<double> IconHeightProperty = 
        AvaloniaProperty.Register<AbstractIconButton, double>(nameof(IconHeight));

    public static readonly StyledProperty<bool> IsMotionEnabledProperty = 
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<AbstractIconButton>();

    public PathIcon? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public IconAnimation LoadingAnimation
    {
        get => GetValue(LoadingAnimationProperty);
        set => SetValue(LoadingAnimationProperty, value);
    }

    public TimeSpan LoadingAnimationDuration
    {
        get => GetValue(LoadingAnimationDurationProperty);
        set => SetValue(LoadingAnimationDurationProperty, value);
    }

    public double IconWidth
    {
        get => GetValue(IconWidthProperty);
        set => SetValue(IconWidthProperty, value);
    }

    public double IconHeight
    {
        get => GetValue(IconHeightProperty);
        set => SetValue(IconHeightProperty, value);
    }

    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }
    
    public IBrush? IconBrush
    {
        get => GetValue(IconBrushProperty);
        set => SetValue(IconBrushProperty, value);
    }
    
    #endregion
    
    #region 内部属性定义
    
    internal static readonly StyledProperty<bool> IsPassthroughMouseEventProperty = 
        AvaloniaProperty.Register<AbstractIconButton, bool>(nameof(IsPassthroughMouseEvent));
    
    public bool IsPassthroughMouseEvent
    {
        get => GetValue(IsPassthroughMouseEventProperty);
        set => SetValue(IsPassthroughMouseEventProperty, value);
    }
    
    #endregion
    
    #region 反射信息定义
    private static readonly Lazy<FieldInfo> IsFlyoutOpenFieldInfo = new(() =>
        FixedMemberReflection.RequireField(
            typeof(AvaloniaButton).GetField("_isFlyoutOpen", BindingFlags.Instance | BindingFlags.NonPublic),
            typeof(AvaloniaButton),
            "_isFlyoutOpen"));
    #endregion

    static AbstractIconButton()
    {
        AffectsMeasure<AbstractIconButton>(IconProperty);
        AffectsRender<AbstractIconButton>(IconBrushProperty);
    }
    
    protected bool IsFlyoutOpen()
    {
        return IsFlyoutOpenFieldInfo.Value.GetValue(this) as bool? ?? false;
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        e.Handled = !IsPassthroughMouseEvent;
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        e.Handled = !IsPassthroughMouseEvent;
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        e.Handled = !IsPassthroughMouseEvent;
    }

}
