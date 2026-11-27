using System.Diagnostics;
using AtomUI;
using AtomUI.Controls;
using AtomUI.Desktop.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace AtomUIGallery.ShowCases.Form;

public record DonationInfo
{
    public string Value { get; init; }
    public string Unit {  get; init; }

    public DonationInfo(string value, string unit)
    {
        Value = value;
        Unit  = unit;
    }
}

public class Donation: TemplatedControl,
                       IMotionAwareControl,
                       ICustomizableSizeTypeAware,
                       IFormItemAware,
                       IInputControlStatusAware,
                       IInputControlStyleVariantAware
{
    #region 公共属性定义
    /// <summary>
    /// Identifies the <see cref="SizeType" /> property.
    /// </summary>
    public static readonly StyledProperty<CustomizableSizeType> SizeTypeProperty =
        CustomizableSizeTypeControlProperty.SizeTypeProperty.AddOwner<Donation>();
    
    /// <summary>
    /// Identifies the <see cref="IsMotionEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<Donation>();
    
    /// <summary>
    /// Identifies the <see cref="Value" /> property.
    /// </summary>
    public static readonly StyledProperty<DonationInfo?> ValueProperty =
        AvaloniaProperty.Register<Donation, DonationInfo?>(nameof(Value));
    
    /// <summary>
    /// Identifies the <see cref="StyleVariant" /> property.
    /// </summary>
    public static readonly StyledProperty<InputControlStyleVariant> StyleVariantProperty =
        InputControlStyleVariantProperty.StyleVariantProperty.AddOwner<Donation>();

    /// <summary>
    /// Identifies the <see cref="Status" /> property.
    /// </summary>
    public static readonly StyledProperty<InputControlStatus> StatusProperty =
        InputControlStatusProperty.StatusProperty.AddOwner<Donation>();
    
    public CustomizableSizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }
    
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }
    
    public DonationInfo? Value
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

    private AtomUILineEdit? _valueInput;
    private AtomUISelect? _unitInput;
    
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        if (_valueInput != null)
        {
            _valueInput.TextChanged -= HandleInputValueChanged;
        }
        if (_unitInput != null)
        {
            _unitInput.SelectionChanged -= HandleUnitSelectionChanged;
        }
        _valueInput = e.NameScope.Find<AtomUILineEdit>("ValueInput");
        _unitInput   = e.NameScope.Find<AtomUISelect>("UnitInput");

        if (_valueInput != null)
        {
            _valueInput.TextChanged += HandleInputValueChanged;
        }

        if (_unitInput != null)
        {
            _unitInput.SelectionChanged += HandleUnitSelectionChanged;
        }
    }

    private void HandleInputValueChanged(object? sender, TextChangedEventArgs e)
    {
        UpdateValueFromInputs();
    }

    private void HandleUnitSelectionChanged(object? sender, SelectSelectionChangedEventArgs e)
    {
        UpdateValueFromInputs();
    }

    private void UpdateValueFromInputs()
    {
        Debug.Assert(_valueInput != null);
        Debug.Assert(_unitInput != null);
        var value = _valueInput?.Text;
        DonationInfo? newValue = null;
        if (!string.IsNullOrWhiteSpace(value))
        {
            var unit  = _unitInput.SelectedOption?.Content?.ToString() ?? "CNY";
            newValue = new DonationInfo(value, unit);
        }

        if (Equals(Value, newValue))
        {
            return;
        }

        Value = newValue;
        HandleValueChanged();
    }

    #region 实现 FormItem 接口
    private EventHandler? _formValueChanged;
    event EventHandler? IFormItemAware.ValueChanged
    {
        add => _formValueChanged += value;
        remove => _formValueChanged -= value;
    }

    void IFormItemAware.SetFormValue(object? value) => NotifySetFormValue(value as DonationInfo);

    object? IFormItemAware.GetFormValue() => NotifyGetFormValue();
    void IFormItemAware.ClearFormValue() => NotifyClearFormValue();
    void IFormItemAware.NotifyValidateStatus(FormValidateStatus status) => NotifyValidateStatus(status);
    
    private void HandleValueChanged()
    {
        _formValueChanged?.Invoke(this, EventArgs.Empty);
    }

    protected virtual void NotifySetFormValue(DonationInfo? value)
    {
        SetCurrentValue(ValueProperty, value);
    }

    protected virtual DonationInfo? NotifyGetFormValue()
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
