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

/// <summary>
/// Base class of buttons that show a single icon and no text, such as the close button of another control. Pointer
/// pressed, released and moved events are marked as handled unless <see cref="IsPassthroughMouseEvent" /> is set,
/// so the embedding control decides whether it still receives them.
/// </summary>
public abstract class AbstractIconButton : AvaloniaButton, IMotionAwareControl
{
    #region 公共属性定义

    /// <summary>
    /// Identifies the <see cref="Icon" /> property.
    /// </summary>
    public static readonly StyledProperty<PathIcon?> IconProperty =
        AvaloniaProperty.Register<AbstractIconButton, PathIcon?>(nameof(Icon));

    /// <summary>
    /// Identifies the <see cref="LoadingAnimation" /> property.
    /// </summary>
    public static readonly StyledProperty<IconAnimation> LoadingAnimationProperty =
        IconControl.LoadingAnimationProperty.AddOwner<AbstractIconButton>();

    /// <summary>
    /// Identifies the <see cref="LoadingAnimationDuration" /> property.
    /// </summary>
    public static readonly StyledProperty<TimeSpan> LoadingAnimationDurationProperty =
        IconControl.LoadingAnimationDurationProperty.AddOwner<AbstractIconButton>();
    
    /// <summary>
    /// Identifies the <see cref="IconBrush" /> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> IconBrushProperty =
        AvaloniaProperty.Register<AbstractIconButton, IBrush?>(
            nameof(IconBrush));

    /// <summary>
    /// Identifies the <see cref="IconWidth" /> property.
    /// </summary>
    public static readonly StyledProperty<double> IconWidthProperty =
        AvaloniaProperty.Register<AbstractIconButton, double>(nameof(IconWidth));

    /// <summary>
    /// Identifies the <see cref="IconHeight" /> property.
    /// </summary>
    public static readonly StyledProperty<double> IconHeightProperty = 
        AvaloniaProperty.Register<AbstractIconButton, double>(nameof(IconHeight));

    /// <summary>
    /// Identifies the <see cref="IsMotionEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsMotionEnabledProperty = 
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<AbstractIconButton>();

    /// <summary>
    /// Icon shown in the center of the button, sized by <see cref="IconWidth" /> and <see cref="IconHeight" /> and
    /// painted with <see cref="IconBrush" />.
    /// </summary>
    public PathIcon? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>
    /// Animation the icon plays while it is visible: <see cref="IconAnimation.Spin" /> or
    /// <see cref="IconAnimation.Pulse" /> for a loading indicator, <see cref="IconAnimation.None" /> for a static
    /// icon. The default theme forwards the value to the icon.
    /// </summary>
    public IconAnimation LoadingAnimation
    {
        get => GetValue(LoadingAnimationProperty);
        set => SetValue(LoadingAnimationProperty, value);
    }

    /// <summary>
    /// Duration of one cycle of <see cref="LoadingAnimation" />, forwarded to the icon. The default is one second.
    /// </summary>
    public TimeSpan LoadingAnimationDuration
    {
        get => GetValue(LoadingAnimationDurationProperty);
        set => SetValue(LoadingAnimationDurationProperty, value);
    }

    /// <summary>
    /// Brush that paints the icon. The default theme takes it from the icon color Token and replaces it in the
    /// pointer-over, pressed and disabled states; an explicit value overrides the Token and those state values.
    /// </summary>
    public IBrush? IconBrush
    {
        get => GetValue(IconBrushProperty);
        set => SetValue(IconBrushProperty, value);
    }

    /// <summary>
    /// Width of the icon. The default theme sets it from the icon size Token; an explicit value overrides that size.
    /// </summary>
    public double IconWidth
    {
        get => GetValue(IconWidthProperty);
        set => SetValue(IconWidthProperty, value);
    }

    /// <summary>
    /// Height of the icon. The default theme sets it from the icon size Token; an explicit value overrides that size.
    /// </summary>
    public double IconHeight
    {
        get => GetValue(IconHeightProperty);
        set => SetValue(IconHeightProperty, value);
    }

    /// <summary>
    /// Enables the background, icon brush and render transform transitions of the default theme, which initializes the
    /// value from the <c>EnableMotion</c> Token.
    /// </summary>
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }
    
    /// <summary>
    /// Lets an embedding control receive the pointer pressed, released and moved events of the button: when it is
    /// <see langword="true" />, those events are left unhandled after the base class processed them, and marked as
    /// handled otherwise.
    /// </summary>
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

    /// <summary>
    /// Ends the transition suppression that was started in the constructor once the button is loaded, so that later
    /// changes animate. Derived overrides must call the base member.
    /// </summary>
    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        _transitionInitialization.CompleteInitialization();
    }

    /// <summary>
    /// Restarts the transition suppression before the button is detached, so that its appearance is committed without
    /// transitions the next time it is attached. Derived overrides must call the base member.
    /// </summary>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _transitionInitialization.Reset();
        base.OnDetachedFromVisualTree(e);
    }

    /// <summary>
    /// Restarts the transition suppression when the template changes, so that the appearance the new template produces
    /// is committed without transitions. Derived overrides must call the base member.
    /// </summary>
    protected override void OnTemplateChanged(AvaloniaPropertyChangedEventArgs e)
    {
        _transitionInitialization?.Reset();
        base.OnTemplateChanged(e);
    }

    /// <summary>
    /// Measures with the base member and then ends the transition suppression if it is still active. Derived overrides
    /// must call the base member.
    /// </summary>
    protected override Size MeasureOverride(Size availableSize)
    {
        var size = base.MeasureOverride(availableSize);
        // Template changes invalidate measure even when several changes coalesce back
        // to the applied template and Avalonia skips OnApplyTemplate.
        _transitionInitialization.CompleteInitialization();
        return size;
    }
    
    /// <summary>
    /// Returns whether the flyout of this button is open. The value is read through reflection from a private field of
    /// the Avalonia base class.
    /// </summary>
    protected bool IsFlyoutOpen()
    {
        return IsFlyoutOpenFieldInfo.Value.GetValue(this) as bool? ?? false;
    }

    /// <summary>
    /// Processes the press with the base class and then marks the event as handled, unless
    /// <see cref="IsPassthroughMouseEvent" /> is <see langword="true" />.
    /// </summary>
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        e.Handled = !IsPassthroughMouseEvent;
    }

    /// <summary>
    /// Processes the release with the base class and then marks the event as handled, unless
    /// <see cref="IsPassthroughMouseEvent" /> is <see langword="true" />.
    /// </summary>
    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        e.Handled = !IsPassthroughMouseEvent;
    }

    /// <summary>
    /// Processes the move with the base class and then marks the event as handled, unless
    /// <see cref="IsPassthroughMouseEvent" /> is <see langword="true" />.
    /// </summary>
    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        e.Handled = !IsPassthroughMouseEvent;
    }

}
