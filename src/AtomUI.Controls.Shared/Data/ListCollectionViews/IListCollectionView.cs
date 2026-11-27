using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using Avalonia.Collections;

namespace AtomUI.Controls.Data;

/// <summary>
/// A view over a source collection that applies filtering, sorting, grouping and paging. It exposes the items that
/// pass <see cref="Filter" />, ordered by <see cref="SortDescriptions" />, and limits them to the page given by
/// <see cref="PageIndex" /> when <see cref="PageSize" /> is greater than zero. Indexes are relative to this view,
/// not to the source collection.
/// </summary>
/// <remarks>
/// <para>
/// While the view is grouped, <see cref="Count" />, <see cref="IndexOf" /> and enumeration address the leaf items
/// below the groups.
/// </para>
/// </remarks>
public interface IListCollectionView: IEnumerable, INotifyCollectionChanged
{
    /// <summary>
    /// Gets or sets the culture the view passes to group descriptions when it computes group keys.
    /// </summary>
    /// <exception cref="ArgumentNullException">
    /// The assigned value is <see langword="null" />.
    /// </exception>
    /// <remarks>
    /// The view does not re-group or re-sort when the culture changes. A sort description receives its culture when it
    /// is created, not from this property.
    /// </remarks>
    CultureInfo Culture { get; set; }
    /// <summary>
    /// Returns whether the item is in the current view, that is, it passes the filter and lies on the current page.
    /// </summary>
    /// <param name="item">
    /// The item to find.
    /// </param>
    /// <returns>
    /// <see langword="true" /> when the item is in the view; otherwise <see langword="false" />.
    /// </returns>
    bool Contains(object item);
    /// <summary>
    /// Gets the collection the view was created over, unaffected by the filter, the sort criteria or paging.
    /// </summary>
    IEnumerable SourceCollection { get; }
    /// <summary>
    /// Gets or sets the predicate that decides which items the view exposes: an item is exposed when the predicate
    /// returns <see langword="true" /> for it. <see langword="null" /> removes the filter, so that all items pass.
    /// </summary>
    /// <exception cref="NotSupportedException">
    /// <see cref="CanFilter" /> is <see langword="false" />.
    /// </exception>
    /// <remarks>
    /// Assigning a value rebuilds the view.
    /// </remarks>
    Func<object, bool>? Filter { get; set; }
    /// <summary>
    /// Gets whether the view accepts a predicate through <see cref="Filter" />. When it returns
    /// <see langword="false" />, assigning <see cref="Filter" /> throws <see cref="NotSupportedException" />.
    /// </summary>
    bool CanFilter { get; }
    /// <summary>
    /// Gets whether the view applies the criteria added to <see cref="SortDescriptions" />.
    /// </summary>
    bool CanSort { get; }
    /// <summary>
    /// Gets whether the view applies <see cref="GroupDescriptions" />. When it returns <see langword="false" />, the
    /// grouping members of the view have no effect.
    /// </summary>
    bool CanGroup { get; }
    /// <summary>
    /// Gets whether the view is grouped, which is the case when <see cref="GroupDescriptions" /> contains a
    /// description. While it is grouped, <see cref="Groups" /> holds the top-level groups.
    /// </summary>
    bool IsGrouping { get; }
    /// <summary>
    /// Gets the number of group levels, which is the number of entries in <see cref="GroupDescriptions" />.
    /// </summary>
    int GroupingDepth { get; }
    /// <summary>
    /// Gets the number of items that pass the filter, before paging is applied.
    /// </summary>
    int TotalItemCount { get; }
    /// <summary>
    /// Gets or sets the number of items on a page. Zero turns paging off: the view exposes all filtered items and
    /// <see cref="PageIndex" /> is -1. A value greater than zero turns paging on and moves to the first page.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// The assigned value is negative.
    /// </exception>
    /// <remarks>
    /// Changing the value raises <see cref="INotifyCollectionChanged.CollectionChanged" /> with a reset. While a
    /// <see cref="DeferRefresh" /> cycle is active, the value is stored and applied when the cycle ends.
    /// </remarks>
    int PageSize { get; set; }
    /// <summary>
    /// Gets the zero-based index of the current page, or -1 when paging is off.
    /// </summary>
    int PageIndex { get; }
    /// <summary>
    /// Gets the top-level groups built from <see cref="GroupDescriptions" />, or <see langword="null" /> when the view
    /// is not grouped. Each group is a <see cref="ListCollectionViewGroup" />. When paging is on, the groups cover
    /// only the current page.
    /// </summary>
    IAvaloniaReadOnlyList<object>? Groups { get; }
    /// <summary>
    /// Gets the group descriptions, one per nesting level: the first entry divides the items into top-level groups, the
    /// next entry divides those groups, and so on. Adding or removing a description rebuilds the groups.
    /// </summary>
    GroupDescriptionList GroupDescriptions { get; }
    /// <summary>
    /// Gets the sort criteria. The view compares items by the first criterion and, when that is equal, by the next
    /// ones.
    /// </summary>
    /// <remarks>
    /// Adding or removing a criterion re-sorts the view and, when paging is on, moves to the first page.
    /// </remarks>
    SortDescriptionList SortDescriptions { get; }
    /// <summary>
    /// Gets the filter descriptions recorded for the view. The view does not evaluate them: only <see cref="Filter" />
    /// decides what an item passes. They serve a predicate that reads them instead, such as the one
    /// <c>ListView</c> installs by default, which accepts an item when every description accepts it.
    /// </summary>
    /// <remarks>
    /// Adding or removing a description rebuilds the view and, when paging is on, moves to the first page.
    /// </remarks>
    FilterDescriptionList FilterDescriptions { get; }
    /// <summary>
    /// Gets whether no item passes the filter, before paging is applied.
    /// </summary>
    bool IsEmpty { get; }
    /// <summary>
    /// Rebuilds the view from the source collection with the current filter, sort criteria and groups, and raises
    /// <see cref="INotifyCollectionChanged.CollectionChanged" /> with a reset.
    /// </summary>
    void Refresh();
    /// <summary>
    /// Starts a deferral cycle. Until the returned object is disposed, the rebuilds that changes to
    /// <see cref="Filter" /> and to the sort, filter and group descriptions would trigger are merged into a single
    /// rebuild, which runs when the outermost cycle ends.
    /// </summary>
    /// <returns>
    /// An object that ends the cycle when it is disposed.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// An <see cref="AddNew" /> transaction is open.
    /// </exception>
    /// <remarks>
    /// While a cycle is active, <see cref="Count" />, <see cref="Contains" />, enumeration, <see cref="GetItemAt" />,
    /// <see cref="IndexOf" /> and <see cref="PageIndexOf" /> throw <see cref="InvalidOperationException" />, as do
    /// <see cref="AddNew" />, <see cref="CancelNew" />, <see cref="CommitNew" />, <see cref="Remove" /> and
    /// <see cref="RemoveAt" />.
    /// </remarks>
    IDisposable DeferRefresh();
    /// <summary>
    /// Starts an add transaction: it creates a new item of the item type, adds it to the source collection and shows it
    /// at the end of the view, in place of the page's last item when the current page is full. End the transaction with
    /// <see cref="CommitNew" /> or <see cref="CancelNew" />.
    /// </summary>
    /// <returns>
    /// The new item.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// A new item cannot be created for the item type, the source collection is not a resizable list, or a
    /// <see cref="DeferRefresh" /> cycle is active.
    /// </exception>
    /// <remarks>
    /// <para>
    /// A transaction that is still open is committed first.
    /// </para>
    /// <para>
    /// While the transaction is open, <see cref="Filter" /> cannot be assigned and the sort, filter and group
    /// descriptions cannot be changed: doing so throws <see cref="InvalidOperationException" />.
    /// </para>
    /// </remarks>
    object AddNew();
    /// <summary>
    /// Ends the add transaction by removing the new item from the source collection. It does nothing when no
    /// transaction is open. An item that implements <see cref="IEditableObject" /> receives
    /// <see cref="IEditableObject.CancelEdit" />.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// A <see cref="DeferRefresh" /> cycle is active.
    /// </exception>
    void CancelNew();
    /// <summary>
    /// Ends the add transaction and keeps the new item in the source collection. The filter, the sort criteria and
    /// paging are then applied to the item, so it can move to another position or leave the view. It does nothing when
    /// no transaction is open. An item that implements <see cref="IEditableObject" /> receives
    /// <see cref="IEditableObject.EndEdit" />.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// A <see cref="DeferRefresh" /> cycle is active.
    /// </exception>
    void CommitNew();
    /// <summary>
    /// Removes the item from the source collection, provided that it is in the current view: it must pass the filter
    /// and lie on the current page. An item outside the view is left untouched.
    /// </summary>
    /// <param name="item">
    /// The item to remove.
    /// </param>
    /// <exception cref="InvalidOperationException">
    /// An <see cref="AddNew" /> transaction is open, the source collection is not a resizable list, or a
    /// <see cref="DeferRefresh" /> cycle is active.
    /// </exception>
    void Remove(object? item);
    /// <summary>
    /// Removes the item at the given position in the view from the source collection. When paging is on and a next page
    /// exists, its first item moves into the current page. When the current page becomes empty and is not the first
    /// page, the view moves to the previous page.
    /// </summary>
    /// <param name="index">
    /// The zero-based position of the item in the view, after filtering, sorting and paging.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index" /> is negative or not less than <see cref="Count" />.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// An <see cref="AddNew" /> transaction is open, the source collection is not a resizable list, or a
    /// <see cref="DeferRefresh" /> cycle is active.
    /// </exception>
    void RemoveAt(int index);
    /// <summary>
    /// Gets the number of items in the view: those that pass the filter and, when paging is on, only those of the
    /// current page. When the view is grouped, the leaf items below the groups are counted.
    /// </summary>
    int Count { get; }
    /// <summary>
    /// Returns the position of the item in the view, or -1 when the item is filtered out or lies on another page.
    /// </summary>
    /// <param name="item">
    /// The item to find.
    /// </param>
    /// <returns>
    /// The zero-based index of the item in the view, or -1.
    /// </returns>
    int IndexOf(object? item);
    /// <summary>
    /// Returns the zero-based index of the page that holds the item. The index is computed over all filtered and sorted
    /// items, so it is also returned for an item that lies outside the current page.
    /// </summary>
    /// <param name="item">
    /// The item to find.
    /// </param>
    /// <returns>
    /// The index of the page, or <see langword="null" /> when paging is off, the view is grouped, or the item is not
    /// found.
    /// </returns>
    int? PageIndexOf(object? item);
    /// <summary>
    /// Returns the item at the given position in the view.
    /// </summary>
    /// <param name="index">
    /// The zero-based position in the view, after filtering, sorting and paging.
    /// </param>
    /// <returns>
    /// The item at the position, or <see langword="null" /> when the position holds no item.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index" /> is negative or not less than <see cref="Count" />.
    /// </exception>
    object? GetItemAt(int index);
    /// <summary>
    /// Moves to the page with the given index. <see cref="PageChanging" /> is raised before the move; cancelling it
    /// keeps the current page.
    /// </summary>
    /// <param name="pageIndex">
    /// The zero-based index of the target page.
    /// </param>
    /// <returns>
    /// <see langword="true" /> when the page index changed; otherwise <see langword="false" />.
    /// </returns>
    /// <remarks>
    /// <para>
    /// The method returns <see langword="false" /> without raising an event when the index is out of range, names the
    /// current page, or is -1 while paging is on. While a <see cref="DeferRefresh" /> cycle is active, the move is made
    /// when the cycle ends.
    /// </para>
    /// <para>
    /// While an <see cref="AddNew" /> transaction is open, the move is abandoned but <see cref="PageChanged" /> is
    /// still raised.
    /// </para>
    /// </remarks>
    bool MoveToPage(int pageIndex);
    /// <summary>
    /// Moves to page 0.
    /// </summary>
    /// <returns>
    /// Whether the page index changed. See <see cref="MoveToPage" /> for when a request is refused.
    /// </returns>
    bool MoveToFirstPage();
    /// <summary>
    /// Moves to the last page, derived from the number of filtered items and <see cref="PageSize" />. Nothing happens
    /// when paging is off.
    /// </summary>
    /// <returns>
    /// Whether the page index changed. See <see cref="MoveToPage" /> for when a request is refused.
    /// </returns>
    bool MoveToLastPage();
    /// <summary>
    /// Moves to the page after <see cref="PageIndex" />.
    /// </summary>
    /// <returns>
    /// Whether the page index changed. See <see cref="MoveToPage" /> for when a request is refused.
    /// </returns>
    bool MoveToNextPage();
    /// <summary>
    /// Moves to the page before <see cref="PageIndex" />.
    /// </summary>
    /// <returns>
    /// Whether the page index changed. See <see cref="MoveToPage" /> for when a request is refused.
    /// </returns>
    bool MoveToPreviousPage();
    
