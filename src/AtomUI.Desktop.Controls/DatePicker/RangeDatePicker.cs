using AtomUI.Desktop.Controls.Primitives;
using AtomUI.Icons.AntDesign;
using AtomUI.Localization;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Two-part input for choosing a range of dates, or of dates with a time, in a calendar popup. Each endpoint is
/// shown in its own input box and can also be typed. The endpoints of a range committed from the popup are
/// ordered by date before they are written to <see cref="RangeStartSelectedDate" /> and
/// <see cref="RangeEndSelectedDate" />.
/// </summary>
public partial class RangeDatePicker : RangeInfoPickerInput
{
    #region 公共属性定义
    
    /// <summary>
    /// Identifies the <see cref="RangeStartSelectedDate" /> property.
    /// </summary>
    public static readonly StyledProperty<DateTime?> RangeStartSelectedDateProperty =
        AvaloniaProperty.Register<RangeDatePicker, DateTime?>(nameof(RangeStartSelectedDate),
            defaultBindingMode: BindingMode.TwoWay,
            enableDataValidation: true);
    
    /// <summary>
    /// Identifies the <see cref="RangeEndSelectedDate" /> property.
    /// </summary>
    public static readonly StyledProperty<DateTime?> RangeEndSelectedDateProperty =
        AvaloniaProperty.Register<RangeDatePicker, DateTime?>(nameof(RangeEndSelectedDate),
            defaultBindingMode: BindingMode.TwoWay,
            enableDataValidation: true);
    
    /// <summary>
    /// Identifies the <see cref="IsNeedConfirm" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsNeedConfirmProperty =
        AvaloniaProperty.Register<RangeDatePicker, bool>(nameof(IsNeedConfirm));
    
    /// <summary>
    /// Identifies the <see cref="IsShowNow" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsShowNowProperty =
        DatePicker.IsShowNowProperty.AddOwner<RangeDatePicker>();
    
    /// <summary>
    /// Identifies the <see cref="IsShowTime" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsShowTimeProperty =
        DatePicker.IsShowTimeProperty.AddOwner<RangeDatePicker>();

    /// <summary>
    /// Identifies the <see cref="ClockIdentifier" /> property.
    /// </summary>
    public static readonly StyledProperty<ClockIdentifierType> ClockIdentifierProperty =
        DatePicker.ClockIdentifierProperty.AddOwner<RangeDatePicker>();

    /// <summary>
    /// Identifies the <see cref="Format" /> property.
    /// </summary>
    public static readonly StyledProperty<string?> FormatProperty =
        DatePicker.FormatProperty.AddOwner<RangeDatePicker>();

    /// <summary>
    /// Identifies the <see cref="PickerMode" /> property.
    /// </summary>
    public static readonly StyledProperty<DatePickerMode> PickerModeProperty =
        DatePicker.PickerModeProperty.AddOwner<RangeDatePicker>();

    /// <summary>
    /// Identifies the <see cref="PickerDisplayDate" /> property.
    /// </summary>
    public static readonly StyledProperty<DateTime?> PickerDisplayDateProperty =
        DatePicker.PickerDisplayDateProperty.AddOwner<RangeDatePicker>();

    /// <summary>
    /// Identifies the <see cref="MinDate" /> property.
    /// </summary>
    public static readonly StyledProperty<DateTime?> MinDateProperty =
        DatePicker.MinDateProperty.AddOwner<RangeDatePicker>();

    /// <summary>
    /// Identifies the <see cref="MaxDate" /> property.
    /// </summary>
    public static readonly StyledProperty<DateTime?> MaxDateProperty =
        DatePicker.MaxDateProperty.AddOwner<RangeDatePicker>();
    
    /// <summary>
    /// Gets or sets the start of the selected range, or <see langword="null" /> while no start is chosen. The
    /// first input box shows it formatted for display. Bindings to it are two-way by default.
    /// </summary>
    public DateTime? RangeStartSelectedDate
    {
        get => GetValue(RangeStartSelectedDateProperty);
        set => SetValue(RangeStartSelectedDateProperty, value);
    }

    /// <summary>
    /// Gets or sets the end of the selected range, or <see langword="null" /> while no end is chosen. The
    /// second input box shows it formatted for display. Bindings to it are two-way by default.
    /// </summary>
    public DateTime? RangeEndSelectedDate
    {
        get => GetValue(RangeEndSelectedDateProperty);
        set => SetValue(RangeEndSelectedDateProperty, value);
    }

    /// <summary>
    /// Gets or sets the date and time that <see cref="Reset" /> restores for the start of the range. It is also
    /// assigned to <see cref="RangeStartSelectedDate" /> when the control attaches to the logical tree and no
    /// start is selected. <see cref="Clear" /> does not apply it.
    /// </summary>
    public DateTime? RangeStartDefaultDate { get; set; }

    /// <summary>
    /// Gets or sets the date and time that <see cref="Reset" /> restores for the end of the range. It is also
    /// assigned to <see cref="RangeEndSelectedDate" /> when the control attaches to the logical tree and no end
    /// is selected. <see cref="Clear" /> does not apply it.
    /// </summary>
    public DateTime? RangeEndDefaultDate { get; set; }
    
