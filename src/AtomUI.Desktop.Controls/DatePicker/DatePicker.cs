using AtomUI.Desktop.Controls.Primitives;
using AtomUI.Icons.AntDesign;
using AtomUI.Localization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// The unit of the value that a date picker lets the user select.
/// </summary>
public enum DatePickerMode
{
    /// <summary>
    /// Selects a day. This is the only mode in which the time selection and the Now and Today buttons appear.
    /// </summary>
    Date,
    /// <summary>
    /// Selects a week.
    /// </summary>
    Week,
    /// <summary>
    /// Selects a month.
    /// </summary>
    Month,
    /// <summary>
    /// Selects a quarter of a year.
    /// </summary>
    Quarter,
    /// <summary>
    /// Selects a year.
    /// </summary>
    Year
}

/// <summary>
/// An input that lets the user pick a single date, optionally with a time, in a calendar popup or by typing text
/// in the format given by <see cref="Format" />.
/// </summary>
public partial class DatePicker : InfoPickerInput
{
    #region 公共属性定义

    /// <summary>
    /// Identifies the <see cref="SelectedDateTime" /> property.
    /// </summary>
    public static readonly StyledProperty<DateTime?> SelectedDateTimeProperty =
        AvaloniaProperty.Register<DatePicker, DateTime?>(nameof(SelectedDateTime),
            defaultBindingMode: BindingMode.TwoWay,
            enableDataValidation: true);

    /// <summary>
    /// Identifies the <see cref="DefaultDateTime" /> property.
    /// </summary>
    public static readonly StyledProperty<DateTime?> DefaultDateTimeProperty =
        AvaloniaProperty.Register<DatePicker, DateTime?>(nameof(DefaultDateTime),
            enableDataValidation: true);

    /// <summary>
    /// Identifies the <see cref="PickerDisplayDate" /> property.
    /// </summary>
    public static readonly StyledProperty<DateTime?> PickerDisplayDateProperty =
        AvaloniaProperty.Register<DatePicker, DateTime?>(nameof(PickerDisplayDate));

    /// <summary>
    /// Identifies the <see cref="MinDate" /> property.
    /// </summary>
    public static readonly StyledProperty<DateTime?> MinDateProperty =
        AvaloniaProperty.Register<DatePicker, DateTime?>(nameof(MinDate));

    /// <summary>
    /// Identifies the <see cref="MaxDate" /> property.
    /// </summary>
    public static readonly StyledProperty<DateTime?> MaxDateProperty =
        AvaloniaProperty.Register<DatePicker, DateTime?>(nameof(MaxDate));

    /// <summary>
    /// Identifies the <see cref="Format" /> property.
    /// </summary>
    public static readonly StyledProperty<string?> FormatProperty =
        AvaloniaProperty.Register<DatePicker, string?>(nameof(Format));

    /// <summary>
    /// Identifies the <see cref="PickerMode" /> property.
    /// </summary>
    public static readonly StyledProperty<DatePickerMode> PickerModeProperty =
        AvaloniaProperty.Register<DatePicker, DatePickerMode>(nameof(PickerMode));

    /// <summary>
    /// Identifies the <see cref="IsShowTime" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsShowTimeProperty =
        AvaloniaProperty.Register<DatePicker, bool>(nameof(IsShowTime), false);

    /// <summary>
    /// Identifies the <see cref="IsNeedConfirm" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsNeedConfirmProperty =
        AvaloniaProperty.Register<DatePicker, bool>(nameof(IsNeedConfirm));

    /// <summary>
    /// Identifies the <see cref="IsShowNow" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsShowNowProperty =
        AvaloniaProperty.Register<DatePicker, bool>(nameof(IsShowNow), true);

    /// <summary>
    /// Identifies the <see cref="ClockIdentifier" /> property.
    /// </summary>
    public static readonly StyledProperty<ClockIdentifierType> ClockIdentifierProperty =
        TimePicker.ClockIdentifierProperty.AddOwner<DatePicker>();

    /// <summary>
    /// Gets or sets the selected date and time, or <see langword="null" /> when no value is selected. The input
    /// shows the value in the format resolved from <see cref="Format" />. With <see langword="null" /> the input is
    /// empty and the clear button is hidden. Bindings to this property are two-way by default.
    /// </summary>
    public DateTime? SelectedDateTime
    {
        get => GetValue(SelectedDateTimeProperty);
        set => SetValue(SelectedDateTimeProperty, value);
    }

