using AtomUI.Animations;
using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Interactivity;
using Avalonia.Metadata;

namespace AtomUI.Controls.Commons;

/// <summary>
/// Base of controls that cover their content with a loading mask while <see cref="IsSpinning" /> is true. The
/// template of the derived control places an <see cref="AbstractSpinIndicator" /> and the <see cref="Tip" /> over
/// the content, and forwards <see cref="SizeType" />, <see cref="MotionDuration" />,
/// <see cref="MotionEasingCurve" /> and the custom indicator to that indicator. While spinning, the content is
/// dimmed or blurred and can get a mask background; see <see cref="IsMaskBlurEnabled" /> and
/// <see cref="IsMaskBackgroundEnabled" />.
/// </summary>
public abstract class AbstractSpin : ContentControl, ICustomizableSizeTypeAware, IMotionAwareControl
{
    #region 公共属性定义

    /// <summary>
    /// Identifies the <see cref="SizeType" /> property.
    /// </summary>
    public static readonly StyledProperty<CustomizableSizeType> SizeTypeProperty =
        CustomizableSizeTypeControlProperty.SizeTypeProperty.AddOwner<AbstractSpin>();

    /// <summary>
    /// Identifies the <see cref="Tip" /> property.
    /// </summary>
    public static readonly StyledProperty<string?> TipProperty =
        AvaloniaProperty.Register<AbstractSpin, string?>(nameof(Tip));

    /// <summary>
    /// Identifies the <see cref="IsTipVisible" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsTipVisibleProperty =
        AvaloniaProperty.Register<AbstractSpin, bool>(nameof(IsTipVisible));
    
    /// <summary>
    /// Identifies the <see cref="CustomIndicator" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> CustomIndicatorProperty =
        AvaloniaProperty.Register<AbstractSpin, object?>(nameof(CustomIndicator));

    /// <summary>
    /// Identifies the <see cref="CustomIndicatorTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> CustomIndicatorTemplateProperty =
        AvaloniaProperty.Register<AbstractSpin, IDataTemplate?>(nameof(CustomIndicatorTemplate));

    /// <summary>
    /// Identifies the <see cref="MotionDuration" /> property.
    /// </summary>
    public static readonly StyledProperty<TimeSpan> MotionDurationProperty =
       MotionAwareControlProperty.MotionDurationProperty.AddOwner<AbstractSpin>();

    /// <summary>
    /// Identifies the <see cref="MotionEasingCurve" /> property.
    /// </summary>
    public static readonly StyledProperty<Easing?> MotionEasingCurveProperty =
        AvaloniaProperty.Register<AbstractSpin, Easing?>(nameof(MotionEasingCurve));

    /// <summary>
    /// Identifies the <see cref="IsSpinning" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsSpinningProperty =
        AvaloniaProperty.Register<AbstractSpin, bool>(nameof(IsSpinning));
    
    /// <summary>
    /// Identifies the <see cref="IsMaskBlurEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsMaskBlurEnabledProperty =
        AvaloniaProperty.Register<AbstractSpin, bool>(nameof(IsMaskBlurEnabled));
    
    /// <summary>
    /// Identifies the <see cref="IsMaskBackgroundEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsMaskBackgroundEnabledProperty =
        AvaloniaProperty.Register<AbstractSpin, bool>(nameof(IsMaskBackgroundEnabled));
    
    /// <summary>
    /// Identifies the <see cref="IsMotionEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<AbstractSpin>();

