namespace AtomUI.Desktop.Controls;

public class CascaderOptionSelectedEventArgs : EventArgs
{
    public ICascaderOption Option { get; }

    public CascaderOptionSelectedEventArgs(ICascaderOption option)
    {
        Option = option;
    }
}