    /// <summary>
    /// Gets or sets the date and time that <see cref="Reset" /> restores. It is also assigned to
    /// <see cref="SelectedDateTime" /> whenever the template is applied and no value is selected.
    /// <see cref="Clear" /> does not apply it.
    /// </summary>
    public DateTime? DefaultDateTime
    {
        get => GetValue(DefaultDateTimeProperty);
        set => SetValue(DefaultDateTimeProperty, value);
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
    /// Gets or sets the earliest selectable date; the boundary date itself can be selected. The bound is normalized
    /// to the unit of the <see cref="PickerMode" />. <see langword="null" /> leaves the range open on this side, and
    /// a value later than <see cref="MaxDate" /> narrows the range to this single date.
    /// </summary>
    public DateTime? MinDate
    {
        get => GetValue(MinDateProperty);
        set => SetValue(MinDateProperty, value);
    }

    /// <summary>
    /// Gets or sets the latest selectable date; the boundary date itself can be selected. The bound is normalized
    /// to the unit of the <see cref="PickerMode" />. <see langword="null" /> leaves the range open on this side, and
    /// an earlier <see cref="MinDate" /> narrows the range to that date.
    /// </summary>
    public DateTime? MaxDate
    {
        get => GetValue(MaxDateProperty);
        set => SetValue(MaxDateProperty, value);
    }

    /// <summary>
    /// Gets or sets the .NET date and time format string used to show the selected value in the input and to parse
    /// typed text, which must match the displayed format in full. <see langword="null" /> selects a built-in format
    /// for the <see cref="PickerMode" />. In <see cref="DatePickerMode.Date" /> mode it is <c>yyyy-MM-dd</c>, plus
    /// the time when <see cref="IsShowTime" /> is <see langword="true" />. A string that is not
    /// <see langword="null" /> is used as is.
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
    /// Gets or sets whether the user also selects a time. The time selection is shown only in
    /// <see cref="DatePickerMode.Date" /> mode, and the default input format then includes the time.
    /// </summary>
    public bool IsShowTime
    {
        get => GetValue(IsShowTimeProperty);
        set => SetValue(IsShowTimeProperty, value);
    }

    /// <summary>
    /// Gets or sets whether a date picked in the panel becomes the value only after the user presses the confirm
    /// button, which the popup shows only in this case. With <see langword="false" />, a picked date is committed
    /// at once.
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
    /// Gets or sets whether the time uses a 12-hour or a 24-hour clock. It selects the default time format and the
    /// AM/PM designators, so it has an effect only when the input text shows a time.
    /// </summary>
    public ClockIdentifierType ClockIdentifier
    {
        get => GetValue(ClockIdentifierProperty);
        set => SetValue(ClockIdentifierProperty, value);
    }

    #endregion

    #region 内部属性定义

    internal static readonly DirectProperty<DatePicker, string?> AmTextProperty =
        AvaloniaProperty.RegisterDirect<DatePicker, string?>(nameof(AmText),
            o => o.AmText,
            (o, v) => o.AmText = v);

    internal static readonly DirectProperty<DatePicker, string?> PmTextProperty =
        AvaloniaProperty.RegisterDirect<DatePicker, string?>(nameof(PmText),
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

    public DatePicker()
    {
    }

    static DatePicker()
    {
        SelectedDateTimeProperty.Changed.AddClassHandler<DatePicker>((datePicker, args) => datePicker.NotifyFormValueChanged(args.NewValue));
    }

    private DatePickerPresenter? _pickerPresenter;
    private ILanguageManager? _languageManager;
    private TextBox? _attachedInputBox;

    // Text typed in this open session, or null while the input shows a formatted value.
    private string? _typedText;
    private bool _isTypedTextAccepted;
    private bool _isApplyingTypedText;

    /// <summary>
    /// Calls the base method, then subscribes to language changes and shows the selected value in the input.
    /// </summary>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _languageManager = Application.Current is { } app ? global::AtomUI.ApplicationExtensions.GetLanguageManager(app) : null;
        if (_languageManager is not null)
        {
            _languageManager.LanguageChanged += OnLanguageChanged;
        }

        Text = FormatDateTime(SelectedDateTime);
    }

    /// <summary>
    /// Unsubscribes from language changes and releases the language manager, then calls the base method.
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
        Text = FormatDateTime(SelectedDateTime);
        CalculatePreferredWidth();
    }