    /// <summary>
    /// Gets or sets whether endpoints picked in the panel become the value only after the user presses the confirm
    /// button. The popup shows that button also when <see cref="IsShowTime" /> is <see langword="true" /> in
    /// <see cref="DatePickerMode.Date" /> mode, and the endpoints then need confirmation regardless of this property.
    /// Each endpoint is written as soon as it is confirmed, and a typed endpoint that is not confirmed is discarded
    /// when the popup closes. Without the confirm button, a picked endpoint is committed at once, and a typed endpoint
    /// that was accepted is kept when the popup closes unless the edit is canceled.
    /// </summary>
    public bool IsNeedConfirm
    {
        get => GetValue(IsNeedConfirmProperty);
        set => SetValue(IsNeedConfirmProperty, value);
    }
    
    /// <summary>
    /// Gets or sets whether the popup shows the Today button, or the Now button when the time selection is visible.
    /// The button appears only in <see cref="DatePickerMode.Date" /> mode.
    /// </summary>
    public bool IsShowNow
    {
        get => GetValue(IsShowNowProperty);
        set => SetValue(IsShowNowProperty, value);
    }
    
    /// <summary>
    /// Gets or sets whether the user also selects a time for each endpoint. The time selection is shown only in
    /// <see cref="DatePickerMode.Date" /> mode, and the default input format then includes the time.
    /// </summary>
    public bool IsShowTime
    {
        get => GetValue(IsShowTimeProperty);
        set => SetValue(IsShowTimeProperty, value);
    }
    
    /// <summary>
    /// Gets or sets whether the time uses a 12-hour or a 24-hour clock. It selects the default time format and the
    /// AM/PM designators, so it has an effect only when the input text shows a time.
    /// </summary>
    public ClockIdentifierType ClockIdentifier
    {
        get => GetValue(ClockIdentifierProperty);
        set => SetValue(ClockIdentifierProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the .NET date and time format string used to show both endpoints in the input boxes and to
    /// parse typed text, which must match the displayed format in full. <see langword="null" /> selects a
    /// built-in format for the <see cref="PickerMode" />. In <see cref="DatePickerMode.Date" /> mode it is
    /// <c>yyyy-MM-dd</c>, plus the time when <see cref="IsShowTime" /> is <see langword="true" />. A string that
    /// is not <see langword="null" /> is used as is.
    /// </summary>
    public string? Format
    {
        get => GetValue(FormatProperty);
        set => SetValue(FormatProperty, value);
    }

    /// <summary>
    /// Gets or sets the unit of the selectable value: a day, a week, a month, a quarter or a year. The mode selects
    /// the panel layout and, when <see cref="Format" /> is <see langword="null" />, the format of the input text.
    /// </summary>
    public DatePickerMode PickerMode
    {
        get => GetValue(PickerModeProperty);
        set => SetValue(PickerModeProperty, value);
    }

    /// <summary>
    /// Gets or sets the date the calendar panel is positioned on when the popup opens.
    /// </summary>
    public DateTime? PickerDisplayDate
    {
        get => GetValue(PickerDisplayDateProperty);
        set => SetValue(PickerDisplayDateProperty, value);
    }

    /// <summary>
    /// Gets or sets the earliest selectable date for both endpoints; the boundary date itself can be selected. The
    /// bound is normalized to the unit of the <see cref="PickerMode" />. <see langword="null" /> leaves the
    /// range open on this side, and a value later than <see cref="MaxDate" /> narrows the selectable range to
    /// this single date.
    /// </summary>
    public DateTime? MinDate
    {
        get => GetValue(MinDateProperty);
        set => SetValue(MinDateProperty, value);
    }

    /// <summary>
    /// Gets or sets the latest selectable date for both endpoints; the boundary date itself can be selected. The
    /// bound is normalized to the unit of the <see cref="PickerMode" />. <see langword="null" /> leaves the
    /// range open on this side, and an earlier <see cref="MinDate" /> narrows the selectable range to that date.
    /// </summary>
    public DateTime? MaxDate
    {
        get => GetValue(MaxDateProperty);
        set => SetValue(MaxDateProperty, value);
    }
    
    #endregion
    
    #region 内部属性定义
    
    internal static readonly DirectProperty<RangeDatePicker, double> PreferredWidthProperty =
        AvaloniaProperty.RegisterDirect<RangeDatePicker, double>(nameof(PreferredWidth),
            o => o.PreferredWidth,
            (o, v) => o.PreferredWidth = v);

    internal static readonly DirectProperty<RangeDatePicker, string?> AmTextProperty =
        AvaloniaProperty.RegisterDirect<RangeDatePicker, string?>(nameof(AmText),
            o => o.AmText,
            (o, v) => o.AmText = v);

    internal static readonly DirectProperty<RangeDatePicker, string?> PmTextProperty =
        AvaloniaProperty.RegisterDirect<RangeDatePicker, string?>(nameof(PmText),
            o => o.PmText,
            (o, v) => o.PmText = v);
    
    internal static readonly DirectProperty<RangeDatePicker, double> RangePickerIndicatorOffsetStartProperty =
        AvaloniaProperty.RegisterDirect<RangeDatePicker, double>(nameof(RangePickerIndicatorOffsetStart),
            o => o.RangePickerIndicatorOffsetStart,
            (o, v) => o.RangePickerIndicatorOffsetStart = v);
    
    internal static readonly DirectProperty<RangeDatePicker, double> RangePickerIndicatorOffsetEndProperty =
        AvaloniaProperty.RegisterDirect<RangeDatePicker, double>(nameof(RangePickerIndicatorOffsetEnd),
            o => o.RangePickerIndicatorOffsetEnd,
            (o, v) => o.RangePickerIndicatorOffsetEnd = v);
    
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
    
    private double _rangePickerIndicatorStart;

    internal double RangePickerIndicatorOffsetStart
    {
        get => _rangePickerIndicatorStart;
        set => SetAndRaise(RangePickerIndicatorOffsetStartProperty, ref _rangePickerIndicatorStart, value);
    }
    
    private double _rangePickerIndicatorEnd;

    internal double RangePickerIndicatorOffsetEnd
    {
        get => _rangePickerIndicatorEnd;
        set => SetAndRaise(RangePickerIndicatorOffsetEndProperty, ref _rangePickerIndicatorEnd, value);
    }

    #endregion

    private RangeDatePickerPresenter? _pickerPresenter;
    private TextBox? _attachedInputBox;
    private TextBox? _attachedSecondaryInputBox;

    // Text typed into the activated part in this open session, or null while both inputs show formatted values.
    private string? _typedText;
    private bool _isTypedTextAccepted;
    private bool _isApplyingTypedText;
    private ILanguageManager? _languageManager;

    /// <summary>
    /// Calls the base method, then subscribes to language changes of the application and refreshes both input texts.
    /// </summary>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _languageManager = Application.Current is { } app ? global::AtomUI.ApplicationExtensions.GetLanguageManager(app) : null;
        if (_languageManager is not null)
        {
            _languageManager.LanguageChanged += OnLanguageChanged;
        }

        RefreshRangeTexts();
    }

    /// <summary>
    /// Unsubscribes from language changes and calls the base method.
    /// </summary>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        if (_languageManager is not null)
        {
            _languageManager.LanguageChanged -= OnLanguageChanged;
        }

        _languageManager = null;
        base.OnDetachedFromVisualTree(e);
    }

