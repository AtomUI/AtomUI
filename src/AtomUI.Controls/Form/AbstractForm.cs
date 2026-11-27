using System.Collections;
using AtomUI.Desktop.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;

namespace AtomUI.Controls;

/// <summary>
/// Base class of controls that validate and submit a set of form items as one unit and expose their values keyed
/// by field name.
/// An item that is not itself an <see cref="AbstractFormItem" /> is wrapped in one created by
/// <see cref="CreateFormItem" />; only form items take part in values, validation and submit.
/// </summary>
public abstract class AbstractForm : ItemsControl, IForm
{
    /// <summary>
    /// Identifies the <see cref="RequiredMark" /> property.
    /// </summary>
    public static readonly StyledProperty<FormRequiredMark> RequiredMarkProperty =
        AvaloniaProperty.Register<AbstractForm, FormRequiredMark>(nameof(RequiredMark), FormRequiredMark.Default);

    /// <summary>
    /// Identifies the <see cref="IsValidateFeedbackEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsValidateFeedbackEnabledProperty =
        AvaloniaProperty.Register<AbstractForm, bool>(
            nameof(IsValidateFeedbackEnabled), false);

    /// <summary>
    /// Identifies the <see cref="FormLayout" /> property.
    /// </summary>
    public static readonly StyledProperty<FormLayout> FormLayoutProperty =
        AvaloniaProperty.Register<AbstractForm, FormLayout>(nameof(FormLayout));

    /// <summary>
    /// Identifies the <see cref="IsScrollToFirstErrorEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsScrollToFirstErrorEnabledProperty =
        AvaloniaProperty.Register<AbstractForm, bool>(nameof(IsScrollToFirstErrorEnabled));

    /// <summary>
    /// Identifies the <see cref="ValidateTrigger" /> property.
    /// </summary>
    public static readonly StyledProperty<FormValidateTrigger> ValidateTriggerProperty =
        AvaloniaProperty.Register<AbstractForm, FormValidateTrigger>(nameof(ValidateTrigger), FormValidateTrigger.OnChanged);

    /// <summary>
    /// Gets or sets the mark shown on the label of an item. <see cref="FormRequiredMark.Default" /> adds an asterisk
    /// to items with <see cref="AbstractFormItem.IsRequired" /> and <see cref="FormRequiredMark.RequiredText" /> the
    /// localized "required" text. <see cref="FormRequiredMark.Optional" /> adds the localized "optional" text to the
    /// items that are not required, <see cref="FormRequiredMark.Customize" /> shows the custom mark of the item and
    /// <see cref="FormRequiredMark.Hidden" /> shows nothing.
    /// The value is set on the form and copied to every item; a value set directly on an item takes precedence.
    /// </summary>
    public FormRequiredMark RequiredMark
    {
        get => GetValue(RequiredMarkProperty);
        set => SetValue(RequiredMarkProperty, value);
    }

    /// <summary>
    /// Gets or sets whether each item attaches a validate feedback to its content, an icon that shows the success,
    /// warning, error or validating state. The default is <see langword="false" />.
    /// The value is set on the form and copied to every item; a value set directly on an item takes precedence.
    /// </summary>
    public bool IsValidateFeedbackEnabled
    {
        get => GetValue(IsValidateFeedbackEnabledProperty);
        set => SetValue(IsValidateFeedbackEnabledProperty, value);
    }

    /// <summary>
    /// Gets or sets the layout every item receives: <see cref="FormLayout.Vertical" /> puts the label above the
    /// content, <see cref="FormLayout.Horizontal" /> and <see cref="FormLayout.Inline" /> put it beside the content.
    /// The default is <see cref="FormLayout.Horizontal" />. The value is set on the form and copied to every item; a
    /// value set directly on an item takes precedence.
    /// How the items are arranged among themselves, such as the single row of an inline form, is decided by the
    /// derived control.
    /// </summary>
    public FormLayout FormLayout
    {
        get => GetValue(FormLayoutProperty);
        set => SetValue(FormLayoutProperty, value);
    }

