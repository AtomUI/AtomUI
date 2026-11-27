namespace AtomUI.Controls;

/// <summary>
/// Lets a form item use a control as its value control: the item reads and writes the value through the
/// control and tells it the validation status.
/// The value returned by <see cref="GetFormValue" /> is written back with <see cref="SetFormValue" /> when the
/// form sets its values.
/// </summary>
public interface IFormItemAware
{
    /// <summary>
    /// Occurs after the value of the control changed, so <see cref="GetFormValue" /> returns the new value to the
    /// handlers.
    /// </summary>
    /// <remarks>
    /// The form item that holds the control publishes the new value to its owner form and, with the validate
    /// trigger <see cref="FormValidateTrigger.OnChanged" />, starts a validation.
    /// A <see cref="ValueChanged" /> raised for a value written with <see cref="SetFormValue" /> or
    /// <see cref="ClearFormValue" /> starts no validation and does not mark the item as touched by the user.
    /// </remarks>
    event EventHandler ValueChanged;
    /// <summary>
    /// Writes a value into the control. The form item calls it for values that do not come from the user: a value
    /// set through the item and the values the form writes when it sets or resets them.
    /// </summary>
    /// <param name="value">
    /// The new value, of the type that <see cref="GetFormValue" /> returns.
    /// </param>
    void SetFormValue(object? value);
    /// <summary>
    /// Returns the current value of the control, which is the value of the form item and of its field in the form
    /// values.
    /// </summary>
    object? GetFormValue();
    /// <summary>
    /// Clears the value of the control. The form item calls it when it is reset and has no initial value to
    /// restore.
    /// </summary>
    void ClearFormValue();
    /// <summary>
    /// Tells the control the validation status of the form item that holds it. The item calls it whenever it
    /// updates its validation state, also with <see cref="FormValidateStatus.Validating" /> while a validation runs.
    /// </summary>
    void NotifyValidateStatus(FormValidateStatus status);
}

/// <summary>
/// Lets a form item hand its content the control that visualizes the validation state of the item. The item
/// does so only while feedback for the content is enabled, see
/// <see cref="AbstractFormItem.IsValidateFeedbackEnabled" />.
/// </summary>
public interface IFormItemFeedbackAware
{
    /// <summary>
    /// Receives the control that shows the validation messages of the form item.
    /// </summary>
    /// <param name="value">
    /// The feedback control, or <see langword="null" /> when the item provides none.
    /// </param>
    void SetFeedbackControl(FormValidateFeedback? value);
}