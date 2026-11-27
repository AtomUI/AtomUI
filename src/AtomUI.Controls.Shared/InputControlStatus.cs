namespace AtomUI.Controls;

/// <summary>
/// The validation state that an input control paints its frame with. Input controls derive the state they paint
/// from this value, their validation errors and the form validation status they hold: validation errors or an
/// error form status show <see cref="Error" />, otherwise a warning value or form status shows
/// <see cref="Warning" />, otherwise the value is painted as it is.
/// </summary>
public enum InputControlStatus
{
    /// <summary>
    /// The frame is painted without a validation state.
    /// </summary>
    Default,
    /// <summary>
    /// The frame is painted in the warning colors.
    /// </summary>
    Warning,
    /// <summary>
    /// The frame is painted in the error colors.
    /// </summary>
    Error
}
