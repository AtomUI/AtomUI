namespace AtomUI.Controls;

public class ItemCountChangedEventArgs : EventArgs
{
    public int ItemCount { get; }
    
    public ItemCountChangedEventArgs(int itemCount)
    {
        ItemCount = itemCount;
    }
}