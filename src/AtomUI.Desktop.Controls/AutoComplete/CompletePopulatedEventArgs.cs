namespace AtomUI.Desktop.Controls;

/// <summary>
/// Data of the <see cref="AbstractAutoComplete.Populated" /> event.
/// </summary>
public class CompletePopulatedEventArgs : EventArgs
{
    /// <summary>
    /// The options that were matched against the typed text when the event was raised. It is the control's own
    /// filtered list, the one that the suggestion popup shows, not a copy.
    /// </summary>
    public IList<IAutoCompleteOption>? Data { get; init; }
    
    /// <summary>
    /// Creates the arguments for the <see cref="AbstractAutoComplete.Populated" /> event.
    /// </summary>
    /// <param name="data">
    /// The matched options; stored as <see cref="Data" /> without being copied.
    /// </param>
    public CompletePopulatedEventArgs(IList<IAutoCompleteOption>? data)
    {
        Data = data;
    }
}