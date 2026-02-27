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
    Task<FormValidateResult> ValidateAsync(string fieldName, object? value);
}