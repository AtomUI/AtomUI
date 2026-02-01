using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using AtomUI.Desktop.Controls.Data;
using AtomUI.Utils;
using Avalonia.Collections;

namespace AtomUI.Desktop.Controls;

internal class ListCollectionView : IListCollectionView, IList, INotifyPropertyChanged
{
    #region 公共属性定义

    /// <summary>
    /// Gets a value indicating whether the view supports AddNew.
    /// </summary>
    public bool CanAddNew => (SourceList != null && !SourceList.IsFixedSize && CanConstructItem);

    public bool CanChangePage =>  true;
    
    /// <summary>
    /// Gets a value indicating whether we support filtering with this ICollectionView.
    /// </summary>
    public bool CanFilter => true;
    
    /// <summary>
    /// Gets a value indicating whether this view supports grouping.
    /// When this returns false, the rest of the interface is ignored.
    /// </summary>
    public bool CanGroup => true;
    
    /// <summary>
    /// Gets a value indicating whether the view supports Remove and RemoveAt.
    /// </summary>
    public bool CanRemove => !IsAddingNew &&
                             (SourceList != null && !SourceList.IsFixedSize);
    
    /// <summary>
    /// Gets a value indicating whether we support sorting with this ICollectionView.
    /// </summary>
    public bool CanSort => true;
    
    /// <summary>
    /// Gets the number of records in the view after 
    /// filtering, sorting, and paging.
    /// </summary>
    //TODO Paging
    public int Count
    {
        get
        {
            EnsureCollectionInSync();
            VerifyRefreshNotDeferred();

            // if we have paging
            if (PageSize > 0 && PageIndex > -1)
            {
                if (IsGrouping && !_isUsingTemporaryGroup)
                {
                    return _group.ItemCount;
                }
                return Math.Max(0, Math.Min(PageSize, InternalCount - (_pageSize * PageIndex)));
            }
            if (IsGrouping)
            {
                if (_isUsingTemporaryGroup)
                {
                    return _temporaryGroup.ItemCount;
                }
                return _group.ItemCount;
            }
            return InternalCount;
        }
    }
    
