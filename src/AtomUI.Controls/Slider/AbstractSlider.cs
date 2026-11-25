using AtomUI.Desktop.Controls;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Automation.Peers;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Media;

namespace AtomUI.Controls;

public abstract class AbstractSlider : RangeBase, IFormItemAware
{
    public static readonly StyledProperty<bool> IsSnapToTickEnabledProperty =
        AvaloniaProperty.Register<AbstractSlider, bool>(nameof(IsSnapToTickEnabled));

    public static readonly StyledProperty<double> TickFrequencyProperty =
        AvaloniaProperty.Register<AbstractSlider, double>(nameof(TickFrequency));

    public static readonly StyledProperty<IReadOnlyList<double>?> RangeValuesProperty =
        AvaloniaProperty.Register<AbstractSlider, IReadOnlyList<double>?>(nameof(RangeValues),
            defaultBindingMode: BindingMode.TwoWay, coerce: CoerceRangeValues, enableDataValidation: true);

    public static readonly StyledProperty<IBrush?> TracksBrushProperty =
        AvaloniaProperty.Register<AbstractSlider, IBrush?>(nameof(TracksBrush));

    public static readonly StyledProperty<bool> IsRangeModeProperty =
        AvaloniaProperty.Register<AbstractSlider, bool>(nameof(IsRangeMode));

    public static readonly StyledProperty<List<SliderMark>?> MarksProperty =
        AvaloniaProperty.Register<AbstractSlider, List<SliderMark>?>(nameof(Marks));

    public static readonly StyledProperty<bool> IsIncludedProperty =
        AvaloniaProperty.Register<AbstractSlider, bool>(nameof(IsIncluded), true);

    public static readonly StyledProperty<bool> IsDotsVisibleProperty =
        AvaloniaProperty.Register<AbstractSlider, bool>(nameof(IsDotsVisible));

    public static readonly StyledProperty<bool> IsSnapToMarksOnlyProperty =
        AvaloniaProperty.Register<AbstractSlider, bool>(nameof(IsSnapToMarksOnly));

    public bool IsSnapToTickEnabled
    {
        get => GetValue(IsSnapToTickEnabledProperty);
        set => SetValue(IsSnapToTickEnabledProperty, value);
    }

    public double TickFrequency
    {
        get => GetValue(TickFrequencyProperty);
        set => SetValue(TickFrequencyProperty, value);
    }

    public IReadOnlyList<double>? RangeValues
    {
        get => GetValue(RangeValuesProperty);
        set => SetValue(RangeValuesProperty, value);
    }

    public IBrush? TracksBrush
    {
        get => GetValue(TracksBrushProperty);
        set => SetValue(TracksBrushProperty, value);
    }

    public bool IsRangeMode
    {
        get => GetValue(IsRangeModeProperty);
        set => SetValue(IsRangeModeProperty, value);
    }

    public List<SliderMark>? Marks
    {
        get => GetValue(MarksProperty);
        set => SetValue(MarksProperty, value);
    }

    public bool IsIncluded
    {
        get => GetValue(IsIncludedProperty);
        set => SetValue(IsIncludedProperty, value);
    }

    public bool IsDotsVisible
    {
        get => GetValue(IsDotsVisibleProperty);
        set => SetValue(IsDotsVisibleProperty, value);
    }

    public bool IsSnapToMarksOnly
    {
        get => GetValue(IsSnapToMarksOnlyProperty);
        set => SetValue(IsSnapToMarksOnlyProperty, value);
    }

    private EventHandler? _formValueChanged;

    static AbstractSlider()
    {
        FocusableProperty.OverrideDefaultValue<AbstractSlider>(true);
        ValueProperty.OverrideMetadata<AbstractSlider>(new StyledPropertyMetadata<double>(enableDataValidation: true));
        AutomationProperties.ControlTypeOverrideProperty.OverrideDefaultValue<AbstractSlider>(AutomationControlType.Slider);

        ValueProperty.Changed.AddClassHandler<AbstractSlider>((slider, args) => slider.NotifyFormValueChanged(args.NewValue));
        RangeValuesProperty.Changed.AddClassHandler<AbstractSlider>((slider, args) => slider.NotifyFormValueChanged(args.NewValue));
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (!IsEnabled || e.Handled)
        {
            return;
        }

        var direction = e.Key switch
        {
            Key.Left or Key.Down => -SmallChange,
            Key.Right or Key.Up  => SmallChange,
            Key.PageDown         => -LargeChange,
            Key.PageUp           => LargeChange,
            Key.Home             => double.NegativeInfinity,
            Key.End              => double.PositiveInfinity,
            _                    => 0
        };

        if (direction == 0)
        {
            base.OnKeyDown(e);
            return;
        }

        MoveForKeyboard(direction);
        e.Handled = true;
    }

