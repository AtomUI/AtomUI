namespace AtomUI.Desktop.Controls;

public class CascaderItemSelectedEventArgs : EventArgs
{
    public ICascaderOption Item { get; }

    public CascaderItemSelectedEventArgs(ICascaderOption item)
    {
        Item = item;
    }
}