    /// <summary>
    /// Occurs after <see cref="PageIndex" /> changed, before the reset of
    /// <see cref="INotifyCollectionChanged.CollectionChanged" /> that reports the new page.
    /// </summary>
    /// <remarks>
    /// It is also raised when a move requested while an <see cref="AddNew" /> transaction is open was abandoned,
    /// although the page index did not change.
    /// </remarks>
    event EventHandler<EventArgs>? PageChanged;
    /// <summary>
    /// Occurs before <see cref="PageIndex" /> changes, with the requested page in
    /// <see cref="PageChangingEventArgs.NewPageIndex" />.
    /// </summary>
    /// <remarks>
    /// Setting <see cref="CancelEventArgs.Cancel" /> on the event arguments keeps the current page: the move returns
    /// <see langword="false" /> and <see cref="PageChanged" /> is not raised. A request that is invalid, such as an
    /// index out of range or the current page, is refused without raising this event.
    /// </remarks>
    event EventHandler<PageChangingEventArgs>? PageChanging;
    /// <summary>
    /// Occurs when a property of the view changes, such as <see cref="Count" />, <see cref="PageIndex" />,
    /// <see cref="PageSize" />, <see cref="Filter" /> or <see cref="Culture" />.
    /// </summary>
    event PropertyChangedEventHandler? PropertyChanged;
}