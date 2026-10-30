using System.Reflection;
using AtomUI.Animations;
using AtomUI.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
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

    public IBrush? IconBrush
    {
        get => GetValue(IconBrushProperty);
        set => SetValue(IconBrushProperty, value);
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
    
    public bool IsPassthroughMouseEvent
    {
        get => GetValue(IsPassthroughMouseEventProperty);
        set => SetValue(IsPassthroughMouseEventProperty, value);
    }
    
    #endregion
    
    #region 内部属性定义
    
    internal static readonly StyledProperty<bool> IsPassthroughMouseEventProperty = 
        AvaloniaProperty.Register<AbstractIconButton, bool>(nameof(IsPassthroughMouseEvent));
    
    #endregion
    
    #region 反射信息定义
    private static readonly Lazy<FieldInfo> IsFlyoutOpenFieldInfo = new(() =>
        FixedMemberReflection.RequireField(
            typeof(AvaloniaButton).GetField("_isFlyoutOpen", BindingFlags.Instance | BindingFlags.NonPublic),
            typeof(AvaloniaButton),
            "_isFlyoutOpen"));
    #endregion

    private readonly TransitionInitializationSession _transitionInitialization;

    static AbstractIconButton()
    {
        AffectsMeasure<AbstractIconButton>(IconProperty);
        AffectsRender<AbstractIconButton>(IconBrushProperty);
    }

    protected AbstractIconButton()
    {
        // Parent templates assign IconBrush through styles after this control is attached,
        // so the initial appearance must commit without running the theme transition.
        _transitionInitialization = new TransitionInitializationSession(this);
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        _transitionInitialization.CompleteInitialization();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _transitionInitialization.Reset();
        base.OnDetachedFromVisualTree(e);
    }

    protected override void OnTemplateChanged(AvaloniaPropertyChangedEventArgs e)
    {
        _transitionInitialization?.Reset();
        base.OnTemplateChanged(e);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var size = base.MeasureOverride(availableSize);
        // Template changes invalidate measure even when several changes coalesce back
        // to the applied template and Avalonia skips OnApplyTemplate.
        _transitionInitialization.CompleteInitialization();
        return size;
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
