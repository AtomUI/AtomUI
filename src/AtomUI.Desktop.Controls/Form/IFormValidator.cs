namespace AtomUI.Desktop.Controls;

public interface IFormValidator
{
    string? ErrorMessage { get; }
    Task<bool> ValidateAsync(string fieldName, object? value);
}