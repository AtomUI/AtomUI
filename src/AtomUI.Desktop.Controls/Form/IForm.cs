namespace AtomUI.Desktop.Controls;

public interface IForm
{
    void Validate();
    void Submit();
    void Reset();
    void SetFormValues(FormValues formValues);
    void DeleteFormItem(FormItem formItem);
}