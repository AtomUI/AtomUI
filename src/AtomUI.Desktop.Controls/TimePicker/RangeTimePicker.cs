using AtomUI.Controls.Utils;
using AtomUI.Desktop.Controls.Primitives;
using AtomUI.Icons.AntDesign;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Input;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// An input field for a range of times of day. Each of its two text boxes, the start and the end, takes a
/// typed time or picks it in a popup with hour, minute, second and AM/PM columns; the results are
/// <see cref="RangeStartSelectedTime" /> and <see cref="RangeEndSelectedTime" />.
/// </summary>
public partial class RangeTimePicker : RangeInfoPickerInput
{
    #region 公共属性定义

    /// <summary>
    /// Identifies the <see cref="RangeStartSelectedTime" /> property.
    /// </summary>
    public static readonly StyledProperty<TimeSpan?> RangeStartSelectedTimeProperty =
        AvaloniaProperty.Register<RangeTimePicker, TimeSpan?>(nameof(RangeStartSelectedTime),
            defaultBindingMode: BindingMode.TwoWay,
            enableDataValidation: true);

    /// <summary>
    /// Identifies the <see cref="RangeEndSelectedTime" /> property.
    /// </summary>
    public static readonly StyledProperty<TimeSpan?> RangeEndSelectedTimeProperty =
        AvaloniaProperty.Register<RangeTimePicker, TimeSpan?>(nameof(RangeEndSelectedTime),
            defaultBindingMode: BindingMode.TwoWay,
            enableDataValidation: true);

    /// <summary>
    /// Identifies the <see cref="RangeStartDefaultTime" /> property.
    /// </summary>
    public static readonly StyledProperty<TimeSpan?> RangeStartDefaultTimeProperty =
        AvaloniaProperty.Register<RangeTimePicker, TimeSpan?>(nameof(RangeStartDefaultTime),
            enableDataValidation: true);

    /// <summary>
    /// Identifies the <see cref="RangeEndDefaultTime" /> property.
    /// </summary>
    public static readonly StyledProperty<TimeSpan?> RangeEndDefaultTimeProperty =
        AvaloniaProperty.Register<RangeTimePicker, TimeSpan?>(nameof(RangeEndDefaultTime),
            enableDataValidation: true);
    
    /// <summary>
    /// Identifies the <see cref="MinuteIncrement" /> property.
    /// </summary>
    public static readonly StyledProperty<int> MinuteIncrementProperty =
        AvaloniaProperty.Register<RangeTimePicker, int>(nameof(MinuteIncrement), 1, coerce: CoerceMinuteIncrement);

    /// <summary>
    /// Identifies the <see cref="SecondIncrement" /> property.
    /// </summary>
    public static readonly StyledProperty<int> SecondIncrementProperty =
        AvaloniaProperty.Register<RangeTimePicker, int>(nameof(SecondIncrement), 1, coerce: CoerceSecondIncrement);
    
    /// <summary>
    /// Identifies the <see cref="ClockIdentifier" /> property.
    /// </summary>
    public static readonly StyledProperty<ClockIdentifierType> ClockIdentifierProperty =
        AvaloniaProperty.Register<RangeTimePicker, ClockIdentifierType>(nameof(ClockIdentifier), ClockIdentifierType.HourClock24);
    
    /// <summary>
    /// Identifies the <see cref="IsNeedConfirm" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsNeedConfirmProperty =
        AvaloniaProperty.Register<RangeTimePicker, bool>(nameof(IsNeedConfirm), true);
    
    /// <summary>
    /// Identifies the <see cref="IsChangeOnScroll" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsChangeOnScrollProperty =
        TimePicker.IsChangeOnScrollProperty.AddOwner<RangeTimePicker>();

    /// <summary>
    /// Identifies the <see cref="IsShowNow" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsShowNowProperty =
        AvaloniaProperty.Register<RangeTimePicker, bool>(nameof(IsShowNow), true);

    /// <summary>
    /// Gets or sets the picked start time of day, or <see langword="null" /> when the start is not selected.
    /// Setting it rewrites the start input with the value formatted for <see cref="ClockIdentifier" /> and
    /// notifies a hosting form of the new value. Clearing it makes the start input show the formatted
    /// <see cref="RangeStartDefaultTime" />, or nothing when no default is set. While the popup is open, the
    /// start input follows the time the popup shows for the start, and the popup writes the start time it
    /// confirms here. Binds two-way by default.
    /// </summary>
    public TimeSpan? RangeStartSelectedTime
    {
        get => GetValue(RangeStartSelectedTimeProperty);
        set => SetValue(RangeStartSelectedTimeProperty, value);
    }

