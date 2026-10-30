using AtomUI.Controls;
using AtomUI.Media;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Automation.Peers;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Mixins;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

public record SliderMark(string Label, double Value)
{
    public IBrush? LabelBrush { get; set; }
    public FontStyle LabelFontStyle { get; set; } = FontStyle.Normal;
    public FontWeight LabelFontWeight { get; set; } = FontWeight.Normal;
    internal Size LabelSize { get; set; }
    internal FormattedText? FormattedText { get; set; }
}

[PseudoClasses(StdPseudoClass.Vertical, StdPseudoClass.Horizontal, StdPseudoClass.Pressed)]
public partial class Slider : RangeBase,
                      IMotionAwareControl,
                      IFormItemAware
{
    #region 公共属性定义

    public static readonly StyledProperty<Orientation> OrientationProperty =
        StackPanel.OrientationProperty.AddOwner<Slider>();

    public static readonly StyledProperty<bool> IsDirectionReversedProperty =
        SliderTrack.IsDirectionReversedProperty.AddOwner<Slider>();

    public static readonly StyledProperty<bool> IsSnapToTickEnabledProperty =
        AvaloniaProperty.Register<Slider, bool>(nameof(IsSnapToTickEnabled));

    public static readonly StyledProperty<double> TickFrequencyProperty =
        AvaloniaProperty.Register<Slider, double>(nameof(TickFrequency));

    public static readonly StyledProperty<IReadOnlyList<double>?> RangeValuesProperty =
        SliderTrack.RangeValuesProperty.AddOwner<Slider>(
            new StyledPropertyMetadata<IReadOnlyList<double>?>(defaultBindingMode: BindingMode.TwoWay,
                coerce: CoerceRangeValues,
                enableDataValidation: true));

    public static readonly StyledProperty<IReadOnlyList<bool>?> DisabledHandlesProperty =
        SliderTrack.DisabledHandlesProperty.AddOwner<Slider>();

    public static readonly StyledProperty<bool> IsDraggableTrackProperty =
        SliderTrack.IsDraggableTrackProperty.AddOwner<Slider>();

    public static readonly StyledProperty<IBrush?> TrackBarBrushProperty =
        SliderTrack.TrackBarBrushProperty.AddOwner<Slider>();

    public static readonly StyledProperty<IBrush?> TracksBrushProperty =
        SliderTrack.TracksBrushProperty.AddOwner<Slider>();

    public static readonly StyledProperty<bool> IsRangeModeProperty =
        SliderTrack.IsRangeModeProperty.AddOwner<Slider>();

    public static readonly StyledProperty<List<SliderMark>?> MarksProperty =
        SliderTrack.MarksProperty.AddOwner<Slider>();

    public static readonly StyledProperty<string> ValueFormatTemplateProperty =
        AvaloniaProperty.Register<Slider, string>(nameof(ValueFormatTemplate), "{0:0}");

    public static readonly StyledProperty<bool> IsIncludedProperty =
        SliderTrack.IsIncludedProperty.AddOwner<Slider>();

    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<Slider>();

    public static readonly StyledProperty<bool> IsWaveSpiritEnabledProperty =
        WaveSpiritAwareControlProperty.IsWaveSpiritEnabledProperty.AddOwner<Slider>();

    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    public bool IsDirectionReversed
    {
        get => GetValue(IsDirectionReversedProperty);
        set => SetValue(IsDirectionReversedProperty, value);
    }

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

    public IReadOnlyList<bool>? DisabledHandles
    {
        get => GetValue(DisabledHandlesProperty);
        set => SetValue(DisabledHandlesProperty, value);
    }

    public bool IsDraggableTrack
    {
        get => GetValue(IsDraggableTrackProperty);
        set => SetValue(IsDraggableTrackProperty, value);
    }

    public IBrush? TrackBarBrush
    {
        get => GetValue(TrackBarBrushProperty);
        set => SetValue(TrackBarBrushProperty, value);
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

    public string ValueFormatTemplate
    {
        get => GetValue(ValueFormatTemplateProperty);
        set => SetValue(ValueFormatTemplateProperty, value);
    }

    public bool IsIncluded
    {
        get => GetValue(IsIncludedProperty);
        set => SetValue(IsIncludedProperty, value);
    }

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

    protected bool IsDragging { get; private set; }

    protected SliderTrack? SliderTrack { get; private set; }

    private SliderThumb? _graspedThumb;
    private double _thumbDragValueOffset;
    private bool _isRangeTrackDragging;
    private double _rangeTrackDragStartValue;
    private IReadOnlyList<double>? _rangeTrackDragStartValues;
    private IDisposable? _pointerMovedDispose;
    private IDisposable? _pointerPressDispose;
    private IDisposable? _pointerReleaseDispose;
    private double _tipHostWidth;
    private EventHandler? _formValueChanged;

    private const double Tolerance = 0.0001;

    static Slider()
    {
        PressedMixin.Attach<Slider>();
        FocusableProperty.OverrideDefaultValue<Slider>(true);
        OrientationProperty.OverrideDefaultValue(typeof(Slider), Orientation.Horizontal);
        SliderThumb.DragStartedEvent.AddClassHandler<Slider>((x, e) => x.OnThumbDragStarted(e),
            RoutingStrategies.Bubble);
        SliderThumb.DragCompletedEvent.AddClassHandler<Slider>((x, e) => x.OnThumbDragCompleted(e),
            RoutingStrategies.Bubble);

        ValueProperty.OverrideMetadata<Slider>(new StyledPropertyMetadata<double>(enableDataValidation: true));
        AutomationProperties.ControlTypeOverrideProperty.OverrideDefaultValue<Slider>(AutomationControlType.Slider);

        ValueProperty.Changed.AddClassHandler<Slider>((slider, args) => slider.NotifyFormValueChanged(args.NewValue));
        RangeValuesProperty.Changed.AddClassHandler<Slider>((slider, args) => slider.NotifyFormValueChanged(args.NewValue));
    }

    public Slider()
    {
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        DisposePointerHandlers();
        if (SliderTrack is not null)
        {
            SliderTrack.ThumbsChanged -= HandleTrackThumbsChanged;
        }

        SliderTrack = e.NameScope.Find<SliderTrack>("PART_Track");
        if (SliderTrack is not null)
        {
            SliderTrack.IgnoreThumbDrag = true;
            SliderTrack.ThumbsChanged += HandleTrackThumbsChanged;
        }

        _pointerPressDispose = this.AddDisposableHandler(PointerPressedEvent, TrackPressed, RoutingStrategies.Tunnel);
        _pointerReleaseDispose = this.AddDisposableHandler(PointerReleasedEvent, TrackReleased, RoutingStrategies.Tunnel);
        _pointerMovedDispose = this.AddDisposableHandler(PointerMovedEvent, TrackMoved, RoutingStrategies.Tunnel);
        ConfigureTipHostWidth();
        ConfigureTemplateThumbTips();
        SetupSliderThumbPlacement();
        UpdatePseudoClasses(Orientation);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        DisposePointerHandlers();
        if (SliderTrack is not null)
        {
            SliderTrack.ThumbsChanged -= HandleTrackThumbsChanged;
        }
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (!IsEnabled)
        {
            return;
        }

        var direction = e.Key switch
        {
            Key.Left or Key.Down  => -SmallChange,
            Key.Right or Key.Up   => SmallChange,
            Key.PageDown          => -LargeChange,
            Key.PageUp            => LargeChange,
            Key.Home              => Minimum - Value,
            Key.End               => Maximum - Value,
            _                     => 0
        };

        if (direction == 0)
        {
            base.OnKeyDown(e);
            return;
        }

        if (!IsRangeMode || SliderTrack?.FocusedThumb is null)
        {
            MoveToNextTick(direction);
        }
        else
        {
            MoveRangeHandle(SliderTrack.FocusedThumb.HandleIndex, SliderTrack.FocusedThumbValue + direction);
        }

        e.Handled = true;
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

    protected override AutomationPeer OnCreateAutomationPeer()
    {
        return new SliderAutomationPeer(this);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == OrientationProperty)
        {
            UpdatePseudoClasses(change.GetNewValue<Orientation>());
            SetupSliderThumbPlacement();
        }
        else if (change.Property == IsRangeModeProperty)
        {
            ConfigureTipHostWidth();
            ConfigureTemplateThumbTips();
            SetupSliderThumbPlacement();
        }
        else if (change.Property == ValueProperty || change.Property == RangeValuesProperty)
        {
            ConfigureTemplateThumbTips();
        }

        if (this.IsAttachedToVisualTree() &&
            (change.Property == MaximumProperty || change.Property == ValueFormatTemplateProperty))
        {
            ConfigureTipHostWidth();
        }
    }

    protected virtual void OnThumbDragStarted(VectorEventArgs e)
    {
        IsDragging = true;
    }

    protected virtual void OnThumbDragCompleted(VectorEventArgs e)
    {
        IsDragging = false;
        _thumbDragValueOffset = 0;
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

    private void DisposePointerHandlers()
    {
        _pointerMovedDispose?.Dispose();
        _pointerMovedDispose = null;
        _pointerPressDispose?.Dispose();
        _pointerPressDispose = null;
        _pointerReleaseDispose?.Dispose();
        _pointerReleaseDispose = null;
    }

    private void ConfigureTemplateThumbTips()
    {
        if (SliderTrack is null)
        {
            return;
        }

        for (var i = 0; i < SliderTrack.Thumbs.Count; i++)
        {
            var thumb = SliderTrack.Thumbs[i];
            ToolTip.SetTip(thumb, FormatValue(SliderTrack.GetThumbValue(i)));
            ToolTip.SetTipHostWidth(thumb, _tipHostWidth);
        }
    }

    private void HandleTrackThumbsChanged(object? sender, EventArgs e)
    {
        ConfigureTemplateThumbTips();
        SetupSliderThumbPlacement();
    }

    private void MoveToNextTick(double direction)
    {
        if (direction == 0.0)
        {
            return;
        }

        var value = Value;
        var next = SnapToTick(Math.Max(Minimum, Math.Min(Maximum, value + direction)));
        var greaterThan = MathUtils.GreaterThan(direction, 0);

        if (Math.Abs(next - value) < Tolerance &&
            !(greaterThan && Math.Abs(value - Maximum) < Tolerance) &&
            !(!greaterThan && Math.Abs(value - Minimum) < Tolerance))
        {
            if (MathUtils.GreaterThan(TickFrequency, 0.0))
            {
                var tickNumber = Math.Round((value - Minimum) / TickFrequency);
                tickNumber += greaterThan ? 1.0 : -1.0;
                next = CalculateTickValue(tickNumber);
            }
        }

        if (Math.Abs(next - value) > Tolerance)
        {
            SetCurrentValue(ValueProperty, next);
        }
    }

    private void MoveRangeHandle(int handleIndex, double value)
    {
        var values = SliderTrack?.EffectiveRangeValues ?? RangeValues ?? [Minimum, Minimum];
        var next = IsSnapToTickEnabled ? SnapToTick(value) : value;
        SetCurrentValue(RangeValuesProperty,
            SliderRangeMath.MoveHandle(values, DisabledHandles, handleIndex, next, Minimum, Maximum));
    }

    private void MoveRangeTrack(double offset)
    {
        if (!IsRangeMode || !IsDraggableTrack)
        {
            return;
        }

        var values = _rangeTrackDragStartValues ?? SliderTrack?.EffectiveRangeValues ?? RangeValues ?? [Minimum, Minimum];
        SetCurrentValue(RangeValuesProperty,
            SliderRangeMath.ApplyTrackOffset(values, DisabledHandles, offset, Minimum, Maximum));
    }

    private bool TryStartRangeTrackDrag(Point point)
    {
        if (SliderTrack is null || !SliderTrack.CanDragRangeTrackAt(point))
        {
            return false;
        }

        _isRangeTrackDragging      = true;
        _rangeTrackDragStartValue  = SliderTrack.ValueFromPoint(point);
        _rangeTrackDragStartValues = SliderTrack.EffectiveRangeValues.ToArray();
        return true;
    }

    private void MoveRangeTrackToPoint(Point point)
    {
        if (SliderTrack is null)
        {
            return;
        }

        var value = SliderTrack.ValueFromPoint(point);
        if (IsSnapToTickEnabled)
        {
            value = SnapToTick(value);
        }

        MoveRangeTrack(value - _rangeTrackDragStartValue);
    }

    private void ClearRangeTrackDragSession()
    {
        _isRangeTrackDragging      = false;
        _rangeTrackDragStartValue  = 0;
        _rangeTrackDragStartValues = null;
    }

    private void TrackMoved(object? sender, PointerEventArgs e)
    {
        if (!IsEnabled)
        {
            IsDragging = false;
            return;
        }

        if (IsDragging && _graspedThumb is not null)
        {
            MoveToPoint(e.GetCurrentPoint(SliderTrack));
        }
        else if (IsDragging && _isRangeTrackDragging && SliderTrack is not null)
        {
            MoveRangeTrackToPoint(e.GetCurrentPoint(SliderTrack).Position);
        }
    }

    private void TrackReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (_graspedThumb is not null)
        {
            ToolTip.SetIsCustomShowAndHide(_graspedThumb, false);
            if (!_graspedThumb.Bounds.Contains(e.GetPosition(SliderTrack)))
            {
                ToolTip.SetIsOpen(_graspedThumb, false);
            }
        }

        IsDragging    = false;
        _graspedThumb = null;
        _thumbDragValueOffset = 0;
        ClearRangeTrackDragSession();
    }

    private void TrackPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed || SliderTrack is null)
        {
            return;
        }

        var posOnTrack = e.GetCurrentPoint(SliderTrack);
        _thumbDragValueOffset = 0;
        if (TryStartRangeTrackDrag(posOnTrack.Position))
        {
            IsDragging = true;
            return;
        }

        _graspedThumb = GetEffectiveMoveThumb(posOnTrack.Position);
        if (_graspedThumb is null)
        {
            return;
        }

        if (_graspedThumb.Bounds.Contains(posOnTrack.Position))
        {
            _thumbDragValueOffset = SliderTrack.ValueFromPoint(posOnTrack.Position) -
                                    SliderTrack.GetThumbValue(_graspedThumb.HandleIndex);
        }

        var mark = SliderTrack.GetMarkForPosition(posOnTrack.Position);
        if (mark is not null)
        {
            MoveCurrentThumbToValue(mark.Value);
            return;
        }

        MoveToPoint(posOnTrack);
        ToolTip.SetIsCustomShowAndHide(_graspedThumb, true);
        IsDragging = true;
    }

    private void MoveToPoint(PointerPoint posOnTrack)
    {
        if (SliderTrack is null || _graspedThumb is null)
        {
            return;
        }

        MoveCurrentThumbToValue(SliderTrack.ValueFromPoint(posOnTrack.Position) - _thumbDragValueOffset);
        if (!_graspedThumb.IsFocused)
        {
            _graspedThumb.Focus();
        }
    }

    private void MoveCurrentThumbToValue(double value)
    {
        if (_graspedThumb is null)
        {
            return;
        }

        var finalValue = IsSnapToTickEnabled ? SnapToTick(value) : value;
        if (!IsRangeMode)
        {
            SetCurrentValue(ValueProperty, finalValue);
        }
        else
        {
            MoveRangeHandle(_graspedThumb.HandleIndex, finalValue);
        }
    }

    private SliderThumb? GetEffectiveMoveThumb(Point point)
    {
        if (SliderTrack is null)
        {
            return null;
        }

        if (!IsRangeMode)
        {
            return SliderTrack.Thumbs.Count > 0 ? SliderTrack.Thumbs[0] : null;
        }

        var value = SliderTrack.ValueFromPoint(point);
        var index = SliderRangeMath.FindNearestEnabledHandleIndex(
            SliderTrack.EffectiveRangeValues,
            DisabledHandles,
            value);
        return index >= 0 && index < SliderTrack.Thumbs.Count ? SliderTrack.Thumbs[index] : null;
    }

    private void SetupSliderThumbPlacement()
    {
        if (SliderTrack is null)
        {
            return;
        }

        var placement = Orientation == Orientation.Horizontal ? PlacementMode.Top : PlacementMode.Right;
        foreach (var thumb in SliderTrack.Thumbs)
        {
            ToolTip.SetPlacement(thumb, placement);
        }
    }

    private string FormatValue(double value)
    {
        return string.Format(ValueFormatTemplate, value);
    }

    private double SnapToTick(double value)
    {
        if (IsSnapToTickEnabled)
        {
            var previous = Minimum;
            var next     = Maximum;

            if (MathUtils.GreaterThan(TickFrequency, 0.0))
            {
                var tickNumber = Math.Round((value - Minimum) / TickFrequency);
                previous = CalculateTickValue(tickNumber);
                next     = CalculateTickValue(tickNumber + 1.0);
            }

            value = MathUtils.GreaterThanOrClose(value, (previous + next) * 0.5) ? next : previous;
        }

        return value;
    }

    private double CalculateTickValue(double tickNumber)
    {
        if (TryCalculateDecimalTickValue(tickNumber, out var decimalTickValue))
        {
            return ClampToRange(decimalTickValue);
        }

        return ClampToRange(Minimum + tickNumber * TickFrequency);
    }

    private bool TryCalculateDecimalTickValue(double tickNumber, out double value)
    {
        value = default;
        if (!double.IsFinite(Minimum) ||
            !double.IsFinite(Maximum) ||
            !double.IsFinite(TickFrequency) ||
            !double.IsFinite(tickNumber))
        {
            return false;
        }

        try
        {
            value = (double)((decimal)Minimum + (decimal)tickNumber * (decimal)TickFrequency);
            return true;
        }
        catch (OverflowException)
        {
            return false;
        }
    }

    private double ClampToRange(double value)
    {
        return Math.Max(Minimum, Math.Min(Maximum, value));
    }

    private void UpdatePseudoClasses(Orientation o)
    {
        PseudoClasses.Set(StdPseudoClass.Vertical, o == Orientation.Vertical);
        PseudoClasses.Set(StdPseudoClass.Horizontal, o == Orientation.Horizontal);
    }

    private void ConfigureTipHostWidth()
    {
        var maxValueText = FormatValue(Maximum);
        var size = TextUtils.CalculateTextSize(maxValueText, FontSize, FontFamily);
        _tipHostWidth = size.Width * 1.1;
        ConfigureTemplateThumbTips();
    }
}
