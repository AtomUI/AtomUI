using System.Diagnostics;
using AtomUI.Controls;
using AtomUI.Generated.AtomUIDesktopControls;
using Avalonia.Media;
using Avalonia.Threading;

namespace AtomUI.Desktop.Controls;

public partial class FormItem
{
    private CancellationTokenSource? _validationTokenSource;
    private IDisposable? _validationDebounceDisposable;

    private void ValidateValueDefer()
    {
        var cancellationToken = BeginValidationRun(CancellationToken.None);
        if (ValidateDebounce != TimeSpan.Zero)
        {
            _validationDebounceDisposable = DispatcherTimer.RunOnce(() =>
            {
                _validationDebounceDisposable = null;
                Dispatcher.InvokeAsync(() => ValidateCurrentValueAsync(cancellationToken));
            }, ValidateDebounce);
        }
        else
        {
            Dispatcher.InvokeAsync(() => ValidateCurrentValueAsync(cancellationToken));
        }
    }

    public Task ValidateValueAsync(CancellationToken cancellationToken)
    {
        var validationToken = BeginValidationRun(cancellationToken);
        return ValidateCurrentValueAsync(validationToken);
    }

    private CancellationToken BeginValidationRun(CancellationToken cancellationToken)
    {
        CancelPendingValidation();
        _validationTokenSource = cancellationToken.CanBeCanceled
            ? CancellationTokenSource.CreateLinkedTokenSource(cancellationToken)
            : new CancellationTokenSource();
        return _validationTokenSource.Token;
    }

    private void CancelPendingValidation()
    {
        _validationDebounceDisposable?.Dispose();
        _validationDebounceDisposable = null;

        _validationTokenSource?.Cancel();
        _validationTokenSource?.Dispose();
        _validationTokenSource = null;
    }

    private async Task ValidateCurrentValueAsync(CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return;
        }

        if (Content == null)
        {
            return;
        }

        if (Validators == null || Validators.Count == 0)
        {
            ApplyValidationOutcome(FormValidateStatus.Success,
                errorMessages: null,
                warningMessages: null,
                Content as IFormItemAware,
                raiseValidateChanged: true);
            return;
        }

        var formItemAware = Content as IFormItemAware;
        Debug.Assert(formItemAware != null);

        ValidateStatus = FormValidateStatus.Validating;
        var value   = formItemAware.GetFormValue();
        var outcome = await ExecuteValidatorsAsync(value, cancellationToken);
        if (cancellationToken.IsCancellationRequested)
        {
            return;
        }

