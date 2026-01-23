namespace AtomUI.Desktop.Controls;

public class CascaderItemSelectedEventArgs : EventArgs
{
    public object Item { get; }

    public CascaderItemSelectedEventArgs(object item)
    {
        Item = item;
    }
}