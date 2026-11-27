namespace AtomUI.Desktop.Controls;

/// <summary>
/// The button of a transfer list row that requests the removal of the item of the row. It is visible when the row
/// is not checkable. Its click makes <see cref="TransferListView" /> raise
/// <see cref="TransferListView.ItemsRemoved" /> with the item of the row.
/// </summary>
public class TransferRemoveItemButton : IconButton
{
    /// <summary>
    /// Returns the type of <see cref="IconButton" />, so that the button is styled by the icon button theme instead
    /// of a theme of its own.
    /// </summary>
    protected override Type StyleKeyOverride => typeof(IconButton);
}