    /// <summary>
    /// Clears the selected value without applying <see cref="DefaultDateTime" />, discards text typed in the
    /// input and closes the popup.
    /// </summary>
    public override void Clear()
    {
        // The value and the picker text are cleared before the input box, so clearing the box is not taken for
        // typing; the draft is dropped and the popup closes, as in Ant Design.
        _typedText           = null;
        _isTypedTextAccepted = false;
        SelectedDateTime     = null;
        _pickerPresenter?.CancelEdit();
        Text = FormatDateTime(SelectedDateTime);
        base.Clear();
        ClosePickerFlyout();
    }

    /// <summary>
    /// Restores <see cref="DefaultDateTime" /> as the selected value. The value stays empty when no default
    /// is set.
    /// </summary>
    public void Reset()
    {
        SelectedDateTime = DefaultDateTime;
    }

    /// <summary>
    /// Formats a date and time with the format resolved from <see cref="Format" />.
    /// </summary>
    /// <param name="dateTime">
    /// The value to format.
    /// </param>
    /// <returns>
    /// The formatted text, or an empty string when <paramref name="dateTime" /> is <see langword="null" />.
    /// </returns>
    protected string FormatDateTime(DateTime? dateTime)
    {
        if (dateTime is null)
        {
            return string.Empty;
        }

        var formatInfo = DatePickerFormattingHelper.CreateFormatInfo(ClockIdentifier, AmText, PmText);
        return DatePickerFormattingHelper.FormatDateTime(dateTime.Value, Format, PickerMode, IsShowTime, ClockIdentifier, formatInfo);
    }

    /// <summary>
    /// Called by the base class to create the presenter that the popup shows. The result is a
    /// <see cref="DatePickerPresenter" /> bound to the motion, value, range, mode and display properties of this
    /// control.
    /// </summary>
    /// <returns>
    /// The presenter of the popup.
    /// </returns>
    protected override Control CreatePickerPresenter()
    {
        var presenter = new DatePickerPresenter();
        presenter[!DatePickerPresenter.IsMotionEnabledProperty]  = this[!IsMotionEnabledProperty];
        presenter[!DatePickerPresenter.SelectedDateTimeProperty] = this[!SelectedDateTimeProperty];
        presenter[!DatePickerPresenter.PickerDisplayDateProperty] = this[!PickerDisplayDateProperty];
        presenter[!DatePickerPresenter.MinDateProperty]          = this[!MinDateProperty];
        presenter[!DatePickerPresenter.MaxDateProperty]          = this[!MaxDateProperty];
        presenter[!DatePickerPresenter.IsNeedConfirmProperty]    = this[!IsNeedConfirmProperty];
        presenter[!DatePickerPresenter.IsShowNowProperty]        = this[!IsShowNowProperty];
        presenter[!DatePickerPresenter.IsShowTimeProperty]       = this[!IsShowTimeProperty];
        presenter[!DatePickerPresenter.ClockIdentifierProperty]  = this[!ClockIdentifierProperty];
        presenter[!DatePickerPresenter.PickerModeProperty]       = this[!PickerModeProperty];

        return presenter;
    }

    /// <summary>
    /// Called by the base class right after the picker presenter was created. Calls the base method, then keeps the
    /// presenter, when it is a <see cref="DatePickerPresenter" />, for the handlers used while the popup is open.
    /// </summary>
    protected override void NotifyPickerPresenterCreated(Control pickerPresenter)
    {
        base.NotifyPickerPresenterCreated(pickerPresenter);
        _pickerPresenter = pickerPresenter as DatePickerPresenter;
    }

    /// <summary>
    /// Called by the base class when it releases the picker presenter. Calls the base method, then forgets the
    /// stored presenter.
    /// </summary>
    protected override void NotifyPickerPresenterCleared(Control pickerPresenter)
    {
        base.NotifyPickerPresenterCleared(pickerPresenter);
        _pickerPresenter = null;
    }

    /// <summary>
    /// Called by the base class when the popup opens. Calls the base method, then, when the presenter exists,
    /// subscribes to its events and property changes and resets its panel state for this opening.
    /// </summary>
    protected override void NotifyPickerOpened()
    {
        base.NotifyPickerOpened();
        if (_pickerPresenter is not null)
        {
            _pickerPresenter.ChoosingStatusChanged += HandleChoosingStatusChanged;
            _pickerPresenter.HoverDateTimeChanged  += HandleHoverDateTimeChanged;
            _pickerPresenter.Confirmed             += HandleConfirmed;
            _pickerPresenter.PropertyChanged       += HandlePresenterPropertyChanged;
            _pickerPresenter.ResetOpenPanelState();
        }
    }