    private void OnLanguageChanged(object? sender, LanguageChangedEventArgs e)
    {
        RefreshRangeTexts();
        CalculatePreferredWidth();
    }

    public RangeDatePicker()
    {
    }

    static RangeDatePicker()
    {
        AffectsMeasure<RangeDatePicker>(PreferredWidthProperty);
        RangeStartSelectedDateProperty.Changed.AddClassHandler<RangeDatePicker>((picker, args) => picker.HandleSelectedValueChanged(args));
        RangeEndSelectedDateProperty.Changed.AddClassHandler<RangeDatePicker>((picker, args) => picker.HandleSelectedValueChanged(args));
    }
    
    /// <summary>
    /// Creates the <c>RangeDatePickerPresenter</c> shown in the popup and binds its endpoints and picker options
    /// two-way to the properties of this control.
    /// </summary>
    protected override Control CreatePickerPresenter()
    {
        var presenter = new RangeDatePickerPresenter();
        presenter[!RangeDatePickerPresenter.IsMotionEnabledProperty]           = this[!IsMotionEnabledProperty];
        presenter[!RangeDatePickerPresenter.SelectedDateTimeProperty]          = this[!RangeStartSelectedDateProperty];
        presenter[!RangeDatePickerPresenter.SecondarySelectedDateTimeProperty] = this[!RangeEndSelectedDateProperty];
        presenter[!RangeDatePickerPresenter.ClockIdentifierProperty]           = this[!ClockIdentifierProperty];
        presenter[!RangeDatePickerPresenter.IsNeedConfirmProperty]             = this[!IsNeedConfirmProperty];
        presenter[!RangeDatePickerPresenter.IsShowNowProperty]                 = this[!IsShowNowProperty];
        presenter[!RangeDatePickerPresenter.IsShowTimeProperty]                = this[!IsShowTimeProperty];
        presenter[!RangeDatePickerPresenter.PickerModeProperty]                = this[!PickerModeProperty];
        presenter[!RangeDatePickerPresenter.PickerDisplayDateProperty]         = this[!PickerDisplayDateProperty];
        presenter[!RangeDatePickerPresenter.MinDateProperty]                   = this[!MinDateProperty];
        presenter[!RangeDatePickerPresenter.MaxDateProperty]                   = this[!MaxDateProperty];

        return presenter;
    }

