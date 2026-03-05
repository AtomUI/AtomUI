namespace AtomUI.Desktop.Controls;

public enum FormValidateResult
{
    Success,
    Error,
    Warning,
}

public interface IFormValidator
{
    string? Message { get; }
    bool WarningOnly { get; }
    Task<FormValidateResult> ValidateAsync(string fieldName, object? value, CancellationToken cancellationToken);
}