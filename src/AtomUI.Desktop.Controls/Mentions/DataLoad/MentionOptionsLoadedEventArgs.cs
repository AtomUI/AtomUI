namespace AtomUI.Desktop.Controls;

/// <summary>
/// Data for the <see cref="Mentions.OptionsLoaded" /> event, which the control raises when an asynchronous
/// load of the candidates ended with a result, whether it succeeded, failed, timed out or was canceled.
/// A load that a newer one superseded raises nothing.
/// </summary>
public class MentionOptionsLoadedEventArgs : EventArgs
{
    /// <summary>
    /// The text the candidates were loaded for; <see langword="null" /> when there was no such text.
    /// </summary>
    public string? Predicate;
    /// <summary>
    /// The outcome of the load, which carries the candidates on success.
    /// </summary>
    public MentionOptionsLoadResult Result { get; }
    
    /// <summary>
    /// Creates the arguments for a finished load.
    /// </summary>
    /// <param name="predicate">
    /// The text the candidates were loaded for; stored as <see cref="Predicate" />.
    /// </param>
    /// <param name="result">
    /// The outcome of the load; stored as <see cref="Result" />.
    /// </param>
    public MentionOptionsLoadedEventArgs(string? predicate, MentionOptionsLoadResult result)
    {
        Predicate = predicate;
        Result    = result;
    }
}