    /// <summary>
    /// Gets or sets whether <see cref="Submit" /> calls <see cref="RevealFirstError" /> for the first visible item in
    /// error when validation fails. The default is <see langword="false" />.
    /// </summary>
    public bool IsScrollToFirstErrorEnabled
    {
        get => GetValue(IsScrollToFirstErrorEnabledProperty);
        set => SetValue(IsScrollToFirstErrorEnabledProperty, value);
    }

    /// <summary>
    /// Gets or sets when an item is validated on its own: <see cref="FormValidateTrigger.OnChanged" /> after the user
    /// changes its value, <see cref="FormValidateTrigger.OnBlur" /> after its content loses focus, and
    /// <see cref="FormValidateTrigger.OnSubmit" /> never, which leaves
    /// <see cref="ValidateAsync(FormValidateOptions, CancellationToken)" />, <see cref="Submit" /> and the item's own
    /// <see cref="AbstractFormItem.ValidateValueAsync" /> as triggers.
    /// The default is <see cref="FormValidateTrigger.OnChanged" />. The value is set on the form and copied to every
    /// item; a value set directly on an item takes precedence.
    /// </summary>
    public FormValidateTrigger ValidateTrigger
    {
        get => GetValue(ValidateTriggerProperty);
        set => SetValue(ValidateTriggerProperty, value);
    }

    /// <summary>
    /// Identifies the <see cref="Values" /> property.
    /// </summary>
    public static readonly DirectProperty<AbstractForm, IFormValues?> ValuesProperty =
        AvaloniaProperty.RegisterDirect<AbstractForm, IFormValues?>(nameof(Values), owner => owner.Values);

    /// <summary>
    /// Identifies the <see cref="InitialValues" /> property.
    /// </summary>
    public static readonly DirectProperty<AbstractForm, IFormValues?> InitialValuesProperty =
        AvaloniaProperty.RegisterDirect<AbstractForm, IFormValues?>(nameof(InitialValues), owner => owner.InitialValues, (owner, value) => owner.InitialValues = value);

    /// <summary>
    /// Identifies the <see cref="IsFormValid" /> property.
    /// </summary>
    public static readonly DirectProperty<AbstractForm, bool?> IsFormValidProperty =
        AvaloniaProperty.RegisterDirect<AbstractForm, bool?>(nameof(IsFormValid), owner => owner.IsFormValid);

    private IFormValues? _values = new FormValues();
    private IFormValues? _initialValues;
    private bool? _isFormValid;
    private readonly HashSet<AbstractFormItem> _realizedItems = new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<string, string[]> _externalErrors = new(StringComparer.Ordinal);
    private readonly HashSet<AbstractFormItem> _initializedItems = new(ReferenceEqualityComparer.Instance);
    private CancellationTokenSource? _validationTokenSource;
    private CancellationTokenSource? _silentValidationTokenSource;
    private int _guardDepth;
    private long _validationVersion;

    protected AbstractForm()
    {
        ItemsView.CollectionChanged += (_, _) => ReconcileItems();
    }

    /// <summary>
    /// Gets the current values of the items keyed by field name. The property is replaced by a new snapshot whenever
    /// an item value changes or an item is added, removed or reordered. Items with a null or blank field name are
    /// left out.
    /// </summary>
    public IFormValues? Values => _values;

    /// <summary>
    /// Gets or sets the values keyed by field name that <see cref="Reset" /> restores the items to. An item whose
    /// field name is not a key restores its own initial value or is cleared.
    /// The value stored for an item is written into the item when it is attached to the form with content and a
    /// field name, and not again while it stays attached; a value assigned afterwards takes effect on the next
    /// reset only.
    /// </summary>
    public IFormValues? InitialValues
    {
        get => _initialValues;
        set => SetAndRaise(InitialValuesProperty, ref _initialValues, value);
    }