    /// <summary>
    /// Size step of the loading indicator, set on the control and forwarded to the inner indicator. The inner
    /// indicator takes the width and height of the indicator area and of its dots from the tokens of that step, and
    /// gives a custom indicator that is a <see cref="PathIcon" /> the same width and height;
    /// <see cref="CustomizableSizeType.Custom" /> uses the middle values. The default is
    /// <see cref="CustomizableSizeType.Middle" />.
    /// </summary>
    public CustomizableSizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }

    /// <summary>
    /// Text shown below the loading indicator while the control spins and <see cref="IsTipVisible" /> is true.
    /// <see langword="null" /> shows no text.
    /// </summary>
    public string? Tip
    {
        get => GetValue(TipProperty);
        set => SetValue(TipProperty, value);
    }

    /// <summary>
    /// Shows <see cref="Tip" /> below the loading indicator; while false, the tip takes no space. The tip belongs to
    /// the loading mask, so it is shown only while the control spins. The default is false.
    /// </summary>
    public bool IsTipVisible
    {
        get => GetValue(IsTipVisibleProperty);
        set => SetValue(IsTipVisibleProperty, value);
    }
    
    /// <summary>
    /// Content shown in place of the built-in loading dots, set on the control and forwarded to the inner indicator.
    /// Use <see cref="CustomIndicatorTemplate" /> to build the content from a data object. The custom content
    /// replaces the dots only while this value is not <see langword="null" />; <see langword="null" /> shows the
    /// built-in dots.
    /// </summary>
    [DependsOn(nameof(CustomIndicatorTemplate))]
    public object? CustomIndicator
    {
        get => GetValue(CustomIndicatorProperty);
        set => SetValue(CustomIndicatorProperty, value);
    }
    
    /// <summary>
    /// Template that builds the custom indicator from <see cref="CustomIndicator" />, set on the control and
    /// forwarded to the inner indicator. With the Spin theme the built content replaces the built-in dots only while
    /// <see cref="CustomIndicator" /> is not <see langword="null" />, so a template on its own has no effect.
    /// </summary>
    public IDataTemplate? CustomIndicatorTemplate
    {
        get => GetValue(CustomIndicatorTemplateProperty);
        set => SetValue(CustomIndicatorTemplateProperty, value);
    }

    /// <summary>
    /// Duration of one cycle of the loading indicator animation, set on the control and forwarded to the inner
    /// indicator. The Spin theme binds it to the <c>IndicatorDuration</c> token. Values below 1 ms are treated as
    /// 1 ms, and a change restarts the animation.
    /// </summary>
    public TimeSpan MotionDuration
    {
        get => GetValue(MotionDurationProperty);
        set => SetValue(MotionDurationProperty, value);
    }

    /// <summary>
    /// Easing of the loading indicator animation, set on the control and forwarded to the inner indicator. The Spin
    /// theme sets <see cref="LinearEasing" />, and <see langword="null" /> also animates linearly. A change restarts
    /// the animation.
    /// </summary>
    public Easing? MotionEasingCurve
    {
        get => GetValue(MotionEasingCurveProperty);
        set => SetValue(MotionEasingCurveProperty, value);
    }

    /// <summary>
    /// Switches the loading state on or off. While true, a mask with the loading indicator and the tip is shown over
    /// the content, and <see cref="IsMaskBlurEnabled" /> and <see cref="IsMaskBackgroundEnabled" /> apply; while
    /// false, the content is shown plain. The default is false.
    /// </summary>
    public bool IsSpinning
    {
        get => GetValue(IsSpinningProperty);
        set => SetValue(IsSpinningProperty, value);
    }
    
    /// <summary>
    /// Lets the Spin theme blur the content while the control spins. While false, the theme dims the content to half
    /// opacity instead. The default is false. It has no effect while <see cref="IsSpinning" /> is false.
    /// </summary>
    public bool IsMaskBlurEnabled
    {
        get => GetValue(IsMaskBlurEnabledProperty);
        set => SetValue(IsMaskBlurEnabledProperty, value);
    }
    
    /// <summary>
    /// Lets the Spin theme paint the mask behind the loading indicator with the <c>ColorBgMask</c> token while the
    /// control spins. While false, the mask stays transparent. The default is false. It has no effect while
    /// <see cref="IsSpinning" /> is false.
    /// </summary>
    public bool IsMaskBackgroundEnabled
    {
        get => GetValue(IsMaskBackgroundEnabledProperty);
        set => SetValue(IsMaskBackgroundEnabledProperty, value);
    }
    
    /// <summary>
    /// Allows the Spin theme to animate the loading state: while true, the dimming of the content fades in and out
    /// when spinning starts or ends; while false, it changes at once. The Spin theme binds the property to the
    /// <c>EnableMotion</c> token. The value is not passed to the inner <see cref="AbstractSpinIndicator" />, whose
    /// own <see cref="AbstractSpinIndicator.IsMotionEnabled" /> decides whether the dots animate.
    /// </summary>
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    #endregion

    #region 内部属性定义

    internal static readonly DirectProperty<AbstractSpin, bool> IsCustomIndicatorProperty =
        AvaloniaProperty.RegisterDirect<AbstractSpin, bool>(nameof(IsCustomIndicator),
            o => o.IsCustomIndicator,
            (o, v) => o.IsCustomIndicator = v);
    
    private bool _isCustomIndicator;

    internal bool IsCustomIndicator
    {
        get => _isCustomIndicator;
        set => SetAndRaise(IsCustomIndicatorProperty, ref _isCustomIndicator, value);
    }
    
    internal static readonly StyledProperty<double> MaskOpacityProperty =
        AvaloniaProperty.Register<AbstractSpin, double>(nameof(MaskOpacity));

    /// <summary>
    /// Opacity of the content of the Spin, from 0 to 1. The theme binds the content presenter to it; the value is 1,
    /// except while the control spins with blur disabled, when it is 0.5. While <see cref="IsMotionEnabled" /> is
    /// <see langword="true" />, a change fades over the <c>MotionDurationMid</c> token.
    /// </summary>
    internal double MaskOpacity
    {
        get => GetValue(MaskOpacityProperty);
        set => SetValue(MaskOpacityProperty, value);
    }
    
    #endregion
    
    private readonly TransitionInitializationSession _transitionInitialization;

    protected AbstractSpin()
    {
        _transitionInitialization = new TransitionInitializationSession(this);
    }

    /// <summary>
    /// Updates the internal <see cref="IsCustomIndicatorProperty" /> flag when <see cref="CustomIndicator" /> or
    /// <see cref="CustomIndicatorTemplate" /> changes. Overrides must call the base member.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == CustomIndicatorTemplateProperty ||
            change.Property == CustomIndicatorProperty)
        {
            SetCurrentValue(IsCustomIndicatorProperty, CustomIndicator != null || CustomIndicatorTemplate != null);
        }
    }

    /// <summary>
    /// Applies the template and computes the internal <see cref="IsCustomIndicatorProperty" /> flag from
    /// <see cref="CustomIndicator" /> and <see cref="CustomIndicatorTemplate" />. Overrides must call the base
    /// member.
    /// </summary>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        SetCurrentValue(IsCustomIndicatorProperty, CustomIndicator != null || CustomIndicatorTemplate != null);
    }

    /// <summary>
    /// Suppresses the transitions of the control again when it is detached, so that property changes made while it
    /// is detached do not animate. <see cref="OnLoaded(RoutedEventArgs)" /> releases the suppression. Overrides must
    /// call the base member.
    /// </summary>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _transitionInitialization.Reset();
        base.OnDetachedFromVisualTree(e);
    }

    /// <summary>
    /// Releases the transition suppression once the control is loaded, so that later property changes animate.
    /// Overrides must call the base member.
    /// </summary>
    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        _transitionInitialization.CompleteInitialization();
    }
}
