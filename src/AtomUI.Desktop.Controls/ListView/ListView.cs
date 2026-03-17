using System.Collections;
using System.Collections.Specialized;
using System.Reactive.Disposables;
using AtomUI.Controls;
using AtomUI.Controls.Data;
using AtomUI.Controls.Utils;
using AtomUI.Data;
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

namespace AtomUI.Desktop.Controls;

public partial class ListView : ItemsControl,
                                ISizeTypeAware,
                                IMotionAwareControl,
                                IControlSharedTokenResourcesHost
{
    #region 公共属性定义
    
    public static readonly StyledProperty<bool> IsSelectableProperty =
        AvaloniaProperty.Register<ListView, bool>(nameof(IsSelectable), true);
    
    public static readonly StyledProperty<SizeType> SizeTypeProperty =
        SizeTypeControlProperty.SizeTypeProperty.AddOwner<ListView>();
    
    public static readonly StyledProperty<bool> IsBorderlessProperty =
        AvaloniaProperty.Register<ListView, bool>(nameof(IsBorderless), false);
        
    public static readonly StyledProperty<IBrush?> ItemHoverBgProperty =
        AvaloniaProperty.Register<ListView, IBrush?>(nameof(ItemHoverBg));
    
    public static readonly StyledProperty<IBrush?> ItemSelectedBgProperty =
        AvaloniaProperty.Register<ListView, IBrush?>(nameof(ItemSelectedBg));
    
    public static readonly StyledProperty<bool> IsShowSelectedIndicatorProperty =
        AvaloniaProperty.Register<ListView, bool>(nameof(IsShowSelectedIndicator), false);
    
    public static readonly StyledProperty<IconTemplate?> SelectedIndicatorProperty =
        AvaloniaProperty.Register<ListView, IconTemplate?>(nameof(SelectedIndicator));
    
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<ListView>();
    
    public static readonly StyledProperty<Thickness> EmptyIndicatorPaddingProperty =
        AvaloniaProperty.Register<ListView, Thickness>(nameof(EmptyIndicatorPadding));
    
    public static readonly StyledProperty<object?> EmptyIndicatorProperty =
        AvaloniaProperty.Register<ListView, object?>(nameof(EmptyIndicator));
    
    public static readonly StyledProperty<IDataTemplate?> EmptyIndicatorTemplateProperty =
        AvaloniaProperty.Register<ListView, IDataTemplate?>(nameof(EmptyIndicatorTemplate));
    
    public static readonly StyledProperty<bool> IsShowEmptyIndicatorProperty =
        AvaloniaProperty.Register<ListView, bool>(nameof(IsShowEmptyIndicator), true);
    
    public static readonly StyledProperty<bool> IsGroupEnabledProperty =
        AvaloniaProperty.Register<ListView, bool>(nameof(IsGroupEnabled), false);
    
    public static readonly StyledProperty<DefaultFilterValueSelector?> GroupPropertySelectorProperty =
        AvaloniaProperty.Register<ListView, DefaultFilterValueSelector?>(
            nameof(GroupPropertySelector));
    
    public static readonly StyledProperty<IList<IListSortDescription>?> SortDescriptionsProperty =
        AvaloniaProperty.Register<ListView, IList<IListSortDescription>?>(nameof(SortDescriptions));
    
    public static readonly StyledProperty<IValueFilter?> FilterProperty =
        AvaloniaProperty.Register<ListView, IValueFilter?>(nameof(Filter));
    
    public static readonly StyledProperty<object?> FilterValueProperty =
        AvaloniaProperty.Register<ListView, object?>(nameof(FilterValue));
    
    public static readonly StyledProperty<DefaultFilterValueSelector?> FilterValueSelectorProperty =
        AvaloniaProperty.Register<ListView, DefaultFilterValueSelector?>(
            nameof(FilterValueSelector));
    
    public static readonly DirectProperty<ListView, bool> IsFilteringProperty =
        AvaloniaProperty.RegisterDirect<ListView, bool>(nameof(IsFiltering),
            o => o.IsFiltering);
    
    public static readonly DirectProperty<ListView, int> TotalItemCountProperty =
        AvaloniaProperty.RegisterDirect<ListView, int>(nameof(TotalItemCount), o => o.TotalItemCount);
    
    public static readonly StyledProperty<IDataTemplate?> GroupItemTemplateProperty =
        AvaloniaProperty.Register<ListView, IDataTemplate?>(nameof(GroupItemTemplate));
    
    public static readonly StyledProperty<bool> IsOperatingProperty =
        AvaloniaProperty.Register<ListView, bool>(nameof(IsOperating));
    
    public static readonly StyledProperty<string?> OperatingMsgProperty =
        AvaloniaProperty.Register<ListView, string?>(nameof(OperatingMsg));
    
    public static readonly StyledProperty<object?> CustomOperatingIndicatorProperty =
        AvaloniaProperty.Register<ListView, object?>(nameof(CustomOperatingIndicator));

    public static readonly StyledProperty<IDataTemplate?> CustomOperatingIndicatorTemplateProperty =
        AvaloniaProperty.Register<ListView, IDataTemplate?>(nameof(CustomOperatingIndicatorTemplate));
    
    public static readonly StyledProperty<AbstractPagination?> TopPaginationProperty =
        AvaloniaProperty.Register<ListView, AbstractPagination?>(nameof(TopPagination));
    
    public static readonly StyledProperty<AbstractPagination?> BottomPaginationProperty =
        AvaloniaProperty.Register<ListView, AbstractPagination?>(nameof(BottomPagination));
    
    public static readonly StyledProperty<bool> IsHideOnSinglePageProperty =
        AbstractPagination.IsHideOnSinglePageProperty.AddOwner<ListView>();
    
    public static readonly StyledProperty<ListPaginationVisibility> PaginationVisibilityProperty =
        AvaloniaProperty.Register<ListView, ListPaginationVisibility>(nameof(PaginationVisibility), ListPaginationVisibility.Bottom);
    
    public static readonly StyledProperty<PaginationAlign> TopPaginationAlignProperty =
        AvaloniaProperty.Register<ListView, PaginationAlign>(nameof(TopPaginationAlign), PaginationAlign.End);
    
    public static readonly StyledProperty<PaginationAlign> BottomPaginationAlignProperty =
        AvaloniaProperty.Register<ListView, PaginationAlign>(nameof(BottomPaginationAlign), PaginationAlign.End);
    
    public bool IsSelectable
    {
        get => GetValue(IsSelectableProperty);
        set => SetValue(IsSelectableProperty, value);
    }
    
    public SizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
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
    
    public bool IsShowSelectedIndicator
    {
        get => GetValue(IsShowSelectedIndicatorProperty);
        set => SetValue(IsShowSelectedIndicatorProperty, value);
    }
    
    public IconTemplate? SelectedIndicator
    {
        get => GetValue(SelectedIndicatorProperty);
        set => SetValue(SelectedIndicatorProperty, value);
    }
    
    public Thickness EmptyIndicatorPadding
    {
        get => GetValue(EmptyIndicatorPaddingProperty);
        set => SetValue(EmptyIndicatorPaddingProperty, value);
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
    
    public DefaultFilterValueSelector? GroupPropertySelector
    {
        get => GetValue(GroupPropertySelectorProperty);
        set => SetValue(GroupPropertySelectorProperty, value);
    }
    
    public IList<IListSortDescription>? SortDescriptions
    {
        get => GetValue(SortDescriptionsProperty);
        set => SetValue(SortDescriptionsProperty, value);
    }
    
    public IValueFilter? Filter
    {
        get => GetValue(FilterProperty);
        set => SetValue(FilterProperty, value);
    }

    public object? FilterValue
    {
        get => GetValue(FilterValueProperty);
        set => SetValue(FilterValueProperty, value);
    }
    
    public DefaultFilterValueSelector? FilterValueSelector
    {
        get => GetValue(FilterValueSelectorProperty);
        set => SetValue(FilterValueSelectorProperty, value);
    }
    
    private bool _isFiltering;
    
    public bool IsFiltering
    {
        get => _isFiltering;
        private set => SetAndRaise(IsFilteringProperty, ref _isFiltering, value);
    }
    
    private int _totalItemCount;
    public int TotalItemCount
    {
        get => _totalItemCount;
        private set
        {
            if (SetAndRaise(TotalItemCountProperty, ref _totalItemCount, value))
            {
                UpdatePseudoClasses();
            }
        }
    }
    
    [InheritDataTypeFromItems(nameof(ItemsSource))]
    public IDataTemplate? GroupItemTemplate
    {
        get => GetValue(GroupItemTemplateProperty);
        set => SetValue(GroupItemTemplateProperty, value);
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
    
    public bool IsHideOnSinglePage
    {
        get => GetValue(IsHideOnSinglePageProperty);
        set => SetValue(IsHideOnSinglePageProperty, value);
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

    #endregion
    
    #region 公共事件定义

    public event EventHandler<ListViewItemClickedEventArgs>? ItemClicked;
    public event EventHandler<ItemCountChangedEventArgs>? ItemCountChanged;

    #endregion
    
    #region 内部属性定义
    internal static readonly DirectProperty<ListView, bool> IsEmptyDataSourceProperty =
        AvaloniaProperty.RegisterDirect<ListView, bool>(
            nameof(IsEmptyDataSource),
            o => o.IsEmptyDataSource,
            (o, v) => o.IsEmptyDataSource = v);

    internal static readonly DirectProperty<ListView, Thickness> EffectiveBorderThicknessProperty =
        AvaloniaProperty.RegisterDirect<ListView, Thickness>(nameof(EffectiveBorderThickness),
            o => o.EffectiveBorderThickness,
            (o, v) => o.EffectiveBorderThickness = v);
    
    internal static readonly DirectProperty<ListView, bool> IsEffectiveEmptyVisibleProperty =
        AvaloniaProperty.RegisterDirect<ListView, bool>(
            nameof(IsEffectiveEmptyVisible),
            o => o.IsEffectiveEmptyVisible,
            (o, v) => o.IsEffectiveEmptyVisible = v);
    
    private bool _isEmptyDataSource = true;
    internal bool IsEmptyDataSource
    {
        get => _isEmptyDataSource;
        set => SetAndRaise(IsEmptyDataSourceProperty, ref _isEmptyDataSource, value);
    }
    
    private Thickness _effectiveBorderThickness;

    internal Thickness EffectiveBorderThickness
    {
        get => _effectiveBorderThickness;
        set => SetAndRaise(EffectiveBorderThicknessProperty, ref _effectiveBorderThickness, value);
    }
    
    private bool _isEffectiveEmptyVisible = false;
    internal bool IsEffectiveEmptyVisible
    {
        get => _isEffectiveEmptyVisible;
        set => SetAndRaise(IsEffectiveEmptyVisibleProperty, ref _isEffectiveEmptyVisible, value);
    }
    
    Control IControlSharedTokenResourcesHost.HostControl => this;
    string IControlSharedTokenResourcesHost.TokenId => ListViewToken.ID;

    #endregion
    
    private protected readonly Dictionary<object, CompositeDisposable> _itemsBindingDisposables = new();
    private bool _areHandlersSuspended;
    private static readonly FuncTemplate<Panel?> DefaultPanel =
        new(() => new VirtualizingStackPanel());

    private IListCollectionView? _collectionView;
    
    static ListView()
    {
        ItemsPanelProperty.OverrideDefaultValue<ListView>(DefaultPanel);
        IsSelectedChangedEvent.AddClassHandler<ListView>((list, e) => list.ContainerSelectionChanged(e));
        ListViewItem.ClickedEvent.AddClassHandler<ListView>((list, args) => list.HandleListViewItemClicked(args));
        IsSelectableProperty.Changed.AddClassHandler<ListView>((list, args) => list.HandleIsSelectableChanged(args));
        TotalItemCountProperty.Changed.AddClassHandler<ListView>((list, args) => list.HandleItemCountChanged());
        ItemsSourceProperty.Changed.AddClassHandler<ListView>((list, e) => list.HandleItemsSourcePropertyChanged(e));
    }
    
    public ListView()
    {
        this.RegisterResources();
        // Selecting 相关设置，只能通过反射设置目前
        ((ItemCollection)ItemsView).AddSourceChangedEvent(OnItemsViewSourceChanged);
        var items = this.GetItems();
        items.CollectionChanged += HandleItemsViewCollectionChanged;
        
        LogicalChildren.CollectionChanged += HandleChildrenChanged;
        Items.CollectionChanged           += HandleItemCollectionChanged;
    }
    
    private void HandleItemsSourcePropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        if (!_areHandlersSuspended)
        {
            var                  oldCollectionView = change.OldValue as ListCollectionView;
            var                  newItemsSource    = (IEnumerable?)change.NewValue;
            IListCollectionView? newCollectionView = null;
            if (newItemsSource is IListCollectionView)
            {
                newCollectionView  = (IListCollectionView)newItemsSource;
            }
            else
            {
                if (newItemsSource != null)
                {
                    newCollectionView = new ListCollectionView(newItemsSource);
                }
            }
            if (oldCollectionView != null)
            {
                oldCollectionView.PropertyChanged   -= HandleCollectionPropertyChanged;
                oldCollectionView.CollectionChanged -= HandleCollectionViewChanged;
                oldCollectionView.PageChanging      -= HandlePageChanging;
                oldCollectionView.PageChanged       -= HandlePageChanged;
            }
            if (newCollectionView != null)
            {
                newCollectionView.PropertyChanged   += HandleCollectionPropertyChanged;
                newCollectionView.CollectionChanged += HandleCollectionViewChanged;
                newCollectionView.PageChanging      += HandlePageChanging;
                newCollectionView.PageChanged       += HandlePageChanged;
                IsEmptyDataSource                   =  newCollectionView.IsEmpty;
                TotalItemCount                      =  newCollectionView.TotalItemCount;
       
                newCollectionView.Filter ??= new ListDefaultFilter(newCollectionView);
            }
            else
            {
                IsEmptyDataSource = true;
            }

            _collectionView = newCollectionView;
            SetValueNoCallback(ItemsSourceProperty, newCollectionView);
            ConfigureFilterDescription();
            ConfigureSortDescriptions();
            ConfigureGroupInfo();
            ReConfigurePagination();
            InvalidateMeasure();
            UpdatePseudoClasses();
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
    
    private void HandleCollectionViewChanged(object? sender, NotifyCollectionChangedEventArgs args)
    {
        if (sender is ListCollectionView view)
        {
            IsEmptyDataSource = view.IsEmpty;
            TotalItemCount    = view.TotalItemCount;
        }
    }
    
    private void ConfigureGroupInfo()
    {
        if (ItemsSource is IListCollectionView collectionView)
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
    
    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(ListPseudoClass.Empty, TotalItemCount == 0);
        PseudoClasses.Set(ListPseudoClass.SingleItem, TotalItemCount == 1);
    }
    
    protected override void OnInitialized()
    {
        base.OnInitialized();
        if (GroupPropertySelector == null)
        {
            SetCurrentValue(GroupPropertySelectorProperty, GroupSelector);
        }
        ConfigureEmptyIndicator();
        ConfigureIsFiltering();
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

    private void HandleChildrenChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems != null)
        {
            if (e.Action == NotifyCollectionChangedAction.Remove && e.OldItems.Count > 0)
            {
                foreach (var item in e.OldItems)
                {
                    DisposableListItem(item);
                }
            }
        }
    }
    
    private void HandleItemCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        _virtualRestoreContext.Clear();
        ConfigureEmptyIndicator();
        foreach (var item in Items)
        {
            if (item is not IListItemData)
            {
                throw new Exception("Unexpected item type, must implement IListItemData.");
            }
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ItemsSourceProperty ||
            change.Property == IsFilteringProperty ||
            change.Property == IsShowEmptyIndicatorProperty ||
            change.Property == IsEmptyDataSourceProperty)
        {
            ConfigureEmptyIndicator();
        }
        else if (change.Property == BorderThicknessProperty ||
                 change.Property == IsBorderlessProperty)
        {
            ConfigureEffectiveBorderThickness();
        }
        else if (change.Property == FilterValueProperty ||
                 change.Property == FilterProperty ||
                 change.Property == FilterValueSelectorProperty)
        {
            ConfigureIsFiltering();
            ConfigureFilterDescription();
        }
        else if (change.Property == SortDescriptionsProperty)
        {
            ConfigureSortDescriptions();
        }
        else if (change.Property == IsGroupEnabledProperty)
        {
            if (_collectionView != null)
            {
                using (_collectionView.DeferRefresh())
                {
                    ConfigureGroupInfo();
                }
            }
        }
        else if (change.Property == GroupPropertySelectorProperty)
        {
            ReConfigureGroupInfo();
        }

        HandlePropertyChangedForSelecting(change);
        HandlePropertyChangedForPagination(change);
    }

    private void HandleIsSelectableChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (e.NewValue is bool && (bool)e.NewValue == false)
        {
            SetCurrentValue(SelectedIndexProperty, -1);
            SetCurrentValue(SelectedItemProperty, null);
            SetCurrentValue(SelectedItemsProperty, null);
        }
    }
    
    private void ConfigureIsFiltering()
    {
        IsFiltering = Filter != null && FilterValue != null;
    }

    private protected void DisposableListItem(object item)
    {
        if (_itemsBindingDisposables.TryGetValue(item, out var disposable))
        {
            disposable.Dispose();
            _itemsBindingDisposables.Remove(item);
        }
    }
    
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        var listItem = new ListViewItem();
        NotifyContainerForItemCreated(listItem, item);
        return listItem;
    }
    
    protected virtual void NotifyContainerForItemCreated(Control container, object? item)
    {
        if (container is ListViewItem listItem && item != null && item is not Visual)
        {
            if (item is IListItemData itemData)
            {
                NotifyRestoreDefaultContext(listItem, itemData);
            }
        }
    }

    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
    {
        return NeedsContainer<ListViewItem>(item, out recycleKey);
    }
    
    protected override void PrepareContainerForItemOverride(Control container, object? item, int index)
    {
        // Ensure that the selection model is created at this point so that accessing it in 
        // ContainerForItemPreparedOverride doesn't cause it to be initialized (which can
        // make containers become deselected when they're synced with the empty selection
        // mode).
        GetOrCreateSelectionModel();
        
        base.PrepareContainerForItemOverride(container, item, index);
        if (container is ListViewItem listItem)
        {
            var disposables = new CompositeDisposable(8);
            
            var isGroupEnabled = false;
            if (item is IGroupListItemData groupListItemData)
            {
                listItem.IsGroupItem = groupListItemData.IsGroupItem;
                isGroupEnabled       = listItem.IsGroupItem;
            }
            if (GroupItemTemplate != null && isGroupEnabled)
            {
                disposables.Add(BindUtils.RelayBind(this, GroupItemTemplateProperty, listItem, ListViewItem.ContentTemplateProperty));
            }
      
            if (ItemTemplate != null)
            {
                disposables.Add(BindUtils.RelayBind(this, ItemTemplateProperty, listItem, ListViewItem.ContentTemplateProperty));
            }
            
            disposables.Add(BindUtils.RelayBind(this, SizeTypeProperty, listItem, ListViewItem.SizeTypeProperty));
            disposables.Add(BindUtils.RelayBind(this, IsMotionEnabledProperty, listItem, ListViewItem.IsMotionEnabledProperty));
            disposables.Add(BindUtils.RelayBind(this, SelectedIndicatorProperty, listItem, ListViewItem.SelectedIndicatorProperty));
            disposables.Add(BindUtils.RelayBind(this, ItemHoverBgProperty, listItem, ListViewItem.ItemHoverBgProperty));
            disposables.Add(BindUtils.RelayBind(this, ItemSelectedBgProperty, listItem, ListViewItem.ItemSelectedBgProperty));
            disposables.Add(BindUtils.RelayBind(this, IsShowSelectedIndicatorProperty, listItem, ListViewItem.IsShowSelectedIndicatorProperty));
           
            PrepareListViewItem(listItem, item, index, disposables);

            var originMotionEnabled = false;
            try
            {
                originMotionEnabled = listItem.IsMotionEnabled;
                listItem.SetCurrentValue(IsMotionEnabledProperty, false);
                if (item is IListItemData itemData)
                {
                    NotifyRestoreDefaultContext(listItem, itemData);
                }

                if (this is IListVirtualizingContextAware listVirtualizingContextAwareControl &&
                    listItem is IListItemVirtualizingContextAware virtualListItem)
                {
                    virtualListItem.VirtualIndex = index;
                    if (_virtualRestoreContext.TryGetValue(index, out var context))
                    {
                        listVirtualizingContextAwareControl.RestoreVirtualizingContext(listItem, context);
                        _virtualRestoreContext.Remove(index);
                    }
                }

                if (_itemsBindingDisposables.TryGetValue(listItem, out var oldDisposables))
                {
                    oldDisposables.Dispose();
                    _itemsBindingDisposables.Remove(listItem);
                }

                _itemsBindingDisposables.Add(listItem, disposables);
            }
            finally
            {
                listItem.SetCurrentValue(IsMotionEnabledProperty, originMotionEnabled);
            }
        }
        else
        {
            throw new ArgumentOutOfRangeException(nameof(container), "The container type is incorrect, it must be type ListViewItem.");
        }
    }
    
    protected virtual void PrepareListViewItem(ListViewItem listItem, object? item, int index, CompositeDisposable disposables)
    {
    }
    
    protected virtual void ConfigureEmptyIndicator()
    {
        SetCurrentValue(IsEffectiveEmptyVisibleProperty, IsShowEmptyIndicator && TotalItemCount == 0);
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
    
    protected internal virtual bool UpdateSelectionFromPointerEvent(Control source, PointerEventArgs e)
    {
        if (IsSelectable)
        {
            // TODO: use TopLevel.PlatformSettings here, but first need to update our tests to use TopLevels. 
            var hotkeys = Application.Current!.PlatformSettings?.HotkeyConfiguration;
            var toggle  = hotkeys is not null && e.KeyModifiers.HasAllFlags(hotkeys.CommandModifiers);

            return UpdateSelectionFromEventSource(
                source,
                true,
                e.KeyModifiers.HasAllFlags(KeyModifiers.Shift),
                toggle,
                e.GetCurrentPoint(source).Properties.IsRightButtonPressed);
        }
        return false;
    }
    
    private void HandleListViewItemClicked(RoutedEventArgs args)
    {
        if (args.Source is ListViewItem item)
        {
            NotifyItemClicked(item);
            ItemClicked?.Invoke(this, new ListViewItemClickedEventArgs(item));
        }
    }
    
    protected internal virtual void NotifyItemClicked(ListViewItem item)
    {
    }
    
    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (IsSelectable)
        {
            var hotkeys = Application.Current!.PlatformSettings?.HotkeyConfiguration;
            var ctrl    = hotkeys is not null && e.KeyModifiers.HasAllFlags(hotkeys.CommandModifiers);

            if (!ctrl &&
                e.Key.ToNavigationDirection() is { } direction && 
                direction.IsDirectional())
            {
                e.Handled |= MoveSelection(
                    direction,
                    WrapSelection,
                    e.KeyModifiers.HasAllFlags(KeyModifiers.Shift));
            }
            else if (SelectionMode.HasAllFlags(SelectionMode.Multiple) &&
                     hotkeys is not null && hotkeys.SelectAll.Any(x => x.Matches(e)))
            {
                Selection.SelectAll();
                e.Handled = true;
            }
            else if (e.Key == Key.Space || e.Key == Key.Enter)
            {
                UpdateSelectionFromEventSource(
                    e.Source,
                    true,
                    e.KeyModifiers.HasFlag(KeyModifiers.Shift),
                    ctrl);
            }

        }
        
        base.OnKeyDown(e);
    }
    
    private void HandleItemCountChanged()
    {
        ItemCountChanged?.Invoke(this, new ItemCountChangedEventArgs(TotalItemCount));
    }
    
    private void ReConfigureGroupInfo()
    {
        if (_collectionView != null)
        {
            _collectionView.GroupDescriptions.Clear();
            if (IsGroupEnabled)
            {
                if (GroupPropertySelector != null)
                {
                    _collectionView.GroupDescriptions.Add(new ListGroupDescription(GroupPropertySelector));
                }
            }
        }
    }
    
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        NotifyApplyTemplateForSelecting();
        
        if (EmptyIndicator == null)
        {
            SetValue(EmptyIndicatorProperty, new Empty()
            {
                SizeType    = SizeType.Small,
                PresetImage = PresetEmptyImage.Simple
            }, BindingPriority.Template);
        }
        
        UpdatePseudoClasses();
        ConfigureEmptyIndicator();
        HandlePaginationVisibility();
    }

    private void ConfigureFilterDescription()
    {
        if (_collectionView != null)
        {
            _collectionView.FilterDescriptions.Clear();
            if (FilterValue != null && Filter != null)
            {
                _collectionView.FilterDescriptions.Add(new ListFilterDescription()
                {
                    FilterPropertySelector = FilterValueSelector ?? (record => (record as IListItemData)?.Content) ,
                    FilterConditions       = [FilterValue],
                    Filter                 = Filter.Filter
                });
            }
        }
    }

    private void ConfigureSortDescriptions()
    {
        if (_collectionView != null)
        {
            _collectionView.SortDescriptions.Clear();
            if (SortDescriptions != null)
            {
                _collectionView.SortDescriptions.AddRange(SortDescriptions);
            }
        }
    }
}