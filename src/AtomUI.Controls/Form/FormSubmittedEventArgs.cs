using AtomUI.Controls;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Carries the values that a form validated for the <see cref="AbstractForm.Submitted" /> event.
/// </summary>
public class FormSubmittedEventArgs : EventArgs
{
    /// <summary>
    /// The values, keyed by field name, that were validated by the submit that raised the event.
    /// </summary>
    public IFormValues Values { get; }
    
    /// <summary>
    /// Creates event args that carry <paramref name="values" />.
    /// </summary>
    public FormSubmittedEventArgs(IFormValues values)
    {
        Values = values;
    }
}