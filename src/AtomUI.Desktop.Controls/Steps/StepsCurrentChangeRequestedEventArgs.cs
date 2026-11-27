namespace AtomUI.Desktop.Controls;

/// <summary>
/// Data of the <see cref="Steps.CurrentChangeRequested" /> event of a <see cref="Steps" /> control: the number
/// of the step that asks to become the current step.
/// </summary>
public sealed class StepsCurrentChangeRequestedEventArgs : EventArgs
{
    /// <summary>
    /// Creates the event data for the request to make the step numbered <paramref name="current" /> current.
    /// </summary>
    /// <param name="current">
    /// The number of the step that asks to become the current step.
    /// </param>
    public StepsCurrentChangeRequestedEventArgs(int current)
    {
        Current = current;
    }

    /// <summary>
    /// Number of the step that asks to become the current step: the value to assign to
    /// <see cref="Steps.Current" />. It is never the number of the current step, because the control drops a
    /// request for the current step.
    /// </summary>
    public int Current { get; }
}
