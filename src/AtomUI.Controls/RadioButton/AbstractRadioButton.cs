using AtomUI.Animations;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;

namespace AtomUI.Controls.Commons;

using AvaloniaRadioButton = Avalonia.Controls.RadioButton;

/// <summary>
/// Base class of the radio button. It adds the motion and click wave switches to the Avalonia radio button and
/// lets a form item read and write the checked state.
/// </summary>
public abstract class AbstractRadioButton : AvaloniaRadioButton,
                                            IInteractionEffectAwareControl,
                                            IFormItemAware
{
    #region 公共属性定义

    /// <summary>
    /// Identifies the <see cref="IsMotionEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<AbstractRadioButton>();

    /// <summary>
    /// Identifies the <see cref="IsInteractionEffectEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsInteractionEffectEnabledProperty =
        InteractionEffectAwareControlProperty.IsInteractionEffectEnabledProperty.AddOwner<AbstractRadioButton>();
    
    /// <summary>
    /// Gets or sets whether changes of the indicator animate: the border brush, the background and the size of the
    /// inner dot. The theme forwards the value to the indicator and sets it from the <c>EnableMotion</c> token.
    /// </summary>
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    /// <summary>
    /// Gets or sets whether interaction feedback plays from the indicator when the radio button state changes.
    /// The theme forwards the value to the indicator and sets it from the <c>EnableInteractionEffect</c> token.
    /// </summary>
    /// <remarks>
    /// Feedback plays only while <see cref="IsMotionEnabled" /> is also <see langword="true" /> and the radio button
    /// is enabled.
    /// </remarks>
    public bool IsInteractionEffectEnabled
    {
        get => GetValue(IsInteractionEffectEnabledProperty);
        set => SetValue(IsInteractionEffectEnabledProperty, value);
    }

    #endregion

    private Control? _feedbackSurface;

    static AbstractRadioButton()
    {
        ToggleInteractionFeedback.Configure<AbstractRadioButton>(static owner => owner._feedbackSurface ?? owner,
            static owner => owner.IsChecked == true, quick: true);
    }

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        ToggleInteractionFeedback.Cancel(this);
        InteractionEffectHost.CancelFor(_feedbackSurface ?? this);
        base.OnApplyTemplate(e);
        _feedbackSurface = e.NameScope.Find<Control>("Indicator");
    }
    
    /// <summary>
    /// Runs the Avalonia handling and then marks the pointer-pressed event as unhandled again, so that it keeps
    /// bubbling to the ancestors of the radio button.
    /// </summary>
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        e.Handled = false;
    }
    
    #region 实现 FormItem 接口
    
    private EventHandler? _formValueChanged;
    event EventHandler? IFormItemAware.ValueChanged
    {
        add => _formValueChanged += value;
        remove => _formValueChanged -= value;
    }

    void IFormItemAware.SetFormValue(object? value) => NotifySetFormValue((bool?)value);

    object? IFormItemAware.GetFormValue() => NotifyGetFormValue();
    void IFormItemAware.ClearFormValue() => NotifyClearFormValue();
    void IFormItemAware.NotifyValidateStatus(FormValidateStatus status) => NotifyValidateStatus(status);
    
    private void HandleCheckedChanged()
    {
        _formValueChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Called when the <c>FormItem</c> that hosts the radio button sets or resets its value. The default implementation
    /// assigns <paramref name="value" /> to <see cref="Avalonia.Controls.Primitives.ToggleButton.IsChecked" /> while
    /// keeping an existing binding on it.
    /// </summary>
    /// <param name="value">
    /// The checked state to apply; <see langword="null" /> clears the checked state.
    /// </param>
    protected virtual void NotifySetFormValue(bool? value)
    {
        SetCurrentValue(IsCheckedProperty, value);
    }

    /// <summary>
    /// Called when the <c>FormItem</c> that hosts the radio button reads its value.
    /// </summary>
    /// <returns>
    /// The value of <see cref="Avalonia.Controls.Primitives.ToggleButton.IsChecked" />.
    /// </returns>
    protected virtual bool? NotifyGetFormValue()
    {
        return IsChecked;
    }

    /// <summary>
    /// Called when the <c>FormItem</c> that hosts the radio button resets and has no initial value. The default
    /// implementation sets <see cref="Avalonia.Controls.Primitives.ToggleButton.IsChecked" /> to
    /// <see langword="null" /> while keeping an existing binding on it.
    /// </summary>
    protected virtual void NotifyClearFormValue()
    {
        SetCurrentValue(IsCheckedProperty, null);
    }
    
    /// <summary>
    /// Called when the validation status of the <c>FormItem</c> that hosts the radio button changes, for example
    /// after a validation run. The default implementation does nothing.
    /// </summary>
    /// <param name="status">
    /// The status that the form item now shows.
    /// </param>
    protected virtual void NotifyValidateStatus(FormValidateStatus status)
    {
    }
    #endregion
}
