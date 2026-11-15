namespace AtomUI.Controls;

public interface IForm
{
    void Validate();
    Task<FormValidateReport> ValidateAsync(FormValidateOptions options, CancellationToken cancellationToken = default);
    void Submit();
    void Reset();
    void SetFormValues(IFormValues formValues);
    void DeleteFormItem(IFormItem formItem);
}