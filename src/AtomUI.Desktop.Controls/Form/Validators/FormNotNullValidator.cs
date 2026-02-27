namespace AtomUI.Desktop.Controls;

public class FormNotNullValidator : IFormValidator
{
    public string? Message { get; set; }

    public async Task<FormValidateResult> ValidateAsync(string fieldName, object? value)
    {
        if (value == null)
        {
            return await Task.FromResult(FormValidateResult.Error);
        }
        return await Task.FromResult(FormValidateResult.Success);
    }
}