    /// <summary>
    /// Gets or sets the picked end time of day, or <see langword="null" /> when the end is not selected.
    /// Setting it rewrites the end input with the value formatted for <see cref="ClockIdentifier" /> and
    /// notifies a hosting form of the new value. Clearing it makes the end input show the formatted
    /// <see cref="RangeEndDefaultTime" />, or nothing when no default is set. While the popup is open, the end
    /// input follows the time the popup shows for the end, and the popup writes the end time it confirms here.
    /// Binds two-way by default.
    /// </summary>
    public TimeSpan? RangeEndSelectedTime
    {
        get => GetValue(RangeEndSelectedTimeProperty);
        set => SetValue(RangeEndSelectedTimeProperty, value);
    }

    /// <summary>
    /// Gets or sets the time that becomes the selected start time when the picker joins the logical tree with
    /// no selected start time, that <see cref="Reset" /> restores and that the start input shows while no start
    /// time is selected. <see cref="Clear" /> does not apply it. The default is <see langword="null" />.
    /// </summary>
    public TimeSpan? RangeStartDefaultTime
    {
        get => GetValue(RangeStartDefaultTimeProperty);
        set => SetValue(RangeStartDefaultTimeProperty, value);
    }

    /// <summary>
    /// Gets or sets the time that becomes the selected end time when the picker joins the logical tree with
    /// no selected end time, that <see cref="Reset" /> restores and that the end input shows while no end time
    /// is selected. <see cref="Clear" /> does not apply it. The default is <see langword="null" />.
    /// </summary>
    public TimeSpan? RangeEndDefaultTime
    {
        get => GetValue(RangeEndDefaultTimeProperty);
        set => SetValue(RangeEndDefaultTimeProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the step, in minutes, between the minute options offered in the popup. The Now button also
    /// rounds the current minute down to a multiple of it. Valid values are 1 to 59; any other value throws an
    /// <see cref="ArgumentOutOfRangeException" />. The default is 1.
    /// </summary>
    public int MinuteIncrement
    {
        get => GetValue(MinuteIncrementProperty);
        set => SetValue(MinuteIncrementProperty, value);
    }

    /// <summary>
    /// Gets or sets the step, in seconds, between the second options offered in the popup. The Now button also
    /// rounds the current second down to a multiple of it. Valid values are 1 to 59; any other value throws an
    /// <see cref="ArgumentOutOfRangeException" />. The default is 1.
    /// </summary>
    public int SecondIncrement
    {
        get => GetValue(SecondIncrementProperty);
        set => SetValue(SecondIncrementProperty, value);
    }

    /// <summary>
    /// Gets or sets the hour notation of the popup, of the two input texts and of the parsing of typed text.
    /// The default is <see cref="ClockIdentifierType.HourClock24" />.
    /// </summary>
    public ClockIdentifierType ClockIdentifier
    {
        get => GetValue(ClockIdentifierProperty);
        set => SetValue(ClockIdentifierProperty, value);
    }
    
    /// <summary>
    /// Gets or sets whether the popup has an OK button that commits the time it shows for the active endpoint.
    /// When <see langword="true" />, the footer shows that button, and closing the popup or switching to the
    /// other endpoint leaves the selected times unchanged. When <see langword="false" />, there is no OK button,
    /// the Now button is centered, and closing the popup or switching to the other endpoint commits the time the
    /// popup shows for the active endpoint; a close that reverts the edit commits nothing.
    /// </summary>
    public bool IsNeedConfirm
    {
        get => GetValue(IsNeedConfirmProperty);
        set => SetValue(IsNeedConfirmProperty, value);
    }
    
    /// <summary>
    /// Gets or sets whether turning the mouse wheel over a column of the popup changes the time picked for the
    /// active endpoint. The default is <see langword="false" />, which keeps scrolling from changing it.
    /// </summary>
    public bool IsChangeOnScroll
    {
        get => GetValue(IsChangeOnScrollProperty);
        set => SetValue(IsChangeOnScrollProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the popup footer shows a Now button, which moves the time shown for the active
    /// endpoint to the current time with the minutes and seconds rounded down to <see cref="MinuteIncrement" />
    /// and <see cref="SecondIncrement" />. The default is <see langword="true" />.
    /// </summary>
    public bool IsShowNow
    {
        get => GetValue(IsShowNowProperty);
        set => SetValue(IsShowNowProperty, value);
    }
    
    #endregion
    
    #region 内部属性定义
    
    internal static readonly DirectProperty<RangeTimePicker, double> PreferredWidthProperty =
        AvaloniaProperty.RegisterDirect<RangeTimePicker, double>(nameof(PreferredWidth),
            o => o.PreferredWidth,
            (o, v) => o.PreferredWidth = v);
    
    internal static readonly DirectProperty<RangeTimePicker, string?> AmTextProperty =
        AvaloniaProperty.RegisterDirect<RangeTimePicker, string?>(nameof(AmText),
            o => o.AmText,
            (o, v) => o.AmText = v);
    
    internal static readonly DirectProperty<RangeTimePicker, string?> PmTextProperty =
        AvaloniaProperty.RegisterDirect<RangeTimePicker, string?>(nameof(PmText),
            o => o.PmText,
            (o, v) => o.PmText = v);

    private double _preferredWidth;

    internal double PreferredWidth
    {
        get => _preferredWidth;
        set
        {
            if (MathUtils.AreClose(_preferredWidth, value))
            {
                return;
            }

            SetAndRaise(PreferredWidthProperty, ref _preferredWidth, value);
            InvalidateMeasure();
        }
    }
    
    private string? _amText;

    internal string? AmText
    {
        get => _amText;
        set => SetAndRaise(AmTextProperty, ref _amText, value);
    }
    
    private string? _pmText;

    internal string? PmText
    {
        get => _pmText;
        set => SetAndRaise(PmTextProperty, ref _pmText, value);
    }

    #endregion

    private const string AntDesignDefaultRangeStartInputWidthReferenceText = "Start time";
    private const string AntDesignDefaultRangeEndInputWidthReferenceText   = "End time";

    private TimePickerPresenter? _pickerPresenter;
    private bool _isStartConfirmed;
    private bool _isEndConfirmed;
    
    static RangeTimePicker()
    {
        AffectsMeasure<RangeTimePicker>(PreferredWidthProperty);
        RangeStartSelectedTimeProperty.Changed.AddClassHandler<RangeTimePicker>((picker, args) => picker.HandleSelectedValueChanged(args));
        RangeEndSelectedTimeProperty.Changed.AddClassHandler<RangeTimePicker>((picker, args) => picker.HandleSelectedValueChanged(args));
    }

    public RangeTimePicker()
    {
    }
    
    /// <summary>
    /// Clears the selected start time and end time and empties both inputs. Unlike <see cref="Reset" />, the
    /// default times are not applied. An edit in progress in the popup is ended first, and the popup is closed
    /// unless it is pinned open.
    /// </summary>
    public override void Clear()
    {
        _pickerPresenter?.EndEditing();
        SetCurrentValue(RangeStartSelectedTimeProperty, null);
        SetCurrentValue(RangeEndSelectedTimeProperty, null);
        base.Clear();
        ClosePickerFlyout();
        if (IsPickerOpen)
        {
            NotifyRangeActivatedPartChanged();
        }
    }
    
    /// <summary>
    /// Sets the selected start time and end time to <see cref="RangeStartDefaultTime" /> and
    /// <see cref="RangeEndDefaultTime" />, so an endpoint without a default time ends up not selected.
    /// </summary>
    public void Reset()
    {
        RangeStartSelectedTime = RangeStartDefaultTime;
        RangeEndSelectedTime = RangeEndDefaultTime;
    }
    
    /// <summary>
    /// Called by the base class to create the control shown in the popup. Returns a new
    /// <see cref="TimePickerPresenter" /> that follows this picker's <see cref="IsChangeOnScroll" />,
    /// <see cref="MinuteIncrement" />, <see cref="SecondIncrement" />, <see cref="ClockIdentifier" />,
    /// <see cref="IsNeedConfirm" />, <see cref="IsShowNow" /> and <c>IsMotionEnabled</c> through bindings. The
    /// selected time of the active endpoint is handed to it by <see cref="NotifyRangeActivatedPartChanged" />
    /// instead of a binding.
    /// </summary>
    protected override Control CreatePickerPresenter()
    {
        // This presenter is created in an independent Content subtree. The owner's
        // ControlTheme cannot bind into its separate template owner.
        var timePickerPresenter = new TimePickerPresenter();
        timePickerPresenter[!TimePickerPresenter.IsChangeOnScrollProperty] = this[!IsChangeOnScrollProperty];
        timePickerPresenter[!TimePickerPresenter.IsMotionEnabledProperty] = this[!IsMotionEnabledProperty];
        timePickerPresenter[!TimePickerPresenter.MinuteIncrementProperty] = this[!MinuteIncrementProperty];
        timePickerPresenter[!TimePickerPresenter.SecondIncrementProperty] = this[!SecondIncrementProperty];
        timePickerPresenter[!TimePickerPresenter.ClockIdentifierProperty] = this[!ClockIdentifierProperty];
        timePickerPresenter[!TimePickerPresenter.IsNeedConfirmProperty]   = this[!IsNeedConfirmProperty];
        timePickerPresenter[!TimePickerPresenter.IsShowNowProperty]       = this[!IsShowNowProperty];
        return timePickerPresenter;
    }

    /// <summary>
    /// Called by the base class with the popup presenter it created. After calling the base implementation, the
    /// presenter is stored as the <see cref="TimePickerPresenter" /> that edits the active endpoint while the
    /// popup is open.
    /// </summary>
    protected override void NotifyPickerPresenterCreated(Control pickerPresenter)
    {
        base.NotifyPickerPresenterCreated(pickerPresenter);
        _pickerPresenter = pickerPresenter as TimePickerPresenter;
    }

    /// <summary>
    /// Called by the base class before it releases the popup presenter it created. After calling the base
    /// implementation, the picker drops its reference to the <see cref="TimePickerPresenter" />.
    /// </summary>
    protected override void NotifyPickerPresenterCleared(Control pickerPresenter)
    {
        base.NotifyPickerPresenterCleared(pickerPresenter);
        _pickerPresenter = null;
    }

    /// <summary>
    /// Called by the base class when the popup opens. It activates the start endpoint when no endpoint is
    /// active and, for this open session, marks every endpoint that has a time as confirmed unless both do.
    /// After calling the base implementation, the picker subscribes to the <see cref="TimePickerPresenter" />
    /// events that drive the preview text, the confirmation, the dismissal and the focus handling, hands the
    /// active endpoint's time to the presenter and resets the presenter's panel state for the new opening.
    /// </summary>
    protected override void NotifyPickerOpened()
    {
        if (RangeActivatedPart == RangeActivatedPart.None)
        {
            RangeActivatedPart = RangeActivatedPart.Start;
        }
        var isComplete = RangeStartSelectedTime is not null && RangeEndSelectedTime is not null;
        _isStartConfirmed = !isComplete && RangeStartSelectedTime is not null;
        _isEndConfirmed   = !isComplete && RangeEndSelectedTime is not null;
        base.NotifyPickerOpened();
        if (_pickerPresenter is not null)
        {
            _pickerPresenter.ChoosingStatusChanged += HandleChoosingStatusChanged;
            _pickerPresenter.HoverTimeChanged      += HandleHoverTimeChanged;
            _pickerPresenter.Confirmed             += HandleConfirmed;
            _pickerPresenter.Dismissed += HandleDismissed;
            _pickerPresenter.CandidateChanged += HandleCandidateChanged;
            _pickerPresenter.FocusLeft += HandleFocusLeft;
            NotifyRangeActivatedPartChanged();
            _pickerPresenter.ResetOpenPanelState();
        }
    }

    /// <summary>
    /// Called when the popup is about to close, before <see cref="NotifyPickerClosed" />. While the presenter is
    /// editing, it ends the editing and, when <see cref="IsNeedConfirm" /> is <see langword="false" /> and the
    /// edit is not canceled, commits the time the popup shows as the selected time of the active endpoint. This
    /// happens before the base call because the base implementation deactivates the active endpoint; an override
    /// must keep that order.
    /// </summary>
    /// <param name="selectedIsValid">
    /// Whether the selection of the closing session is valid. This implementation does not use it.
    /// </param>
    protected override void NotifyFlyoutAboutToClose(bool selectedIsValid)
    {
        // RangeInfoPickerInput clears the active endpoint here. Finalize the draft
        // while its endpoint is still known, before that shared lifecycle transition.
        if (_pickerPresenter is { IsEditing: true })
        {
            var candidate = _pickerPresenter.EndEditing();
            if (!IsNeedConfirm && !IsCancelingEdit)
            {
                CommitEndpoint(RangeActivatedPart, candidate);
            }
        }
        base.NotifyFlyoutAboutToClose(selectedIsValid);
    }

    private void CommitEndpoint(RangeActivatedPart part, TimeSpan? value)
    {
        if (part == RangeActivatedPart.End)
        {
            SetCurrentValue(RangeEndSelectedTimeProperty, value);
        }
        else if (part == RangeActivatedPart.Start)
        {
            SetCurrentValue(RangeStartSelectedTimeProperty, value);
        }
    }

    /// <summary>
    /// Called by the base class when the popup closes. After calling the base implementation, the picker
    /// unsubscribes from the <see cref="TimePickerPresenter" />, ends its editing and rewrites both input texts
    /// from the selected times. The time of the active endpoint is committed before this, in
    /// <see cref="NotifyFlyoutAboutToClose" />.
    /// </summary>
    protected override void NotifyPickerClosed()
    {
        base.NotifyPickerClosed();
        if (_pickerPresenter is not null)
        {
            _pickerPresenter.ChoosingStatusChanged -= HandleChoosingStatusChanged;
            _pickerPresenter.HoverTimeChanged      -= HandleHoverTimeChanged;
            _pickerPresenter.Confirmed             -= HandleConfirmed;
            _pickerPresenter.Dismissed -= HandleDismissed;
            _pickerPresenter.CandidateChanged -= HandleCandidateChanged;
            _pickerPresenter.FocusLeft -= HandleFocusLeft;
            _pickerPresenter.EndEditing();
            RefreshRangeTexts();
        }
    }
    
    private void HandleChoosingStatusChanged(object? sender, ChoosingStatusEventArgs args)
    {
        IsChoosing = args.IsChoosing;
        UpdatePseudoClasses();
        if (!args.IsChoosing)
        {
            ClearHoverSelectedInfo();
        }
    }
    
    private void ClearHoverSelectedInfo()
    {
        SetPreviewing(_pickerPresenter?.PreviewTime != null);
        var display = _pickerPresenter?.DisplayTime;
        var text = _pickerPresenter?.PreviewTime == null && _pickerPresenter?.InputText != null
            ? _pickerPresenter.InputText
            : DateTimeUtils.FormatTimeSpan(display, ClockIdentifier == ClockIdentifierType.HourClock12, AmText, PmText);
        if (RangeActivatedPart == RangeActivatedPart.End)
        {
            SecondaryText = text;
        }
        else
        {
            Text = text;
        }
    }

    private void HandleCandidateChanged(object? sender, EventArgs args) => ClearHoverSelectedInfo();
    private void HandleHoverTimeChanged(object? sender, TimeSelectedEventArgs args) => ClearHoverSelectedInfo();
    // Escape inside the popup panel reverts like Escape in the input.
    private void HandleDismissed(object? sender, EventArgs args) => CancelPickerEdit();

    private void HandleConfirmed(object? sender, EventArgs args)
    {
        // As in Ant Design, a confirmed endpoint hands over to the other one until both are confirmed in this open
        // session; an endpoint that was the only filled one at open counts as confirmed.
        if (RangeActivatedPart == RangeActivatedPart.Start)
        {
            SetCurrentValue(RangeStartSelectedTimeProperty, _pickerPresenter?.SelectedTime);
            _isStartConfirmed = RangeStartSelectedTime is not null;
            if (RangeEndSelectedTime is null || !_isEndConfirmed)
            {
                RangeActivatedPart = RangeActivatedPart.End;
                _pickerPresenter?.ResetOpenPanelState();
                return;
            }
        }
        else if (RangeActivatedPart == RangeActivatedPart.End)
        {
            SetCurrentValue(RangeEndSelectedTimeProperty, _pickerPresenter?.SelectedTime);
            _isEndConfirmed = RangeEndSelectedTime is not null;
            if (RangeStartSelectedTime is null || !_isStartConfirmed)
            {
                RangeActivatedPart = RangeActivatedPart.Start;
                _pickerPresenter?.ResetOpenPanelState();
                return;
            }
        }

        ClosePickerFlyout();
        if (IsPickerOpen)
        {
            _pickerPresenter?.ResetOpenPanelState();
        }
    }
    
    private static int CoerceMinuteIncrement(AvaloniaObject sender, int value)
    {
        if (value < 1 || value > 59)
        {
            throw new ArgumentOutOfRangeException(null, "1 >= MinuteIncrement <= 59");
        }

        return value;
    }

    private static int CoerceSecondIncrement(AvaloniaObject sender, int value)
    {
        if (value < 1 || value > 59)
        {
            throw new ArgumentOutOfRangeException(null, "1 >= SecondIncrement <= 59");
        }

        return value;
    }
    
    /// <summary>
    /// Calls the base implementation and keeps the popup, the input texts and the preferred width in step with
    /// the changed property. Before the base call: when the active endpoint changes while the presenter is
    /// editing, the editing ends and, when <see cref="IsNeedConfirm" /> is <see langword="false" />, its time is
    /// committed as the selected time of the endpoint that was active before; both input texts are rewritten.
    /// After the base call: when the selected time of the active endpoint changes while the popup is open and
    /// the presenter is editing, the presenter shows that time; a change of <see cref="ClockIdentifier" /> or of
    /// the AM/PM text updates both input texts and the preferred width, a change of a font, size, width or
    /// alignment property updates only the preferred width, and while the control is attached to the visual tree
    /// a changed selected time rewrites its input text, which then shows the default time when the selected time
    /// was cleared and a default time is set.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        if (change.Property == RangeActivatedPartProperty && _pickerPresenter is { IsEditing: true })
        {
            var candidate = _pickerPresenter.EndEditing();
            if (!IsNeedConfirm)
            {
                CommitEndpoint(change.GetOldValue<RangeActivatedPart>(), candidate);
            }
            RefreshRangeTexts();
        }
        base.OnPropertyChanged(change);
        if (IsPickerOpen && _pickerPresenter is { IsEditing: true } &&
            ((change.Property == RangeStartSelectedTimeProperty && RangeActivatedPart == RangeActivatedPart.Start) ||
             (change.Property == RangeEndSelectedTimeProperty && RangeActivatedPart == RangeActivatedPart.End)))
        {
            _pickerPresenter.SetCurrentValue(TimePickerPresenter.SelectedTimeProperty,
                RangeActivatedPart == RangeActivatedPart.Start ? RangeStartSelectedTime : RangeEndSelectedTime);
            _pickerPresenter.ResetOpenPanelState();
        }
        if (IsFormattedTextAffectingProperty(change.Property))
        {
            RefreshRangeTexts();
            CalculatePreferredWidth();
        }
        else if (IsPreferredWidthAffectingProperty(change.Property))
        {
            CalculatePreferredWidth();
        }

        if (this.IsAttachedToVisualTree())
        {
            if (change.Property == RangeStartSelectedTimeProperty)
            {
                if (RangeStartSelectedTime.HasValue)
                {
                    Text = DateTimeUtils.FormatTimeSpan(RangeStartSelectedTime.Value,
                        ClockIdentifier == ClockIdentifierType.HourClock12, AmText, PmText);
                }
                else
                {
                    ResetRangeStartTimeValue();
                }
                CalculatePreferredWidth();
            }
            else if (change.Property == RangeEndSelectedTimeProperty)
            {
                if (RangeEndSelectedTime.HasValue)
                {
                    SecondaryText = DateTimeUtils.FormatTimeSpan(RangeEndSelectedTime.Value,
                        ClockIdentifier == ClockIdentifierType.HourClock12, AmText, PmText);
                }
                else
                {
                    ResetRangeEndTimeValue();
                }
                CalculatePreferredWidth();
            }
        }
    }

    private static bool IsFormattedTextAffectingProperty(AvaloniaProperty property)
    {
        return property == ClockIdentifierProperty ||
               property == AmTextProperty ||
               property == PmTextProperty;
    }

    private static bool IsPreferredWidthAffectingProperty(AvaloniaProperty property)
    {
        return property == FontSizeProperty ||
               property == FontFamilyProperty ||
               property == FontStyleProperty ||
               property == FontWeightProperty ||
               property == SizeTypeProperty ||
               property == MinWidthProperty ||
               property == WidthProperty ||
               property == MaxWidthProperty ||
               property == HorizontalAlignmentProperty;
    }

    private void RefreshRangeTexts()
    {
        if (RangeStartSelectedTime.HasValue)
        {
            Text = DateTimeUtils.FormatTimeSpan(RangeStartSelectedTime.Value,
                ClockIdentifier == ClockIdentifierType.HourClock12, AmText, PmText);
        }
        else
        {
            ResetRangeStartTimeValue();
        }

        if (RangeEndSelectedTime.HasValue)
        {
            SecondaryText = DateTimeUtils.FormatTimeSpan(RangeEndSelectedTime.Value,
                ClockIdentifier == ClockIdentifierType.HourClock12, AmText, PmText);
        }
        else
        {
            ResetRangeEndTimeValue();
        }
    }
    
    private void CalculatePreferredWidth()
    {
        if (!double.IsNaN(Width) || HorizontalAlignment == HorizontalAlignment.Stretch)
        {
            PreferredInputWidth = double.NaN;
            PreferredWidth      = 0;
        }
        else
        {
            var preferredInputWidth = CalculateContentPreferredWidth();

            if (!double.IsNaN(MinWidth))
            {
                preferredInputWidth = Math.Max(MinWidth, preferredInputWidth);
            }

            if (!double.IsNaN(MaxWidth))
            {
                preferredInputWidth = Math.Min(MaxWidth, preferredInputWidth);
            }
            PreferredInputWidth = preferredInputWidth;
            PreferredWidth      = preferredInputWidth;
        }
    }

    private double CalculateContentPreferredWidth()
    {
        var formatWidth = DateTimeUtils.CalculateWidestFormattedTimeSpanSize(
            ClockIdentifier == ClockIdentifierType.HourClock12,
            AmText, PmText,
            FontSize, FontFamily, FontStyle, FontWeight).Width;
        var defaultInputBaselineWidth = DatePickerFormattingHelper.CalculateAntDesignInputBaselineWidth(
            FontSize,
            FontFamily,
            FontStyle,
            FontWeight,
            AntDesignDefaultRangeStartInputWidthReferenceText,
            AntDesignDefaultRangeEndInputWidthReferenceText);

        return Math.Max(formatWidth, defaultInputBaselineWidth);
    }
    
    /// <summary>
    /// Rewrites the start input with <see cref="RangeStartDefaultTime" /> formatted for
    /// <see cref="ClockIdentifier" />, or with nothing when no default start time is set. The selected start
    /// time is not changed; the method only gives the start input its text while no start time is selected.
    /// </summary>
    protected void ResetRangeStartTimeValue()
    {
        if (RangeStartDefaultTime is not null)
        {
            Text = DateTimeUtils.FormatTimeSpan(RangeStartDefaultTime.Value,
                ClockIdentifier == ClockIdentifierType.HourClock12, AmText, PmText);
        }
        else
        {
            Text = null;
        }
    }
    
    /// <summary>
    /// Rewrites the end input with <see cref="RangeEndDefaultTime" /> formatted for
    /// <see cref="ClockIdentifier" />, or with nothing when no default end time is set. The selected end time
    /// is not changed; the method only gives the end input its text while no end time is selected.
    /// </summary>
    protected void ResetRangeEndTimeValue()
    {
        if (RangeEndDefaultTime is not null)
        {
            SecondaryText = DateTimeUtils.FormatTimeSpan(RangeEndDefaultTime.Value,
                ClockIdentifier == ClockIdentifierType.HourClock12, AmText, PmText);
        }
        else
        {
            SecondaryText = null;
        }
    }
    
    /// <summary>
    /// Calls the base implementation, then makes the popup show the selected time of the active endpoint, or no
    /// time when no endpoint is active, and, while the picker is open with an active endpoint, resets the panel
    /// state of the <see cref="TimePickerPresenter" /> for that endpoint. Call the base method from an override
    /// to keep the popup placement and the range indicator.
    /// </summary>
    protected override void NotifyRangeActivatedPartChanged()
    {
        base.NotifyRangeActivatedPartChanged();
        if (_pickerPresenter != null)
        {
            var value = RangeActivatedPart switch
            {
                RangeActivatedPart.Start => RangeStartSelectedTime,
                RangeActivatedPart.End => RangeEndSelectedTime,
                _ => (TimeSpan?)null
            };
            _pickerPresenter.SetCurrentValue(TimePickerPresenter.SelectedTimeProperty, value);
            if (IsPickerOpen && RangeActivatedPart != RangeActivatedPart.None)
            {
                _pickerPresenter.ResetOpenPanelState();
            }
        }
    }

    /// <summary>
    /// Measures the content with the base implementation and, while <c>PreferredWidth</c> is positive and both
    /// input boxes are present, widens the result so that each input box can get <c>PreferredWidth</c> instead
    /// of the width it asked for: the width becomes the base width minus the desired widths of the two input
    /// boxes plus twice <c>PreferredWidth</c>, or stays the base width when that is larger. The height is
    /// unchanged.
    /// </summary>
    protected override Size MeasureOverride(Size availableSize)
    {
        var size   = base.MeasureOverride(availableSize);
        var width  = size.Width;
        var height = size.Height;
        if (PreferredWidth > 0 &&
            InfoInputBox is not null &&
            SecondaryInfoInputBox is not null)
        {
            var currentInputWidth = InfoInputBox.DesiredSize.Width + SecondaryInfoInputBox.DesiredSize.Width;
            var preferredWidth    = size.Width - currentInputWidth + PreferredWidth * 2;
            width = Math.Max(width, preferredWidth);
        }

        return new Size(width, height);
    }

    /// <summary>
    /// Whether the start time or the end time is selected. The base class does not call this predicate; it
    /// drives the clear button through <see cref="RangeInfoPickerInput.ConfigureIsClearButtonVisible" /> instead.
    /// </summary>
    /// <returns>
    /// <see langword="true" /> when <see cref="RangeStartSelectedTime" /> or <see cref="RangeEndSelectedTime" />
    /// is not <see langword="null" />.
    /// </returns>
    protected override bool ShowClearButtonPredicate()
    {
        return RangeStartSelectedTime is not null || RangeEndSelectedTime is not null;
    }

    /// <summary>
    /// Calls the base implementation, then applies the default times: an endpoint whose selected time is
    /// <see langword="null" /> while its default time is set gets that default time as its selected time.
    /// </summary>
    protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnAttachedToLogicalTree(e);
        if (RangeStartDefaultTime is not null && RangeStartSelectedTime is null)
        {
            RangeStartSelectedTime = RangeStartDefaultTime;
        }
        
        if (RangeEndDefaultTime is not null && RangeEndSelectedTime is null)
        {
            RangeEndSelectedTime = RangeEndDefaultTime;
        }
    }
    
    // The presenter reports focus leaving it, which may happen inside the popup window, where the picker's own
    // LostFocus does not see the new focus target.
    private void HandleFocusLeft(object? sender, EventArgs args) =>
        CloseIfFocusLeftPicker(TopLevel.GetTopLevel(this)?.FocusManager?.GetFocusedElement());

    private void ReleaseInputHandlers()
    {
        if (InfoInputBox != null)
        {
            InfoInputBox.PropertyChanged -= HandleInputTextChanged;
        }
        if (SecondaryInfoInputBox != null)
        {
            SecondaryInfoInputBox.PropertyChanged -= HandleInputTextChanged;
        }
    }

    private void AttachInputHandlers()
    {
        ReleaseInputHandlers();
        if (InfoInputBox != null)
        {
            InfoInputBox.PropertyChanged += HandleInputTextChanged;
        }
        if (SecondaryInfoInputBox != null)
        {
            SecondaryInfoInputBox.PropertyChanged += HandleInputTextChanged;
        }
    }

    private void HandleInputTextChanged(object? sender, AvaloniaPropertyChangedEventArgs args)
    {
        if (args.Property != TextBox.TextProperty || sender is not TextBox input ||
            string.Equals(input.Text ?? string.Empty, (ReferenceEquals(input, SecondaryInfoInputBox) ? SecondaryText : Text) ?? string.Empty, StringComparison.Ordinal))
        {
            return;
        }
        var text = input.Text;
        RangeActivatedPart = ReferenceEquals(input, SecondaryInfoInputBox) ? RangeActivatedPart.End : RangeActivatedPart.Start;
        SetCurrentValue(IsPickerOpenProperty, true);
        var valid = TimePickerPresenter.TryParseInput(text, ClockIdentifier, AmText, PmText, out var time);
        _pickerPresenter?.SetInputCandidate(text, time, valid);
        if (RangeActivatedPart == RangeActivatedPart.End)
        {
            SetCurrentValue(SecondaryTextProperty, text);
        }
        else
        {
            SetCurrentValue(TextProperty, text);
        }
    }

    /// <summary>
    /// Whether the popup still holds text forwarded from one of the two inputs, including an empty string
    /// after an input was emptied; a confirmation, a cancel, a close or a reset of the popup drops it. While
    /// this is <see langword="true" />, the base class does not forward navigation keys to the popup.
    /// </summary>
    private protected override bool HasPendingInputText => _pickerPresenter?.InputText is not null;

    /// <summary>
    /// Forwards a key pressed without modifiers to the popup panel, which steps or switches its time columns,
    /// and returns whether the panel used the key. A key pressed with a modifier returns
    /// <see langword="false" />.
    /// </summary>
    private protected override bool NotifyNavigationKey(Key key, KeyModifiers modifiers) =>
        modifiers == KeyModifiers.None && _pickerPresenter?.NavigateByKey(key) == true;

    /// <summary>
    /// Enter in the open picker. When the user emptied the input of the active endpoint in this open session,
    /// clears both selected times as <see cref="Clear" /> does; otherwise the popup confirms the time it shows
    /// for the active endpoint, which makes it the selected time of that endpoint and then hands over to the
    /// other endpoint or closes the popup unless it is pinned open. It does nothing while the popup cannot
    /// confirm its candidate. The base implementation is not called, so Enter alone does not close the popup.
    /// </summary>
    private protected override void NotifyInputSubmit()
    {
        if (_pickerPresenter?.InputText is { Length: 0 })
        {
            Clear();
        }
        else
        {
            _pickerPresenter?.ConfirmCandidate();
        }
    }

    /// <summary>
    /// Reverts the edit in the popup at once: the typed text and the candidate are dropped and the popup
    /// shows the selected time of the active endpoint again, so a popup that stays open because it is pinned
    /// shows the reverted value in its input too.
    /// </summary>
    private protected override void NotifyEditCanceled() => _pickerPresenter?.ResetOpenPanelState();

    /// <summary>
    /// Calls the base implementation, then subscribes to text changes of both input boxes. A typed text
    /// activates its endpoint, opens the popup and becomes the candidate of that endpoint in the popup.
    /// </summary>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        AttachInputHandlers();
    }

    /// <summary>
    /// Unsubscribes from the text changes of the input boxes, then calls the base implementation.
    /// </summary>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        ReleaseInputHandlers();
        base.OnDetachedFromVisualTree(e);
    }

