using AtomUI.Controls.Data;

namespace AtomUI.Desktop.Controls;

public class ListCollectionViewChangedEventArgs : EventArgs
{
    public IListCollectionView? OldValue { get; }
    public IListCollectionView? NewValue { get; }

    public ListCollectionViewChangedEventArgs(IListCollectionView? oldValue, IListCollectionView? newValue)
    {
        OldValue = oldValue;
        NewValue = newValue;
    }
}