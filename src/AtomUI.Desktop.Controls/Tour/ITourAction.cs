namespace AtomUI.Desktop.Controls;

/// <summary>
/// Contract for a custom action of a tour: a control added to <see cref="Tour.CustomActions" /> that is shown in
/// the footer in front of the built-in navigation buttons. The tour keeps <see cref="StepCount" />,
/// <see cref="ActiveIndex" /> and <see cref="StyleType" /> of the control in sync with its own state: when the
/// control is added, and whenever the step count, the active step or the style type changes.
/// </summary>
public interface ITourAction
{
    /// <summary>
    /// The number of steps of the tour.
    /// </summary>
    int StepCount { get; set; }
    /// <summary>
    /// The zero-based index of the step that is currently active.
    /// </summary>
    int ActiveIndex { get; set; }
    /// <summary>
    /// The style type that the tour currently uses.
    /// </summary>
    TourStyleType StyleType { get; set; }

    /// <summary>
    /// Called with the tour that shows the control whenever the tour passes its custom actions to its steps view.
    /// It lets the control reach that tour. The default implementation does nothing.
    /// </summary>
    public void NotifyAttached(Tour tour) {}
}