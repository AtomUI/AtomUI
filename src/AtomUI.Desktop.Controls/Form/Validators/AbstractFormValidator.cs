namespace AtomUI.Desktop.Controls;

public abstract class AbstractFormValidator : IFormValidator
{
    public string? Message { get; set; }
    public bool WarningOnly { get; set; }
    
    public async Task<FormValidateResult> ValidateAsync(string fieldName, object? value, CancellationToken cancellationToken)
    {
        var isValid = await NotifyValidateAsync(fieldName, value, cancellationToken);
        if (isValid)
        {
            return await Task.FromResult(FormValidateResult.Success);
        }

        if (WarningOnly)
        {
            return await Task.FromResult(FormValidateResult.Warning);
        }
        return await Task.FromResult(FormValidateResult.Error);
    }

    protected abstract Task<bool> NotifyValidateAsync(string fieldName, object? value, CancellationToken cancellationToken);
}