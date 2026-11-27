using Avalonia;

namespace AtomUI.Controls.Commons;

using AvaloniaCheckBox = Avalonia.Controls.CheckBox;

/// <summary>
/// Base class of the AtomUI check boxes. It adds state change motion, click wave feedback and form item
/// integration to the Avalonia check box.
/// </summary>
public abstract class AbstractCheckBox : AvaloniaCheckBox, 
                                         IWaveSpiritAwareControl,
                                         IFormItemAware
{
    #region 公共属性定义

    /// <summary>
    /// Identifies the <see cref="IsMotionEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<AbstractCheckBox>();

    /// <summary>
    /// Identifies the <see cref="IsWaveSpiritEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsWaveSpiritEnabledProperty =
        WaveSpiritAwareControlProperty.IsWaveSpiritEnabledProperty.AddOwner<AbstractCheckBox>();
    
    /// <summary>
    /// Enables the animated transitions of the indicator when the checked state changes. When the value is not set,
    /// the theme supplies it from the <c>EnableMotion</c> token.
    /// </summary>
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    /// <summary>
    /// Enables the wave feedback on the indicator when the check box changes to checked. The wave plays only while
    /// <see cref="IsMotionEnabled" /> is <see langword="true" /> and the check box is enabled. When the value is not
    /// set, the theme supplies it from the <c>EnableWaveSpirit</c> token.
    /// </summary>
    public bool IsWaveSpiritEnabled
    {
        get => GetValue(IsWaveSpiritEnabledProperty);
        set => SetValue(IsWaveSpiritEnabledProperty, value);
    }

    #endregion

    static AbstractCheckBox()
    {
        IsCheckedChangedEvent.AddClassHandler<AbstractCheckBox>((checkbox, args) => checkbox.HandleCheckedChanged());
    }
    
    #region 实现 FormItem 接口
    private EventHandler? _formValueChanged;

    /// <summary>
    /// Raised after <see cref="Avalonia.Controls.Primitives.ToggleButton.IsChecked" /> changed.
    /// </summary>
    event EventHandler? IFormItemAware.ValueChanged
    {
        add => _formValueChanged += value;
        remove => _formValueChanged -= value;
    }

    /// <summary>
    /// Called by the form system to assign a value to the check box.
    /// </summary>
    /// <param name="value">
    /// The value to assign: a <see cref="bool" /> or <see langword="null" />.
    /// </param>
    void IFormItemAware.SetFormValue(object? value) => NotifySetFormValue((bool?)value);

    /// <summary>
    /// Called by the form system to read the current value of the check box.
    /// </summary>
    /// <returns>
    /// The current value of <see cref="Avalonia.Controls.Primitives.ToggleButton.IsChecked" />.
    /// </returns>
    object? IFormItemAware.GetFormValue() => NotifyGetFormValue();

    /// <summary>
    /// Called by the form system to reset the check box. The check box ends up unchecked.
    /// </summary>
    void IFormItemAware.ClearFormValue() => NotifyClearFormValue();

    /// <summary>
    /// Called by the form system when the validation status of the form item changes.
    /// </summary>
    /// <param name="status">
    /// The current validation status.
    /// </param>
    void IFormItemAware.NotifyValidateStatus(FormValidateStatus status) => NotifyValidateStatus(status);
    
    private void HandleCheckedChanged()
    {
        _formValueChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Applies a value assigned by the form system. The default sets
    /// <see cref="Avalonia.Controls.Primitives.ToggleButton.IsChecked" /> with <c>SetCurrentValue</c>, so a binding on
    /// the property is kept.
    /// </summary>
    /// <param name="value">
    /// The value to assign, or <see langword="null" />.
    /// </param>
    protected virtual void NotifySetFormValue(bool? value)
    {
        SetCurrentValue(IsCheckedProperty, value);
    }

    /// <summary>
    /// Returns the value reported to the form system.
    /// </summary>
    /// <returns>
    /// The current value of <see cref="Avalonia.Controls.Primitives.ToggleButton.IsChecked" />.
    /// </returns>
    protected virtual bool? NotifyGetFormValue()
    {
        return IsChecked;
    }

    /// <summary>
    /// Resets the check box on behalf of the form system. The default unchecks it with <c>SetCurrentValue</c>.
    /// </summary>
    protected virtual void NotifyClearFormValue()
    {
        SetCurrentValue(IsCheckedProperty, false);
    }
    
    /// <summary>
    /// Called when the form system reports a new validation status. The default ignores the status.
    /// </summary>
    /// <param name="status">
    /// The validation status reported by the form system.
    /// </param>
    protected virtual void NotifyValidateStatus(FormValidateStatus status)
    {
    }
    #endregion
}
