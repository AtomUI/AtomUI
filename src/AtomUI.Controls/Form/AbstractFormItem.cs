using AtomUI.Desktop.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;

namespace AtomUI.Controls;

/// <summary>
/// Base class of a form item. It hosts one value control, tracks the value of that control under
/// <see cref="FieldName" />, and runs the <see cref="Validators" />.
/// The outcome is published through <see cref="ValidateStatus" />, <see cref="ValidateResult" />,
/// <see cref="ValidateErrorMessages" /> and <see cref="ValidateWarningMessages" />.
/// </summary>
public abstract partial class AbstractFormItem : TemplatedControl, IFormItem
{
    /// <summary>
    /// Identifies the <see cref="ValidateChanged" /> routed event.
    /// </summary>
    public static readonly RoutedEvent<FormItemValidateChangedEventArgs> ValidateChangedEvent =
        RoutedEvent.Register<AbstractFormItem, FormItemValidateChangedEventArgs>(nameof(ValidateChanged), RoutingStrategies.Bubble | RoutingStrategies.Tunnel);

    /// <summary>
    /// Identifies the <c>ValueChanged</c> routed event, which bubbles up from the item each time the content control
    /// reports a value change while the item belongs to a form.
    /// </summary>
    internal static readonly RoutedEvent<RoutedEventArgs> ValueChangedEvent =
        RoutedEvent.Register<AbstractFormItem, RoutedEventArgs>("ValueChanged", RoutingStrategies.Bubble);

    /// <summary>
    /// Identifies the <see cref="Dependencies" /> property.
    /// </summary>
    public static readonly StyledProperty<IReadOnlyList<string>?> DependenciesProperty =
        AvaloniaProperty.Register<AbstractFormItem, IReadOnlyList<string>?>(nameof(Dependencies));

    /// <summary>
    /// Identifies the <see cref="IsTouched" /> property.
    /// </summary>
    public static readonly DirectProperty<AbstractFormItem, bool> IsTouchedProperty =
        AvaloniaProperty.RegisterDirect<AbstractFormItem, bool>(nameof(IsTouched), item => item.IsTouched);

    /// <summary>
    /// Identifies the <see cref="IsValidating" /> property.
    /// </summary>
    public static readonly DirectProperty<AbstractFormItem, bool> IsValidatingProperty =
        AvaloniaProperty.RegisterDirect<AbstractFormItem, bool>(nameof(IsValidating), item => item.IsValidating);

    /// <summary>
    /// Identifies the <see cref="ValidateErrorMessages" /> property.
    /// </summary>
    public static readonly DirectProperty<AbstractFormItem, IList<string>?> ValidateErrorMessagesProperty =
        AvaloniaProperty.RegisterDirect<AbstractFormItem, IList<string>?>(nameof(ValidateErrorMessages), item => item.ValidateErrorMessages);

    /// <summary>
    /// Identifies the <see cref="ValidateWarningMessages" /> property.
    /// </summary>
    public static readonly DirectProperty<AbstractFormItem, IList<string>?> ValidateWarningMessagesProperty =
        AvaloniaProperty.RegisterDirect<AbstractFormItem, IList<string>?>(nameof(ValidateWarningMessages), item => item.ValidateWarningMessages);

    /// <summary>
    /// Identifies the <see cref="ValidateStatus" /> property.
    /// </summary>
    public static readonly DirectProperty<AbstractFormItem, FormValidateStatus> ValidateStatusProperty =
        AvaloniaProperty.RegisterDirect<AbstractFormItem, FormValidateStatus>(nameof(ValidateStatus), owner => owner.ValidateStatus);

    /// <summary>
    /// Identifies the <see cref="ValidateResult" /> property.
    /// </summary>
    public static readonly DirectProperty<AbstractFormItem, FormValidateResult> ValidateResultProperty =
        AvaloniaProperty.RegisterDirect<AbstractFormItem, FormValidateResult>(nameof(ValidateResult), owner => owner.ValidateResult);

    /// <summary>
    /// Identifies the <see cref="Extra" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> ExtraProperty =
        AvaloniaProperty.Register<AbstractFormItem, object?>(nameof(Extra));

    /// <summary>
    /// Identifies the <see cref="ExtraTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> ExtraTemplateProperty =
        AvaloniaProperty.Register<AbstractFormItem, IDataTemplate?>(nameof(ExtraTemplate));

    /// <summary>
    /// Identifies the <see cref="Help" /> property.
    /// </summary>
    public static readonly StyledProperty<string?> HelpProperty =
        AvaloniaProperty.Register<AbstractFormItem, string?>(nameof(Help));

    /// <summary>
    /// Identifies the <see cref="IsValidateFeedbackEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsValidateFeedbackEnabledProperty =
        AbstractForm.IsValidateFeedbackEnabledProperty.AddOwner<AbstractFormItem>();

    /// <summary>
    /// Identifies the <see cref="InitialValue" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> InitialValueProperty =
        AvaloniaProperty.Register<AbstractFormItem, object?>(nameof(InitialValue));