    /// <summary>
    /// Clears both endpoints and the input texts, drops any typed draft and closes the popup.
    /// <see cref="RangeStartDefaultDate" /> and <see cref="RangeEndDefaultDate" /> are not applied.
    /// </summary>
    public override void Clear()
    {
        // The values and the picker texts are cleared before the input boxes, so clearing the boxes is not taken for
        // typing; the draft is dropped and the popup closes, as in Ant Design.
        ResetTypedText();
        RangeStartSelectedDate = null;
        RangeEndSelectedDate   = null;
        RefreshRangeTexts();
        base.Clear();
        ClosePickerFlyout();
    }

    /// <summary>
    /// Assigns <see cref="RangeStartDefaultDate" /> and <see cref="RangeEndDefaultDate" /> to the two endpoints. A
    /// default of <see langword="null" /> clears its endpoint.
    /// </summary>
    public void Reset()
    {
        RangeStartSelectedDate = RangeStartDefaultDate;
        RangeEndSelectedDate   = RangeEndDefaultDate;
    }

    /// <summary>
    /// Calls the base method, keeps the presenter created for the popup as the range presenter of the open
    /// session and enables its repair of a reversed range.
    /// </summary>
    protected override void NotifyPickerPresenterCreated(Control pickerPresenter)
    {
        base.NotifyPickerPresenterCreated(pickerPresenter);
        _pickerPresenter = pickerPresenter as RangeDatePickerPresenter;
        _pickerPresenter?.NotifyRepairReverseRange(true);
    }

    /// <summary>
    /// Calls the base method and drops the stored range presenter.
    /// </summary>
    protected override void NotifyPickerPresenterCleared(Control pickerPresenter)
    {
        base.NotifyPickerPresenterCleared(pickerPresenter);
        _pickerPresenter = null;
    }

    /// <summary>
    /// Calls the base method, then starts a popup session on the presenter. When no part is activated, the start
    /// part is activated if no start is selected, else the end part if no end is selected, else the start part.
    /// The presenter receives the selected endpoints, a reset pick and panel state, and this control subscribes
    /// to its events.
    /// </summary>
    protected override void NotifyPickerOpened()
    {
        base.NotifyPickerOpened();
        if (_pickerPresenter is not null)
        {
            if (RangeActivatedPart == RangeActivatedPart.None)
            {
                RangeActivatedPart = RangeStartSelectedDate is null
                    ? RangeActivatedPart.Start
                    : RangeEndSelectedDate is null ? RangeActivatedPart.End : RangeActivatedPart.Start;
            }

            _pickerPresenter.ChoosingStatusChanged += HandleChoosingStatusChanged;
            _pickerPresenter.HoverDateTimeChanged  += HandleHoverDateTimeChanged;
            _pickerPresenter.Confirmed             += HandleConfirmed;
            _pickerPresenter.RangePartConfirmed    += HandleRangePartConfirmed;
            _pickerPresenter.PropertyChanged       += HandlePresenterPropertyChanged;

            if (RangeActivatedPart == RangeActivatedPart.Start)
            {
                _pickerPresenter.NotifySelectRangeStart(true);
            }
            else
            {
                _pickerPresenter.NotifySelectRangeStart(false);
            }

            _pickerPresenter.SelectedDateTime          = RangeStartSelectedDate;
            _pickerPresenter.SecondarySelectedDateTime = RangeEndSelectedDate;
            _pickerPresenter.ResetRangePickState();
            _pickerPresenter.ResetRangeOpenPanelState();
        }
    }

    /// <summary>
    /// Calls the base method, then unsubscribes from the events of the presenter and restores the committed range
    /// in it. It leaves the choosing state, updates the pseudo-classes and refreshes the input texts and the
    /// preferred width.
    /// </summary>
    protected override void NotifyPickerClosed()
    {
        base.NotifyPickerClosed();
        if (_pickerPresenter is not null)
        {
            _pickerPresenter.ChoosingStatusChanged -= HandleChoosingStatusChanged;
            _pickerPresenter.HoverDateTimeChanged  -= HandleHoverDateTimeChanged;
            _pickerPresenter.Confirmed             -= HandleConfirmed;
            _pickerPresenter.RangePartConfirmed    -= HandleRangePartConfirmed;
            _pickerPresenter.PropertyChanged       -= HandlePresenterPropertyChanged;
            _pickerPresenter.RestoreCommittedRangeAfterClose(
                new DateViewerRange(RangeStartSelectedDate, RangeEndSelectedDate));
        }
        IsChoosing = false;
        UpdatePseudoClasses();
        RefreshRangeTexts();
        CalculatePreferredWidth();
    }

