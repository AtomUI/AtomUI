namespace AtomUI.Desktop.Controls;

public class TransferViewCreatedEventArgs : EventArgs
{
    public ITransferView TransferView { get; }

    public TransferViewCreatedEventArgs(ITransferView transferView)
    {
        TransferView = transferView;
    }
}