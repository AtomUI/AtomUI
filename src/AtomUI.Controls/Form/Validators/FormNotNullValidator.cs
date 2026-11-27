namespace AtomUI.Controls;

/// <summary>
/// A validator that fails only when the value is <see langword="null" />. An empty or a blank string passes; use
/// <see cref="FormStringNotEmptyValidator" /> to reject those as well.
/// </summary>
public class FormNotNullValidator : AbstractFormValidator
{
    /// <summary>
    /// Checks that the value is not <see langword="null" />. The field name and the cancellation token are not used.
    /// </summary>
    /// <returns>
    /// <see langword="true" /> when the value is not <see langword="null" />, otherwise <see langword="false" />.
    /// </returns>
    protected override async Task<bool> ValidateCoreAsync(string fieldName, object? value, CancellationToken cancellationToken)
    {
        return await Task.FromResult(value != null);
    }
}