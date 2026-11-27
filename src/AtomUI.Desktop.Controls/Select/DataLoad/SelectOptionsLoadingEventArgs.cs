using System.ComponentModel;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Data for the <see cref="Select.OptionsLoading" /> event. The select raises the event with the
/// <see cref="Select.OptionsAsyncLoadContext" /> when an options load starts without an
/// <see cref="Select.OptionsLoader" />. Cancelling the event skips the <see cref="Select.OptionsLoaded" />
/// event that ends the load.
/// </summary>
public class SelectOptionsLoadingEventArgs : CancelEventArgs
{
    /// <summary>
    /// The <see cref="Select.OptionsAsyncLoadContext" /> of the select at the time of the load.
    /// </summary>
    public object? Context { get; }

    /// <summary>
    /// Creates the event data for a starting options load.
    /// </summary>
    /// <param name="context">
    /// The <see cref="Select.OptionsAsyncLoadContext" /> of the load.
    /// </param>
    public SelectOptionsLoadingEventArgs(object? context)
    {
        Context = context;
    }
}