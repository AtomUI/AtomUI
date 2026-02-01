using System.Collections;
using System.Collections.Specialized;
using System.Globalization;
using Avalonia.Collections;

namespace AtomUI.Desktop.Controls.Data;

public interface IListCollectionView: IEnumerable, INotifyCollectionChanged
{
    CultureInfo Culture { get; set; }
    bool Contains(object item);
    IEnumerable SourceCollection { get; }
    Func<object, bool>? Filter { get; set; }
    bool CanFilter { get; }
    bool CanSort { get; }
    bool CanGroup { get; }
    bool IsGrouping { get; }
    int GroupingDepth { get; }
    IAvaloniaReadOnlyList<object>? Groups { get; }
    ListSortDescriptionList SortDescriptions { get; }
    ListFilterDescriptionList FilterDescriptions { get; }
    bool IsEmpty { get; }
    void Refresh();
    IDisposable DeferRefresh();
    object AddNew();
    void CancelNew();
    void CommitNew();
    void Remove(object? item);
    void RemoveAt(int index);
    int Count { get; }
}