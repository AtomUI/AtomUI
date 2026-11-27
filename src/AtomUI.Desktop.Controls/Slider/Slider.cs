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

/// <summary>
/// A slider that lets the user pick one value, or one value per handle while <see cref="IsRangeMode" /> is set,
/// by moving a thumb along a track. Each thumb shows a tooltip with its value, and the rail can carry marks.
/// </summary>
[PseudoClasses(StdPseudoClass.Vertical, StdPseudoClass.Horizontal, StdPseudoClass.Pressed)]
public partial class Slider : AbstractSlider,
                      IMotionAwareControl
{
    #region 公共属性定义

    /// <summary>
    /// Identifies the <see cref="Orientation" /> property.
    /// </summary>
    public static readonly StyledProperty<Orientation> OrientationProperty =
        StackPanel.OrientationProperty.AddOwner<Slider>();

    /// <summary>
    /// Identifies the <see cref="IsDirectionReversed" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsDirectionReversedProperty =
        SliderTrack.IsDirectionReversedProperty.AddOwner<Slider>();

    /// <summary>
    /// Identifies the <see cref="IsSnapToTickEnabled" /> property.
    /// </summary>
    public new static readonly StyledProperty<bool> IsSnapToTickEnabledProperty =
        AbstractSlider.IsSnapToTickEnabledProperty.AddOwner<Slider>();

    /// <summary>
    /// Identifies the <see cref="TickFrequency" /> property.
    /// </summary>
    public new static readonly StyledProperty<double> TickFrequencyProperty =
        AbstractSlider.TickFrequencyProperty.AddOwner<Slider>();

    /// <summary>
    /// Identifies the <see cref="RangeValues" /> property.
    /// </summary>
    public new static readonly StyledProperty<IReadOnlyList<double>?> RangeValuesProperty =
        AbstractSlider.RangeValuesProperty.AddOwner<Slider>();

    /// <summary>
    /// Identifies the <see cref="DisabledHandles" /> property.
    /// </summary>
    public static readonly StyledProperty<IReadOnlyList<bool>?> DisabledHandlesProperty =
        SliderTrack.DisabledHandlesProperty.AddOwner<Slider>();

    /// <summary>
    /// Identifies the <see cref="IsDraggableTrack" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsDraggableTrackProperty =
        SliderTrack.IsDraggableTrackProperty.AddOwner<Slider>();

    /// <summary>
    /// Identifies the <see cref="TrackBarBrush" /> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> TrackBarBrushProperty =
        SliderTrack.TrackBarBrushProperty.AddOwner<Slider>();

    /// <summary>
    /// Identifies the <see cref="TracksBrush" /> property.
    /// </summary>
    public new static readonly StyledProperty<IBrush?> TracksBrushProperty =
        AbstractSlider.TracksBrushProperty.AddOwner<Slider>();

    /// <summary>
    /// Identifies the <see cref="IsRangeMode" /> property.
    /// </summary>
    public new static readonly StyledProperty<bool> IsRangeModeProperty =
        AbstractSlider.IsRangeModeProperty.AddOwner<Slider>();

    /// <summary>
    /// Identifies the <see cref="Marks" /> property.
    /// </summary>
    public new static readonly StyledProperty<List<SliderMark>?> MarksProperty =
        AbstractSlider.MarksProperty.AddOwner<Slider>();

    /// <summary>
    /// Identifies the <see cref="ValueFormatTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<string> ValueFormatTemplateProperty =
        AvaloniaProperty.Register<Slider, string>(nameof(ValueFormatTemplate), "{0:0}");

    /// <summary>
    /// Identifies the <see cref="IsIncluded" /> property.
    /// </summary>
    public new static readonly StyledProperty<bool> IsIncludedProperty =
        AbstractSlider.IsIncludedProperty.AddOwner<Slider>();

    /// <summary>
    /// Identifies the <see cref="IsMotionEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<Slider>();

    /// <summary>
    /// Identifies the <see cref="IsWaveSpiritEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsWaveSpiritEnabledProperty =
        WaveSpiritAwareControlProperty.IsWaveSpiritEnabledProperty.AddOwner<Slider>();

    /// <summary>
    /// Direction in which the track is laid out: horizontal by default. A vertical slider fills the height of its
    /// parent and aligns to the left; a horizontal one fills the width and aligns to the top. The tooltip of a
    /// thumb opens to the right of a vertical slider and above it otherwise.
    /// </summary>
    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    /// <summary>
    /// Flips the direction in which the values grow along the track: the minimum then lies at the right end of a
    /// horizontal slider and at the top of a vertical one, instead of at the left end and at the bottom. The value
    /// is passed to the track by a template binding; the track mirrors the positions of the handles accordingly.
    /// </summary>
    public bool IsDirectionReversed
    {
        get => GetValue(IsDirectionReversedProperty);
        set => SetValue(IsDirectionReversedProperty, value);
    }

    /// <inheritdoc cref="AbstractSlider.IsSnapToTickEnabled" />
    public new bool IsSnapToTickEnabled
    {
        get => GetValue(IsSnapToTickEnabledProperty);
        set => SetValue(IsSnapToTickEnabledProperty, value);
    }

    /// <inheritdoc cref="AbstractSlider.TickFrequency" />
    public new double TickFrequency
    {
        get => GetValue(TickFrequencyProperty);
        set => SetValue(TickFrequencyProperty, value);
    }

    /// <inheritdoc cref="AbstractSlider.RangeValues" />
    public new IReadOnlyList<double>? RangeValues
    {
        get => GetValue(RangeValuesProperty);
        set => SetValue(RangeValuesProperty, value);
    }

    /// <summary>
    /// Handles that the user cannot move, one entry per handle in the order of <see cref="RangeValues" />. A
    /// disabled handle gets a disabled thumb and is never picked by a press on the track. While a handle is
    /// disabled, the range cannot be dragged as a whole. <see langword="null" />, the default, leaves every handle
    /// movable.
    /// </summary>
    public IReadOnlyList<bool>? DisabledHandles
    {
        get => GetValue(DisabledHandlesProperty);
        set => SetValue(DisabledHandlesProperty, value);
    }

    /// <summary>
    /// In range mode, lets the user move all handles together by pressing the track and dragging. It has no effect
    /// outside range mode and while a handle is disabled. The theme does not set it, so the default is
    /// <see langword="false" />.
    /// </summary>
    public bool IsDraggableTrack
    {
        get => GetValue(IsDraggableTrackProperty);
        set => SetValue(IsDraggableTrackProperty, value);
    }

    /// <summary>
    /// Brush of the filled track segments. The theme sets it to the <c>TrackBg</c> token, to <c>TrackHoverBg</c>
    /// while the pointer is over the slider and to <c>TrackBgDisabled</c> while the slider is disabled. A value
    /// that is set on the slider replaces these colors in every state; when it is cleared, the colors of the theme
    /// apply again.
    /// </summary>
    public IBrush? TrackBarBrush
    {
        get => GetValue(TrackBarBrushProperty);
        set => SetValue(TrackBarBrushProperty, value);
    }

    /// <inheritdoc cref="AbstractSlider.TracksBrush" />
    public new IBrush? TracksBrush
    {
        get => GetValue(TracksBrushProperty);
        set => SetValue(TracksBrushProperty, value);
    }

    /// <inheritdoc cref="AbstractSlider.IsRangeMode" />
    public new bool IsRangeMode
    {
        get => GetValue(IsRangeModeProperty);
        set => SetValue(IsRangeModeProperty, value);
    }

    /// <inheritdoc cref="AbstractSlider.Marks" />
    public new List<SliderMark>? Marks
    {
        get => GetValue(MarksProperty);
        set => SetValue(MarksProperty, value);
    }

    /// <summary>
    /// Composite format string for the value in the tooltip of a thumb; item 0 is the value. The default is
    /// <c>{0:0}</c>. The width of the tooltip is the width of the formatted <see cref="RangeBase.Maximum" /> plus
    /// ten percent, measured with the font of the slider.
    /// </summary>
    /// <exception cref="FormatException">
    /// The template is not a valid composite format string. It is thrown when the tooltip text is produced, for
    /// example when the template is applied or when this property changes on an attached slider.
    /// </exception>
    public string ValueFormatTemplate
    {
        get => GetValue(ValueFormatTemplateProperty);
        set => SetValue(ValueFormatTemplateProperty, value);
    }

    /// <inheritdoc cref="AbstractSlider.IsIncluded" />
    public new bool IsIncluded
    {
        get => GetValue(IsIncludedProperty);
        set => SetValue(IsIncludedProperty, value);
    }

    /// <summary>
    /// Enables the transitions of the slider. While it is set, the brushes of the groove and of the filled track
    /// segments change with a transition instead of at once, and the theme of the thumb applies its motion
    /// styles. The value is passed to the track by a template binding and from there to the thumbs. When the
    /// value is cleared, the theme sets it from the <c>EnableMotion</c> token.
    /// </summary>
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    /// <summary>
    /// Whether the wave spirit effect is enabled. This is the <c>IsWaveSpiritEnabled</c> styled property that
    /// <c>WaveSpiritAwareControlProperty</c> defines, owned by the slider as well. The templates of the slider, of
    /// its track and of its thumbs do not read the value.
    /// </summary>
    public bool IsWaveSpiritEnabled
    {
        get => GetValue(IsWaveSpiritEnabledProperty);
        set => SetValue(IsWaveSpiritEnabledProperty, value);
    }

    #endregion

    /// <summary>
    /// Whether the pointer is currently dragging a thumb or the whole range of the track. It is set when a thumb
    /// starts its drag and when a press with the left button on the track picks a thumb or the range. It is cleared
    /// when the pointer is released, when the drag of the thumb completes and when the pointer moves over a
    /// disabled slider.
    /// </summary>
    protected bool IsDragging { get; private set; }

    /// <summary>
    /// The track of the template, found under the name <c>PART_Track</c> when the template is applied. It is
    /// <see langword="null" /> until then and when the template has no such part.
    /// </summary>
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

    /// <summary>
    /// Connects the slider to the track of the template. It finds <see cref="SliderTrack" />, tells the track to
    /// leave the movement of its thumbs to the slider, and subscribes to the changes of its thumbs. It attaches
    /// the pointer handlers that drag a thumb or the whole range, and sets up the tooltips, their placement and
    /// the orientation pseudo-classes. The handlers and the subscription of a previously applied template are
    /// released first. It is called when a template is applied; an override must call the base member, otherwise
    /// the slider cannot be dragged.
    /// </summary>
    /// <param name="e">
    /// The event data of the applied template.
    /// </param>
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

    /// <summary>
    /// Releases the pointer handlers and the subscription to the track when the slider leaves the visual tree.
    /// They are attached again only when <see cref="OnApplyTemplate" /> runs. An override must call the base member.
    /// </summary>
    /// <param name="e">
    /// The event data of the detachment.
    /// </param>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        DisposePointerHandlers();
        if (SliderTrack is not null)
        {
            SliderTrack.ThumbsChanged -= HandleTrackThumbsChanged;
        }
    }

    /// <inheritdoc />
    protected override void OnKeyDown(KeyEventArgs e) => base.OnKeyDown(e);

    /// <summary>
    /// Applies a keyboard step with <c>SetCurrentValue</c>, so a binding stays in place. Outside range mode it adds
    /// the step to <see cref="RangeBase.Value" />. In range mode it adds the step to the handle of the thumb that
    /// has the focus and leaves <see cref="RangeBase.Value" /> unchanged; when no thumb has the focus, it adds the
    /// step to <see cref="RangeBase.Value" /> instead of to the first handle, which the base member would move. It
    /// does not call the base member.
    /// </summary>
    /// <param name="direction">
    /// The step to add, in the units of <see cref="RangeBase.Value" />.
    /// <see cref="double.NegativeInfinity" /> steps to <see cref="RangeBase.Minimum" />, and
    /// <see cref="double.PositiveInfinity" /> steps to <see cref="RangeBase.Maximum" />.
    /// </param>
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

    /// <inheritdoc />
    protected override void UpdateDataValidation(AvaloniaProperty property, BindingValueType state, Exception? error) =>
        base.UpdateDataValidation(property, state, error);

    /// <summary>
    /// Creates a <c>SliderAutomationPeer</c> so that UI Automation sees the slider as a slider control.
    /// </summary>
    protected override AutomationPeer OnCreateAutomationPeer()
    {
        return new SliderAutomationPeer(this);
    }

    /// <summary>
    /// Updates the parts of the template after the base member has handled a property change. A change of
    /// <see cref="Orientation" /> updates the pseudo-classes and the placement of the tooltips. A change of
    /// <see cref="RangeBase.Value" /> or <see cref="RangeValues" /> refreshes the text of the tooltips, and a
    /// change of <see cref="IsRangeMode" /> refreshes the text, the width of the tooltip host and the placement.
    /// While the slider is attached to the visual tree, a change of <see cref="RangeBase.Maximum" /> or
    /// <see cref="ValueFormatTemplate" /> updates the width of the tooltip host. An override must call the base
    /// member.
    /// </summary>
    /// <param name="change">
    /// The change to handle.
    /// </param>
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

    /// <summary>
    /// Sets <see cref="IsDragging" /> when a thumb of the slider starts a drag. The slider receives the event of
    /// the thumb through a class handler. An override must call the base member.
    /// </summary>
    /// <param name="e">
    /// The drag event of the thumb.
    /// </param>
    protected virtual void OnThumbDragStarted(VectorEventArgs e)
    {
        IsDragging = true;
    }

    /// <summary>
    /// Clears <see cref="IsDragging" /> and the offset between the value at the pointer and the value of the
    /// grasped thumb when a thumb of the slider completes a drag. The slider receives the event of the thumb
    /// through a class handler. An override must call the base member.
    /// </summary>
    /// <param name="e">
    /// The drag event of the thumb.
    /// </param>
    protected virtual void OnThumbDragCompleted(VectorEventArgs e)
    {
        IsDragging = false;
        _thumbDragValueOffset = 0;
    }

    /// <inheritdoc />
    protected override void NotifyFormValueChanged(object? value) => base.NotifyFormValueChanged(value);

    /// <inheritdoc />
    protected override void NotifySetFormValue(object? value) => base.NotifySetFormValue(value);

    /// <inheritdoc />
    protected override object? NotifyGetFormValue() => base.NotifyGetFormValue();

    /// <inheritdoc />
    protected override void NotifyClearFormValue() => base.NotifyClearFormValue();

    /// <inheritdoc />
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
