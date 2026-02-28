using AtomUI.Controls;
using AtomUI.Theme;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;

namespace AtomUI.Desktop.Controls;

using AvaloniaCheckBox = Avalonia.Controls.CheckBox;

public class CheckBox : AvaloniaCheckBox, 
                        IWaveSpiritAwareControl, 
                        IControlSharedTokenResourcesHost,
                        IFormItemAware
{
    #region 公共属性定义

    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<CheckBox>();

    public static readonly StyledProperty<bool> IsWaveSpiritEnabledProperty =
        WaveSpiritAwareControlProperty.IsWaveSpiritEnabledProperty.AddOwner<CheckBox>();
    
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    public bool IsWaveSpiritEnabled
    {
        get => GetValue(IsWaveSpiritEnabledProperty);
        set => SetValue(IsWaveSpiritEnabledProperty, value);
    }

    #endregion
    
    #region 公共事件定义

    private EventHandler? _formValueChanged;
    event EventHandler? IFormItemAware.ValueChanged
    {
        add => _formValueChanged += value;
        remove => _formValueChanged -= value;
    }

    #endregion
    
    #region 内部属性定义
    
    Control IControlSharedTokenResourcesHost.HostControl => this;
    string IControlSharedTokenResourcesHost.TokenId => CheckBoxToken.ID;

    #endregion

    static CheckBox()
    {
        IsCheckedChangedEvent.AddClassHandler<CheckBox>((checkbox, args) => checkbox.HandleCheckedChanged());
    }

    public CheckBox()
    {
        this.RegisterResources();
    }
    
    #region 实现 FormItem 接口

    void IFormItemAware.SetFormValue(object? value) => NotifySetFormValue((bool?)value);

    object? IFormItemAware.GetFormValue() => NotifyGetFormValue();
    void IFormItemAware.ClearFormValue() => NotifyClearFormValue();
    void IFormItemAware.NotifyValidateStatus(FormValidateStatus status) => NotifyValidateStatus(status);
    
    private void HandleCheckedChanged()
    {
        _formValueChanged?.Invoke(this, EventArgs.Empty);
    }

    protected virtual void NotifySetFormValue(bool? value)
    {
        SetCurrentValue(IsCheckedProperty, value);
    }

    protected virtual bool? NotifyGetFormValue()
    {
        return IsChecked;
    }

    protected virtual void NotifyClearFormValue()
    {
        SetCurrentValue(IsCheckedProperty, null);
    }
    
    protected virtual void NotifyValidateStatus(FormValidateStatus status)
    {
    }
    #endregion
}