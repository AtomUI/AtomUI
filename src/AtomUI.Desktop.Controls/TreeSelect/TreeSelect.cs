using System.Collections;
using System.Reactive.Disposables.Fluent;
using AtomUI.Controls;
using AtomUI.Desktop.Controls.Primitives;
using AtomUI.Desktop.Controls.Themes;
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

public class TreeSelect : AbstractSelect, IControlSharedTokenResourcesHost
{
    #region 公共属性定义
    
    public static readonly StyledProperty<TreeSelectCheckedStrategy> ShowCheckedStrategyProperty =
        AvaloniaProperty.Register<TreeSelect, TreeSelectCheckedStrategy>(
            nameof(ShowCheckedStrategy), TreeSelectCheckedStrategy.All);
    
    public static readonly StyledProperty<bool> AutoScrollToSelectedItemProperty =
        SelectingItemsControl.AutoScrollToSelectedItemProperty.AddOwner<TreeSelect>();

    public static readonly StyledProperty<bool> IsShowIconProperty =
        TreeView.IsShowIconProperty.AddOwner<TreeSelect>();
    
    public static readonly StyledProperty<bool> IsShowLeafIconProperty =
        TreeView.IsShowLeafIconProperty.AddOwner<TreeSelect>();
    
    public static readonly StyledProperty<bool> IsShowLineProperty =
        TreeView.IsShowLineProperty.AddOwner<TreeSelect>();
    
    public static readonly StyledProperty<bool> IsTreeCheckableProperty =
        AvaloniaProperty.Register<TreeSelect, bool>(
            nameof(IsTreeCheckable));
    
    public static readonly StyledProperty<bool> IsMultipleProperty =
        AvaloniaProperty.Register<TreeSelect, bool>(
            nameof(IsMultiple));
    
    public static readonly StyledProperty<bool> IsTreeDefaultExpandAllProperty =
        AvaloniaProperty.Register<TreeSelect, bool>(
            nameof(IsTreeDefaultExpandAll));
    
    public static readonly StyledProperty<bool> IsShowTreeLineProperty =
        AvaloniaProperty.Register<TreeSelect, bool>(
            nameof(IsShowTreeLine));
    
    public static readonly StyledProperty<bool> IsSwitcherRotationProperty = 
        TreeView.IsSwitcherRotationProperty.AddOwner<TreeSelect>();
    
    public static readonly StyledProperty<bool> IsTreeCheckStrictlyProperty = 
        AvaloniaProperty.Register<TreeView, bool>(nameof(IsTreeCheckStrictly), false);
    
    public static readonly DirectProperty<TreeSelect, IList<TreeNodePath>?> TreeDefaultExpandedPathsProperty =
        AvaloniaProperty.RegisterDirect<TreeSelect, IList<TreeNodePath>?>(
            nameof(TreeDefaultExpandedPaths),
            o => o.TreeDefaultExpandedPaths,
            (o, v) => o.TreeDefaultExpandedPaths = v);
    
    public static readonly StyledProperty<IEnumerable?> ItemsSourceProperty =
        AvaloniaProperty.Register<TreeSelect, IEnumerable?>(nameof(ItemsSource));
    
    public static readonly StyledProperty<IDataTemplate?> ItemTemplateProperty =
        AvaloniaProperty.Register<TreeSelect, IDataTemplate?>(nameof(ItemTemplate));
    
    public static readonly DirectProperty<TreeSelect, ITreeItemDataLoader?> DataLoaderProperty =
        AvaloniaProperty.RegisterDirect<TreeSelect, ITreeItemDataLoader?>(
            nameof(DataLoader),
            o => o.DataLoader,
            (o, v) => o.DataLoader = v);
    
    public static readonly DirectProperty<TreeSelect, ITreeItemFilter?> FilterProperty =
        AvaloniaProperty.RegisterDirect<TreeSelect, ITreeItemFilter?>(
            nameof(Filter),
            o => o.Filter,
            (o, v) => o.Filter = v);
    
    public static readonly DirectProperty<TreeSelect, TreeFilterHighlightStrategy> FilterHighlightStrategyProperty =
        AvaloniaProperty.RegisterDirect<TreeSelect, TreeFilterHighlightStrategy>(
            nameof(FilterHighlightStrategy),
            o => o.FilterHighlightStrategy,
            (o, v) => o.FilterHighlightStrategy = v);
    
    public static readonly StyledProperty<IBrush?> FilterHighlightForegroundProperty =
        AvaloniaProperty.Register<TreeSelect, IBrush?>(nameof(FilterHighlightForeground));
    
