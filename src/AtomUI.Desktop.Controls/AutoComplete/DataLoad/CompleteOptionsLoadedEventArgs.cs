namespace AtomUI.Desktop.Controls;

/// <summary>
/// Data of the <see cref="AbstractAutoComplete.OptionsLoaded" /> event.
/// </summary>
public class CompleteOptionsLoadedEventArgs : EventArgs
{
    /// <summary>
    /// The text the options were loaded for: the value of the input when the load was requested.
    /// </summary>
    public string? Predicate { get; }
    /// <summary>
    /// The outcome of the load. It is the result that the loader returned, unless the load was canceled, timed
    /// out or threw; then the control supplies a result with the status <c>Timeout</c>, <c>Cancelled</c> or
    /// <c>Unknown</c> and no data.
    /// </summary>
    public CompleteOptionsLoadResult Result { get; }
    
    /// <summary>
    /// Creates the arguments for the <see cref="AbstractAutoComplete.OptionsLoaded" /> event.
    /// </summary>
    /// <param name="predicate">
    /// The text the options were loaded for; stored as <see cref="Predicate" />.
    /// </param>
    /// <param name="result">
    /// The outcome of the load; stored as <see cref="Result" />.
    /// </param>
    public CompleteOptionsLoadedEventArgs(string? predicate, CompleteOptionsLoadResult result)
    {
        Predicate = predicate;
        Result    = result;
    }
}