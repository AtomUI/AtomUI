using System.Diagnostics;
using AtomUI;
using AtomUI.Controls;
using AtomUI.Desktop.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using NumericUpDown = AtomUI.Desktop.Controls.NumericUpDown;

namespace AtomUIGallery.ShowCases.ShowCaseControls;

public record PriceInfo
{
    public decimal Value { get; init; }
    public string Unit {  get; init; }

    public PriceInfo(decimal value, string unit)
    {
        Value = value;
        Unit = unit;
    }
}

public class PriceInput : TemplatedControl,
                          IMotionAwareControl,
                          ISizeTypeAware,
                          IFormItemAware,
                          IInputControlStatusAware,
                          IInputControlStyleVariantAware
{
    #region 公共属性定义
    public static readonly StyledProperty<SizeType> SizeTypeProperty =
        SizeTypeControlProperty.SizeTypeProperty.AddOwner<PriceInput>();
    
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<PriceInput>();
    
    public static readonly StyledProperty<PriceInfo?> ValueProperty =
        AvaloniaProperty.Register<PriceInput, PriceInfo?>(nameof(Value));
    
    public static readonly StyledProperty<InputControlStyleVariant> StyleVariantProperty =
        InputControlStyleVariantProperty.StyleVariantProperty.AddOwner<PriceInput>();

    public static readonly StyledProperty<InputControlStatus> StatusProperty =
        InputControlStatusProperty.StatusProperty.AddOwner<PriceInput>();
    
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
    
    public PriceInfo? Value
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

    private NumericUpDown? _numberInput;
    private Select? _unitInput;
    
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        if (_numberInput != null)
        {
            _numberInput.ValueChanged -= HandleNumberInputValueChanged;
        }
        if (_unitInput != null)
        {
            _unitInput.SelectionChanged -= HandleUnitSelectionChanged;
        }
        _numberInput = e.NameScope.Find<NumericUpDown>("NumberInput");
        _unitInput   = e.NameScope.Find<Select>("UnitInput");

        if (_numberInput != null)
        {
            _numberInput.ValueChanged += HandleNumberInputValueChanged;
        }

        if (_unitInput != null)
        {
            _unitInput.SelectionChanged += HandleUnitSelectionChanged;
        }
    }

    private void HandleNumberInputValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
    {
        Debug.Assert(_numberInput != null);
        Debug.Assert(_unitInput != null);
        var value = _numberInput.Value ?? decimal.Zero;
        var unit = _unitInput.SelectedOption?.Content?.ToString() ?? "RMB";
        Value = new PriceInfo(value, unit);
        HandleValueChanged();
    }

    private void HandleUnitSelectionChanged(object? sender, SelectSelectionChangedEventArgs e)
    {
        Debug.Assert(_numberInput != null);
        Debug.Assert(_unitInput != null);
        var value = _numberInput.Value ?? decimal.Zero;
        var unit  = _unitInput.SelectedOption?.Content?.ToString() ?? "RMB";
        Value = new PriceInfo(value, unit);
        HandleValueChanged();
    }

    #region 实现 FormItem 接口
    private EventHandler? _formValueChanged;
    event EventHandler? IFormItemAware.ValueChanged
    {
        add => _formValueChanged += value;
        remove => _formValueChanged -= value;
    }

    void IFormItemAware.SetFormValue(object? value) => NotifySetFormValue(value as PriceInfo);

    object? IFormItemAware.GetFormValue() => NotifyGetFormValue();
    void IFormItemAware.ClearFormValue() => NotifyClearFormValue();
    void IFormItemAware.NotifyValidateStatus(FormValidateStatus status) => NotifyValidateStatus(status);
    
    private void HandleValueChanged()
    {
        _formValueChanged?.Invoke(this, EventArgs.Empty);
    }

    protected virtual void NotifySetFormValue(PriceInfo? value)
    {
        SetCurrentValue(ValueProperty, value);
    }

    protected virtual PriceInfo? NotifyGetFormValue()
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