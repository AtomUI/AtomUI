namespace AtomUI.Desktop.Controls;

/// <summary>
/// The pointer event that selects a tab.
/// </summary>
public enum TabActivationTrigger
{
    /// <summary>
    /// A tab is selected when the left mouse button is released on it. This is the default.
    /// </summary>
    PointerReleased,
    /// <summary>
    /// A tab is selected when the left mouse button is pressed on it.
    /// </summary>
    PointerPressed
}
