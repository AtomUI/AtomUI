namespace AtomUI.Desktop.Controls;

/// <summary>
/// Loads the options of an AutoComplete control on demand, for example from a service. Assign an
/// implementation to <see cref="AbstractAutoComplete.OptionsAsyncLoader" />; the control then calls
/// <see cref="LoadAsync" /> for the current input text each time it populates its options.
/// </summary>
public interface ICompleteOptionsAsyncLoader
{
    /// <summary>
    /// Loads the options for a text and returns them when the load has finished.
    /// </summary>
    /// <param name="context">
    /// The text to load the options for: the value of the input when the load was requested.
    /// </param>
    /// <param name="token">
    /// Canceled when the control stops waiting for this load, for example because a newer request replaced it or
    /// because <see cref="AbstractAutoComplete.AsyncLoadTimeout" /> elapsed.
    /// </param>
    /// <returns>
    /// The loaded options with a successful status, or a result that states why none were loaded.
    /// </returns>
    /// <remarks>
    /// The control drops the result of a load that was canceled or timed out, and reports a load that threw as
    /// failed with the exception message. It does not read the status of a returned result; see
    /// <see cref="CompleteOptionsLoadResult.StatusCode" />.
    /// </remarks>
    Task<CompleteOptionsLoadResult> LoadAsync(string? context, CancellationToken token);
}