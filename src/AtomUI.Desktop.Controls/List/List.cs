using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Reactive.Disposables;
using AtomUI.Controls;
using AtomUI.Data;
using AtomUI.Desktop.Controls.Data;
using AtomUI.Desktop.Controls.Themes;
using AtomUI.Theme;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Metadata;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

public class List : TemplatedControl,
                    ISizeTypeAware,
                    IMotionAwareControl,
                    IControlSharedTokenResourcesHost
{
    #region 公共属性定义
    
    public static readonly StyledProperty<IEnumerable?> ItemsSourceProperty =
        ItemsControl.ItemsSourceProperty.AddOwner<List>();
    
    public static readonly StyledProperty<IDataTemplate?> ItemTemplateProperty =
        ItemsControl.ItemTemplateProperty.AddOwner<List>();
    
    public static readonly StyledProperty<IDataTemplate?> GroupItemTemplateProperty =
        AvaloniaProperty.Register<List, IDataTemplate?>(nameof(GroupItemTemplate));
    
    public static readonly DirectProperty<List, int> ItemCountProperty =
        AvaloniaProperty.RegisterDirect<List, int>(nameof(ItemCount), o => o.ItemCount);
    
    public static readonly DirectProperty<List, IListCollectionView?> CollectionViewProperty =
        AvaloniaProperty.RegisterDirect<List, IListCollectionView?>(nameof(CollectionView),
            o => o.CollectionView);
    
    public static readonly StyledProperty<bool> IsOperatingProperty =
        AvaloniaProperty.Register<List, bool>(nameof(IsOperating));
    
    public static readonly StyledProperty<string?> OperatingMsgProperty =
        AvaloniaProperty.Register<List, string?>(nameof(OperatingMsg));
    
    public static readonly StyledProperty<object?> CustomOperatingIndicatorProperty =
        AvaloniaProperty.Register<List, object?>(nameof(CustomOperatingIndicator));

    public static readonly StyledProperty<IDataTemplate?> CustomOperatingIndicatorTemplateProperty =
        AvaloniaProperty.Register<List, IDataTemplate?>(nameof(CustomOperatingIndicatorTemplate));
    
    public static readonly StyledProperty<object?> EmptyIndicatorProperty =
        AvaloniaProperty.Register<List, object?>(nameof(EmptyIndicator));
    
    public static readonly StyledProperty<IDataTemplate?> EmptyIndicatorTemplateProperty =
        AvaloniaProperty.Register<List, IDataTemplate?>(nameof(EmptyIndicatorTemplate));
    
    public static readonly StyledProperty<bool> IsShowEmptyIndicatorProperty =
        AvaloniaProperty.Register<List, bool>(nameof(IsShowEmptyIndicator), true);
    
    public static readonly StyledProperty<bool> IsGroupEnabledProperty =
        AvaloniaProperty.Register<List, bool>(nameof(IsGroupEnabled), false);
    
    public static readonly StyledProperty<string> GroupPropertyPathProperty =
        AvaloniaProperty.Register<List, string>(nameof(GroupPropertyPath), "Group");
    
    public static readonly DirectProperty<List, int> SelectedIndexProperty =
        AvaloniaProperty.RegisterDirect<List, int>(
            nameof(SelectedIndex),
            o => o.SelectedIndex,
            (o, v) => o.SelectedIndex = v,
            unsetValue: -1,
            defaultBindingMode: BindingMode.TwoWay);
    
    public static readonly DirectProperty<List, IList?> SelectedItemsProperty =
        AvaloniaProperty.RegisterDirect<List, IList?>(nameof(SelectedItems), 
            o => o.SelectedItems, 
            (o, v) => o.SelectedItems = v);
    
    public static readonly DirectProperty<List, object?> SelectedItemProperty =
        AvaloniaProperty.RegisterDirect<List, object?>(
            nameof(SelectedItem),
            o => o.SelectedItem,
            (o, v) => o.SelectedItem = v,
            defaultBindingMode: BindingMode.TwoWay, enableDataValidation: true);
    
    public static readonly StyledProperty<SelectionMode> SelectionModeProperty =
        AvaloniaProperty.Register<List, SelectionMode>(nameof(SelectionMode));
    
    public static readonly StyledProperty<bool> IsItemSelectableProperty =
        AvaloniaProperty.Register<List, bool>(nameof(IsItemSelectable), true);
    
    public static readonly StyledProperty<SizeType> SizeTypeProperty =
        SizeTypeControlProperty.SizeTypeProperty.AddOwner<List>();
    
    public static readonly StyledProperty<bool> IsBorderlessProperty =
        AvaloniaProperty.Register<List, bool>(nameof(IsBorderless), false);
    
    public static readonly StyledProperty<bool> IsShowSelectedIndicatorProperty =
        AvaloniaProperty.Register<List, bool>(nameof(IsShowSelectedIndicator), false);
    
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<List>();
    
    public static readonly StyledProperty<IBrush?> ItemHoverBgProperty =
        ListBox.ItemHoverBgProperty.AddOwner<List>();
    
    public static readonly StyledProperty<IBrush?> ItemSelectedBgProperty =
        ListBox.ItemSelectedBgProperty.AddOwner<List>();
    
    public static readonly StyledProperty<int> PageSizeProperty =
        AvaloniaProperty.Register<List, int>(nameof(PageSize), 0);
    
    public static readonly StyledProperty<bool> IsHideOnSinglePageProperty =
        AbstractPagination.IsHideOnSinglePageProperty.AddOwner<List>();
    
    public static readonly StyledProperty<ListPaginationVisibility> PaginationVisibilityProperty =
        AvaloniaProperty.Register<List, ListPaginationVisibility>(nameof(PaginationVisibility), ListPaginationVisibility.Bottom);
    
    public static readonly StyledProperty<PaginationAlign> TopPaginationAlignProperty =
        AvaloniaProperty.Register<List, PaginationAlign>(nameof(TopPaginationAlign), PaginationAlign.End);
    
    public static readonly StyledProperty<PaginationAlign> BottomPaginationAlignProperty =
        AvaloniaProperty.Register<List, PaginationAlign>(nameof(BottomPaginationAlign), PaginationAlign.End);
    
    public static readonly StyledProperty<Thickness> EmptyIndicatorPaddingProperty =
        AvaloniaProperty.Register<List, Thickness>(nameof(EmptyIndicatorPadding));
    
    public static readonly StyledProperty<bool> AutoScrollToSelectedItemProperty =
        SelectingItemsControl.AutoScrollToSelectedItemProperty.AddOwner<List>();
    
    public static readonly StyledProperty<ITemplate<Panel?>> ItemsPanelProperty = 
        ItemsControl.ItemsPanelProperty.AddOwner<List>();
    
    public IEnumerable? ItemsSource
    {
        get => GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }
    
    [InheritDataTypeFromItems(nameof(ItemsSource))]
    public IDataTemplate? ItemTemplate
    {
        get => GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }
    
    public IDataTemplate? GroupItemTemplate
    {
        get => GetValue(GroupItemTemplateProperty);
        set => SetValue(GroupItemTemplateProperty, value);
    }
    
    [DependsOn(nameof(EmptyIndicatorTemplate))]
    public object? EmptyIndicator
    {
        get => GetValue(EmptyIndicatorProperty);
        set => SetValue(EmptyIndicatorProperty, value);
    }

    public IDataTemplate? EmptyIndicatorTemplate
    {
        get => GetValue(EmptyIndicatorTemplateProperty);
        set => SetValue(EmptyIndicatorTemplateProperty, value);
    }
    
    public bool IsShowEmptyIndicator
    {
        get => GetValue(IsShowEmptyIndicatorProperty);
        set => SetValue(IsShowEmptyIndicatorProperty, value);
    }
    
    public bool IsGroupEnabled
    {
        get => GetValue(IsGroupEnabledProperty);
        set => SetValue(IsGroupEnabledProperty, value);
    }
    
    public string GroupPropertyPath
    {
        get => GetValue(GroupPropertyPathProperty);
        set => SetValue(GroupPropertyPathProperty, value);
    }
    
    private int _selectedIndex;

    public int SelectedIndex
    {
        get => _selectedIndex;
        set => SetAndRaise(SelectedIndexProperty, ref _selectedIndex, value);
    }
    
    private IList? _selectedItems;

    public IList? SelectedItems
    {
        get => _selectedItems;
        set => SetAndRaise(SelectedItemsProperty, ref _selectedItems, value);
    }
    
    private object? _selectedItem;

    public object? SelectedItem
    {
        get => _selectedItem;
        set => SetAndRaise(SelectedItemProperty, ref _selectedItem, value);
    }
    
    public SelectionMode SelectionMode
    {
        get => GetValue(SelectionModeProperty);
        set => SetValue(SelectionModeProperty, value);
    }
    
    public bool IsItemSelectable
    {
        get => GetValue(IsItemSelectableProperty);
        set => SetValue(IsItemSelectableProperty, value);
    }
    
    public SizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }
    
    public bool IsShowSelectedIndicator
    {
        get => GetValue(IsShowSelectedIndicatorProperty);
        set => SetValue(IsShowSelectedIndicatorProperty, value);
    }
    
    public bool IsBorderless
    {
        get => GetValue(IsBorderlessProperty);
        set => SetValue(IsBorderlessProperty, value);
    }
    
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }
    
    public IBrush? ItemHoverBg
    {
        get => GetValue(ItemHoverBgProperty);
        set => SetValue(ItemHoverBgProperty, value);
    }
    
    public IBrush? ItemSelectedBg
    {
        get => GetValue(ItemSelectedBgProperty);
        set => SetValue(ItemSelectedBgProperty, value);
    }

    public int ItemCount => CollectionView?.Count ?? 0;
    
    public int PageSize
    {
        get => GetValue(PageSizeProperty);
        set => SetValue(PageSizeProperty, value);
    }
    
    public bool IsHideOnSinglePage
    {
        get => GetValue(IsHideOnSinglePageProperty);
        set => SetValue(IsHideOnSinglePageProperty, value);
    }

    public IListCollectionView? CollectionView
    {
        get => _listCollectionView;
        private set => _listCollectionView = value;
    }
    
    public bool IsOperating
    {
        get => GetValue(IsOperatingProperty);
        set => SetValue(IsOperatingProperty, value);
    }
    
    public string? OperatingMsg
    {
        get => GetValue(OperatingMsgProperty);
        set => SetValue(OperatingMsgProperty, value);
    }
    
    [DependsOn(nameof(CustomOperatingIndicatorTemplate))]
    public object? CustomOperatingIndicator
    {
        get => GetValue(CustomOperatingIndicatorProperty);
        set => SetValue(CustomOperatingIndicatorProperty, value);
    }
    
    public IDataTemplate? CustomOperatingIndicatorTemplate
    {
        get => GetValue(CustomOperatingIndicatorTemplateProperty);
        set => SetValue(CustomOperatingIndicatorTemplateProperty, value);
    }
    
    public ListPaginationVisibility PaginationVisibility
    {
        get => GetValue(PaginationVisibilityProperty);
        set => SetValue(PaginationVisibilityProperty, value);
    }
    
    public PaginationAlign TopPaginationAlign
    {
        get => GetValue(TopPaginationAlignProperty);
        set => SetValue(TopPaginationAlignProperty, value);
    }
    
    public PaginationAlign BottomPaginationAlign
    {
        get => GetValue(BottomPaginationAlignProperty);
        set => SetValue(BottomPaginationAlignProperty, value);
    }
    
    public Thickness EmptyIndicatorPadding
    {
        get => GetValue(EmptyIndicatorPaddingProperty);
        set => SetValue(EmptyIndicatorPaddingProperty, value);
    }
    
    public bool AutoScrollToSelectedItem
    {
        get => GetValue(AutoScrollToSelectedItemProperty);
        set => SetValue(AutoScrollToSelectedItemProperty, value);
    }
    
    public ListSortDescriptionList? SortDescriptions
    {
        get
        {
            if (CollectionView != null && CollectionView.CanSort)
            {
                return CollectionView.SortDescriptions;
            }
            return null;
        }
    }
    
    public ListFilterDescriptionList? FilterDescriptions
    {
        get
        {
            if (CollectionView != null && CollectionView.CanFilter)
            {
                return CollectionView.FilterDescriptions;
            }
            return null;
        }
    }
    
    public ITemplate<Panel?> ItemsPanel
    {
        get => GetValue(ItemsPanelProperty);
        set => SetValue(ItemsPanelProperty, value);
    }
    
    #endregion

    #region 公共事件定义

    public event EventHandler<ListCollectionViewChangedEventArgs>? CollectionViewChanged;
    
    public static readonly RoutedEvent<SelectionChangedEventArgs> SelectionChangedEvent =
        RoutedEvent.Register<List, SelectionChangedEventArgs>(
            nameof(SelectionChanged),
            RoutingStrategies.Bubble);

    public event EventHandler<SelectionChangedEventArgs>? SelectionChanged
    {
        add => AddHandler(SelectionChangedEvent, value);
        remove => RemoveHandler(SelectionChangedEvent, value);
    }
    
    #endregion
    
    #region 内部属性定义
    
    internal static readonly DirectProperty<List, Thickness> EffectiveBorderThicknessProperty =
        AvaloniaProperty.RegisterDirect<List, Thickness>(nameof(EffectiveBorderThickness),
            o => o.EffectiveBorderThickness,
            (o, v) => o.EffectiveBorderThickness = v);
    
    internal static readonly DirectProperty<List, bool> IsEmptyDataSourceProperty =
        AvaloniaProperty.RegisterDirect<List, bool>(
            nameof(IsEmptyDataSource),
            o => o.IsEmptyDataSource,
            (o, v) => o.IsEmptyDataSource = v);
    
    internal static readonly DirectProperty<List, bool> IsEffectiveEmptyVisibleProperty =
        AvaloniaProperty.RegisterDirect<List, bool>(
            nameof(IsEffectiveEmptyVisible),
            o => o.IsEffectiveEmptyVisible,
            (o, v) => o.IsEffectiveEmptyVisible = v);
    
    private Thickness _effectiveBorderThickness;

    internal Thickness EffectiveBorderThickness
    {
        get => _effectiveBorderThickness;
        set => SetAndRaise(EffectiveBorderThicknessProperty, ref _effectiveBorderThickness, value);
    }
    
    private bool _isEmptyDataSource = true;
    internal bool IsEmptyDataSource
    {
        get => _isEmptyDataSource;
        set => SetAndRaise(IsEmptyDataSourceProperty, ref _isEmptyDataSource, value);
    }
    
    private bool _isEffectiveEmptyVisible = false;
    internal bool IsEffectiveEmptyVisible
    {
        get => _isEffectiveEmptyVisible;
        set => SetAndRaise(IsEffectiveEmptyVisibleProperty, ref _isEffectiveEmptyVisible, value);
    }

    internal bool EventsWired
    {
        get;
        private set;
    }
    
    protected static object DefaultRecycleKey { get; } = new ();
    
    Control IControlSharedTokenResourcesHost.HostControl => this;
    string IControlSharedTokenResourcesHost.TokenId => ListToken.ID;

    #endregion
    
    private static readonly FuncTemplate<Panel?> DefaultPanel =
        new(() => new VirtualizingStackPanel());
    
    private IListCollectionView? _listCollectionView;
    private bool _areHandlersSuspended;
    private bool _measured;
    private Pagination? _topPagination;
    private Pagination? _bottomPagination;
    internal GroupableListView? ListView;
    private CompositeDisposable? _relayBindingDisposables;
    
    static List()
    {
        ItemsPanelProperty.OverrideDefaultValue<List>(DefaultPanel);
        ItemsSourceProperty.Changed.AddClassHandler<List>((x, e) => x.HandleItemsSourcePropertyChanged(e));
        IsHideOnSinglePageProperty.OverrideDefaultValue<List>(true);
    }
    
    public List()
    {
        this.RegisterResources();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == BorderThicknessProperty ||
            change.Property == IsBorderlessProperty)
        {
            ConfigureEffectiveBorderThickness();
        }
        else if (change.Property == SelectionModeProperty)
        {
            SyncSelectionState();
        }
        else if (change.Property == IsGroupEnabledProperty)
        {
            if (_listCollectionView != null)
            {
                using (_listCollectionView.DeferRefresh())
                {
                    ConfigureGroupInfo();
                }
            }
        }
        else if (change.Property == IsShowEmptyIndicatorProperty ||
                 change.Property == IsEmptyDataSourceProperty)
        {
            ConfigureEmptyIndicator();
        }
        else if (change.Property == GroupPropertyPathProperty)
        {
            ReConfigureGroupInfo();
        }
    }

    private void ConfigureEffectiveBorderThickness()
    {
        if (IsBorderless)
        {
            EffectiveBorderThickness = new Thickness(0);
        }
        else
        {
            EffectiveBorderThickness = BorderThickness;
        }
    }
    
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        if (!IsSet(EmptyIndicatorProperty))
        {
            SetValue(EmptyIndicatorProperty, new Empty()
            {
                SizeType    = SizeType.Small,
                PresetImage = PresetEmptyImage.Simple
            }, BindingPriority.Template);
        }
        _topPagination    = e.NameScope.Find<Pagination>(ListThemeConstants.TopPaginationPart);
        _bottomPagination = e.NameScope.Find<Pagination>(ListThemeConstants.BottomPaginationPart);

        if (ListView != null)
        {
            ListView.SelectionChanged -= HandleListViewSelectionChanged;
        }

        ListView   = e.NameScope.Find<GroupableListView>(ListThemeConstants.ListViewPart);
        if (ListView != null)
        {
            ListView.OwningList       =  this;
            ListView.SelectionChanged += HandleListViewSelectionChanged;
            SyncSelectionState();
        }
        UpdatePseudoClasses();
        ConfigureEmptyIndicator();
    }

    private void HandleListViewSelectionChanged(object? sender, SelectionChangedEventArgs args)
    {
        RaiseEvent(new SelectionChangedEventArgs(SelectionChangedEvent, args.RemovedItems, args.AddedItems));
    }
    
    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(ListPseudoClass.Empty, ItemCount == 0);
        PseudoClasses.Set(ListPseudoClass.SingleItem, ItemCount == 1);
    }

    private void HandleItemsSourcePropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        if (!_areHandlersSuspended)
        {
            var oldCollectionView = _listCollectionView;
            
            var newItemsSource = (IEnumerable?)change.NewValue;

            _listCollectionView = null;
            IListCollectionView? newCollectionView;
            if (newItemsSource is IListCollectionView)
            {
                newCollectionView   = (IListCollectionView)newItemsSource;
            }
            else
            {
                newCollectionView = newItemsSource is not null
                    ? CreateView(newItemsSource)
                    : default;
            }
            if (oldCollectionView != null)
            {
                oldCollectionView.CollectionChanged -= HandleDataCollectionViewChanged;
                if (oldCollectionView is ListCollectionView oldDataGridCollectionView)
                {
                    oldDataGridCollectionView.PageChanging -= HandlePageChanging;
                }
            }
            if (newCollectionView != null)
            {
                newCollectionView.CollectionChanged += HandleDataCollectionViewChanged;
                if (newCollectionView is ListCollectionView newDataGridCollectionView)
                {
                    newDataGridCollectionView.PageChanging += HandlePageChanging;
                }

                IsEmptyDataSource = newCollectionView.IsEmpty;
                if (newCollectionView.Filter == null)
                {
                    newCollectionView.Filter = new ListDefaultFilter(newCollectionView);
                }
            }
            else
            {
                IsEmptyDataSource = true;
            }
            
            _listCollectionView = newCollectionView;
            
            if (oldCollectionView != newCollectionView)
            {
                RaisePropertyChanged(CollectionViewProperty, oldCollectionView, newCollectionView);
                CollectionViewChanged?.Invoke(this, new ListCollectionViewChangedEventArgs(oldCollectionView, newCollectionView));
            }
            ConfigureGroupInfo();
            _measured     = false;
            SelectedItems = null;
            SelectedItem  = null;
            SelectedIndex = -1;
            ReConfigurePagination();
            InvalidateMeasure();
            UpdatePseudoClasses();
        }
    }
    
    private void HandleDataCollectionViewChanged(object? sender, NotifyCollectionChangedEventArgs args)
    {
        if (sender is ListCollectionView view)
        {
            IsEmptyDataSource = view.IsEmpty;
        }
    }
    
    private void HandlePageChanging(object? sender, PageChangingEventArgs args)
    {
        var targetPage = args.NewPageIndex + 1;
        if (_topPagination != null && _topPagination.CurrentPage != targetPage)
        {
            _topPagination.CurrentPage = targetPage;
        }
        
        if (_bottomPagination != null && _bottomPagination.CurrentPage != targetPage)
        {
            _bottomPagination.CurrentPage = targetPage;
        }
    }
    
    private void SetValueNoCallback<T>(AvaloniaProperty<T> property, T value,
                                       BindingPriority priority = BindingPriority.LocalValue)
    {
        _areHandlersSuspended = true;
        try
        {
            SetValue(property, value, priority);
        }
        finally
        {
            _areHandlersSuspended = false;
        }
    }
    
    internal static IListCollectionView CreateView(IEnumerable source)
    {
        Debug.Assert(source != null, "source unexpectedly null");
        Debug.Assert(!(source is IListCollectionView), "source is an IListCollectionView");

        IListCollectionView? collectionView = null;

        if (source is IListCollectionViewFactory collectionViewFactory)
        {
            // If the source is a collection view factory, give it a chance to produce a custom collection view.
            collectionView = collectionViewFactory.CreateView();
            // Intentionally not catching potential exception thrown by ICollectionViewFactory.CreateView().
        }
        if (collectionView == null)
        {
            // If we still do not have a collection view, default to a PagedCollectionView.
            collectionView = new ListCollectionView(source);
        }
        return collectionView;
    }
    
    private void ReConfigurePagination()
    {
        if (CollectionView is ListCollectionView collectionView)
        {
            collectionView.PageSize = PageSize;
            if (_topPagination != null)
            {
                _topPagination.Total       = collectionView.ItemCount;
                _topPagination.PageSize    = PageSize;
                _topPagination.CurrentPage = Pagination.DefaultCurrentPage;
              
            }
            if (_bottomPagination != null)
            {
                _bottomPagination.Total       = collectionView.ItemCount;
                _bottomPagination.PageSize    = PageSize;
                _bottomPagination.CurrentPage = Pagination.DefaultCurrentPage;
            }
        }
    }
    
    protected override Size MeasureOverride(Size availableSize)
    {
        if (!_measured)
        {
            _measured = true;
        }
        return base.MeasureOverride(availableSize);
    }
    
    internal bool Any()
    {
        return TryGetCount(false, true, out var count) && count > 0;
    }
    
    internal bool TryGetCount(bool allowSlow, bool getAny, out int count)
    {
        bool result;
        (result, count) = CollectionView switch
        {
            ICollection collection => (true, collection.Count),
            IEnumerable enumerable when allowSlow && !getAny => (true, enumerable.Cast<object>().Count()),
            IEnumerable enumerable when getAny => (true, enumerable.Cast<object>().Any() ? 1 : 0),
            _ => (false, 0)
        };
        return result;
    }

    private void ConfigureGroupInfo()
    {
        if (_listCollectionView is ListCollectionView collectionView)
        {
            if (IsGroupEnabled)
            {
                collectionView.GroupDescriptions.Add(new ListPathGroupDescription(GroupPropertyPath));
            }
            else
            {
                collectionView.GroupDescriptions.Clear();
            }
        }
    }

    private void ReConfigureGroupInfo()
    {
        if (_listCollectionView is ListCollectionView collectionView)
        {
            collectionView.GroupDescriptions.Clear();
            if (IsGroupEnabled)
            {
                collectionView.GroupDescriptions.Add(new ListPathGroupDescription(GroupPropertyPath));
            }
        }
    }

    internal virtual Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        return new ListItem();
    }

    internal virtual bool? NeedsContainerOverride(object? item, int index, out object? recycleKey)
    {
        return NeedsContainer<ListItem>(item, out recycleKey);
    }
    
    internal bool NeedsContainer<T>(object? item, out object? recycleKey) where T : Control
    {
        if (item is T)
        {
            recycleKey = null;
            return false;
        }
        recycleKey = DefaultRecycleKey;
        return true;
    }

    internal virtual void PrepareListBoxItem(ListItem listItem, object? item, int index,
                                             CompositeDisposable disposables)
    {
        if (GroupItemTemplate != null)
        {
            disposables.Add(BindUtils.RelayBind(this, GroupItemTemplateProperty, listItem, ListItem.ContentTemplateProperty));
        }

        if (item is IGroupListItemData groupListItemData)
        {
            listItem.IsGroupItem = groupListItemData.IsGroupItem;
        }
    }

    internal virtual bool UpdateSelectionFromPointerEvent(Control source, PointerEventArgs e)
    {
        return false;
    }
    
    protected Control? GetContainerFromEventSource(object? eventSource)
    {
        if (ListView != null)
        {
            for (var current = eventSource as Visual; current != null; current = current.GetVisualParent())
            {
                if (current is Control control && control.Parent == ListView &&
                    ListView.IndexFromContainer(control) != -1)
                {
                    return control;
                }
            }
        }
        return null;
    }
    
    private void SyncSelectionState()
    {
        if (ListView != null)
        {
            _relayBindingDisposables?.Dispose();
            _relayBindingDisposables = new CompositeDisposable(2); 
            _relayBindingDisposables.Add(BindUtils.RelayBind(this, SelectionModeProperty, ListView, GroupableListView.SelectionModeProperty));
        }
    }

    protected virtual void ConfigureEmptyIndicator()
    {
        SetCurrentValue(IsEffectiveEmptyVisibleProperty, IsShowEmptyIndicator && IsEmptyDataSource);
    }
    
    protected internal virtual void NotifyListItemClicked(ListItem item)
    {
    }
}