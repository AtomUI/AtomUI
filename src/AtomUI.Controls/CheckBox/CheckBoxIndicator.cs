using AtomUI.Controls.Primitives;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Threading;

namespace AtomUI.Controls;

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

    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(StdPseudoClass.Checked, State == CheckBoxIndicatorState.Checked);
        PseudoClasses.Set(StdPseudoClass.UnChecked, State == CheckBoxIndicatorState.Unchecked);
        PseudoClasses.Set(StdPseudoClass.Indeterminate, State == CheckBoxIndicatorState.Indeterminate);
    }

    private void PlayWaveSpirit()
    {
        if (_waveSpiritDecorator is null)
        {
            return;
        }

        var waveBrush = WaveSpiritDecorator.ResolveWaveSpiritBrush(BorderBrush, Background);
        if (waveBrush is not null)
        {
            _waveSpiritDecorator.WaveBrush = waveBrush;
        }
        else
        {
            _waveSpiritDecorator.ClearValue(WaveSpiritDecorator.WaveBrushProperty);
        }

        _waveSpiritDecorator.Play();
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
                IsLoaded &&
                PseudoClasses.Contains(StdPseudoClass.Checked))
            {
                Dispatcher.Post(PlayWaveSpirit);
            }
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _waveSpiritDecorator = e.NameScope.Find<WaveSpiritDecorator>(WaveSpiritDecorator.WaveSpiritPart);
    }

}