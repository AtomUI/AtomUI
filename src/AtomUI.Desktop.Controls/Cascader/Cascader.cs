using System.Collections;
using System.Reactive.Disposables.Fluent;
using AtomUI.Controls;
using AtomUI.Desktop.Controls.DataLoad;
using AtomUI.Desktop.Controls.Primitives;
using AtomUI.Theme;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Metadata;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

public class Cascader : AbstractSelect, IControlSharedTokenResourcesHost
{
    #region 公共属性定义
    public static readonly StyledProperty<TreeSelectCheckedStrategy> ShowCheckedStrategyProperty =
        AvaloniaProperty.Register<Cascader, TreeSelectCheckedStrategy>(
            nameof(ShowCheckedStrategy), TreeSelectCheckedStrategy.All);
    
    public static readonly StyledProperty<bool> IsMultipleProperty =
        AvaloniaProperty.Register<Cascader, bool>(
            nameof(IsMultiple));
    
    public static readonly StyledProperty<IEnumerable?> ItemsSourceProperty =
        ItemsControl.ItemsSourceProperty.AddOwner<Cascader>();
    
    public static readonly StyledProperty<IDataTemplate?> ItemTemplateProperty =
        ItemsControl.ItemTemplateProperty.AddOwner<Cascader>();
    
    public static readonly DirectProperty<Cascader, object?> SelectedItemProperty =
        AvaloniaProperty.RegisterDirect<Cascader, object?>(
            nameof(SelectedItem),
            o => o.SelectedItem,
            (o, v) => o.SelectedItem = v);
    
    public static readonly DirectProperty<Cascader, IList?> SelectedItemsProperty =
        AvaloniaProperty.RegisterDirect<Cascader, IList?>(
            nameof(SelectedItems),
            o => o.SelectedItems,
            (o, v) => o.SelectedItems = v);
    
    public static readonly StyledProperty<IconTemplate?> ExpandIconProperty =
        CascaderView.ExpandIconProperty.AddOwner<Cascader>();
    
    public static readonly StyledProperty<IconTemplate?> LoadingIconProperty =
        CascaderView.LoadingIconProperty.AddOwner<Cascader>();
    
    public static readonly StyledProperty<CascaderViewExpandTrigger> ExpandTriggerProperty =
        CascaderView.ExpandTriggerProperty.AddOwner<Cascader>();

    public static readonly StyledProperty<ICascaderItemDataLoader?> DataLoaderProperty =
        CascaderView.DataLoaderProperty.AddOwner<Cascader>();
    
    public static readonly StyledProperty<ICascaderItemFilter?> FilterProperty =
        CascaderView.FilterProperty.AddOwner<Cascader>();
    
    public static readonly StyledProperty<TextBlockHighlightStrategy> FilterHighlightStrategyProperty =
        CascaderView.FilterHighlightStrategyProperty.AddOwner<Cascader>();
    
    public static readonly StyledProperty<IBrush?> FilterHighlightForegroundProperty =
        CascaderView.FilterHighlightForegroundProperty.AddOwner<Cascader>();
    
    public static readonly StyledProperty<TreeNodePath?> DefaultSelectItemPathProperty =
        AvaloniaProperty.Register<Cascader, TreeNodePath?>(nameof(DefaultSelectItemPath));
    
    public static readonly StyledProperty<bool> IsAllowSelectParentProperty =
        CascaderView.IsAllowSelectParentProperty.AddOwner<Cascader>();
    
    public TreeSelectCheckedStrategy ShowCheckedStrategy
    {
        get => GetValue(ShowCheckedStrategyProperty);
        set => SetValue(ShowCheckedStrategyProperty, value);
    }
    
    public bool IsMultiple
    {
        get => GetValue(IsMultipleProperty);
        set => SetValue(IsMultipleProperty, value);
    }
    
    public IEnumerable? ItemsSource
    {
        get => GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }
    
    [InheritDataTypeFromItems("ItemsSource")]
    public IDataTemplate? ItemTemplate
    {
        get => GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }
    
    private object? _selectedItem;
    
    public object? SelectedItem
    {
        get => _selectedItem;
        set => SetAndRaise(SelectedItemProperty, ref _selectedItem, value);
    }
    
