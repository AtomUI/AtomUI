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

public abstract partial class AbstractFormItem : TemplatedControl, IFormItem
{
    public static readonly RoutedEvent<FormItemValidateChangedEventArgs> ValidateChangedEvent =
        RoutedEvent.Register<AbstractFormItem, FormItemValidateChangedEventArgs>(nameof(ValidateChanged), RoutingStrategies.Bubble | RoutingStrategies.Tunnel);

    internal static readonly RoutedEvent<RoutedEventArgs> ValueChangedEvent =
        RoutedEvent.Register<AbstractFormItem, RoutedEventArgs>("ValueChanged", RoutingStrategies.Bubble);

    public static readonly StyledProperty<IReadOnlyList<string>?> DependenciesProperty =
        AvaloniaProperty.Register<AbstractFormItem, IReadOnlyList<string>?>(nameof(Dependencies));

    public static readonly DirectProperty<AbstractFormItem, bool> IsTouchedProperty =
        AvaloniaProperty.RegisterDirect<AbstractFormItem, bool>(nameof(IsTouched), item => item.IsTouched);

    public static readonly DirectProperty<AbstractFormItem, bool> IsValidatingProperty =
        AvaloniaProperty.RegisterDirect<AbstractFormItem, bool>(nameof(IsValidating), item => item.IsValidating);

    public static readonly DirectProperty<AbstractFormItem, IList<string>?> ValidateErrorMessagesProperty =
        AvaloniaProperty.RegisterDirect<AbstractFormItem, IList<string>?>(nameof(ValidateErrorMessages), item => item.ValidateErrorMessages);

    public static readonly DirectProperty<AbstractFormItem, IList<string>?> ValidateWarningMessagesProperty =
        AvaloniaProperty.RegisterDirect<AbstractFormItem, IList<string>?>(nameof(ValidateWarningMessages), item => item.ValidateWarningMessages);

    public static readonly DirectProperty<AbstractFormItem, FormValidateStatus> ValidateStatusProperty =
        AvaloniaProperty.RegisterDirect<AbstractFormItem, FormValidateStatus>(nameof(ValidateStatus), owner => owner.ValidateStatus);

    public static readonly DirectProperty<AbstractFormItem, FormValidateResult> ValidateResultProperty =
        AvaloniaProperty.RegisterDirect<AbstractFormItem, FormValidateResult>(nameof(ValidateResult), owner => owner.ValidateResult);

    public static readonly StyledProperty<object?> ExtraProperty =
        AvaloniaProperty.Register<AbstractFormItem, object?>(nameof(Extra));

    public static readonly StyledProperty<IDataTemplate?> ExtraTemplateProperty =
        AvaloniaProperty.Register<AbstractFormItem, IDataTemplate?>(nameof(ExtraTemplate));

    public static readonly StyledProperty<string?> HelpProperty =
        AvaloniaProperty.Register<AbstractFormItem, string?>(nameof(Help));

    public static readonly StyledProperty<bool> IsValidateFeedbackEnabledProperty =
        AbstractForm.IsValidateFeedbackEnabledProperty.AddOwner<AbstractFormItem>();

    public static readonly StyledProperty<object?> InitialValueProperty =
        AvaloniaProperty.Register<AbstractFormItem, object?>(nameof(InitialValue));

    public static readonly StyledProperty<string?> LabelTextProperty =
        AvaloniaProperty.Register<AbstractFormItem, string?>(nameof(LabelText));

    public static readonly StyledProperty<string?> FieldNameProperty =
        AvaloniaProperty.Register<AbstractFormItem, string?>(nameof(FieldName));

    public static readonly StyledProperty<bool> IsRequiredProperty =
        AvaloniaProperty.Register<AbstractFormItem, bool>(nameof(IsRequired));

    public static readonly StyledProperty<IList<IFormValidator>?> ValidatorsProperty =
        AvaloniaProperty.Register<AbstractFormItem, IList<IFormValidator>?>(nameof(Validators));

    public static readonly StyledProperty<FormValidateTrigger> ValidateTriggerProperty =
        AbstractForm.ValidateTriggerProperty.AddOwner<AbstractFormItem>();

    public static readonly StyledProperty<TimeSpan> ValidateDebounceProperty =
        AvaloniaProperty.Register<AbstractFormItem, TimeSpan>(nameof(ValidateDebounce), TimeSpan.Zero);

    public static readonly StyledProperty<FormValidateStrategy> ValidateStrategyProperty =
        AvaloniaProperty.Register<AbstractFormItem, FormValidateStrategy>(nameof(ValidateStrategy), FormValidateStrategy.StopWhenFirstFailed);

    public static readonly StyledProperty<FormItemLayout> LayoutProperty =
        AvaloniaProperty.Register<AbstractFormItem, FormItemLayout>(nameof(Layout), FormItemLayout.Horizontal);

    public static readonly StyledProperty<Control?> ContentProperty =
        AvaloniaProperty.Register<AbstractFormItem, Control?>(nameof(Content));