    /// <summary>
    /// Calls the base implementation and subscribes again to text changes of the input boxes of the new
    /// template. It then sets a clock icon as the info icon when none is set, and updates both input texts and
    /// the preferred width from the current values.
    /// </summary>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        ReleaseInputHandlers();
        base.OnApplyTemplate(e);
        AttachInputHandlers();
        if (InfoIcon is null)
        {
            SetValue(InfoIconProperty, new ClockCircleOutlined(), BindingPriority.Template);
        }
        RefreshRangeTexts();
        CalculatePreferredWidth();
    }
    
    #region 实现 FormItem 接口
    /// <summary>
    /// Applies a value passed in by the owning form item. The value is a tuple of the start time and the end
    /// time, <c>(TimeSpan?, TimeSpan?)</c>, whose items become the selected times; <see langword="null" /> or a
    /// value of another type leaves the selection unchanged.
    /// </summary>
    /// <param name="value">
    /// The value supplied by the form item.
    /// </param>
    protected override void NotifySetFormValue(object? value)
    {
        var rangeValue = value as (TimeSpan?, TimeSpan?)?;
        if (rangeValue != null)
        {
            RangeStartSelectedTime = rangeValue.Value.Item1;
            RangeEndSelectedTime   = rangeValue.Value.Item2;
        }
    }

    /// <summary>
    /// Returns the value of the picker for the owning form item: a boxed tuple of the selected start time and
    /// end time, or <see langword="null" /> when either is <see langword="null" />.
    /// </summary>
    /// <returns>
    /// A boxed <c>(TimeSpan?, TimeSpan?)</c> tuple, or <see langword="null" /> when the range is incomplete.
    /// </returns>
    protected override object? NotifyGetFormValue()
    {
        if (RangeStartSelectedTime is null || RangeEndSelectedTime is null)
        {
            return null;
        }
        return (RangeStartSelectedTime, RangeEndSelectedTime);
    }

    /// <summary>
    /// Clears the value of the picker at the request of the owning form item: sets both selected times to
    /// <see langword="null" />. Unlike <see cref="Clear" />, it neither ends an edit in the popup nor closes the
    /// popup.
    /// </summary>
    protected override void NotifyClearFormValue()
    {
        RangeStartSelectedTime = null;
        RangeEndSelectedTime   = null;
    }

    private void HandleSelectedValueChanged(AvaloniaPropertyChangedEventArgs args)
    {
        NotifyFormValueChanged((RangeStartSelectedTime, RangeEndSelectedTime));
    }
    #endregion
}
