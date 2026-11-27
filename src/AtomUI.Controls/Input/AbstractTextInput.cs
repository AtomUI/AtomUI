using AtomUI.Icons.AntDesign;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Input;
using Avalonia.LogicalTree;

namespace AtomUI.Controls;

using AvaloniaTextBox = Avalonia.Controls.TextBox;
using AvaloniaButton = Avalonia.Controls.Button;

/// <summary>
/// Shared editing and validation behavior for AtomUI text inputs.
/// </summary>
public abstract partial class AbstractTextInput : AvaloniaTextBox,
                                           IMotionAwareControl,
                                           IFormItemAware,
                                           IFormItemFeedbackAware,
                                           IInputControlStatusAware
{
    #region 公共属性定义

    /// <summary>
    /// Identifies the <see cref="Status" /> property.
    /// </summary>
    public static readonly StyledProperty<InputControlStatus> StatusProperty =
        InputControlStatusProperty.StatusProperty.AddOwner<AbstractTextInput>();

    /// <summary>
    /// Identifies the <see cref="IsAllowClear" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsAllowClearProperty =
        AvaloniaProperty.Register<AbstractTextInput, bool>(nameof(IsAllowClear));

    /// <summary>
    /// Identifies the <see cref="IsEnableRevealButton" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsEnableRevealButtonProperty =
        AvaloniaProperty.Register<AbstractTextInput, bool>(nameof(IsEnableRevealButton));

    /// <summary>
    /// Identifies the <see cref="IsShowCount" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsShowCountProperty =
        AvaloniaProperty.Register<AbstractTextInput, bool>(nameof(IsShowCount));

    /// <summary>
    /// Identifies the <see cref="ClearIcon" /> property.
    /// </summary>
    public static readonly StyledProperty<PathIcon?> ClearIconProperty =
        AvaloniaProperty.Register<AbstractTextInput, PathIcon?>(nameof(ClearIcon));

    /// <summary>
    /// Identifies the <see cref="IsMotionEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<AbstractTextInput>();

    /// <summary>
    /// Validation state of the input frame: <see cref="InputControlStatus.Warning" /> or
    /// <see cref="InputControlStatus.Error" /> paints the frame in that state, and
    /// <see cref="InputControlStatus.Default" /> leaves it neutral. Data validation errors and an error reported by
    /// the form take precedence and force the error state; a warning reported by the form forces the warning state.
    /// </summary>
    public InputControlStatus Status
    {
        get => GetValue(StatusProperty);
        set => SetValue(StatusProperty, value);
    }

    /// <summary>
    /// <see langword="true" /> lets a clear button appear in the input. The button is visible only while the text can
    /// be cleared (<see cref="CanClearText" />) and, with <see cref="IsClearButtonOnlyWhenFocused" /> set, only while
    /// the input has keyboard focus. Clicking it calls <see cref="Clear" /> and raises <see cref="Cleared" />.
    /// </summary>
    public bool IsAllowClear
    {
        get => GetValue(IsAllowClearProperty);
        set => SetValue(IsAllowClearProperty, value);
    }

    /// <summary>
    /// <see langword="true" /> shows the reveal button of the theme, which toggles <c>RevealPassword</c> with its
    /// checked state. Themes without a reveal button ignore the value.
    /// </summary>
    public bool IsEnableRevealButton
    {
        get => GetValue(IsEnableRevealButtonProperty);
        set => SetValue(IsEnableRevealButtonProperty, value);
    }

    /// <summary>
    /// <see langword="true" /> shows a counter with the length of the text and <see cref="TextBox.MaxLength" />,
    /// separated by a slash. The length is measured in the unit selected by <see cref="TextLengthMode" />.
    /// </summary>
    public bool IsShowCount
    {
        get => GetValue(IsShowCountProperty);
        set => SetValue(IsShowCountProperty, value);
    }

    /// <summary>
    /// Icon of the clear button. While <see cref="IsAllowClear" /> is <see langword="true" />, a
    /// <see langword="null" /> value is replaced with the default icon (<c>CloseCircleFilled</c>) when the control is
    /// initialized or when <see cref="IsAllowClear" /> changes to <see langword="true" />.
    /// </summary>
    public PathIcon? ClearIcon
    {
        get => GetValue(ClearIconProperty);
        set => SetValue(ClearIconProperty, value);
    }

    /// <summary>
    /// <see langword="true" /> enables the animated state transitions of the input frame, the clear button and the
    /// reveal button. When the value is not set, the theme supplies it from the <c>EnableMotion</c> token.
    /// </summary>
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    #endregion

    #region 内部属性定义

    internal static readonly DirectProperty<AbstractTextInput, bool> IsEffectiveShowClearButtonProperty =
        AvaloniaProperty.RegisterDirect<AbstractTextInput, bool>(
            nameof(IsEffectiveShowClearButton),
            o => o.IsEffectiveShowClearButton,
            (o, value) => o.IsEffectiveShowClearButton = value);

    internal static readonly DirectProperty<AbstractTextInput, string?> CountTextProperty =
        AvaloniaProperty.RegisterDirect<AbstractTextInput, string?>(
            nameof(CountText),
            o => o.CountText,
            (o, value) => o.CountText = value);

    internal static readonly StyledProperty<FormValidateFeedback?> FormFeedbackProperty =
        AvaloniaProperty.Register<AbstractTextInput, FormValidateFeedback?>(nameof(FormFeedback));

    internal static readonly StyledProperty<FormValidateStatus> FormStatusProperty =
        InputControlState.FormStatusProperty.AddOwner<AbstractTextInput>();

    internal static readonly DirectProperty<AbstractTextInput, bool> IsFormFeedbackVisibleProperty =
        AvaloniaProperty.RegisterDirect<AbstractTextInput, bool>(
            nameof(IsFormFeedbackVisible),
            o => o.IsFormFeedbackVisible,
            (o, value) => o.IsFormFeedbackVisible = value);

    internal static readonly DirectProperty<AbstractTextInput, bool> IsPlaceholderTextVisibleProperty =
        AvaloniaProperty.RegisterDirect<AbstractTextInput, bool>(
            nameof(IsPlaceholderTextVisible),
            o => o.IsPlaceholderTextVisible,
            (o, value) => o.IsPlaceholderTextVisible = value);

    private bool _isEffectiveShowClearButton;

    internal bool IsEffectiveShowClearButton
    {
        get => _isEffectiveShowClearButton;
        private set => SetAndRaise(IsEffectiveShowClearButtonProperty, ref _isEffectiveShowClearButton, value);
    }

    private string? _countText;

    internal string? CountText
    {
        get => _countText;
        private set => SetAndRaise(CountTextProperty, ref _countText, value);
    }

    /// <summary>
    /// The feedback control that shows the validation messages of the form item that owns this input; the theme
    /// presents it as content of the feedback area. The form system assigns it through
    /// <see cref="NotifySetFeedBackControl" />; <see langword="null" />, the default, shows no feedback. While the
    /// input is in a logical tree, the validation status of the control is observed to update
    /// <see cref="IsFormFeedbackVisible" />.
    /// </summary>
    internal FormValidateFeedback? FormFeedback
    {
        get => GetValue(FormFeedbackProperty);
        private set => SetCurrentValue(FormFeedbackProperty, value);
    }

    /// <summary>
    /// The validation status that the hosting form item reports for this input; the form system assigns it through
    /// <see cref="NotifyValidateStatus" />, and <see cref="FormValidateStatus.Default" /> means that nothing is
    /// reported. The themes pass it to a template part. Together with <see cref="Status" /> it decides the state of
    /// the input frame, as described there.
    /// </summary>
    internal FormValidateStatus FormStatus
    {
        get => GetValue(FormStatusProperty);
        private set => SetCurrentValue(FormStatusProperty, value);
    }

    private bool _isFormFeedbackVisible;

    /// <summary>
    /// Whether the feedback area of the theme is visible; the theme binds the visibility of the feedback presenter
    /// to it. The control sets it to <see langword="true" /> while <see cref="FormFeedback" /> reports a status
    /// other than <see cref="FormValidateStatus.Default" />, and to <see langword="false" /> while there is no
    /// feedback control or the input is not in a logical tree.
    /// </summary>
    internal bool IsFormFeedbackVisible
    {
        get => _isFormFeedbackVisible;
        private set => SetAndRaise(IsFormFeedbackVisibleProperty, ref _isFormFeedbackVisible, value);
    }

    private bool _isPlaceholderTextVisible = true;

    /// <summary>
    /// Whether the placeholder text of the theme is visible; the themes bind the visibility of the placeholder to
    /// it. The control sets it to <see langword="true" /> while the text and the pre-edit text of the input method
    /// are both empty, and to <see langword="false" /> otherwise. The initial value is <see langword="true" />.
    /// </summary>
    internal bool IsPlaceholderTextVisible
    {
        get => _isPlaceholderTextVisible;
        private set => SetAndRaise(IsPlaceholderTextVisibleProperty, ref _isPlaceholderTextVisible, value);
    }

    #endregion

    #region 内部协作 API

    /// <summary>
    /// The <c>PART_TextPresenter</c> part of the current template, or <see langword="null" /> while no such part is
    /// attached.
    /// </summary>
    protected TextPresenter? InputTextPresenter => _textPresenter;

    /// <summary>
    /// Whether the text can be cleared in the current state: the input is not read-only, the text is not empty and,
    /// for multi-line input, <see cref="AllowsClearForMultilineInput" /> is <see langword="true" />. Together with
    /// <see cref="IsAllowClear" /> and <see cref="IsClearButtonOnlyWhenFocused" /> it decides whether the clear
    /// button is visible.
    /// </summary>
    protected virtual bool CanClearText =>
        !IsReadOnly && !string.IsNullOrEmpty(Text) && (!AcceptsReturn || AllowsClearForMultilineInput);

    /// <summary>
    /// Allows multiline text controls to opt into the same clear semantics as
    /// single-line controls without making the shared layer depend on a concrete type.
    /// </summary>
    protected virtual bool AllowsClearForMultilineInput => false;

    #endregion

    private AvaloniaButton? _clearButton;
    private TextPresenter? _textPresenter;
    private IDisposable? _preeditTextSubscription;
    private IDisposable? _feedbackStatusSubscription;
    private EventHandler? _formValueChanged;

    static AbstractTextInput()
    {
        TextChangedEvent.AddClassHandler<AbstractTextInput>((input, _) => input.HandleTextChanged());
    }

    protected AbstractTextInput()
    {
    }

    /// <summary>
    /// Creates the default clear icon when <see cref="IsAllowClear" /> is <see langword="true" /> and
    /// <see cref="ClearIcon" /> is <see langword="null" />. An override must call the base method to get the default
    /// icon.
    /// </summary>
    protected override void OnInitialized()
    {
        base.OnInitialized();
        if (TextLengthMode == TextLengthMode.TextElement)
        {
            UpdateTextLengthMode();
        }
        if (IsAllowClear && ClearIcon is null)
        {
            SetCurrentValue(ClearIconProperty, new CloseCircleFilled());
        }
    }

    /// <summary>
    /// Updates the state that depends on other properties: the visibility of the clear button and of the placeholder,
    /// the text length limit, the counter text and the warning pseudo-class. An override must call the base method,
    /// otherwise these states stop following their sources.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == AcceptsReturnProperty ||
            change.Property == IsReadOnlyProperty ||
            change.Property == TextProperty ||
            change.Property == IsAllowClearProperty ||
            change.Property == IsClearButtonOnlyWhenFocusedProperty ||
            change.Property == IsKeyboardFocusWithinProperty)
        {
            ConfigureEffectiveShowClearButton();
            ConfigurePlaceholderTextVisibility();
        }
        else if (change.Property == TextLengthModeProperty)
        {
            UpdateTextLengthMode();
        }
        else if (change.Property == IsShowCountProperty || change.Property == MaxLengthProperty)
        {
            HandleInputChanged(Text);
        }
        else if (change.Property == FormFeedbackProperty)
        {
            ConfigureFormFeedbackSubscription();
        }
        if (change.Property == IsAllowClearProperty && IsInitialized && IsAllowClear && ClearIcon is null)
        {
            SetCurrentValue(ClearIconProperty, new CloseCircleFilled());
        }

        if (change.Property == StatusProperty ||
            change.Property == FormStatusProperty ||
            change.Property == DataValidationErrors.HasErrorsProperty ||
            change.Property == DataValidationErrors.ErrorsProperty)
        {
            UpdateWarningPseudoClass();
        }
    }

    /// <summary>
    /// Releases the parts of the previous template, that is, the clear button subscription and the subscription to
    /// pre-edit changes, before the new template is applied. An override must call the base method.
    /// </summary>
    protected override void OnTemplateChanged(AvaloniaPropertyChangedEventArgs change)
    {
        ReleaseTemplateResources();
        base.OnTemplateChanged(change);
    }

    /// <summary>
    /// Resolves <c>PART_ClearButton</c> and <c>PART_TextPresenter</c>, subscribes to pre-edit changes and refreshes
    /// the visibility of the clear button and of the placeholder, the counter text and the warning state. An override
    /// must call the base method, otherwise the clear button and the input method state do not work.
    /// </summary>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        if (e.NameScope.Find<AvaloniaButton>("PART_ClearButton") is { } clearButton)
        {
            SetClearButton(clearButton);
        }
        _preeditTextSubscription?.Dispose();

        _textPresenter = e.NameScope.Find<TextPresenter>("PART_TextPresenter");
        _preeditTextSubscription = _textPresenter?.GetObservable(TextPresenter.PreeditTextProperty)
            .Subscribe(_ => UpdatePreeditState());

        ConfigureEffectiveShowClearButton();
        ConfigurePlaceholderTextVisibility();
        HandleInputChanged(Text);
        UpdateWarningPseudoClass();
    }

    /// <summary>
    /// Starts observing the validation status of the assigned form feedback control. An override must call the base
    /// method, otherwise the feedback is never shown.
    /// </summary>
    protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnAttachedToLogicalTree(e);
        ConfigureFormFeedbackSubscription();
    }

    /// <summary>
    /// Stops observing the validation status of the form feedback control. An override must call the base method to
    /// release the subscription.
    /// </summary>
    protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromLogicalTree(e);
        _feedbackStatusSubscription?.Dispose();
        _feedbackStatusSubscription = null;
    }

    internal void SetClearButton(AvaloniaButton? button)
    {
        if (ReferenceEquals(button, _clearButton))
        {
            return;
        }
        if (_clearButton is not null)
        {
            _clearButton.Click -= HandleClearButtonClicked;
        }
        _clearButton = button;
        if (_clearButton is not null)
        {
            _clearButton.Click += HandleClearButtonClicked;
        }
    }

    /// <summary>
    /// Returns how focus moves back to the input after its clear button was clicked. The base implementation returns
    /// <see langword="null" />, which leaves the focus where it is.
    /// </summary>
    /// <param name="button">
    /// The clear button that was clicked.
    /// </param>
    /// <returns>
    /// The navigation method used to focus the input, or <see langword="null" /> to leave the focus unchanged.
    /// </returns>
    protected virtual NavigationMethod? GetClearButtonFocusReturnMethod(AvaloniaButton? button) => null;

    /// <summary>
    /// Called when the clear button of the input was clicked. The base implementation calls <see cref="Clear" />.
    /// </summary>
    protected virtual void NotifyClearButtonClicked() => Clear();

    /// <summary>
    /// Applies a value assigned by the form system. The default sets <see cref="TextBox.Text" /> with
    /// <c>SetCurrentValue</c>, so a binding on <see cref="TextBox.Text" /> is kept.
    /// </summary>
    /// <param name="value">
    /// The text to assign, or <see langword="null" />.
    /// </param>
    protected virtual void NotifySetFormValue(string? value) => SetCurrentValue(TextProperty, value);

    /// <summary>
    /// Returns the value reported to the form system.
    /// </summary>
    /// <returns>
    /// The current <see cref="TextBox.Text" />.
    /// </returns>
    protected virtual string? NotifyGetFormValue() => Text;

    /// <summary>
    /// Resets the value on behalf of the form system. The default sets <see cref="TextBox.Text" /> to
    /// <see langword="null" /> with <c>SetCurrentValue</c>.
    /// </summary>
    protected virtual void NotifyClearFormValue() => SetCurrentValue(TextProperty, null);

    /// <summary>
    /// Called when the form system reports a new validation status. The default stores a status that differs in
    /// <c>FormStatus</c>, which lets the input frame follow the form. An override must call the base method to keep
    /// that behavior.
    /// </summary>
    /// <param name="status">
    /// The validation status reported by the form system.
    /// </param>
    protected virtual void NotifyValidateStatus(FormValidateStatus status)
    {
        if (FormStatus != status)
        {
            SetCurrentValue(FormStatusProperty, status);
        }
    }

    /// <summary>
    /// Called by the form system to assign the control that shows validation messages. The default stores a control
    /// that differs from the current one in <c>FormFeedback</c>. An override must call the base method, otherwise the
    /// feedback is never shown.
    /// </summary>
    /// <param name="value">
    /// The feedback control, or <see langword="null" /> to show none.
    /// </param>
    protected virtual void NotifySetFeedBackControl(FormValidateFeedback? value)
    {
        if (!ReferenceEquals(FormFeedback, value))
        {
            SetCurrentValue(FormFeedbackProperty, value);
        }
    }

    #region 实现 FormItem 接口

    event EventHandler? IFormItemAware.ValueChanged
    {
        add => _formValueChanged += value;
        remove => _formValueChanged -= value;
    }

    void IFormItemAware.SetFormValue(object? value) => NotifySetFormValue(value?.ToString());

    object? IFormItemAware.GetFormValue() => NotifyGetFormValue();

    void IFormItemAware.ClearFormValue() => NotifyClearFormValue();

    void IFormItemAware.NotifyValidateStatus(FormValidateStatus status) => NotifyValidateStatus(status);

    void IFormItemFeedbackAware.SetFeedbackControl(FormValidateFeedback? value) => NotifySetFeedBackControl(value);

    #endregion

    private void HandleTextChanged()
    {
        HandleInputChanged(Text);
        if (!IsComposing)
        {
            _formValueChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private void UpdateWarningPseudoClass()
    {
        var isWarning = InputControlState.ResolveEffectiveStatus(this, Status, FormStatus) ==
                        InputControlStatus.Warning;
        PseudoClasses.Set(StdPseudoClass.Warning, isWarning);
    }

    private void HandleInputChanged(string? text)
    {
        if (IsShowCount)
        {
            SetCurrentValue(CountTextProperty, $"{GetTextLength(text)} / {MaxLength}");
        }
    }

    private void ConfigureEffectiveShowClearButton()
    {
        SetCurrentValue(IsEffectiveShowClearButtonProperty,
            IsAllowClear && CanClearText && (!IsClearButtonOnlyWhenFocused || IsKeyboardFocusWithin));
    }

    private void ConfigurePlaceholderTextVisibility()
    {
        SetCurrentValue(IsPlaceholderTextVisibleProperty,
            string.IsNullOrEmpty(Text) && string.IsNullOrEmpty(_textPresenter?.PreeditText));
    }

    private void ConfigureFormFeedbackSubscription()
    {
        _feedbackStatusSubscription?.Dispose();
        _feedbackStatusSubscription = null;
        if (!((ILogical)this).IsAttachedToLogicalTree)
        {
            IsFormFeedbackVisible = false;
            return;
        }

        _feedbackStatusSubscription = FormFeedback?.GetObservable(FormValidateFeedback.ValidateStatusProperty)
            .Subscribe(status => IsFormFeedbackVisible = status != FormValidateStatus.Default);
        if (FormFeedback is null)
        {
            IsFormFeedbackVisible = false;
        }
    }

    private void HandleClearButtonClicked(object? sender, RoutedEventArgs args)
    {
        // Where the clear button is a tab stop (TextArea, Mentions), focus returns to the input, as in Ant Design;
        // the button hides once the text is gone.
        var focusReturnMethod = GetClearButtonFocusReturnMethod(sender as AvaloniaButton);
        NotifyClearButtonClicked();
        if (focusReturnMethod is { } navigationMethod)
        {
            Focus(navigationMethod);
        }
    }

    private void ReleaseTemplateResources()
    {
        SetClearButton(null);
        _preeditTextSubscription?.Dispose();
        _preeditTextSubscription = null;
        _textPresenter = null;
        UpdatePreeditState();
    }

}
