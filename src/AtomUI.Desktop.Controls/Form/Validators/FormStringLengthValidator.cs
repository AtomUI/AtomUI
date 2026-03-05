using System.Diagnostics;

namespace AtomUI.Desktop.Controls;

public class FormStringLengthValidator : AbstractFormValidator
{
    public int MinimumLength { get; set; } = 0;
    public int MaximumLength { get; set; } = int.MaxValue;
    
    protected override async Task<bool> NotifyValidateAsync(string fieldName, object? value, CancellationToken cancellationToken)
    {
        var strValue = value as string;
        var isValid = true;
        if (string.IsNullOrWhiteSpace(strValue))
        {
            isValid = MinimumLength == 0;
        }
        else
        {
            Debug.Assert(strValue != null);
            isValid = strValue.Length >= MinimumLength && strValue.Length <= MaximumLength;
        }
        return await Task.FromResult(isValid);
    }
}