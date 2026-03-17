using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Reactive.Disposables;
using AtomUI.Controls;
using AtomUI.Controls.Data;
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
using Avalonia.Input.Raw;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Metadata;
using Avalonia.VisualTree;
using VirtualizingStackPanel = Avalonia.Controls.VirtualizingStackPanel;

namespace AtomUI.Desktop.Controls;

public partial class List : TemplatedControl,
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
    
    public static readonly StyledProperty<ListGroupPropertySelector?> GroupPropertySelectorProperty =
        AvaloniaProperty.Register<List, ListGroupPropertySelector?>(
            nameof(GroupPropertySelector));
    
    public static readonly StyledProperty<SizeType> SizeTypeProperty =
        SizeTypeControlProperty.SizeTypeProperty.AddOwner<List>();
    
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
    
    public static readonly StyledProperty<AbstractPagination?> TopPaginationProperty =
        AvaloniaProperty.Register<List, AbstractPagination?>(nameof(TopPagination));
    
    public static readonly StyledProperty<AbstractPagination?> BottomPaginationProperty =
        AvaloniaProperty.Register<List, AbstractPagination?>(nameof(BottomPagination));
    
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
    
    [InheritDataTypeFromItems(nameof(ItemsSource))]
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
    
    public ListGroupPropertySelector? GroupPropertySelector
    {
        get => GetValue(GroupPropertySelectorProperty);
        set => SetValue(GroupPropertySelectorProperty, value);
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

    private int _itemCount;

    public int ItemCount
    {
        get => _itemCount;
        private set => SetAndRaise(ItemCountProperty, ref _itemCount, value);
    }
    
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
    
    public SortDescriptionList? SortDescriptions
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
    
    public FilterDescriptionList? FilterDescriptions
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
    
    public AbstractPagination? TopPagination
    {
        get => GetValue(TopPaginationProperty);
        set => SetValue(TopPaginationProperty, value);
    }
    
    public AbstractPagination? BottomPagination
    {
        get => GetValue(BottomPaginationProperty);
        set => SetValue(BottomPaginationProperty, value);
    }
    #endregion

    #region 公共事件定义
    public static readonly RoutedEvent<SelectionChangedEventArgs> SelectionChangedEvent =
        RoutedEvent.Register<List, SelectionChangedEventArgs>(
            nameof(SelectionChanged),
            RoutingStrategies.Bubble);

    public event EventHandler<SelectionChangedEventArgs>? SelectionChanged
    {
        add => AddHandler(SelectionChangedEvent, value);
        remove => RemoveHandler(SelectionChangedEvent, value);
    }
    
    public event EventHandler<ListCollectionViewChangedEventArgs>? CollectionViewChanged;
    public event EventHandler<ItemCountChangedEventArgs>? ItemCountChanged;
    
    #endregion
    
    #region 内部属性定义
    
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
    
    protected static object DefaultRecycleKey { get; } = new ();
    
    Control IControlSharedTokenResourcesHost.HostControl => this;
    string IControlSharedTokenResourcesHost.TokenId => ListToken.ID;

    #endregion
    
    private static readonly FuncTemplate<Panel?> DefaultPanel =
        new(() => new VirtualizingStackPanel());
    
    private IListCollectionView? _listCollectionView;
    private bool _areHandlersSuspended;
    private bool _measured;
    private AbstractPagination? _topPagination;
    private AbstractPagination? _bottomPagination;
    internal GroupableListView? ListView;
    private CompositeDisposable? _relayBindingDisposables;
    private CompositeDisposable? _topPaginationDisposables;
    private CompositeDisposable? _bottomPaginationDisposables;
    private IDisposable? _inputManagerDisposable;
    
    static List()
    {
        ItemsPanelProperty.OverrideDefaultValue<List>(DefaultPanel);
        ItemsSourceProperty.Changed.AddClassHandler<List>((list, e) => list.HandleItemsSourcePropertyChanged(e));
        IsHideOnSinglePageProperty.OverrideDefaultValue<List>(true);
        ItemCountProperty.Changed.AddClassHandler<List>((list, args) => list.HandleItemCountChanged());
    }
    
    public List()
    {
        this.RegisterResources();
    }
    
    protected override void OnDataContextBeginUpdate()
    {
        base.OnDataContextBeginUpdate();
        BeginUpdating();
    }
    
    protected override void OnDataContextEndUpdate()
    {
        base.OnDataContextEndUpdate();
        EndUpdating();
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        if (GroupPropertySelector == null)
        {
            SetCurrentValue(GroupPropertySelectorProperty, GroupSelector);
        }
        TryInitializeSelectionSource(_selection, _updateState is null);
    }

    private object? GroupSelector(object item)
    {
        if (item is IGroupHeader groupHeader)
        {
            return groupHeader.Group;
        }

        return null;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == SelectionModeProperty)
        {
            SyncSelectionMode();
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
        else if (change.Property == GroupPropertySelectorProperty)
        {
            ReConfigureGroupInfo();
        }
        else if (change.Property == TopPaginationProperty)
        {
            HandleTopPaginationChanged(change);
        }
        else if (change.Property == BottomPaginationProperty)
        {
            HandleBottomPaginationChanged(change);
        }
        else if (change.Property == PaginationVisibilityProperty)
        {
            HandlePaginationVisibility();
        }

        NotifyPropertyChangedForSelection(change);
    }

    private void HandleTopPaginationChanged(AvaloniaPropertyChangedEventArgs args)
    {
        if (args.OldValue is AbstractPagination oldPagination)
        {
            oldPagination.CurrentPageChanged -= HandlePageChangeRequest;
            _topPaginationDisposables?.Dispose();
        }

        if (args.NewValue is AbstractPagination newPagination)
        {
            newPagination.CurrentPageChanged += HandlePageChangeRequest;
            _topPaginationDisposables        =  new CompositeDisposable();
            _topPaginationDisposables.Add(BindUtils.RelayBind(this, TopPaginationAlignProperty, newPagination, AbstractPagination.AlignProperty));
            _topPaginationDisposables.Add(BindUtils.RelayBind(this, IsHideOnSinglePageProperty, newPagination, AbstractPagination.IsHideOnSinglePageProperty));
            _topPaginationDisposables.Add(BindUtils.RelayBind(this, IsEnabledProperty, newPagination, AbstractPagination.IsEnabledProperty));
            _topPaginationDisposables.Add(BindUtils.RelayBind(this, SizeTypeProperty, newPagination, AbstractPagination.SizeTypeProperty));
            _topPaginationDisposables.Add(BindUtils.RelayBind(this, IsMotionEnabledProperty, newPagination, AbstractPagination.IsMotionEnabledProperty));
            _topPaginationDisposables.Add(BindUtils.RelayBind(this, PaginationVisibilityProperty, newPagination, AbstractPagination.IsMotionEnabledProperty));
            _topPagination = newPagination;
            HandlePaginationVisibility();
        }
    }
    
    private void HandleBottomPaginationChanged(AvaloniaPropertyChangedEventArgs args)
    {
        if (args.OldValue is AbstractPagination oldPagination)
        {
            oldPagination.CurrentPageChanged -= HandlePageChangeRequest;
            _bottomPaginationDisposables?.Dispose();
        }

        if (args.NewValue is AbstractPagination newPagination)
        {
            newPagination.CurrentPageChanged += HandlePageChangeRequest;
            _bottomPaginationDisposables     =  new  CompositeDisposable();
            _bottomPaginationDisposables.Add(BindUtils.RelayBind(this, TopPaginationAlignProperty, newPagination, AbstractPagination.AlignProperty));
            _bottomPaginationDisposables.Add(BindUtils.RelayBind(this, IsHideOnSinglePageProperty, newPagination, AbstractPagination.IsHideOnSinglePageProperty));
            _bottomPaginationDisposables.Add(BindUtils.RelayBind(this, IsEnabledProperty, newPagination, AbstractPagination.IsEnabledProperty));
            _bottomPaginationDisposables.Add(BindUtils.RelayBind(this, SizeTypeProperty, newPagination, AbstractPagination.SizeTypeProperty));
            _bottomPaginationDisposables.Add(BindUtils.RelayBind(this, IsMotionEnabledProperty, newPagination, AbstractPagination.IsMotionEnabledProperty));
            _bottomPaginationDisposables.Add(BindUtils.RelayBind(this, PaginationVisibilityProperty, newPagination, AbstractPagination.IsMotionEnabledProperty));
            _bottomPagination = newPagination;
            HandlePaginationVisibility();
        }
    }

    private void HandlePaginationVisibility()
    {
        if (PaginationVisibility == ListPaginationVisibility.None)
        {
            _topPagination?.IsVisible    = false;
            _bottomPagination?.IsVisible = false;
        }
        else if (PaginationVisibility == ListPaginationVisibility.Both)
        {
            _topPagination?.IsVisible    = true;
            _bottomPagination?.IsVisible = true;
        }
        else if (PaginationVisibility == ListPaginationVisibility.Top)
        {
            _topPagination?.IsVisible    = true;
            _bottomPagination?.IsVisible = false;
        }
        else
        {
            _topPagination?.IsVisible    = false;
            _bottomPagination?.IsVisible = true;
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

        if (ListView != null)
        {
            ListView.SyncSelectionRequest -= HandleSyncSelectionRequest;
        }

        ListView = e.NameScope.Find<GroupableListView>(ListThemeConstants.ListViewPart);
        if (ListView != null)
        {
            ListView.OwningList           =  this;
            ListView.SyncSelectionRequest += HandleSyncSelectionRequest;
            SyncSelectionMode();
            SyncSelectionToListView();
        }
        UpdatePseudoClasses();
        ConfigureEmptyIndicator();
    }
    
    private void HandlePageChangeRequest(object? sender, PageChangedEventArgs args)
    {
        if (CollectionView is ListCollectionView collectionView)
        {
            collectionView.MoveToPage(args.PageIndex - 1);
        }
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
                    oldDataGridCollectionView.PageChanged  -= HandlePageChanged;
                }
            }
            if (newCollectionView != null)
            {
                newCollectionView.CollectionChanged += HandleDataCollectionViewChanged;
                if (newCollectionView is ListCollectionView newDataGridCollectionView)
                {
                    newDataGridCollectionView.PageChanging += HandlePageChanging;
                    newDataGridCollectionView.PageChanged  += HandlePageChanged;
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
            NotifyItemsSourceChangedForSelection();
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
            ItemCount         = view.TotalItemCount;
        }
    }
    
    private void HandlePageChanging(object? sender, PageChangingEventArgs args)
    {
        if (ListView != null)
        {
            if (ListView.Selection.Count > 0)
            {
                ListView.Selection.Clear();
            }
        }
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

    private void HandlePageChanged(object? sender, EventArgs args)
    {
        SyncSelectionToListView();
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
                _topPagination.Total       = collectionView.TotalItemCount;
                _topPagination.PageSize    = PageSize;
                _topPagination.CurrentPage = Pagination.DefaultCurrentPage;
            }
            if (_bottomPagination != null)
            {
                _bottomPagination.Total       = collectionView.TotalItemCount;
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
                if (GroupPropertySelector != null)
                {
                    collectionView.GroupDescriptions.Add(new ListGroupDescription(GroupPropertySelector));
                }
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
                if (GroupPropertySelector != null)
                {
                    collectionView.GroupDescriptions.Add(new ListGroupDescription(GroupPropertySelector));
                }
            }
        }
    }

    protected internal virtual Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        return new ListItem();
    }

    protected internal virtual bool? NeedsContainerOverride(object? item, int index, out object? recycleKey)
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

    protected internal virtual void AboutToPrepareContainerForItemOverride(Control container, object? item, int index)
    {
        // Ensure that the selection model is created at this point so that accessing it in 
        // ContainerForItemPreparedOverride doesn't cause it to be initialized (which can
        // make containers become deselected when they're synced with the empty selection
        // mode).
        GetOrCreateSelectionModel();
    }
    
    protected internal virtual void PrepareContainerForItemOverride(Control container, object? item, int index)
    {}

    protected internal virtual void PrepareListBoxItem(ListItem listItem, object? item, int index,
                                                       CompositeDisposable disposables)
    {
        var isGroupEnabled = false;
        if (item is IGroupListItemData groupListItemData)
        {
            listItem.IsGroupItem = groupListItemData.IsGroupItem;
            isGroupEnabled       = listItem.IsGroupItem;
        }
        if (GroupItemTemplate != null && isGroupEnabled)
        {
            disposables.Add(BindUtils.RelayBind(this, GroupItemTemplateProperty, listItem, ListItem.ContentTemplateProperty));
        }
    }

    protected internal virtual bool UpdateSelectionFromPointerEvent(ListItem listItem, PointerEventArgs e)
    {
        return false;
    }

    protected internal virtual bool UpdateSelection(ListItem listItem,
                                                    bool select = true,
                                                    bool rangeModifier = false,
                                                    bool toggleModifier = false,
                                                    bool rightButton = false,
                                                    bool fromFocus = false)
    {
        if (ListView != null)
        {
            return ListView.UpdateSelection(listItem, select, rangeModifier, toggleModifier, rightButton, fromFocus);
        }
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
    
    private void SyncSelectionMode()
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

    private void HandleItemCountChanged()
    {
        ItemCountChanged?.Invoke(this, new ItemCountChangedEventArgs(ItemCount));
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        var inputManager = AvaloniaLocator.Current.GetService<IInputManager>()!;
        _inputManagerDisposable = inputManager.Process.Subscribe(HandleKeyDown);
    }

    protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromLogicalTree(e);
        _inputManagerDisposable?.Dispose();
    }

    private void HandleKeyDown(RawInputEventArgs e)
    {
        if (e is RawKeyEventArgs keyEventArgs)
        {
            if (keyEventArgs.Type == RawKeyEventType.KeyDown)
            {
                _rawKeyEventArgs = keyEventArgs;
            }
            else
            {
                _rawKeyEventArgs = null;
            }
        }
    }
    
    #region ItemsControl 的方法转发

    public Control? ContainerFromIndex(int index) => ListView?.ContainerFromIndex(index);
    public Control? ContainerFromItem(object item) => ListView?.ContainerFromItem(item);
    public int IndexFromContainer(Control container) => ListView?.IndexFromContainer(container) ?? -1;
    public object? ItemFromContainer(Control container) => ListView?.ItemFromContainer(container);
    public IEnumerable<Control> GetRealizedContainers() => ListView?.GetRealizedContainers() ?? Array.Empty<Control>();
    public void ScrollIntoView(int index) => ListView?.ScrollIntoView(index);
    public void ScrollIntoView(object item) => ListView?.ScrollIntoView(item);
    #endregion
    
    #region 虚拟化上下文管理
    protected internal virtual void NotifyRestoreDefaultContext(ListItem item, IListItemData itemData)
    {
        if (itemData is IGroupListItemData groupListItemData)
        {
            item.SetCurrentValue(ListItem.IsGroupItemProperty, groupListItemData.IsGroupItem);
        }
    }

    protected internal virtual void NotifyClearContainerForVirtualizingContext(ListItem item)
    {
        item.ClearValue(ListItem.IsGroupItemProperty);
    }
    
    protected internal virtual void NotifySaveVirtualizingContext(ListItem item, IDictionary<object, object?> context)
    {
        context.Add(ListItem.IsGroupItemProperty, item.IsGroupItem);
    }

    protected internal virtual void NotifyRestoreVirtualizingContext(ListItem item, IDictionary<object, object?> context)
    {
        if (context.TryGetValue(ListItem.IsGroupItemProperty, out var value))
        {
            if (value is bool isGroupItem)
            {
                item.SetCurrentValue(ListItem.IsGroupItemProperty, isGroupItem);
            }
        }
    }
    #endregion
}