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

/// <summary>
/// Base class of slider controls. Besides the single value of <see cref="RangeBase" /> it manages a range of
/// handle values, marks and tick snapping, maps keyboard input to value steps, and reports its value to a Form
/// item through <see cref="IFormItemAware" />.
/// </summary>
public abstract class AbstractSlider : RangeBase, IFormItemAware
{
    /// <summary>
    /// Identifies the <see cref="IsSnapToTickEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsSnapToTickEnabledProperty =
        AvaloniaProperty.Register<AbstractSlider, bool>(nameof(IsSnapToTickEnabled));

    /// <summary>
    /// Identifies the <see cref="TickFrequency" /> property.
    /// </summary>
    public static readonly StyledProperty<double> TickFrequencyProperty =
        AvaloniaProperty.Register<AbstractSlider, double>(nameof(TickFrequency));

    /// <summary>
    /// Identifies the <see cref="RangeValues" /> property.
    /// </summary>
    public static readonly StyledProperty<IReadOnlyList<double>?> RangeValuesProperty =
        AvaloniaProperty.Register<AbstractSlider, IReadOnlyList<double>?>(nameof(RangeValues),
            defaultBindingMode: BindingMode.TwoWay, coerce: CoerceRangeValues, enableDataValidation: true);

    /// <summary>
    /// Identifies the <see cref="TracksBrush" /> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> TracksBrushProperty =
        AvaloniaProperty.Register<AbstractSlider, IBrush?>(nameof(TracksBrush));

    /// <summary>
    /// Identifies the <see cref="IsRangeMode" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsRangeModeProperty =
        AvaloniaProperty.Register<AbstractSlider, bool>(nameof(IsRangeMode));

    /// <summary>
    /// Identifies the <see cref="Marks" /> property.
    /// </summary>
    public static readonly StyledProperty<List<SliderMark>?> MarksProperty =
        AvaloniaProperty.Register<AbstractSlider, List<SliderMark>?>(nameof(Marks));

    /// <summary>
    /// Identifies the <see cref="IsIncluded" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsIncludedProperty =
        AvaloniaProperty.Register<AbstractSlider, bool>(nameof(IsIncluded), true);

    /// <summary>
    /// Identifies the <see cref="IsDotsVisible" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsDotsVisibleProperty =
        AvaloniaProperty.Register<AbstractSlider, bool>(nameof(IsDotsVisible));

    /// <summary>
    /// Identifies the <see cref="IsSnapToMarksOnly" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsSnapToMarksOnlyProperty =
        AvaloniaProperty.Register<AbstractSlider, bool>(nameof(IsSnapToMarksOnly));

    /// <summary>
    /// Rounds every value that the user picks with the keyboard or by moving a handle to the nearest tick of the
    /// grid that <see cref="TickFrequency" /> defines. A keyboard step that would stay on the same tick moves to
    /// the adjacent tick instead. When both are set, <see cref="IsSnapToMarksOnly" /> takes precedence. Values
    /// that are set directly are not rounded.
    /// </summary>
    public bool IsSnapToTickEnabled
    {
        get => GetValue(IsSnapToTickEnabledProperty);
        set => SetValue(IsSnapToTickEnabledProperty, value);
    }

    /// <summary>
    /// Distance between the ticks on the rail, in the units of <see cref="RangeBase.Value" />. Ticks are drawn at
    /// <see cref="RangeBase.Minimum" />, at every multiple of the distance and at <see cref="RangeBase.Maximum" />, but only while
    /// <see cref="IsDotsVisible" /> is set. The default, 0, and any value that is not finite draw no ticks; with
    /// <see cref="IsSnapToTickEnabled" /> set they make every value that the user picks snap to the nearer end of
    /// the range.
    /// </summary>
    public double TickFrequency
    {
        get => GetValue(TickFrequencyProperty);
        set => SetValue(TickFrequencyProperty, value);
    }

    /// <summary>
    /// Values of the handles while <see cref="IsRangeMode" /> is set, in ascending order. Every value is clamped to the
    /// range from <see cref="RangeBase.Minimum" /> to <see cref="RangeBase.Maximum" />, and values that are out of
    /// order are sorted when the list is assigned. <see langword="null" />, the default, and a list with fewer than two
    /// values place the handles at <see cref="RangeBase.Minimum" />. A list that contains a value that is not finite is
    /// rejected and the previous list is kept. Binds two-way by default.
    /// </summary>
    public IReadOnlyList<double>? RangeValues
    {
        get => GetValue(RangeValuesProperty);
        set => SetValue(RangeValuesProperty, value);
    }

