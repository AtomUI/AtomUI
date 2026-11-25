using AtomUI.Controls;
using AtomUI.Media;
using Avalonia;
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

[PseudoClasses(StdPseudoClass.Vertical, StdPseudoClass.Horizontal, StdPseudoClass.Pressed)]
public partial class Slider : AbstractSlider,
                      IMotionAwareControl
{
    #region 公共属性定义

    public static readonly StyledProperty<Orientation> OrientationProperty =
        StackPanel.OrientationProperty.AddOwner<Slider>();

    public static readonly StyledProperty<bool> IsDirectionReversedProperty =
        SliderTrack.IsDirectionReversedProperty.AddOwner<Slider>();

    public new static readonly StyledProperty<bool> IsSnapToTickEnabledProperty =
        AbstractSlider.IsSnapToTickEnabledProperty.AddOwner<Slider>();

    public new static readonly StyledProperty<double> TickFrequencyProperty =
        AbstractSlider.TickFrequencyProperty.AddOwner<Slider>();

    public new static readonly StyledProperty<IReadOnlyList<double>?> RangeValuesProperty =
        AbstractSlider.RangeValuesProperty.AddOwner<Slider>();

    public static readonly StyledProperty<IReadOnlyList<bool>?> DisabledHandlesProperty =
        SliderTrack.DisabledHandlesProperty.AddOwner<Slider>();

    public static readonly StyledProperty<bool> IsDraggableTrackProperty =
        SliderTrack.IsDraggableTrackProperty.AddOwner<Slider>();

    public static readonly StyledProperty<IBrush?> TrackBarBrushProperty =
        SliderTrack.TrackBarBrushProperty.AddOwner<Slider>();

    public new static readonly StyledProperty<IBrush?> TracksBrushProperty =
        AbstractSlider.TracksBrushProperty.AddOwner<Slider>();

    public new static readonly StyledProperty<bool> IsRangeModeProperty =
        AbstractSlider.IsRangeModeProperty.AddOwner<Slider>();

    public new static readonly StyledProperty<List<SliderMark>?> MarksProperty =
        AbstractSlider.MarksProperty.AddOwner<Slider>();

    public static readonly StyledProperty<string> ValueFormatTemplateProperty =
        AvaloniaProperty.Register<Slider, string>(nameof(ValueFormatTemplate), "{0:0}");

    public new static readonly StyledProperty<bool> IsIncludedProperty =
        AbstractSlider.IsIncludedProperty.AddOwner<Slider>();

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

    public new bool IsSnapToTickEnabled
    {
        get => GetValue(IsSnapToTickEnabledProperty);
        set => SetValue(IsSnapToTickEnabledProperty, value);
    }

    public new double TickFrequency
    {
        get => GetValue(TickFrequencyProperty);
        set => SetValue(TickFrequencyProperty, value);
    }

    public new IReadOnlyList<double>? RangeValues
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

    public new IBrush? TracksBrush
    {
        get => GetValue(TracksBrushProperty);
        set => SetValue(TracksBrushProperty, value);
    }

    public new bool IsRangeMode
    {
        get => GetValue(IsRangeModeProperty);
        set => SetValue(IsRangeModeProperty, value);
    }

    public new List<SliderMark>? Marks
    {
        get => GetValue(MarksProperty);
        set => SetValue(MarksProperty, value);
    }

    public string ValueFormatTemplate
    {
        get => GetValue(ValueFormatTemplateProperty);
        set => SetValue(ValueFormatTemplateProperty, value);
    }

    public new bool IsIncluded
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

    static Slider()
    {
        PressedMixin.Attach<Slider>();
        OrientationProperty.OverrideDefaultValue(typeof(Slider), Orientation.Horizontal);
        SliderThumb.DragStartedEvent.AddClassHandler<Slider>((x, e) => x.OnThumbDragStarted(e),
            RoutingStrategies.Bubble);
        SliderThumb.DragCompletedEvent.AddClassHandler<Slider>((x, e) => x.OnThumbDragCompleted(e),
            RoutingStrategies.Bubble);
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

    protected override void OnKeyDown(KeyEventArgs e) => base.OnKeyDown(e);

    protected override void MoveForKeyboard(double direction)
    {
        if (!IsRangeMode || SliderTrack?.FocusedThumb is null)
        {
            MoveToNextTick(direction);
        }
        else
        {
            var thumb = SliderTrack.FocusedThumb;
            MoveRangeHandle(thumb.HandleIndex, GetKeyboardValue(SliderTrack.FocusedThumbValue, direction));
        }
    }

    protected override void UpdateDataValidation(AvaloniaProperty property, BindingValueType state, Exception? error) =>
        base.UpdateDataValidation(property, state, error);

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

    protected override void NotifyFormValueChanged(object? value) => base.NotifyFormValueChanged(value);

    protected override void NotifySetFormValue(object? value) => base.NotifySetFormValue(value);

    protected override object? NotifyGetFormValue() => base.NotifyGetFormValue();

    protected override void NotifyClearFormValue() => base.NotifyClearFormValue();

    protected override void NotifyValidateStatus(FormValidateStatus status) => base.NotifyValidateStatus(status);

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
        SetCurrentValue(ValueProperty, GetKeyboardValue(Value, direction));
    }

    private void MoveRangeHandle(int handleIndex, double value)
    {
        var values = SliderTrack?.EffectiveRangeValues ?? RangeValues ?? [Minimum, Minimum];
        var next = QuantizeValue(value);
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

        var value = QuantizeValue(SliderTrack.ValueFromPoint(point));
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

        if (!IsRangeMode)
        {
            SetCurrentValue(ValueProperty, QuantizeValue(value));
        }
        else
        {
            MoveRangeHandle(_graspedThumb.HandleIndex, value);
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