    /// <summary>
    /// Gets whether every item with <see cref="AbstractFormItem.IsValueItem" /> set to <see langword="true" /> has
    /// the validation status <see cref="FormValidateStatus.Success" /> or <see cref="FormValidateStatus.Warning" />.
    /// An item that is validating or was not validated yet makes the value <see langword="false" />.
    /// The value is <see langword="null" /> until the first validation state update, for example a finished
    /// validation or a call to <see cref="SetFieldErrors" /> or <see cref="ClearFieldErrors" />; <see cref="Reset" />
    /// sets it to <see langword="false" />.
    /// </summary>
    public bool? IsFormValid
    {
        get => _isFormValid;
        private set => SetAndRaise(IsFormValidProperty, ref _isFormValid, value);
    }

    /// <summary>
    /// Occurs when a validation started by <see cref="Validate" />,
    /// <see cref="ValidateAsync(FormValidateOptions, CancellationToken)" /> or <see cref="Submit" /> begins, before
    /// any item is validated. A silent validation does not raise it.
    /// </summary>
    public event EventHandler<EventArgs>? AboutToValidate;
    /// <summary>
    /// Occurs after a validation that is not silent finished and the item states were updated.
    /// <see cref="ValidateAsync(FormValidateOptions, CancellationToken)" /> raises it only for an
    /// <see cref="FormValidateResult.Error" /> result, with <c>Values</c> set to <see langword="null" /> and
    /// every error and warning message in <c>ErrorMessages</c>;
    /// <see cref="Submit" /> raises it for both results and passes the values on success.
    /// </summary>
    public event EventHandler<FormValidatedEventArgs>? Validated;
    /// <summary>
    /// Occurs when <see cref="Submit" /> passes validation, after <see cref="NotifySubmit" /> was called. The event
    /// args carry the values that were validated. A failed validation raises <see cref="SubmitFailed" /> instead, and
    /// a submission canceled by <see cref="CancelValidation" />, by a newer validation that is not silent or by
    /// detaching the form raises neither.
    /// </summary>
    public event EventHandler<FormSubmittedEventArgs>? Submitted;
    /// <summary>
    /// Occurs when <see cref="Submit" /> fails validation, after the item states were updated and before the first
    /// error is revealed. The event args are the validation report with the result and the messages of every field.
    /// </summary>
    public event EventHandler<FormValidateReport>? SubmitFailed;
    /// <summary>
    /// Occurs when an item's content reports a new value, after <see cref="Values" /> was refreshed. The event args
    /// carry the item and its new value. It is raised for any change the content reports, including the writes of
    /// <see cref="SetFormValues" />, and does not tell a user edit from a programmatic write.
    /// </summary>
    public event EventHandler<FormItemValueChangedEventArgs>? ItemValueChanged;
    /// <summary>
    /// Occurs at the end of <see cref="Reset" />, after the items were restored, <see cref="NotifyReset" /> was called
    /// and <see cref="IsFormValid" /> was set to <see langword="false" />.
    /// </summary>
    public event EventHandler? ResetCompleted;

    /// <summary>
    /// When overridden to return <see langword="true" />, a validation also covers the items that skip validation,
    /// which are the invisible items by default. The base class returns <see langword="false" />, so those items are
    /// left out of <see cref="ValidateAsync(FormValidateOptions, CancellationToken)" /> and their errors do not
    /// block <see cref="Submit" />.
    /// </summary>
    protected virtual bool ShouldValidateHiddenItems => false;

    internal bool IsWritingValues => _guardDepth > 0;

    /// <summary>
    /// Creates the form item that wraps an item which is not itself an <see cref="AbstractFormItem" />. It is called
    /// whenever such an item needs a container.
    /// An override must return a form item: any other control is displayed but takes no part in values, validation
    /// and submit.
    /// </summary>
    protected virtual AbstractFormItem CreateFormItem() => new DefaultFormItem();