    /// <summary>
    /// Brush of the strip behind the filled range segments. The strip is shown only while
    /// <see cref="IsIncluded" /> is set and spans the selected range: from the first handle to the last one
    /// while <see cref="IsRangeMode" /> is set, and from <see cref="RangeBase.Minimum" /> to
    /// <see cref="RangeBase.Value" /> otherwise. <see langword="null" />, the default, leaves it unpainted.
    /// </summary>
    public IBrush? TracksBrush
    {
        get => GetValue(TracksBrushProperty);
        set => SetValue(TracksBrushProperty, value);
    }

    /// <summary>
    /// Switches the slider to a range: <see cref="RangeValues" /> holds one value per handle and replaces
    /// <see cref="RangeBase.Value" /> as the value that the Form item reads and writes.
    /// </summary>
    public bool IsRangeMode
    {
        get => GetValue(IsRangeModeProperty);
        set => SetValue(IsRangeModeProperty, value);
    }

    /// <summary>
    /// Marks that are drawn on the rail at the value of each entry, with the label of the entry beside it. A mark
    /// that lies inside the selected range is drawn as included. <see langword="null" />, the default, and an
    /// empty list draw none.
    /// </summary>
    public List<SliderMark>? Marks
    {
        get => GetValue(MarksProperty);
        set => SetValue(MarksProperty, value);
    }

    /// <summary>
    /// Whether the strip that <see cref="TracksBrush" /> paints and the filled range segments of the track
    /// are shown, and whether marks inside the selected range are drawn as included.
    /// <see langword="true" /> by default.
    /// </summary>
    public bool IsIncluded
    {
        get => GetValue(IsIncludedProperty);
        set => SetValue(IsIncludedProperty, value);
    }

    /// <summary>
    /// Draws a dot on the rail at every tick. It needs a positive, finite <see cref="TickFrequency" />, and
    /// ticks that fall on the same pixel are merged into one dot.
    /// </summary>
    public bool IsDotsVisible
    {
        get => GetValue(IsDotsVisibleProperty);
        set => SetValue(IsDotsVisibleProperty, value);
    }

    /// <summary>
    /// Rounds every value that the user picks with the keyboard or by moving a handle to the value of the nearest mark
    /// in <see cref="Marks" />, taking precedence over <see cref="IsSnapToTickEnabled" />. A keyboard step that starts
    /// on a mark moves to the adjacent mark, or leaves the value unchanged when no mark lies in that direction. Only
    /// marks with a finite value from <see cref="RangeBase.Minimum" /> to <see cref="RangeBase.Maximum" /> take part.
    /// </summary>
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

    /// <summary>
    /// Moves the slider for the arrow keys by <see cref="RangeBase.SmallChange" />, for PageUp and PageDown by <see
    /// cref="RangeBase.LargeChange" />, and for Home and End to <see cref="RangeBase.Minimum" /> and <see
    /// cref="RangeBase.Maximum" />, and marks the key as handled. Other keys go to the base member. Nothing happens
    /// while the control is disabled or the key is already handled.
    /// </summary>
    /// <param name="e">
    /// The key event.
    /// </param>
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

    /// <summary>
    /// Applies a keyboard step to the slider. It is called by <see cref="OnKeyDown" /> for the arrow, paging, Home and
    /// End keys, and writes the new value with <c>SetCurrentValue</c>, so a binding on the property stays in place.
    /// Outside range mode it sets <see cref="RangeBase.Value" />. In range mode it moves the first handle of <see
    /// cref="RangeValues" />, unless that list is <see langword="null" /> or empty, in which case it sets <see
    /// cref="RangeBase.Value" />. An override that moves another handle can replace it without calling the base member.
    /// </summary>
    /// <param name="direction">
    /// The step to add, in the units of <see cref="RangeBase.Value" />. <see cref="double.NegativeInfinity" /> steps to
    /// <see cref="RangeBase.Minimum" /> and <see cref="double.PositiveInfinity" /> to <see cref="RangeBase.Maximum" />.
    /// </param>
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

