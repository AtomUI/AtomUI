using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using Avalonia.Collections;

namespace AtomUI.Controls.Data;

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
    int TotalItemCount { get; }
    int PageSize { get; set; }
    int PageIndex { get; }
    IAvaloniaReadOnlyList<object>? Groups { get; }
    GroupDescriptionList GroupDescriptions { get; }
    SortDescriptionList SortDescriptions { get; }
    FilterDescriptionList FilterDescriptions { get; }
    bool IsEmpty { get; }
    void Refresh();
    IDisposable DeferRefresh();
    object AddNew();
    void CancelNew();
    void CommitNew();
    void Remove(object? item);
    void RemoveAt(int index);
    int Count { get; }
    int IndexOf(object? item);
    int? PageIndexOf(object? item);
    object? GetItemAt(int index);
    bool MoveToPage(int pageIndex);
    bool MoveToFirstPage();
    bool MoveToLastPage();
    bool MoveToNextPage();
    bool MoveToPreviousPage();
    
    event EventHandler<EventArgs>? PageChanged;
    event EventHandler<PageChangingEventArgs>? PageChanging;
    event PropertyChangedEventHandler? PropertyChanged;
}