    private IList? _selectedItems;
    
    public IList? SelectedItems
    {
        get => _selectedItems;
        set => SetAndRaise(SelectedItemsProperty, ref _selectedItems, value);
    }
    
    public IconTemplate? ExpandIcon
    {
        get => GetValue(ExpandIconProperty);
        set => SetValue(ExpandIconProperty, value);
    }
    
    public IconTemplate? LoadingIcon
    {
        get => GetValue(LoadingIconProperty);
        set => SetValue(LoadingIconProperty, value);
    }
    
    public CascaderViewExpandTrigger ExpandTrigger
    {
        get => GetValue(ExpandTriggerProperty);
        set => SetValue(ExpandTriggerProperty, value);
    }

    public ICascaderItemDataLoader? DataLoader
    {
        get => GetValue(DataLoaderProperty);
        set => SetValue(DataLoaderProperty, value);
    }
    
    public ICascaderItemFilter? Filter
    {
        get => GetValue(FilterProperty);
        set => SetValue(FilterProperty, value);
    }
    
    public TextBlockHighlightStrategy FilterHighlightStrategy
    {
        get => GetValue(FilterHighlightStrategyProperty);
        set => SetValue(FilterHighlightStrategyProperty, value);
    }
    
    public IBrush? FilterHighlightForeground
    {
        get => GetValue(FilterHighlightForegroundProperty);
        set => SetValue(FilterHighlightForegroundProperty, value);
    }
    
    public TreeNodePath? DefaultSelectItemPath
    {
        get => GetValue(DefaultSelectItemPathProperty);
        set => SetValue(DefaultSelectItemPathProperty, value);
    }
    
    public bool IsAllowSelectParent
    {
        get => GetValue(IsAllowSelectParentProperty);
        set => SetValue(IsAllowSelectParentProperty, value);
    }
    #endregion

    #region 内部属性定义
    
    internal static readonly DirectProperty<Cascader, bool> IsMaxSelectReachedProperty =
        AvaloniaProperty.RegisterDirect<Cascader, bool>(nameof(IsMaxSelectReached),
            o => o.IsMaxSelectReached,
            (o, v) => o.IsMaxSelectReached = v);
    
    internal static readonly DirectProperty<Cascader, IList?> EffectiveSelectedItemsProperty =
        AvaloniaProperty.RegisterDirect<Cascader, IList?>(
            nameof(EffectiveSelectedItems),
            o => o.EffectiveSelectedItems,
            (o, v) => o.EffectiveSelectedItems = v);
    
    internal static readonly DirectProperty<Cascader, string?> SelectedItemPathProperty =
        AvaloniaProperty.RegisterDirect<Cascader, string?>(
            nameof(SelectedItemPath),
            o => o.SelectedItemPath,
            (o, v) => o.SelectedItemPath = v);
    
    private bool _isMaxSelectReached;

    internal bool IsMaxSelectReached
    {
        get => _isMaxSelectReached;
        set => SetAndRaise(IsMaxSelectReachedProperty, ref _isMaxSelectReached, value);
    }
    
    private IList? _effectiveSelectedItems;

    internal IList? EffectiveSelectedItems
    {
        get => _effectiveSelectedItems;
        set => SetAndRaise(EffectiveSelectedItemsProperty, ref _effectiveSelectedItems, value);
    }
    
    private string? _selectItemPath;

    internal string? SelectedItemPath
    {
        get => _selectItemPath;
        set => SetAndRaise(SelectedItemPathProperty, ref _selectItemPath, value);
    }

    Control IControlSharedTokenResourcesHost.HostControl => this;
    string IControlSharedTokenResourcesHost.TokenId => CascaderToken.ID;

    #endregion
    
    private SelectFilterTextBox? _singleFilterInput;
    private CascaderView? _cascaderView;
    private bool _needSkipSyncCheckedItems;

    static Cascader()
    {
        FocusableProperty.OverrideDefaultValue<Cascader>(true);
        SelectHandle.ClearRequestedEvent.AddClassHandler<Cascader>((target, args) => target.HandleClearRequest());
        SelectFilterTextBox.TextChangedEvent.AddClassHandler<Cascader>((x, e) => x.HandleSearchInputTextChanged(e));
        SelectTag.ClosedEvent.AddClassHandler<Cascader>((x, e) => x.HandleTagCloseRequest(e));
    }
    
