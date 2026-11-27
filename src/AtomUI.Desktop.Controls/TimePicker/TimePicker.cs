using AtomUI.Controls.Utils;
using AtomUI.Desktop.Controls.Primitives;
using AtomUI.Icons.AntDesign;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.LogicalTree;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// The hour notation a time picker uses for its popup, its displayed text and the text it parses.
/// </summary>
public enum ClockIdentifierType
{
    /// <summary>
    /// The 12-hour notation, with an AM/PM column in the popup and an AM or PM marker in the text.
    /// </summary>
    HourClock12,
    /// <summary>
    /// The 24-hour notation. It is the default of <see cref="TimePicker.ClockIdentifier" />.
    /// </summary>
    HourClock24
}

/// <summary>
/// An input field for a single time of day. The user types the time or picks it in a popup with
/// hour, minute, second and AM/PM columns; the result is <see cref="SelectedTime" />.
/// </summary>
public partial class TimePicker : InfoPickerInput
{
    #region 公共属性定义

    /// <summary>
    /// Identifies the <see cref="IsNeedConfirm" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsNeedConfirmProperty =
        AvaloniaProperty.Register<TimePicker, bool>(nameof(IsNeedConfirm), true);

    /// <summary>
    /// Identifies the <see cref="IsChangeOnScroll" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsChangeOnScrollProperty =
        AvaloniaProperty.Register<TimePicker, bool>(nameof(IsChangeOnScroll));

    /// <summary>
    /// Identifies the <see cref="IsShowNow" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsShowNowProperty =
        AvaloniaProperty.Register<TimePicker, bool>(nameof(IsShowNow), true);

    /// <summary>
    /// Identifies the <see cref="MinuteIncrement" /> property.
    /// </summary>
    public static readonly StyledProperty<int> MinuteIncrementProperty =
        AvaloniaProperty.Register<TimePicker, int>(nameof(MinuteIncrement), 1, coerce: CoerceMinuteIncrement);

    /// <summary>
    /// Identifies the <see cref="SecondIncrement" /> property.
    /// </summary>
    public static readonly StyledProperty<int> SecondIncrementProperty =
        AvaloniaProperty.Register<TimePicker, int>(nameof(SecondIncrement), 1, coerce: CoerceSecondIncrement);

    /// <summary>
    /// Identifies the <see cref="ClockIdentifier" /> property.
    /// </summary>
    public static readonly StyledProperty<ClockIdentifierType> ClockIdentifierProperty =
        AvaloniaProperty.Register<TimePicker, ClockIdentifierType>(nameof(ClockIdentifier), ClockIdentifierType.HourClock24);

    /// <summary>
    /// Identifies the <see cref="SelectedTime" /> property.
    /// </summary>
    public static readonly StyledProperty<TimeSpan?> SelectedTimeProperty =
        AvaloniaProperty.Register<TimePicker, TimeSpan?>(nameof(SelectedTime),
            defaultBindingMode: BindingMode.TwoWay,
            enableDataValidation: true);

    /// <summary>
    /// Identifies the <see cref="DefaultTime" /> property.
    /// </summary>
    public static readonly StyledProperty<TimeSpan?> DefaultTimeProperty =
        AvaloniaProperty.Register<TimePicker, TimeSpan?>(nameof(DefaultTime),
            enableDataValidation: true);

    /// <summary>
    /// Identifies the <see cref="PickerDisplayTime" /> property.
    /// </summary>
    public static readonly StyledProperty<TimeSpan?> PickerDisplayTimeProperty =
        AvaloniaProperty.Register<TimePicker, TimeSpan?>(nameof(PickerDisplayTime));

    /// <summary>
    /// Gets or sets whether the popup has an OK button that commits the time it shows.
    /// When <see langword="true" />, the footer shows that button, and closing the popup leaves
    /// <see cref="SelectedTime" /> unchanged.
    /// When <see langword="false" />, there is no OK button, the Now button is centered, and closing
    /// the popup commits the time it shows unless the edit is canceled.
    /// The default is <see langword="true" />.
    /// </summary>
    public bool IsNeedConfirm
    {
        get => GetValue(IsNeedConfirmProperty);
        set => SetValue(IsNeedConfirmProperty, value);
    }