    public static readonly StyledProperty<bool> IsValidateContentTypeProperty =
        AvaloniaProperty.Register<AbstractFormItem, bool>(nameof(IsValidateContentType), true);

    public static readonly StyledProperty<bool> IsValueItemProperty =
        AvaloniaProperty.Register<AbstractFormItem, bool>(nameof(IsValueItem), true);

    public static readonly StyledProperty<FormRequiredMark> RequiredMarkProperty =
        AbstractForm.RequiredMarkProperty.AddOwner<AbstractFormItem>();

    public static readonly StyledProperty<FormLayout> FormLayoutProperty =
        AvaloniaProperty.Register<AbstractFormItem, FormLayout>(nameof(FormLayout));

    public object? Extra
    {
        get => GetValue(ExtraProperty);
        set => SetValue(ExtraProperty, value);
    }

    public IDataTemplate? ExtraTemplate
    {
        get => GetValue(ExtraTemplateProperty);
        set => SetValue(ExtraTemplateProperty, value);
    }

    public string? Help
    {
        get => GetValue(HelpProperty);
        set => SetValue(HelpProperty, value);
    }

    public bool IsValidateFeedbackEnabled
    {
        get => GetValue(IsValidateFeedbackEnabledProperty);
        set => SetValue(IsValidateFeedbackEnabledProperty, value);
    }

    public object? InitialValue
    {
        get => GetValue(InitialValueProperty);
        set => SetValue(InitialValueProperty, value);
    }

    public string? LabelText
    {
        get => GetValue(LabelTextProperty);
        set => SetValue(LabelTextProperty, value);
    }

    public string? FieldName
    {
        get => GetValue(FieldNameProperty);
        set => SetValue(FieldNameProperty, value);
    }

    public bool IsRequired
    {
        get => GetValue(IsRequiredProperty);
        set => SetValue(IsRequiredProperty, value);
    }

    public IList<IFormValidator>? Validators
    {
        get => GetValue(ValidatorsProperty);
        set => SetValue(ValidatorsProperty, value);
    }

    public FormValidateTrigger ValidateTrigger
    {
        get => GetValue(ValidateTriggerProperty);
        set => SetValue(ValidateTriggerProperty, value);
    }

    public TimeSpan ValidateDebounce
    {
        get => GetValue(ValidateDebounceProperty);
        set => SetValue(ValidateDebounceProperty, value);
    }

    public FormValidateStrategy ValidateStrategy
    {
        get => GetValue(ValidateStrategyProperty);
        set => SetValue(ValidateStrategyProperty, value);
    }

    public FormItemLayout Layout
    {
        get => GetValue(LayoutProperty);
        set => SetValue(LayoutProperty, value);
    }

    public Control? Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    public bool IsValidateContentType
    {
        get => GetValue(IsValidateContentTypeProperty);
        set => SetValue(IsValidateContentTypeProperty, value);
    }

    public bool IsValueItem
    {
        get => GetValue(IsValueItemProperty);
        set => SetValue(IsValueItemProperty, value);
    }

    public FormRequiredMark RequiredMark
    {
        get => GetValue(RequiredMarkProperty);
        set => SetValue(RequiredMarkProperty, value);
    }

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

    public FormValidateStatus ValidateStatus
    {
        get => _validateStatus;
        set => SetAndRaise(ValidateStatusProperty, ref _validateStatus, value);
    }

    public FormValidateResult ValidateResult
    {
        get => _validateResult;
        private set => SetAndRaise(ValidateResultProperty, ref _validateResult, value);
    }

    public IList<string>? ValidateErrorMessages
    {
        get => _validateErrorMessages;
        private set => SetAndRaise(ValidateErrorMessagesProperty, ref _validateErrorMessages, value);
    }

    public IList<string>? ValidateWarningMessages
    {
        get => _validateWarningMessages;
        private set => SetAndRaise(ValidateWarningMessagesProperty, ref _validateWarningMessages, value);
    }

    public bool IsTouched => _isTouched;
    public bool IsValidating => _isValidating;

    public IReadOnlyList<string>? Dependencies
    {
        get => GetValue(DependenciesProperty);
        set => SetValue(DependenciesProperty, value);
    }

    public event EventHandler<FormItemValidateChangedEventArgs>? ValidateChanged
    {
        add => AddHandler(ValidateChangedEvent, value);
        remove => RemoveHandler(ValidateChangedEvent, value);
    }

    protected virtual bool IsField => !string.IsNullOrWhiteSpace(FieldName);

    internal virtual bool IsSkipValidate() => !IsVisible;

    protected internal virtual void NotifyFormItemChanged(IFormItem formItem)
    {
    }

    protected virtual void OnValidationMessagesChanged()
    {
    }

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

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        CancelValidation();
        if (Content is { } content)
        {
            StopObservingContent(content);
        }
        base.OnDetachedFromVisualTree(e);
    }

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

    public object? GetItemValue() => (Content as IFormItemAware)?.GetFormValue();

    public void SetItemValue(object? value)
    {
        if (Content is IFormItemAware content)
        {
            BeginGuardedWrite();
            content.SetFormValue(value);
        }
    }

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