    /// <summary>
    /// Called by the base class before the popup flyout closes. Unless it is a lifecycle close, it commits the
    /// range ordered by date, taking over a typed endpoint only when the text was accepted, the edit is not being
    /// canceled and the presenter requires no confirmation. It then drops the typed draft and calls the base
    /// method, which clears the activated part.
    /// </summary>
    protected override void NotifyFlyoutAboutToClose(bool selectedIsValid)
    {
        // The base clears the activated part here, so a typed endpoint is settled while its part is still known:
        // kept when no confirmation is required, otherwise discarded with the rest of the draft on close. A lifecycle
        // close (detach, template change) writes nothing back.
        if (!IsLifecycleClosing)
        {
            var rangeStart = RangeStartSelectedDate;
            var rangeEnd   = RangeEndSelectedDate;
            if (_typedText is not null && _isTypedTextAccepted && !IsCancelingEdit &&
                _pickerPresenter is { RequiresConfirmation: false })
            {
                if (RangeActivatedPart == RangeActivatedPart.Start)
                {
                    rangeStart = _pickerPresenter.SelectedDateTime;
                }
                else if (RangeActivatedPart == RangeActivatedPart.End)
                {
                    rangeEnd = _pickerPresenter.SecondarySelectedDateTime;
                }
            }

            // A partial confirmation writes its endpoint at once and may pass the other one, so the range is ordered
            // when the session ends, as the final confirmation does.
            CommitRange(rangeStart, rangeEnd);
        }

        ResetTypedText();
        base.NotifyFlyoutAboutToClose(selectedIsValid);
    }

    private void ResetTypedText()
    {
        _typedText           = null;
        _isTypedTextAccepted = false;
    }

