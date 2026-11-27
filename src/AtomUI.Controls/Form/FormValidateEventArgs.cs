namespace AtomUI.Controls;

/// <summary>
/// Carries the outcome of a form validation for the <see cref="AbstractForm.Validated" /> event: the result,
/// and either the validated values or the messages of every field.
/// </summary>
public class FormValidatedEventArgs : EventArgs
{
    /// <summary>
    /// The values, keyed by field name, that were validated, or <see langword="null" /> when
    /// <see cref="Result" /> is <see cref="FormValidateResult.Error" />; that case reports the messages
    /// in <see cref="ErrorMessages" /> instead.
    /// </summary>
    public IFormValues? Values { get; }
    /// <summary>
    /// The overall result of the validation. With <see cref="FormValidateResult.Error" /> the event carries the
    /// messages; with <see cref="FormValidateResult.Success" /> it carries the values.
    /// </summary>
    public FormValidateResult Result { get; }
    /// <summary>
    /// Every error and warning message of every field, or <see langword="null" /> when
    /// <see cref="Result" /> is <see cref="FormValidateResult.Success" />.
    /// </summary>
    public IList<FormValidateMessage>? ErrorMessages { get; }
    
    /// <summary>
    /// Creates event args for a validation that ended with <paramref name="result" />.
    /// </summary>
    /// <param name="result">
    /// The overall result of the validation.
    /// </param>
    /// <param name="values">
    /// The values that were validated, or <see langword="null" /> to report none.
    /// </param>
    /// <param name="errorMessages">
    /// The messages of every field, or <see langword="null" /> to report none.
    /// </param>
    public FormValidatedEventArgs(FormValidateResult result, IFormValues? values, IList<FormValidateMessage>? errorMessages)
    {
        Values        = values;
        ErrorMessages = errorMessages;
        Result        = result;
    }
}