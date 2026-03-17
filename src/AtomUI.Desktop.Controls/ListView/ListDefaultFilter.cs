using AtomUI.Controls.Data;

namespace AtomUI.Desktop.Controls;

internal class ListDefaultFilter
{
    private readonly WeakReference<IListCollectionView> CollectionViewRef;
    
    public ListDefaultFilter(IListCollectionView collectionView)
    {
        CollectionViewRef = new WeakReference<IListCollectionView>(collectionView);
    }
    
    public bool Filter(object value)
    {
        if (CollectionViewRef.TryGetTarget(out var collectionView))
        {
            var filterDescriptions = collectionView.FilterDescriptions;
            if (filterDescriptions.Count == 0)
            {
                return true;
            }
            
            foreach (var filterDescription in filterDescriptions)
            {
                if (!filterDescription.FilterBy(value))
                {
                    return false;
                }
            }
            return true;
        }
        return false;
    }
    
    public static implicit operator Func<object, bool>(ListDefaultFilter filter)
    {
        return filter.Filter;
    }
}