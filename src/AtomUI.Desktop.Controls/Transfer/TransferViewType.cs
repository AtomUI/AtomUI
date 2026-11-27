namespace AtomUI.Desktop.Controls;

/// <summary>
/// Names the side of a transfer that a view belongs to: the panel with the items to choose from, or the panel
/// with the chosen items.
/// </summary>
public enum TransferViewType
{
    /// <summary>
    /// The panel that lists the items to choose from.
    /// </summary>
    Source,
    /// <summary>
    /// The panel that lists the chosen items.
    /// </summary>
    Target,
}