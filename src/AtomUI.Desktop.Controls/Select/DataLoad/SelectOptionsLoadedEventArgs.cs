namespace AtomUI.Desktop.Controls;

/// <summary>
/// Data for the <see cref="Select.OptionsLoaded" /> event. The select raises the event when an options load
/// finished: after the loaded options became its <see cref="Select.OptionsSource" />, or with a failed result
/// when the loader threw or timed out. The dropdown opens or closes after the event.
/// </summary>
public class SelectOptionsLoadedEventArgs
{
    /// <summary>
    /// The <see cref="Select.OptionsAsyncLoadContext" /> of the select at the time of the load.
    /// </summary>
    public object? Context { get; }
    /// <summary>
    /// The outcome of the load, or <see langword="null" /> when no <see cref="Select.OptionsLoader" /> was set.
    /// </summary>
    public SelectOptionsLoadResult? Result { get; }

    /// <summary>
    /// Creates the event data for a finished options load.
    /// </summary>
    /// <param name="context">
    /// The <see cref="Select.OptionsAsyncLoadContext" /> of the load.
    /// </param>
    /// <param name="result">
    /// The outcome of the load, or <see langword="null" /> when no <see cref="Select.OptionsLoader" /> was set.
    /// </param>
    public SelectOptionsLoadedEventArgs(object? context, SelectOptionsLoadResult? result)
    {
        Context = context;
        Result  = result;
    }
}