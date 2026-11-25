namespace AtomUI.Controls;

public abstract partial class AbstractFormItem
{
    private async Task<(FormValidateStatus Status, IList<string>? ErrorMessages, IList<string>? WarningMessages)> ExecuteValidatorsAsync(
        object? value,
        CancellationToken cancellationToken)
    {
        if (Validators is not { Count: > 0 })
        {
            return (FormValidateStatus.Success, null, null);
        }

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
        var validators = Validators!.ToArray();

        var tasks           = new List<(Task<FormValidateResult> Task, IFormValidator Validator)>(validators.Length);
        var validationTasks = new Task<FormValidateResult>[validators.Length];
        var index           = 0;
        foreach (var validator in validators)
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

        var errorMessages   = new List<string>(validators.Length);
        var warningMessages = new List<string>(validators.Length);
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
        var validators = Validators!.ToArray();

        var errorMessages   = new List<string>(validators.Length);
        var warningMessages = new List<string>(validators.Length);
        var hasError        = false;
        var hasWarning      = false;
        foreach (var validator in validators)
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

    private void CollectValidatorResult(
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
            errorMessages.Add(FormValidationMessages.Resolve(this, validator));
        }
        else if (result == FormValidateResult.Warning)
        {
            hasWarning = true;
            warningMessages.Add(FormValidationMessages.Resolve(this, validator));
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
}
