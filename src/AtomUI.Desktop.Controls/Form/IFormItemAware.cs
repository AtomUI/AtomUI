namespace AtomUI.Desktop.Controls;

public interface IFormItemAware
{
    void SetFormValue(object? value);
    void ProvideFormValue(IFormValue value);
    void ClearFormValue();
}