    /// <summary>
    /// Gets or sets Culture to use during sorting.
    /// </summary>
    public CultureInfo Culture
    {
        get => _culture;

        set
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (_culture != value)
            {
                _culture = value;
                NotifyPropertyChanged(nameof(Culture));
            }
        }
    }
    
    /// <summary>
    /// Gets the new item when an AddNew transaction is in progress
    /// Otherwise it returns null.
    /// </summary>
    public object? CurrentAddItem
    {
        get => _newItem;

        private set
        {
            if (_newItem != value)
            {
                Debug.Assert(value == null || _newItem == null,
                    "Old and new _newItem values are unexpectedly non null");
                _newItem = value;
                NotifyPropertyChanged(nameof(IsAddingNew));
                NotifyPropertyChanged(nameof(CurrentAddItem));
            }
        }
    }
    
    /// <summary>
    /// Gets or sets the Filter, which is a callback set by the consumer of the ICollectionView
    /// and used by the implementation of the ICollectionView to determine if an
    /// item is suitable for inclusion in the view.
    /// </summary>        
    /// <exception cref="NotSupportedException">
    /// Simpler implementations do not support filtering and will throw a NotSupportedException.
    /// Use <seealso cref="CanFilter"/> property to test if filtering is supported before
    /// assigning a non-null value.
    /// </exception>
    public Func<object, bool>? Filter
    {
        get => _filter;

        set
        {
            if (IsAddingNew)
            {
                throw new InvalidOperationException(GetOperationNotAllowedDuringAddOrEditText(nameof(Filter)));
            }

            if (!CanFilter)
            {
                throw new NotSupportedException(
                    "The Filter property cannot be set when the CanFilter property returns false.");
            }

            if (_filter != value)
            {
                _filter = value;
                RefreshOrDefer();
                NotifyPropertyChanged(nameof(Filter));
            }
        }
    }
    
    /// <summary>
    /// Gets the description of grouping, indexed by level.
    /// </summary>
    public AvaloniaList<IListGroupDescription> GroupDescriptions => _group.GroupDescriptions;

    int IListCollectionView.GroupingDepth => GroupDescriptions.Count;

    /// <summary>
    /// Gets the top-level groups, constructed according to the descriptions
    /// given in GroupDescriptions.
    /// </summary>
    public IAvaloniaReadOnlyList<object>? Groups
    {
        get
        {
            if (!IsGrouping)
            {
                return null;
            }

            return RootGroup.Items;
        }
    }

    /// <summary>
    /// Gets a value indicating whether an "AddNew" transaction is in progress.
    /// </summary>
    public bool IsAddingNew => _newItem != null;
    
    /// <summary>
    /// Gets a value indicating whether the resulting (filtered) view is empty.
    /// </summary>
    public bool IsEmpty
    {
        get
        {
            EnsureCollectionInSync();
            return InternalCount == 0;
        }
    }

    /// <summary>
    /// Gets a value indicating whether a page index change is in process or not.
    /// </summary>
    //TODO Paging
    public bool IsPageChanging
    {
        get => CheckFlag(CollectionViewFlags.IsPageChanging);

        private set
        {
            if (CheckFlag(CollectionViewFlags.IsPageChanging) != value)
            {
                SetFlag(CollectionViewFlags.IsPageChanging, value);
                NotifyPropertyChanged(nameof(IsPageChanging));
            }
        }
    }

    /// <summary>
    /// Gets the minimum number of items known to be in the source collection
    /// that verify the current filter if any
    /// </summary>
    public int ItemCount => InternalList.Count;
    
    /// <summary>
    /// Gets a value indicating whether this view needs to be refreshed.
    /// </summary>
    public bool NeedsRefresh => CheckFlag(CollectionViewFlags.NeedsRefresh);

    /// <summary>
    /// Gets the current page we are on. (zero based)
    /// </summary>
    //TODO Paging
    public int PageIndex => _pageIndex;
    
    /// <summary>
    /// Gets or sets the number of items to display on a page. If the
    /// PageSize = 0, then we are not paging, and will display all items
    /// in the collection. Otherwise, we will have separate pages for 
    /// the items to display.
    /// </summary>
    //TODO Paging
    public int PageSize
    {
        get => _pageSize;
        set
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "PageSize cannot have a negative value.");
            }

            // if the Refresh is currently deferred, cache the desired PageSize
            // and set the flag so that once the defer is over, we can then
            // update the PageSize.
            if (IsRefreshDeferred)
            {
                // set cached value and flag so that we update the PageSize on EndDefer
                _cachedPageSize = value;
                SetFlag(CollectionViewFlags.IsUpdatePageSizeDeferred, true);
                return;
            }

            // to see whether or not to fire an NotifyPropertyChanged
            int oldCount = Count;

            if (_pageSize != value)
            {
                _pageSize = value;
                NotifyPropertyChanged(nameof(PageSize));

                if (_pageSize == 0)
                {
                    // update the groups for the current page
                    //***************************************
                    PrepareGroups();

                    // if we are not paging
                    MoveToPage(-1);
                }
                else if (_pageIndex != 0)
                {
                    if (!CheckFlag(CollectionViewFlags.IsMoveToPageDeferred))
                    {
                        // if the temporaryGroup was not created yet and is out of sync
                        // then create it so that we can use it as a reference while paging.
                        if (IsGrouping && _temporaryGroup.ItemCount != InternalList.Count)
                        {
                            PrepareTemporaryGroups();
                        }

                        MoveToFirstPage();
                    }
                }
                else if (IsGrouping)
                {
                    // if the temporaryGroup was not created yet and is out of sync
                    // then create it so that we can use it as a reference while paging.
                    if (_temporaryGroup.ItemCount != InternalList.Count)
                    {
                        // update the groups that get created for the
                        // entire collection as well as the current page
                        PrepareTemporaryGroups();
                    }

                    // update the groups for the current page
                    PrepareGroupsForCurrentPage();
                }

                // if the count has changed
                if (Count != oldCount)
                {
                    NotifyPropertyChanged(nameof(Count));
                }

                // send a notification that our collection has been updated
                HandleCollectionChanged(
                    new NotifyCollectionChangedEventArgs(
                        NotifyCollectionChangedAction.Reset));
            }
        }
    }

    /// <summary>
    /// Gets the Sort criteria to sort items in collection.
    /// </summary>
    /// <remarks>
    /// <p>
    /// Clear a sort criteria by assigning SortDescription.Empty to this property.
    /// One or more sort criteria in form of <seealso cref="Data.ListSortDescription"/>
    /// can be used, each specifying a property and direction to sort by.
    /// </p>
    /// </remarks>
    /// <exception cref="NotSupportedException">
    /// Simpler implementations do not support sorting and will throw a NotSupportedException.
    /// Use <seealso cref="CanSort"/> property to test if sorting is supported before adding
    /// to SortDescriptions.
    /// </exception>
    public ListSortDescriptionList SortDescriptions
    {
        get
        {
            if (_sortDescriptions == null)
            {
                SetSortDescriptions(new ListSortDescriptionList());
            }
            Debug.Assert(_sortDescriptions != null);
            return _sortDescriptions;
        }
    }

    public ListFilterDescriptionList FilterDescriptions
    {
        get
        {
            if (_filterDescriptions == null)
            {
                SetFilterDescriptions(new ListFilterDescriptionList());
            }
            Debug.Assert(_filterDescriptions != null);
            return _filterDescriptions;
        }
    }

    /// <summary>
    /// Gets the source of the IEnumerable collection we are using for our view.
    /// </summary>
    public IEnumerable SourceCollection => _sourceCollection;

    /// <summary>
    /// Gets the total number of items in the view before paging is applied.
    /// </summary>
    public int TotalItemCount => InternalList.Count;
    
    #endregion

    #region 公共事件定义

    /// <summary>
    /// Raise this event when the (filtered) view changes
    /// </summary>
    public event NotifyCollectionChangedEventHandler? CollectionChanged;

    /// <summary>
    /// CollectionChanged event (per INotifyCollectionChanged).
    /// </summary>
    event NotifyCollectionChangedEventHandler? INotifyCollectionChanged.CollectionChanged
    {
        add => CollectionChanged    += value;
        remove => CollectionChanged -= value;
    }

    /// <summary>
    /// Raised when a page index change completed
    /// </summary>
    //TODO Paging
    public event EventHandler<EventArgs>? PageChanged;

    /// <summary>
    /// Raised when a page index change is requested
    /// </summary>
    //TODO Paging
    public event EventHandler<PageChangingEventArgs>? PageChanging;

    /// <summary>
    /// PropertyChanged event.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// PropertyChanged event (per INotifyPropertyChanged)
    /// </summary>
    event PropertyChangedEventHandler? INotifyPropertyChanged.PropertyChanged
    {
        add => PropertyChanged    += value;
        remove => PropertyChanged -= value;
    }

    #endregion

    #region 内部属性定义

    private Type? _itemType;

    private Type? ItemType
    {
        get
        {
            if (_itemType == null)
            {
                _itemType = GetItemType(true);
            }
            return _itemType;
        }
    }

    /// <summary>
    /// Gets a value indicating whether we have a valid ItemConstructor of the correct type
    /// </summary>
    private bool CanConstructItem
    {
        get
        {
            if (!_itemConstructorIsValid)
            {
                EnsureItemConstructor();
            }

            return _itemConstructor != null;
        }
    }

    /// <summary>
    /// Gets the private count without taking paging or
    /// placeholders into account
    /// </summary>
    private int InternalCount => InternalList.Count;

    /// <summary>
    /// Gets the InternalList
    /// </summary>
    private IList InternalList => _internalList;

    /// <summary>
    /// Gets a value indicating whether or not we have grouping 
    /// taking place in this collection.
    /// </summary>
    private bool IsGrouping => _isGrouping;

    bool IListCollectionView.IsGrouping => IsGrouping;

    /// <summary>
    /// Gets a value indicating whether there
    /// is still an outstanding DeferRefresh in
    /// use.  If at all possible, derived classes
    /// should not call Refresh if IsRefreshDeferred
    /// is true.
    /// </summary>
    private bool IsRefreshDeferred => _deferLevel > 0;

    /// <summary>
    /// Gets whether the current page is empty and we need
    /// to move to a previous page.
    /// </summary>
    //TODO Paging
    private bool NeedToMoveToPreviousPage => (PageSize > 0 && Count == 0 && PageIndex != 0 && PageCount == PageIndex);

    /// <summary>
    /// Gets a value indicating whether we are on the last local page
    /// </summary>
    //TODO Paging
    private bool OnLastLocalPage
    {
        get
        {
            if (PageSize == 0)
            {
                return false;
            }

            Debug.Assert(PageCount > 0, "Unexpected PageCount <= 0");

            // if we have no items (PageCount==1) or there is just one page
            if (PageCount == 1)
            {
                return true;
            }

            return (PageIndex == PageCount - 1);
        }
    }

    /// <summary>
    /// Gets the number of pages we currently have
    /// </summary>
    //TODO Paging
    private int PageCount => _pageSize > 0 ? Math.Max(1, (int)Math.Ceiling((double)ItemCount / _pageSize)) : 0;

    /// <summary>
    /// Gets the root of the Group that we expose to the user
    /// </summary>
    private ListCollectionViewGroupRoot RootGroup => _isUsingTemporaryGroup ? _temporaryGroup : _group;

    /// <summary>
    /// Gets the SourceCollection as an IList
    /// </summary>
    private IList? SourceList => SourceCollection as IList;

    /// <summary>
    /// Gets Timestamp used by the NewItemAwareEnumerator to determine if a
    /// collection change has occurred since the enumerator began.  (If so,
    /// MoveNext should throw.)
    /// </summary>
    private int Timestamp => _timestamp;

    /// <summary>
    /// Gets a value indicating whether a private copy of the data 
    /// is needed for sorting, filtering, and paging. We want any deriving 
    /// classes to also be able to access this value to see whether or not 
    /// to use the default source collection, or the internal list.
    /// </summary>
    //TODO Paging
    private bool UsesLocalArray => SortDescriptions.Count > 0 || 
                                   Filter != null || 
                                   _pageSize > 0 || 
                                   GroupDescriptions.Count > 0;
    #endregion
    
    /// <summary>
    /// Value that we cache for the PageIndex if we are in a DeferRefresh,
    /// and the user has attempted to move to a different page.
    /// </summary>
    private int _cachedPageIndex = -1;
    
    /// <summary>
    /// Value that we cache for the PageSize if we are in a DeferRefresh,
    /// and the user has attempted to change the PageSize.
    /// </summary>
    private int _cachedPageSize;
    
    /// <summary>
    /// CultureInfo used in this ListCollectionView
    /// </summary>
    private CultureInfo _culture;
    
    /// <summary>
    /// Private accessor for the Monitor we use to prevent recursion
    /// </summary>
    private SimpleMonitor _currentChangedMonitor = new SimpleMonitor();
    
    /// <summary>
    /// The number of requests to defer Refresh()
    /// </summary>
    private int _deferLevel;
    
    /// <summary>
    /// Private accessor for the Filter
    /// </summary>
    private Func<object, bool>? _filter;
    
    /// <summary>
    /// Private accessor for the CollectionViewFlags
    /// </summary>
    private CollectionViewFlags _flags = CollectionViewFlags.ShouldProcessCollectionChanged;
    
    /// <summary>
    /// Private accessor for the Grouping data
    /// </summary>
    private ListCollectionViewGroupRoot _group;
    
    /// <summary>
    /// Private accessor for the InternalList
    /// </summary>
    private IList _internalList;
    
    /// <summary>
    /// Keeps track of whether groups have been applied to the
    /// collection already or not. Note that this can still be set
    /// to false even though we specify a GroupDescription, as the 
    /// collection may not have gone through the PrepareGroups function.
    /// </summary>
    private bool _isGrouping;
    
    /// <summary>
    /// Private accessor for indicating whether we want to point to the temporary grouping data for calculations
    /// </summary>
    private bool _isUsingTemporaryGroup;
    
    /// <summary>
    /// ConstructorInfo obtained from reflection for generating new items
    /// </summary>
    private ConstructorInfo? _itemConstructor;
    
    /// <summary>
    /// Whether we have the correct ConstructorInfo information for the ItemConstructor
    /// </summary>
    private bool _itemConstructorIsValid;
    
    /// <summary>
    /// The new item we are getting ready to add to the collection
    /// </summary>
    private object? _newItem;
    
    /// <summary>
    /// Private accessor for the PageIndex
    /// </summary>
    private int _pageIndex = -1;
    
    /// <summary>
    /// Private accessor for the PageSize
    /// </summary>
    private int _pageSize;
    
    /// <summary>
    /// Whether the source needs to poll for changes
    /// (if it did not implement INotifyCollectionChanged)
    /// </summary>
    private bool _pollForChanges;
    
    /// <summary>
    /// Private accessor for the SortDescriptions
    /// </summary>
    private ListSortDescriptionList? _sortDescriptions;
    
    /// <summary>
    /// Private accessor for the FilterDescriptions
    /// </summary>
    private ListFilterDescriptionList? _filterDescriptions;
    
    /// <summary>
    /// Private accessor for the SourceCollection
    /// </summary>
    private IEnumerable _sourceCollection;

    /// <summary>
    /// Private accessor for the Grouping data on the entire collection
    /// </summary>
    private ListCollectionViewGroupRoot _temporaryGroup;

    /// <summary>
    /// Timestamp used to see if there was a collection change while 
    /// processing enumerator changes
    /// </summary>
    private int _timestamp;

    /// <summary>
    /// Private accessor for the TrackingEnumerator
    /// </summary>
    private IEnumerator _trackingEnumerator;
    
    /// <summary>
    /// Helper constructor that sets default values for isDataSorted and isDataInGroupOrder.
    /// </summary>
    /// <param name="source">The source for the collection</param>
    public ListCollectionView(IEnumerable source)
        : this(source, false /*isDataSorted*/, false /*isDataInGroupOrder*/)
    {
    }
    
    /// <summary>
    /// Initializes a new instance of the ListCollectionView class.
    /// </summary>
    /// <param name="source">The source for the collection</param>
    /// <param name="isDataSorted">Determines whether the source is already sorted</param>
    /// <param name="isDataInGroupOrder">Whether the source is already in the correct order for grouping</param>
    public ListCollectionView(IEnumerable source, bool isDataSorted, bool isDataInGroupOrder)
    {
        _sourceCollection = source ?? throw new ArgumentNullException(nameof(source));

        SetFlag(CollectionViewFlags.IsDataSorted, isDataSorted);
        SetFlag(CollectionViewFlags.IsDataInGroupOrder, isDataInGroupOrder);

        _temporaryGroup                            =  new ListCollectionViewGroupRoot(this, isDataInGroupOrder);
        _group                                     =  new ListCollectionViewGroupRoot(this, false);
        _group.GroupDescriptionChanged             += HandleGroupDescriptionChanged;
        _group.GroupDescriptions.CollectionChanged += HandleGroupByChanged;

        CopySourceToInternalList();
        _trackingEnumerator = source.GetEnumerator();

        Debug.Assert(_internalList != null);
        
        // Set flag for whether the collection is empty
        SetFlag(CollectionViewFlags.CachedIsEmpty, Count == 0);

        // If we implement INotifyCollectionChanged
        if (source is INotifyCollectionChanged coll)
        {
            coll.CollectionChanged += (_, args) => ProcessCollectionChanged(args);
        }
        else
        {
            // If the source doesn't raise collection change events, try to
            // detect changes by polling the enumerator
            _pollForChanges = true;
        }

        _culture = CultureInfo.InvariantCulture;
    }
    
    private string GetOperationNotAllowedDuringAddOrEditText(string action)
    {
        return $"'{action}' is not allowed during an AddNew or EditItem transaction.";
    }

    private string GetOperationNotAllowedText(string action, string? transaction = null)
    {
        if (string.IsNullOrWhiteSpace(transaction))
        {
            return $"'{action}' is not allowed for this view.";
        }
        return $"'{action}' is not allowed during a transaction started by '{transaction}'.";
    }
    
     /// <summary>
    /// Return the item at the specified index
    /// </summary>
    /// <param name="index">Index of the item we want to retrieve</param>
    /// <returns>The item at the specified index</returns>
    public object? this[int index] => GetItemAt(index);

    bool IList.IsFixedSize => SourceList?.IsFixedSize ?? true;
    bool IList.IsReadOnly => SourceList?.IsReadOnly ?? true;
    bool ICollection.IsSynchronized => false;
    object ICollection.SyncRoot => this;

    object? IList.this[int index]
    {
        get => this[index];
        set
        {
            Debug.Assert(SourceList != null);
            SourceList[index] = value;
            if (SourceList is not INotifyCollectionChanged)
            {
                // TODO: implement Replace
                ProcessCollectionChanged(
                    new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset, value));
            }
        }
    }

    /// <summary>
    /// Add a new item to the underlying collection.  Returns the new item.
    /// After calling AddNew and changing the new item as desired, either
    /// CommitNew or CancelNew" should be called to complete the transaction.
    /// </summary>
    /// <returns>The new item we are adding</returns>

    public object AddNew()
    {
        EnsureCollectionInSync();
        VerifyRefreshNotDeferred();
        
        // Implicitly close a previous AddNew
        CommitNew();

        // Checking CanAddNew will validate that we have the correct itemConstructor
        if (!CanAddNew)
        {
            throw new InvalidOperationException(GetOperationNotAllowedText(nameof(AddNew)));
        }

        object? newItem = null;

        if (_itemConstructor != null)
        {
            newItem = _itemConstructor.Invoke(null);
        }
        
        Debug.Assert(newItem != null);

        try
        {
            // temporarily disable the CollectionChanged event
            // handler so filtering, sorting, or grouping
            // doesn't get applied yet
            SetFlag(CollectionViewFlags.ShouldProcessCollectionChanged, false);

            if (SourceList != null)
            {
                SourceList.Add(newItem);
            }
        }
        finally
        {
            SetFlag(CollectionViewFlags.ShouldProcessCollectionChanged, true);
        }

        // Modify our _trackingEnumerator so that it shows that our collection is "up to date" 
        // and will not refresh for now.
        _trackingEnumerator = _sourceCollection.GetEnumerator();

        int addIndex;
        int removeIndex = -1;

        // Adjust index based on where it should be displayed in view.
        if (PageSize > 0)
        {
            // if the page is full (Count==PageSize), then replace last item (Count-1).
            // otherwise, we just append at end (Count).
            addIndex = Count - ((Count == PageSize) ? 1 : 0);

            // if the page is full, remove the last item to make space for the new one.
            removeIndex = (Count == PageSize) ? addIndex : -1;
        }
        else
        {
            // for non-paged lists, we want to insert the item 
            // as the last item in the view
            addIndex = Count;
        }

        // if we need to remove an item from the view due to paging
        if (removeIndex > -1)
        {
            object? removeItem = GetItemAt(removeIndex);
            if (IsGrouping && removeItem != null)
            {
                _group.RemoveFromSubgroups(removeItem);
            }

            HandleCollectionChanged(
                new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Remove,
                    removeItem,
                    removeIndex));
        }

        // add the new item to the internal list
        _internalList.Insert(ConvertToInternalIndex(addIndex), newItem);
        NotifyPropertyChanged(nameof(ItemCount));

        if (IsGrouping)
        {
            _group.InsertSpecialItem(_group.Items.Count, newItem, false);
            if (PageSize > 0)
            {
                _temporaryGroup.InsertSpecialItem(_temporaryGroup.Items.Count, newItem, false);
            }
        }

        // fire collection changed.
        HandleCollectionChanged(
            new NotifyCollectionChangedEventArgs(
                NotifyCollectionChangedAction.Add,
                newItem,
                addIndex));

        // set the current new item
        CurrentAddItem = newItem;

        // if the new item is editable, call BeginEdit on it
        if (newItem is IEditableObject editableObject)
        {
            editableObject.BeginEdit();
        }

        return newItem;
    }
    
    /// <summary>
    /// Complete the transaction started by AddNew. The new
    /// item is removed from the collection.
    /// </summary>
    //TODO Paging
    public void CancelNew()
    {
        VerifyRefreshNotDeferred();

        if (CurrentAddItem == null)
        {
            return;
        }

        // get index of item before it is removed
        int index = IndexOf(CurrentAddItem);

        // remove the new item from the underlying collection
        try
        {
            // temporarily disable the CollectionChanged event
            // handler so filtering, sorting, or grouping
            // doesn't get applied yet
            SetFlag(CollectionViewFlags.ShouldProcessCollectionChanged, false);

            if (SourceList != null)
            {
                SourceList.Remove(CurrentAddItem);
            }
        }
        finally
        {
            SetFlag(CollectionViewFlags.ShouldProcessCollectionChanged, true);
        }

        // Modify our _trackingEnumerator so that it shows that our collection is "up to date" 
        // and will not refresh for now.
        _trackingEnumerator = _sourceCollection.GetEnumerator();

        // fire the correct events
        if (CurrentAddItem != null)
        {
            object? newItem = EndAddNew(true);
            Debug.Assert(newItem != null);
            int addIndex = -1;

            // Adjust index based on where it should be displayed in view.
            if (PageSize > 0 && !OnLastLocalPage)
            {
                // if there is paging and we are not on the last page, we need
                // to bring in an item from the next page.
                addIndex = Count - 1;
            }

            // remove the new item from the internal list 
            InternalList.Remove(newItem);

            if (IsGrouping)
            {
                _group.RemoveSpecialItem(_group.Items.Count - 1, newItem, false);
                if (PageSize > 0)
                {
                    _temporaryGroup.RemoveSpecialItem(_temporaryGroup.Items.Count - 1, newItem, false);
                }
            }

            NotifyPropertyChanged(nameof(ItemCount));

            // fire collection changed.
            HandleCollectionChanged(
                new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Remove,
                    newItem,
                    index));
            
            // if we need to add an item into the view due to paging
            if (addIndex > -1)
            {
                int    internalIndex = ConvertToInternalIndex(addIndex);
                object? addItem       = null;
                if (IsGrouping)
                {
                    addItem = _temporaryGroup.LeafAt(internalIndex);
                    if (addItem != null)
                    {
                        _group.AddToSubgroups(addItem, loading: false);
                    }
                }
                else
                {
                    addItem = InternalItemAt(internalIndex);
                }

                if (addItem != null)
                {
                    HandleCollectionChanged(
                        new NotifyCollectionChangedEventArgs(
                            NotifyCollectionChangedAction.Add,
                            addItem,
                            IndexOf(addItem)));
                }
            }
        }
    }
    
    /// <summary>
    /// Common functionality used by CommitNew, CancelNew, and when the
    /// new item is removed by Remove or Refresh.
    /// </summary>
    /// <param name="cancel">Whether we canceled the add</param>
    /// <returns>The new item we ended adding</returns>
    private object? EndAddNew(bool cancel)
    {
        object? newItem = CurrentAddItem;

        CurrentAddItem = null; // leave "adding-new" mode

        if (newItem is IEditableObject ieo)
        {
            if (cancel)
            {
                ieo.CancelEdit();
            }
            else
            {
                ieo.EndEdit();
            }
        }

        return newItem;
    }
    
    /// <summary>
    /// Complete the transaction started by AddNew. We follow the WPF
    /// convention in that the view's sort, filter, and paging
    /// specifications (if any) are applied to the new item.
    /// </summary>
    //TODO Paging
    public void CommitNew()
    {
        VerifyRefreshNotDeferred();

        if (CurrentAddItem == null)
        {
            return;
        }

        // End the AddNew transaction
        object? newItem = EndAddNew(false);
        Debug.Assert(newItem != null);
        // keep track of the current item

        // Modify our _trackingEnumerator so that it shows that our collection is "up to date" 
        // and will not refresh for now.
        _trackingEnumerator = _sourceCollection.GetEnumerator();

        if (UsesLocalArray)
        {
            // first remove the item from the array so that we can insert into the correct position
            int removeIndex   = Count - 1;
            int internalIndex = _internalList.IndexOf(newItem);
            _internalList.Remove(newItem);

            if (IsGrouping)
            {
                _group.RemoveSpecialItem(_group.Items.Count - 1, newItem, false);
                if (PageSize > 0)
                {
                    _temporaryGroup.RemoveSpecialItem(_temporaryGroup.Items.Count - 1, newItem, false);
                }
            }
            
            // raise the remove event so we can next insert it into the correct place
            HandleCollectionChanged(
                new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Remove,
                    newItem,
                    removeIndex));

            // check to see that the item will be added back in
            bool passedFilter = PassesFilter(newItem);

            // next process adding it into the correct location
            ProcessInsertToCollection(newItem, internalIndex);

            int pageStartIndex     = PageIndex * PageSize;
            int nextPageStartIndex = pageStartIndex + PageSize;

            if (IsGrouping)
            {
                int leafIndex = -1;
                if (passedFilter && PageSize > 0)
                {
                    _temporaryGroup.AddToSubgroups(newItem, false /*loading*/);
                    leafIndex = _temporaryGroup.LeafIndexOf(newItem);
                }

                // if we are not paging, we should just be able to add the item.
                // otherwise, we need to validate that it is within the current page.
                if (passedFilter && (PageSize == 0 ||
                                     (pageStartIndex <= leafIndex && nextPageStartIndex > leafIndex)))
                {
                    _group.AddToSubgroups(newItem, false /*loading*/);
                    int addIndex = IndexOf(newItem);

                    HandleCollectionChanged(
                        new NotifyCollectionChangedEventArgs(
                            NotifyCollectionChangedAction.Add,
                            newItem,
                            addIndex));
                }
                else
                {
                    if (PageSize > 0)
                    {
                        int addIndex = -1;
                        if (passedFilter && leafIndex < pageStartIndex)
                        {
                            // if the item was added to an earlier page, then we need to bring
                            // in the item that would have been pushed down to this page
                            addIndex = pageStartIndex;
                        }
                        else if (!OnLastLocalPage)
                        {
                            // if the item was added to a later page, then we need to bring in the
                            // first item from the next page
                            addIndex = nextPageStartIndex - 1;
                        }

                        object? addItem = _temporaryGroup.LeafAt(addIndex);
                        if (addItem != null)
                        {
                            _group.AddToSubgroups(addItem, false /*loading*/);
                            addIndex = IndexOf(addItem);
                            
                            HandleCollectionChanged(
                                new NotifyCollectionChangedEventArgs(
                                    NotifyCollectionChangedAction.Add,
                                    addItem,
                                    addIndex));
                        }
                    }
                }
            }
            else
            {
                // if we are still within the view
                int addIndex = IndexOf(newItem);
                if (addIndex >= 0)
                {
                    HandleCollectionChanged(
                        new NotifyCollectionChangedEventArgs(
                            NotifyCollectionChangedAction.Add,
                            newItem,
                            addIndex));
                }
                else
                {
                    if (PageSize > 0)
                    {
                        bool insertedToPreviousPage = InternalIndexOf(newItem) < ConvertToInternalIndex(0);
                        addIndex = insertedToPreviousPage ? 0 : Count - 1;

                        // don't fire the event if we are on the last page
                        // and we don't have any items to bring in.
                        if (insertedToPreviousPage || !OnLastLocalPage)
                        {
                            HandleCollectionChanged(
                                new NotifyCollectionChangedEventArgs(
                                    NotifyCollectionChangedAction.Add,
                                    GetItemAt(addIndex),
                                    addIndex));
                        }
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// Return true if the item belongs to this view.  No assumptions are
    /// made about the item. This method will behave similarly to IList.Contains().
    /// If the caller knows that the item belongs to the
    /// underlying collection, it is more efficient to call PassesFilter.
    /// </summary>
    /// <param name="item">The item we are checking to see whether it is within the collection</param>
    /// <returns>Boolean value of whether or not the collection contains the item</returns>
    public bool Contains(object? item)
    {
        EnsureCollectionInSync();
        VerifyRefreshNotDeferred();
        return IndexOf(item) >= 0;
    }

    /// <summary>
    /// Enter a Defer Cycle.
    /// Defer cycles are used to coalesce changes to the ICollectionView.
    /// </summary>
    /// <returns>IDisposable used to notify that we no longer need to defer, when we dispose</returns>
    public IDisposable DeferRefresh()
    {
        if (IsAddingNew)
        {
            throw new InvalidOperationException(GetOperationNotAllowedDuringAddOrEditText(nameof(DeferRefresh)));
        }

        ++_deferLevel;
        return new DeferHelper(this);
    }
    
    /// <summary> 
    /// Implementation of IEnumerable.GetEnumerator().
    /// This provides a way to enumerate the members of the collection
    /// without changing the currency.
    /// </summary>
    /// <returns>IEnumerator for the collection</returns>
    //TODO Paging
    public IEnumerator GetEnumerator()
    {
        EnsureCollectionInSync();
        VerifyRefreshNotDeferred();

        if (IsGrouping)
        {
            return RootGroup.GetLeafEnumerator();
        }

        // if we are paging
        if (PageSize > 0)
        {
            var list = new List<object?>();

            // if we are in the middle of asynchronous load
            if (PageIndex < 0)
            {
                return list.GetEnumerator();
            }

            for (int index = _pageSize * PageIndex;
                 index < (int)Math.Min(_pageSize * (PageIndex + 1), InternalList.Count);
                 index++)
            {
                list.Add(InternalList[index]);
            }

            return new NewItemAwareEnumerator(this, list.GetEnumerator(), CurrentAddItem);
        }
        return new NewItemAwareEnumerator(this, InternalList.GetEnumerator(), CurrentAddItem);
    }

    /// <summary>
    /// Interface Implementation for GetEnumerator()
    /// </summary>
    /// <returns>IEnumerator that we get from our internal collection</returns>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>
    /// Retrieve item at the given zero-based index in this ListCollectionView, after the source collection
    /// is filtered, sorted, and paged.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown if index is out of range
    /// </exception>
    /// <param name="index">Index of the item we want to retrieve</param>
    /// <returns>Item at specified index</returns>
    public object? GetItemAt(int index)
    {
        EnsureCollectionInSync();
        VerifyRefreshNotDeferred();

        // for indices larger than the count
        if (index >= Count || index < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        if (IsGrouping)
        {
            return RootGroup.LeafAt(_isUsingTemporaryGroup ? ConvertToInternalIndex(index) : index);
        }

        if (IsAddingNew && UsesLocalArray && index == Count - 1)
        {
            return CurrentAddItem;
        }

        return InternalItemAt(ConvertToInternalIndex(index));
    }

    /// <summary> 
    /// Return the index where the given item appears, or -1 if doesn't appear.
    /// </summary>
    /// <param name="item">Item we are searching for</param>
    /// <returns>Index of specified item</returns>
    //TODO Paging
    public int IndexOf(object? item)
    {
        EnsureCollectionInSync();
        VerifyRefreshNotDeferred();

        if (IsGrouping)
        {
            return RootGroup?.LeafIndexOf(item) ?? -1;
        }

        if (IsAddingNew && Equals(item, CurrentAddItem) && UsesLocalArray)
        {
            return Count - 1;
        }

        int internalIndex = InternalIndexOf(item);

        if (PageSize > 0 && internalIndex != -1)
        {
            if ((internalIndex >= (PageIndex * _pageSize)) &&
                (internalIndex < ((PageIndex + 1) * _pageSize)))
            {
                return internalIndex - (PageIndex * _pageSize);
            }
            return -1;
        }
        return internalIndex;
    }
    
    /// <summary>
    /// Moves to the first page.
    /// </summary>
    /// <returns>Whether or not the move was successful.</returns>
    //TODO Paging
    public bool MoveToFirstPage()
    {
        return MoveToPage(0);
    }
    
    /// <summary>
    /// Moves to the last page.
    /// The move is only attempted when TotalItemCount is known.
    /// </summary>
    /// <returns>Whether or not the move was successful.</returns>
    //TODO Paging
    public bool MoveToLastPage()
    {
        if (TotalItemCount != -1 && PageSize > 0)
        {
            return MoveToPage(PageCount - 1);
        }
        return false;
    }
    
    /// <summary>
    /// Moves to the page after the current page we are on.
    /// </summary>
    /// <returns>Whether or not the move was successful.</returns>
    //TODO Paging
    public bool MoveToNextPage()
    {
        return MoveToPage(_pageIndex + 1);
    }
    
     /// <summary>
    /// Requests a page move to page <paramref name="pageIndex"/>.
    /// </summary>
    /// <param name="pageIndex">Index of the target page</param>
    /// <returns>Whether or not the move was successfully initiated.</returns>
    //TODO Paging
    public bool MoveToPage(int pageIndex)
    {
        // Boundary checks for negative pageIndex
        if (pageIndex < -1)
        {
            return false;
        }

        // if the Refresh is deferred, cache the requested PageIndex so that we
        // can move to the desired page when EndDefer is called.
        if (IsRefreshDeferred)
        {
            // set cached value and flag so that we move to the page on EndDefer
            _cachedPageIndex = pageIndex;
            SetFlag(CollectionViewFlags.IsMoveToPageDeferred, true);
            return false;
        }

        // check for invalid pageIndex
        if (pageIndex == -1 && PageSize > 0)
        {
            return false;
        }

        // Check if the target page is out of bound, or equal to the current page
        if (pageIndex >= PageCount || _pageIndex == pageIndex)
        {
            return false;
        }
        

        if (RaisePageChanging(pageIndex) && pageIndex != -1)
        {
            // Page move was cancelled. Abort the move, but only if the target index isn't -1.
            return false;
        }

        // Check if there is a current edited or new item so changes can be committed first.
        if (CurrentAddItem != null)
        {
            // Since PageChanging was raised and not cancelled, a PageChanged notification needs to be raised
            // even though the PageIndex actually did not change.
            RaisePageChanged();
            return false;
        }

        IsPageChanging = true;
        CompletePageMove(pageIndex);

        return true;
    }

    /// <summary>
    /// Moves to the page before the current page we are on.
    /// </summary>
    /// <returns>Whether or not the move was successful.</returns>
    //TODO Paging
    public bool MoveToPreviousPage()
    {
        return MoveToPage(_pageIndex - 1);
    }

    /// <summary>
    /// Return true if the item belongs to this view.  The item is assumed to belong to the
    /// underlying DataCollection;  this method merely takes filters into account.
    /// It is commonly used during collection-changed notifications to determine if the added/removed
    /// item requires processing.
    /// Returns true if no filter is set on collection view.
    /// </summary>
    /// <param name="item">The item to compare against the Filter</param>
    /// <returns>Whether the item passes the filter</returns>
    public bool PassesFilter(object item)
    {
        if (Filter != null)
        {
            return Filter(item);
        }

        return true;
    }

    /// <summary>
    /// Re-create the view, using any SortDescriptions and/or Filters.
    /// </summary>
    public void Refresh()
    {
        RefreshInternal();
    }

    /// <summary>
    /// Remove the given item from the underlying collection. It
    /// needs to be in the current filtered, sorted, and paged view
    /// to call 
    /// </summary>
    /// <param name="item">Item we want to remove</param>
    public void Remove(object? item)
    {
        int index = IndexOf(item);
        if (index >= 0)
        {
            RemoveAt(index);
        }
    }

    /// <summary>
    /// Remove the item at the given index from the underlying collection.
    /// The index is interpreted with respect to the view (filtered, sorted,
    /// and paged list).
    /// </summary>
    /// <param name="index">Index of the item we want to remove</param>
    //TODO Paging
    public void RemoveAt(int index)
    {
        if (index < 0 || index >= Count)
        {
            throw new ArgumentOutOfRangeException(nameof(index),
                "Index was out of range. Must be non-negative and less than the size of the collection.");
        }

        if (IsAddingNew)
        {
            throw new InvalidOperationException(GetOperationNotAllowedDuringAddOrEditText(nameof(RemoveAt)));
        }
        if (!CanRemove)
        {
            throw new InvalidOperationException("Remove/RemoveAt is not supported.");
        }

        VerifyRefreshNotDeferred();

        // convert the index from "view-relative" to "list-relative"
        object? item = GetItemAt(index);

        // before we remove the item, see if we are not on the last page
        // and will have to bring in a new item to replace it
        bool replaceItem = PageSize > 0 && !OnLastLocalPage;

        try
        {
            // temporarily disable the CollectionChanged event
            // handler so filtering, sorting, or grouping
            // doesn't get applied yet
            SetFlag(CollectionViewFlags.ShouldProcessCollectionChanged, false);

            if (SourceList != null)
            {
                SourceList.Remove(item);
            }
        }
        finally
        {
            SetFlag(CollectionViewFlags.ShouldProcessCollectionChanged, true);
        }

        // Modify our _trackingEnumerator so that it shows that our collection is "up to date" 
        // and will not refresh for now.
        _trackingEnumerator = _sourceCollection.GetEnumerator();

        Debug.Assert(index == IndexOf(item), "IndexOf returned unexpected value");

        // remove the item from the internal list
        _internalList.Remove(item);

        if (IsGrouping && item != null)
        {
            if (PageSize > 0)
            {
                _temporaryGroup.RemoveFromSubgroups(item);
            }

            _group.RemoveFromSubgroups(item);
        }
        
        // fire remove notification
        HandleCollectionChanged(
            new NotifyCollectionChangedEventArgs(
                NotifyCollectionChangedAction.Remove,
                item,
                index));

        // if we removed all items from the current page,
        // move to the previous page. we do not need to 
        // fire additional notifications, as moving the page will
        // trigger a reset.
        if (NeedToMoveToPreviousPage)
        {
            MoveToPreviousPage();
            return;
        }

        // if we are paging, we may have to fire another notification for the item
        // that needs to replace the one we removed on this page.
        if (replaceItem)
        {
            // we first need to add the item into the current group
            if (IsGrouping)
            {
                object? newItem = _temporaryGroup.LeafAt((PageSize * (PageIndex + 1)) - 1);
                if (newItem != null)
                {
                    _group.AddToSubgroups(newItem, loading: false);
                }
            }

            // fire the add notification
            HandleCollectionChanged(
                new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Add,
                    GetItemAt(PageSize - 1),
                    PageSize - 1));
        }
    }

    /// <summary>
    /// Helper for SortList to handle nested properties (e.g. Address.Street)
    /// </summary>
    /// <param name="item">parent object</param>
    /// <param name="propertyPath">property names path</param>
    /// <param name="propertyType">property type that we want to check for</param>
    /// <returns>child object</returns>
    private static object? InvokePath(object item, string propertyPath, Type propertyType)
    {
        object? propertyValue =
            TypeHelper.GetNestedPropertyValue(item, propertyPath, propertyType, out var exception);
        if (exception != null)
        {
            throw exception;
        }

        return propertyValue;
    }
    
     /// <summary>
    /// Returns true if specified flag in flags is set.
    /// </summary>
    /// <param name="flags">Flag we are checking for</param>
    /// <returns>Whether the specified flag is set</returns>
    private bool CheckFlag(CollectionViewFlags flags)
    {
        return _flags.HasAllFlags(flags);
    }

    /// <summary>
    /// Called to complete the page move operation to set the
    /// current page index.
    /// </summary>
    /// <param name="pageIndex">Final page index</param>
    //TODO Paging
    private void CompletePageMove(int pageIndex)
    {
        if (_pageIndex == pageIndex)
        {
            return;
        }

        // to see whether or not to fire an NotifyPropertyChanged
        int     oldCount                = Count;

        _pageIndex = pageIndex;

        // update the groups
        if (IsGrouping && PageSize > 0)
        {
            PrepareGroupsForCurrentPage();
        }

        IsPageChanging = false;
        NotifyPropertyChanged(nameof(PageIndex));
        RaisePageChanged();

        // if the count has changed
        if (Count != oldCount)
        {
            NotifyPropertyChanged(nameof(Count));
        }

        HandleCollectionChanged(
            new NotifyCollectionChangedEventArgs(
                NotifyCollectionChangedAction.Reset));
    }

    /// <summary>
    /// Convert a value for the index passed in to the index it would be
    /// relative to the InternalIndex property.
    /// </summary>
    /// <param name="index">Index to convert</param>
    /// <returns>Value for the InternalIndex</returns>
    //TODO Paging
    private int ConvertToInternalIndex(int index)
    {
        Debug.Assert(index > -1, "Unexpected index == -1");
        if (PageSize > 0)
        {
            return (_pageSize * PageIndex) + index;
        }
        return index;
    }

    /// <summary>
    /// Copy all items from the source collection to the internal list for processing.
    /// </summary>
    private void CopySourceToInternalList()
    {
        _internalList = new List<object>();
        var enumerator = SourceCollection.GetEnumerator();
        try
        {
            while (enumerator.MoveNext())
            {
                _internalList.Add(enumerator.Current);
            }
        }
        finally
        {
            if (enumerator is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
    
    /// <summary>
    /// Subtracts from the deferLevel counter and calls Refresh() if there are no other defers
    /// </summary>
    private void EndDefer()
    {
        --_deferLevel;

        if (_deferLevel == 0)
        {
            if (CheckFlag(CollectionViewFlags.IsUpdatePageSizeDeferred))
            {
                SetFlag(CollectionViewFlags.IsUpdatePageSizeDeferred, false);
                PageSize = _cachedPageSize;
            }

            if (CheckFlag(CollectionViewFlags.IsMoveToPageDeferred))
            {
                SetFlag(CollectionViewFlags.IsMoveToPageDeferred, false);
                MoveToPage(_cachedPageIndex);
                _cachedPageIndex = -1;
            }

            if (CheckFlag(CollectionViewFlags.NeedsRefresh))
            {
                Refresh();
            }
        }
    }

    /// <summary>
    /// Makes sure that the ItemConstructor is set for the correct type
    /// </summary>
    private void EnsureItemConstructor()
    {
        if (!_itemConstructorIsValid)
        {
            Type? itemType = ItemType;
            if (itemType != null)
            {
                _itemConstructor        = itemType.GetConstructor(Type.EmptyTypes);
                _itemConstructorIsValid = true;
            }
        }
    }

    /// <summary>
    ///  If the IEnumerable has changed, bring the collection up to date.
    ///  (This isn't necessary if the IEnumerable is also INotifyCollectionChanged
    ///  because we keep the collection in sync incrementally.)
    /// </summary>
    private void EnsureCollectionInSync()
    {
        // if the IEnumerable is not a INotifyCollectionChanged
        if (_pollForChanges)
        {
            try
            {
                _trackingEnumerator.MoveNext();
            }
            catch (InvalidOperationException)
            {
                // When the collection has been modified, calling MoveNext()
                // on the enumerator throws an InvalidOperationException, stating
                // that the collection has been modified. Therefore, we know when
                // to update our internal collection.
                _trackingEnumerator = SourceCollection.GetEnumerator();
                RefreshOrDefer();
            }
        }
    }

    /// <summary>
    /// Helper function used to determine the type of an item
    /// </summary>
    /// <param name="useRepresentativeItem">Whether we should use a representative item</param>
    /// <returns>The type of the items in the collection</returns>
    private Type? GetItemType(bool useRepresentativeItem)
    {
        Type   collectionType = SourceCollection.GetType();
        Type[] interfaces     = collectionType.GetInterfaces();

        // Look for IEnumerable<T>.  All generic collections should implement
        //   We loop through the interface list, rather than call
        // GetInterface(IEnumerableT), so that we handle an ambiguous match
        // (by using the first match) without an exception.
        for (int i = 0; i < interfaces.Length; ++i)
        {
            Type interfaceType = interfaces[i];
            if (interfaceType.Name == typeof(IEnumerable<>).Name)
            {
                // found IEnumerable<>, extract T
                Type[] typeParameters = interfaceType.GetGenericArguments();
                if (typeParameters.Length == 1)
                {
                    return typeParameters[0];
                }
            }
        }

        // No generic information found.  Use a representative item instead.
        if (useRepresentativeItem)
        {
            // get type of a representative item
            object? item = GetRepresentativeItem();
            if (item != null)
            {
                return item.GetType();
            }
        }

        return null;
    }

    /// <summary>
    /// Gets a representative item from the collection
    /// </summary>
    /// <returns>An item that can represent the collection</returns>
    private object? GetRepresentativeItem()
    {
        if (IsEmpty)
        {
            return null;
        }

        var enumerator = GetEnumerator();
        try
        {
            while (enumerator.MoveNext())
            {
                object item = enumerator.Current;
                // Since this collection view does not support a NewItemPlaceholder, 
                // simply return the first non-null item.
                if (item != null)
                {
                    return item;
                }
            }
        }
        finally
        {
            if (enumerator is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
        

        return null;
    }

    /// <summary>
    /// Return index of item in the internal list.
    /// </summary>
    /// <param name="item">The item we are checking</param>
    /// <returns>Integer value on where in the InternalList the object is located</returns>
    private int InternalIndexOf(object? item)
    {
        return InternalList.IndexOf(item);
    }

    /// <summary>
    /// Return item at the given index in the internal list.
    /// </summary>
    /// <param name="index">The index we are checking</param>
    /// <returns>The item at the specified index</returns>
    private object? InternalItemAt(int index)
    {
        if (index >= 0 && index < InternalList.Count)
        {
            return InternalList[index];
        }
        return null;
    }

    /// <summary>
    ///     Notify listeners that this View has changed
    /// </summary>
    /// <remarks>
    ///     CollectionViews (and sub-classes) should take their filter/sort/grouping/paging
    ///     into account before calling this method to forward CollectionChanged events.
    /// </remarks>
    /// <param name="args">
    ///     The NotifyCollectionChangedEventArgs to be passed to the EventHandler
    /// </param>
    //TODO Paging
    private void HandleCollectionChanged(NotifyCollectionChangedEventArgs args)
    {
        if (args == null)
        {
            throw new ArgumentNullException(nameof(args));
        }

        unchecked
        {
            // invalidate enumerators because of a change
            ++_timestamp;
        }

        if (CollectionChanged != null)
        {
            if (args.Action != NotifyCollectionChangedAction.Add || PageSize == 0 || args.NewStartingIndex < Count)
            {
                CollectionChanged(this, args);
            }
        }

        // Collection changes change the count unless an item is being
        // replaced within the collection.
        if (args.Action != NotifyCollectionChangedAction.Replace)
        {
            NotifyPropertyChanged(nameof(Count));
        }

        bool listIsEmpty = IsEmpty;
        if (listIsEmpty != CheckFlag(CollectionViewFlags.CachedIsEmpty))
        {
            SetFlag(CollectionViewFlags.CachedIsEmpty, listIsEmpty);
            NotifyPropertyChanged(nameof(IsEmpty));
        }
    }
    
     /// <summary>
    /// GroupBy changed handler
    /// </summary>
    /// <param name="sender">CollectionViewGroup whose GroupBy has changed</param>
    /// <param name="e">Arguments for the NotifyCollectionChanged event</param>
    private void HandleGroupByChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (IsAddingNew)
        {
            throw new InvalidOperationException(GetOperationNotAllowedDuringAddOrEditText("Grouping"));
        }

        RefreshOrDefer();
    }

    /// <summary>
    /// GroupDescription changed handler
    /// </summary>
    /// <param name="sender">CollectionViewGroup whose GroupDescription has changed</param>
    /// <param name="e">Arguments for the GroupDescriptionChanged event</param>
    //TODO Paging
    private void HandleGroupDescriptionChanged(object? sender, EventArgs e)
    {
        if (IsAddingNew)
        {
            throw new InvalidOperationException(GetOperationNotAllowedDuringAddOrEditText("Grouping"));
        }

        // we want to make sure that the data is refreshed before we try to move to a page
        // since the refresh would take care of the filtering, sorting, and grouping.
        RefreshOrDefer();

        if (PageSize > 0)
        {
            if (IsRefreshDeferred)
            {
                // set cached value and flag so that we move to first page on EndDefer
                _cachedPageIndex = 0;
                SetFlag(CollectionViewFlags.IsMoveToPageDeferred, true);
            }
            else
            {
                MoveToFirstPage();
            }
        }
    }

    /// <summary>
    /// Raises a PropertyChanged event.
    /// </summary>
    /// <param name="e">PropertyChangedEventArgs for this change</param>
    private void NotifyPropertyChanged(PropertyChangedEventArgs e)
    {
        PropertyChanged?.Invoke(this, e);
    }

    /// <summary>
    /// Helper to raise a PropertyChanged event.
    /// </summary>
    /// <param name="propertyName">Property name for the property that changed</param>
    private void NotifyPropertyChanged(string propertyName)
    {
        NotifyPropertyChanged(new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// Sets up the ActiveComparer for the ListCollectionViewGroupRoot specified
    /// </summary>
    /// <param name="groupRoot">The ListCollectionViewGroupRoot</param>
    private void PrepareGroupingComparer(ListCollectionViewGroupRoot groupRoot)
    {
        if (groupRoot == _temporaryGroup || PageSize == 0)
        {
            if (groupRoot.ActiveComparer is ListCollectionViewGroupInternal.ListComparer listComparer)
            {
                listComparer.ResetList(InternalList);
            }
            else
            {
                groupRoot.ActiveComparer = new ListCollectionViewGroupInternal.ListComparer(InternalList);
            }
        }
        else if (groupRoot == _group)
        {
            // create the new comparer based on the current _temporaryGroup
            groupRoot.ActiveComparer =
                new ListCollectionViewGroupInternal.CollectionViewGroupComparer(_temporaryGroup);
        }
    }

    /// <summary>
    /// Use the GroupDescriptions to place items into their respective groups.
    /// This assumes that there is no paging, so we just group the entire collection
    /// of items that the CollectionView holds.
    /// </summary>
    private void PrepareGroups()
    {
        // we should only use this method if we aren't paging
        Debug.Assert(PageSize == 0, "Unexpected PageSize != 0");
        
        _group.Clear();
        _group.Initialize();

        _group.IsDataInGroupOrder = CheckFlag(CollectionViewFlags.IsDataInGroupOrder);

        // set to false so that we access internal collection items
        // instead of the group items, as they have been cleared
        _isGrouping = false;

        if (_group.GroupDescriptions.Count > 0)
        {
            for (int num = 0, count = _internalList.Count; num < count; ++num)
            {
                object? item = _internalList[num];
                if (item != null && (!IsAddingNew || !object.Equals(CurrentAddItem, item)))
                {
                    _group.AddToSubgroups(item, loading: true);
                }
            }

            if (IsAddingNew && CurrentAddItem != null)
            {
                _group.InsertSpecialItem(_group.Items.Count, CurrentAddItem, true);
            }
        }

        _isGrouping = _group.GroupBy != null;

        // now we set the value to false, so that subsequent adds will insert
        // into the correct groups.
        _group.IsDataInGroupOrder = false;

        // reset the grouping comparer
        PrepareGroupingComparer(_group);
    }

    /// <summary>
    /// Use the GroupDescriptions to place items into their respective groups.
    /// Because of the fact that we have paging, it is possible that we are only
    /// going to need a subset of the items to be displayed. However, before we 
    /// actually group the entire collection, we can't display the items in the
    /// correct order. We therefore want to just create a temporary group with
    /// the entire collection, and then using this data we can create the group
    /// that is exposed with just the items we need.
    /// </summary>
    private void PrepareTemporaryGroups()
    {
        Debug.Assert(_group != null);
        _temporaryGroup = new ListCollectionViewGroupRoot(this, CheckFlag(CollectionViewFlags.IsDataInGroupOrder));

        foreach (var gd in _group.GroupDescriptions)
        {
            _temporaryGroup.GroupDescriptions.Add(gd);
        }

        _temporaryGroup.Initialize();

        // set to false so that we access internal collection items
        // instead of the group items, as they have been cleared
        _isGrouping = false;

        if (_temporaryGroup.GroupDescriptions.Count > 0)
        {
            for (int num = 0, count = _internalList.Count; num < count; ++num)
            {
                object? item = _internalList[num];
                if (item != null && (!IsAddingNew || !object.Equals(CurrentAddItem, item)))
                {
                    _temporaryGroup.AddToSubgroups(item, loading: true);
                }
            }

            if (IsAddingNew && CurrentAddItem != null)
            {
                _temporaryGroup.InsertSpecialItem(_temporaryGroup.Items.Count, CurrentAddItem, true);
            }
        }

        _isGrouping = _temporaryGroup.GroupBy != null;

        // reset the grouping comparer
        PrepareGroupingComparer(_temporaryGroup);
    }

    /// <summary>
    /// Update our Groups private accessor to point to the subset of data
    /// covered by the current page, or to display the entire group if paging is not
    /// being used.
    /// </summary>
    //TODO Paging
    private void PrepareGroupsForCurrentPage()
    {
        _group.Clear();
        _group.Initialize();

        // set to indicate that we will be pulling data from the temporary group data
        _isUsingTemporaryGroup = true;

        // since we are getting our data from the temporary group, it should
        // already be in group order
        _group.IsDataInGroupOrder = true;
        _group.ActiveComparer     = null;

        if (GroupDescriptions.Count > 0)
        {
            for (int num = 0, count = Count; num < count; ++num)
            {
                object? item = GetItemAt(num);
                if (item != null && (!IsAddingNew || !object.Equals(CurrentAddItem, item)))
                {
                    _group.AddToSubgroups(item, loading: true);
                }
            }

            if (IsAddingNew && CurrentAddItem != null)
            {
                _group.InsertSpecialItem(_group.Items.Count, CurrentAddItem, true);
            }
        }

        // set flag to indicate that we do not need to access the temporary data any longer
        _isUsingTemporaryGroup = false;

        // now we set the value to false, so that subsequent adds will insert
        // into the correct groups.
        _group.IsDataInGroupOrder = false;

        // reset the grouping comparer
        PrepareGroupingComparer(_group);

        _isGrouping = _group.GroupBy != null;
    }

    /// <summary>
    /// Create, filter and sort the local index array.
    /// called from Refresh(), override in derived classes as needed.
    /// </summary>
    /// <param name="enumerable">new IEnumerable to associate this view with</param>
    /// <returns>new local array to use for this view</returns>
    private IList PrepareLocalArray(IEnumerable enumerable)
    {
        Debug.Assert(enumerable != null, "Input list to filter/sort should not be null");

        // filter the collection's array into the local array
        List<object> localList = new List<object>();

        foreach (object item in enumerable)
        {
            if (Filter == null || PassesFilter(item))
            {
                localList.Add(item);
            }
        }

        // sort the local array
        if (!CheckFlag(CollectionViewFlags.IsDataSorted) && SortDescriptions.Count > 0)
        {
            localList = SortList(localList);
        }

        return localList;
    }

    /// <summary>
    /// Process an Add operation from an INotifyCollectionChanged event handler.
    /// </summary>
    /// <param name="addedItem">Item added to the source collection</param>
    /// <param name="addIndex">Index item was added into</param>
    //TODO Paging
    private void ProcessAddEvent(object addedItem, int addIndex)
    {
        // item to fire remove notification for if necessary
        object? removeNotificationItem = null;
        if (PageSize > 0 && !IsGrouping)
        {
            removeNotificationItem = (Count == PageSize) ? GetItemAt(PageSize - 1) : null;
        }

        // process the add by filtering and sorting the item
        ProcessInsertToCollection(
            addedItem,
            addIndex);

        // next check if we need to add an item into the current group
        bool needsGrouping = false;
        if (Count == 1 && GroupDescriptions.Count > 0)
        {
            // if this is the first item being added
            // we want to setup the groups with the
            // correct element type comparer
            if (PageSize > 0)
            {
                PrepareGroupingComparer(_temporaryGroup);
            }

            PrepareGroupingComparer(_group);
        }

        if (IsGrouping)
        {
            int leafIndex = -1;

            if (PageSize > 0)
            {
                _temporaryGroup.AddToSubgroups(addedItem, false /*loading*/);
                leafIndex = _temporaryGroup.LeafIndexOf(addedItem);
            }

            // if we are not paging, we should just be able to add the item.
            // otherwise, we need to validate that it is within the current page.
            if (PageSize == 0 || (PageIndex + 1) * PageSize > leafIndex)
            {
                needsGrouping = true;

                int pageStartIndex = PageIndex * PageSize;

                // if the item was inserted on a previous page
                if (pageStartIndex > leafIndex && PageSize > 0)
                {
                    addedItem = _temporaryGroup.LeafAt(pageStartIndex)!;
                }

                // if we're grouping and have more items than the 
                // PageSize will allow, remove the last item
                if (PageSize > 0 && _group.ItemCount == PageSize)
                {
                    removeNotificationItem = _group.LeafAt(PageSize - 1)!;
                    _group.RemoveFromSubgroups(removeNotificationItem);
                }
            }
        }

        // if we are paging, we may have to fire another notification for the item
        // that needs to be removed for the one we added on this page.
        if (PageSize > 0 && !OnLastLocalPage &&
            (((IsGrouping && removeNotificationItem != null) ||
              (!IsGrouping && (PageIndex + 1) * PageSize > InternalIndexOf(addedItem)))))
        {
            if (removeNotificationItem != null && removeNotificationItem != addedItem)
            {
                HandleCollectionChanged(
                    new NotifyCollectionChangedEventArgs(
                        NotifyCollectionChangedAction.Remove,
                        removeNotificationItem,
                        PageSize - 1));
            }
        }

        // if we need to add the item into the current group
        // that will be displayed
        if (needsGrouping)
        {
            _group.AddToSubgroups(addedItem, false /*loading*/);
        }

        int addedIndex = IndexOf(addedItem);

        // if the item is within the current page
        if (addedIndex >= 0)
        {
            // fire add notification
            HandleCollectionChanged(
                new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Add,
                    addedItem,
                    addedIndex));
        }
        else if (PageSize > 0)
        {
            // otherwise if the item was added into a previous page
            int internalIndex = InternalIndexOf(addedItem);

            if (internalIndex < ConvertToInternalIndex(0))
            {
                // fire add notification for item pushed in
                HandleCollectionChanged(
                    new NotifyCollectionChangedEventArgs(
                        NotifyCollectionChangedAction.Add,
                        GetItemAt(0),
                        0));
            }
        }
    }

    /// <summary>
    /// Process CollectionChanged event on source collection 
    /// that implements INotifyCollectionChanged.
    /// </summary>
    /// <param name="args">
    /// The NotifyCollectionChangedEventArgs to be processed.
    /// </param>
    private void ProcessCollectionChanged(NotifyCollectionChangedEventArgs args)
    {
        // if we do not want to handle the CollectionChanged event, return
        if (!CheckFlag(CollectionViewFlags.ShouldProcessCollectionChanged))
        {
            return;
        }

        if (args.Action == NotifyCollectionChangedAction.Reset)
        {
            var enumerator = SourceCollection.GetEnumerator();
            try
            {
                // if we have no items now, clear our own internal list
                if (!enumerator.MoveNext())
                {
                    _internalList.Clear();
                }
            }
            finally
            {
                if (enumerator is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }

            // calling Refresh, will fire the collectionchanged event
            RefreshOrDefer();
            return;
        }

        // fire notifications for removes
        if (args.OldItems != null &&
            (args.Action == NotifyCollectionChangedAction.Remove ||
             args.Action == NotifyCollectionChangedAction.Replace))
        {
            foreach (var removedItem in args.OldItems)
            {
                ProcessRemoveEvent(removedItem, args.Action == NotifyCollectionChangedAction.Replace);
            }
        }

        // fire notifications for adds
        if (args.NewItems != null &&
            (args.Action == NotifyCollectionChangedAction.Add ||
             args.Action == NotifyCollectionChangedAction.Replace))
        {
            for (var i = 0; i < args.NewItems.Count; i++)
            {
                if (Filter == null || PassesFilter(args.NewItems[i]!))
                {
                    ProcessAddEvent(args.NewItems[i]!, args.NewStartingIndex + i);
                }
            }
        }

        if (args.Action != NotifyCollectionChangedAction.Replace)
        {
            NotifyPropertyChanged(nameof(ItemCount));
        }
    }

    /// <summary>
    /// Process a Remove operation from an INotifyCollectionChanged event handler.
    /// </summary>
    /// <param name="removedItem">Item removed from the source collection</param>
    /// <param name="isReplace">Whether this was part of a Replace operation</param>
    //TODO Paging
    private void ProcessRemoveEvent(object removedItem, bool isReplace)
    {
        int internalRemoveIndex = -1;

        if (IsGrouping)
        {
            internalRemoveIndex =
                PageSize > 0 ? _temporaryGroup.LeafIndexOf(removedItem) : _group.LeafIndexOf(removedItem);
        }
        else
        {
            internalRemoveIndex = InternalIndexOf(removedItem);
        }

        int removeIndex = IndexOf(removedItem);

        // remove the item from the collection
        _internalList.Remove(removedItem);

        // only fire the remove if it was removed from either the current page, or a previous page
        bool needToRemove = (PageSize == 0 && removeIndex >= 0) || (internalRemoveIndex < (PageIndex + 1) * PageSize);

        if (IsGrouping)
        {
            if (PageSize > 0)
            {
                _temporaryGroup.RemoveFromSubgroups(removedItem);
            }

            if (needToRemove)
            {
                _group.RemoveFromSubgroups(removeIndex >= 0 ? removedItem : _group.LeafAt(0)!);
            }
        }

        if (needToRemove)
        {
            // fire remove notification 
            // if we removed from current page, remove from removeIndex,
            // if we removed from previous page, remove first item (index=0)
            HandleCollectionChanged(
                new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Remove,
                    removedItem,
                    Math.Max(0, removeIndex)));

            // if we removed all items from the current page,
            // move to the previous page. we do not need to 
            // fire additional notifications, as moving the page will
            // trigger a reset.
            if (NeedToMoveToPreviousPage && !isReplace)
            {
                MoveToPreviousPage();
                return;
            }

            // if we are paging, we may have to fire another notification for the item
            // that needs to replace the one we removed on this page.
            if (PageSize > 0 && Count == PageSize)
            {
                // we first need to add the item into the current group
                if (IsGrouping)
                {
                    object? newItem = _temporaryGroup.LeafAt((PageSize * (PageIndex + 1)) - 1);
                    if (newItem != null)
                    {
                        _group.AddToSubgroups(newItem, false /*loading*/);
                    }
                }

                // fire the add notification
                HandleCollectionChanged(
                    new NotifyCollectionChangedEventArgs(
                        NotifyCollectionChangedAction.Add,
                        GetItemAt(PageSize - 1),
                        PageSize - 1));
            }
        }
    }

    /// <summary>
    /// Handles adding an item into the collection, and applying sorting, filtering, grouping, paging.
    /// </summary>
    /// <param name="item">Item to insert in the collection</param>
    /// <param name="index">Index to insert item into</param>
    private void ProcessInsertToCollection(object item, int index)
    {
        // first check to see if it passes the filter
        if (Filter == null || PassesFilter(item))
        {
            if (SortDescriptions.Count > 0)
            {
                var itemType = ItemType;
                Debug.Assert(itemType != null);
                foreach (var sort in SortDescriptions)
                    sort.Initialize(itemType);

                // create the SortFieldComparer to use
                var sortFieldComparer = new MergedComparer(this);

                // check if the item would be in sorted order if inserted into the specified index
                // otherwise, calculate the correct sorted index
                if (index < 0 || /* if item was not originally part of list */
                    (index > 0 &&
                     (sortFieldComparer.Compare(item, InternalItemAt(index - 1)) <
                      0)) || /* item has moved up in the list */
                    ((index < InternalList.Count - 1) &&
                     (sortFieldComparer.Compare(item, InternalItemAt(index)) >
                      0))) /* item has moved down in the list */
                {
                    index = sortFieldComparer.FindInsertIndex(item, _internalList);
                }
            }

            // make sure that the specified insert index is within the valid range
            // otherwise, just add it to the end. the index can be set to an invalid
            // value if the item was originally not in the collection, on a different
            // page, or if it had been previously filtered out.
            if (index < 0 || index > _internalList.Count)
            {
                index = _internalList.Count;
            }

            _internalList.Insert(index, item);
        }
    }

     /// <summary>
    /// Raises the PageChanged event
    /// </summary>
    private void RaisePageChanged()
    {
        PageChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Raises the PageChanging event
    /// </summary>
    /// <param name="newPageIndex">Index of the requested page</param>
    /// <returns>True if the event is cancelled (e.Cancel was set to True), False otherwise</returns>
    private bool RaisePageChanging(int newPageIndex)
    {
        EventHandler<PageChangingEventArgs>? handler = PageChanging;
        if (handler != null)
        {
            PageChangingEventArgs pageChangingEventArgs = new PageChangingEventArgs(newPageIndex);
            handler(this, pageChangingEventArgs);
            return pageChangingEventArgs.Cancel;
        }

        return false;
    }

    /// <summary>
    /// Will call RefreshOverride and clear the NeedsRefresh flag
    /// </summary>
    private void RefreshInternal()
    {
        RefreshOverride();
        SetFlag(CollectionViewFlags.NeedsRefresh, false);
    }

    /// <summary>
    /// Refresh, or mark that refresh is needed when defer cycle completes.
    /// </summary>
    private void RefreshOrDefer()
    {
        if (IsRefreshDeferred)
        {
            SetFlag(CollectionViewFlags.NeedsRefresh, true);
        }
        else
        {
            RefreshInternal();
        }
    }

    /// <summary>
    /// Re-create the view, using any SortDescriptions. 
    /// Also updates currency information.
    /// </summary>
    //TODO Paging
    private void RefreshOverride()
    {
        // set IsGrouping to false
        _isGrouping = false;

        // if there's no sort/filter/paging/grouping, just use the collection's array
        if (UsesLocalArray)
        {
            try
            {
                // apply filtering/sorting through the PrepareLocalArray method
                _internalList = PrepareLocalArray(_sourceCollection);

                // apply grouping
                if (PageSize == 0)
                {
                    PrepareGroups();
                }
                else
                {
                    PrepareTemporaryGroups();
                    PrepareGroupsForCurrentPage();
                }
            }
            catch (TargetInvocationException e)
            {
                // If there's an exception while invoking PrepareLocalArray,
                // we want to unwrap it and throw its inner exception
                if (e.InnerException != null)
                {
                    throw e.InnerException;
                }
                throw;
            }
        }
        else
        {
            CopySourceToInternalList();
        }

        // check if PageIndex is still valid after filter/sort
        if (PageSize > 0 &&
            PageIndex > 0 &&
            PageIndex >= PageCount)
        {
            MoveToPage(PageCount - 1);
        }

        HandleCollectionChanged(
            new NotifyCollectionChangedEventArgs(
                NotifyCollectionChangedAction.Reset));
    }
    
     /// <summary>
    /// Sets the specified Flag(s)
    /// </summary>
    /// <param name="flags">Flags we want to set</param>
    /// <param name="value">Value we want to set these flags to</param>
    private void SetFlag(CollectionViewFlags flags, bool value)
    {
        if (value)
        {
            _flags |= flags;
        }
        else
        {
            _flags &= ~flags;
        }
    }

    /// <summary>
    /// Set new SortDescription collection; re-hook collection change notification handler
    /// </summary>
    /// <param name="descriptions">SortDescriptionCollection to set the property value to</param>
    private void SetSortDescriptions(ListSortDescriptionList descriptions)
    {
        if (_sortDescriptions != null)
        {
            _sortDescriptions.CollectionChanged -= SortDescriptionsChanged;
        }

        _sortDescriptions = descriptions;

        if (_sortDescriptions != null)
        {
            Debug.Assert(_sortDescriptions.Count == 0, "must be empty SortDescription collection");
            _sortDescriptions.CollectionChanged += SortDescriptionsChanged;
        }
    }

    /// <summary>
    /// Set new FilterDescription collection; re-hook collection change notification handler
    /// </summary>
    /// <param name="descriptions"></param>
    private void SetFilterDescriptions(ListFilterDescriptionList descriptions)
    {
        if (_filterDescriptions != null)
        {
            _filterDescriptions.CollectionChanged -= FilterDescriptionsChanged;
        }

        _filterDescriptions = descriptions;

        if (_filterDescriptions != null)
        {
            Debug.Assert(_filterDescriptions.Count == 0, "must be empty FilterDescriptions collection");
            _filterDescriptions.CollectionChanged += FilterDescriptionsChanged;
        }
    }

    /// <summary>
    /// SortDescription was added/removed, refresh ListCollectionView
    /// </summary>
    /// <param name="sender">Sender that triggered this handler</param>
    /// <param name="e">NotifyCollectionChangedEventArgs for this change</param>
    private void SortDescriptionsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (IsAddingNew)
        {
            throw new InvalidOperationException(GetOperationNotAllowedDuringAddOrEditText("Sorting"));
        }

        // we want to make sure that the data is refreshed before we try to move to a page
        // since the refresh would take care of the filtering, sorting, and grouping.
        RefreshOrDefer();

        if (PageSize > 0)
        {
            if (IsRefreshDeferred)
            {
                // set cached value and flag so that we move to first page on EndDefer
                _cachedPageIndex = 0;
                SetFlag(CollectionViewFlags.IsMoveToPageDeferred, true);
            }
            else
            {
                MoveToFirstPage();
            }
        }

        NotifyPropertyChanged("SortDescriptions");
    }
    
    /// <summary>
    /// SortDescription was added/removed, refresh ListCollectionView
    /// </summary>
    /// <param name="sender">Sender that triggered this handler</param>
    /// <param name="e">NotifyCollectionChangedEventArgs for this change</param>
    private void FilterDescriptionsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (IsAddingNew)
        {
            throw new InvalidOperationException(GetOperationNotAllowedDuringAddOrEditText("Filtering"));
        }

        // we want to make sure that the data is refreshed before we try to move to a page
        // since the refresh would take care of the filtering, sorting, and grouping.
        RefreshOrDefer();

        if (PageSize > 0)
        {
            if (IsRefreshDeferred)
            {
                // set cached value and flag so that we move to first page on EndDefer
                _cachedPageIndex = 0;
                SetFlag(CollectionViewFlags.IsMoveToPageDeferred, true);
            }
            else
            {
                MoveToFirstPage();
            }
        }

        NotifyPropertyChanged("FilterDescriptions");
    }

    /// <summary>
    /// Sort the List based on the SortDescriptions property.
    /// </summary>
    /// <param name="list">List of objects to sort</param>
    /// <returns>The sorted list</returns>
    private List<object> SortList(List<object> list)
    {
        Debug.Assert(list != null, "Input list to sort should not be null");

        IEnumerable<object> seq      = list;
        var                 itemType = ItemType;
        Debug.Assert(itemType != null);

        foreach (var sort in SortDescriptions)
        {
            sort.Initialize(itemType);

            if (seq is IOrderedEnumerable<object> orderedEnum)
            {
                seq = sort.ThenBy(orderedEnum);
            }
            else
            {
                seq = sort.OrderBy(seq);
            }
        }

        return seq.ToList();
    }

    /// <summary>
    /// Helper to validate that we are not in the middle of a DeferRefresh
    /// and throw if that is the case.
    /// </summary>
    private void VerifyRefreshNotDeferred()
    {
        // If the Refresh is being deferred to change filtering or sorting of the
        // data by this ListCollectionView, then ListCollectionView will not reflect the correct
        // state of the underlying data.
        if (IsRefreshDeferred)
        {
            throw new InvalidOperationException(
                "Cannot change or check the contents or current position of the CollectionView while Refresh is being deferred.");
        }
    }

    int IList.Add(object? value)
    {
        if (SourceList == null)
        {
            return -1;
        }
        var index = SourceList.Add(value);
        if (SourceList is not INotifyCollectionChanged)
        {
            ProcessCollectionChanged(
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, value));
        }

        return index;
    }

    void IList.Clear()
    {
        SourceList?.Clear();
        if (SourceList is not INotifyCollectionChanged)
        {
            ProcessCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }

    void IList.Insert(int index, object? value)
    {
        SourceList?.Insert(index, value);
        if (SourceList is not INotifyCollectionChanged)
        {
            // TODO: implement Insert
            ProcessCollectionChanged(
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset, value));
        }
    }

    void ICollection.CopyTo(Array array, int index) => InternalList.CopyTo(array, index);

    
    [Flags]
    private enum CollectionViewFlags
    {
        /// <summary>
        /// Whether the list of items (after applying the sort and filters, if any) 
        /// is already in the correct order for grouping. 
        /// </summary>
        IsDataInGroupOrder = 0x01,

        /// <summary>
        /// Whether the source collection is already sorted according to the SortDescriptions collection
        /// </summary>
        IsDataSorted = 0x02,

        /// <summary>
        /// Whether we should process the collection changed event
        /// </summary>
        ShouldProcessCollectionChanged = 0x04,

        /// <summary>
        /// Whether we need to refresh
        /// </summary>
        NeedsRefresh = 0x10,

        /// <summary>
        /// Whether we cache the IsEmpty value
        /// </summary>
        CachedIsEmpty = 0x20,

        /// <summary>
        /// Indicates whether a page index change is in process or not
        /// </summary>
        IsPageChanging = 0x40,

        /// <summary>
        /// Whether we need to move to another page after EndDefer
        /// </summary>
        IsMoveToPageDeferred = 0x100,

        /// <summary>
        /// Whether we need to update the PageSize after EndDefer
        /// </summary>
        IsUpdatePageSizeDeferred = 0x200
    }
    
    /// <summary>
    /// A simple monitor class to help prevent re-entrant calls
    /// </summary>
    private class SimpleMonitor : IDisposable
    {
        /// <summary>
        /// Whether the monitor is entered
        /// </summary>
        private bool _entered;

        /// <summary>
        /// Gets a value indicating whether we have been entered or not
        /// </summary>
        public bool Busy => _entered;

        /// <summary>
        /// Sets a value indicating that we have been entered
        /// </summary>
        /// <returns>Boolean value indicating whether we were already entered</returns>
        public bool Enter()
        {
            if (_entered)
            {
                return false;
            }

            _entered = true;
            return true;
        }

        /// <summary>
        /// Cleanup method called when done using this class
        /// </summary>
        public void Dispose()
        {
            _entered = false;
            GC.SuppressFinalize(this);
        }
    }
    
    /// <summary>
    /// IEnumerator generated using the new item taken into account
    /// </summary>
    private class NewItemAwareEnumerator : IEnumerator
    {
        private enum Position
        {
            /// <summary>
            /// Whether the position is before the new item
            /// </summary>
            BeforeNewItem,

            /// <summary>
            /// Whether the position is on the new item that is being created
            /// </summary>
            OnNewItem,

            /// <summary>
            /// Whether the position is after the new item
            /// </summary>
            AfterNewItem
        }

        /// <summary>
        /// Initializes a new instance of the NewItemAwareEnumerator class.
        /// </summary>
        /// <param name="collectionView">The ListCollectionView we are creating the enumerator for</param>
        /// <param name="baseEnumerator">The baseEnumerator that we pass in</param>
        /// <param name="newItem">The new item we are adding to the collection</param>
        public NewItemAwareEnumerator(ListCollectionView collectionView, IEnumerator baseEnumerator, object? newItem)
        {
            _collectionView = collectionView;
            _timestamp      = collectionView.Timestamp;
            _baseEnumerator = baseEnumerator;
            _newItem        = newItem;
        }

        /// <summary>
        /// Implements the MoveNext function for IEnumerable
        /// </summary>
        /// <returns>Whether we can move to the next item</returns>
        public bool MoveNext()
        {
            if (_timestamp != _collectionView.Timestamp)
            {
                throw new InvalidOperationException("Collection was modified; enumeration operation cannot execute.");
            }

            switch (_position)
            {
                case Position.BeforeNewItem:
                    if (_baseEnumerator.MoveNext() &&
                        (_newItem == null || _baseEnumerator.Current != _newItem
                                          || _baseEnumerator.MoveNext()))
                    {
                        // advance base, skipping the new item
                    }
                    else if (_newItem != null)
                    {
                        // if base has reached the end, move to new item
                        _position = Position.OnNewItem;
                    }
                    else
                    {
                        return false;
                    }

                    return true;
            }

            // in all other cases, simply advance base, skipping the new item
            _position = Position.AfterNewItem;
            return _baseEnumerator.MoveNext() &&
                   (_newItem == null
                    || _baseEnumerator.Current != _newItem
                    || _baseEnumerator.MoveNext());
        }
        /// <summary>
        /// Gets the Current value for IEnumerable
        /// </summary>
        public object? Current => (_position == Position.OnNewItem) ? _newItem : _baseEnumerator.Current;

        /// <summary>
        /// Implements the Reset function for IEnumerable
        /// </summary>
        public void Reset()
        {
            _position = Position.BeforeNewItem;
            _baseEnumerator.Reset();
        }

        /// <summary>
        /// CollectionView that we are creating the enumerator for
        /// </summary>
        private ListCollectionView _collectionView;

        /// <summary>
        /// The Base Enumerator that we are passing in
        /// </summary>
        private IEnumerator _baseEnumerator;

        /// <summary>
        /// The position we are appending items to the enumerator
        /// </summary>
        private Position _position;

        /// <summary>
        /// Reference to any new item that we want to add to the collection
        /// </summary>
        private object? _newItem;

        /// <summary>
        /// Timestamp to let us know whether there have been updates to the collection
        /// </summary>
        private int _timestamp;
    }
        
     internal class MergedComparer
    {
        private readonly IComparer<object>[] _comparers;

        public MergedComparer(ListSortDescriptionList coll)
        {
            _comparers = MakeComparerArray(coll);
        }

        public MergedComparer(ListCollectionView collectionView)
            : this(collectionView.SortDescriptions)
        {
        }

        private static IComparer<object>[] MakeComparerArray(ListSortDescriptionList coll)
        {
            return
                coll.Select(c => c.Comparer)
                    .ToArray();
        }

        /// <summary>
        /// Compares two objects and returns a value indicating whether one is less than, equal to or greater than the other.
        /// </summary>
        /// <param name="x">first item to compare</param>
        /// <param name="y">second item to compare</param>
        /// <returns>Negative number if x is less than y, zero if equal, and a positive number if x is greater than y</returns>
        /// <remarks>
        /// Compares the 2 items using the list of property names and directions.
        /// </remarks>
        public int Compare(object? x, object? y)
        {
            int result = 0;

            // compare both objects by each of the properties until property values don't match
            for (int k = 0; k < _comparers.Length; ++k)
            {
                var comparer = _comparers[k];
                result = comparer.Compare(x, y);

                if (result != 0)
                {
                    break;
                }
            }

            return result;
        }

        /// <summary>
        /// Steps through the given list using the comparer to find where
        /// to insert the specified item to maintain sorted order
        /// </summary>
        /// <param name="x">Item to insert into the list</param>
        /// <param name="list">List where we want to insert the item</param>
        /// <returns>Index where we should insert into</returns>
        public int FindInsertIndex(object? x, IList list)
        {
            int min = 0;
            int max = list.Count - 1;
            int index;

            // run a binary search to find the right index
            // to insert into.
            while (min <= max)
            {
                index = (min + max) / 2;

                int result = Compare(x, list[index]);
                if (result == 0)
                {
                    return index;
                } 
                if (result > 0)
                {
                    min = index + 1;
                }
                else
                {
                    max = index - 1;
                }
            }

            return min;
        }
    }
    
    private class DeferHelper : IDisposable
    {
        /// <summary>
        /// Private reference to the CollectionView that created this DeferHelper
        /// </summary>
        private ListCollectionView? _collectionView;

        /// <summary>
        /// Initializes a new instance of the DeferHelper class
        /// </summary>
        /// <param name="collectionView">CollectionView that created this DeferHelper</param>
        public DeferHelper(ListCollectionView? collectionView)
        {
            _collectionView = collectionView;
        }

        /// <summary>
        /// Cleanup method called when done using this class
        /// </summary>
        public void Dispose()
        {
            if (_collectionView != null)
            {
                _collectionView.EndDefer();
                _collectionView = null;
            }

            GC.SuppressFinalize(this);
        }
    }
    
    /// <summary>
    /// Creates a comparer class that takes in a CultureInfo as a parameter,
    /// which it will use when comparing strings.
    /// </summary>
    private class CultureSensitiveComparer : IComparer<object>
    {
        /// <summary>
        /// Private accessor for the CultureInfo of our comparer
        /// </summary>
        private CultureInfo _culture;

        /// <summary>
        /// Creates a comparer which will respect the CultureInfo
        /// that is passed in when comparing strings.
        /// </summary>
        /// <param name="culture">The CultureInfo to use in string comparisons</param>
        public CultureSensitiveComparer(CultureInfo? culture)
        {
            _culture = culture ?? CultureInfo.InvariantCulture;
        }

        /// <summary>
        /// Compares two objects and returns a value indicating whether one is less than, equal to or greater than the other.
        /// </summary>
        /// <param name="x">first item to compare</param>
        /// <param name="y">second item to compare</param>
        /// <returns>Negative number if x is less than y, zero if equal, and a positive number if x is greater than y</returns>
        /// <remarks>
        /// Compares the 2 items using the specified CultureInfo for string and using the default object comparer for all other objects.
        /// </remarks>
        public int Compare(object? x, object? y)
        {
            if (x == null)
            {
                if (y != null)
                {
                    return -1;
                }

                return 0;
            }

            if (y == null)
            {
                return 1;
            }

            // at this point x and y are not null
            if (x is string xString && y is string yString)
            {
                return _culture.CompareInfo.Compare(xString, yString);
            }
            return Comparer<object>.Default.Compare(x, y);
        }
    }
}