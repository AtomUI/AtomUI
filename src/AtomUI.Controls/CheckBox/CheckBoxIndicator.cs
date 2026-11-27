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

    internal static readonly StyledProperty<bool> IsInteractionEffectEnabledProperty =
        InteractionEffectAwareControlProperty.IsInteractionEffectEnabledProperty.AddOwner<CheckBoxIndicator>();

    /// <summary>
    /// Motion switch of the click wave: the wave is played when <see cref="State" /> changes to checked only while
    /// this is <see langword="true" />, <see cref="IsWaveSpiritEnabled" /> is <see langword="true" /> and the
    /// indicator is enabled and loaded. The theme of the check box binds it to its own motion switch, and the theme
    /// of the indicator passes it to the wave decorator of the template.
    /// </summary>
    internal bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    /// <summary>
    /// Switch of the interaction feedback of the indicator; <see cref="IsMotionEnabled" /> states when it plays.
    /// The theme of the check box binds it to its own <c>IsInteractionEffectEnabled</c>.
    /// </summary>
    internal bool IsInteractionEffectEnabled
    {
        get => GetValue(IsInteractionEffectEnabledProperty);
        set => SetValue(IsInteractionEffectEnabledProperty, value);
    }

    #endregion
    

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


    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IsPointerOverProperty ||
            change.Property == StateProperty ||
            change.Property == IsEnabledProperty)
        {
            UpdatePseudoClasses();
        }
    }


}