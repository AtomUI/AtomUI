using AtomUI.Animations;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace AtomUI.Controls.Commons;

/// <summary>
/// Base class of toggle buttons that show one icon while unchecked and another while checked, with no text.
/// </summary>
public abstract class AbstractToggleIconButton : ToggleButton, IMotionAwareControl
{
    #region 公共属性定义

    /// <summary>
    /// Identifies the <see cref="CheckedIcon" /> property.
    /// </summary>
    public static readonly StyledProperty<PathIcon?> CheckedIconProperty =
        AvaloniaProperty.Register<AbstractToggleIconButton, PathIcon?>(nameof(CheckedIcon));

    /// <summary>
    /// Identifies the <see cref="UnCheckedIcon" /> property.
    /// </summary>
    public static readonly StyledProperty<PathIcon?> UnCheckedIconProperty =
        AvaloniaProperty.Register<AbstractToggleIconButton, PathIcon?>(nameof(UnCheckedIcon));

    /// <summary>
    /// Identifies the <see cref="IconWidth" /> property.
    /// </summary>
    public static readonly StyledProperty<double> IconWidthProperty =
        AvaloniaProperty.Register<AbstractToggleIconButton, double>(nameof(IconWidth));

    /// <summary>
    /// Identifies the <see cref="IconHeight" /> property.
    /// </summary>
    public static readonly StyledProperty<double> IconHeightProperty =
        AvaloniaProperty.Register<AbstractToggleIconButton, double>(nameof(IconHeight));
        
    /// <summary>
    /// Identifies the <see cref="IconBrush" /> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> IconBrushProperty =
        AvaloniaProperty.Register<AbstractToggleIconButton, IBrush?>(
            nameof(IconBrush));
    
    /// <summary>
    /// Identifies the <see cref="NormalIconBrush" /> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> NormalIconBrushProperty =
        AvaloniaProperty.Register<AbstractToggleIconButton, IBrush?>(
            nameof(NormalIconBrush));
    
    /// <summary>
    /// Identifies the <see cref="ActiveIconBrush" /> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> ActiveIconBrushProperty =
        AvaloniaProperty.Register<AbstractToggleIconButton, IBrush?>(
            nameof(ActiveIconBrush));
    
    /// <summary>
    /// Identifies the <see cref="SelectedIconBrush" /> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> SelectedIconBrushProperty =
        AvaloniaProperty.Register<AbstractToggleIconButton, IBrush?>(
            nameof(SelectedIconBrush));
    
    /// <summary>
    /// Identifies the <see cref="DisabledIconBrush" /> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> DisabledIconBrushProperty =
        AvaloniaProperty.Register<AbstractToggleIconButton, IBrush?>(
            nameof(DisabledIconBrush));
    
    /// <summary>
    /// Identifies the <see cref="IsMotionEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsMotionEnabledProperty = 
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<AbstractToggleIconButton>();

    /// <summary>
    /// Icon shown while the button is checked, sized by <see cref="IconWidth" /> and <see cref="IconHeight" /> and
    /// painted with <see cref="IconBrush" />.
    /// </summary>
    public PathIcon? CheckedIcon
    {
        get => GetValue(CheckedIconProperty);
        set => SetValue(CheckedIconProperty, value);
    }

    /// <summary>
    /// Icon shown while the button is unchecked, sized by <see cref="IconWidth" /> and <see cref="IconHeight" /> and
    /// painted with <see cref="IconBrush" />.
    /// </summary>
    public PathIcon? UnCheckedIcon
    {
        get => GetValue(UnCheckedIconProperty);
        set => SetValue(UnCheckedIconProperty, value);
    }

    /// <summary>
    /// Width of both icons. The default theme sets it from the icon size Token; an explicit value overrides that size.
    /// </summary>
    public double IconWidth
    {
        get => GetValue(IconWidthProperty);
        set => SetValue(IconWidthProperty, value);
    }

    /// <summary>
    /// Height of both icons. The default theme sets it from the icon size Token; an explicit value overrides that size.
    /// </summary>
    public double IconHeight
    {
        get => GetValue(IconHeightProperty);
        set => SetValue(IconHeightProperty, value);
    }

    /// <summary>
    /// Brush that paints both icons. The default theme takes it from the icon color Token and replaces it in the
    /// pointer-over, pressed and disabled states; an explicit value overrides the Token and those state values.
    /// </summary>
    public IBrush? IconBrush
    {
        get => GetValue(IconBrushProperty);
        set => SetValue(IconBrushProperty, value);
    }
    
    /// <summary>
    /// Brush meant for the icon in the normal state. Neither the control nor the default theme reads it; both icons
    /// are painted with <see cref="IconBrush" />.
    /// </summary>
    public IBrush? NormalIconBrush
    {
        get => GetValue(NormalIconBrushProperty);
        set => SetValue(NormalIconBrushProperty, value);
    }

    /// <summary>
    /// Brush meant for the icon in the active state. Neither the control nor the default theme reads it; both icons
    /// are painted with <see cref="IconBrush" />.
    /// </summary>
    public IBrush? ActiveIconBrush
    {
        get => GetValue(ActiveIconBrushProperty);
        set => SetValue(ActiveIconBrushProperty, value);
    }

    /// <summary>
    /// Brush meant for the icon in the selected state. Neither the control nor the default theme reads it; both icons
    /// are painted with <see cref="IconBrush" />.
    /// </summary>
    public IBrush? SelectedIconBrush
    {
        get => GetValue(SelectedIconBrushProperty);
        set => SetValue(SelectedIconBrushProperty, value);
    }

    /// <summary>
    /// Brush meant for the icon while the button is disabled. Neither the control nor the default theme reads it; both
    /// icons are painted with <see cref="IconBrush" />.
    /// </summary>
    public IBrush? DisabledIconBrush
    {
        get => GetValue(DisabledIconBrushProperty);
        set => SetValue(DisabledIconBrushProperty, value);
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
    
    #endregion

    private readonly TransitionInitializationSession _transitionInitialization;

    static AbstractToggleIconButton()
    {
        AffectsMeasure<AbstractToggleIconButton>(CheckedIconProperty, UnCheckedIconProperty, IsCheckedProperty, IconWidthProperty, IconHeightProperty);
        AffectsRender<AbstractToggleIconButton>(IconBrushProperty);
    }

    protected AbstractToggleIconButton()
    {
        // Parent templates assign IconBrush and checked appearance through styles after
        // this control is attached, so the initial appearance must commit without transitions.
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
    /// Returns whether <paramref name="point" /> counts as a hit, by asking <see cref="NotifyHistTest" />.
    /// </summary>
    /// <param name="point">
    /// The point to test.
    /// </param>
    /// <returns>
    /// The result of <see cref="NotifyHistTest" />.
    /// </returns>
    public bool HitTest(Point point)
    {
        return NotifyHistTest(point);
    }

    /// <summary>
    /// Decides whether <paramref name="point" /> counts as a hit of the button. The base member always returns
    /// <see langword="true" />; override it to exclude areas. Called by <see cref="HitTest" />.
    /// </summary>
    /// <param name="point">
    /// The point to test.
    /// </param>
    /// <returns>
    /// <see langword="true" /> when the point hits the button.
    /// </returns>
    protected virtual bool NotifyHistTest(Point point)
    {
        return true;
    }

}
