namespace AtomUI.Desktop.Controls;

public interface IFormItem
{
    string? FieldName { get; }
    object? GetValue();
    void SetValue(object? value);
    void ResetValue();
    Task ValidateValueAsync();
}