    /// <summary>
    /// Identifies the <see cref="LabelText" /> property.
    /// </summary>
    public static readonly StyledProperty<string?> LabelTextProperty =
        AvaloniaProperty.Register<AbstractFormItem, string?>(nameof(LabelText));

    /// <summary>
    /// Identifies the <see cref="FieldName" /> property.
    /// </summary>
    public static readonly StyledProperty<string?> FieldNameProperty =
        AvaloniaProperty.Register<AbstractFormItem, string?>(nameof(FieldName));

    /// <summary>
    /// Identifies the <see cref="IsRequired" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsRequiredProperty =
        AvaloniaProperty.Register<AbstractFormItem, bool>(nameof(IsRequired));

    /// <summary>
    /// Identifies the <see cref="Validators" /> property.
    /// </summary>
    public static readonly StyledProperty<IList<IFormValidator>?> ValidatorsProperty =
        AvaloniaProperty.Register<AbstractFormItem, IList<IFormValidator>?>(nameof(Validators));

    /// <summary>
    /// Identifies the <see cref="ValidateTrigger" /> property.
    /// </summary>
    public static readonly StyledProperty<FormValidateTrigger> ValidateTriggerProperty =
        AbstractForm.ValidateTriggerProperty.AddOwner<AbstractFormItem>();

    /// <summary>
    /// Identifies the <see cref="ValidateDebounce" /> property.
    /// </summary>
    public static readonly StyledProperty<TimeSpan> ValidateDebounceProperty =
        AvaloniaProperty.Register<AbstractFormItem, TimeSpan>(nameof(ValidateDebounce), TimeSpan.Zero);

    /// <summary>
    /// Identifies the <see cref="ValidateStrategy" /> property.
    /// </summary>
    public static readonly StyledProperty<FormValidateStrategy> ValidateStrategyProperty =
        AvaloniaProperty.Register<AbstractFormItem, FormValidateStrategy>(nameof(ValidateStrategy), FormValidateStrategy.StopWhenFirstFailed);

    /// <summary>
    /// Identifies the <see cref="Layout" /> property.
    /// </summary>
    public static readonly StyledProperty<FormItemLayout> LayoutProperty =
        AvaloniaProperty.Register<AbstractFormItem, FormItemLayout>(nameof(Layout), FormItemLayout.Horizontal);

    /// <summary>
    /// Identifies the <see cref="Content" /> property.
    /// </summary>
    public static readonly StyledProperty<Control?> ContentProperty =
        AvaloniaProperty.Register<AbstractFormItem, Control?>(nameof(Content));

    /// <summary>
    /// Identifies the <see cref="IsValidateContentType" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsValidateContentTypeProperty =
        AvaloniaProperty.Register<AbstractFormItem, bool>(nameof(IsValidateContentType), true);

    /// <summary>
    /// Identifies the <see cref="IsValueItem" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsValueItemProperty =
        AvaloniaProperty.Register<AbstractFormItem, bool>(nameof(IsValueItem), true);

    /// <summary>
    /// Identifies the <see cref="RequiredMark" /> property.
    /// </summary>
    public static readonly StyledProperty<FormRequiredMark> RequiredMarkProperty =
        AbstractForm.RequiredMarkProperty.AddOwner<AbstractFormItem>();

    /// <summary>
    /// Identifies the <see cref="FormLayout" /> property.
    /// </summary>
    public static readonly StyledProperty<FormLayout> FormLayoutProperty =
        AvaloniaProperty.Register<AbstractFormItem, FormLayout>(nameof(FormLayout));

    /// <summary>
    /// Gets or sets extra content that the item shows in addition to the value control.
    /// When it is <see langword="null" />, the default, the item shows no extra content.
    /// </summary>
    public object? Extra
    {
        get => GetValue(ExtraProperty);
        set => SetValue(ExtraProperty, value);
    }

    /// <summary>
    /// Gets or sets the template used to present the <see cref="Extra" /> content.
    /// When it is <see langword="null" />, the default, the content is presented without a template.
    /// </summary>
    public IDataTemplate? ExtraTemplate
    {
        get => GetValue(ExtraTemplateProperty);
        set => SetValue(ExtraTemplateProperty, value);
    }

