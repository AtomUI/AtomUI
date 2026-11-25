using Avalonia;

namespace AtomUI.Controls.Commons;

using AvaloniaCheckBox = Avalonia.Controls.CheckBox;

/// <summary>
/// AtomUI 复选框的抽象基类，提供动效、水波反馈以及表单项集成能力。
/// </summary>
public abstract class AbstractCheckBox : AvaloniaCheckBox, 
                                         IWaveSpiritAwareControl,
                                         IFormItemAware
{
    #region 公共属性定义

    /// <summary>
    /// 标识 <see cref="IsMotionEnabled"/> 是否启用的 Avalonia 样式属性。
    /// </summary>
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<AbstractCheckBox>();

    /// <summary>
    /// 标识 <see cref="IsWaveSpiritEnabled"/> 是否启用的 Avalonia 样式属性。
    /// </summary>
    public static readonly StyledProperty<bool> IsWaveSpiritEnabledProperty =
        WaveSpiritAwareControlProperty.IsWaveSpiritEnabledProperty.AddOwner<AbstractCheckBox>();
    
    /// <summary>
    /// 获取或设置控件是否启用状态切换相关动效。
    /// </summary>
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    /// <summary>
    /// 获取或设置控件是否启用点击时的水波反馈效果。
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
    /// 当复选框的表单值发生变化时触发。
    /// </summary>
    event EventHandler? IFormItemAware.ValueChanged
    {
        add => _formValueChanged += value;
        remove => _formValueChanged -= value;
    }

    /// <summary>
    /// 由表单系统设置复选框的当前值。
    /// </summary>
    /// <param name="value">要设置的表单值，应为 <see cref="bool"/> 或 <see langword="null"/>。</param>
    void IFormItemAware.SetFormValue(object? value) => NotifySetFormValue((bool?)value);

    /// <summary>
    /// 由表单系统读取复选框的当前值。
    /// </summary>
    /// <returns>当前复选框值。</returns>
    object? IFormItemAware.GetFormValue() => NotifyGetFormValue();

    /// <summary>
    /// 由表单系统清空复选框的当前值。
    /// </summary>
    void IFormItemAware.ClearFormValue() => NotifyClearFormValue();

    /// <summary>
    /// 由表单系统通知复选框当前的校验状态。
    /// </summary>
    /// <param name="status">当前表单校验状态。</param>
    void IFormItemAware.NotifyValidateStatus(FormValidateStatus status) => NotifyValidateStatus(status);
    
    private void HandleCheckedChanged()
    {
        _formValueChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// 设置复选框绑定到表单系统的值。
    /// </summary>
    /// <param name="value">要设置的复选框值。</param>
    protected virtual void NotifySetFormValue(bool? value)
    {
        SetCurrentValue(IsCheckedProperty, value);
    }

    /// <summary>
    /// 获取复选框绑定到表单系统的值。
    /// </summary>
    /// <returns>当前复选框值。</returns>
    protected virtual bool? NotifyGetFormValue()
    {
        return IsChecked;
    }

    /// <summary>
    /// 清空复选框绑定到表单系统的值。
    /// </summary>
    protected virtual void NotifyClearFormValue()
    {
        SetCurrentValue(IsCheckedProperty, false);
    }
    
    /// <summary>
    /// 响应表单校验状态变化。
    /// </summary>
    /// <param name="status">当前表单校验状态。</param>
    protected virtual void NotifyValidateStatus(FormValidateStatus status)
    {
    }
    #endregion
}
