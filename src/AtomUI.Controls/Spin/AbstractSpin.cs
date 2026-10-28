using AtomUI.Animations;
using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Interactivity;
using Avalonia.Metadata;

namespace AtomUI.Controls.Commons;

public abstract class AbstractSpin : ContentControl, ICustomizableSizeTypeAware, IMotionAwareControl
{
    #region 公共属性定义

    public static readonly StyledProperty<CustomizableSizeType> SizeTypeProperty =
        CustomizableSizeTypeControlProperty.SizeTypeProperty.AddOwner<AbstractSpin>();

    public static readonly StyledProperty<string?> TipProperty =
        AvaloniaProperty.Register<AbstractSpin, string?>(nameof(Tip));

    public static readonly StyledProperty<bool> IsTipVisibleProperty =
        AvaloniaProperty.Register<AbstractSpin, bool>(nameof(IsTipVisible));
    
    public static readonly StyledProperty<object?> CustomIndicatorProperty =
        AvaloniaProperty.Register<AbstractSpin, object?>(nameof(CustomIndicator));

    public static readonly StyledProperty<IDataTemplate?> CustomIndicatorTemplateProperty =
        AvaloniaProperty.Register<AbstractSpin, IDataTemplate?>(nameof(CustomIndicatorTemplate));

    public static readonly StyledProperty<TimeSpan> MotionDurationProperty =
       MotionAwareControlProperty.MotionDurationProperty.AddOwner<AbstractSpin>();

    public static readonly StyledProperty<Easing?> MotionEasingCurveProperty =
        AvaloniaProperty.Register<AbstractSpin, Easing?>(nameof(MotionEasingCurve));

    public static readonly StyledProperty<bool> IsSpinningProperty =
        AvaloniaProperty.Register<AbstractSpin, bool>(nameof(IsSpinning));
    
    public static readonly StyledProperty<bool> IsMaskBlurEnabledProperty =
        AvaloniaProperty.Register<AbstractSpin, bool>(nameof(IsMaskBlurEnabled));
    
    public static readonly StyledProperty<bool> IsMaskBackgroundEnabledProperty =
        AvaloniaProperty.Register<AbstractSpin, bool>(nameof(IsMaskBackgroundEnabled));
    
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<AbstractSpin>();

    public CustomizableSizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }

    public string? Tip
    {
        get => GetValue(TipProperty);
        set => SetValue(TipProperty, value);
    }

    public bool IsTipVisible
    {
        get => GetValue(IsTipVisibleProperty);
        set => SetValue(IsTipVisibleProperty, value);
    }
    
    [DependsOn(nameof(CustomIndicatorTemplate))]
    public object? CustomIndicator
    {
        get => GetValue(CustomIndicatorProperty);
        set => SetValue(CustomIndicatorProperty, value);
    }
    
    public IDataTemplate? CustomIndicatorTemplate
    {
        get => GetValue(CustomIndicatorTemplateProperty);
        set => SetValue(CustomIndicatorTemplateProperty, value);
    }

    public TimeSpan MotionDuration
    {
        get => GetValue(MotionDurationProperty);
        set => SetValue(MotionDurationProperty, value);
    }

    public Easing? MotionEasingCurve
    {
        get => GetValue(MotionEasingCurveProperty);
        set => SetValue(MotionEasingCurveProperty, value);
    }

    public bool IsSpinning
    {
        get => GetValue(IsSpinningProperty);
        set => SetValue(IsSpinningProperty, value);
    }
    
    public bool IsMaskBlurEnabled
    {
        get => GetValue(IsMaskBlurEnabledProperty);
        set => SetValue(IsMaskBlurEnabledProperty, value);
    }
    
    public bool IsMaskBackgroundEnabled
    {
        get => GetValue(IsMaskBackgroundEnabledProperty);
        set => SetValue(IsMaskBackgroundEnabledProperty, value);
    }
    
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

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == CustomIndicatorTemplateProperty ||
            change.Property == CustomIndicatorProperty)
        {
            SetCurrentValue(IsCustomIndicatorProperty, CustomIndicator != null || CustomIndicatorTemplate != null);
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        SetCurrentValue(IsCustomIndicatorProperty, CustomIndicator != null || CustomIndicatorTemplate != null);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _transitionInitialization.Reset();
        base.OnDetachedFromVisualTree(e);
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        _transitionInitialization.CompleteInitialization();
    }
}
