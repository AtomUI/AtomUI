namespace AtomUI.Controls;

/// <summary>
/// The outcome of a validation: of one validator, of a form item or of the whole form.
/// </summary>
public enum FormValidateResult
{
    /// <summary>
    /// The value passed the validation.
    /// </summary>
    Success,
    /// <summary>
    /// The value failed the validation. Its message is shown as an error and the item does not count as valid.
    /// </summary>
    Error,
    /// <summary>
    /// The value passed with a warning. Its message is shown as a warning and the item still counts as valid.
    /// </summary>
    Warning,
}

/// <summary>
/// A validation rule that a form item runs on its value.
/// <see cref="AbstractFormValidator" /> is the base implementation.
/// </summary>
public interface IFormValidator
{
    /// <summary>
    /// Gets the text shown when the validator reports an error or a warning.
    /// When it is <see langword="null" /> or blank, the localized default message of the validator is used.
    /// <c>{label}</c> in the text is replaced by the label of the item, or by its field name when it has no label.
    /// </summary>
    string? Message { get; }
    /// <summary>
    /// Gets whether a failed check is reported as <see cref="FormValidateResult.Warning" /> instead of
    /// <see cref="FormValidateResult.Error" />. A warning does not make the item invalid for the form.
    /// The form item takes the outcome from <see cref="ValidateAsync" /> and does not read this property itself.
    /// </summary>
    bool WarningOnly { get; }
    /// <summary>
    /// Checks a value and reports the outcome. A form item calls it each time the item is validated.
    /// </summary>
    /// <param name="fieldName">
    /// The field name of the validated item, or an empty string when it has none.
    /// </param>
    /// <param name="value">
    /// The value to check: the current value of the item, which may be <see langword="null" />.
    /// </param>
    /// <param name="cancellationToken">
    /// Cancels the check. The item discards the outcome of a canceled run.
    /// </param>
    /// <returns>
    /// <see cref="FormValidateResult.Success" /> when the value passes, otherwise
    /// <see cref="FormValidateResult.Error" /> or <see cref="FormValidateResult.Warning" />.
    /// </returns>
    Task<FormValidateResult> ValidateAsync(string fieldName, object? value, CancellationToken cancellationToken);
}