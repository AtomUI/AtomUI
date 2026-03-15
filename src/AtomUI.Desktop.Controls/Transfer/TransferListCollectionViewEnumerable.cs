using System.Collections;
using AtomUI.Controls.Data;

namespace AtomUI.Desktop.Controls;

internal class TransferListCollectionViewEnumerable : IEnumerable
{
    private readonly ListCollectionView _listView;
    public TransferListCollectionViewEnumerable(ListCollectionView listView)
    {
        _listView = listView;
    }
    
    public IEnumerator GetEnumerator() => _listView.GetAllRangeEnumerator();
}