    private void HandlePresenterPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs args)
    {
        // A candidate chosen in the panel replaces the typed text.
        if ((args.Property == DatePickerPresenter.SelectedDateTimeProperty ||
             args.Property == RangeDatePickerPresenter.SecondarySelectedDateTimeProperty) && !_isApplyingTypedText)
        {
            ResetTypedText();
            // A hover preview keeps the inputs until it ends; otherwise they show the new candidate at once.
            if (!IsPreviewing)
            {
                ClearHoverSelectedInfo();
            }
        }
    }

    private void AttachInputHandlers()
    {
        if (_attachedInputBox is not null)
        {
            _attachedInputBox.PropertyChanged -= HandleInputTextChanged;
        }
        if (_attachedSecondaryInputBox is not null)
        {
            _attachedSecondaryInputBox.PropertyChanged -= HandleInputTextChanged;
        }
        _attachedInputBox          = InfoInputBox;
        _attachedSecondaryInputBox = SecondaryInfoInputBox;
        if (_attachedInputBox is not null)
        {
            _attachedInputBox.PropertyChanged += HandleInputTextChanged;
        }
        if (_attachedSecondaryInputBox is not null)
        {
            _attachedSecondaryInputBox.PropertyChanged += HandleInputTextChanged;
        }
    }

    private void HandleInputTextChanged(object? sender, AvaloniaPropertyChangedEventArgs args)
    {
        if (args.Property != TextBox.TextProperty || sender is not TextBox input || IsReadOnly)
        {
            return;
        }

        var isEnd = ReferenceEquals(input, SecondaryInfoInputBox);
        var text  = input.Text ?? string.Empty;
        if (string.Equals(text, (isEnd ? SecondaryText : Text) ?? string.Empty, StringComparison.Ordinal))
        {
            return;
        }

        // Typing opens the popup and previews a valid value in the panel, as in Ant Design.
        RangeActivatedPart = isEnd ? RangeActivatedPart.End : RangeActivatedPart.Start;
        SetPickerOpenIfChanged(true);
        _typedText = text;
        SetCurrentValue(isEnd ? SecondaryTextProperty : TextProperty, text);
        _isTypedTextAccepted = TryParseText(text, out var value) && ApplyTypedValue(value);
    }

    private bool ApplyTypedValue(DateTime value)
    {
        if (_pickerPresenter is null)
        {
            return false;
        }

        _isApplyingTypedText = true;
        try
        {
            return _pickerPresenter.InputDate(value);
        }
        finally
        {
            _isApplyingTypedText = false;
        }
    }

    private bool TryParseText(string text, out DateTime value)
    {
        var formatInfo = DatePickerFormattingHelper.CreateFormatInfo(ClockIdentifier, AmText, PmText);
        return DatePickerFormattingHelper.TryParseDateTime(text, Format, PickerMode, IsShowTime, ClockIdentifier,
            formatInfo, out value);
    }

    /// <summary>
    /// Returns <see langword="true" /> while the activated input box shows text typed in this open session rather
    /// than a formatted value. The base class then leaves navigation keys to the input instead of passing them to
    /// <see cref="NotifyNavigationKey" />.
    /// </summary>
    private protected override bool HasPendingInputText => _typedText is not null;

    /// <summary>
    /// Handles Enter while the popup is open. When the popup has no presenter, it closes the popup. A typed endpoint
    /// that was not accepted keeps the popup open and is not submitted. Otherwise the candidate is submitted from the
    /// keyboard, which needs no confirm button. When the input box shows a formatted value and the keyboard has moved
    /// the panel focus, Enter activates the focused cell instead. The popup closes when the submission commits
    /// nothing.
    /// </summary>
    private protected override void NotifyInputSubmit()
    {
        if (_pickerPresenter is null)
        {
            ClosePickerFlyout();
            return;
        }

        // An empty or invalid typed endpoint keeps the popup open; Enter on the start part hands over to the end
        // part until both are confirmed, then commits and closes.
        if (_typedText is not null && !_isTypedTextAccepted)
        {
            return;
        }

        if (_typedText is null && _pickerPresenter.ActivateKeyboardFocus())
        {
            return;
        }

        if (_pickerPresenter.SubmitCandidate() == DatePickerCommitKind.None)
        {
            ClosePickerFlyout();
        }
    }

    /// <summary>
    /// Forwards a key pressed without modifiers to the panel of the popup. Returns <see langword="true" /> when the
    /// panel consumed it for keyboard navigation, even when its focus did not move; keys pressed with a modifier and
    /// keys the panel does not use are left to the input.
    /// </summary>
    private protected override bool NotifyNavigationKey(Key key, KeyModifiers modifiers) =>
        modifiers == KeyModifiers.None && _pickerPresenter?.NavigateByKey(key) == true;

    /// <summary>
    /// Handles Tab in an input box while the popup is open and text is being typed. It submits the typed endpoint from
    /// the keyboard, moves focus to the other input box and returns <see langword="true" /> only when Tab leads from
    /// the start part to the end part, or with Shift from the end part to the start part. Otherwise it returns
    /// <see langword="false" /> and leaves focus to the input: typed text that was not accepted is dropped, and
    /// accepted text that is left behind is settled when the popup closes.
    /// </summary>
    /// <param name="isBackward">
    /// <see langword="true" /> when Shift is held.
    /// </param>
    private protected override bool NotifyInputTab(bool isBackward)
    {
        if (!IsPickerOpen || _pickerPresenter is null || _typedText is null)
        {
            return false;
        }

        var leavesToOtherPart = isBackward
            ? RangeActivatedPart == RangeActivatedPart.End
            : RangeActivatedPart == RangeActivatedPart.Start;
        if (!_isTypedTextAccepted)
        {
            // Invalid text is dropped when focus moves on.
            ResetTypedText();
            ClearHoverSelectedInfo();
            return false;
        }

        if (!leavesToOtherPart)
        {
            // Focus moves on past the range; a valid typed endpoint is settled like any focus loss when it closes.
            return false;
        }

        // Tab submits the typed endpoint weakly: the editor hands over to the other part and focus follows it.
        var target = isBackward ? InfoInputBox : SecondaryInfoInputBox;
        _pickerPresenter.SubmitCandidate();
        target?.Focus(NavigationMethod.Tab);
        return true;
    }

    /// <summary>
    /// Called when the edit of the open session is canceled, for example by Escape. The presenter reverts its
    /// candidate to the committed range, the typed text is forgotten and both input boxes show the committed
    /// endpoints again at once, so that nothing is left over when the popup is pinned open.
    /// </summary>
    private protected override void NotifyEditCanceled()
    {
        _pickerPresenter?.CancelEdit();
        // A pinned popup stays open, so the inputs return to the draft here rather than on close.
        ResetTypedText();
        ClearHoverSelectedInfo();
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
        SetPreviewing(false);
        Text          = FormatDateTime(_pickerPresenter?.SelectedDateTime ?? RangeStartSelectedDate);
        SecondaryText = FormatDateTime(_pickerPresenter?.SecondarySelectedDateTime ?? RangeEndSelectedDate);
        if (_typedText is not null)
        {
            SetCurrentValue(RangeActivatedPart == RangeActivatedPart.End ? SecondaryTextProperty : TextProperty,
                _typedText);
        }
        CalculatePreferredWidth();
    }
    
    /// <summary>
    /// Formats an endpoint for its input box with <see cref="Format" />, <see cref="PickerMode" />,
    /// <see cref="IsShowTime" /> and <see cref="ClockIdentifier" />, or returns an empty string when it is
    /// <see langword="null" />.
    /// </summary>
    protected string FormatDateTime(DateTime? dateTime)
    {
        if (dateTime is null)
        {
            return string.Empty;
        }

        var formatInfo = DatePickerFormattingHelper.CreateFormatInfo(ClockIdentifier, AmText, PmText);
        return DatePickerFormattingHelper.FormatDateTime(dateTime.Value, Format, PickerMode, IsShowTime, ClockIdentifier, formatInfo);
    }
    
    private void HandleHoverDateTimeChanged(object? sender, DateSelectedEventArgs args)
    {
        if (args.Date.HasValue)
        {
            var previewStart = _pickerPresenter?.SelectedDateTime ?? RangeStartSelectedDate;
            var previewEnd   = _pickerPresenter?.SecondarySelectedDateTime ?? RangeEndSelectedDate;
            if (RangeActivatedPart == RangeActivatedPart.Start)
            {
                previewStart = args.Date;
            }
            else if (RangeActivatedPart == RangeActivatedPart.End)
            {
                previewEnd = args.Date;
            }
            (previewStart, previewEnd) = NormalizePreviewRange(previewStart, previewEnd);
            Text          = FormatDateTime(previewStart);
            SecondaryText = FormatDateTime(previewEnd);
            // Hovering the confirm button shows the candidate itself, which is not dimmed as a preview.
            var candidate = RangeActivatedPart == RangeActivatedPart.End
                ? _pickerPresenter?.SecondarySelectedDateTime
                : _pickerPresenter?.SelectedDateTime;
            SetPreviewing(args.Date != candidate);
            CalculatePreferredWidth();
        }
        else
        {
            // The preview ended, for example when the pointer left the cells after keyboard navigation.
            ClearHoverSelectedInfo();
        }
    }

    private static (DateTime? Start, DateTime? End) NormalizePreviewRange(DateTime? start, DateTime? end)
    {
        return start is not null && end is not null && end.Value.Date < start.Value.Date
            ? (end, start)
            : (start, end);
    }

    private void HandleRangePartConfirmed(object? sender, EventArgs args)
    {
        ResetTypedText();
        if (RangeActivatedPart == RangeActivatedPart.Start)
        {
            RangeStartSelectedDate = _pickerPresenter?.SelectedDateTime;
            RangeActivatedPart     = RangeActivatedPart.End;
        }
        else if (RangeActivatedPart == RangeActivatedPart.End)
        {
            RangeEndSelectedDate = _pickerPresenter?.SecondarySelectedDateTime;
            RangeActivatedPart   = RangeActivatedPart.Start;
        }
    }
    
    private void HandleConfirmed(object? sender, EventArgs args)
    {
        ResetTypedText();
        var rangeStart = _pickerPresenter?.SelectedDateTime;
        var rangeEnd   = _pickerPresenter?.SecondarySelectedDateTime;
        if (rangeStart is not null && rangeEnd is not null)
        {
            CommitRange(rangeStart, rangeEnd);
        }

        ClosePickerFlyout();
    }

    /// <summary>
    /// Writes both endpoints ordered by date; endpoints on the same day keep their clock order.
    /// </summary>
    private void CommitRange(DateTime? rangeStart, DateTime? rangeEnd)
    {
        if (rangeStart is not null && rangeEnd is not null && rangeEnd.Value.Date < rangeStart.Value.Date)
        {
            (rangeStart, rangeEnd) = (rangeEnd, rangeStart);
        }

        // Unchanged endpoints are not written, so a close does not replace their bindings with local values.
        if (RangeStartSelectedDate != rangeStart)
        {
            RangeStartSelectedDate = rangeStart;
        }

        if (RangeEndSelectedDate != rangeEnd)
        {
            RangeEndSelectedDate = rangeEnd;
        }
    }

    /// <summary>
    /// Calls the base method, then reacts to the changed property: a new activated part drops a typed draft and
    /// notifies the part change; a property that affects the formatted input text or the preferred width
    /// updates them; and while the control is attached to the visual tree a new endpoint updates its input text
    /// and the preferred width.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == RangeActivatedPartProperty)
        {
            // Typed text belongs to the part that was active; the inputs fall back to the draft values.
            if (_typedText is not null)
            {
                ResetTypedText();
                if (IsPickerOpen)
                {
                    ClearHoverSelectedInfo();
                }
            }
            NotifyRangeActivatedPartChanged();
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
            if (change.Property == RangeStartSelectedDateProperty)
            {
                Text = FormatDateTime(RangeStartSelectedDate);
                CalculatePreferredWidth();
            }
            else if (change.Property == RangeEndSelectedDateProperty)
            {
                SecondaryText = FormatDateTime(RangeEndSelectedDate);
                CalculatePreferredWidth();
            }
        }
    }

    private void RefreshRangeTexts()
    {
        Text          = FormatDateTime(RangeStartSelectedDate);
        SecondaryText = FormatDateTime(RangeEndSelectedDate);
    }

    private static bool IsFormattedTextAffectingProperty(AvaloniaProperty property)
    {
        return DatePickerFormattingHelper.IsFormattedTextAffectingProperty(
            property,
            IsShowTimeProperty,
            FormatProperty,
            PickerModeProperty,
            ClockIdentifierProperty,
            AmTextProperty,
            PmTextProperty);
    }

    private static bool IsPreferredWidthAffectingProperty(AvaloniaProperty property)
    {
        return DatePickerFormattingHelper.IsPreferredWidthAffectingProperty(
            property,
            FontSizeProperty,
            FontFamilyProperty,
            FontStyleProperty,
            FontWeightProperty,
            SizeTypeProperty,
            MinWidthProperty,
            WidthProperty,
            MaxWidthProperty,
            HorizontalAlignmentProperty);
    }
    
    private void CalculatePreferredWidth()
    {
        // The reserved width of each input box always follows a content baseline shared by the placeholder and the
        // selected value. Under an explicit Width or Stretch the input area therefore keeps its width while the text
        // changes, and the total control width is left to the outer layout: PreferredWidth is set to 0, which turns
        // off the widening of the control.
        var formatInfo = DatePickerFormattingHelper.CreateFormatInfo(ClockIdentifier, AmText, PmText);
        var preferredInputWidth = DatePickerFormattingHelper.CalculateBoundedRangePreferredInputWidth(
            Format,
            PickerMode,
            IsShowTime,
            ClockIdentifier,
            FontSize,
            FontFamily,
            FontStyle,
            FontWeight,
            MinWidth,
            MaxWidth,
            formatInfo);
        PreferredInputWidth = preferredInputWidth;
        PreferredWidth      = (!double.IsNaN(Width) || HorizontalAlignment == HorizontalAlignment.Stretch)
            ? 0
            : preferredInputWidth;
    }
    
    /// <summary>
    /// Updates the range picker indicator for the new activated part and tells the presenter whether the start or
    /// the end is being chosen. The base method is not called.
    /// </summary>
    protected override void NotifyRangeActivatedPartChanged()
    {
        SetupPickerIndicatorPosition();
        if (RangeActivatedPart == RangeActivatedPart.Start)
        {
            _pickerPresenter?.NotifySelectRangeStart(true);
        }
        else if (RangeActivatedPart == RangeActivatedPart.End)
        {
            _pickerPresenter?.NotifySelectRangeStart(false);
        }
    }

    /// <summary>
    /// Arranges as the base method does and returns its size unchanged, then positions the range picker indicator
    /// under the activated input box, inset by a tenth of the box width on each side.
    /// </summary>
    protected override Size ArrangeOverride(Size finalSize)
    {
        var size = base.ArrangeOverride(finalSize);
        if (InfoInputBox is not null && SecondaryInfoInputBox != null)
        {
            Rect targetBounds = default;
            if (RangeActivatedPart == RangeActivatedPart.Start)
            {
                targetBounds = InfoInputBox.Bounds;
            }
            else if (RangeActivatedPart == RangeActivatedPart.End)
            {
                targetBounds = SecondaryInfoInputBox.Bounds;
            }

            var delta = targetBounds.Width * 0.1;
            RangePickerIndicatorOffsetStart = targetBounds.X + delta;
            RangePickerIndicatorOffsetEnd   = DesiredSize.Width - targetBounds.X - delta;
        }
        return size;
    }

    /// <summary>
    /// Returns <see langword="true" /> when the start or the end of the range is selected, so the clear button is
    /// also shown for a half-selected range.
    /// </summary>
    protected override bool ShowClearButtonPredicate()
    {
        return RangeStartSelectedDate is not null || RangeEndSelectedDate is not null;
    }
    
    /// <summary>
    /// Measures as the base method does, then widens the result, but never below the width of the base method, so
    /// that each input box can take <c>PreferredWidth</c> when it is positive.
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
    /// Calls the base method, then assigns <see cref="RangeStartDefaultDate" /> and
    /// <see cref="RangeEndDefaultDate" /> to the start and the end of the range while they are
    /// <see langword="null" />.
    /// </summary>
    protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnAttachedToLogicalTree(e);
        if (RangeStartDefaultDate is not null && RangeStartSelectedDate is null)
        {
            RangeStartSelectedDate = RangeStartDefaultDate;
        }
        
        if (RangeEndDefaultDate is not null && RangeEndSelectedDate is null)
        {
            RangeEndSelectedDate = RangeEndDefaultDate;
        }
    }

    /// <summary>
    /// Calls the base method, then subscribes to text changes of the two input boxes, falls back to a calendar icon
    /// when no info icon is set, and refreshes the input texts and the preferred width.
    /// </summary>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        AttachInputHandlers();
        if (InfoIcon is null)
        {
            SetValue(InfoIconProperty, new CalendarOutlined(), BindingPriority.Template);
        }
        RefreshRangeTexts();
        CalculatePreferredWidth();
    }
    
    #region 实现 FormItem 接口
    /// <summary>
    /// Assigns a value of type <c>(DateTime?, DateTime?)</c> to the start and the end of the range. A value of any
    /// other type, including <see langword="null" />, leaves the range unchanged.
    /// </summary>
    protected override void NotifySetFormValue(object? value)
    {
        var rangeValue = value as (DateTime?, DateTime?)?;
        if (rangeValue != null)
        {
            RangeStartSelectedDate = rangeValue.Value.Item1;
            RangeEndSelectedDate   = rangeValue.Value.Item2;
        }
    }

    /// <summary>
    /// Returns the start and the end of the range as a <c>(DateTime?, DateTime?)</c> tuple, or
    /// <see langword="null" /> when either endpoint is <see langword="null" />.
    /// </summary>
    protected override object? NotifyGetFormValue()
    {
        if (RangeStartSelectedDate == null || RangeEndSelectedDate == null)
        {
            return null;
        }
        return (RangeStartSelectedDate, RangeEndSelectedDate);
    }

    /// <summary>
    /// Clears the start and the end of the range.
    /// </summary>
    protected override void NotifyClearFormValue()
    {
        RangeStartSelectedDate = null;
        RangeEndSelectedDate   = null;
    }

    private void HandleSelectedValueChanged(AvaloniaPropertyChangedEventArgs args)
    {
        NotifyFormValueChanged((RangeStartSelectedDate, RangeEndSelectedDate));
    }
    #endregion
}
