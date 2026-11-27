namespace AtomUI.Desktop.Controls;

/// <summary>
/// The commands of the selection menu in the title bar of a transfer panel. Every command is passed to
/// the view of the panel through <see cref="ITransferView.NotifySelectAction" />. The panel also applies
/// <see cref="SelectAll" /> and <see cref="DeselectAll" /> itself, and the transfer applies
/// <see cref="RemoveAll" />.
/// </summary>
public enum TransferSelectAction
{
    /// <summary>
    /// Selects all rows of the panel.
    /// </summary>
    SelectAll,
    /// <summary>
    /// Clears the selection of the panel.
    /// </summary>
    DeselectAll,
    /// <summary>
    /// Selects the rows of the current page, or all rows when the panel is not paged.
    /// </summary>
    SelectCurrentPage,
    /// <summary>
    /// Switches the selection state of each row on the current page.
    /// </summary>
    InvertSelectCurrentPage,
    /// <summary>
    /// Removes the rows of the current page from the target panel, which moves them back to the source panel.
    /// </summary>
    RemoveCurrentPage,
    /// <summary>
    /// Removes every row from the target panel, which moves them all back to the source panel.
    /// </summary>
    RemoveAll
}