    /// <summary>
    /// Gets or sets whether turning the mouse wheel over a column of the popup changes the picked
    /// time. The default is <see langword="false" />, which keeps scrolling from changing it.
    /// </summary>
    public bool IsChangeOnScroll
    {
        get => GetValue(IsChangeOnScrollProperty);
        set => SetValue(IsChangeOnScrollProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the popup footer shows a Now button, which sets the picker to the
    /// current time with the minutes and seconds rounded down to <see cref="MinuteIncrement" />
    /// and <see cref="SecondIncrement" />. The default is <see langword="true" />.
    /// </summary>
    public bool IsShowNow
    {
        get => GetValue(IsShowNowProperty);
        set => SetValue(IsShowNowProperty, value);
    }

    /// <summary>
    /// Gets or sets the step, in minutes, between the minute options offered in the popup.
    /// The Now button also rounds the current minute down to a multiple of it. Valid values
    /// are 1 to 59; any other value throws an <see cref="ArgumentOutOfRangeException" />.
    /// The default is 1.
    /// </summary>
    public int MinuteIncrement
    {
        get => GetValue(MinuteIncrementProperty);
        set => SetValue(MinuteIncrementProperty, value);
    }

    /// <summary>
    /// Gets or sets the step, in seconds, between the second options offered in the popup.
    /// The Now button also rounds the current second down to a multiple of it. Valid values
    /// are 1 to 59; any other value throws an <see cref="ArgumentOutOfRangeException" />.
    /// The default is 1.
    /// </summary>
    public int SecondIncrement
    {
        get => GetValue(SecondIncrementProperty);
        set => SetValue(SecondIncrementProperty, value);
    }

    /// <summary>
    /// Gets or sets the hour notation of the popup, of the input text and of the parsing of typed
    /// text. The default is <see cref="ClockIdentifierType.HourClock24" />.
    /// </summary>
    public ClockIdentifierType ClockIdentifier
    {
        get => GetValue(ClockIdentifierProperty);
        set => SetValue(ClockIdentifierProperty, value);
    }

    /// <summary>
    /// Gets or sets the picked time of day, or <see langword="null" /> when no time is selected
    /// and the clear button is hidden. Setting it rewrites the input text with the value formatted
    /// for <see cref="ClockIdentifier" /> and notifies a hosting form of the new value. While the
    /// popup is open, the text follows the time shown in the popup. The popup writes the time
    /// it confirms here. The default is <see langword="null" />.
    /// </summary>
    public TimeSpan? SelectedTime
    {
        get => GetValue(SelectedTimeProperty);
        set => SetValue(SelectedTimeProperty, value);
    }

    /// <summary>
    /// Gets or sets the time that becomes selected when the picker joins the logical tree with no
    /// selected time, and that <see cref="Reset" /> restores. <see cref="Clear" /> does not apply it.
    /// The default is <see langword="null" />.
    /// </summary>
    public TimeSpan? DefaultTime
    {
        get => GetValue(DefaultTimeProperty);
        set => SetValue(DefaultTimeProperty, value);
    }

    /// <summary>
    /// Gets or sets the time shown in the popup when it opens without a selected time; a set
    /// <see cref="SelectedTime" /> takes precedence. When this is also <see langword="null" />, the
    /// popup shows 00:00. The default is <see langword="null" />.
    /// </summary>
    public TimeSpan? PickerDisplayTime
    {
        get => GetValue(PickerDisplayTimeProperty);
        set => SetValue(PickerDisplayTimeProperty, value);
    }

    #endregion

    #region 内部属性定义

    internal static readonly DirectProperty<TimePicker, string?> AmTextProperty =
        AvaloniaProperty.RegisterDirect<TimePicker, string?>(nameof(AmText),
            o => o.AmText,
            (o, v) => o.AmText = v);
    
    internal static readonly DirectProperty<TimePicker, string?> PmTextProperty =
        AvaloniaProperty.RegisterDirect<TimePicker, string?>(nameof(PmText),
            o => o.PmText,
            (o, v) => o.PmText = v);

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

    private const string AntDesignDefaultInputWidthReferenceText = "Select time";

    private TimePickerPresenter? _pickerPresenter;

    static TimePicker()
    {
        SelectedTimeProperty.Changed.AddClassHandler<TimePicker>((timePicker, args) => timePicker.NotifyFormValueChanged(args.NewValue));
    }

    public TimePicker()
    {
    }

    /// <summary>
    /// Called by the base class to create the control shown in the popup. Returns a new
    /// <see cref="TimePickerPresenter" /> that follows this picker's <see cref="IsChangeOnScroll" />,
    /// <see cref="MinuteIncrement" />, <see cref="SecondIncrement" />, <see cref="ClockIdentifier" />,
    /// <see cref="SelectedTime" />, <see cref="PickerDisplayTime" />, <see cref="IsNeedConfirm" />,
    /// <see cref="IsShowNow" /> and <c>IsMotionEnabled</c> through bindings.
    /// </summary>
    protected override Control CreatePickerPresenter()
    {
        // This presenter is created in an independent Content subtree. The owner's
        // ControlTheme cannot bind into its separate template owner.
        var timePickerPresenter = new TimePickerPresenter();
        timePickerPresenter[!TimePickerPresenter.IsChangeOnScrollProperty] = this[!IsChangeOnScrollProperty];
        timePickerPresenter[!TimePickerPresenter.IsMotionEnabledProperty]  = this[!IsMotionEnabledProperty];
        timePickerPresenter[!TimePickerPresenter.MinuteIncrementProperty]  = this[!MinuteIncrementProperty];
        timePickerPresenter[!TimePickerPresenter.SecondIncrementProperty]  = this[!SecondIncrementProperty];
        timePickerPresenter[!TimePickerPresenter.ClockIdentifierProperty]  = this[!ClockIdentifierProperty];
        timePickerPresenter[!TimePickerPresenter.SelectedTimeProperty]     = this[!SelectedTimeProperty];
        timePickerPresenter[!TimePickerPresenter.PickerDisplayTimeProperty] = this[!PickerDisplayTimeProperty];
        timePickerPresenter[!TimePickerPresenter.IsNeedConfirmProperty]    = this[!IsNeedConfirmProperty];
        timePickerPresenter[!TimePickerPresenter.IsShowNowProperty]        = this[!IsShowNowProperty];

        return timePickerPresenter;
    }

    /// <summary>
    /// Called by the base class with the popup presenter it created. After calling the base
    /// implementation, the presenter is stored as the <see cref="TimePickerPresenter" /> used while
    /// the popup is open.
    /// </summary>
    protected override void NotifyPickerPresenterCreated(Control pickerPresenter)
    {
        base.NotifyPickerPresenterCreated(pickerPresenter);
        _pickerPresenter = pickerPresenter as TimePickerPresenter;
    }

    /// <summary>
    /// Called by the base class when the popup presenter is cleared. After calling the base
    /// implementation, the picker drops its reference to the <see cref="TimePickerPresenter" />.
    /// </summary>
    protected override void NotifyPickerPresenterCleared(Control pickerPresenter)
    {
        base.NotifyPickerPresenterCleared(pickerPresenter);
        _pickerPresenter = null;
    }

    /// <summary>
    /// Called by the base class when the popup opens. After calling the base implementation, the
    /// picker subscribes to the <see cref="TimePickerPresenter" /> events that drive the preview text,
    /// the confirmation and the dismissal, and resets the presenter's panel state for the new opening.
    /// </summary>
    protected override void NotifyPickerOpened()
    {
        base.NotifyPickerOpened();
        if (_pickerPresenter is not null)
        {
            _pickerPresenter.ChoosingStatusChanged += HandleChoosingStatusChanged;
            _pickerPresenter.HoverTimeChanged      += HandleHoverTimeChanged;
            _pickerPresenter.Confirmed             += HandleConfirmed;
            _pickerPresenter.Dismissed += HandleDismissed;
            _pickerPresenter.CandidateChanged += HandleCandidateChanged;
            _pickerPresenter.FocusLeft += HandleFocusLeft;
            _pickerPresenter.ResetOpenPanelState();
        }
    }

    /// <summary>
    /// Called by the base class when the popup closes. After calling the base implementation, the
    /// picker unsubscribes from the <see cref="TimePickerPresenter" /> and ends its editing. When
    /// <see cref="IsNeedConfirm" /> is <see langword="false" /> and the edit is not canceled, the time
    /// the popup shows is committed as <see cref="SelectedTime" />. The input text is then rewritten
    /// from the selected time.
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
            var candidate = _pickerPresenter.EndEditing();
            if (!IsNeedConfirm && !IsCancelingEdit)
            {
                SetCurrentValue(SelectedTimeProperty, candidate);
            }
            Text = DateTimeUtils.FormatTimeSpan(SelectedTime,
                ClockIdentifier == ClockIdentifierType.HourClock12, AmText, PmText);
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
        SetPreviewing(IsPickerOpen && _pickerPresenter?.PreviewTime != null);
        var displayTime = IsPickerOpen ? _pickerPresenter?.DisplayTime : SelectedTime;
        Text = IsPickerOpen && _pickerPresenter?.PreviewTime == null && _pickerPresenter?.InputText != null
            ? _pickerPresenter.InputText
            : DateTimeUtils.FormatTimeSpan(displayTime,
                ClockIdentifier == ClockIdentifierType.HourClock12, AmText, PmText);
    }

    private void HandleCandidateChanged(object? sender, EventArgs args) => ClearHoverSelectedInfo();
    private void HandleHoverTimeChanged(object? sender, TimeSelectedEventArgs args) => ClearHoverSelectedInfo();
    // Escape inside the popup panel reverts like Escape in the input.
    private void HandleDismissed(object? sender, EventArgs args) => CancelPickerEdit();

    private void HandleConfirmed(object? sender, EventArgs args)
    {
        SetCurrentValue(SelectedTimeProperty, _pickerPresenter?.SelectedTime);
        ClosePickerFlyout();
        if (IsPickerOpen)
        {
            _pickerPresenter?.ResetOpenPanelState();
        }
    }

    /// <summary>
    /// Clears the selected time, ignoring <see cref="DefaultTime" />, and closes an open popup unless it is
    /// pinned open.
    /// </summary>
    public override void Clear()
    {
        SetCurrentValue(SelectedTimeProperty, null);
        base.Clear();
        ClosePickerFlyout();
    }

    /// <summary>
    /// Resets the selected time to <see cref="DefaultTime" />, which clears it when no default time
    /// is set.
    /// </summary>
    public void Reset()
    {
        SelectedTime = DefaultTime;
    }

    /// <summary>
    /// Whether a time is selected. The base class does not call this predicate; it drives the clear button
    /// through <see cref="InfoPickerInput.ConfigureIsClearButtonVisible" /> instead.
    /// </summary>
    /// <returns>
    /// <see langword="true" /> when a <see cref="SelectedTime" /> is set; otherwise <see langword="false" />.
    /// </returns>
    protected override bool ShowClearButtonPredicate()
    {
        return SelectedTime is not null;
    }

    /// <summary>
    /// After calling the base implementation, keeps the input text and the preferred width in step
    /// with the picker. A change of <see cref="SelectedTime" />, <see cref="ClockIdentifier" />,
    /// <see cref="AmText" /> or <see cref="PmText" /> rewrites the text from the selected time. A
    /// change of the font, the size type, the width limits or the horizontal alignment recalculates
    /// the preferred width.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == SelectedTimeProperty)
        {
            Text = DateTimeUtils.FormatTimeSpan(SelectedTime,
                ClockIdentifier == ClockIdentifierType.HourClock12, AmText, PmText);
            CalculatePreferredWidth();
        }
        else if (IsFormattedTextAffectingProperty(change.Property))
        {
            Text = DateTimeUtils.FormatTimeSpan(SelectedTime,
                ClockIdentifier == ClockIdentifierType.HourClock12, AmText, PmText);
            CalculatePreferredWidth();
        }
        else if (IsPreferredWidthAffectingProperty(change.Property))
        {
            CalculatePreferredWidth();
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

    private void CalculatePreferredWidth()
    {
        // The reserved input width always follows the content baseline: the placeholder and the selected value
        // share one width baseline, so the input area does not jump with the text when Width is explicit or the
        // control is stretched. The total control width is still decided by the outer layout in those cases.
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
    }

    private double CalculateContentPreferredWidth()
    {
        var formatWidth = DateTimeUtils.CalculateWidestFormattedTimeSpanSize(
            ClockIdentifier == ClockIdentifierType.HourClock12,
            AmText, PmText,
            FontSize, FontFamily, FontStyle, FontWeight).Width;
        var defaultInputBaselineWidth = DatePickerFormattingHelper.CalculateAntDesignInputBaselineWidth(
            FontSize, FontFamily, FontStyle, FontWeight, AntDesignDefaultInputWidthReferenceText);

        return Math.Max(formatWidth, defaultInputBaselineWidth);
    }

    /// <summary>
    /// After calling the base implementation, selects <see cref="DefaultTime" /> when a default time
    /// is set and no time is selected.
    /// </summary>
    protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnAttachedToLogicalTree(e);
        if (DefaultTime is not null && SelectedTime is null)
        {
            SelectedTime = DefaultTime;
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
    }

    private void AttachInputHandlers()
    {
        ReleaseInputHandlers();
        if (InfoInputBox != null)
        {
            InfoInputBox.PropertyChanged += HandleInputTextChanged;
        }
    }

    private void HandleInputTextChanged(object? sender, AvaloniaPropertyChangedEventArgs args)
    {
        if (args.Property != TextBox.TextProperty || sender is not TextBox input ||
            string.Equals(input.Text ?? string.Empty, Text ?? string.Empty, StringComparison.Ordinal))
        {
            return;
        }
        var text = input.Text;
        SetCurrentValue(IsPickerOpenProperty, true);
        var valid = TimePickerPresenter.TryParseInput(text, ClockIdentifier, AmText, PmText, out var time);
        _pickerPresenter?.SetInputCandidate(text, time, valid);
        SetCurrentValue(TextProperty, text);
    }

    /// <summary>
    /// Whether the popup still holds text forwarded from the input, including an empty string after the input
    /// was emptied; a confirmation, a cancel, a close or a reset of the popup drops it. While this is
    /// <see langword="true" />, the base class does not forward navigation keys to the popup.
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
    /// Enter in the open picker. When the user emptied the input in this open session, clears the selected
    /// time as <see cref="Clear" /> does; otherwise the popup confirms the time it shows, which makes it the
    /// <see cref="SelectedTime" /> and then closes the popup unless it is pinned open. It does nothing while
    /// the popup cannot confirm its candidate. The base implementation is not called, so Enter alone does not
    /// close the popup.
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
    /// shows the selected time again, so a popup that stays open because it is pinned shows the reverted
    /// value in the input too.
    /// </summary>
    private protected override void NotifyEditCanceled() => _pickerPresenter?.ResetOpenPanelState();

    /// <summary>
    /// After calling the base implementation, subscribes to text changes of the input box, so that
    /// the text the user types is forwarded to the popup as a time candidate.
    /// </summary>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        AttachInputHandlers();
    }

    /// <summary>
    /// Unsubscribes from the text changes of the input box, then calls the base implementation.
    /// </summary>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        ReleaseInputHandlers();
        base.OnDetachedFromVisualTree(e);
    }

    /// <summary>
    /// Rebinds the handlers for the input box's text changes to the new template: releases them,
    /// calls the base implementation and subscribes again. Then sets the info icon to a clock icon
    /// when it has no value, and recalculates the preferred input width.
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
        CalculatePreferredWidth();
    }
    
    #region 实现 FormItem 接口
    /// <summary>
    /// Called by the base class when a form sets the value of this field. A <see cref="TimeSpan" />
    /// becomes <see cref="SelectedTime" />; <see langword="null" /> or a value of another type clears it.
    /// </summary>
    protected override void NotifySetFormValue(object? value)
    {
        SelectedTime = value as TimeSpan?;
    }

    /// <summary>
    /// Called by the base class when a form reads the value of this field.
    /// </summary>
    /// <returns>
    /// The <see cref="SelectedTime" />, or <see langword="null" /> when no time is selected.
    /// </returns>
    protected override object? NotifyGetFormValue()
    {
        return SelectedTime;
    }

    /// <summary>
    /// Called by the base class when a form clears this field. Sets <see cref="SelectedTime" /> to
    /// <see langword="null" /> without applying <see cref="DefaultTime" />.
    /// </summary>
    protected override void NotifyClearFormValue()
    {
        SelectedTime = null;
    }
    #endregion
}