    public static readonly DirectProperty<TreeSelect, object?> SelectedItemProperty =
        SelectingItemsControl.SelectedItemProperty.AddOwner<TreeSelect>(
            o => o.SelectedItem,
            (o, v) => o.SelectedItem = v);
    
    public static readonly DirectProperty<TreeSelect, IList?> SelectedItemsProperty =
        AvaloniaProperty.RegisterDirect<TreeSelect, IList?>(
            nameof(SelectedItems),
            o => o.SelectedItems,
            (o, v) => o.SelectedItems = v);
    
    public TreeSelectCheckedStrategy ShowCheckedStrategy
    {
        get => GetValue(ShowCheckedStrategyProperty);
        set => SetValue(ShowCheckedStrategyProperty, value);
    }
    
    public bool AutoScrollToSelectedItem
    {
        get => GetValue(AutoScrollToSelectedItemProperty);
        set => SetValue(AutoScrollToSelectedItemProperty, value);
    }
    
    public bool IsShowIcon
    {
        get => GetValue(IsShowIconProperty);
        set => SetValue(IsShowIconProperty, value);
    }
    
    public bool IsShowLeafIcon
    {
        get => GetValue(IsShowLeafIconProperty);
        set => SetValue(IsShowLeafIconProperty, value);
    }
    
    public bool IsShowLine
    {
        get => GetValue(IsShowLineProperty);
        set => SetValue(IsShowLineProperty, value);
    }
    
    public bool IsTreeCheckable
    {
        get => GetValue(IsTreeCheckableProperty);
        set => SetValue(IsTreeCheckableProperty, value);
    }
    
    public bool IsMultiple
    {
        get => GetValue(IsMultipleProperty);
        set => SetValue(IsMultipleProperty, value);
    }
    
    public bool IsTreeDefaultExpandAll
    {
        get => GetValue(IsTreeDefaultExpandAllProperty);
        set => SetValue(IsTreeDefaultExpandAllProperty, value);
    }
    
    public bool IsShowTreeLine
    {
        get => GetValue(IsShowTreeLineProperty);
        set => SetValue(IsShowTreeLineProperty, value);
    }
    
    public bool IsSwitcherRotation
    {
        get => GetValue(IsSwitcherRotationProperty);
        set => SetValue(IsSwitcherRotationProperty, value);
    }
    
    public bool IsTreeCheckStrictly
    {
        get => GetValue(IsTreeCheckStrictlyProperty);
        set => SetValue(IsTreeCheckStrictlyProperty, value);
    }
    
    private IList<TreeNodePath>? _treeDefaultExpandedPaths;
    
    public IList<TreeNodePath>? TreeDefaultExpandedPaths
    {
        get => _treeDefaultExpandedPaths;
        set => SetAndRaise(TreeDefaultExpandedPathsProperty, ref _treeDefaultExpandedPaths, value);
    }
    
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
    
    private ITreeItemDataLoader? _dataLoader;
    
    public ITreeItemDataLoader? DataLoader
    {
        get => _dataLoader;
        set => SetAndRaise(DataLoaderProperty, ref _dataLoader, value);
    }
    
    private ITreeItemFilter? _filter;
    
    public ITreeItemFilter? Filter
    {
        get => _filter;
        set => SetAndRaise(FilterProperty, ref _filter, value);
    }

    private TreeFilterHighlightStrategy _itemFilterAction = TreeFilterHighlightStrategy.HighlightedWhole | TreeFilterHighlightStrategy.BoldedMatch | TreeFilterHighlightStrategy.ExpandPath | TreeFilterHighlightStrategy.HideUnMatched;
    
    public TreeFilterHighlightStrategy FilterHighlightStrategy
    {
        get => _itemFilterAction;
        set => SetAndRaise(FilterHighlightStrategyProperty, ref _itemFilterAction, value);
    }
    
