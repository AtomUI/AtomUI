namespace AtomUI.Controls;

public class SelectionCountChangedEventArgs
{
    public int Count { get; }
    
    public SelectionCountChangedEventArgs(int count)
    {
        Count = count;
    }
}