    /// <summary>
    /// Called by the base class when the popup closes. Calls the base method, then unsubscribes from the presenter
    /// and settles pending typed text: the text is committed when it was accepted, the close is neither a cancel nor
    /// a lifecycle close and the presenter needs no confirmation, and otherwise the edit is canceled. Finally the
    /// input shows the selected value again and the preferred input width is updated.
    /// </summary>
    protected override void NotifyPickerClosed()
    {
        base.NotifyPickerClosed();
        if (_pickerPresenter is not null)
        {
            _pickerPresenter.ChoosingStatusChanged -= HandleChoosingStatusChanged;
            _pickerPresenter.HoverDateTimeChanged  -= HandleHoverDateTimeChanged;
            _pickerPresenter.Confirmed             -= HandleConfirmed;
            _pickerPresenter.PropertyChanged       -= HandlePresenterPropertyChanged;
            if (_typedText is not null && !IsCancelingEdit)
            {
                // Focus left, the window was deactivated or the popup was dismissed: a valid typed value is kept
                // when no confirmation is required, otherwise the edit is discarded, as in Ant Design. A lifecycle
                // close (detach, template change) always discards it.
                if (_isTypedTextAccepted && !IsLifecycleClosing && !_pickerPresenter.RequiresConfirmation)
                {
                    SelectedDateTime = _pickerPresenter.SelectedDateTime;
                }
                else
                {
                    _pickerPresenter.CancelEdit();
                }
            }
        }

        _typedText           = null;
        _isTypedTextAccepted = false;
        Text                 = FormatDateTime(SelectedDateTime);
        CalculatePreferredWidth();
    }