    public IBrush? FilterHighlightForeground
    {
        get => GetValue(FilterHighlightForegroundProperty);
        set => SetValue(FilterHighlightForegroundProperty, value);
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

    #endregion
    
    #region 内部属性定义
    
    internal static readonly StyledProperty<SelectionMode> TreeViewSelectionModeProperty = 
        AvaloniaProperty.Register<TreeSelect, SelectionMode>(nameof (TreeViewSelectionMode));
    
    internal static readonly DirectProperty<TreeSelect, ItemToggleType> TreeViewToggleTypeProperty =
        AvaloniaProperty.RegisterDirect<TreeSelect, ItemToggleType>(
            nameof(TreeViewToggleType),
            o => o.TreeViewToggleType,
            (o, v) => o.TreeViewToggleType = v);
    
    internal static readonly DirectProperty<TreeSelect, bool> IsTreeViewSelectableProperty =
        AvaloniaProperty.RegisterDirect<TreeSelect, bool>(
            nameof(IsTreeViewSelectable),
            o => o.IsTreeViewSelectable,
            (o, v) => o.IsTreeViewSelectable = v);
    
    internal static readonly DirectProperty<TreeSelect, bool> IsMaxSelectReachedProperty =
        AvaloniaProperty.RegisterDirect<TreeSelect, bool>(nameof(IsMaxSelectReached),
            o => o.IsMaxSelectReached,
            (o, v) => o.IsMaxSelectReached = v);
    
    internal static readonly DirectProperty<TreeSelect, IList?> EffectiveSelectedItemsProperty =
        AvaloniaProperty.RegisterDirect<TreeSelect, IList?>(
            nameof(EffectiveSelectedItems),
            o => o.EffectiveSelectedItems,
            (o, v) => o.EffectiveSelectedItems = v);
    
    internal SelectionMode TreeViewSelectionMode
    {
        get => GetValue(TreeViewSelectionModeProperty);
        set => SetValue(TreeViewSelectionModeProperty, value);
    }
    
    private ItemToggleType _treeViewToggleType = ItemToggleType.None;

    internal ItemToggleType TreeViewToggleType
    {
        get => _treeViewToggleType;
        set => SetAndRaise(TreeViewToggleTypeProperty, ref _treeViewToggleType, value);
    }

    private bool _isTreeViewSelectable = true;

    internal bool IsTreeViewSelectable
    {
        get => _isTreeViewSelectable;
        set => SetAndRaise(IsTreeViewSelectableProperty, ref _isTreeViewSelectable, value);
    }
        
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
    
    Control IControlSharedTokenResourcesHost.HostControl => this;
    string IControlSharedTokenResourcesHost.TokenId => TreeSelectToken.ID;

    #endregion
    
    private SelectFilterTextBox? _singleFilterInput;
    private TreeView? _treeView;
    private bool _needSkipSyncSelection;
    private bool _needSkipCollectionChangedEvent;
    
    static TreeSelect()
    {
        FocusableProperty.OverrideDefaultValue<TreeSelect>(true);
        SelectHandle.ClearRequestedEvent.AddClassHandler<TreeSelect>((target, args) => target.HandleClearRequest());
        TreeViewItem.ClickEvent.AddClassHandler<TreeSelect>((treeSelect, args) =>
        {
            if (args.Source is TreeViewItem item)
            {
                treeSelect.HandleTreeViewItemClicked(item);
            }
        });
        SelectFilterTextBox.TextChangedEvent.AddClassHandler<TreeSelect>((x, e) => x.HandleSearchInputTextChanged(e));
        SelectTag.ClosedEvent.AddClassHandler<TreeSelect>((x, e) => x.HandleTagCloseRequest(e));
    }
    
    public TreeSelect()
    {
        this.RegisterResources();
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        Filter ??= new DefaultTreeItemFilter();
        ConfigureMaxSelectReached();
    }

    private void HandleClearRequest()
    {
        Clear();
    }
    
    public void Clear()
    {
        SelectedItems = null;
        SelectedItem  = null;
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
                 change.Property == SelectedItemProperty)
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
        else if (change.Property == IsMultipleProperty)
        {
            ConfigureTreeSelectionMode();
        }
        else if (change.Property == IsTreeCheckableProperty)
        {
            SetCurrentValue(IsMultipleProperty, true);
            HandleIsCheckableChanged();
        }

        if (change.Property == SelectedItemsProperty)
        {
            SyncSelectedItemsToTreeView();
        }
        else if (change.Property == SelectedItemProperty)
        {
            SyncSelectedItemToTreeView();
        }

        if (change.Property == IsMultipleProperty ||
            change.Property == MaxCountProperty ||
            change.Property == EffectiveSelectedItemsProperty ||
            change.Property == IsTreeCheckableProperty)
        {
            ConfigureMaxSelectReached();
        }
        
        if (change.Property == SelectedItemsProperty ||
            change.Property == ShowCheckedStrategyProperty)
        {
            BuildEffectiveSelectedItems();
        }
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
        SetCurrentValue(IsPlaceholderTextVisibleProperty, SelectedItem == null && (SelectedItems == null || SelectedItems?.Count == 0) && string.IsNullOrEmpty(ActivateFilterValue));
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        
        if (Popup != null)
        {
            Popup.Opened -= HandlePopupOpened;
            Popup.Closed -= HandlePopupClosed;
        }

        if (_treeView != null)
        {
            _treeView.SelectionChanged    -= HandleTreeViewSelectionChanged;
            _treeView.CheckedItemsChanged -= HandleTreeViewItemsCheckedChanged;
        }

        _singleFilterInput = e.NameScope.Find<SelectFilterTextBox>(TreeSelectThemeConstants.SingleFilterInputPart);
        Popup              = e.NameScope.Find<Popup>(TreeSelectThemeConstants.PopupPart);
        _treeView          = e.NameScope.Find<TreeView>(TreeSelectThemeConstants.TreeViewPart);
        
        if (_treeView != null)
        {
            _treeView.SelectionChanged    += HandleTreeViewSelectionChanged;
            _treeView.CheckedItemsChanged += HandleTreeViewItemsCheckedChanged;
        }

        if (Popup != null)
        {
            Popup.ClickHidePredicate =  PopupClosePredicate;
            Popup.Opened             += HandlePopupOpened;
            Popup.Closed             += HandlePopupClosed;
        }
        
        ConfigureSelectionIsEmpty();
        UpdatePseudoClasses();
        ConfigureSingleFilterTextBox();
        ConfigurePlaceholderVisible();
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
            SyncSelectedItemToTreeView();
            _singleFilterInput?.Focus();
        }
    }

    private void HandleTreeViewSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_treeView == null || _needSkipCollectionChangedEvent)
        {
            return;
        }
        
        var needSync = false;

        if (IsMultiple)
        {
            if (_selectedItems == null || _selectedItems?.Count != _treeView.SelectedItems.Count)
            {
                needSync = true;
            }
            else
            {
                var currentSet  = _selectedItems.Cast<object>().ToHashSet();
                var treeViewSet = _treeView.SelectedItems.Cast<object>().ToHashSet();
                if (!currentSet.SetEquals(treeViewSet))
                {
                    needSync = true;
                }
            }
        
            if (needSync)
            {
                try
                {
                    _needSkipSyncSelection = true;
                    SelectedItems          = _treeView.SelectedItems.Cast<object>().ToList();
                }
                finally
                {
                    _needSkipSyncSelection = false;
                }
            }
        }
        else
        {
            try
            {
                _needSkipSyncSelection = true;
                SelectedItem           = _treeView.SelectedItem;
            }
            finally
            {
                _needSkipSyncSelection = false;
            }
        }
    }

    private void HandleTreeViewItemsCheckedChanged(object? sender, TreeViewCheckedItemsChangedEventArgs e)
    {
        if (_treeView == null)
        {
            return;
        }
        
        var needSync = false;

        if (_selectedItems == null || _selectedItems?.Count != _treeView.CheckedItems.Count)
        {
            needSync = true;
        }
        else
        {
            var currentSet  = _selectedItems.Cast<object>().ToHashSet();
            var treeViewSet = _treeView.CheckedItems.Cast<object>().ToHashSet();
            if (!currentSet.SetEquals(treeViewSet))
            {
                needSync = true;
            }
        }
        
        if (needSync)
        {
            try
            {
                _needSkipSyncSelection = true;
                SelectedItems          = _treeView.CheckedItems.Cast<object>().ToList();
            }
            finally
            {
                _needSkipSyncSelection = false;
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
        SetCurrentValue(IsSelectionEmptyProperty, SelectedItem == null && (SelectedItems == null || SelectedItems?.Count == 0));
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

    private void HandleTreeViewItemClicked(TreeViewItem item)
    {
        if (!IsMultiple)
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

    private void ConfigureTreeSelectionMode()
    {
        if (!IsTreeCheckable)
        {
            if (IsMultiple)
            {
                TreeViewSelectionMode = SelectionMode.Multiple | SelectionMode.Toggle;
            }
            else
            {
                TreeViewSelectionMode = SelectionMode.Single;
            }

            TreeViewToggleType   = ItemToggleType.None;
            IsTreeViewSelectable = true;
        }
        else
        {
            TreeViewSelectionMode = SelectionMode.Toggle;
            TreeViewToggleType    = ItemToggleType.CheckBox;
            IsTreeViewSelectable  = false;
        }
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
        ConfigureTreeSelectionMode();
        if (_treeView != null)
        {
            try
            {
                _needSkipSyncSelection = true;
                _treeView.SelectedItem = null;
                _treeView.SelectedItems.Clear();
                _treeView.CheckedItems.Clear();
            }
            finally
            {
                _needSkipSyncSelection = false;
            }
        }
    }
    
    private void HandleTagCloseRequest(RoutedEventArgs e)
    {
        if (!IsMultiple)
        {
            return;
        }
        if (e.Source is SelectTag tag && tag.Item is ITreeViewItemData treeItemData)
        {
            if (SelectedItems != null)
            {
                var selectedItems = new List<object>();
                foreach (var item in SelectedItems)
                {
                    selectedItems.Add(item);
                }
                RemoveItemRecursive(selectedItems, treeItemData);
                SelectedItems = selectedItems;
            }
        }
        e.Handled = true;
    }

    private void RemoveItemRecursive(List<object> items, ITreeViewItemData item)
    {
        foreach (var child in item.Children)
        {
            RemoveItemRecursive(items, child);
        }
        items.Remove(item);
    }

    private void SyncSelectedItemToTreeView()
    {
        if (!_needSkipSyncSelection)
        {
            if (_treeView != null)
            {
                _treeView.SelectedItem = SelectedItem;
            }
        }
    }
    
    private void SyncSelectedItemsToTreeView()
    {
        if (!_needSkipSyncSelection)
        {
            if (_treeView != null)
            {
                if (SelectedItems != null)
                {
                    if (!IsTreeCheckable)
                    {
                        _needSkipCollectionChangedEvent = true;
                        try
                        {
                            foreach (var item in SelectedItems)
                            {
                                if (item is ITreeViewItemData itemData)
                                {
                                    itemData.IsSelected = true;
                                }
                            }
                            if (_treeView.SelectedItems.Count == 0)
                            {
                                foreach (var item in SelectedItems)
                                {
                                    _treeView.SelectedItems.Add(item);
                                }
                            }
                            else
                            {
                                var treeViewSet  = _treeView.SelectedItems.Cast<object>().ToList();
                                var currentSet   = SelectedItems.Cast<object>().ToList();
                                var deletedItems = treeViewSet.Except(currentSet);
                                var addedItems   = currentSet.Except(treeViewSet);
                       
                                foreach (var item in deletedItems)
                                {
                                    _treeView.SelectedItems.Remove(item);
                                }

                                foreach (var item in addedItems)
                                {
                                    _treeView.SelectedItems.Add(item);
                                }
                            }
                        }
                        finally
                        {
                            _needSkipCollectionChangedEvent = false;
                        }
                    }
                    else
                    {
                        if (_treeView.CheckedItems.Count == 0)
                        {
                            foreach (var item in SelectedItems)
                            {
                                _treeView.CheckedItems.Add(item);
                            }
                        }
                        else
                        {
                            var treeViewSet  = _treeView.CheckedItems.Cast<object>().ToList();
                            var currentSet   = SelectedItems.Cast<object>().ToList();
                            var deletedItems = treeViewSet.Except(currentSet);
                            var addedItems   = currentSet.Except(treeViewSet);
                            foreach (var item in deletedItems)
                            {
                                _treeView.CheckedItems.Remove(item);
                            }

                            foreach (var item in addedItems)
                            {
                                _treeView.CheckedItems.Add(item);
                            }
                        }
                    }
                }
                else
                {
                    _treeView.SelectedItems.Clear();
                    _treeView.CheckedItems.Clear();
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
                    var fullySelectedParents = SelectedItems.Cast<ITreeViewItemData>()
                                                            .Where(node => node.Children.All(child => selectedSet.Contains(child)))
                                                            .ToHashSet();
                    foreach (var node in SelectedItems)
                    {
                        if (node is ITreeViewItemData treeViewItemData)
                        {
                            bool isDescendantOfFullySelectedParent = fullySelectedParents
                                .Any(parent => IsDescendantOf(treeViewItemData, parent));
            
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
                        if (item is ITreeViewItemData treeViewItemData)
                        {
                            if (treeViewItemData.Children.Count == 0)
                            {
                                effectiveSelectedItems.Add(treeViewItemData);
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
        
    private bool IsDescendantOf(ITreeViewItemData node, ITreeViewItemData parent)
    {
        if (node == parent)
        {
            return false;
        }
        ITreeNode<ITreeViewItemData>? current = node;
        while (current != null)
        {
            if (current == parent)
            {
                return true;
            }
            current = current.ParentNode;
        }
        return false;
    }
}