    /// <summary>
    /// Gets or sets the help text of the item. It is shown whatever the validation state is.
    /// When it is <see langword="null" />, the default, no help text is shown.
    /// </summary>
    public string? Help
    {
        get => GetValue(HelpProperty);
        set => SetValue(HelpProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the item hands its content a feedback control that visualizes the validation state.
    /// It applies only to <see cref="Content" /> that implements <see cref="IFormItemFeedbackAware" />.
    /// The default is <see langword="false" />. A value set on the item overrides the value of the owner form.
    /// </summary>
    public bool IsValidateFeedbackEnabled
    {
        get => GetValue(IsValidateFeedbackEnabledProperty);
        set => SetValue(IsValidateFeedbackEnabledProperty, value);
    }

    /// <summary>
    /// Gets or sets the value written to the content control when a content control is set and when
    /// <see cref="ResetItemValue()" /> restores the item.
    /// While it is not set, the default, a new content control keeps its own value and resetting clears the value.
    /// An initial value that the owner form holds for <see cref="FieldName" /> takes precedence over this value when
    /// the content is set and when the owner form is reset.
    /// </summary>
    public object? InitialValue
    {
        get => GetValue(InitialValueProperty);
        set => SetValue(InitialValueProperty, value);
    }

    /// <summary>
    /// Gets or sets the label text of the item.
    /// When it is <see langword="null" />, the default, the item shows no label.
    /// Default validation messages name the field by this text and fall back to <see cref="FieldName" />.
    /// </summary>
    public string? LabelText
    {
        get => GetValue(LabelTextProperty);
        set => SetValue(LabelTextProperty, value);
    }

    /// <summary>
    /// Gets or sets the name of the field that the item edits.
    /// The name is the key of the item's value in the owner form, the name passed to <see cref="Validators" />,
    /// and the field name in the validation report.
    /// The item is a field, see <see cref="IsField" />, only while the name is neither
    /// <see langword="null" /> nor blank.
    /// </summary>
    public string? FieldName
    {
        get => GetValue(FieldNameProperty);
        set => SetValue(FieldNameProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the field of the item is required.
    /// Whether the label shows a mark for that follows <see cref="RequiredMark" />, and marks are shown only on
    /// items whose <see cref="IsValueItem" /> is <see langword="true" />.
    /// </summary>
    public bool IsRequired
    {
        get => GetValue(IsRequiredProperty);
        set => SetValue(IsRequiredProperty, value);
    }

    /// <summary>
    /// Gets or sets the validators that the item runs on the value of <see cref="Content" />.
    /// When it is <see langword="null" /> or empty, the default, a validation reports success.
    /// </summary>
    public IList<IFormValidator>? Validators
    {
        get => GetValue(ValidatorsProperty);
        set => SetValue(ValidatorsProperty, value);
    }

    /// <summary>
    /// Gets or sets when the item starts a validation on its own.
    /// <see cref="FormValidateTrigger.OnChanged" />, the default, validates after the value changes, waiting
    /// <see cref="ValidateDebounce" /> first. <see cref="FormValidateTrigger.OnBlur" /> validates when keyboard focus
    /// leaves the item. <see cref="FormValidateTrigger.OnSubmit" /> starts no validation itself; use
    /// <see cref="ValidateValueAsync" /> instead.
    /// Both automatic triggers need an owner form and are suppressed while a value is written through
    /// <see cref="SetItemValue" /> or by the owner form.
    /// A value set on the item overrides the value of the owner form.
    /// </summary>
    public FormValidateTrigger ValidateTrigger
    {
        get => GetValue(ValidateTriggerProperty);
        set => SetValue(ValidateTriggerProperty, value);
    }

    /// <summary>
    /// Gets or sets how long the item waits after a value change before it validates.
    /// It applies only with <see cref="FormValidateTrigger.OnChanged" />.
    /// <see cref="TimeSpan.Zero" />, the default, validates without waiting.
    /// Every further value change restarts the wait.
    /// </summary>
    public TimeSpan ValidateDebounce
    {
        get => GetValue(ValidateDebounceProperty);
        set => SetValue(ValidateDebounceProperty, value);
    }

    /// <summary>
    /// Gets or sets how the item runs <see cref="Validators" />.
    /// <see cref="FormValidateStrategy.StopWhenFirstFailed" />, the default, runs them one after another and stops at
    /// the first one that fails. <see cref="FormValidateStrategy.Sequential" /> runs all of them one after another.
    /// <see cref="FormValidateStrategy.Parallel" /> starts them all at once.
    /// </summary>
    public FormValidateStrategy ValidateStrategy
    {
        get => GetValue(ValidateStrategyProperty);
        set => SetValue(ValidateStrategyProperty, value);
    }

    /// <summary>
    /// Gets or sets how the label and the value control of the item are arranged.
    /// <see cref="FormItemLayout.Horizontal" />, the default, places them side by side, and
    /// <see cref="FormItemLayout.Vertical" /> stacks them.
    /// A value set on the item overrides the layout that the item takes over from the owner form.
    /// </summary>
    public FormItemLayout Layout
    {
        get => GetValue(LayoutProperty);
        set => SetValue(LayoutProperty, value);
    }

    /// <summary>
    /// Gets or sets the value control of the item.
    /// The control must implement <see cref="IFormItemAware" /> so that the item can read and write its value.
    /// A control that does not is rejected when it is added, unless <see cref="IsValidateContentType" /> is
    /// <see langword="false" />.
    /// When it is <see langword="null" />, the default, <see cref="GetItemValue" /> returns
    /// <see langword="null" /> and <see cref="SetItemValue" /> does nothing.
    /// </summary>
    public Control? Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the item requires <see cref="Content" /> to implement <see cref="IFormItemAware" />.
    /// When it is <see langword="true" />, the default, adding other content throws an
    /// <see cref="InvalidOperationException" />. When it is <see langword="false" />, any control is accepted,
    /// but the item cannot read or write the value of a control that is not <see cref="IFormItemAware" />.
    /// </summary>
    public bool IsValidateContentType
    {
        get => GetValue(IsValidateContentTypeProperty);
        set => SetValue(IsValidateContentTypeProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the item is a field that the owner form takes into account.
    /// The owner form judges its validity by such items only, and the theme shows required marks only on them.
    /// The item updates this value from <see cref="IsField" /> whenever <see cref="Content" />,
    /// <see cref="FieldName" /> or <see cref="LabelText" /> changes.
    /// </summary>
    public bool IsValueItem
    {
        get => GetValue(IsValueItemProperty);
        set => SetValue(IsValueItemProperty, value);
    }

    /// <summary>
    /// Gets or sets how the label of the item marks required and optional fields.
    /// The mark is shown only on items whose <see cref="IsValueItem" /> is <see langword="true" />.
    /// <see cref="FormRequiredMark.Default" /> and <see cref="FormRequiredMark.RequiredText" /> mark items with
    /// <see cref="IsRequired" /> set to <see langword="true" />, and <see cref="FormRequiredMark.Optional" /> marks
    /// items with <see cref="IsRequired" /> set to <see langword="false" />.
    /// A value set on the item overrides the value of the owner form.
    /// </summary>
    public FormRequiredMark RequiredMark
    {
        get => GetValue(RequiredMarkProperty);
        set => SetValue(RequiredMarkProperty, value);
    }

    /// <summary>
    /// Gets or sets the layout of the owner form, mirrored on the item.
    /// A value set on the item overrides the value that the item takes over from the form.
    /// </summary>
    public FormLayout FormLayout
    {
        get => GetValue(FormLayoutProperty);
        set => SetValue(FormLayoutProperty, value);
    }

    private FormValidateStatus _validateStatus;
    private FormValidateStatus _settledStatus;
    private FormValidateResult _validateResult;
    private IList<string>? _validateErrorMessages;
    private IList<string>? _validateWarningMessages;
    private bool _isTouched;
    private bool _isValidating;
    private bool _isUpdatingErrors;
    private bool _hasPendingErrorRefresh;
    private bool _hasValidated;
    private bool _observesContent;
    private object? _lastValue;
    private string[]? _externalErrors;
    private string[]? _validationErrors;
    private IDisposable? _validationDebounce;
    private CancellationTokenSource? _validationRun;
    private CancellationTokenSource? _silentValidationRun;
    private long _guardedEpoch;
    private bool _guardedWrite;

    internal AbstractForm? OwnerForm { get; set; }
    internal bool CanValidate => Content is IFormItemAware || Validators is { Count: > 0 };

    /// <summary>
    /// Gets or sets the validation status of the item.
    /// While a validation runs the status is <see cref="FormValidateStatus.Validating" />. Otherwise it is
    /// <see cref="FormValidateStatus.Error" /> while <see cref="ValidateErrorMessages" /> has entries, or else the
    /// status that the last finished validation produced. Before the first one and after a reset it is
    /// <see cref="FormValidateStatus.Default" />.
    /// The item assigns the status again whenever it refreshes its validation state, so a value set from outside
    /// lasts only until the next refresh.
    /// </summary>
    public FormValidateStatus ValidateStatus
    {
        get => _validateStatus;
        set => SetAndRaise(ValidateStatusProperty, ref _validateStatus, value);
    }

    /// <summary>
    /// Gets the validation result derived from <see cref="ValidateStatus" />.
    /// <see cref="FormValidateResult.Error" /> and <see cref="FormValidateResult.Warning" /> follow the status.
    /// Every other status, including <see cref="FormValidateStatus.Validating" />, is reported as
    /// <see cref="FormValidateResult.Success" />.
    /// </summary>
    public FormValidateResult ValidateResult
    {
        get => _validateResult;
        private set => SetAndRaise(ValidateResultProperty, ref _validateResult, value);
    }

    /// <summary>
    /// Gets the error messages currently reported for the item, or <see langword="null" /> when there are none.
    /// They are read back from <see cref="Content" /> and include the failures of the last finished validation and
    /// the errors set on the item from outside its own validation.
    /// While there are entries the status is <see cref="FormValidateStatus.Error" />.
    /// </summary>
    public IList<string>? ValidateErrorMessages
    {
        get => _validateErrorMessages;
        private set => SetAndRaise(ValidateErrorMessagesProperty, ref _validateErrorMessages, value);
    }

    /// <summary>
    /// Gets the warning messages of the last finished validation, or <see langword="null" /> when there are none.
    /// Warnings do not make the owner form invalid. <see cref="ResetItemValue()" /> clears them.
    /// </summary>
    public IList<string>? ValidateWarningMessages
    {
        get => _validateWarningMessages;
        private set => SetAndRaise(ValidateWarningMessagesProperty, ref _validateWarningMessages, value);
    }

    /// <summary>
    /// Gets whether the value of the item was changed by the user since the item was created or last reset by
    /// <see cref="ResetItemValue()" />.
    /// Values written through <see cref="SetItemValue" /> or by the owner form do not count.
    /// The flag is tracked only while the item belongs to a form.
    /// </summary>
    public bool IsTouched => _isTouched;
    /// <summary>
    /// Gets whether a validation that will update the state of the item is running.
    /// It is <see langword="true" /> from the moment the validators start until they finish or the validation is
    /// canceled by <see cref="CancelValidation" />.
    /// </summary>
    public bool IsValidating => _isValidating;

    /// <summary>
    /// Gets or sets the field names of the items whose user changes revalidate this item.
    /// When such an item changes by user input and this item has been validated before, it validates at once,
    /// regardless of <see cref="ValidateTrigger" /> and <see cref="ValidateDebounce" />.
    /// When it is <see langword="null" /> or empty, the default, no item triggers this item.
    /// </summary>
    public IReadOnlyList<string>? Dependencies
    {
        get => GetValue(DependenciesProperty);
        set => SetValue(DependenciesProperty, value);
    }

    /// <summary>
    /// Occurs when a validation of the item has finished and updated the validation state of the item.
    /// It is raised after <see cref="ValidateStatus" />, <see cref="ValidateResult" /> and the messages are committed,
    /// and carries the new status. It is not raised for a validation that was canceled or replaced by a newer one.
    /// </summary>
    public event EventHandler<FormItemValidateChangedEventArgs>? ValidateChanged
    {
        add => AddHandler(ValidateChangedEvent, value);
        remove => RemoveHandler(ValidateChangedEvent, value);
    }

    /// <summary>
    /// Gets whether the item is a field, which it is while <see cref="FieldName" /> is neither
    /// <see langword="null" /> nor blank.
    /// </summary>
    /// <remarks>
    /// The result is copied into <see cref="IsValueItem" /> when the content, the field name or the label changes.
    /// An override should call the base getter to keep requiring a field name.
    /// </remarks>
    protected virtual bool IsField => !string.IsNullOrWhiteSpace(FieldName);

    /// <summary>
    /// Determines whether <c>AbstractForm</c> skips this item when it validates its items; <c>FormItem</c> in
    /// AtomUI.Desktop.Controls overrides it.
    /// </summary>
    /// <remarks>
    /// The form skips the item when this returns <see langword="true" /> and the form does not validate hidden
    /// items. The base implementation returns <see langword="true" /> while the item is not visible.
    /// </remarks>
    internal virtual bool IsSkipValidate() => !IsVisible;

    /// <summary>
    /// Called when the owner form reports that the value of an item of the form changed.
    /// </summary>
    /// <param name="formItem">
    /// The item whose value changed.
    /// </param>
    /// <remarks>
    /// The default implementation does nothing. It is called before this item revalidates itself because
    /// <see cref="Dependencies" /> names the changed item.
    /// </remarks>
    protected internal virtual void NotifyFormItemChanged(IFormItem formItem)
    {
    }

    /// <summary>
    /// Called after the item refreshed its validation state, that is after <see cref="ValidateStatus" />,
    /// <see cref="ValidateResult" /> and <see cref="ValidateErrorMessages" /> were assigned.
    /// </summary>
    /// <remarks>
    /// The default implementation does nothing. The call is repeated on every refresh, even when no value changed.
    /// </remarks>
    protected virtual void OnValidationMessagesChanged()
    {
    }

    /// <summary>
    /// Called when <see cref="Content" /> changes. It disconnects the item from the previous control and connects
    /// the new one.
    /// </summary>
    /// <param name="change">
    /// The change of the <see cref="Content" /> property.
    /// </param>
    /// <exception cref="InvalidOperationException">
    /// The new content does not implement <see cref="IFormItemAware" /> while <see cref="IsValidateContentType" />
    /// is <see langword="true" />.
    /// </exception>
    /// <remarks>
    /// An override must call the base member. It cancels running validations, observes the new control, applies
    /// <see cref="InitialValue" />, writes the stored errors to the control and refreshes the validation state.
    /// </remarks>
    protected virtual void NotifyContentAdded(AvaloniaPropertyChangedEventArgs change)
    {
        CancelValidation();
        if (change.OldValue is Control oldControl)
        {
            StopObservingContent(oldControl);
            FormDataValidationErrors.ClearFormErrors(oldControl);
            if (oldControl is IFormItemFeedbackAware feedbackAware)
            {
                feedbackAware.SetFeedbackControl(null);
            }
        }

        if (change.NewValue is Control content)
        {
            if (content is not IFormItemAware && IsValidateContentType)
            {
                throw new InvalidOperationException($"Form item content must implement {nameof(IFormItemAware)}.");
            }
            ObserveContent(content);
            if (IsSet(InitialValueProperty) && OwnerForm?.InitialValues?.ContainsKey(FieldName ?? string.Empty) != true)
            {
                SetItemValue(InitialValue);
            }
            SetStoredErrors(content);
        }

        SetCurrentValue(IsValueItemProperty, IsField);
        RefreshEffectiveState();
        OwnerForm?.OnFieldIdentityChanged(this);
    }

    /// <summary>
    /// Called when a property of the item changes.
    /// </summary>
    /// <remarks>
    /// An override must call the base member. Besides what the base class does, it hands a change of
    /// <see cref="Content" /> to <see cref="NotifyContentAdded" />, and updates <see cref="IsValueItem" /> and
    /// informs the owner form when <see cref="FieldName" /> or <see cref="LabelText" /> changes.
    /// </remarks>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ContentProperty)
        {
            NotifyContentAdded(change);
        }
        else if (change.Property == FieldNameProperty || change.Property == LabelTextProperty)
        {
            SetCurrentValue(IsValueItemProperty, IsField);
            OwnerForm?.OnFieldIdentityChanged(this);
        }
    }

    /// <summary>
    /// Called after the item is attached to the visual tree.
    /// </summary>
    /// <remarks>
    /// An override must call the base member. It observes <see cref="Content" /> again, writes the stored errors
    /// to it and refreshes the validation state, because a detached item does not observe its content.
    /// </remarks>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if (Content is { } content)
        {
            ObserveContent(content);
            SetStoredErrors(content);
            RefreshEffectiveState();
        }
    }

    /// <summary>
    /// Called when the item is detached from the visual tree.
    /// </summary>
    /// <remarks>
    /// An override must call the base member. It cancels running validations and stops observing
    /// <see cref="Content" />.
    /// </remarks>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        CancelValidation();
        if (Content is { } content)
        {
            StopObservingContent(content);
        }
        base.OnDetachedFromVisualTree(e);
    }

    /// <summary>
    /// Called when the item loses keyboard focus.
    /// </summary>
    /// <remarks>
    /// An override must call the base member. It validates the item without delay when
    /// <see cref="ValidateTrigger" /> is <see cref="FormValidateTrigger.OnBlur" /> and keyboard focus is no longer
    /// within the item. It skips that while the item has no owner form or a value is being written through
    /// <see cref="SetItemValue" /> or by the owner form.
    /// </remarks>
    protected override void OnLostFocus(FocusChangedEventArgs e)
    {
        base.OnLostFocus(e);
        if (OwnerForm is { IsWritingValues: false } && !_guardedWrite &&
            ValidateTrigger == FormValidateTrigger.OnBlur && !IsKeyboardFocusWithin)
        {
            ScheduleValidation(TimeSpan.Zero);
        }
    }

    internal void SetOwner(AbstractForm? owner)
    {
        if (ReferenceEquals(OwnerForm, owner))
        {
            return;
        }
        if (OwnerForm is not null)
        {
            CancelValidation();
        }
        OwnerForm = owner;
    }

    internal void SynchronizeWithForm(AbstractForm form)
    {
        SetValue(RequiredMarkProperty, form.RequiredMark, BindingPriority.Style);
        SetValue(FormLayoutProperty, form.FormLayout, BindingPriority.Style);
        SetValue(ValidateTriggerProperty, form.ValidateTrigger, BindingPriority.Style);
        SetValue(IsValidateFeedbackEnabledProperty, form.IsValidateFeedbackEnabled, BindingPriority.Style);
        var layout = form.FormLayout == FormLayout.Vertical ? FormItemLayout.Vertical : FormItemLayout.Horizontal;
        SetValue(LayoutProperty, layout, BindingPriority.Style);
    }

    internal void NotifyOwnerFieldChanged(AbstractFormItem item, bool userChange)
    {
        NotifyFormItemChanged(item);
        if (userChange && _hasValidated && item.FieldName is { } field && Dependencies?.Contains(field) == true)
        {
            ScheduleValidation(TimeSpan.Zero);
        }
    }

    /// <summary>
    /// Returns the value of <see cref="Content" />. The result is <see langword="null" /> when there is no content
    /// or when the content does not implement <see cref="IFormItemAware" />.
    /// </summary>
    public object? GetItemValue() => (Content as IFormItemAware)?.GetFormValue();

    /// <summary>
    /// Writes a value to <see cref="Content" />.
    /// </summary>
    /// <remarks>
    /// Nothing happens when there is no content or the content does not implement <see cref="IFormItemAware" />.
    /// The write counts as a programmatic change: it does not set <see cref="IsTouched" />, starts no validation and
    /// is reported to the owner form as not made by the user. The validation state is left as it is.
    /// </remarks>
    public void SetItemValue(object? value)
    {
        if (Content is IFormItemAware content)
        {
            BeginGuardedWrite();
            content.SetFormValue(value);
        }
    }

    /// <summary>
    /// Restores the value and the validation state of the item.
    /// </summary>
    /// <remarks>
    /// <see cref="Content" /> receives <see cref="InitialValue" />, or is cleared while <see cref="InitialValue" />
    /// is not set. The write starts no validation. Running validations are canceled, <see cref="IsTouched" /> is
    /// reset, the errors and warnings are dropped, including errors set from outside, and the status returns to
    /// <see cref="FormValidateStatus.Default" />.
    /// </remarks>
    public void ResetItemValue() => ResetItemValue(IsSet(InitialValueProperty), InitialValue);

    internal void ResetItemValue(bool hasInitial, object? initial)
    {
        CancelValidation();
        BeginGuardedWrite();
        if (Content is IFormItemAware content)
        {
            if (hasInitial)
            {
                content.SetFormValue(initial);
            }
            else if (IsSet(InitialValueProperty))
            {
                content.SetFormValue(InitialValue);
            }
            else
            {
                content.ClearFormValue();
            }
        }
        _hasValidated = false;
        SetAndRaise(IsTouchedProperty, ref _isTouched, false);
        _externalErrors = _validationErrors = null;
        if (Content is { } target)
        {
            SetStoredErrors(target);
        }
        ValidateWarningMessages = null;
        _settledStatus = FormValidateStatus.Default;
        RefreshEffectiveState();
    }

    private void BeginGuardedWrite()
    {
        _guardedWrite = true;
        var version = ++_guardedEpoch;
        Dispatcher.Post(() =>
        {
            if (version == _guardedEpoch)
            {
                _guardedWrite = false;
            }
        }, Avalonia.Threading.DispatcherPriority.Background);
    }

    private void ObserveContent(Control content)
    {
        if (_observesContent)
        {
            return;
        }
        _observesContent = true;
        _lastValue = GetItemValue();
        content.PropertyChanged += HandleContentPropertyChanged;
        if (content is IFormItemAware aware)
        {
            aware.ValueChanged += HandleContentValueChanged;
        }
    }

    private void StopObservingContent(Control content)
    {
        _observesContent = false;
        content.PropertyChanged -= HandleContentPropertyChanged;
        if (content is IFormItemAware aware)
        {
            aware.ValueChanged -= HandleContentValueChanged;
        }
    }

    private void HandleContentPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs args)
    {
        if (args.Property != DataValidationErrors.ErrorsProperty || _isUpdatingErrors || _hasPendingErrorRefresh)
        {
            return;
        }

        _hasPendingErrorRefresh = true;
        Dispatcher.Post(() =>
        {
            _hasPendingErrorRefresh = false;
            if (_observesContent && Content is { } target)
            {
                SetStoredErrors(target);
                RefreshEffectiveState();
            }
        });
    }

    private void HandleContentValueChanged(object? sender, EventArgs e)
    {
        var value = GetItemValue();
        var changed = !Equals(_lastValue, value);
        _lastValue = value;
        if (OwnerForm is not { } owner)
        {
            return;
        }

        var canValidate = !_guardedWrite && !owner.IsWritingValues;
        var userChange = changed && canValidate;
        if (userChange)
        {
            SetAndRaise(IsTouchedProperty, ref _isTouched, true);
        }
        if (canValidate && ValidateTrigger == FormValidateTrigger.OnChanged)
        {
            ScheduleValidation(ValidateDebounce);
        }
        owner.OnFieldValueChanged(this, userChange);
        RaiseEvent(new RoutedEventArgs(ValueChangedEvent, this));
    }

    internal void SetExternalErrors(string[]? messages)
    {
        if (ReferenceEquals(_externalErrors, messages))
        {
            return;
        }
        _externalErrors = messages;
        if (Content is { } target)
        {
            SetStoredErrors(target);
        }
        RefreshEffectiveState();
    }

    private void SetStoredErrors(Control target)
    {
        _isUpdatingErrors = true;
        try
        {
            FormDataValidationErrors.Write(target, _externalErrors, _validationErrors);
        }
        finally
        {
            _isUpdatingErrors = false;
        }
    }

    private void RefreshEffectiveState()
    {
        var messages = Content is { } target ? FormDataValidationErrors.GetErrorMessages(target) : [];
        ValidateErrorMessages = messages.Count == 0 ? null : messages;
        var status = messages.Count > 0 ? FormValidateStatus.Error : _settledStatus;
        ValidateResult = StatusToResult(status);
        ValidateStatus = IsValidating ? FormValidateStatus.Validating : status;
        PseudoClasses.Set(":has-error", messages.Count > 0);
        (Content as IFormItemAware)?.NotifyValidateStatus(ValidateStatus);
        OnValidationMessagesChanged();
        OwnerForm?.UpdateFormValidity();
    }

    private static FormValidateResult StatusToResult(FormValidateStatus status) => status switch
    {
        FormValidateStatus.Error => FormValidateResult.Error,
        FormValidateStatus.Warning => FormValidateResult.Warning,
        _ => FormValidateResult.Success
    };

    private void ScheduleValidation(TimeSpan debounce)
    {
        CancelRun(isSilent: false);
        if (debounce > TimeSpan.Zero)
        {
            _validationDebounce = DispatcherTimer.RunOnce(() =>
            {
                _validationDebounce = null;
                Dispatcher.InvokeAsync(async () => await ValidateValueAsync(CancellationToken.None));
            }, debounce);
        }
        else
        {
            var tokenSource = new CancellationTokenSource();
            _validationRun = tokenSource;
            Dispatcher.InvokeAsync(async () =>
            {
                if (!ReferenceEquals(_validationRun, tokenSource))
                {
                    tokenSource.Dispose();
                    return;
                }
                await RunFieldAsync(tokenSource, isSilent: false);
            });
        }
    }

    /// <summary>
    /// Validates the current value of the item and updates the validation state with the outcome.
    /// </summary>
    /// <param name="cancellationToken">
    /// Cancels the validation.
    /// </param>
    /// <returns>
    /// A task that completes when the validators have finished.
    /// </returns>
    /// <remarks>
    /// <see cref="ValidateChanged" /> is raised before the task completes. A validation that is already running is
    /// canceled and its outcome is discarded. The outcome of this run is discarded as well when the token is
    /// canceled or a newer validation starts; the validation state then stays as it is.
    /// The method validates whatever <see cref="ValidateTrigger" /> is.
    /// </remarks>
    public async Task ValidateValueAsync(CancellationToken cancellationToken) =>
        await ValidateFieldAsync(isSilent: false, cancellationToken);

    internal Task<FormFieldValidateResult> ValidateFieldAsync(bool isSilent, CancellationToken cancellationToken)
    {
        CancelRun(isSilent);
        var run = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        if (isSilent)
        {
            _silentValidationRun = run;
        }
        else
        {
            _validationRun = run;
        }
        return RunFieldAsync(run, isSilent);
    }

    /// <summary>
    /// Cancels the validations of the item that are pending or running, including a pending debounce.
    /// </summary>
    /// <remarks>
    /// A canceled validation leaves the validation state as it was and does not raise
    /// <see cref="ValidateChanged" />. When a validation was running, <see cref="IsValidating" /> becomes
    /// <see langword="false" /> and the status returns to the last settled one.
    /// </remarks>
    public void CancelValidation()
    {
        CancelRun(isSilent: false);
        CancelRun(isSilent: true);
    }

    private void CancelRun(bool isSilent)
    {
        if (isSilent)
        {
            var run = _silentValidationRun;
            _silentValidationRun = null;
            run?.Cancel();
            return;
        }
        _validationDebounce?.Dispose();
        _validationDebounce = null;
        var regular = _validationRun;
        _validationRun = null;
        regular?.Cancel();
        if (_isValidating)
        {
            SetAndRaise(IsValidatingProperty, ref _isValidating, false);
            RefreshEffectiveState();
        }
    }

    private async Task<FormFieldValidateResult> RunFieldAsync(CancellationTokenSource run, bool isSilent)
    {
        var field = FieldName ?? string.Empty;
        var token = run.Token;
        try
        {
            if (!isSilent)
            {
                SetAndRaise(IsValidatingProperty, ref _isValidating, true);
                ValidateStatus = FormValidateStatus.Validating;
            }
            var (status, errors, warnings) = await ExecuteValidatorsAsync(GetItemValue(), token);
            if (token.IsCancellationRequested || !ReferenceEquals(run, isSilent ? _silentValidationRun : _validationRun))
            {
                return SnapshotResult(field);
            }

            var errorList = errors?.ToArray() ?? [];
            var warningList = warnings?.ToArray() ?? [];
            if (isSilent)
            {
                var intrinsic = FormDataValidationErrors.GetOtherErrorMessages(Content);
                var combined = intrinsic.Concat(errorList).ToArray();
                return new FormFieldValidateResult(field, combined.Length > 0 ? FormValidateResult.Error : StatusToResult(status),
                    combined, warningList);
            }

            _hasValidated = true;
            _validationErrors = errorList;
            ValidateWarningMessages = warningList.Length > 0 ? warningList : null;
            _settledStatus = status;
            SetAndRaise(IsValidatingProperty, ref _isValidating, false);
            if (Content is { } target)
            {
                SetStoredErrors(target);
            }
            RefreshEffectiveState();
            RaiseEvent(new FormItemValidateChangedEventArgs(ValidateStatus) { RoutedEvent = ValidateChangedEvent, Source = this });
            return SnapshotResult(field);
        }
        finally
        {
            if (ReferenceEquals(run, _validationRun))
            {
                _validationRun = null;
                if (_isValidating)
                {
                    SetAndRaise(IsValidatingProperty, ref _isValidating, false);
                    RefreshEffectiveState();
                }
            }
            if (ReferenceEquals(run, _silentValidationRun))
            {
                _silentValidationRun = null;
            }
            run.Dispose();
        }
    }

    private FormFieldValidateResult SnapshotResult(string field) =>
        new(field, ValidateResult, ValidateErrorMessages?.ToArray() ?? [], ValidateWarningMessages?.ToArray() ?? []);
}