    private void HandlePresenterPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs args)
    {
        // A candidate chosen in the panel replaces the typed text.
        if (args.Property == DatePickerPresenter.SelectedDateTimeProperty && !_isApplyingTypedText)
        {
            _typedText = null;
            // A hover preview keeps the input until it ends; otherwise the input shows the new candidate at once.
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
        _attachedInputBox = InfoInputBox;
        if (_attachedInputBox is not null)
        {
            _attachedInputBox.PropertyChanged += HandleInputTextChanged;
        }
    }

    private void HandleInputTextChanged(object? sender, AvaloniaPropertyChangedEventArgs args)
    {
        if (args.Property != TextBox.TextProperty || sender is not TextBox input || IsReadOnly)
        {
            return;
        }

        var text = input.Text ?? string.Empty;
        if (string.Equals(text, Text ?? string.Empty, StringComparison.Ordinal))
        {
            return;
        }

        // Typing opens the popup and previews a valid value in the panel, as in Ant Design.
        SetPickerOpenIfChanged(true);
        _typedText = text;
        SetCurrentValue(TextProperty, text);
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
    /// Returns <see langword="true" /> while the input shows text typed in this open session instead of a formatted
    /// value. The base class then leaves navigation keys to the input instead of passing them to
    /// <see cref="NotifyNavigationKey" />.
    /// </summary>
    private protected override bool HasPendingInputText => _typedText is not null;

    /// <summary>
    /// Handles Enter while the popup is open. When the popup has no presenter, it closes the popup. Whitespace only
    /// text typed in the input clears the selected value and closes the popup, and typed text that was not accepted
    /// keeps the popup open. Otherwise the candidate is submitted from the keyboard, which needs no confirm button.
    /// When the input shows a formatted value and the keyboard has moved the panel focus, Enter activates the
    /// focused cell instead. The popup closes when the submission commits nothing.
    /// </summary>
    private protected override void NotifyInputSubmit()
    {
        if (_pickerPresenter is null)
        {
            ClosePickerFlyout();
            return;
        }

        if (_typedText is not null)
        {
            if (string.IsNullOrWhiteSpace(_typedText))
            {
                Clear();
                ClosePickerFlyout();
                return;
            }

            if (!_isTypedTextAccepted)
            {
                return;
            }
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
    /// Called when the edit of the open session is canceled, for example by Escape. The presenter reverts its
    /// candidate to the committed value, the typed text is forgotten and the input shows the selected value again at
    /// once, so that nothing is left over when the popup is pinned open.
    /// </summary>
    private protected override void NotifyEditCanceled()
    {
        _pickerPresenter?.CancelEdit();
        // A pinned popup stays open, so the input returns to the committed value here rather than on close.
        _typedText           = null;
        _isTypedTextAccepted = false;
        Text                 = FormatDateTime(SelectedDateTime);
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

    private void HandleHoverDateTimeChanged(object? sender, DateSelectedEventArgs args)
    {
        if (args.Date.HasValue)
        {
            Text = FormatDateTime(args.Date);
            // Hovering the confirm button shows the candidate itself, which is not dimmed as a preview.
            SetPreviewing(args.Date != _pickerPresenter?.SelectedDateTime);
            CalculatePreferredWidth();
        }
        else
        {
            // The preview ended, for example when the pointer left the cells after keyboard navigation.
            ClearHoverSelectedInfo();
        }
    }

    private void HandleConfirmed(object? sender, EventArgs args)
    {
        _typedText = null;
        SelectedDateTime = _pickerPresenter?.SelectedDateTime;
        ClosePickerFlyout();
    }

    private void ClearHoverSelectedInfo()
    {
        SetPreviewing(false);
        Text = _typedText ?? FormatDateTime(_pickerPresenter?.SelectedDateTime ?? SelectedDateTime);
        CalculatePreferredWidth();
    }

    /// <summary>
    /// Calls the base method, then subscribes to text changes of the template's input box and assigns
    /// <see cref="DefaultDateTime" /> to <see cref="SelectedDateTime" /> while no value is selected. It then shows
    /// the selected value in the input, installs the default calendar icon when <c>InfoIcon</c> is not set, and
    /// updates the preferred input width.
    /// </summary>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        AttachInputHandlers();
        if (DefaultDateTime is not null && SelectedDateTime is null)
        {
            SelectedDateTime = DefaultDateTime;
        }

        Text = FormatDateTime(SelectedDateTime);

        if (InfoIcon is null)
        {
            SetValue(InfoIconProperty, new CalendarOutlined(), BindingPriority.Template);
        }
        CalculatePreferredWidth();
    }

    /// <summary>
    /// Calls the base method. When <see cref="SelectedDateTime" /> or a property that affects the formatted text
    /// changes, the input shows the selected value again and the preferred input width is updated. When a property
    /// that affects only the preferred input width changes, such as the font, size and alignment properties, only
    /// the width is updated.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == SelectedDateTimeProperty)
        {
            Text = FormatDateTime(SelectedDateTime);
            CalculatePreferredWidth();
        }
        else if (IsFormattedTextAffectingProperty(change.Property))
        {
            Text = FormatDateTime(SelectedDateTime);
            CalculatePreferredWidth();
        }
        else if (IsPreferredWidthAffectingProperty(change.Property))
        {
            CalculatePreferredWidth();
        }
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
        // The preferred input width is always computed from one content baseline, shared by the placeholder and the
        // selected value, so the input area does not change width with the text under an explicit Width or Stretch.
        // The total control width under an explicit Width or Stretch is still decided by the outer layout.
        var formatInfo = DatePickerFormattingHelper.CreateFormatInfo(ClockIdentifier, AmText, PmText);
        PreferredInputWidth = DatePickerFormattingHelper.CalculateBoundedPreferredInputWidth(
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
    }

    /// <summary>
    /// Called by the base class to decide whether the clear button is shown.
    /// </summary>
    /// <returns>
    /// <see langword="true" /> only while <see cref="SelectedDateTime" /> has a value.
    /// </returns>
    protected override bool ShowClearButtonPredicate()
    {
        return SelectedDateTime is not null;
    }
    
    #region 实现 FormItem 接口
    /// <summary>
    /// Called by the base class when a form sets the value of this field. A <see cref="DateTime" /> is assigned to
    /// <see cref="SelectedDateTime" />; any other value, including <see langword="null" />, clears it.
    /// </summary>
    protected override void NotifySetFormValue(object? value)
    {
        SelectedDateTime = value as DateTime?;
    }

    /// <summary>
    /// Called by the base class when a form reads the value of this field.
    /// </summary>
    /// <returns>
    /// The value of <see cref="SelectedDateTime" />.
    /// </returns>
    protected override object? NotifyGetFormValue()
    {
        return SelectedDateTime;
    }

    /// <summary>
    /// Called by the base class when a form clears this field. It sets <see cref="SelectedDateTime" /> to
    /// <see langword="null" /> without applying <see cref="DefaultDateTime" />.
    /// </summary>
    protected override void NotifyClearFormValue()
    {
        SelectedDateTime = null;
    }
    #endregion
}
