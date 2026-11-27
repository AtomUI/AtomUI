namespace AtomUI.Desktop.Controls;

/// <summary>
/// Carries the completed text of a one-time password input.
/// </summary>
public class OtpLineEditCompletedEventArgs : EventArgs
{
    /// <summary>
    /// Creates the event arguments.
    /// </summary>
    /// <param name="text">
    /// The complete text of the input.
    /// </param>
    public OtpLineEditCompletedEventArgs(string text)
    {
        Text = text;
    }

    /// <summary>
    /// The complete text of the input when it was completed.
    /// </summary>
    public string Text { get; }
}
