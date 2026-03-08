using AtomUI;
using AtomUI.Controls;
using AtomUI.Desktop.Controls;
using Avalonia;
using Avalonia.Controls.Primitives;

namespace AtomUIGallery.ShowCases.ShowCaseControls;

public class Captcha : TemplatedControl,
                       IMotionAwareControl,
                       ISizeTypeAware,
                       IFormItemAware,
                       IInputControlStatusAware,
                       IInputControlStyleVariantAware
{
    #region 公共属性定义
    public static readonly StyledProperty<SizeType> SizeTypeProperty =
        SizeTypeControlProperty.SizeTypeProperty.AddOwner<Captcha>();
    
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<Captcha>();
    
    public static readonly StyledProperty<string?> ValueProperty =
        AvaloniaProperty.Register<Captcha, string?>(nameof(Value));
    
    public static readonly StyledProperty<InputControlStyleVariant> StyleVariantProperty =
        InputControlStyleVariantProperty.StyleVariantProperty.AddOwner<Captcha>();

    public static readonly StyledProperty<InputControlStatus> StatusProperty =
        InputControlStatusProperty.StatusProperty.AddOwner<Captcha>();
    
    public SizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }
    
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }
    
    public string? Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }
    
    public InputControlStyleVariant StyleVariant
    {
        get => GetValue(StyleVariantProperty);
        set => SetValue(StyleVariantProperty, value);
    }

    public InputControlStatus Status
    {
        get => GetValue(StatusProperty);
        set => SetValue(StatusProperty, value);
    }
    #endregion

    static Captcha()
    {
        ValueProperty.Changed.AddClassHandler<Captcha>((captcha, args) => captcha.HandleValueChanged());
    }

    #region 实现 FormItem 接口
    private EventHandler? _formValueChanged;
    event EventHandler? IFormItemAware.ValueChanged
    {
        add => _formValueChanged += value;
        remove => _formValueChanged -= value;
    }

    void IFormItemAware.SetFormValue(object? value) => NotifySetFormValue(value as string);

    object? IFormItemAware.GetFormValue() => NotifyGetFormValue();
    void IFormItemAware.ClearFormValue() => NotifyClearFormValue();
    void IFormItemAware.NotifyValidateStatus(FormValidateStatus status) => NotifyValidateStatus(status);
    
    private void HandleValueChanged()
    {
        _formValueChanged?.Invoke(this, EventArgs.Empty);
    }

    protected virtual void NotifySetFormValue(string? value)
    {
        SetCurrentValue(ValueProperty, value);
    }

    protected virtual string? NotifyGetFormValue()
    {
        return Value;
    }

    protected virtual void NotifyClearFormValue()
    {
        SetCurrentValue(ValueProperty, null);
    }

    protected virtual void NotifyValidateStatus(FormValidateStatus status)
    {
        if (status == FormValidateStatus.Error)
        {
            SetCurrentValue(StatusProperty, InputControlStatus.Error);
        }
        else if (status == FormValidateStatus.Warning)
        {
            SetCurrentValue(StatusProperty, InputControlStatus.Warning);
        }
        else
        {
            SetCurrentValue(StatusProperty, InputControlStatus.Default);
        }
    }
    #endregion
}