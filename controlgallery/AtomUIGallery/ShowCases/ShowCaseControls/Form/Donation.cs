using System.Diagnostics;
using AtomUI;
using AtomUI.Controls;
using AtomUI.Desktop.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using NumericUpDown = AtomUI.Desktop.Controls.NumericUpDown;

namespace AtomUIGallery.ShowCases.ShowCaseControls;

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
                       ISizeTypeAware,
                       IFormItemAware,
                       IInputControlStatusAware,
                       IInputControlStyleVariantAware
{
    #region 公共属性定义
    public static readonly StyledProperty<SizeType> SizeTypeProperty =
        SizeTypeControlProperty.SizeTypeProperty.AddOwner<Donation>();
    
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<Donation>();
    
    public static readonly StyledProperty<DonationInfo?> ValueProperty =
        AvaloniaProperty.Register<Donation, DonationInfo?>(nameof(Value));
    
    public static readonly StyledProperty<InputControlStyleVariant> StyleVariantProperty =
        InputControlStyleVariantProperty.StyleVariantProperty.AddOwner<Donation>();

    public static readonly StyledProperty<InputControlStatus> StatusProperty =
        InputControlStatusProperty.StatusProperty.AddOwner<Donation>();
    
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

    private LineEdit? _valueInput;
    private Select? _unitInput;
    
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
        _valueInput = e.NameScope.Find<LineEdit>("ValueInput");
        _unitInput   = e.NameScope.Find<Select>("UnitInput");

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
        Debug.Assert(_valueInput != null);
        Debug.Assert(_unitInput != null);
        var value = _valueInput?.Text;
        if (!string.IsNullOrWhiteSpace(value))
        {
            var unit  = _unitInput.SelectedOption?.Value?.ToString() ?? "CNY";
            Value = new DonationInfo(value, unit);
        }
        else 
        {
            Value = null;
        }
        HandleValueChanged();
    }

    private void HandleUnitSelectionChanged(object? sender, SelectSelectionChangedEventArgs e)
    {
        Debug.Assert(_valueInput != null);
        Debug.Assert(_unitInput != null);
        var value = _valueInput?.Text;
        if (!string.IsNullOrWhiteSpace(value))
        {
            var unit  = _unitInput.SelectedOption?.Value?.ToString() ?? "CNY";
            Value = new DonationInfo(value, unit);
        }
        else 
        {
            Value = null;
        }
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