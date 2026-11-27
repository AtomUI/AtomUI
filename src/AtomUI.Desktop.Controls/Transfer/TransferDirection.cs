namespace AtomUI.Desktop.Controls;

/// <summary>
/// Names the panel that a transfer moves the checked items to. The operation buttons of the theme carry it as
/// their <c>Tag</c>.
/// </summary>
public enum TransferDirection
{
    /// <summary>
    /// Moves the items checked in the target panel back to the source panel.
    /// </summary>
    ToSource,
    /// <summary>
    /// Moves the items checked in the source panel to the target panel.
    /// </summary>
    ToTarget,
}