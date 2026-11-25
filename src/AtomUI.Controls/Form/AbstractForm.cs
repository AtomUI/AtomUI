using System.Collections;
using AtomUI.Desktop.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;

namespace AtomUI.Controls;

public abstract class AbstractForm : ItemsControl, IForm
{
    public static readonly StyledProperty<FormRequiredMark> RequiredMarkProperty =
        AvaloniaProperty.Register<AbstractForm, FormRequiredMark>(nameof(RequiredMark), FormRequiredMark.Default);

    public static readonly StyledProperty<bool> IsValidateFeedbackEnabledProperty =
        AvaloniaProperty.Register<AbstractForm, bool>(
            nameof(IsValidateFeedbackEnabled), false);

    public static readonly StyledProperty<FormLayout> FormLayoutProperty =
        AvaloniaProperty.Register<AbstractForm, FormLayout>(nameof(FormLayout));

    public static readonly StyledProperty<bool> IsScrollToFirstErrorEnabledProperty =
        AvaloniaProperty.Register<AbstractForm, bool>(nameof(IsScrollToFirstErrorEnabled));

    public static readonly StyledProperty<FormValidateTrigger> ValidateTriggerProperty =
        AvaloniaProperty.Register<AbstractForm, FormValidateTrigger>(nameof(ValidateTrigger), FormValidateTrigger.OnChanged);

    public FormRequiredMark RequiredMark
    {
        get => GetValue(RequiredMarkProperty);
        set => SetValue(RequiredMarkProperty, value);
    }

    public bool IsValidateFeedbackEnabled
    {
        get => GetValue(IsValidateFeedbackEnabledProperty);
        set => SetValue(IsValidateFeedbackEnabledProperty, value);
    }

    public FormLayout FormLayout
    {
        get => GetValue(FormLayoutProperty);
        set => SetValue(FormLayoutProperty, value);
    }

    public bool IsScrollToFirstErrorEnabled
    {
        get => GetValue(IsScrollToFirstErrorEnabledProperty);
        set => SetValue(IsScrollToFirstErrorEnabledProperty, value);
    }

    public FormValidateTrigger ValidateTrigger
    {
        get => GetValue(ValidateTriggerProperty);
        set => SetValue(ValidateTriggerProperty, value);
    }

    public static readonly DirectProperty<AbstractForm, IFormValues?> ValuesProperty =
        AvaloniaProperty.RegisterDirect<AbstractForm, IFormValues?>(nameof(Values), owner => owner.Values);

    public static readonly DirectProperty<AbstractForm, IFormValues?> InitialValuesProperty =
        AvaloniaProperty.RegisterDirect<AbstractForm, IFormValues?>(nameof(InitialValues), owner => owner.InitialValues, (owner, value) => owner.InitialValues = value);

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

    public IFormValues? Values => _values;

    public IFormValues? InitialValues
    {
        get => _initialValues;
        set => SetAndRaise(InitialValuesProperty, ref _initialValues, value);
    }

    public bool? IsFormValid
    {
        get => _isFormValid;
        private set => SetAndRaise(IsFormValidProperty, ref _isFormValid, value);
    }

    public event EventHandler<EventArgs>? AboutToValidate;
    public event EventHandler<FormValidatedEventArgs>? Validated;
    public event EventHandler<FormSubmittedEventArgs>? Submitted;
    public event EventHandler<FormValidateReport>? SubmitFailed;
    public event EventHandler<FormItemValueChangedEventArgs>? ItemValueChanged;
    public event EventHandler? ResetCompleted;

    protected virtual bool ShouldValidateHiddenItems => false;

    internal bool IsWritingValues => _guardDepth > 0;

    protected virtual AbstractFormItem CreateFormItem() => new DefaultFormItem();

    protected virtual void RevealFirstError(AbstractFormItem item) => item.BringIntoView();

    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey) => CreateFormItem();

    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey) =>
        NeedsContainer<AbstractFormItem>(item, out recycleKey);

    protected override void PrepareContainerForItemOverride(Control container, object? item, int index)
    {
        base.PrepareContainerForItemOverride(container, item, index);
        if (container is AbstractFormItem field)
        {
            _realizedItems.Add(field);
            AttachItem(field);
        }
    }

    protected override void ContainerForItemPreparedOverride(Control container, object? item, int index)
    {
        base.ContainerForItemPreparedOverride(container, item, index);
        PublishValues();
    }

    protected override void ContainerIndexChangedOverride(Control container, int oldIndex, int newIndex)
    {
        base.ContainerIndexChangedOverride(container, oldIndex, newIndex);
        PublishValues();
    }

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

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        ReconcileItems();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        ReconcileItems();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        CancelValidation();
        base.OnDetachedFromVisualTree(e);
    }

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

    public async Task<FormValidateResult> ValidateAsync(CancellationToken cancellationToken)
    {
        var report = await ValidateAsync(new FormValidateOptions(), cancellationToken);
        return report.Result;
    }

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

    protected virtual void NotifySubmit(FormValues values)
    {
    }

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

    protected virtual void NotifyReset()
    {
    }

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
