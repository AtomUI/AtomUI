namespace AtomUI.Controls;

/// <summary>
/// Base class of a validator. A derived class overrides <see cref="ValidateCoreAsync" /> with its rule; this class
/// turns the verdict into the <see cref="FormValidateResult" /> that <see cref="IFormValidator.ValidateAsync" />
/// reports.
/// </summary>
public abstract class AbstractFormValidator : IFormValidator
{
    /// <summary>
    /// The text shown when the validator reports an error or a warning. When it is <see langword="null" /> or blank,
    /// the localized default message of the validator is used; a validator that derives from none of the built-in
    /// validators gets the generic invalid-value message. Within the text, <c>{label}</c> is replaced by the label of
    /// the item, or by its field name when it has no label.
    /// </summary>
    public string? Message { get; set; }
    /// <summary>
    /// Whether a failed check is reported as <see cref="FormValidateResult.Warning" /> instead of
    /// <see cref="FormValidateResult.Error" />. A warning does not make the item invalid for the form.
    /// </summary>
    public bool WarningOnly { get; set; }
    
    /// <summary>
    /// Runs <see cref="ValidateCoreAsync" /> and reports its verdict as the outcome of this validator.
    /// </summary>
    /// <param name="fieldName">
    /// The field name of the validated item, or an empty string when it has none.
    /// </param>
    /// <param name="value">
    /// The value to check: the current value of the item, which may be <see langword="null" />.
    /// </param>
    /// <param name="cancellationToken">
    /// Passed to <see cref="ValidateCoreAsync" /> unchanged. This method does not react to a cancellation itself.
    /// </param>
    /// <returns>
    /// <see cref="FormValidateResult.Success" /> when the rule returns <see langword="true" />. Otherwise
    /// <see cref="FormValidateResult.Warning" /> when <see cref="WarningOnly" /> is <see langword="true" />, and
    /// <see cref="FormValidateResult.Error" /> when it is not.
    /// </returns>
    public async Task<FormValidateResult> ValidateAsync(string fieldName, object? value, CancellationToken cancellationToken)
    {
        var isValid = await ValidateCoreAsync(fieldName, value, cancellationToken);
        if (isValid)
        {
            return await Task.FromResult(FormValidateResult.Success);
        }

        if (WarningOnly)
        {
            return await Task.FromResult(FormValidateResult.Warning);
        }
        return await Task.FromResult(FormValidateResult.Error);
    }

    /// <summary>
    /// Holds the rule of the validator. <see cref="ValidateAsync" /> calls it each time the form item is validated,
    /// with the arguments it received.
    /// </summary>
    /// <param name="fieldName">
    /// The field name of the validated item, or an empty string when it has none.
    /// </param>
    /// <param name="value">
    /// The value to check: the current value of the item, which may be <see langword="null" />.
    /// </param>
    /// <param name="cancellationToken">
    /// Cancels the check. Hand the token to every operation that the rule awaits, so that a canceled validation ends
    /// with an operation canceled exception instead of a verdict.
    /// </param>
    /// <returns>
    /// <see langword="true" /> when the value passes the rule, otherwise <see langword="false" />.
    /// </returns>
    protected abstract Task<bool> ValidateCoreAsync(string fieldName, object? value, CancellationToken cancellationToken);
}