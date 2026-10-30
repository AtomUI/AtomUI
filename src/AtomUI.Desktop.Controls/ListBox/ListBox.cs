using AtomUI.Animations;
using System.Collections.Specialized;
using AtomUI.Controls;
using AtomUI.Controls.Data;
using AtomUI.Controls.Utils;
using AtomUI.Generated.AtomUIDesktopControls;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Metadata;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

using AvaloniaListBox = Avalonia.Controls.ListBox;

public partial class ListBox : AvaloniaListBox,
                       ICustomizableSizeTypeAware,
                       IMotionAwareControl,
                       IListVirtualizingContextAware
{
    #region 公共属性定义
    public static readonly StyledProperty<bool> IsSelectableProperty =
        AvaloniaProperty.Register<ListBox, bool>(nameof(IsSelectable), true);
    
    public static readonly StyledProperty<CustomizableSizeType> SizeTypeProperty =
        CustomizableSizeTypeControlProperty.SizeTypeProperty.AddOwner<ListBox>();
    
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
    
    public static readonly StyledProperty<IValueFilter?> FilterProperty =
        AvaloniaProperty.Register<ListBox, IValueFilter?>(nameof(Filter));

    public static readonly StyledProperty<object?> FilterValueProperty =
        AvaloniaProperty.Register<ListBox, object?>(nameof(FilterValue));

    public static readonly StyledProperty<DefaultFilterValueSelector?> FilterValueSelectorProperty =
        AvaloniaProperty.Register<ListBox, DefaultFilterValueSelector?>(nameof(FilterValueSelector));

    public static readonly StyledProperty<TextBlockHighlightStrategy> FilterHighlightStrategyProperty =
        AvaloniaProperty.Register<ListBox, TextBlockHighlightStrategy>(nameof(FilterHighlightStrategy), TextBlockHighlightStrategy.All);
    
    public static readonly DirectProperty<ListBox, int> FilterResultCountProperty =
        AvaloniaProperty.RegisterDirect<ListBox, int>(nameof(FilterResultCount),
            o => o.FilterResultCount);
    
    public static readonly DirectProperty<ListBox, bool> IsFilteringProperty =
        AvaloniaProperty.RegisterDirect<ListBox, bool>(nameof(IsFiltering),
            o => o.IsFiltering);
    
    public static readonly StyledProperty<IBrush?> FilterHighlightForegroundProperty =
        AvaloniaProperty.Register<ListBox, IBrush?>(nameof(FilterHighlightForeground));
    
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

    public TextBlockHighlightStrategy FilterHighlightStrategy
    {
        get => GetValue(FilterHighlightStrategyProperty);
        set => SetValue(FilterHighlightStrategyProperty, value);
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
    public event EventHandler<ItemCountChangedEventArgs>? ItemCountChanged;

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

    internal static readonly DirectProperty<ListBox, bool> IsDefaultEmptyIndicatorVisibleProperty =
        AvaloniaProperty.RegisterDirect<ListBox, bool>(
            nameof(IsDefaultEmptyIndicatorVisible),
            o => o.IsDefaultEmptyIndicatorVisible,
            (o, v) => o.IsDefaultEmptyIndicatorVisible = v);
    
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
    
    private protected readonly Dictionary<object, bool> _filterContext = new();
    private protected readonly Dictionary<int, bool> _filterResults = new();
    private protected readonly Dictionary<object, IDictionary<object, object?>> _virtualRestoreContext = new();

    static ListBox()
    {
        ListBoxItem.ClickedEvent.AddClassHandler<ListBox>((list, args) => list.HandleListBoxItemClicked(args));
        IsSelectableProperty.Changed.AddClassHandler<ListBox>((list, args) => list.HandleIsSelectableChanged(args));
        ItemCountProperty.Changed.AddClassHandler<ListBox>((list, args) => list.HandleItemCountChanged());
    }
    
    public ListBox()
    {
        Items.CollectionChanged += HandleItemCollectionChanged;
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        if (Filter == null)
        {
            SetCurrentValue(FilterProperty, ValueFilterFactory.BuildFilter(ValueFilterMode.Contains));
        }

        ConfigureEmptyIndicator();
        ConfigureIsFiltering();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        if (Filter != null && FilterValue != null)
        {
            FilterItems();
        }
    }
    
    private void HandleItemCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        _virtualRestoreContext.Clear();
        ConfigureEmptyIndicator();
        RefreshContainerSplitLines();
        FilterItems();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ItemsSourceProperty ||
            change.Property == IsFilteringProperty ||
            change.Property == FilterResultCountProperty ||
            change.Property == IsShowEmptyIndicatorProperty ||
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
        else if (change.Property == FilterValueProperty ||
                 change.Property == FilterProperty ||
                 change.Property == FilterValueSelectorProperty ||
                 change.Property == FilterHighlightStrategyProperty)
        {
            ConfigureIsFiltering();
            FilterItems();
        }
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
    
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        var listBoxItem = new ListBoxItem();
        if (GetType() == typeof(ListBox))
        {
            listBoxItem.Classes.Add(ListBoxSemanticParts.ItemClass);
        }
        NotifyContainerForItemCreated(listBoxItem, item);
        return listBoxItem;
    }
    
    protected virtual void NotifyContainerForItemCreated(Control container, object? item)
    {
        if (container is ListBoxItem listBoxItem && item != null && item is not Visual)
        {
            if (item is IListItemData itemData)
            {
                NotifyRestoreDefaultContext(listBoxItem, itemData);
            }
        }
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
            if (GetType() == typeof(ListBox))
            {
                listBoxItem.Classes.Add(ListBoxSemanticParts.ItemClass);
            }

            listBoxItem.IsSplitLineVisible = ShouldShowSplitLine(index == ItemCount - 1);
            if (ItemTemplate != null)
            {
                listBoxItem[!ListBoxItem.ContentTemplateProperty] = this[!ItemTemplateProperty];
            }
            else
            {
                listBoxItem.ClearValue(ListBoxItem.ContentTemplateProperty);
            }
            listBoxItem[!ListBoxItem.SizeTypeProperty]                  = this[!SizeTypeProperty];
            listBoxItem[!ListBoxItem.IsMotionEnabledProperty]           = this[!IsMotionEnabledProperty];
            listBoxItem[!ListBoxItem.SelectedIndicatorProperty]         = this[!SelectedIndicatorProperty];
            listBoxItem[!ListBoxItem.ItemHoverBgProperty]               = this[!ItemHoverBgProperty];
            listBoxItem[!ListBoxItem.ItemSelectedBgProperty]            = this[!ItemSelectedBgProperty];
            listBoxItem[!ListBoxItem.IsShowSelectedIndicatorProperty]   = this[!IsShowSelectedIndicatorProperty];
            listBoxItem[!ListBoxItem.FilterHighlightStrategyProperty]   = this[!FilterHighlightStrategyProperty];
            listBoxItem[!ListBoxItem.FilterHighlightForegroundProperty] = this[!FilterHighlightForegroundProperty];
           
            PrepareListBoxItem(listBoxItem, item, index);

            var originMotionEnabled = false;
            try
            {
                originMotionEnabled = listBoxItem.IsMotionEnabled;
                listBoxItem.SetCurrentValue(IsMotionEnabledProperty, false);
                if (item is IListItemData itemData)
                {
                    NotifyRestoreDefaultContext(listBoxItem, itemData);
                }

                if (this is IListVirtualizingContextAware listVirtualizingContextAwareControl &&
                    listBoxItem is IListItemVirtualizingContextAware virtualListItem)
                {
                    virtualListItem.VirtualIndex = index;
                    if (_virtualRestoreContext.TryGetValue(index, out var context))
                    {
                        listVirtualizingContextAwareControl.RestoreVirtualizingContext(listBoxItem, context);
                        _virtualRestoreContext.Remove(index);
                    }
                }

                ApplyCurrentFilterState(listBoxItem, index);
            }
            finally
            {
                listBoxItem.SetCurrentValue(IsMotionEnabledProperty, originMotionEnabled);
            }
        }
        else
        {
            throw new ArgumentOutOfRangeException(nameof(container), "The container type is incorrect, it must be type ListBoxItem.");
        }
    }
    
    protected virtual void PrepareListBoxItem(ListBoxItem listBoxItem, object? item, int index)
    {
    }
    
    protected virtual void ConfigureEmptyIndicator()
    {
        IsEffectiveEmptyVisible        = IsShowEmptyIndicator && (ItemCount == 0 || (IsFiltering && FilterResultCount == 0));
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
        return IsBorderless || !isLastItem;
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
            if (child is not ListBoxItem listBoxItem)
            {
                continue;
            }

            listBoxItem.IsSplitLineVisible = ShouldShowSplitLine(IsLastItem(listBoxItem));
        }
    }
    
    public override bool UpdateSelectionFromEvent(Control container, RoutedEventArgs eventArgs)
    {
        if (!IsSelectable)
        {
            return false;
        }
        return base.UpdateSelectionFromEvent(container, eventArgs);
    }

    protected internal virtual bool UpdateSelectionFromPointerEvent(Control source, PointerEventArgs e)
    {
        return UpdateSelectionFromEvent(source, e);
    }

    protected bool FilterItem(ListBoxItem item)
    {
        return FilterDataItem(item.Content);
    }

    private bool FilterDataItem(object? item)
    {
        if (Filter == null)
        {
            return false;
        }
        var value = SelectFilterValue(item);
        return Filter.Filter(value, FilterValue);
    }

    private object? SelectFilterValue(object? item)
    {
        var content = item is ListBoxItem listBoxItem ? listBoxItem.Content : item;
        var selector = FilterValueSelector;
        if (selector != null)
        {
            return selector(content);
        }
        if (content is IListItemData listItemData)
        {
            return listItemData.Content?.ToString();
        }
        if (content is string header)
        {
            return header;
        }
        return content;
    }

    private void FilterItems()
    {
        if (Filter != null && FilterValue != null && IsLoaded)
        {
            _filterResults.Clear();
            var count = 0;
            for (var index = 0; index < Items.Count; ++index)
            {
                var item = Items[index];
                if (item != null)
                {
                    var filterResult = FilterDataItem(item);
                    _filterResults[index] = filterResult;
                    if (filterResult)
                    {
                        ++count;
                    }
                }
            }
            FilterResultCount = count;
            IsFiltering       = true;
            ApplyFilterResultsToRealizedContainers();
        }
        else
        {
            ClearFilter();
        }
    }

    private void ClearFilter()
    {
        for (var index = 0; index < Items.Count; ++index)
        {
            if (ContainerFromIndex(index) is ListBoxItem listBoxItem)
            {
                ClearFilterState(listBoxItem);
            }
        }
        FilterResultCount = 0;
        IsFiltering       = false;
        _filterContext.Clear();
        _filterResults.Clear();
    }

    private void ApplyFilterResultsToRealizedContainers()
    {
        for (var index = 0; index < Items.Count; ++index)
        {
            if (_filterResults.TryGetValue(index, out var filterResult) &&
                ContainerFromIndex(index) is ListBoxItem listBoxItem)
            {
                ApplyFilterState(listBoxItem, filterResult);
            }
        }
    }

    private void ApplyCurrentFilterState(ListBoxItem listBoxItem, int index)
    {
        if (Filter != null &&
            FilterValue != null &&
            IsLoaded &&
            _filterResults.TryGetValue(index, out var filterResult))
        {
            ApplyFilterState(listBoxItem, filterResult);
        }
        else
        {
            ClearFilterState(listBoxItem);
        }
    }

    private void ApplyFilterState(ListBoxItem listBoxItem, bool filterResult)
    {
        if (IsHideUnmatchedFilterEnabled())
        {
            if (!_filterContext.ContainsKey(listBoxItem))
            {
                _filterContext[listBoxItem] = listBoxItem.IsVisible;
            }
            listBoxItem.SetCurrentValue(ListBoxItem.IsVisibleProperty, filterResult);
        }
        else
        {
            RestoreFilterVisibility(listBoxItem);
        }

        listBoxItem.IsFiltering = true;
        listBoxItem.FilterValue = FilterValue;
    }

    private void ClearFilterState(ListBoxItem listBoxItem)
    {
        RestoreFilterVisibility(listBoxItem);
        listBoxItem.IsFiltering = false;
        listBoxItem.FilterValue = null;
    }

    private void RestoreFilterVisibility(ListBoxItem listBoxItem)
    {
        if (_filterContext.TryGetValue(listBoxItem, out var value))
        {
            listBoxItem.SetCurrentValue(ListBoxItem.IsVisibleProperty, value);
            _filterContext.Remove(listBoxItem);
        }
    }

    private bool IsHideUnmatchedFilterEnabled()
    {
        return (FilterHighlightStrategy & TextBlockHighlightStrategy.HideUnMatched) == TextBlockHighlightStrategy.HideUnMatched;
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
    
    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (IsSelectable)
        {
            var hotkeys = this.GetPlatformSettings()?.HotkeyConfiguration;
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
                     hotkeys is not null && MatchesAnyGesture(hotkeys.SelectAll, e))
            {
                Selection.SelectAll();
                e.Handled = true;
            }
        }

        base.OnKeyDown(e);
    }

    private static bool MatchesAnyGesture(IEnumerable<KeyGesture> gestures, KeyEventArgs e)
    {
        foreach (var gesture in gestures)
        {
            if (gesture.Matches(e))
            {
                return true;
            }
        }
        return false;
    }
    
    private void HandleItemCountChanged()
    {
        ItemCountChanged?.Invoke(this, new ItemCountChangedEventArgs(ItemCount));
    }

    #region 虚拟化上下文管理
    protected sealed override void ClearContainerForItemOverride(Control element)
    {
        if (element is ITransitionInitializationOwner owner)
        {
            owner.TransitionInitialization.Reset();
        }

        var originMotionEnabled = false; 
        try
        {
            if (element is IMotionAwareControl motionAwareControl)
            {
                originMotionEnabled = motionAwareControl.IsMotionEnabled;
                element.SetCurrentValue(IsMotionEnabledProperty, false);
            }
        
            if (this is IListVirtualizingContextAware list && element is IListItemVirtualizingContextAware listItem)
            {
                var context = new Dictionary<object, object?>(1);
                list.SaveVirtualizingContext(element, context);
                _virtualRestoreContext[listItem.VirtualIndex] = context;
                list.ClearContainerValues(element);
            }

            base.ClearContainerForItemOverride(element);
        }
        finally
        {
            if (element is IMotionAwareControl)
            {
                element.SetCurrentValue(IsMotionEnabledProperty, originMotionEnabled);
            }
        }
    }
    
    protected virtual void NotifyRestoreDefaultContext(ListBoxItem item, IListItemData itemData)
    {
        item.SetCurrentValue(ListBoxItem.ContentProperty, itemData);
        item.SetCurrentValue(ListBoxItem.IsEnabledProperty, itemData.IsEnabled);
    }

    protected virtual void NotifyClearContainerForVirtualizingContext(ListBoxItem item)
    {
        _filterContext.Remove(item);
        item.ClearValue(ListBoxItem.ContentProperty);
        item.ClearValue(ListBoxItem.ContentTemplateProperty);
        item.ClearValue(ListBoxItem.IsEnabledProperty);
        item.ClearValue(ListBoxItem.IsVisibleProperty);
        item.IsFiltering          = false;
        item.FilterValue          = null;
        item.FilterHighlightWords = null;
    }
    
    protected virtual void NotifySaveVirtualizingContext(ListBoxItem item, IDictionary<object, object?> context)
    {
        context.Add(ListBoxItem.IsEnabledProperty, item.IsEnabled);
    }

    protected virtual void NotifyRestoreVirtualizingContext(ListBoxItem item, IDictionary<object, object?> context)
    {
        {
            if (context.TryGetValue(ListBoxItem.IsEnabledProperty, out var value))
            {
                if (value is bool isEnabled)
                {
                    item.SetCurrentValue(ListBoxItem.IsEnabledProperty, isEnabled);
                }
            }
        }
    }

    void IListVirtualizingContextAware.SaveVirtualizingContext(Control item, IDictionary<object, object?> context)
    {
        if (item is ListBoxItem listBoxItem)
        {
            ListVirtualizingContextAwareUtils.ExecuteWithinContextClosure(listBoxItem, listItem => NotifySaveVirtualizingContext(listItem, context));
        }
    }

    void IListVirtualizingContextAware.RestoreVirtualizingContext(Control item, IDictionary<object, object?> context)
    {
        if (item is ListBoxItem listBoxItem)
        {
            ListVirtualizingContextAwareUtils.ExecuteWithinContextClosure(listBoxItem, listItem => NotifyRestoreVirtualizingContext(listItem, context));
        }
    }

    void IListVirtualizingContextAware.RestoreDefaultContext(Control item, object defaultContext)
    {
        if (item is ListBoxItem listBoxItem && defaultContext is IListItemData listBoxItemData)
        {
            ListVirtualizingContextAwareUtils.ExecuteWithinContextClosure(listBoxItem, listItem => NotifyRestoreDefaultContext(listItem, listBoxItemData));
        }
    }

    void IListVirtualizingContextAware.ClearContainerValues(Control item)
    {
        if (item is ListBoxItem listBoxItem)
        {
            ListVirtualizingContextAwareUtils.ExecuteWithinContextClosure(listBoxItem, NotifyClearContainerForVirtualizingContext);
        }
    }
    #endregion
}
