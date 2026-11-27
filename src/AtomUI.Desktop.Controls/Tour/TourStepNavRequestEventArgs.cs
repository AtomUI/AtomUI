namespace AtomUI.Desktop.Controls;

/// <summary>
/// Data for the <c>NavRequest</c> event of <c>TourStepsView</c>, raised when the built-in previous or next button
/// of the step view is clicked. It names the step that the tour is asked to show.
/// </summary>
public class TourStepNavRequestEventArgs : EventArgs
{
    /// <summary>
    /// Zero-based index of the step that the tour is asked to show.
    /// </summary>
    public int Index { get; }

    /// <summary>
    /// Creates a request to show the step at <paramref name="index" />.
    /// </summary>
    /// <param name="index">
    /// Zero-based index of the step to show.
    /// </param>
    public TourStepNavRequestEventArgs(int index)
    {
        Index = index;
    }
}