    /// <summary>
    /// Computes the value that a keyboard step reaches from <paramref name="value" />, after rounding by
    /// <see cref="QuantizeValue" />. When the rounded result equals <paramref name="value" />, it returns the
    /// next mark or tick in the direction of the step instead, so that a step smaller than the mark or tick
    /// spacing still moves the slider.
    /// </summary>
    /// <param name="value">
    /// The value to step from.
    /// </param>
    /// <param name="direction">
    /// The step to add, in the units of <see cref="RangeBase.Value" />. <see cref="double.NegativeInfinity" /> steps to
    /// <see cref="RangeBase.Minimum" /> and <see cref="double.PositiveInfinity" /> to <see cref="RangeBase.Maximum" />.
    /// </param>
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

    /// <summary>
    /// Clamps a value to the range from <see cref="RangeBase.Minimum" /> to <see cref="RangeBase.Maximum" /> and rounds
    /// it the way the slider rounds user input: to the nearest mark while <see cref="IsSnapToMarksOnly" /> is set,
    /// otherwise to the nearest tick while <see cref="IsSnapToTickEnabled" /> is set.
    /// </summary>
    /// <param name="value">
    /// The value to clamp and round.
    /// </param>
    /// <returns>
    /// The clamped value, rounded to a mark or a tick when snapping is enabled.
    /// </returns>
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

    /// <summary>
    /// Converts a value to its position on the rail: 0 at <see cref="RangeBase.Minimum" />, 1 at <see
    /// cref="RangeBase.Maximum" /> and clamped to that range in between. Returns 0 when the range is empty or not
    /// finite.
    /// </summary>
    /// <param name="value">
    /// The value to locate on the rail.
    /// </param>
    protected double GetValueRatio(double value) => SliderRangeMath.ValueToRatio(value, Minimum, Maximum);

    /// <summary>
    /// Converts a position on the rail to the value at that position: <see cref="RangeBase.Minimum" /> at 0 and
    /// <see cref="RangeBase.Maximum" /> at 1. Positions outside 0 to 1 are clamped.
    /// </summary>
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

    /// <summary>
    /// Stores the binding error of <see cref="RangeBase.Value" /> or <see cref="RangeValues" /> in
    /// <c>DataValidationErrors</c>. Errors of other properties are ignored, and the base member is not called.
    /// </summary>
    /// <param name="property">
    /// The property whose binding reported the state.
    /// </param>
    /// <param name="state">
    /// The state that the binding reported.
    /// </param>
    /// <param name="error">
    /// The error that the binding produced, or <see langword="null" /> when there is none.
    /// </param>
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

    /// <summary>
    /// Raises <c>IFormItemAware.ValueChanged</c> to tell the Form item that the value of the slider changed. It
    /// runs after <see cref="RangeBase.Value" /> or <see cref="RangeValues" /> changed; an override must call the base
    /// member to keep the Form item informed.
    /// </summary>
    /// <param name="value">
    /// The value that was set. The base member does not use it.
    /// </param>
    protected virtual void NotifyFormValueChanged(object? value)
    {
        _formValueChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Writes a value that the Form item supplies into the slider. In range mode <paramref name="value" /> must
    /// be an <c>IReadOnlyList&lt;double&gt;</c>, which is assigned to <see cref="RangeValues" />; any other
    /// object assigns <see langword="null" /> to it. Outside range mode a <see cref="double" /> is assigned to
    /// <see cref="RangeBase.Value" />, <see langword="null" /> assigns 0, and any other object throws
    /// <c>InvalidCastException</c>.
    /// </summary>
    /// <param name="value">
    /// The value of the slider as the Form item stores it.
    /// </param>
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

    /// <summary>
    /// Reads the value of the slider for the Form item: <see cref="RangeValues" /> in range mode, otherwise
    /// <see cref="RangeBase.Value" />.
    /// </summary>
    protected virtual object? NotifyGetFormValue()
    {
        return IsRangeMode ? RangeValues : Value;
    }

    /// <summary>
    /// Resets the slider when the Form item clears its field: it assigns <see langword="null" /> to <see
    /// cref="RangeValues" />, which places the handles at <see cref="RangeBase.Minimum" />, in range mode, and sets
    /// <see cref="RangeBase.Value" /> to 0 otherwise.
    /// </summary>
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

    /// <summary>
    /// Receives the validation status that a Form item reports for the slider. The base member does nothing, so
    /// an override that reacts to the status does not need to call it.
    /// </summary>
    /// <param name="status">
    /// The current validation status of the Form item.
    /// </param>
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