        ApplyValidationOutcome(outcome.Status,
            outcome.ErrorMessages,
            outcome.WarningMessages,
            formItemAware,
            raiseValidateChanged: true);
    }

    private async Task<(FormValidateStatus Status, IList<string>? ErrorMessages, IList<string>? WarningMessages)> ExecuteValidatorsAsync(
        object? value,
        CancellationToken cancellationToken)
    {
        if (ValidateStrategy == FormValidateStrategy.Parallel)
        {
            return await ExecuteValidatorsInParallelAsync(value, cancellationToken);
        }

        var stopWhenFirstFailed = ValidateStrategy == FormValidateStrategy.StopWhenFirstFailed;
        return await ExecuteValidatorsSequentiallyAsync(value, stopWhenFirstFailed, cancellationToken);
    }

    private async Task<(FormValidateStatus Status, IList<string>? ErrorMessages, IList<string>? WarningMessages)> ExecuteValidatorsInParallelAsync(
        object? value,
        CancellationToken cancellationToken)
    {
        Debug.Assert(Validators != null);

        var tasks           = new List<(Task<FormValidateResult> Task, IFormValidator Validator)>(Validators.Count);
        var validationTasks = new Task<FormValidateResult>[Validators.Count];
        var index           = 0;
        foreach (var validator in Validators)
        {
            var task = validator.ValidateAsync(FieldName ?? string.Empty, value, cancellationToken);
            tasks.Add((task, validator));
            validationTasks[index++] = task;
        }

        try
        {
            await Task.WhenAll(validationTasks);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return (ValidateStatus, ValidateErrorMessages, ValidateWarningMessages);
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return (ValidateStatus, ValidateErrorMessages, ValidateWarningMessages);
        }

        var errorMessages   = new List<string>(Validators.Count);
        var warningMessages = new List<string>(Validators.Count);
        var hasError        = false;
        var hasWarning      = false;
        foreach (var (task, validator) in tasks)
        {
            var result = await task;
            CollectValidatorResult(result, validator, errorMessages, warningMessages, ref hasError, ref hasWarning);
        }

        return BuildValidationOutcome(hasError, hasWarning, errorMessages, warningMessages);
    }

    private async Task<(FormValidateStatus Status, IList<string>? ErrorMessages, IList<string>? WarningMessages)> ExecuteValidatorsSequentiallyAsync(
        object? value,
        bool stopWhenFirstFailed,
        CancellationToken cancellationToken)
    {
        Debug.Assert(Validators != null);

        var errorMessages   = new List<string>(Validators.Count);
        var warningMessages = new List<string>(Validators.Count);
        var hasError        = false;
        var hasWarning      = false;
        foreach (var validator in Validators)
        {
            FormValidateResult result;
            try
            {
                result = await validator.ValidateAsync(FieldName ?? string.Empty, value, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                return (ValidateStatus, ValidateErrorMessages, ValidateWarningMessages);
            }

            if (cancellationToken.IsCancellationRequested)
            {
                return (ValidateStatus, ValidateErrorMessages, ValidateWarningMessages);
            }

            CollectValidatorResult(result, validator, errorMessages, warningMessages, ref hasError, ref hasWarning);
            if (stopWhenFirstFailed && result == FormValidateResult.Error)
            {
                break;
            }
        }

        return BuildValidationOutcome(hasError, hasWarning, errorMessages, warningMessages);
    }

    private static void CollectValidatorResult(
        FormValidateResult result,
        IFormValidator validator,
        IList<string> errorMessages,
        IList<string> warningMessages,
        ref bool hasError,
        ref bool hasWarning)
    {
        if (result == FormValidateResult.Error)
        {
            hasError = true;
            if (!string.IsNullOrWhiteSpace(validator.Message))
            {
                errorMessages.Add(validator.Message);
            }
        }
        else if (result == FormValidateResult.Warning)
        {
            hasWarning = true;
            if (!string.IsNullOrWhiteSpace(validator.Message))
            {
                warningMessages.Add(validator.Message);
            }
        }
    }

    private static (FormValidateStatus Status, IList<string>? ErrorMessages, IList<string>? WarningMessages) BuildValidationOutcome(
        bool hasError,
        bool hasWarning,
        IList<string> errorMessages,
        IList<string> warningMessages)
    {
        if (hasError)
        {
            return (FormValidateStatus.Error, errorMessages, hasWarning ? warningMessages : null);
        }

        if (hasWarning)
        {
            return (FormValidateStatus.Warning, null, warningMessages);
        }

        return (FormValidateStatus.Success, null, null);
    }

    private void ResetValidationState(IFormItemAware? formItemAware)
    {
        ApplyValidationOutcome(FormValidateStatus.Default,
            errorMessages: null,
            warningMessages: null,
            formItemAware,
            raiseValidateChanged: false);
    }

    private void ApplyValidationOutcome(
        FormValidateStatus status,
        IList<string>? errorMessages,
        IList<string>? warningMessages,
        IFormItemAware? formItemAware,
        bool raiseValidateChanged)
    {
        var validationTarget = Content;
        if (validationTarget != null)
        {
            if (status == FormValidateStatus.Error)
            {
                FormDataValidationErrors.SetFormErrors(validationTarget, errorMessages);
            }
            else
            {
                FormDataValidationErrors.ClearFormErrors(validationTarget);
            }
        }

        var nativeErrorMessages = validationTarget != null
            ? FormDataValidationErrors.GetErrorMessages(validationTarget)
            : [];
        var effectiveStatus = nativeErrorMessages.Count > 0
            ? FormValidateStatus.Error
            : status;

        ValidateErrorMessages   = nativeErrorMessages.Count > 0 ? nativeErrorMessages : null;
        ValidateWarningMessages = warningMessages;
        ValidateStatus          = effectiveStatus;
        ValidateResult          = effectiveStatus switch
        {
            FormValidateStatus.Error   => FormValidateResult.Error,
            FormValidateStatus.Warning => FormValidateResult.Warning,
            _                          => FormValidateResult.Success
        };

        formItemAware?.NotifyValidateStatus(effectiveStatus);
        HasErrorOrWarningMsg = ValidateErrorMessages?.Count > 0 ||
                               ValidateWarningMessages?.Count > 0 ||
                               !string.IsNullOrWhiteSpace(Help);
        BuildMessageItems();

        if (raiseValidateChanged)
        {
            RaiseEvent(new FormItemValidateChangedEventArgs(effectiveStatus)
            {
                RoutedEvent = ValidateChangedEvent,
                Source      = this,
            });
        }
    }

    private void BuildMessageItems()
    {
        ReleaseMessageItems();
        if (_extraInfoLayout is null)
        {
            return;
        }
        if (ValidateResult == FormValidateResult.Success)
        {
            return;
        }
        if (ValidateErrorMessages != null)
        {
            foreach (var message in ValidateErrorMessages)
            {
                AddMessageItem(message, ErrorMessageForeground);
            }
        }
        if (ValidateWarningMessages != null)
        {
            foreach (var message in ValidateWarningMessages)
            {
                AddMessageItem(message, WarningMessageForeground);
            }
        }
    }

    private void AddMessageItem(string message, IBrush? foreground)
    {
        var item = new Avalonia.Controls.TextBlock
        {
            Text       = message,
            Foreground = foreground
        };
        item.Classes.Add(FormItemSemanticParts.HelpItemClass);
        var insertIndex = _extraInfoLayout!.Children.Count;
        if (_helpText is not null && _extraInfoLayout.Children.Contains(_helpText))
        {
            insertIndex = _extraInfoLayout.Children.IndexOf(_helpText);
        }
        _extraInfoLayout.Children.Insert(insertIndex, item);
        _messageItems.Add(item);
    }

    private void ReleaseMessageItems()
    {
        foreach (var item in _messageItems)
        {
            _extraInfoLayout?.Children.Remove(item);
        }
        _messageItems.Clear();
    }
}
