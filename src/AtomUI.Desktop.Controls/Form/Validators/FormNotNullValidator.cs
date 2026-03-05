namespace AtomUI.Desktop.Controls;

public class FormNotNullValidator : AbstractFormValidator
{
    protected override async Task<bool> NotifyValidateAsync(string fieldName, object? value, CancellationToken cancellationToken)
    {
        return await Task.FromResult(value != null);
    }
}