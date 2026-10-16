using System.Collections;
using System.Collections.Specialized;
using AtomUI.Controls;
using AtomUI.Controls.Data;
using AtomUI.Controls.Utils;
using AtomUI.Generated.AtomUIDesktopControls;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Metadata;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

public partial class ListView : ItemsControl, ICustomizableSizeTypeAware, IMotionAwareControl
{
    #region 公共属性定义
    
    public static readonly StyledProperty<bool> IsSelectableProperty =
        AvaloniaProperty.Register<ListView, bool>(nameof(IsSelectable), true);
    
    public static readonly StyledProperty<CustomizableSizeType> SizeTypeProperty =
        CustomizableSizeTypeControlProperty.SizeTypeProperty.AddOwner<ListView>();
    
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
    
    public static readonly StyledProperty<ClickMode> ItemClickModeProperty =
        AvaloniaProperty.Register<ListView, ClickMode>(nameof(ItemClickMode));
    
    public bool IsSelectable
    {
        get => GetValue(IsSelectableProperty);
        set => SetValue(IsSelectableProperty, value);
    }
    
    public CustomizableSizeType SizeType
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

    public ClickMode ItemClickMode
    {
        get => GetValue(ItemClickModeProperty);
        set => SetValue(ItemClickModeProperty, value);
    }
    #endregion
    
    #region 公共事件定义

    public event EventHandler<ListViewItemClickedEventArgs>? ItemClicked;
    public event EventHandler<ItemCountChangedEventArgs>? ItemCountChanged;
    public event EventHandler? FilterContextChanged;

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

    internal static readonly DirectProperty<ListView, bool> IsDefaultEmptyIndicatorVisibleProperty =
        AvaloniaProperty.RegisterDirect<ListView, bool>(
            nameof(IsDefaultEmptyIndicatorVisible),
            o => o.IsDefaultEmptyIndicatorVisible,
            (o, v) => o.IsDefaultEmptyIndicatorVisible = v);
    
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

    private bool _isDefaultEmptyIndicatorVisible;
    internal bool IsDefaultEmptyIndicatorVisible
    {
        get => _isDefaultEmptyIndicatorVisible;
        set => SetAndRaise(IsDefaultEmptyIndicatorVisibleProperty, ref _isDefaultEmptyIndicatorVisible, value);
    }

    #endregion
    
    private bool _areHandlersSuspended;
    private static readonly object GroupHeaderRecycleKey = new();
    private static readonly FuncTemplate<Panel?> DefaultPanel =
        new(() => new VirtualizingStackPanel());

    private IListCollectionView? _collectionView;
    /// <summary>Indicates whether _collectionView was created by ListView (and should be disposed by it).</summary>
    private bool _ownsCollectionView;
    private IListCollectionView? _defaultFilterOwner;
    private Func<object, bool>? _defaultFilterCallback;
    
    static ListView()
    {
        ItemsPanelProperty.OverrideDefaultValue<ListView>(DefaultPanel);
        IsSelectedChangedEvent.AddClassHandler<ListView>((list, e) => list.ContainerSelectionChanged(e));
        IsSelectableProperty.Changed.AddClassHandler<ListView>((list, args) => list.HandleIsSelectableChanged(args));
        TotalItemCountProperty.Changed.AddClassHandler<ListView>((list, args) => list.HandleItemCountChanged());
        ItemsSourceProperty.Changed.AddClassHandler<ListView>((list, e) => list.HandleItemsSourcePropertyChanged(e));
    }

    internal static void RegisterItemClickedEvent(RoutedEvent<RoutedEventArgs> clickedEvent)
    {
        clickedEvent.AddClassHandler<ListView>((list, args) => list.HandleListViewItemClicked(args));
    }
    
    public ListView()
    {
        // Selecting 相关设置，只能通过反射设置目前
        ((ItemCollection)ItemsView).AddSourceChangedEvent(OnItemsViewSourceChanged);
        _selection.Changed += (_, change) => HandleSelectionChange(change);
        var items = Items;
        items.CollectionChanged += HandleItemsViewCollectionChanged;
        Items.CollectionChanged += HandleItemCollectionChanged;
    }

    private void OnItemsViewSourceChanged(object? sender, EventArgs e)
    {
        AttachSelectionView();
    }
    
