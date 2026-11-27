namespace AtomUI.Controls;

/// <summary>
/// A validator that requires the value to be a string with at least one character that is not white space.
/// </summary>
public class FormStringNotEmptyValidator : AbstractFormValidator
{
    /// <summary>
    /// Checks that the value is a string that is not null, empty or white space only; a value of another type fails.
    /// The field name and the cancellation token are not used.
    /// </summary>
    /// <returns>
    /// <see langword="true" /> when the value is a non-blank string, otherwise <see langword="false" />.
    /// </returns>
    protected override async Task<bool> ValidateCoreAsync(string fieldName, object? value, CancellationToken cancellationToken)
    {
        var strValue = value as string;
        return await Task.FromResult(!string.IsNullOrWhiteSpace(strValue));
    }
}