    protected virtual void MoveForKeyboard(double direction)
    {
        if (IsRangeMode && RangeValues is { Count: > 0 } values)
        {
            SetCurrentValue(RangeValuesProperty, SliderRangeMath.MoveHandle(values, null, 0,
                GetKeyboardValue(values[0], direction), Minimum, Maximum));
            return;
        }

        SetCurrentValue(ValueProperty, GetKeyboardValue(Value, direction));
    }

    protected double GetKeyboardValue(double value, double direction)
    {
        var target = GetStepTarget(value, direction);
        var next = QuantizeValue(target);
        if (next == value && double.IsFinite(direction))
        {
            if (IsSnapToMarksOnly && TryFindMark(value, 0, out _))
            {
                return TryFindMark(value, Math.Sign(direction), out var mark) ? mark : value;
            }
            if (IsSnapToTickEnabled && TickFrequency > 0 && double.IsFinite(TickFrequency))
            {
                return Math.Clamp(SliderRangeMath.AdjacentTick(value, Minimum, TickFrequency, direction), Minimum, Maximum);
            }
        }
        return next;
    }

    private double GetStepTarget(double value, double direction)
    {
        if (double.IsNegativeInfinity(direction))
        {
            return Minimum;
        }
        if (double.IsPositiveInfinity(direction))
        {
            return Maximum;
        }
        return value + direction;
    }

    protected double QuantizeValue(double value)
    {
        value = Math.Clamp(value, Minimum, Maximum);
        if (IsSnapToMarksOnly && TryFindMark(value, 0, out var mark))
        {
            return mark;
        }
        if (!IsSnapToTickEnabled)
        {
            return value;
        }
        return SliderRangeMath.SnapToTick(value, Minimum, Maximum, TickFrequency);
    }

    protected double GetValueRatio(double value) => SliderRangeMath.ValueToRatio(value, Minimum, Maximum);

    protected double GetValueFromRatio(double ratio) => SliderRangeMath.RatioToValue(ratio, Minimum, Maximum);

    private bool TryFindMark(double value, int direction, out double nearest)
    {
        nearest = default;
        var distance = double.PositiveInfinity;
        if (Marks is null)
        {
            return false;
        }

        foreach (var mark in Marks)
        {
            var candidate = mark.Value;
            if (!double.IsFinite(candidate) || candidate < Minimum || candidate > Maximum ||
                (direction < 0 && candidate >= value) || (direction > 0 && candidate <= value))
            {
                continue;
            }

            var delta = Math.Abs(candidate - value);
            if (delta < distance || (delta == distance && candidate > nearest))
            {
                distance = delta;
                nearest = candidate;
            }
        }
        return distance < double.PositiveInfinity;
    }

    protected override void UpdateDataValidation(
        AvaloniaProperty property,
        BindingValueType state,
        Exception? error)
    {
        if (property == ValueProperty || property == RangeValuesProperty)
        {
            DataValidationErrors.SetError(this, error);
        }
    }

    #region 实现 FormItem 接口

    event EventHandler? IFormItemAware.ValueChanged
    {
        add => _formValueChanged += value;
        remove => _formValueChanged -= value;
    }

    void IFormItemAware.SetFormValue(object? value) => NotifySetFormValue(value);

    object? IFormItemAware.GetFormValue() => NotifyGetFormValue();
    void IFormItemAware.ClearFormValue() => NotifyClearFormValue();
    void IFormItemAware.NotifyValidateStatus(FormValidateStatus status) => NotifyValidateStatus(status);

    protected virtual void NotifyFormValueChanged(object? value)
    {
        _formValueChanged?.Invoke(this, EventArgs.Empty);
    }

    protected virtual void NotifySetFormValue(object? value)
    {
        if (IsRangeMode)
        {
            RangeValues = value as IReadOnlyList<double>;
        }
        else
        {
            Value = value != null ? (double)value : 0.0;
        }
    }

    protected virtual object? NotifyGetFormValue()
    {
        return IsRangeMode ? RangeValues : Value;
    }

    protected virtual void NotifyClearFormValue()
    {
        if (IsRangeMode)
        {
            RangeValues = null;
        }
        else
        {
            Value = 0.0;
        }
    }

    protected virtual void NotifyValidateStatus(FormValidateStatus status)
    {
    }

    #endregion

    private static IReadOnlyList<double>? CoerceRangeValues(AvaloniaObject sender, IReadOnlyList<double>? values)
    {
        var normalized = SliderRangeMath.NormalizeRangeValues(
            values,
            sender.GetValue(MinimumProperty),
            sender.GetValue(MaximumProperty));
        return normalized.Count == 0 ? sender.GetValue(RangeValuesProperty) : normalized;
    }

}
