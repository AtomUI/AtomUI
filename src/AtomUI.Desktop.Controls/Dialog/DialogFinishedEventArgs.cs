namespace AtomUI.Desktop.Controls;

/// <summary>
/// Carries the result of the close request that ended a dialog, for the <see cref="Dialog.Finished" /> event.
/// </summary>
public class DialogFinishedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the result of the close request: the <see cref="DialogCode" /> when the dialog was accepted or
    /// rejected, and otherwise the value that application code supplied, or <see langword="null" /> when there
    /// is none.
    /// </summary>
    public object? Result { get; }
    /// <summary>
    /// Creates the event data for a close that ended with the given result.
    /// </summary>
    /// <param name="result">
    /// The result of the close request. See <see cref="Result" />.
    /// </param>
    public DialogFinishedEventArgs(object? result)
    {
        Result = result;
    }
}