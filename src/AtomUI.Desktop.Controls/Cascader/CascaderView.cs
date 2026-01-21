using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reactive.Disposables;
using AtomUI.Controls;
using AtomUI.Data;
using AtomUI.Desktop.Controls.DataLoad;
using AtomUI.Desktop.Controls.Primitives;
using AtomUI.Theme;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Generators;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Metadata;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

[Flags]
public enum CascaderItemFilterAction
{
    HighlightedMatch = 0x01,
    HighlightedWhole = 0x02,
    BoldedMatch = 0x04,
    ExpandPath = 0x08,
    HideUnMatched = 0x10,
    All = HighlightedMatch | BoldedMatch | ExpandPath | HideUnMatched
}

public enum CascaderViewExpandTrigger
{
    Click,
    Hover
}

public partial class CascaderView : ItemsControl, IMotionAwareControl, IControlSharedTokenResourcesHost
{
    #region 公共属性定义
    public new TreeItemContainerGenerator ItemContainerGenerator =>
        (TreeItemContainerGenerator)base.ItemContainerGenerator;
    
    public static readonly StyledProperty<bool> IsAutoExpandParentProperty =
        AvaloniaProperty.Register<CascaderView, bool>(nameof(IsAutoExpandParent), true);
    
    public static readonly StyledProperty<bool> IsShowIconProperty =
        AvaloniaProperty.Register<CascaderView, bool>(nameof(IsShowIcon));
    
    public static readonly StyledProperty<IconTemplate?> ItemExpandIconProperty =
        AvaloniaProperty.Register<CascaderView, IconTemplate?>(nameof(ItemExpandIcon));
    
    public static readonly StyledProperty<IconTemplate?> ItemLoadingIconProperty =
        AvaloniaProperty.Register<CascaderView, IconTemplate?>(nameof(ItemLoadingIcon));
    
    public static readonly StyledProperty<CascaderViewExpandTrigger> ExpandTriggerProperty =
        AvaloniaProperty.Register<CascaderView, CascaderViewExpandTrigger>(nameof(ExpandTrigger), CascaderViewExpandTrigger.Click);
    
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<CascaderView>();
    
    public static readonly StyledProperty<bool> IsCheckableProperty =
        AvaloniaProperty.Register<CascaderView, bool>(nameof(IsCheckable));
    
    public static readonly DirectProperty<CascaderView, TreeNodePath?> DefaultExpandedPathProperty =
        AvaloniaProperty.RegisterDirect<CascaderView, TreeNodePath?>(
            nameof(DefaultExpandedPath),
            o => o.DefaultExpandedPath,
            (o, v) => o.DefaultExpandedPath = v);
    
    public static readonly DirectProperty<CascaderView, ICascaderItemDataLoader?> ItemDataLoaderProperty =
        AvaloniaProperty.RegisterDirect<CascaderView, ICascaderItemDataLoader?>(
            nameof(ItemDataLoader),
            o => o.ItemDataLoader,
            (o, v) => o.ItemDataLoader = v);
    
    public static readonly DirectProperty<CascaderView, ICascaderItemFilter?> ItemFilterProperty =
        AvaloniaProperty.RegisterDirect<CascaderView, ICascaderItemFilter?>(
            nameof(ItemFilter),
            o => o.ItemFilter,
            (o, v) => o.ItemFilter = v);
    
    public static readonly DirectProperty<CascaderView, object?> ItemFilterValueProperty =
        AvaloniaProperty.RegisterDirect<CascaderView, object?>(
            nameof(ItemFilterValue),
            o => o.ItemFilterValue,
            (o, v) => o.ItemFilterValue = v);
    
    public static readonly StyledProperty<TextBlockHighlightStrategy> ItemFilterHighlightStrategyProperty =
        AvaloniaProperty.Register<HighlightableTextBlock, TextBlockHighlightStrategy>(nameof(ItemFilterHighlightStrategy), TextBlockHighlightStrategy.All);
    
    public static readonly DirectProperty<CascaderView, int> FilterResultCountProperty =
        AvaloniaProperty.RegisterDirect<CascaderView, int>(nameof(FilterResultCount),
            o => o.FilterResultCount,
            (o, v) => o.FilterResultCount = v);
    
    public static readonly StyledProperty<IBrush?> FilterHighlightForegroundProperty =
        AvaloniaProperty.Register<CascaderView, IBrush?>(nameof(FilterHighlightForeground));
    
    public static readonly StyledProperty<object?> EmptyIndicatorProperty =
        AvaloniaProperty.Register<CascaderView, object?>(nameof(EmptyIndicator));
    
    public static readonly StyledProperty<IDataTemplate?> EmptyIndicatorTemplateProperty =
        AvaloniaProperty.Register<CascaderView, IDataTemplate?>(nameof(EmptyIndicatorTemplate));
    
    public static readonly StyledProperty<bool> IsShowEmptyIndicatorProperty =
        AvaloniaProperty.Register<CascaderView, bool>(nameof(IsShowEmptyIndicator), true);
    
    public static readonly StyledProperty<Thickness> EmptyIndicatorPaddingProperty =
        AvaloniaProperty.Register<CascaderView, Thickness>(nameof(EmptyIndicatorPadding));
    
    public bool IsAutoExpandParent
    {
        get => GetValue(IsAutoExpandParentProperty);
        set => SetValue(IsAutoExpandParentProperty, value);
    }
    
    public bool IsShowIcon
    {
        get => GetValue(IsShowIconProperty);
        set => SetValue(IsShowIconProperty, value);
    }

    public IconTemplate? ItemExpandIcon
    {
        get => GetValue(ItemExpandIconProperty);
        set => SetValue(ItemExpandIconProperty, value);
    }
    