    /// <summary>
    /// Called by <see cref="Submit" /> for the first visible item in error when
    /// <see cref="IsScrollToFirstErrorEnabled" /> is enabled. The base class brings the item into view; an override
    /// that wants the scrolling as well must call it.
    /// </summary>
    protected virtual void RevealFirstError(AbstractFormItem item) => item.BringIntoView();

    /// <summary>
    /// Returns the container for an item that needs one, which is an item that is not itself an
    /// <see cref="AbstractFormItem" />: the form item created by <see cref="CreateFormItem" />.
    /// An override must also return a form item.
    /// </summary>
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey) => CreateFormItem();

    /// <summary>
    /// An item that is an <see cref="AbstractFormItem" /> is its own container and needs none; every other item gets
    /// one from <see cref="CreateFormItem" />.
    /// </summary>
    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey) =>
        NeedsContainer<AbstractFormItem>(item, out recycleKey);

    /// <summary>
    /// Attaches a container that is an <see cref="AbstractFormItem" /> to the form after the base class prepared it.
    /// An attached item takes part in values, validation and submit. An override must call the base method;
    /// otherwise the form item stays detached.
    /// </summary>
    protected override void PrepareContainerForItemOverride(Control container, object? item, int index)
    {
        base.PrepareContainerForItemOverride(container, item, index);
        if (container is AbstractFormItem field)
        {
            _realizedItems.Add(field);
            AttachItem(field);
        }
    }

    /// <summary>
    /// Refreshes <see cref="Values" /> after the base class prepared the container, so that the values include the
    /// item. An override must call the base method.
    /// </summary>
    protected override void ContainerForItemPreparedOverride(Control container, object? item, int index)
    {
        base.ContainerForItemPreparedOverride(container, item, index);
        PublishValues();
    }

    /// <summary>
    /// Refreshes <see cref="Values" /> after the base class moved the container to another index. An override must
    /// call the base method.
    /// </summary>
    protected override void ContainerIndexChangedOverride(Control container, int oldIndex, int newIndex)
    {
        base.ContainerIndexChangedOverride(container, oldIndex, newIndex);
        PublishValues();
    }

    /// <summary>
    /// Detaches a container that is an <see cref="AbstractFormItem" /> from the form before the base class clears it,
    /// and refreshes <see cref="Values" /> afterwards so that the item's value is left out. An override must call the
    /// base method to keep the items and values of the form current.
    /// </summary>
    protected override void ClearContainerForItemOverride(Control container)
    {
        if (container is AbstractFormItem field)
        {
            _realizedItems.Remove(field);
            DetachItem(field);
        }
        base.ClearContainerForItemOverride(container);
        PublishValues();
    }

    /// <summary>
    /// After the base class applied the template, synchronizes the form items with the current items: items that are
    /// not attached are attached and items that were removed are dropped. An override must call the base method.
    /// </summary>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        ReconcileItems();
    }

    /// <summary>
    /// After the base class handled loading, synchronizes the form items with the current items in the same way as
    /// <see cref="OnApplyTemplate" />. An override must call the base method.
    /// </summary>
    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        ReconcileItems();
    }

    /// <summary>
    /// Cancels the pending validations and submissions before the base class handles detaching, so that a detached
    /// form raises no result event. An override must call the base method.
    /// </summary>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        CancelValidation();
        base.OnDetachedFromVisualTree(e);
    }

    /// <summary>
    /// After the base class handled the change, copies the new value to every form item when
    /// <see cref="RequiredMark" />, <see cref="FormLayout" />, <see cref="ValidateTrigger" /> or
    /// <see cref="IsValidateFeedbackEnabled" /> changed. An override must call the base method, or the items keep the
    /// old value.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == RequiredMarkProperty || change.Property == FormLayoutProperty ||
            change.Property == ValidateTriggerProperty || change.Property == IsValidateFeedbackEnabledProperty)
        {
            foreach (var item in GetFormItems())
            {
                item.SynchronizeWithForm(this);
            }
        }
    }

    /// <summary>
    /// Starts a validation of every item on the dispatcher and returns before it finishes. Its result is visible
    /// through the item states, <see cref="IsFormValid" /> and <see cref="Validated" />.
    /// A run canceled by a newer validation that is not silent, by <see cref="CancelValidation" /> or by detaching the
    /// form ends without raising an event.
    /// </summary>
    public void Validate()
    {
        Dispatcher.InvokeAsync(async () =>
        {
            try
            {
                await ValidateAsync(CancellationToken.None);
            }
            catch (OperationCanceledException)
            {
                // A newer request or detach canceled validation.
            }
        });
    }

    /// <summary>
    /// Validates every item and shows the result on the items. The task completes when the validation finished; it
    /// ends with <see cref="OperationCanceledException" /> when the run was canceled.
    /// </summary>
    /// <returns>
    /// <see cref="FormValidateResult.Error" /> when an item has an error; a warning does not count as an error.
    /// Otherwise <see cref="FormValidateResult.Success" />.
    /// </returns>
    public async Task<FormValidateResult> ValidateAsync(CancellationToken cancellationToken)
    {
        var report = await ValidateAsync(new FormValidateOptions(), cancellationToken);
        return report.Result;
    }

    /// <summary>
    /// Validates the selected items, all at the same time, and returns a report with the values and the result of
    /// every validated item.
    /// The task completes when the validation finished and, for a run that is not silent, the item states and
    /// <see cref="IsFormValid" /> are updated; <see cref="Validated" /> is raised before the task completes.
    /// </summary>
    /// <param name="options">
    /// Narrows the run to the given field names and decides whether it is silent. A silent run
    /// (<see cref="FormValidateOptions.IsSilent" />) computes the report without changing the item states, updating
    /// <see cref="IsFormValid" /> or raising <see cref="AboutToValidate" /> or <see cref="Validated" />.
    /// </param>
    /// <param name="cancellationToken">
    /// Cancels the run. The task then ends with <see cref="OperationCanceledException" />; items that finished before
    /// keep their new state.
    /// </param>
    /// <returns>
    /// The report: <see cref="FormValidateResult.Error" /> when a validated item has an error, otherwise
    /// <see cref="FormValidateResult.Success" />.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="options" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// The run was canceled through <paramref name="cancellationToken" />, by a newer run with the same silence
    /// setting, or by <see cref="CancelValidation" />.
    /// </exception>
    /// <remarks>
    /// An item takes part when its content implements <see cref="IFormItemAware" /> or it has validators, when it is
    /// not skipped as a hidden item (see <see cref="ShouldValidateHiddenItems" />), and when
    /// <see cref="FormValidateOptions.FieldNames" /> is <see langword="null" /> or contains its field name; an empty
    /// list selects no item.
    /// </remarks>
    public async Task<FormValidateReport> ValidateAsync(FormValidateOptions options, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);
        var report = await ValidateCoreAsync(options, cancellationToken);
        if (!options.IsSilent && report.Result == FormValidateResult.Error)
        {
            Validated?.Invoke(this, new FormValidatedEventArgs(report.Result, null, report.Messages.ToList()));
        }
        return report;
    }

    private async Task<FormValidateReport> ValidateCoreAsync(FormValidateOptions options, CancellationToken cancellationToken)
    {
        var run = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var previous = options.IsSilent ? _silentValidationTokenSource : _validationTokenSource;
        if (options.IsSilent)
        {
            _silentValidationTokenSource = run;
        }
        else
        {
            _validationTokenSource = run;
            _validationVersion++;
        }
        previous?.Cancel();
        var token = run.Token;
        try
        {
            token.ThrowIfCancellationRequested();
            if (!options.IsSilent)
            {
                AboutToValidate?.Invoke(this, EventArgs.Empty);
            }

            var names = options.FieldNames is null ? null : new HashSet<string>(options.FieldNames, StringComparer.Ordinal);
            var fields = GetFormItems().Where(item => item.CanValidate &&
                (ShouldValidateHiddenItems || !item.IsSkipValidate()) &&
                (names is null || names.Contains(item.FieldName ?? string.Empty))).ToArray();
            var results = await Task.WhenAll(fields.Select(field => field.ValidateFieldAsync(options.IsSilent, token)));
            token.ThrowIfCancellationRequested();
            var report = new FormValidateReport(BuildValues(), results);
            if (!options.IsSilent)
            {
                UpdateFormValidity();
            }
            return report;
        }
        finally
        {
            if (ReferenceEquals(run, _validationTokenSource))
            {
                _validationTokenSource = null;
            }
            if (ReferenceEquals(run, _silentValidationTokenSource))
            {
                _silentValidationTokenSource = null;
            }
            run.Dispose();
        }
    }

    /// <summary>
    /// Validates every item on the dispatcher and returns before the validation finishes. When the validation
    /// passes, <see cref="Validated" /> is raised, <see cref="NotifySubmit" /> is called and <see cref="Submitted" />
    /// is raised with the values that were validated. When it fails, <see cref="Validated" /> and
    /// <see cref="SubmitFailed" /> are raised, and <see cref="RevealFirstError" /> is called when
    /// <see cref="IsScrollToFirstErrorEnabled" /> is enabled.
    /// A submission canceled by <see cref="CancelValidation" />, by a newer validation that is not silent or by
    /// detaching the form raises neither <see cref="Submitted" /> nor <see cref="SubmitFailed" />.
    /// </summary>
    public void Submit()
    {
        Dispatcher.InvokeAsync(async () =>
        {
            try
            {
                var expectedVersion = _validationVersion + 1;
                var report = await ValidateAsync(new FormValidateOptions(), CancellationToken.None);
                if (_validationVersion != expectedVersion)
                {
                    return;
                }
                if (report.Result == FormValidateResult.Success)
                {
                    var values = (FormValues)report.Values;
                    Validated?.Invoke(this, new FormValidatedEventArgs(report.Result, values, null));
                    NotifySubmit(values);
                    Submitted?.Invoke(this, new FormSubmittedEventArgs(values));
                }
                else
                {
                    SubmitFailed?.Invoke(this, report);
                    if (IsScrollToFirstErrorEnabled && GetFormItems().FirstOrDefault(item =>
                        item.ValidateResult == FormValidateResult.Error && item.IsVisible) is { } first)
                    {
                        RevealFirstError(first);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // A superseding request or detach ends submission without emitting results.
            }
        });
    }

    /// <summary>
    /// Called by <see cref="Submit" /> after the validation passed and before <see cref="Submitted" /> is raised,
    /// with the values that are about to be submitted. The base method does nothing.
    /// </summary>
    protected virtual void NotifySubmit(FormValues values)
    {
    }

    /// <summary>
    /// Writes the values into the items whose field name is a key of <paramref name="formValues" />, then refreshes
    /// <see cref="Values" />. Keys without an item and items without a key are left alone.
    /// The writes do not count as user input: they start no validation and keep the errors set with
    /// <see cref="SetFieldErrors" />.
    /// </summary>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="formValues" /> is <see langword="null" />.
    /// </exception>
    public void SetFormValues(IFormValues formValues)
    {
        ArgumentNullException.ThrowIfNull(formValues);
        using var write = BeginFormWrite();
        foreach (var item in GetFormItems())
        {
            if (item.FieldName is { Length: > 0 } field && formValues.TryGetValue(field, out var value))
            {
                item.SetItemValue(value);
            }
        }
        PublishValues();
    }

    /// <summary>
    /// Cancels the pending validations and restores every item to its initial value: the value from
    /// <see cref="InitialValues" /> when the item's field name is a key there, otherwise the item's own initial value;
    /// an item that has neither is cleared. The validation state of the items and the errors set with
    /// <see cref="SetFieldErrors" /> are cleared.
    /// Then <see cref="Values" /> is refreshed, <see cref="NotifyReset" /> is called, <see cref="IsFormValid" /> is
    /// set to <see langword="false" /> and <see cref="ResetCompleted" /> is raised.
    /// </summary>
    public void Reset()
    {
        CancelValidation();
        using var write = BeginFormWrite();
        _externalErrors.Clear();
        foreach (var item in GetFormItems())
        {
            var hasInitial = item.FieldName is { } name && InitialValues?.ContainsKey(name) == true;
            item.ResetItemValue(hasInitial, hasInitial ? InitialValues![item.FieldName!] : null);
        }
        PublishValues();
        NotifyReset();
        IsFormValid = false;
        ResetCompleted?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Called by <see cref="Reset" /> after the items were restored and before <see cref="IsFormValid" /> is set to
    /// <see langword="false" /> and <see cref="ResetCompleted" /> is raised. The base method does nothing.
    /// </summary>
    protected virtual void NotifyReset()
    {
    }

    /// <summary>
    /// Cancels the validation and submission runs in progress and the pending validation of every item, including a
    /// debounced one. The affected tasks end with <see cref="OperationCanceledException" />, and no
    /// <see cref="Validated" />, <see cref="Submitted" /> or <see cref="SubmitFailed" /> is raised for them.
    /// Results that items already committed stay; an item that shows the validating status returns to the status it
    /// had before the canceled run started.
    /// </summary>
    public void CancelValidation()
    {
        _validationVersion++;
        _validationTokenSource?.Cancel();
        _silentValidationTokenSource?.Cancel();
        foreach (var item in GetFormItems())
        {
            item.CancelValidation();
        }
    }

    /// <summary>
    /// Shows the messages as validation errors on every item with the field name and stores them on the form. Such
    /// an item fails validation until the errors are removed, which blocks <see cref="Submit" />.
    /// The errors stay until <see cref="ClearFieldErrors" /> is called, the user changes the item's value or
    /// <see cref="Reset" /> is called; they are shown again on an item with that field name that is attached later.
    /// Blank messages are dropped; when none is left, the call acts as <see cref="ClearFieldErrors" />.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// <paramref name="fieldName" /> is <see langword="null" />, empty or white space.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="messages" /> is <see langword="null" />.
    /// </exception>
    public void SetFieldErrors(string fieldName, IEnumerable<string> messages)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fieldName);
        ArgumentNullException.ThrowIfNull(messages);
        var supplied = messages.Where(message => !string.IsNullOrWhiteSpace(message)).ToArray();
        if (supplied.Length == 0)
        {
            ClearFieldErrors(fieldName);
            return;
        }
        _externalErrors[fieldName] = supplied;
        foreach (var item in GetFormItems().Where(item => item.FieldName == fieldName))
        {
            item.SetExternalErrors(supplied);
        }
        UpdateFormValidity();
    }

    /// <summary>
    /// Removes the errors that <see cref="SetFieldErrors" /> set for the field name from every item with that field
    /// name and from the form. The items return to the status of their last validation and <see cref="IsFormValid" />
    /// is updated.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// <paramref name="fieldName" /> is <see langword="null" />, empty or white space.
    /// </exception>
    public void ClearFieldErrors(string fieldName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fieldName);
        _externalErrors.Remove(fieldName);
        foreach (var item in GetFormItems().Where(item => item.FieldName == fieldName))
        {
            item.SetExternalErrors(null);
        }
        UpdateFormValidity();
    }

    /// <summary>
    /// Removes the form item from the items of the form. With <see cref="ItemsControl.ItemsSource" /> set to
    /// <see langword="null" />, the item is removed from <see cref="ItemsControl.Items" />. With an
    /// <see cref="ItemsControl.ItemsSource" /> that is a writable list of variable size, the entry at the item's index
    /// is removed from that list, which requires that the form realized the item as a container.
    /// Nothing happens for a read-only or fixed-size list, for an item of such a list without a realized container,
    /// or for an item the form does not contain.
    /// </summary>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="formItem" /> is <see langword="null" />.
    /// </exception>
    public void DeleteFormItem(IFormItem formItem)
    {
        ArgumentNullException.ThrowIfNull(formItem);
        var index = formItem is Control container ? IndexFromContainer(container) : -1;
        if (index >= 0)
        {
            if (ItemsSource is IList { IsReadOnly: false, IsFixedSize: false } source)
            {
                source.RemoveAt(index);
            }
            else if (ItemsSource is null)
            {
                Items.RemoveAt(index);
            }
        }
        else if (ItemsSource is null)
        {
            Items.Remove(formItem);
        }
    }

    internal void OnFieldIdentityChanged(AbstractFormItem item)
    {
        if (ReferenceEquals(item.OwnerForm, this))
        {
            AttachItem(item);
            PublishValues();
        }
    }

    internal void OnFieldValueChanged(AbstractFormItem item, bool userChange)
    {
        if (userChange && item.FieldName is { Length: > 0 } name)
        {
            ClearFieldErrors(name);
        }
        PublishValues();
        ItemValueChanged?.Invoke(this, new FormItemValueChangedEventArgs(item, item.GetItemValue()));
        foreach (var other in GetFormItems())
        {
            if (!ReferenceEquals(item, other))
            {
                other.NotifyOwnerFieldChanged(item, userChange);
            }
        }
    }

    internal void UpdateFormValidity()
    {
        var fields = GetFormItems().Where(item => item.IsValueItem);
        IsFormValid = fields.All(item => item.ValidateStatus is FormValidateStatus.Success or FormValidateStatus.Warning);
    }

    private FormValues BuildValues()
    {
        var snapshot = new FormValues();
        foreach (var item in GetFormItems())
        {
            if (item.FieldName is { Length: > 0 } name && !string.IsNullOrWhiteSpace(name))
            {
                snapshot[name] = item.GetItemValue();
            }
        }
        return snapshot;
    }

    private void PublishValues() => SetAndRaise(ValuesProperty, ref _values, BuildValues());

    private IEnumerable<AbstractFormItem> GetFormItems()
    {
        for (var index = 0; index < ItemCount; index++)
        {
            if (ItemsView[index] is AbstractFormItem direct)
            {
                yield return direct;
            }
            else if (ContainerFromIndex(index) is AbstractFormItem item && _realizedItems.Contains(item))
            {
                yield return item;
            }
        }
    }

    private void ReconcileItems()
    {
        foreach (var field in _initializedItems.Concat(_realizedItems).Distinct().ToArray())
        {
            if (IndexFromContainer(field) < 0 && !Items.Contains(field))
            {
                _realizedItems.Remove(field);
                DetachItem(field);
            }
        }
        foreach (var field in GetFormItems())
        {
            AttachItem(field);
        }
        PublishValues();
    }

    private void AttachItem(AbstractFormItem item)
    {
        item.SetOwner(this);
        item.SynchronizeWithForm(this);
        if (item.Content is not null && item.FieldName is { Length: > 0 } field && _initializedItems.Add(item) &&
            InitialValues?.TryGetValue(field, out var value) == true)
        {
            using var write = BeginFormWrite();
            item.SetItemValue(value);
        }
        item.SetExternalErrors(item.FieldName is { Length: > 0 } name && _externalErrors.TryGetValue(name, out var errors)
            ? errors : null);
    }

    private void DetachItem(AbstractFormItem field)
    {
        _initializedItems.Remove(field);
        if (ReferenceEquals(field.OwnerForm, this))
        {
            field.SetOwner(null);
        }
    }

    private FormWrite BeginFormWrite()
    {
        _guardDepth++;
        return new FormWrite(this);
    }

    private void EndFormWrite() => _guardDepth--;

    private readonly struct FormWrite(AbstractForm form) : IDisposable
    {
        public void Dispose() => form.EndFormWrite();
    }

    private sealed class DefaultFormItem : AbstractFormItem;
}