    private void HandleItemsSourcePropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        if (!_areHandlersSuspended)
        {
            var                  oldCollectionView = change.OldValue as ListCollectionView;
            var                  newItemsSource    = (IEnumerable?)change.NewValue;
            IListCollectionView? newCollectionView = null;
            bool                 ownsNew           = false;
            if (newItemsSource is IListCollectionView)
            {
                newCollectionView  = (IListCollectionView)newItemsSource;
            }
            else
            {
                if (newItemsSource != null)
                {
                    newCollectionView = new ListCollectionView(newItemsSource);
                    ownsNew           = true;
                }
            }
            if (oldCollectionView != null)
            {
                oldCollectionView.PropertyChanged   -= HandleCollectionPropertyChanged;
                oldCollectionView.CollectionChanged -= HandleCollectionViewChanged;
                oldCollectionView.PageChanging      -= HandlePageChanging;
                oldCollectionView.PageChanged       -= HandlePageChanged;
                ClearDefaultFilterCallback(oldCollectionView);
                // Dispose only if we created this view (not user-provided)
                if (_ownsCollectionView)
                {
                    oldCollectionView.Dispose();
                }
            }
            if (newCollectionView != null)
            {
                newCollectionView.PropertyChanged   += HandleCollectionPropertyChanged;
                newCollectionView.CollectionChanged += HandleCollectionViewChanged;
                newCollectionView.PageChanging      += HandlePageChanging;
                newCollectionView.PageChanged       += HandlePageChanged;
                IsEmptyDataSource                   =  newCollectionView.IsEmpty;
                TotalItemCount                      =  newCollectionView.TotalItemCount;
            }
            else
            {
                IsEmptyDataSource = true;
            }

            _collectionView     = newCollectionView;
            _ownsCollectionView = ownsNew;
            SetValueNoCallback(ItemsSourceProperty, newCollectionView);
            ConfigureFilterDescription();
            ConfigureSortDescriptions();
            ConfigureGroupInfo();
            ReConfigurePagination();
            AttachSelectionView();
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
        if (sender is IListCollectionView view)
        {
            IsEmptyDataSource = view.IsEmpty;
            TotalItemCount    = view.TotalItemCount;
            SyncPaginationState();
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
        AttachSelectionView();
    }
    
    private object? GroupSelector(object? item)
    {
        if (item is IGroupHeader groupHeader)
        {
            return groupHeader.Group;
        }

        return null;
    }

    private void HandleItemCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        _virtualRestoreContext.Clear();
        ConfigureEmptyIndicator();
        RefreshContainerSplitLines();
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
            change.Property == IsEmptyDataSourceProperty ||
            change.Property == EmptyIndicatorProperty ||
            change.Property == EmptyIndicatorTemplateProperty)
        {
            ConfigureEmptyIndicator();
        }
        else if (change.Property == BorderThicknessProperty ||
                 change.Property == IsBorderlessProperty)
        {
            ConfigureEffectiveBorderThickness();
            RefreshContainerSplitLines();
        }
        else if (change.Property == BottomPaginationProperty)
        {
            RefreshContainerSplitLines();
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
                AttachSelectionView();
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
            Selection.Clear();
        }
    }
    
    private void ConfigureIsFiltering()
    {
        IsFiltering = Filter != null && FilterValue != null;
    }
    
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        ListViewItem listItem;
        if (IsGroupDataItem(item))
        {
            listItem = new GroupHeaderItem();
            listItem.Classes.Add(ListViewSemanticParts.GroupHeaderClass);
        }
        else
        {
            listItem = new ListViewItem();
            listItem.Classes.Add(ListViewSemanticParts.ItemClass);
        }

        NotifyContainerForItemCreated(listItem, item);
        return listItem;
    }

    private static bool IsGroupDataItem(object? item)
    {
        return item is IGroupListItemData { IsGroupItem: true };
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
        if (IsGroupDataItem(item))
        {
            // Group headers own a dedicated recycle key so they can never be
            // recycled as regular item containers (and vice versa).
            recycleKey = GroupHeaderRecycleKey;
            return true;
        }

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
            listItem.IsSplitLineVisible = ShouldShowSplitLine(index == ItemCount - 1);
            if (item is IGroupListItemData groupListItemData)
            {
                listItem.IsGroupItem = groupListItemData.IsGroupItem;
                var isGroupEnabled = listItem.IsGroupItem;
                if (GroupItemTemplate != null && isGroupEnabled)
                {
                    listItem[!ListViewItem.ContentTemplateProperty] = this[!GroupItemTemplateProperty];
                }
                else if (ItemTemplate != null)
                {
                    listItem[!ListViewItem.ContentTemplateProperty] = this[!ItemTemplateProperty];
                }
                else
                {
                    listItem.ClearValue(ListViewItem.ContentTemplateProperty);
                }
            }
            else
            {
                if (ItemTemplate != null)
                {
                    listItem[!ListViewItem.ContentTemplateProperty] = this[!ItemTemplateProperty];
                }
                else
                {
                    listItem.ClearValue(ListViewItem.ContentTemplateProperty);
                }
            }
            
            listItem[!ListViewItem.SizeTypeProperty]                = this[!SizeTypeProperty];
            listItem[!ListViewItem.IsMotionEnabledProperty]         = this[!IsMotionEnabledProperty];
            listItem[!ListViewItem.SelectedIndicatorProperty]       = this[!SelectedIndicatorProperty];
            listItem[!ListViewItem.ItemHoverBgProperty]             = this[!ItemHoverBgProperty];
            listItem[!ListViewItem.ItemSelectedBgProperty]          = this[!ItemSelectedBgProperty];
            listItem[!ListViewItem.IsShowSelectedIndicatorProperty] = this[!IsShowSelectedIndicatorProperty];
            listItem[!ListViewItem.ItemClickModeProperty]           = this[!ItemClickModeProperty];
            
            PrepareListViewItem(listItem, item, index);

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
    
    protected virtual void PrepareListViewItem(ListViewItem listItem, object? item, int index)
    {
    }
    
    protected virtual void ConfigureEmptyIndicator()
    {
        IsEffectiveEmptyVisible        = IsShowEmptyIndicator && TotalItemCount == 0;
        IsDefaultEmptyIndicatorVisible = IsEffectiveEmptyVisible && EmptyIndicator is null && EmptyIndicatorTemplate is null;
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

    private bool ShouldShowSplitLine(bool isLastItem)
    {
        if (IsBorderless)
        {
            return true;
        }

        if (!isLastItem)
        {
            return true;
        }

        return BottomPagination is not null;
    }

    private bool IsLastItem(Control container)
    {
        return IndexFromContainer(container) == ItemCount - 1;
    }

    private void RefreshContainerSplitLines()
    {
        var presenter = this.GetVisualDescendants().OfType<ItemsPresenter>().FirstOrDefault();
        if (presenter?.Panel is null)
        {
            return;
        }

        // Realized containers only; unrealized ones receive the correct value when
        // they are prepared. A container's item is its Content for generated
        // containers and the container itself for directly supplied containers.
        foreach (var child in presenter.Panel.Children)
        {
            if (child is not ListViewItem listItem)
            {
                continue;
            }

            listItem.IsSplitLineVisible = ShouldShowSplitLine(IsLastItem(listItem));
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
                     hotkeys is not null && MatchesSelectAllHotkey(hotkeys.SelectAll, e))
            {
                Selection.SelectAll();
                e.Handled = true;
            }
            else if (e.Key == Key.Space || e.Key == Key.Enter)
            {
                UpdateSelectionFromEventSource(
                    e.Source,
                    true,
                    (e.KeyModifiers & KeyModifiers.Shift) == KeyModifiers.Shift,
                    ctrl);
            }

        }

        base.OnKeyDown(e);
    }

    private static bool MatchesSelectAllHotkey(IEnumerable<KeyGesture> hotkeys, KeyEventArgs args)
    {
        foreach (var hotkey in hotkeys)
        {
            if (hotkey.Matches(args))
            {
                return true;
            }
        }

        return false;
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
                EnsureDefaultFilterCallback(_collectionView);
            }
            else
            {
                ClearDefaultFilterCallback(_collectionView);
            }
            IsFiltering = _collectionView.FilterDescriptions.Count > 0;
        }

        NotifyFilterContextChanged();
        FilterContextChanged?.Invoke(this, EventArgs.Empty);
    }

    protected virtual void NotifyFilterContextChanged()
    {
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

    private void EnsureDefaultFilterCallback(IListCollectionView collectionView)
    {
        if (ReferenceEquals(_defaultFilterOwner, collectionView) &&
            ReferenceEquals(collectionView.Filter, _defaultFilterCallback))
        {
            return;
        }

        if (collectionView.Filter == null)
        {
            _defaultFilterOwner    = collectionView;
            _defaultFilterCallback = new ListDefaultFilter(collectionView);
            collectionView.Filter  = _defaultFilterCallback;
        }
    }

    private void ClearDefaultFilterCallback(IListCollectionView collectionView)
    {
        if (ReferenceEquals(_defaultFilterOwner, collectionView) &&
            ReferenceEquals(collectionView.Filter, _defaultFilterCallback))
        {
            collectionView.Filter = null;
        }

        if (ReferenceEquals(_defaultFilterOwner, collectionView))
        {
            _defaultFilterOwner    = null;
            _defaultFilterCallback = null;
        }
    }
}