    public Cascader()
    {
        this.RegisterResources();
        
    }

    private void HandleClearRequest()
    {
        Clear();
    }
    
    public void Clear()
    {
        SelectedItems  = null;
        SelectedItem   = null;
        SelectedItemPath = null;
    }
    
    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(StdPseudoClass.Error, Status == AddOnDecoratedStatus.Error);
        PseudoClasses.Set(StdPseudoClass.Warning, Status == AddOnDecoratedStatus.Warning);
        PseudoClasses.Set(AddOnDecoratedBoxPseudoClass.Outline, StyleVariant == AddOnDecoratedVariant.Outline);
        PseudoClasses.Set(AddOnDecoratedBoxPseudoClass.Filled, StyleVariant == AddOnDecoratedVariant.Filled);
        PseudoClasses.Set(AddOnDecoratedBoxPseudoClass.Borderless, StyleVariant == AddOnDecoratedVariant.Borderless);
    }
    
    private void ConfigurePlaceholderVisible()
    {
        if (IsMultiple)
        {
            SetCurrentValue(IsPlaceholderTextVisibleProperty, (SelectedItems == null || SelectedItems?.Count == 0) && 
                                                              string.IsNullOrWhiteSpace(ActivateFilterValue) &&
                                                              string.IsNullOrWhiteSpace(SelectedItemPath));
        }
        else
        {
            SetCurrentValue(IsPlaceholderTextVisibleProperty, SelectedItem == null && 
                                                              string.IsNullOrWhiteSpace(ActivateFilterValue) &&
                                                              string.IsNullOrWhiteSpace(SelectedItemPath));
        }
       
    }
    
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IsDropDownOpenProperty)
        {
            PseudoClasses.Set(SelectPseudoClass.DropdownOpen, change.GetNewValue<bool>());
            ConfigureSingleFilterTextBox();
        }
        if (change.Property == StyleVariantProperty ||
            change.Property == StatusProperty)
        {
            UpdatePseudoClasses();
        }
        else if (change.Property == IsPopupMatchSelectWidthProperty)
        {
            ConfigurePopupMinWith(DesiredSize.Width);
        }
        else if (change.Property == SelectedItemsProperty ||
                 change.Property == SelectedItemProperty ||
                 change.Property == SelectedItemPathProperty ||
                 change.Property == IsMultipleProperty)
        {
            ConfigurePlaceholderVisible();
            ConfigureSelectionIsEmpty();
            if (IsMultiple)
            {
                SetCurrentValue(SelectedCountProperty, SelectedItems?.Count ?? 0);
            }
            else
            {
                SetCurrentValue(SelectedCountProperty, SelectedItem != null ? 1 : 0);
            }
        }
        
        if (change.Property == IsMultipleProperty)
        {
            HandleIsCheckableChanged();
        }
        
        if (change.Property == SelectedItemsProperty ||
            change.Property == IsMultipleProperty)
        {
            SyncSelectedItemsToCascaderView();
        }

        if (change.Property == IsMultipleProperty ||
            change.Property == MaxCountProperty ||
            change.Property == EffectiveSelectedItemsProperty ||
            change.Property == IsMultipleProperty)
        {
            ConfigureMaxSelectReached();
        }
        
        if (change.Property == SelectedItemsProperty ||
            change.Property == ShowCheckedStrategyProperty)
        {
            BuildEffectiveSelectedItems();
        }

        if (change.Property == SelectedItemProperty)
        {
            ConfigureSelectedItemPath();
        }
    }
    
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        
        if (Popup != null)
        {
            Popup.Opened -= HandlePopupOpened;
            Popup.Closed -= HandlePopupClosed;
        }
        
        if (_cascaderView != null)
        {
            _cascaderView.CheckedItemsChanged -= HandleCascaderViewItemsCheckedChanged;
            _cascaderView.ItemClicked         -= HandleCascaderViewItemClicked;
            _cascaderView.ItemDoubleClicked   -= HandleCascaderViewItemDoubleClicked;
        }
        
        _singleFilterInput = e.NameScope.Find<SelectFilterTextBox>(CascaderThemeConstants.SingleFilterInputPart);
        Popup              = e.NameScope.Find<Popup>(CascaderThemeConstants.PopupPart);
        _cascaderView      = e.NameScope.Find<CascaderView>(CascaderThemeConstants.CascaderViewPart);
        
        if (_cascaderView != null)
        {
            _cascaderView.CheckedItemsChanged += HandleCascaderViewItemsCheckedChanged;
            _cascaderView.ItemClicked         += HandleCascaderViewItemClicked;
            _cascaderView.ItemDoubleClicked   += HandleCascaderViewItemDoubleClicked;
        }
        
        if (Popup != null)
        {
            Popup.ClickHidePredicate =  PopupClosePredicate;
            Popup.Opened             += HandlePopupOpened;
            Popup.Closed             += HandlePopupClosed;
        }
        
        ConfigurePlaceholderVisible();
        ConfigureSelectionIsEmpty();
        UpdatePseudoClasses();
        ConfigureSingleFilterTextBox();
        UpdatePseudoClasses();
    }
    
    private void HandlePopupClosed(object? sender, EventArgs e)
    {
        SubscriptionsOnOpen.Clear();
        NotifyPopupClosed();
        if (!IsMultiple)
        {
            if (_singleFilterInput != null)
            {
                _singleFilterInput.Clear();
                _singleFilterInput.Width = double.NaN;
                ActivateFilterValue      = null;
            }
        }
    }

    private void HandlePopupOpened(object? sender, EventArgs e)
    {
        SubscriptionsOnOpen.Clear();
        this.GetObservable(IsVisibleProperty).Subscribe(HandleIsVisibleChanged).DisposeWith(SubscriptionsOnOpen);
        foreach (var parent in this.GetVisualAncestors().OfType<Control>())
        {
            parent.GetObservable(IsVisibleProperty).Subscribe(HandleIsVisibleChanged).DisposeWith(SubscriptionsOnOpen);
        }
        NotifyPopupOpened();
        if (!IsMultiple)
        {
            _singleFilterInput?.Focus();
        }
    }

    private void HandleCascaderViewItemsCheckedChanged(object? sender, CascaderViewCheckedItemsChangedEventArgs e)
    {
        if (_cascaderView == null)
        {
            return;
        }
        
        var needSync = false;

        if (_selectedItems == null || _selectedItems?.Count != _cascaderView.CheckedItems.Count)
        {
            needSync = true;
        }
        else
        {
            var currentSet  = _selectedItems.Cast<object>().ToHashSet();
            var cascaderViewSet = _cascaderView.CheckedItems.Cast<object>().ToHashSet();
            if (!currentSet.SetEquals(cascaderViewSet))
            {
                needSync = true;
            }
        }
        
        if (needSync)
        {
            try
            {
                _needSkipSyncCheckedItems = true;
                SelectedItems             = _cascaderView.CheckedItems.Cast<object>().ToList();
            }
            finally
            {
                _needSkipSyncCheckedItems = false;
            }
        }
    }

    private void HandleIsVisibleChanged(bool isVisible)
    {
        if (!isVisible && IsDropDownOpen)
        {
            SetCurrentValue(IsDropDownOpenProperty, false);
        }
    }
    
    private void ConfigureSelectionIsEmpty()
    {
        if (IsMultiple)
        {
            SetCurrentValue(IsSelectionEmptyProperty, SelectedItems == null || SelectedItems?.Count == 0);
        }
        else
        {
            SetCurrentValue(IsSelectionEmptyProperty, SelectedItem == null);
        }
    }
    
    private void ConfigureSingleFilterTextBox()
    {
        if (_singleFilterInput != null)
        {
            if (IsDropDownOpen)
            {
                _singleFilterInput.Width = _singleFilterInput.Bounds.Width;
            }
        }
    }
    
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        
        if(!e.Handled && e.Source is Visual source)
        {
            if (Popup?.IsInsidePopup(source) == true)
            {
                e.Handled = true;
                return;
            }
        }
        if (IsDropDownOpen)
        {
            // When a drop-down is open with OverlayDismissEventPassThrough enabled and the control
            // is pressed, close the drop-down
            if (e.Source is Control sourceControl)
            {
                var filterTextBox = sourceControl.FindAncestorOfType<SelectFilterTextBox>();
                if (filterTextBox != null)
                {
                    IgnorePopupClose = true;
                    return;
                }
       
                var parent = sourceControl.FindAncestorOfType<IconButton>();
                var tag    = parent?.FindAncestorOfType<SelectTag>();
                if (tag != null)
                {
                    IgnorePopupClose = true;
                }
                
            }
            else
            {
                SetCurrentValue(IsDropDownOpenProperty, false); 
                e.Handled = true;
            }
        }
        else
        {
            PseudoClasses.Set(StdPseudoClass.Pressed, true);
        }
    }
    
    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        if (!e.Handled && e.Source is Visual source)
        {
            if (Popup?.IsInsidePopup(source) == true)
            {
                e.Handled = true;
            }
            else if (PseudoClasses.Contains(StdPseudoClass.Pressed))
            {
                var clickInTagCloseButton = false;
                if (e.Source is Control sourceControl)
                {
                    var parent = sourceControl.FindAncestorOfType<IconButton>();
                    var tag    = parent?.FindAncestorOfType<SelectTag>();
                    if (tag != null)
                    {
                        clickInTagCloseButton = true;
                    }
                }
    
                if (!clickInTagCloseButton)
                {
                    SetCurrentValue(IsDropDownOpenProperty, !IsDropDownOpen);
                }
                e.Handled = true;
            }
        }
    
        PseudoClasses.Set(StdPseudoClass.Pressed, false);
        base.OnPointerReleased(e);
    }

    private void HandleCascaderViewItemClicked(object? sender, CascaderItemClickedEventArgs eventArgs)
    {
        if (!IsMultiple && 
            eventArgs.Item.DataContext is ICascaderViewItemData itemData && 
            itemData.Children.Count == 0)
        {
            if (DataLoader != null)
            {
                if (eventArgs.Item.AsyncLoaded || itemData.IsLeaf)
                {
                    SetCurrentValue(IsDropDownOpenProperty, false);
                }
            }
            else
            {
                SetCurrentValue(IsDropDownOpenProperty, false);
            }
        }
    }
    
    private void HandleCascaderViewItemDoubleClicked(object? sender, CascaderItemDoubleClickedEventArgs eventArgs)
    {
        if (!IsMultiple && eventArgs.Item.DataContext is ICascaderViewItemData && IsAllowSelectParent)
        {
            SetCurrentValue(IsDropDownOpenProperty, false);
        }
    }
    
    private void HandleSearchInputTextChanged(TextChangedEventArgs e)
    {
        if (Filter != null)
        {
            if (e.Source is TextBox textBox)
            {
                ActivateFilterValue = textBox.Text?.Trim();
            }
            ConfigurePlaceholderVisible();
        }

        e.Handled = true;
    }

    private void ConfigureMaxSelectReached()
    {
        if (IsMultiple)
        {
            IsMaxSelectReached = MaxCount <= SelectedItems?.Count;
        }
        else
        {
            IsMaxSelectReached = false;
        }
    }

    private void HandleIsCheckableChanged()
    {
        if (_cascaderView != null)
        {
            try
            {
                _needSkipSyncCheckedItems = true;
                _cascaderView.SelectedItem = null;
                _cascaderView.CheckedItems.Clear();
            }
            finally
            {
                _needSkipSyncCheckedItems = false;
            }
        }
    }
    
    private void HandleTagCloseRequest(RoutedEventArgs e)
    {
        if (!IsMultiple)
        {
            return;
        }
        if (e.Source is SelectTag tag && tag.Item is ICascaderViewItemData treeItemData)
        {
            if (SelectedItems != null)
            {
                var selectedItems = new List<object>();
                foreach (var item in SelectedItems)
                {
                    selectedItems.Add(item);
                }
                selectedItems.Remove(treeItemData);
                SelectedItems = selectedItems;
            }
        }
        e.Handled = true;
    }
    
    private void SyncSelectedItemsToCascaderView()
    {
        if (!_needSkipSyncCheckedItems)
        {
            if (_cascaderView != null)
            {
                if (SelectedItems != null && IsMultiple)
                {
                    if (_cascaderView.CheckedItems.Count == 0)
                    {
                        foreach (var item in SelectedItems)
                        {
                            _cascaderView.CheckedItems.Add(item);
                        }
                    }
                    else
                    {
                        var cascaderViewSet  = _cascaderView.CheckedItems.Cast<object>().ToList();
                        var currentSet   = SelectedItems.Cast<object>().ToList();
                        var deletedItems = cascaderViewSet.Except(currentSet);
                        var addedItems   = currentSet.Except(cascaderViewSet);
                        foreach (var item in deletedItems)
                        {
                            _cascaderView.CheckedItems.Remove(item);
                        }
    
                        foreach (var item in addedItems)
                        {
                            _cascaderView.CheckedItems.Add(item);
                        }
                    }
                }
                else
                {
                    _cascaderView.SelectedItem = null;
                    _cascaderView.CheckedItems.Clear();
                }
            }
        }
    }
    
    private void BuildEffectiveSelectedItems()
    {
        if (SelectedItems != null)
        {
            if (ShowCheckedStrategy == TreeSelectCheckedStrategy.All)
            {
                EffectiveSelectedItems = SelectedItems;
            }
            else
            {
                var effectiveSelectedItems = new List<object>();
                if (ShowCheckedStrategy.HasFlag(TreeSelectCheckedStrategy.ShowParent))
                {
                    var selectedSet = SelectedItems.Cast<object>().ToHashSet();
                    var fullySelectedParents = SelectedItems.Cast<ICascaderViewItemData>()
                                                            .Where(node => node.Children.All(child => selectedSet.Contains(child)))
                                                            .ToHashSet();
                    foreach (var node in SelectedItems)
                    {
                        if (node is ICascaderViewItemData cascaderViewItemData)
                        {
                            bool isDescendantOfFullySelectedParent = fullySelectedParents
                                .Any(parent => IsDescendantOf(cascaderViewItemData, parent));
            
                            if (!isDescendantOfFullySelectedParent)
                            {
                                effectiveSelectedItems.Add(node);
                            }
                        }
                    }
                }
                if (ShowCheckedStrategy.HasFlag(TreeSelectCheckedStrategy.ShowChild))
                {
                    foreach (var item in SelectedItems)
                    {
                        if (item is ICascaderViewItemData cascaderViewItemData)
                        {
                            if (cascaderViewItemData.Children.Count == 0)
                            {
                                effectiveSelectedItems.Add(cascaderViewItemData);
                            }
                        }
                    }
                }
                EffectiveSelectedItems = effectiveSelectedItems;
            }
        }
        else
        {
            EffectiveSelectedItems = null;
        }
    }
        
    private bool IsDescendantOf(ICascaderViewItemData node, ICascaderViewItemData parent)
    {
        if (node == parent)
        {
            return false;
        }
        ICascaderViewItemData? current = node;
        while (current != null)
        {
            if (current == parent)
            {
                return true;
            }
            current = current.ParentNode as ICascaderViewItemData;
        }
        return false;
    }

    private void ConfigureSelectedItemPath()
    {
        if (!IsMultiple)
        {
            if (SelectedItem == null)
            {
                SelectedItemPath = null;
            }
            else
            {
                var current = SelectedItem as ICascaderViewItemData;
                var parts   = new List<string>();
                while (current != null)
                {
                    parts.Add(current.Header?.ToString() ?? string.Empty);
                    current = current.ParentNode as ICascaderViewItemData;
                }

                parts.Reverse();
                SelectedItemPath = string.Join('/', parts);
            }
        }
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        if (DefaultSelectItemPath != null && SelectedItemPath == null)
        {
            if (_cascaderView != null)
            {
                if (_cascaderView.TryParseSelectPath(DefaultSelectItemPath, out var nodes))
                {
                    var parts = new List<string>();
                    foreach (var node in nodes)
                    {
                        if (node is ICascaderViewItemData cascaderViewItemData)
                        {
                            parts.Add(cascaderViewItemData.Header?.ToString() ?? string.Empty);
                        }
                    }
                    SetCurrentValue(SelectedItemPathProperty, string.Join("/", parts));
                }
            }
        }
    }
}