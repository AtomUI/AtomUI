using AtomUI.Animations;
using AtomUI.Controls;
using AtomUI.Desktop.Controls.Primitives;
using AtomUI.Theme.Styling;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

internal enum CheckBoxIndicatorState
{
    Checked,
    Indeterminate,
    Unchecked,
}

internal class CheckBoxIndicator : TemplatedControl
{
    #region 公共属性定义

    public static readonly StyledProperty<CheckBoxIndicatorState> StateProperty =
        AvaloniaProperty.Register<CheckBoxIndicator, CheckBoxIndicatorState>(nameof(State));
    
    public static readonly StyledProperty<IBrush?> CheckedMarkBrushProperty =
        AvaloniaProperty.Register<CheckBoxIndicator, IBrush?>(nameof(CheckedMarkBrush));

    public static readonly StyledProperty<IBrush?> TristateMarkBrushProperty =
        AvaloniaProperty.Register<CheckBoxIndicator, IBrush?>(nameof(TristateMarkBrush));

    public static readonly StyledProperty<double> TristateMarkSizeProperty =
        AvaloniaProperty.Register<CheckBoxIndicator, double>(nameof(TristateMarkSize));
    
    public static readonly StyledProperty<ITransform?> CheckedMarkRenderTransformProperty =
        AvaloniaProperty.Register<CheckBoxIndicator, ITransform?>(nameof (CheckedMarkRenderTransform));

    public CheckBoxIndicatorState State
    {
        get => GetValue(StateProperty);
        set => SetValue(StateProperty, value);
    }
    
    public IBrush? CheckedMarkBrush
    {
        get => GetValue(CheckedMarkBrushProperty);
        set => SetValue(CheckedMarkBrushProperty, value);
    }

    public IBrush? TristateMarkBrush
    {
        get => GetValue(TristateMarkBrushProperty);
        set => SetValue(TristateMarkBrushProperty, value);
    }

    public double TristateMarkSize
    {
        get => GetValue(TristateMarkSizeProperty);
        set => SetValue(TristateMarkSizeProperty, value);
    }
    
    public ITransform? CheckedMarkRenderTransform
    {
        get => GetValue(CheckedMarkRenderTransformProperty);
        set => SetValue(CheckedMarkRenderTransformProperty, value);
    }

    #endregion

    #region 内部属性定义

    internal static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<CheckBoxIndicator>();

    internal static readonly StyledProperty<bool> IsWaveSpiritEnabledProperty =
        WaveSpiritAwareControlProperty.IsWaveSpiritEnabledProperty.AddOwner<CheckBoxIndicator>();

    internal bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    internal bool IsWaveSpiritEnabled
    {
        get => GetValue(IsWaveSpiritEnabledProperty);
        set => SetValue(IsWaveSpiritEnabledProperty, value);
    }

    #endregion
    
    private WaveSpiritDecorator? _waveSpiritDecorator;

    static CheckBoxIndicator()
    {
        AffectsRender<CheckBoxIndicator>(
            StateProperty,
            CheckedMarkBrushProperty,
            TristateMarkBrushProperty);
        AffectsArrange<CheckBoxIndicator>(TristateMarkSizeProperty);
    }

    private void ConfigureTransitions(bool force)
    {
        if (IsMotionEnabled)
        {
            if (force || Transitions == null)
            {
                Transitions = [
                    TransitionUtils.CreateTransition<SolidColorBrushTransition>(BackgroundProperty),
                    TransitionUtils.CreateTransition<SolidColorBrushTransition>(BorderBrushProperty),
                    TransitionUtils.CreateTransition<SolidColorBrushTransition>(TristateMarkBrushProperty),
                    TransitionUtils.CreateTransition<TransformOperationsTransition>(CheckedMarkRenderTransformProperty, SharedTokenKey.MotionDurationMid,
                        new BackEaseOut())
                ];
            }
        }
        else
        {
            Transitions = null;
        }
    }
    
    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(StdPseudoClass.Checked, State == CheckBoxIndicatorState.Checked);
        PseudoClasses.Set(StdPseudoClass.UnChecked, State == CheckBoxIndicatorState.Unchecked);
        PseudoClasses.Set(StdPseudoClass.Indeterminate, State == CheckBoxIndicatorState.Indeterminate);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IsPointerOverProperty ||
            change.Property == StateProperty ||
            change.Property == IsEnabledProperty)
        {
            UpdatePseudoClasses();
            if (change.Property == StateProperty &&
                IsWaveSpiritEnabled &&
                IsEnabled &&
                IsMotionEnabled &&
                PseudoClasses.Contains(StdPseudoClass.Checked))
            {
                _waveSpiritDecorator?.Play();
            }
        }
        
        if (IsLoaded)
        {
            if (change.Property == IsMotionEnabledProperty)
            {
                ConfigureTransitions(true);
            }
        }
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

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _waveSpiritDecorator = e.NameScope.Find<WaveSpiritDecorator>(WaveSpiritDecorator.WaveSpiritPart);
    }
    
}