using Avalonia.Controls;

namespace AtomUI.Desktop.Controls;

public interface IFormItem
{
    string? FieldName { get; }
    public Control? Content { get; }
    object? GetItemValue();
    void SetItemValue(object? value);
    void ResetItemValue();
    Task ValidateValueAsync();
}