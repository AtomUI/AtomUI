using Avalonia.Interactivity;

namespace AtomUI.Controls;

/// <summary>
/// Carries the validation status that a form item reached when a validation of the item finished,
/// for the <see cref="AbstractFormItem.ValidateChanged" /> event.
/// </summary>
public class FormItemValidateChangedEventArgs : RoutedEventArgs
{
    /// <summary>
    /// Gets the validation status of the item after the finished validation.
    /// </summary>
    public FormValidateStatus Status { get; }
    /// <summary>
    /// Creates the arguments for a finished validation.
    /// </summary>
    /// <param name="status">
    /// The validation status of the item after the validation.
    /// </param>
    public FormItemValidateChangedEventArgs(FormValidateStatus status)
    {
        Status = status;
    }
}