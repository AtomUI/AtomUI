namespace AtomUI.Desktop.Controls;

public interface IFormItemAware
{
    event EventHandler ValueChanged;
    void SetFormValue(object? value);
    object? GetFormValue();
    void ClearFormValue();
    void NotifyValidateStatus(FormValidateStatus status);
}