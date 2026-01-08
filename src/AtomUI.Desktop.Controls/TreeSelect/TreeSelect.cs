using System.Collections;
using System.Reactive.Disposables.Fluent;
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
            nameof(ShowCheckedStrategy), TreeSelectCheckedStrategy.ShowChild);
    
    public static readonly StyledProperty<bool> AutoScrollToSelectedItemProperty =
        SelectingItemsControl.AutoScrollToSelectedItemProperty.AddOwner<TreeSelect>();
    
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
    
    public static readonly DirectProperty<TreeSelect, IList<TreeNodePath>?> TreeDefaultExpandedPathsProperty =
        AvaloniaProperty.RegisterDirect<TreeSelect, IList<TreeNodePath>?>(
            nameof(TreeDefaultExpandedPaths),
            o => o.TreeDefaultExpandedPaths,
            (o, v) => o.TreeDefaultExpandedPaths = v);
    
    public static readonly StyledProperty<IEnumerable?> TreeItemsSourceProperty =
        AvaloniaProperty.Register<TreeSelect, IEnumerable?>(nameof(TreeItemsSource));
    
    public static readonly StyledProperty<IDataTemplate?> TreeItemTemplateProperty =
        AvaloniaProperty.Register<TreeSelect, IDataTemplate?>(nameof(TreeItemTemplate));
    
    public static readonly DirectProperty<TreeSelect, ITreeItemFilter?> ItemFilterProperty =
        AvaloniaProperty.RegisterDirect<TreeSelect, ITreeItemFilter?>(
            nameof(ItemFilter),
            o => o.ItemFilter,
            (o, v) => o.ItemFilter = v);
    
    public static readonly DirectProperty<TreeSelect, TreeItemFilterAction> ItemFilterActionProperty =
        AvaloniaProperty.RegisterDirect<TreeSelect, TreeItemFilterAction>(
            nameof(ItemFilterAction),
            o => o.ItemFilterAction,
            (o, v) => o.ItemFilterAction = v);
    
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
    
    private IList<TreeNodePath>? _treeDefaultExpandedPaths;
    
    public IList<TreeNodePath>? TreeDefaultExpandedPaths
    {
        get => _treeDefaultExpandedPaths;
        set => SetAndRaise(TreeDefaultExpandedPathsProperty, ref _treeDefaultExpandedPaths, value);
    }
    
    public IEnumerable? TreeItemsSource
    {
        get => GetValue(TreeItemsSourceProperty);
        set => SetValue(TreeItemsSourceProperty, value);
    }
    
    [InheritDataTypeFromItems(nameof(TreeItemsSource))]
    public IDataTemplate? TreeItemTemplate
    {
        get => GetValue(TreeItemTemplateProperty);
        set => SetValue(TreeItemTemplateProperty, value);
    }
    
    private ITreeItemFilter? _itemFilter;
    
    public ITreeItemFilter? ItemFilter
    {
        get => _itemFilter;
        set => SetAndRaise(ItemFilterProperty, ref _itemFilter, value);
    }

    private TreeItemFilterAction _itemFilterAction = TreeItemFilterAction.HighlightedWhole | TreeItemFilterAction.BoldedMatch | TreeItemFilterAction.ExpandPath | TreeItemFilterAction.HideUnMatched;
    
    public TreeItemFilterAction ItemFilterAction
    {
        get => _itemFilterAction;
        set => SetAndRaise(ItemFilterActionProperty, ref _itemFilterAction, value);
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
    
    internal SelectionMode TreeViewSelectionMode
    {
        get => GetValue(TreeViewSelectionModeProperty);
        set => SetValue(TreeViewSelectionModeProperty, value);
    }

    Control IControlSharedTokenResourcesHost.HostControl => this;
    string IControlSharedTokenResourcesHost.TokenId => TreeSelectToken.ID;

    #endregion
    
    private SelectFilterTextBox? _singleFilterInput;
    private TreeView? _treeView;
    private bool _needSkipSyncSelection;
    
    static TreeSelect()
    {
        FocusableProperty.OverrideDefaultValue<TreeSelect>(true);
        SelectHandle.ClearRequestedEvent.AddClassHandler<TreeSelect>((target, args) =>
        {
            target.HandleClearRequest();
        });
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
        if (ItemFilter == null)
        {
            ItemFilter = new DefaultTreeItemFilter();
        }
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
        else if (change.Property == DisplayPageSizeProperty ||
                 change.Property == ItemHeightProperty)
        {
            ConfigureMaxDropdownHeight();
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

        if (change.Property == SelectedItemsProperty)
        {
            SyncSelectedItemsToTreeView();
        }
        else if (change.Property == SelectedItemProperty)
        {
            if (!_needSkipSyncSelection)
            {
                if (_treeView != null)
                {
                    _treeView.SelectedItem = SelectedItem;
                }
            }
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
    
    private void ConfigureMaxDropdownHeight()
    {
        SetCurrentValue(MaxPopupHeightProperty, ItemHeight * DisplayPageSize + PopupContentPadding.Top + PopupContentPadding.Bottom);
    }
    
    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);
        ConfigurePopupMinWith(e.NewSize.Width);
    }

    private void ConfigurePopupMinWith(double selectWidth)
    {
        if (IsPopupMatchSelectWidth)
        {
            SetCurrentValue(EffectivePopupWidthProperty, selectWidth);
        }
        else
        {
            SetCurrentValue(EffectivePopupWidthProperty, double.NaN);
        }
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
            _treeView.SelectionChanged -= HandleTreeViewSelectionChanged;
        }

        _singleFilterInput = e.NameScope.Find<SelectFilterTextBox>(TreeSelectThemeConstants.SingleFilterInputPart);
        Popup              = e.NameScope.Find<Popup>(TreeSelectThemeConstants.PopupPart);
        _treeView          = e.NameScope.Find<TreeView>(TreeSelectThemeConstants.TreeViewPart);
        
        if (_treeView != null)
        {
            _treeView.SelectionChanged += HandleTreeViewSelectionChanged;
        }

        if (Popup != null)
        {
            Popup.ClickHidePredicate =  PopupClosePredicate;
            Popup.Opened             += HandlePopupOpened;
            Popup.Closed             += HandlePopupClosed;
        }

        ConfigureMaxDropdownHeight();
        ConfigurePlaceholderVisible();
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
        this.GetObservable(IsVisibleProperty).Subscribe(IsVisibleChanged).DisposeWith(SubscriptionsOnOpen);
        foreach (var parent in this.GetVisualAncestors().OfType<Control>())
        {
            parent.GetObservable(IsVisibleProperty).Subscribe(IsVisibleChanged).DisposeWith(SubscriptionsOnOpen);
        }
        NotifyPopupOpened();
        if (!IsMultiple)
        {
            _singleFilterInput?.Focus();
        }
    }

    private void HandleTreeViewSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_treeView == null)
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
                SelectedItem          = _treeView.SelectedItem;
            }
            finally
            {
                _needSkipSyncSelection = false;
            }
        }
    }
    
    private void IsVisibleChanged(bool isVisible)
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
                if (!IsMultiple)
                {
                    var filterTextBox = sourceControl.FindAncestorOfType<SelectFilterTextBox>();
                    if (filterTextBox != null)
                    {
                        IgnorePopupClose = true;
                        return;
                    }
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
        if (ItemFilter != null)
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
        if (IsMultiple)
        {
            TreeViewSelectionMode = SelectionMode.Multiple | SelectionMode.Toggle;
        }
        else
        {
            TreeViewSelectionMode = SelectionMode.Single;
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
                    selectedItems.AddRange(item);
                }
                selectedItems.Remove(treeItemData);
                SelectedItems = selectedItems;
            }
        }
        e.Handled = true;
    }

    private void SyncSelectedItemsToTreeView()
    {
        if (!_needSkipSyncSelection)
        {
            if (_treeView != null)
            {
                if (SelectedItems != null)
                {
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
                        var addedItems = currentSet.Except(treeViewSet);
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
                else
                {
                    _treeView.SelectedItems.Clear();
                }
            }
        }
    }
}