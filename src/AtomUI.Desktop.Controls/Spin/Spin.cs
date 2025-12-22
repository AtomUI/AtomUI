using AtomUI.Controls;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Interactivity;
using Avalonia.Metadata;

namespace AtomUI.Desktop.Controls;

public class Spin : ContentControl, IMotionAwareControl
{
    #region 公共属性定义

    public static readonly StyledProperty<SizeType> SizeTypeProperty =
        SizeTypeControlProperty.SizeTypeProperty.AddOwner<Spin>();

    public static readonly StyledProperty<string?> TipProperty =
        AvaloniaProperty.Register<Spin, string?>(nameof(Tip));

    public static readonly StyledProperty<bool> IsShowTipProperty =
        AvaloniaProperty.Register<Spin, bool>(nameof(IsShowTip));
    
    public static readonly StyledProperty<object?> CustomIndicatorProperty =
        AvaloniaProperty.Register<Spin, object?>(nameof(CustomIndicator));

    public static readonly StyledProperty<IDataTemplate?> CustomIndicatorTemplateProperty =
        AvaloniaProperty.Register<Spin, IDataTemplate?>(nameof(CustomIndicatorTemplate));

    public static readonly StyledProperty<TimeSpan> MotionDurationProperty =
       MotionAwareControlProperty.MotionDurationProperty.AddOwner<Spin>();

    public static readonly StyledProperty<Easing?> MotionEasingCurveProperty =
        AvaloniaProperty.Register<Spin, Easing?>(nameof(MotionEasingCurve));

    public static readonly StyledProperty<bool> IsSpinningProperty =
        AvaloniaProperty.Register<Spin, bool>(nameof(IsSpinning));
    
    public static readonly StyledProperty<bool> IsMaskBlurEnabledProperty =
        AvaloniaProperty.Register<Spin, bool>(nameof(IsMaskBlurEnabled));
    
    public static readonly StyledProperty<bool> IsMaskBackgroundEnabledProperty =
        AvaloniaProperty.Register<Spin, bool>(nameof(IsMaskBackgroundEnabled));
    
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<Spin>();

    public SizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }

    public string? Tip
    {
        get => GetValue(TipProperty);
        set => SetValue(TipProperty, value);
    }

    public bool IsShowTip
    {
        get => GetValue(IsShowTipProperty);
        set => SetValue(IsShowTipProperty, value);
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

    internal static readonly DirectProperty<Spin, bool> IsCustomIndicatorProperty =
        AvaloniaProperty.RegisterDirect<Spin, bool>(nameof(IsCustomIndicator),
            o => o.IsCustomIndicator,
            (o, v) => o.IsCustomIndicator = v);
    
    private bool _isCustomIndicator;

    internal bool IsCustomIndicator
    {
        get => _isCustomIndicator;
        set => SetAndRaise(IsCustomIndicatorProperty, ref _isCustomIndicator, value);
    }
    
    internal static readonly StyledProperty<double> MaskOpacityProperty =
        AvaloniaProperty.Register<Spin, double>(nameof(MaskOpacity));

    internal double MaskOpacity
    {
        get => GetValue(MaskOpacityProperty);
        set => SetValue(MaskOpacityProperty, value);
    }
    
    #endregion

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
    
    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        ConfigureTransitions(false);
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);
        Transitions = null;
    }
    
    private void ConfigureTransitions(bool force)
    {
        if (IsMotionEnabled)
        {
            if (force || Transitions == null)
            {
                Transitions = [
                    TransitionUtils.CreateTransition<DoubleTransition>(MaskOpacityProperty)
                ];
            }
        }
        else
        {
            Transitions = null;
        }
    }
}