    public IconTemplate? ItemLoadingIcon
    {
        get => GetValue(ItemLoadingIconProperty);
        set => SetValue(ItemLoadingIconProperty, value);
    }
    
    public CascaderViewExpandTrigger ExpandTrigger
    {
        get => GetValue(ExpandTriggerProperty);
        set => SetValue(ExpandTriggerProperty, value);
    }
    
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }
    
    public bool IsCheckable
    {
        get => GetValue(IsCheckableProperty);
        set => SetValue(IsCheckableProperty, value);
    }
    
    private TreeNodePath? _defaultExpandedPath;
    
    public TreeNodePath? DefaultExpandedPath
    {
        get => _defaultExpandedPath;
        set => SetAndRaise(DefaultExpandedPathProperty, ref _defaultExpandedPath, value);
    }
    
    private ICascaderItemDataLoader? _itemDataLoader;
    
    public ICascaderItemDataLoader? ItemDataLoader
    {
        get => _itemDataLoader;
        set => SetAndRaise(ItemDataLoaderProperty, ref _itemDataLoader, value);
    }
    
    private ICascaderItemFilter? _itemFilter;
    
    public ICascaderItemFilter? ItemFilter
    {
        get => _itemFilter;
        set => SetAndRaise(ItemFilterProperty, ref _itemFilter, value);
    }

    private object? _itemFilterValue;
    
    public object? ItemFilterValue
    {
        get => _itemFilterValue;
        set => SetAndRaise(ItemFilterValueProperty, ref _itemFilterValue, value);
    }
    
    public TextBlockHighlightStrategy ItemFilterHighlightStrategy
    {
        get => GetValue(ItemFilterHighlightStrategyProperty);
        set => SetValue(ItemFilterHighlightStrategyProperty, value);
    }
    
    private int _filterResultCount;
    
    public int FilterResultCount
    {
        get => _filterResultCount;
        set => SetAndRaise(FilterResultCountProperty, ref _filterResultCount, value);
    }
    
    public IBrush? FilterHighlightForeground
    {
        get => GetValue(FilterHighlightForegroundProperty);
        set => SetValue(FilterHighlightForegroundProperty, value);
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
    
    public Thickness EmptyIndicatorPadding
    {
        get => GetValue(EmptyIndicatorPaddingProperty);
        set => SetValue(EmptyIndicatorPaddingProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the selected items.
    /// </summary>
    [AllowNull]
    public IList CheckedItems
    {
        get
        {
            if (_checkedItems == null)
            {
                _checkedItems = new AvaloniaList<object>();
                SubscribeToCheckedItems();
            }

            return _checkedItems;
        }
        set
        {
            if (value?.IsReadOnly == true)
            {
                throw new NotSupportedException(
                    "Cannot use a fixed size or read-only collection as CheckedItems.");
            }

            UnsubscribeFromCheckedItems();
            _checkedItems = value ?? new AvaloniaList<object>();
            SubscribeToCheckedItems();
        }
    }
    #endregion
    
    #region 公共事件定义
    
    public event EventHandler<CascaderViewCheckedItemsChangedEventArgs>? CheckedItemsChanged;
    public event EventHandler<CascaderViewItemLoadedEventArgs>? ItemAsyncLoaded;
    public event EventHandler<CascaderItemExpandedEventArgs>? ItemExpanded;
    public event EventHandler<CascaderItemCollapsedEventArgs>? ItemCollapsed;
    public event EventHandler<CascaderItemClickedEventArgs>? ItemClicked;
    
    #endregion

    #region 内部属性定义
    internal static readonly DirectProperty<CascaderView, bool> IsEffectiveEmptyVisibleProperty =
        AvaloniaProperty.RegisterDirect<CascaderView, bool>(
            nameof(IsEffectiveEmptyVisible),
            o => o.IsEffectiveEmptyVisible,
            (o, v) => o.IsEffectiveEmptyVisible = v);
    
    internal static readonly DirectProperty<CascaderView, ItemToggleType> EffectiveToggleTypeProperty =
        AvaloniaProperty.RegisterDirect<CascaderView, ItemToggleType>(
            nameof(EffectiveToggleType),
            o => o.EffectiveToggleType,
            (o, v) => o.EffectiveToggleType = v);
    
    private bool _isEffectiveEmptyVisible = false;
    internal bool IsEffectiveEmptyVisible
    {
        get => _isEffectiveEmptyVisible;
        set => SetAndRaise(IsEffectiveEmptyVisibleProperty, ref _isEffectiveEmptyVisible, value);
    }
    
    private ItemToggleType _effectiveToggleType = ItemToggleType.None;
    internal ItemToggleType EffectiveToggleType
    {
        get => _effectiveToggleType;
        set => SetAndRaise(EffectiveToggleTypeProperty, ref _effectiveToggleType, value);
    }

    Control IControlSharedTokenResourcesHost.HostControl => this;
    string IControlSharedTokenResourcesHost.TokenId => TreeViewToken.ID;
    
    protected internal ICascaderViewInteractionHandler InteractionHandler { get; }
    #endregion
    
    private readonly Dictionary<CascaderViewItem, CompositeDisposable> _itemsBindingDisposables = new();
    private static readonly IList Empty = Array.Empty<object>();
    private IList? _checkedItems;
    private bool _syncingCheckedItems;
    private StackPanel? _itemsPanel;
    private int _ignoreExpandAndCollapseLevel;
    private CascaderViewItem? _lastHoveredItem; // 触发器是 hover 的时候使用
    private bool _defaultExpandPathApplied;

    static CascaderView()
    {
        CascaderViewItem.ExpandedEvent.AddClassHandler<CascaderView>((view, args) => view.HandleCascaderItemExpanded(args));
        CascaderViewItem.CollapsedEvent.AddClassHandler<CascaderView>((view, args) => view.HandleCascaderItemCollapsed(args));
        CascaderViewItem.ClickEvent.AddClassHandler<CascaderView>((view, args) => view.HandleCascaderItemClicked(args));
    }
    
    public CascaderView()
        : this(new DefaultCascaderViewInteractionHandler())
    {
    }
    
    protected CascaderView(ICascaderViewInteractionHandler interactionHandler)
    {
        InteractionHandler = interactionHandler ?? throw new ArgumentNullException(nameof(interactionHandler));
        this.RegisterResources();
        LogicalChildren.CollectionChanged += HandleLogicalChildrenCollectionChanged;
        Items.CollectionChanged           += HandleCollectionChanged;
    }
    
    protected override void OnInitialized()
    {
        base.OnInitialized();
        ConfigureEmptyIndicator();
        ConfigureEffectiveToggleType();
        ItemFilter ??= new DefaultCascaderItemFilter();
    }
    
    public IEnumerable<Control> GetRealizedTreeContainers()
    {
        return GetRealizedContainers(this);
    }
    
    private static IEnumerable<Control> GetRealizedContainers(ItemsControl itemsControl)
    {
        foreach (var container in itemsControl.GetRealizedContainers())
        {
            yield return container;
            if (container is ItemsControl itemsControlContainer)
            {
                foreach (var child in GetRealizedContainers(itemsControlContainer))
                {
                    yield return child;
                }
            }
        }
    }
    
    public Control? CascaderContainerFromItem(object item)
    {
        return CascaderContainerFromItem(this, item);
    }
    
    private Control? CascaderContainerFromItem(ItemsControl itemsControl, object item)
    {
        if (itemsControl.ContainerFromItem(item) is { } container)
        {
            return container;
        }
        
        if (_itemsPanel != null)
        {
            for (var i = 0; i < _itemsPanel.Children.Count; i++)
            {
                if (_itemsPanel.Children[i] is CascaderViewLevelList levelList)
                {
                    if (levelList.ContainerFromItem(item) is { } childContainer)
                    {
                        return childContainer;
                    }
                }
            }
        }

        return null;
    }
    
    public object? CascaderItemFromContainer(Control container)
    {
        return CascaderItemFromContainer(this, container);
    }
    
    private object? CascaderItemFromContainer(ItemsControl itemsControl, Control container)
    {
        if (itemsControl.ItemFromContainer(container) is { } item)
        {
            return item;
        }

        if (_itemsPanel != null)
        {
            for (var i = 0; i < _itemsPanel.Children.Count; i++)
            {
                if (_itemsPanel.Children[i] is CascaderViewLevelList levelList)
                {
                    if (levelList.ItemFromContainer(container) is { } childItem)
                    {
                        return childItem;
                    }
                }
            }
        }

        return null;
    }
    
    private void HandleLogicalChildrenCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems != null)
        {
            if (e.Action == NotifyCollectionChangedAction.Remove && e.OldItems.Count > 0)
            {
                foreach (var item in e.OldItems)
                {
                    if (item is CascaderViewItem cascaderViewItem)
                    {
                        if (_itemsBindingDisposables.TryGetValue(cascaderViewItem, out var disposable))
                        {
                            disposable.Dispose();
                            _itemsBindingDisposables.Remove(cascaderViewItem);
                        }
                    }
                }
            }
        }
    }
    
    private void HandleCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        ConfigureEmptyIndicator();
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Remove:
            case NotifyCollectionChangedAction.Replace:
                foreach (var i in e.OldItems!)
                    CheckedItems.Remove(i);
                break;
            case NotifyCollectionChangedAction.Reset:
                CheckedItems.Clear();
                break;
        }
        Filter();
    }
    
    protected CascaderViewItem? GetContainerFromEventSource(object eventSource)
    {
        var item = ((Visual)eventSource).GetSelfAndVisualAncestors()
                                        .OfType<CascaderViewItem>()
                                        .FirstOrDefault();
    
        return item?.CascaderViewOwner == this ? item : null;
    }
    
    public void CheckedSubTree(CascaderViewItem item)
    {
        if (!item.IsEffectiveCheckable())
        {
            return;
        }

        ISet<object>? checkedItems = null;
        if (ItemsSource != null && item.DataContext is ICascaderViewItemData itemData)
        {
            checkedItems = DoCheckedSubTree(itemData);
        }
        else
        {
            checkedItems = DoCheckedSubTree(item);
        }
        try
        {
            _syncingCheckedItems = true;
            foreach (var checkedItem in checkedItems)
            {
                if (!CheckedItems.Contains(checkedItem))
                {
                    CheckedItems.Add(checkedItem);
                }
            }
        }
        finally
        {
            _syncingCheckedItems = false; 
        }
    }

    private ISet<object> DoCheckedSubTree(CascaderViewItem item)
    {
        var checkedItems = new HashSet<object>();
        item.SetCurrentValue(CascaderViewItem.IsCheckedProperty, true);
        
        checkedItems.Add(item);

        foreach (var childItem in item.Items)
        {
            if (childItem is CascaderViewItem cascaderViewItem && (!cascaderViewItem.IsAttachedToVisualTree() || cascaderViewItem.IsEffectiveCheckable()))
            {
                var childCheckedItems = DoCheckedSubTree(cascaderViewItem);
                checkedItems.UnionWith(childCheckedItems);
            }
        }
        
        var (checkedParentItems, _) = SetupParentNodeCheckedStatus(item);
        checkedItems.UnionWith(checkedParentItems);
        return checkedItems;
    }

    private ISet<object> DoCheckedSubTree(ICascaderViewItemData item)
    {
        var checkedItems = new HashSet<object>();
        item.IsChecked = true;
        
        checkedItems.Add(item);

        foreach (var childItem in item.Children)
        {
            if (childItem.IsEnabled)
            {
                var childCheckedItems = DoCheckedSubTree(childItem);
                checkedItems.UnionWith(childCheckedItems);
            }
        }
        
        var (checkedParentItems, _) = SetupParentNodeCheckedStatus(item);
        checkedItems.UnionWith(checkedParentItems);
        return checkedItems;
    }

    public void UnCheckedSubTree(CascaderViewItem item)
    {
        if (!item.IsEffectiveCheckable())
        {
            return;
        }
        
        ISet<object>? unCheckedItems = null;
        if (ItemsSource != null && item.DataContext is ICascaderViewItemData itemData)
        {
            unCheckedItems = DoUnCheckedSubTree(itemData);
        }
        else
        {
            unCheckedItems = DoUnCheckedSubTree(item);
        }
        
        try
        {
            _syncingCheckedItems = true;
            foreach (var unCheckedItem in unCheckedItems)
            {
                CheckedItems.Remove(unCheckedItem);
            }
            CheckedItems.Remove(item);
        }
        finally
        {
            _syncingCheckedItems = false; 
        }
    }

    public ISet<object> DoUnCheckedSubTree(CascaderViewItem item)
    {
        var unCheckedItems = new HashSet<object>();
        if (item.IsChecked == true)
        {
            unCheckedItems.Add(item);
        }
        item.SetCurrentValue(CascaderViewItem.IsCheckedProperty, false);

        foreach (var childItem in item.Items)
        {
            if (childItem is CascaderViewItem cascaderViewItem && (!cascaderViewItem.IsAttachedToVisualTree() || cascaderViewItem.IsEffectiveCheckable()))
            {
                var childUnCheckedItems = DoUnCheckedSubTree(cascaderViewItem);
                unCheckedItems.UnionWith(childUnCheckedItems);
            }
        }
        var (_, unCheckedParentItems) = SetupParentNodeCheckedStatus(item);
        unCheckedItems.UnionWith(unCheckedParentItems);
        return unCheckedItems;
    }
    
    public ISet<object> DoUnCheckedSubTree(ICascaderViewItemData item)
    {
        var unCheckedItems = new HashSet<object>();
        if (item.IsChecked == true)
        {
            unCheckedItems.Add(item);
        }

        item.IsChecked = false;

        foreach (var childItem in item.Children)
        {
            if (childItem.IsEnabled)
            {
                var childUnCheckedItems = DoUnCheckedSubTree(childItem);
                unCheckedItems.UnionWith(childUnCheckedItems);
            }
        }
        var (_, unCheckedParentItems) = SetupParentNodeCheckedStatus(item);
        unCheckedItems.UnionWith(unCheckedParentItems);
        return unCheckedItems;
    }
    
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        return new CascaderViewItem();
    }

    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
    {
        return NeedsContainer<CascaderViewItem>(item, out recycleKey);
    }

    protected override void ContainerForItemPreparedOverride(Control container, object? item, int index)
    {
        base.ContainerForItemPreparedOverride(container, item, index);
        if (container is CascaderViewItem cascaderViewItem)
        {
            cascaderViewItem.OwnerView = this;
            var disposables = new CompositeDisposable(8);
            
            if (item != null && item is not Visual && item is ICascaderViewItemData cascaderViewItemData)
            {
                CascaderViewItem.ApplyNodeData(cascaderViewItem, cascaderViewItemData, disposables);
            }
            
            if (ItemTemplate != null)
            {
                disposables.Add(BindUtils.RelayBind(this, ItemTemplateProperty, cascaderViewItem, CascaderViewItem.HeaderTemplateProperty));
            }
            
            disposables.Add(BindUtils.RelayBind(this, ItemLoadingIconProperty, cascaderViewItem, CascaderViewItem.LoadingIconProperty));
            disposables.Add(BindUtils.RelayBind(this, ItemExpandIconProperty, cascaderViewItem, CascaderViewItem.ExpandIconProperty));
            disposables.Add(BindUtils.RelayBind(this, IsMotionEnabledProperty, cascaderViewItem, CascaderViewItem.IsMotionEnabledProperty));
            disposables.Add(BindUtils.RelayBind(this, IsShowIconProperty, cascaderViewItem, CascaderViewItem.IsShowIconProperty));
            disposables.Add(BindUtils.RelayBind(this, EffectiveToggleTypeProperty, cascaderViewItem, CascaderViewItem.ToggleTypeProperty));
            disposables.Add(BindUtils.RelayBind(this, HasItemAsyncDataLoaderProperty, cascaderViewItem,
                CascaderViewItem.HasItemAsyncDataLoaderProperty));
            disposables.Add(BindUtils.RelayBind(this, IsAutoExpandParentProperty, cascaderViewItem,
                CascaderViewItem.IsAutoExpandParentProperty));
            
            PrepareCascaderViewItem(cascaderViewItem, item, index, disposables);
            
            if (_itemsBindingDisposables.TryGetValue(cascaderViewItem, out var oldDisposables))
            {
                oldDisposables.Dispose();
                _itemsBindingDisposables.Remove(cascaderViewItem);
            }
            _itemsBindingDisposables.Add(cascaderViewItem, disposables);
        }
        else
        {
            throw new ArgumentOutOfRangeException(nameof(container), "The container type is incorrect, it must be type CascaderViewItem.");
        }
    }
    
    protected virtual void PrepareCascaderViewItem(CascaderViewItem cascaderViewItem, object? item, int index, CompositeDisposable disposables)
    {
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        InteractionHandler.Attach(this);
    }
    
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        InteractionHandler.Detach(this);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _itemsPanel = e.NameScope.Find<StackPanel>(CascaderViewThemeConstants.ItemsPanelPart);
        if (_filterList != null)
        {
            _filterList.SelectionChanged -= HandleFilterListSelectionChanged;
        }
        _filterList = e.NameScope.Find<CascaderViewFilterList>(CascaderViewThemeConstants.FilterListPart);
        
        if (_filterList != null)
        {
            _filterList.SelectionChanged += HandleFilterListSelectionChanged;
        }
    }

    private CascaderViewItem? GetCascaderViewItemContainer(object childNode, ItemsControl current)
    {
        if (current.Presenter?.Panel == null && this.GetVisualRoot() is ILayoutRoot visualRoot)
        {
            var layoutManager = visualRoot.GetLayoutManager();
            layoutManager.ExecuteLayoutPass();
        }
        return current.ContainerFromItem(childNode) as CascaderViewItem;
    }
    
    private void SubscribeToCheckedItems()
    {
        if (_checkedItems is INotifyCollectionChanged collection)
        {
            collection.CollectionChanged += HandleCheckedItemsCollectionChanged;
        }

        HandleCheckedItemsCollectionChanged(
            _checkedItems,
            new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }
    
    private void UnsubscribeFromCheckedItems()
    {
        if (_checkedItems is INotifyCollectionChanged incc)
        {
            incc.CollectionChanged -= HandleCheckedItemsCollectionChanged;
        }
    }
    
    private void HandleCheckedItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        IList? added   = null;
        IList? removed = null;

        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:

            {
                if (e.NewItems != null)
                {
                    if (!_syncingCheckedItems)
                    {
                        CheckedItemsAdded(e.NewItems); 
                    }
                    added = e.NewItems;
                }
               
                break;
            }
            case NotifyCollectionChangedAction.Remove:
                if (!_syncingCheckedItems)
                {
                    if (e.OldItems != null)
                    {
                        foreach (var item in e.OldItems)
                        {
                            MarkItemChecked(item, false);
                        }
                    }
                }

                removed = e.OldItems;

                break;
            case NotifyCollectionChangedAction.Reset:
                if (!_syncingCheckedItems)
                {
                    foreach (var container in GetRealizedTreeContainers())
                    {
                        MarkContainerChecked(container, false);
                    }
                    if (CheckedItems.Count > 0)
                    {
                        CheckedItemsAdded(CheckedItems);
                    }
                }

                if (CheckedItems.Count > 0)
                {
                    added = CheckedItems;
                }

                break;
            case NotifyCollectionChangedAction.Replace:
            {
                if (!_syncingCheckedItems)
                {
                    if (e.OldItems != null)
                    {
                        foreach (var item in e.OldItems)
                        {
                            MarkItemChecked(item, false);
                        }
                    }

                    if (e.NewItems != null)
                    {
                        foreach (var item in e.NewItems)
                        {
                            MarkItemChecked(item, true);
                        }
                    }
                }
                
                added   = e.NewItems;
                removed = e.OldItems;
                break;
            }
        }
        if (added?.Count > 0 || removed?.Count > 0)
        {
            CheckedItemsChanged?.Invoke(this, new CascaderViewCheckedItemsChangedEventArgs(
                removed ?? Empty,
                added ?? Empty));
        }
    }
    
    private void CheckedItemsAdded(IList items)
    {
        if (items.Count == 0)
        {
            return;
        }
        foreach (var item in items)
        {
            MarkItemChecked(item, true);
        }
    }
    
    private void MarkItemChecked(object item, bool isChecked)
    {
        var container = CascaderContainerFromItem(item);
        if (container != null)
        {
            MarkContainerChecked(container, isChecked);
        }
    }
    
    private void MarkContainerChecked(Control container, bool isChecked)
    {
        container.SetCurrentValue(CascaderViewItem.IsCheckedProperty, isChecked);
    }
    
    private (ISet<object>, ISet<object>) SetupParentNodeCheckedStatus(CascaderViewItem item)
    {
        var parent           = item.Parent;
        var checkedParents   =  new HashSet<object>();
        var unCheckedParents =  new HashSet<object>();
        while (parent is CascaderViewItem parentCascaderItem && parentCascaderItem.IsEnabled)
        {
            var isAllChecked = false;
            var isAnyChecked = false;

            if (parentCascaderItem.Items.Count > 0)
            {
                isAllChecked = parentCascaderItem.Items.All(childItem =>
                {
                    if (childItem is CascaderViewItem cascaderViewItem)
                    {
                        return (cascaderViewItem.IsAttachedToVisualTree() && !cascaderViewItem.IsEffectiveCheckable()) || cascaderViewItem.IsChecked.HasValue && cascaderViewItem.IsChecked.Value;
                    }
                    return false;
                });

                isAnyChecked = parentCascaderItem.Items.Any(childItem =>
                {
                    if (childItem is CascaderViewItem cascaderViewItem)
                    {
                        return (cascaderViewItem.IsEffectiveCheckable() || !cascaderViewItem.IsAttachedToVisualTree()) && (!cascaderViewItem.IsChecked.HasValue || cascaderViewItem.IsChecked.HasValue && cascaderViewItem.IsChecked.Value);
                    }
                    return false;
                });
            }

            if (parentCascaderItem.IsChecked == true && !isAllChecked)
            {
                unCheckedParents.Add(parentCascaderItem);
            }
            
            var originMotionEnabled = parentCascaderItem.IsMotionEnabled;
            try
            {
                parentCascaderItem.SetCurrentValue(CascaderViewItem.IsMotionEnabledProperty, false);
                if (isAllChecked)
                {
                    parentCascaderItem.SetCurrentValue(CascaderViewItem.IsCheckedProperty, true);
                }
                else if (isAnyChecked)
                {
                    parentCascaderItem.SetCurrentValue(CascaderViewItem.IsCheckedProperty, null);
                }
                else
                {
                    parentCascaderItem.SetCurrentValue(CascaderViewItem.IsCheckedProperty, false);
                }
            }
            finally
            {
                parentCascaderItem.SetCurrentValue(CascaderViewItem.IsMotionEnabledProperty, originMotionEnabled);
            }
       
            if (parentCascaderItem.IsChecked == true)
            {
                checkedParents.Add(parentCascaderItem);
            }
            parent = parent.Parent;
        }

        return (checkedParents, unCheckedParents);
    }

    private (ISet<object>, ISet<object>) SetupParentNodeCheckedStatus(ICascaderViewItemData item)
    {
        var parent           = item.ParentNode;
        var checkedParents   =  new HashSet<object>();
        var unCheckedParents =  new HashSet<object>();
        while (parent is ICascaderViewItemData parentCascaderItemData && parentCascaderItemData.IsEnabled)
        {
            var isAllChecked = false;
            var isAnyChecked = false;

            if (parentCascaderItemData.Children.Count > 0)
            {
                isAllChecked = parentCascaderItemData.Children.All(childItem =>
                {
                    return !childItem.IsEnabled || childItem.IsChecked.HasValue && childItem.IsChecked.Value;
                });

                isAnyChecked = parentCascaderItemData.Children.Any(childItem =>
                {
                    return childItem.IsEnabled && (!childItem.IsChecked.HasValue || childItem.IsChecked.HasValue && childItem.IsChecked.Value);
                });
            }

            if (parentCascaderItemData.IsChecked == true && !isAllChecked)
            {
                unCheckedParents.Add(parentCascaderItemData);
            }
            
            if (isAllChecked)
            {
                parentCascaderItemData.IsChecked = true;
            }
            else if (isAnyChecked)
            {
                parentCascaderItemData.IsChecked = null;
            }
            else
            {
                parentCascaderItemData.IsChecked = false;
            }
       
            if (parentCascaderItemData.IsChecked == true)
            {
                checkedParents.Add(parentCascaderItemData);
            }
            parent = parent.ParentNode;
        }

        return (checkedParents, unCheckedParents);
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        Filter();
        if (DefaultExpandedPath != null && !_defaultExpandPathApplied)
        {
            ApplyDefaultExpandPath();
        }
    }

    private void ApplyDefaultExpandPath()
    {
        Debug.Assert(DefaultExpandedPath != null);
        var segments = DefaultExpandedPath.Segments;
        var count    = DefaultExpandedPath.Segments.Count;
        if (ItemsSource != null)
        {
            IList<ICascaderViewItemData> currentItems = Items.Cast<ICascaderViewItemData>().ToList();
            var     isPathValid  = true;
            var     itemDatas    = new List<ICascaderViewItemData>();
            for (var i = 0; i < count; i++)
            {
                var segment = segments[i];
                var found   = false;
                foreach (var currentItem in currentItems)
                {
                    if (segment == currentItem.ItemKey)
                    {
                        itemDatas.Add(currentItem);
                        currentItems = currentItem.Children;
                        found        = true;
                    }
                }

                if (!found)
                {
                    isPathValid = false;
                    break;
                }
            }

            if (isPathValid)
            {
                if (itemDatas.Count > 0)
                {
                    foreach (var itemData in itemDatas)
                    {
                        itemData.IsExpanded = true;
                    }
                }
            }
        }
        else
        {
            var currentItems = Items;
            var isPathValid  = true;
            var pathNodes    = new List<CascaderViewItem>();
            for (var i = 0; i < count; i++)
            {
                var segment = segments[i];
                var found   = false;
                foreach (var currentItem in currentItems)
                {
                    if (currentItem is CascaderViewItem cascaderViewItem)
                    {
                        if (segment == cascaderViewItem.ItemKey)
                        {
                            pathNodes.Add(cascaderViewItem);
                            currentItems = cascaderViewItem.Items;
                            found = true;
                        }
                    }
                }

                if (!found)
                {
                    isPathValid = false;
                    break;
                }
            }

            if (isPathValid)
            {
                if (pathNodes.Count > 0)
                {
                    Dispatcher.UIThread.InvokeAsync(async () =>
                    {
                        foreach (var cascaderViewItem in pathNodes)
                        {
                            await ExpandItemAsync(cascaderViewItem);
                        }
                    });
                }
            }
        }
    }
    
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ItemDataLoaderProperty)
        {
            HasItemAsyncDataLoader = ItemDataLoader != null;
        }
        else if (change.Property == ItemFilterProperty ||
                 change.Property == ItemFilterHighlightStrategyProperty ||
                 change.Property == ItemsSourceProperty ||
                 change.Property == ItemFilterValueProperty)
        {
            Filter();
        }

        if (change.Property == IsShowEmptyIndicatorProperty ||
            change.Property == ItemsSourceProperty ||
            change.Property == FilterResultCountProperty)
        {
            ConfigureEmptyIndicator();
        }
        else if (change.Property == IsCheckableProperty)
        {
            ConfigureEffectiveToggleType();
        }
    }
    
    private void HandleCascaderItemClicked(RoutedEventArgs args)
    {
        if (args.Source is CascaderViewItem item)
        {
            NotifyCascaderItemClicked(item);
            ItemClicked?.Invoke(this, new CascaderItemClickedEventArgs(item));
        }
    }

    protected virtual void NotifyCascaderItemClicked(CascaderViewItem item)
    {
    }
    
    protected virtual void ConfigureEmptyIndicator()
    {
        var isEmpty = false;
        if (IsFiltering)
        {
            isEmpty = FilterResultCount == 0;
        }
        else
        {
            if (ItemsSource != null)
            {
                var enumerator = ItemsSource.GetEnumerator();
                isEmpty = !enumerator.MoveNext();
                if (enumerator is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
            else
            {
                isEmpty = Items.Count == 0;
            }
        }
        IsEffectiveEmptyVisible = IsShowEmptyIndicator && isEmpty;
    }
    
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        if (e.Source is Visual source)
        {
            var point = e.GetCurrentPoint(source);
            if (point.Properties.IsLeftButtonPressed || point.Properties.IsRightButtonPressed)
            {
                if (ExpandTrigger == CascaderViewExpandTrigger.Click)
                {
                    var cascaderViewItem = GetContainerFromEventSource(e.Source);
                    if (cascaderViewItem != null)
                    {
                        cascaderViewItem.IsExpanded = !cascaderViewItem.IsExpanded;
                    }
                }
            }
        }
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        if (e.Source is Visual source)
        {
            if (ExpandTrigger == CascaderViewExpandTrigger.Hover)
            {
                var cascaderViewItem = GetContainerFromEventSource(e.Source);
                if (_lastHoveredItem != null &&
                    cascaderViewItem != null && 
                    _lastHoveredItem != cascaderViewItem &&
                    !IsDescendantOf(cascaderViewItem, _lastHoveredItem, ItemsSource != null))
                {
                    _lastHoveredItem.IsExpanded = false;
                }
                if (cascaderViewItem != null)
                {
                    cascaderViewItem.IsExpanded = true;
                    _lastHoveredItem            = cascaderViewItem;
                }
            }
        }
    }

    private bool IsDescendantOf(CascaderViewItem lhs, CascaderViewItem rhs, bool isTemplateMode)
    {
        if (isTemplateMode)
        {
            var lhsData = lhs.DataContext as ICascaderViewItemData;
            var rhsData = rhs.DataContext as ICascaderViewItemData;
            Debug.Assert(lhsData != null && rhsData != null);
            var currentData = lhsData;
            while (currentData != null)
            {
                if (currentData == rhsData)
                {
                    return true;
                }
                currentData = currentData.ParentNode as ICascaderViewItemData;
            }

            return false;
        }

        var current = lhs;
        while (current != null)
        {
            if (current == rhs)
            {
                return true;
            }
            current = current.Parent as CascaderViewItem;
        }
        return false;
    }

    private void HandleCascaderItemExpanded(RoutedEventArgs args)
    {
        if (args.Source is CascaderViewItem item)
        {
            if (_ignoreExpandAndCollapseLevel == 0)
            {
                Dispatcher.UIThread.InvokeAsync(async () => { await ExpandItemAsync(item); });
            }
            ItemExpanded?.Invoke(this, new CascaderItemExpandedEventArgs(item));
        }
    }
    
    private void HandleCascaderItemCollapsed(RoutedEventArgs args)
    {
        if (args.Source is CascaderViewItem item)
        {
            if (_ignoreExpandAndCollapseLevel == 0)
            {
                CollapseItem(item);
            }

            ItemCollapsed?.Invoke(this, new CascaderItemCollapsedEventArgs(item));
        }
    }

    private async Task ExpandItemAsync(CascaderViewItem cascaderViewItem)
    {
        if (_itemsPanel == null || _ignoreExpandAndCollapseLevel > 0)
        {
            return;
        }
        try
        {
            ++_ignoreExpandAndCollapseLevel;

            if (cascaderViewItem.ItemsSource == null)
            {
                // 保证当前 Item 的父亲都在了
                var parents = new List<CascaderViewItem>();
                var parent  = cascaderViewItem.Parent;
                while (parent != null && parent is CascaderViewItem parentCascaderItem)
                {
                    parents.Add(parentCascaderItem);
                    parent = parent.Parent;
                }

                parents.Reverse();

                foreach (var parentItem in parents)
                {
                    if (!parentItem.IsExpanded)
                    {
                        var parentList = new CascaderViewLevelList()
                        {
                            ItemsPanel = ItemsPanel,
                            OwnerView  = this
                        };
                        if (parentItem.ItemsSource == null)
                        {
                            foreach (var item in parentItem.Items)
                            {
                                parentList.Items.Add(item);
                            }
                        }

                        BindUtils.RelayBind(parentItem, CascaderViewItem.ItemsSourceProperty, parentList,
                            CascaderViewLevelList.ItemsSourceProperty);
                        BindUtils.RelayBind(parentItem, CascaderViewItem.ItemTemplateProperty, parentList,
                            CascaderViewLevelList.ItemTemplateProperty);
                        _itemsPanel.Children.Add(parentList);
                        parentItem.IsExpanded = true;
                    }
                }
            }
            else
            {
                // 保证当前 Item 的父亲都在了
                var parents = new List<ICascaderViewItemData>();
                var parent  = cascaderViewItem.DataContext as ICascaderViewItemData;
                while (parent != null)
                {
                    parents.Add(parent);
                    parent = parent.ParentNode as ICascaderViewItemData;
                }
                parents.Reverse();
                foreach (var parentItem in parents)
                {
                    if (!parentItem.IsExpanded)
                    {
                        var parentList = new CascaderViewLevelList()
                        {
                            ItemsPanel = ItemsPanel,
                            OwnerView  = this
                        };
                        
                        BindUtils.RelayBind(this, ItemTemplateProperty, parentList,
                            CascaderViewLevelList.ItemTemplateProperty);
                        _itemsPanel.Children.Add(parentList);
                        if (parentItem is CascaderViewItemData parentItemData)
                        {
                            parentItemData.IsExpanded = true;
                        }
                    }
                }
            }

            ClearExpandedState(cascaderViewItem.Level);
            cascaderViewItem.IsExpanded = true;
            
            var targetIndex = cascaderViewItem.Level + 1;
            var count       = _itemsPanel.Children.Count;
            while (count > targetIndex)
            {
                --count;
                if (_itemsPanel.Children[count] is CascaderViewLevelList levelList)
                {
                    levelList.ItemsSource = null;
                    levelList.Items.Clear(); 
                }
                _itemsPanel.Children.RemoveAt(count);
            }
            
            if (cascaderViewItem.DataContext is ICascaderViewItemData cascaderViewItemData && 
                cascaderViewItemData.Children.Count == 0)
            {
                if (ItemDataLoader == null || cascaderViewItemData.IsLeaf)
                {
                    return;
                }

                try
                {
                    --_ignoreExpandAndCollapseLevel;
                    await Dispatcher.UIThread.InvokeAsync(async () => { await LoadItemDataAsync(cascaderViewItem); });
                }
                finally
                {
                    ++_ignoreExpandAndCollapseLevel;
                }
                
                if (!cascaderViewItem.IsExpanded)
                {
                    return;
                }
            }
            else if (cascaderViewItem.DataContext is not ICascaderViewItemData && cascaderViewItem.ItemCount == 0)
            {
                return;
            }
            
            var childList = new CascaderViewLevelList()
            {
                ItemsPanel = ItemsPanel,
                OwnerView  = this
            };

            BindUtils.RelayBind(cascaderViewItem, CascaderViewItem.ItemsSourceProperty, childList, CascaderViewLevelList.ItemsSourceProperty);
            BindUtils.RelayBind(cascaderViewItem, CascaderViewItem.ItemTemplateProperty, childList, CascaderViewLevelList.ItemTemplateProperty);
            if (cascaderViewItem.ItemsSource == null)
            {
                foreach (var item in cascaderViewItem.Items)
                {
                    childList.Items.Add(item);
                }
            }
            _itemsPanel.Children.Add(childList);
            InvalidateMeasure();
        }
        finally
        {
            --_ignoreExpandAndCollapseLevel;
        }
    }
    
    private void CollapseItem(CascaderViewItem cascaderViewItem)
    {
        if (_itemsPanel == null || _ignoreExpandAndCollapseLevel > 0)
        {
            return;
        }
  
        try
        {
            ++_ignoreExpandAndCollapseLevel;
            ClearExpandedState(cascaderViewItem.Level + 1);
            cascaderViewItem.IsExpanded = false;
            if (cascaderViewItem.ItemCount == 0 && 
                cascaderViewItem.DataContext is ICascaderViewItemData cascaderViewItemData && cascaderViewItemData.Children.Count == 0)
            {
                return;
            }
            
            var targetIndex = cascaderViewItem.Level + 1;
            var count       = _itemsPanel.Children.Count;
            while (count > targetIndex)
            {
                --count;
                if (_itemsPanel.Children[count] is CascaderViewLevelList levelList)
                {
                    levelList.ItemsSource = null;
                    levelList.Items.Clear();
                }

                _itemsPanel.Children.RemoveAt(count);
            }
            InvalidateMeasure();
        }
        finally
        {
            --_ignoreExpandAndCollapseLevel;
        }
    }

    private void ClearExpandedState(int level)
    {
        if (_itemsPanel == null)
        {
            return;
        }
        for (var i = _itemsPanel.Children.Count - 1; i >= level; i--)
        {
            if (_itemsPanel.Children[i] is CascaderViewLevelList levelList)
            {
                foreach (var item in levelList.Items)
                {
                    if (item != null)
                    {
                        if (levelList.ContainerFromItem(item) is CascaderViewItem cascaderViewItem)
                        {
                            cascaderViewItem.IsExpanded = false;
                        }
                    }
                }
            }
            else
            {
                foreach (var item in Items)
                {
                    if (item != null)
                    {
                        if (CascaderContainerFromItem(item) is CascaderViewItem cascaderViewItem)
                        {
                            cascaderViewItem.IsExpanded = false;
                        }
                    }
                }
            }
        }
    }
    
    public override void Render(DrawingContext context)
    {
        if (_itemsPanel == null || IsFiltering)
        {
            return;
        }
        var penWidth = BorderThickness.Top;
        using var state = context.PushRenderOptions(new RenderOptions
        {
            EdgeMode = EdgeMode.Aliased
        });
        var count = _itemsPanel.Children.Count;
        var height = DesiredSize.Height;
        for (var i = 0; i < count; i++)
        {
            if (i > 0)
            {
                var child      = _itemsPanel.Children[i];
                var offset     = child.TranslatePoint(new Point(0, 0), this);
                if (offset != null)
                {
                    var pointStart = new Point(offset.Value.X, 0);
                    var pointEnd   = new Point(offset.Value.X, height);
                    context.DrawLine(new Pen(BorderBrush, penWidth), pointStart, pointEnd);
                }
            }
        }
    }

    private void ConfigureEffectiveToggleType()
    {
        if (IsCheckable)
        {
            EffectiveToggleType = ItemToggleType.CheckBox;
        }
        else
        {
            EffectiveToggleType = ItemToggleType.None;
        }
    }
    
}