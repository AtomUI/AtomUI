namespace AtomUI.Desktop.Controls;

public class FormStringNotEmptyValidator : IFormValidator
{
    public string? Message { get; set; }

    public async Task<FormValidateResult> ValidateAsync(string fieldName, object? value)
    {
        var strValue = value as string;
        if (string.IsNullOrWhiteSpace(strValue))
        {
            return await Task.FromResult(FormValidateResult.Error);
        }
        return await Task.FromResult(FormValidateResult.Success);
    }
}