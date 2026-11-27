namespace AtomUI.Controls;

/// <summary>
/// Carries the previous and the new rating of a rate control.
/// </summary>
/// <remarks>
/// The scale of the two values depends on the event. For <c>AbstractRate.ValueChanged</c> they are the previous
/// and the new rating of the control. For <c>AbstractRate.HoverValueChanged</c> they are the zero-based index of
/// the last star covered by the previous and the new rating, or -1 for a rating of 0.
/// </remarks>
public class RateValueChangedEventArgs : EventArgs
{
    /// <summary>
    /// The previous rating; for its scale see the remarks of <see cref="RateValueChangedEventArgs" />.
    /// </summary>
    public double OldValue { get; }
    /// <summary>
    /// The new rating; for its scale see the remarks of <see cref="RateValueChangedEventArgs" />.
    /// </summary>
    /// <value>
    /// A handler may replace the value, and later handlers then receive the replaced one. The control does not read
    /// it back.
    /// </value>
    public double NewValue { get; set; }

    /// <summary>
    /// Initializes the arguments with a previous and a new rating.
    /// </summary>
    /// <param name="oldValue">
    /// The rating before the change.
    /// </param>
    /// <param name="newValue">
    /// The rating after the change.
    /// </param>
    public RateValueChangedEventArgs(double oldValue, double newValue)
    {
        OldValue = oldValue;
        NewValue = newValue;
    }
}