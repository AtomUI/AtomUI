using System.Collections.Specialized;
using System.Reactive.Disposables;
using AtomUI.Controls;
using AtomUI.Data;
using AtomUI.Theme;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Metadata;

namespace AtomUI.Desktop.Controls;

using AvaloniaListBox = Avalonia.Controls.ListBox;

public class ListBox : AvaloniaListBox,
                       ISizeTypeAware,
                       IMotionAwareControl,
                       IControlSharedTokenResourcesHost
{
    #region 公共属性定义
    public static readonly StyledProperty<bool> IsItemSelectableProperty =
        AvaloniaProperty.Register<ListBox, bool>(nameof(IsItemSelectable), true);
    
    public static readonly StyledProperty<SizeType> SizeTypeProperty =
        SizeTypeControlProperty.SizeTypeProperty.AddOwner<ListBox>();
    
    public static readonly StyledProperty<bool> IsBorderlessProperty =
        AvaloniaProperty.Register<ListBox, bool>(nameof(IsBorderless), false);
        
    public static readonly StyledProperty<IBrush?> ItemHoverBgProperty =
        AvaloniaProperty.Register<ListBox, IBrush?>(nameof(ItemHoverBg));
    
    public static readonly StyledProperty<IBrush?> ItemSelectedBgProperty =
        AvaloniaProperty.Register<ListBox, IBrush?>(nameof(ItemSelectedBg));
    
    public static readonly StyledProperty<bool> IsShowSelectedIndicatorProperty =
        AvaloniaProperty.Register<ListBox, bool>(nameof(IsShowSelectedIndicator), false);
    
    public static readonly StyledProperty<IconTemplate?> SelectedIndicatorProperty =
        AvaloniaProperty.Register<ListBox, IconTemplate?>(nameof(SelectedIndicator));
    
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<ListBox>();
    
    public static readonly StyledProperty<Thickness> EmptyIndicatorPaddingProperty =
        AvaloniaProperty.Register<ListBox, Thickness>(nameof(EmptyIndicatorPadding));
    
    public static readonly StyledProperty<object?> EmptyIndicatorProperty =
        AvaloniaProperty.Register<ListBox, object?>(nameof(EmptyIndicator));
    
    public static readonly StyledProperty<IDataTemplate?> EmptyIndicatorTemplateProperty =
        AvaloniaProperty.Register<ListBox, IDataTemplate?>(nameof(EmptyIndicatorTemplate));
    
    public static readonly StyledProperty<bool> IsShowEmptyIndicatorProperty =
        AvaloniaProperty.Register<ListBox, bool>(nameof(IsShowEmptyIndicator), true);
    
    public static readonly DirectProperty<ListBox, IListBoxItemFilter?> ItemFilterProperty =
        AvaloniaProperty.RegisterDirect<ListBox, IListBoxItemFilter?>(
            nameof(ItemFilter),
            o => o.ItemFilter,
            (o, v) => o.ItemFilter = v);
    
    public static readonly DirectProperty<ListBox, object?> ItemFilterValueProperty =
        AvaloniaProperty.RegisterDirect<ListBox, object?>(
            nameof(ItemFilterValue),
            o => o.ItemFilterValue,
            (o, v) => o.ItemFilterValue = v);
    
    public static readonly DirectProperty<ListBox, TextBlockHighlightStrategy> ItemFilterHighlightStrategyProperty =
        AvaloniaProperty.RegisterDirect<ListBox, TextBlockHighlightStrategy>(
            nameof(ItemFilterHighlightStrategy),
            o => o.ItemFilterHighlightStrategy,
            (o, v) => o.ItemFilterHighlightStrategy = v);
    
    public static readonly DirectProperty<ListBox, int> FilterResultCountProperty =
        AvaloniaProperty.RegisterDirect<ListBox, int>(nameof(FilterResultCount),
            o => o.FilterResultCount);
    
    public static readonly DirectProperty<ListBox, bool> IsFilteringProperty =
        AvaloniaProperty.RegisterDirect<ListBox, bool>(nameof(IsFiltering),
            o => o.IsFiltering);
    
    public static readonly StyledProperty<IBrush?> FilterHighlightForegroundProperty =
        AvaloniaProperty.Register<ListBox, IBrush?>(nameof(FilterHighlightForeground));
    
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
    
    private IListBoxItemFilter? _itemFilter;
    
    public IListBoxItemFilter? ItemFilter
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
    
    private TextBlockHighlightStrategy _itemFilterHighlightStrategy = TextBlockHighlightStrategy.All;
    
    public TextBlockHighlightStrategy ItemFilterHighlightStrategy
    {
        get => _itemFilterHighlightStrategy;
        set => SetAndRaise(ItemFilterHighlightStrategyProperty, ref _itemFilterHighlightStrategy, value);
    }
    
    private int _filterResultCount;
    
    public int FilterResultCount
    {
        get => _filterResultCount;
        private set => SetAndRaise(FilterResultCountProperty, ref _filterResultCount, value);
    }
    
    private bool _isFiltering;
    
    public bool IsFiltering
    {
        get => _isFiltering;
        private set => SetAndRaise(IsFilteringProperty, ref _isFiltering, value);
    }

    public IBrush? FilterHighlightForeground
    {
        get => GetValue(FilterHighlightForegroundProperty);
        set => SetValue(FilterHighlightForegroundProperty, value);
    }
    #endregion

    #region 公共事件定义

    public event EventHandler<ListBoxItemClickedEventArgs>? ItemClicked;

    #endregion

    #region 内部属性定义

    internal static readonly DirectProperty<ListBox, Thickness> EffectiveBorderThicknessProperty =
        AvaloniaProperty.RegisterDirect<ListBox, Thickness>(nameof(EffectiveBorderThickness),
            o => o.EffectiveBorderThickness,
            (o, v) => o.EffectiveBorderThickness = v);
    
    internal static readonly DirectProperty<ListBox, bool> IsEffectiveEmptyVisibleProperty =
        AvaloniaProperty.RegisterDirect<ListBox, bool>(
            nameof(IsEffectiveEmptyVisible),
            o => o.IsEffectiveEmptyVisible,
            (o, v) => o.IsEffectiveEmptyVisible = v);
    
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
    string IControlSharedTokenResourcesHost.TokenId => ListBoxToken.ID;

    #endregion
    
    private protected readonly Dictionary<object, CompositeDisposable> _itemsBindingDisposables = new();
    private protected readonly Dictionary<object, bool> _filterContext = new();

    static ListBox()
    {
        ListBoxItem.ClickedEvent.AddClassHandler<ListBox>((list, args) => list.HandleListBoxItemClicked(args));
    }
    
    public ListBox()
    {
        this.RegisterResources();
        LogicalChildren.CollectionChanged += HandleChildrenChanged;
        Items.CollectionChanged += HandleItemCollectionChanged;
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        ItemFilter ??= new DefaultListBoxItemFilter();
        ConfigureEmptyIndicator();
        ConfigureIsFiltering();
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
        ConfigureEmptyIndicator();
        FilterItems();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ItemsSourceProperty ||
            change.Property == IsFilteringProperty ||
            change.Property == FilterResultCountProperty)
        {
            ConfigureEmptyIndicator();
        }
        else if (change.Property == BorderThicknessProperty ||
                 change.Property == IsBorderlessProperty)
        {
            ConfigureEffectiveBorderThickness();
        }
        else if (change.Property == IsItemSelectableProperty)
        {
            if (IsItemSelectable)
            {
                SetCurrentValue(SelectedIndexProperty, -1);
                SetCurrentValue(SelectedItemProperty, null);
                SetCurrentValue(SelectedItemsProperty, null);
            }
        }
        else if (change.Property == ItemFilterValueProperty ||
                 change.Property == ItemFilterProperty)
        {
            ConfigureIsFiltering();
            FilterItems();
        }
    }
    
    private void ConfigureIsFiltering()
    {
        IsFiltering = ItemFilter != null && ItemFilterValue != null;
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
        return new ListBoxItem();
    }

    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
    {
        return NeedsContainer<ListBoxItem>(item, out recycleKey);
    }
    
    protected override void PrepareContainerForItemOverride(Control container, object? item, int index)
    {
        base.PrepareContainerForItemOverride(container, item, index);
        if (container is ListBoxItem listBoxItem)
        {
            var disposables = new CompositeDisposable(8);
            
            if (item != null && item is not Visual)
            {
                if (item is ListBoxItemData itemData)
                {
                    if (!listBoxItem.IsSet(ListBoxItem.ContentProperty))
                    {
                        listBoxItem.SetCurrentValue(ListBoxItem.ContentProperty, item);
                    }
                    disposables.Add(BindUtils.RelayBind(itemData, ListBoxItemData.IsEnabledProperty, listBoxItem, ListBoxItem.IsEnabledProperty, BindingMode.TwoWay));
                    disposables.Add(BindUtils.RelayBind(itemData, ListBoxItemData.IsSelectedProperty, listBoxItem, ListBoxItem.IsSelectedProperty, BindingMode.TwoWay));
                }
                else if (item is IListBoxItemData iItemData)
                {
                    if (!listBoxItem.IsSet(ListBoxItem.ContentProperty))
                    {
                        listBoxItem.SetCurrentValue(ListBoxItem.ContentProperty, item);
                    }
                    if (!listBoxItem.IsSet(ListBoxItem.IsEnabledProperty))
                    {
                        listBoxItem.SetCurrentValue(ListBoxItem.IsEnabledProperty, iItemData.IsEnabled);
                    }
                    if (!listBoxItem.IsSet(ListBoxItem.IsSelectedProperty))
                    {
                        listBoxItem.SetCurrentValue(ListBoxItem.IsSelectedProperty, iItemData.IsSelected);
                    }
                }
            }
            
            if (ItemTemplate != null)
            {
                disposables.Add(BindUtils.RelayBind(this, ItemTemplateProperty, listBoxItem, ListBoxItem.ContentTemplateProperty));
            }
            
            disposables.Add(BindUtils.RelayBind(this, SizeTypeProperty, listBoxItem, ListBoxItem.SizeTypeProperty));
            disposables.Add(BindUtils.RelayBind(this, IsMotionEnabledProperty, listBoxItem, ListBoxItem.IsMotionEnabledProperty));
            disposables.Add(BindUtils.RelayBind(this, SelectedIndicatorProperty, listBoxItem, ListBoxItem.SelectedIndicatorProperty));
            disposables.Add(BindUtils.RelayBind(this, ItemHoverBgProperty, listBoxItem, ListBoxItem.ItemHoverBgProperty));
            disposables.Add(BindUtils.RelayBind(this, ItemSelectedBgProperty, listBoxItem, ListBoxItem.ItemSelectedBgProperty));
            disposables.Add(BindUtils.RelayBind(this, IsShowSelectedIndicatorProperty, listBoxItem, ListBoxItem.IsShowSelectedIndicatorProperty));
            disposables.Add(BindUtils.RelayBind(this, ItemFilterHighlightStrategyProperty, listBoxItem, ListBoxItem.FilterHighlightStrategyProperty));
            disposables.Add(BindUtils.RelayBind(this, FilterHighlightForegroundProperty, listBoxItem, ListBoxItem.FilterHighlightForegroundProperty));
            
            PrepareListBoxItem(listBoxItem, item, index, disposables);
            
            if (_itemsBindingDisposables.TryGetValue(listBoxItem, out var oldDisposables))
            {
                oldDisposables.Dispose();
                _itemsBindingDisposables.Remove(listBoxItem);
            }
            _itemsBindingDisposables.Add(listBoxItem, disposables);
        }
        else
        {
            throw new ArgumentOutOfRangeException(nameof(container), "The container type is incorrect, it must be type ListBoxItem.");
        }
    }
    
    protected virtual void PrepareListBoxItem(ListBoxItem listBoxItem, object? item, int index, CompositeDisposable compositeDisposable)
    {
    }
    
    protected virtual void ConfigureEmptyIndicator()
    {
        SetCurrentValue(IsEffectiveEmptyVisibleProperty, IsShowEmptyIndicator && (ItemCount == 0 || (IsFiltering && FilterResultCount == 0)));
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
    
    internal bool UpdateSelectionFromPointerEvent(Control source, PointerEventArgs e)
    {
        if (IsItemSelectable)
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

    protected bool FilterItem(ListBoxItem item)
    {
        if (ItemFilter == null)
        {
            return false;
        }
        return ItemFilter.Filter(this, item, ItemFilterValue);
    }

    private void FilterItems()
    {
        if (ItemFilter != null && ItemFilterValue != null && IsLoaded)
        {
            if (ItemFilterHighlightStrategy.HasFlag(TextBlockHighlightStrategy.HideUnMatched))
            {
                if (_filterContext.Count == 0)
                {
                    foreach (var item in Items)
                    {
                        if (item != null)
                        {
                            var container = ContainerFromItem(item);
                            if (container is ListBoxItem listBoxItem)
                            {
                                _filterContext[listBoxItem] = listBoxItem.IsVisible;
                            }
                        }
                    }
                }
            }
            IsFiltering = true;
            var count = 0;
            foreach (var item in Items)
            {
                if (item != null)
                {
                    var container    = ContainerFromItem(item);
                    if (container is ListBoxItem listBoxItem)
                    {
                        var filterResult = FilterItem(listBoxItem);
                        if (filterResult)
                        {
                            ++count;
                        }

                        if (ItemFilterHighlightStrategy.HasFlag(TextBlockHighlightStrategy.HideUnMatched))
                        {
                            listBoxItem.SetCurrentValue(ListBoxItem.IsVisibleProperty, filterResult);
                        }
                       
                        listBoxItem.IsFiltering = true;
                        listBoxItem.FilterValue = ItemFilterValue;
                    }
                }
            }
            FilterResultCount = count;
        }
        else
        {
            ClearFilter();
        }
    }

    private void ClearFilter()
    {
        foreach (var item in Items)
        {
            if (item != null)
            {
                var container = ContainerFromItem(item);
                if (container is ListBoxItem listBoxItem)
                {
                    if (_filterContext.TryGetValue(listBoxItem, out bool value))
                    {
                        listBoxItem.SetCurrentValue(ListBoxItem.IsVisibleProperty, value);
                    }
                    listBoxItem.IsFiltering = false;
                    listBoxItem.FilterValue = null;
                }
            }
        }
        IsFiltering = false;
        _filterContext.Clear();
    }
    
    private void HandleListBoxItemClicked(RoutedEventArgs args)
    {
        if (args.Source is ListBoxItem item)
        {
            NotifyListBoxItemClicked(item);
            ItemClicked?.Invoke(this, new ListBoxItemClickedEventArgs(item));
        }
    }
    
    protected virtual void NotifyListBoxItemClicked(ListBoxItem item)
    {
    }
}