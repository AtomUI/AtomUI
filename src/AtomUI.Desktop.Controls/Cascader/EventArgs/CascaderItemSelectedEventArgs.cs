namespace AtomUI.Desktop.Controls;

public class CascaderItemSelectedEventArgs : EventArgs
{
    public ICascaderViewOption Item { get; }

    public CascaderItemSelectedEventArgs(ICascaderViewOption item)
    {
        Item = item;
    }
}