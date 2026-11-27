namespace AtomUI.Desktop.Controls;

/// <summary>
/// Carries the transfer view that was created for one panel of a transfer.
/// </summary>
public class TransferViewCreatedEventArgs : EventArgs
{
    /// <summary>
    /// The created view. Its <see cref="ITransferView.ViewType" /> tells which panel it belongs to.
    /// </summary>
    public ITransferView TransferView { get; }

    /// <summary>
    /// Initializes the event data with the created view.
    /// </summary>
    /// <param name="transferView">
    /// The view that was created for the panel.
    /// </param>
    public TransferViewCreatedEventArgs(ITransferView transferView)
    {
        TransferView = transferView;
    }
}