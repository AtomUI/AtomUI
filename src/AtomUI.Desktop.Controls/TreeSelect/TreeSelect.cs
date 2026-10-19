using System.Collections;
using System.Collections.Specialized;
using System.Reactive.Disposables;
using AtomUI.Controls;
using AtomUI.Controls.Primitives;
using AtomUI.Controls.Utils;
using AtomUI.Data;
using AtomUI.Generated.AtomUIDesktopControls;
using AtomUI.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Metadata;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

using ItemCollection = AtomUI.Collections.ItemCollection;
using AvaloniaTreeView = Avalonia.Controls.TreeView;

public partial class TreeSelect : AbstractSelect
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

    public static readonly StyledProperty<bool> IsDefaultExpandAllProperty =
        AvaloniaProperty.Register<TreeSelect, bool>(
            nameof(IsDefaultExpandAll));

    public static readonly StyledProperty<bool> IsShowTreeLineProperty =
        AvaloniaProperty.Register<TreeSelect, bool>(
            nameof(IsShowTreeLine));

    public static readonly StyledProperty<bool> IsSwitcherRotationProperty =
        TreeView.IsSwitcherRotationProperty.AddOwner<TreeSelect>();

    public static readonly StyledProperty<bool> IsTreeCheckStrictlyProperty =
        AvaloniaProperty.Register<TreeSelect, bool>(nameof(IsTreeCheckStrictly), false);

    public static readonly DirectProperty<TreeSelect, IList<TreeNodePath>?> TreeDefaultExpandedPathsProperty =
        AvaloniaProperty.RegisterDirect<TreeSelect, IList<TreeNodePath>?>(
            nameof(TreeDefaultExpandedPaths),
            o => o.TreeDefaultExpandedPaths,
            (o, v) => o.TreeDefaultExpandedPaths = v);

    public static readonly StyledProperty<IEnumerable<ITreeItemNode>?> ItemsSourceProperty =
        AvaloniaProperty.Register<TreeSelect, IEnumerable<ITreeItemNode>?>(nameof(ItemsSource));

    public static readonly StyledProperty<IDataTemplate?> ItemTemplateProperty =
        AvaloniaProperty.Register<TreeSelect, IDataTemplate?>(nameof(ItemTemplate));

    public static readonly StyledProperty<ITreeItemNodeLoader?> DataLoaderProperty =
        AvaloniaProperty.Register<TreeSelect, ITreeItemNodeLoader?>(
            nameof(DataLoader));

    public static readonly StyledProperty<IValueFilter?> FilterProperty =
        AvaloniaProperty.Register<TreeSelect, IValueFilter?>(
            nameof(Filter));

    public static readonly StyledProperty<DefaultFilterValueSelector?> FilterValueSelectorProperty =
        AvaloniaProperty.Register<TreeSelect, DefaultFilterValueSelector?>(nameof(FilterValueSelector));

    public static readonly DirectProperty<TreeSelect, TreeFilterStrategy> FilterStrategyProperty =
        AvaloniaProperty.RegisterDirect<TreeSelect, TreeFilterStrategy>(
            nameof(FilterStrategy),
            o => o.FilterStrategy,
            (o, v) => o.FilterStrategy = v);

    public static readonly StyledProperty<IBrush?> FilterHighlightForegroundProperty =
        AvaloniaProperty.Register<TreeSelect, IBrush?>(nameof(FilterHighlightForeground));

    public static readonly DirectProperty<TreeSelect, ITreeItemNode?> SelectedItemProperty =
        AvaloniaProperty.RegisterDirect<TreeSelect, ITreeItemNode?>(
            nameof(SelectedItem),
            o => o.SelectedItem,
            (o, v) => o.SelectedItem = v,
            defaultBindingMode: BindingMode.TwoWay,
            enableDataValidation: true);

    public static readonly DirectProperty<TreeSelect, IList<ITreeItemNode>?> SelectedItemsProperty =
        AvaloniaProperty.RegisterDirect<TreeSelect, IList<ITreeItemNode>?>(
            nameof(SelectedItems),
            o => o.SelectedItems,
            (o, v) => o.SelectedItems = v,
            defaultBindingMode: BindingMode.TwoWay,
            enableDataValidation: true);

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

    public bool IsDefaultExpandAll
    {
        get => GetValue(IsDefaultExpandAllProperty);
        set => SetValue(IsDefaultExpandAllProperty, value);
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

    public IEnumerable<ITreeItemNode>? ItemsSource
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

    public ITreeItemNodeLoader? DataLoader
    {
        get => GetValue(DataLoaderProperty);
        set => SetValue(DataLoaderProperty, value);
    }

    public IValueFilter? Filter
    {
        get => GetValue(FilterProperty);
        set => SetValue(FilterProperty, value);
    }

    public DefaultFilterValueSelector? FilterValueSelector
    {
        get => GetValue(FilterValueSelectorProperty);
        set => SetValue(FilterValueSelectorProperty, value);
    }

    private TreeFilterStrategy _filterStrategy = TreeFilterStrategy.HighlightedWhole |
                                                 TreeFilterStrategy.BoldedMatch |
                                                 TreeFilterStrategy.ExpandPath |
                                                 TreeFilterStrategy.HideUnMatched;

    public TreeFilterStrategy FilterStrategy
    {
        get => _filterStrategy;
        set => SetAndRaise(FilterStrategyProperty, ref _filterStrategy, value);
    }

    public IBrush? FilterHighlightForeground
    {
        get => GetValue(FilterHighlightForegroundProperty);
        set => SetValue(FilterHighlightForegroundProperty, value);
    }

    private ITreeItemNode? _selectedItem;

    public ITreeItemNode? SelectedItem
    {
        get => _selectedItem;
        set => SetAndRaise(SelectedItemProperty, ref _selectedItem, value);
    }

    private IList<ITreeItemNode>? _selectedItems;

    public IList<ITreeItemNode>? SelectedItems
    {
        get => _selectedItems;
        set => SetAndRaise(SelectedItemsProperty, ref _selectedItems, value);
    }

    public ItemsSourceView ItemsView => ItemsSourceView.GetOrCreate(_items);

    [Content]
    public ItemCollection Items => _items;
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

    internal static readonly DirectProperty<TreeSelect, IList<ITreeItemNode>?> EffectiveSelectedItemsProperty =
        AvaloniaProperty.RegisterDirect<TreeSelect, IList<ITreeItemNode>?>(
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

    private IList<ITreeItemNode>? _effectiveSelectedItems;

    internal IList<ITreeItemNode>? EffectiveSelectedItems
    {
        get => _effectiveSelectedItems;
        set => SetAndRaise(EffectiveSelectedItemsProperty, ref _effectiveSelectedItems, value);
    }

    #endregion

    private SelectFilterTextBox? _singleFilterInput;
    private TreeView? _treeView;
    private CompositeDisposable? _selectHandleInputStateBindings;
    private INotifyCollectionChanged? _selectedItemsCollectionChangedSource;
    private bool _needSkipSyncSelection;
    private bool _needSkipCollectionChangedEvent;
    private bool _syncingSingleFilterInputText;
    private readonly ItemCollection _items = new();

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
        SelectFilterTextBox.TextChangedEvent.AddClassHandler<TreeSelect>((view, e) => view.HandleSearchInputTextChanged(e));
        SelectTag.ClosedEvent.AddClassHandler<TreeSelect>((view, e) => view.HandleTagCloseRequest(e));
        ItemsSourceProperty.Changed.AddClassHandler<TreeSelect>((view, args) => view.HandleItemsSourceChanged(args));
        SelectedItemProperty.Changed.AddClassHandler<TreeSelect>((view, args) =>
            view.HandleSelectedItemChanged(args));
        SelectedItemsProperty.Changed.AddClassHandler<TreeSelect>((view, args) =>
            view.HandleSelectedItemsChanged(args));
    }

    public TreeSelect()
    {
        Items.CollectionChanged += HandleItemsChanged;
    }

    public void Clear()
    {
        SelectedItems = null;
        SelectedItem  = null;
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        if (Filter == null)
        {
            SetCurrentValue(FilterProperty, ValueFilterFactory.BuildFilter(ValueFilterMode.Contains));
        }
        ConfigureMaxSelectReached();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        ConfigureSelectedItemsCollectionChangedSource(SelectedItems);
        ConfigureSelectedItemsValueState();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        ConfigureSingleFilterTextBox();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        ReleaseSelectedItemsCollectionChangedSource();
        ClearPopupContent();
        base.OnDetachedFromVisualTree(e);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IsDropDownOpenProperty)
        {
            ConfigureSingleResultVisible();
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
            ConfigureSingleResultVisible();
            ConfigureSelectionIsEmpty();
            if (IsMultiple)
            {
                SetCurrentValue(SelectedCountProperty, SelectedItems?.Count ?? 0);
            }
            else
            {
                SetCurrentValue(SelectedCountProperty, SelectedItem != null ? 1 : 0);
            }
            ConfigureSingleFilterTextBox();
        }
        else if (change.Property == IsMultipleProperty)
        {
            ConfigureTreeSelectionMode();
            ConfigureSingleResultVisible();
            ConfigureSingleFilterTextBox();
        }
        else if (change.Property == FilterValueProperty)
        {
            ConfigurePlaceholderVisible();
            ConfigureSingleResultVisible();
            if (!IsDropDownOpen)
            {
                ConfigureSingleFilterTextBox();
            }
        }
        else if (change.Property == PlaceholderTextProperty)
        {
            ConfigureSingleFilterTextBox();
        }
        else if (change.Property == IsFilterEnabledProperty)
        {
            ConfigureSingleFilterTextBox();
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

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        ClearPopupContent();
        base.OnApplyTemplate(e);

        _singleFilterInput = e.NameScope.Find<SelectFilterTextBox>("PART_SingleFilterInput");
        if (Popup != null)
        {
            Popup.OverlayInputPassThroughElement =
                e.NameScope.Find<AddOnDecoratedBox>(AddOnDecoratedBox.AddOnDecoratedBoxPart);
        }

        ConfigureSelectionIsEmpty();
        UpdatePseudoClasses();
        ConfigureSingleFilterTextBox();
        ConfigurePlaceholderVisible();
        ConfigureSingleResultVisible();
        UpdatePseudoClasses();
        SetupSelectHandleInputStateBindings(e);
        if (IsDropDownOpen)
        {
            EnsurePopupContent();
        }
    }

    protected override void EnsurePopupContent()
    {
        if (Popup == null)
        {
            return;
        }

        if (_treeView == null)
        {
            var treeView = new TreeSelectTreeView
            {
                Name                = "PART_TreeView",
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment   = VerticalAlignment.Stretch,
                NodeHoverMode       = TreeItemHoverMode.Block,
                ItemsSource         = BuildItemsSourceList(Items)
            };
            treeView.Classes.Add(TreeSelectSemanticParts.PopupListClass);
            treeView.SetTemplatedParent(this);
            treeView[!SelectingItemsControl.AutoScrollToSelectedItemProperty] = this[!AutoScrollToSelectedItemProperty];
            treeView[!TreeView.ToggleTypeProperty]                            = this[!TreeViewToggleTypeProperty];
            treeView[!TreeView.IsDefaultExpandAllProperty]                    = this[!IsDefaultExpandAllProperty];
            treeView[!TreeView.FilterValueProperty]                           = this[!FilterValueProperty];
            treeView[!TreeView.FilterProperty]                                = this[!FilterProperty];
            treeView[!TreeView.FilterStrategyProperty]                        = this[!FilterStrategyProperty];
            treeView[!ItemsControl.ItemTemplateProperty]                      = this[!ItemTemplateProperty];
            treeView[!TreeView.DataLoaderProperty]                            = this[!DataLoaderProperty];
            treeView[!TreeView.FilterHighlightForegroundProperty]             = this[!FilterHighlightForegroundProperty];
            treeView[!AvaloniaTreeView.SelectionModeProperty]                 = this[!TreeViewSelectionModeProperty];
            treeView[!TreeView.IsSelectableProperty]                          = this[!IsTreeViewSelectableProperty];
            treeView[!TreeView.IsCheckStrictlyProperty]                       = this[!IsTreeCheckStrictlyProperty];
            treeView[!TreeView.IsShowIconProperty]                            = this[!IsShowIconProperty];
            treeView[!TreeView.IsShowLeafIconProperty]                        = this[!IsShowLeafIconProperty];
            treeView[!TreeView.IsShowLineProperty]                            = this[!IsShowLineProperty];
            treeView[!TreeView.IsShowEmptyIndicatorProperty]                  = this[!IsShowEmptyIndicatorProperty];
            treeView[!TreeView.IsSwitcherRotationProperty]                    = this[!IsSwitcherRotationProperty];
            treeView[!TreeSelectTreeView.IsMaxSelectReachedProperty]          = this[!IsMaxSelectReachedProperty];
            treeView.SelectionChanged    += HandleTreeViewSelectionChanged;
            treeView.CheckedItemsChanged += HandleTreeViewItemsCheckedChanged;
            _treeView = treeView;
            SyncSelectedItemToTreeView();
            SyncSelectedItemsToTreeView();
        }

        // PopupFrame 现在是 TreeSelectTheme 中静态模板 Border（即 Popup.Child），
        // 每次以 Popup.Child 为准取回，避免缓存字段在模板重应用后指向旧实例。
        if (Popup.Child is Border popupFrame && !ReferenceEquals(popupFrame.Child, _treeView))
        {
            popupFrame.Child = _treeView;
        }
    }

    protected override void PopupClosed(object? sender, EventArgs e)
    {
        base.PopupClosed(sender, e);
        if (!IsMultiple)
        {
            FilterValue = null;
            ConfigureSingleFilterTextBox();
        }
    }

    protected override void PopupOpened(object? sender, EventArgs e)
    {
        base.PopupOpened(sender, e);
        if (!IsMultiple)
        {
            SyncSelectedItemToTreeView();
            if (IsFilterEnabled)
            {
                _singleFilterInput?.Focus();
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
                var filterTextBox = sourceControl.FindAncestorOfType<SelectFilterTextBox>(includeSelf: true);
                if (filterTextBox != null)
                {
                    return;
                }

                if (SelectTag.IsCloseButtonSource(sourceControl))
                {
                    e.Handled = true;
                    return;
                }
            }

            SetCurrentValue(IsDropDownOpenProperty, false);
            e.Handled = true;
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
                    clickInTagCloseButton = SelectTag.IsCloseButtonSource(sourceControl);
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

    #region 实现 FormItem 接口

    protected override void NotifySetFormValue(object? value)
    {
        if (!IsMultiple)
        {
            SelectedItem = value as ITreeItemNode;
        }
        else
        {
            SelectedItems = value as IList<ITreeItemNode>;
        }
    }

    protected override object? NotifyGetFormValue()
    {
        if (!IsMultiple)
        {
            return SelectedItem;
        }
        return SelectedItems;
    }

    protected override void NotifyClearFormValue()
    {
        if (!IsMultiple)
        {
            SelectedItem = null;
        }
        else
        {
            SelectedItems = null;
        }
    }
    #endregion

    private void HandleItemsSourceChanged(AvaloniaPropertyChangedEventArgs args)
    {
        var selectedItemPath  = !IsMultiple ? BuildNodeIdentityPath(SelectedItem) : null;
        var selectedItemPaths = IsMultiple ? BuildSelectedNodeIdentityPaths(SelectedItems) : null;

        _items.SetItemsSource(args.GetNewValue<IEnumerable<ITreeItemNode>?>());

        if (!IsMultiple)
        {
            if (selectedItemPath != null &&
                TryParseNodePath(selectedItemPath, out var selectedItems) &&
                selectedItems.Count > 0)
            {
                SelectedItem = selectedItems[^1];
            }
            else
            {
                SelectedItem = null;
            }
        }
        else if (selectedItemPaths != null)
        {
            var remappedItems = new List<ITreeItemNode>(selectedItemPaths.Count);
            foreach (var path in selectedItemPaths)
            {
                if (TryParseNodePath(path, out var selectedItems) &&
                    selectedItems.Count > 0)
                {
                    remappedItems.Add(selectedItems[^1]);
                }
            }

            SelectedItems = remappedItems.Count > 0 ? remappedItems : null;
        }

        // The popup tree may still be replaying its source. Its owner selection is authoritative
        // after the remap, even if checked items were synchronized one node at a time.
        _treeView?.CaptureOwnerSelectionForItemsSourceReplay();
    }

    private void HandleItemsChanged(object? sender, NotifyCollectionChangedEventArgs args)
    {
        if (_treeView != null)
        {
            _treeView.ItemsSource = BuildItemsSourceList(Items);
        }
    }

    private void HandleSelectedItemChanged(AvaloniaPropertyChangedEventArgs args)
    {
        NotifyFormValueChanged(args.NewValue);
    }

    private void HandleSelectedItemsChanged(AvaloniaPropertyChangedEventArgs args)
    {
        ConfigureSelectedItemsCollectionChangedSource(args.GetNewValue<IList<ITreeItemNode>?>());
        NotifyFormValueChanged(args.NewValue);
    }

    private void ConfigureSelectedItemsCollectionChangedSource(IList<ITreeItemNode>? selectedItems)
    {
        if (!this.IsAttachedToVisualTree())
        {
            ReleaseSelectedItemsCollectionChangedSource();
            return;
        }

        if (ReferenceEquals(_selectedItemsCollectionChangedSource, selectedItems))
        {
            return;
        }

        ReleaseSelectedItemsCollectionChangedSource();

        _selectedItemsCollectionChangedSource = selectedItems as INotifyCollectionChanged;
        if (_selectedItemsCollectionChangedSource != null)
        {
            _selectedItemsCollectionChangedSource.CollectionChanged += HandleSelectedItemsCollectionChanged;
        }
    }

    private void ReleaseSelectedItemsCollectionChangedSource()
    {
        if (_selectedItemsCollectionChangedSource != null)
        {
            _selectedItemsCollectionChangedSource.CollectionChanged -= HandleSelectedItemsCollectionChanged;
            _selectedItemsCollectionChangedSource = null;
        }
    }

    private void HandleSelectedItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs args)
    {
        if (!ReferenceEquals(sender, _selectedItemsCollectionChangedSource))
        {
            return;
        }

        NotifyFormValueChanged(SelectedItems);
        ConfigureSelectedItemsValueState();
    }

    private void ConfigureSelectedItemsValueState()
    {
        ConfigurePlaceholderVisible();
        ConfigureSingleResultVisible();
        ConfigureSelectionIsEmpty();
        if (IsMultiple)
        {
            SetCurrentValue(SelectedCountProperty, SelectedItems?.Count ?? 0);
        }
        else
        {
            SetCurrentValue(SelectedCountProperty, SelectedItem != null ? 1 : 0);
        }
        ConfigureSingleFilterTextBox();
        SyncSelectedItemsToTreeView();
        BuildEffectiveSelectedItems();
        ConfigureMaxSelectReached();
    }

    private void HandleClearRequest()
    {
        Clear();
    }

    private void ConfigurePlaceholderVisible()
    {
        SetCurrentValue(IsPlaceholderTextVisibleProperty,
            IsMultiple &&
            SelectedItem == null &&
            (SelectedItems == null || SelectedItems?.Count == 0) &&
            string.IsNullOrEmpty(FilterValue?.ToString()));
    }

    private void ConfigureSingleResultVisible()
    {
        SetCurrentValue(IsSingleResultVisibleProperty, false);
    }

    private void ClearPopupContent()
    {
        if (_treeView != null)
        {
            _treeView.SelectionChanged    -= HandleTreeViewSelectionChanged;
            _treeView.CheckedItemsChanged -= HandleTreeViewItemsCheckedChanged;
            _treeView.ItemsSource         =  null;
            _treeView.SetTemplatedParent(null);
        }

        // PopupFrame 是 TreeSelectTheme 中静态模板 Border（即 Popup.Child），
        // 仅清空其内部的树视图，保留 Border 自身作为弹层根。
        if (Popup != null && Popup.Child is Border popupFrame && ReferenceEquals(popupFrame.Child, _treeView))
        {
            popupFrame.Child = null;
        }

        _treeView = null;
    }

    private void SetupSelectHandleInputStateBindings(TemplateAppliedEventArgs e)
    {
        _selectHandleInputStateBindings?.Dispose();
        _selectHandleInputStateBindings = null;

        var handle   = e.NameScope.Find<SelectHandle>("PART_SelectHandle");
        var addOnBox = e.NameScope.Find<AddOnDecoratedBox>(AddOnDecoratedBox.AddOnDecoratedBoxPart);
        if (handle == null || addOnBox == null)
        {
            return;
        }

        _selectHandleInputStateBindings = new CompositeDisposable();
        _selectHandleInputStateBindings.Add(BindUtils.RelayBind(addOnBox,
            AddOnDecoratedBox.IsInnerBoxHoverProperty, handle, SelectHandle.IsInputHoverProperty));
        _selectHandleInputStateBindings.Add(BindUtils.RelayBind(addOnBox,
            AddOnDecoratedBox.IsInnerBoxPressedProperty, handle, SelectHandle.IsInputPressedProperty));
    }

    private void HandleTreeViewSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        // The detached popup tree is a projection, not an input source. Its source reset must not
        // overwrite the selection owned by TreeSelect before the popup is opened again.
        if (_treeView == null || !_treeView.IsLoaded || _needSkipCollectionChangedEvent)
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
                var currentSet  = BuildTreeNodeSet(_selectedItems);
                var treeViewSet = BuildTreeNodeSet(_treeView.SelectedItems);
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
                    SelectedItems          = BuildTreeNodeList(_treeView.SelectedItems);
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
                SelectedItem           = _treeView.SelectedItem as ITreeItemNode;
            }
            finally
            {
                _needSkipSyncSelection = false;
            }
        }
    }

    private void HandleTreeViewItemsCheckedChanged(object? sender, TreeViewCheckedItemsChangedEventArgs e)
    {
        if (_treeView == null || !_treeView.IsLoaded)
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
            var currentSet  = BuildTreeNodeSet(_selectedItems);
            var treeViewSet = BuildTreeNodeSet(_treeView.CheckedItems);
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
                SelectedItems          = BuildTreeNodeList(_treeView.CheckedItems);
            }
            finally
            {
                _needSkipSyncSelection = false;
            }
        }
    }

    private void ConfigureSelectionIsEmpty()
    {
        SetCurrentValue(IsSelectionEmptyProperty, SelectedItem == null && (SelectedItems == null || SelectedItems?.Count == 0));
    }

    private void ConfigureSingleFilterTextBox()
    {
        if (_singleFilterInput == null)
        {
            return;
        }

        _singleFilterInput.Width = double.NaN;

        if (IsMultiple)
        {
            _singleFilterInput.Focusable            = false;
            _singleFilterInput.IsCaretLockedToStart = true;
            _singleFilterInput.IsReadOnly           = true;
            _singleFilterInput.PlaceholderText      = null;
            SetSingleFilterInputText(null);
            ResetSingleFilterInputCaret();
            return;
        }

        var selectedText = SelectedItem?.Header?.ToString();
        var isEditableSearch = IsDropDownOpen && IsFilterEnabled;
        _singleFilterInput.Focusable            = IsFilterEnabled;
        _singleFilterInput.IsCaretLockedToStart = !isEditableSearch;
        if (isEditableSearch)
        {
            _singleFilterInput.IsReadOnly           = false;
            _singleFilterInput.PlaceholderText      = selectedText ?? PlaceholderText;
            SetSingleFilterInputText(string.Empty);
            ResetSingleFilterInputCaret();
        }
        else
        {
            _singleFilterInput.IsReadOnly           = true;
            _singleFilterInput.PlaceholderText      = PlaceholderText;
            SetSingleFilterInputText(selectedText ?? string.Empty);
            ResetSingleFilterInputCaret();
        }
    }

    private void SetSingleFilterInputText(string? text)
    {
        if (_singleFilterInput == null || _singleFilterInput.Text == text)
        {
            return;
        }

        _syncingSingleFilterInputText = true;
        try
        {
            _singleFilterInput.Text = text;
        }
        finally
        {
            _syncingSingleFilterInputText = false;
        }
    }

    private void ResetSingleFilterInputCaret()
    {
        if (_singleFilterInput == null)
        {
            return;
        }

        _singleFilterInput.ResetCaretToStart();
    }

    private void HandleTreeViewItemClicked(TreeViewItem viewItem)
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
            if (e.Source is AbstractTextInput textInput)
            {
                if (ReferenceEquals(textInput, _singleFilterInput) &&
                    !IsMultiple &&
                    (_syncingSingleFilterInputText || !IsDropDownOpen || !IsFilterEnabled))
                {
                    e.Handled = true;
                    return;
                }

                var searchText = textInput.Text?.Trim();
                FilterValue = string.IsNullOrEmpty(searchText) ? null : searchText;
            }
            ConfigurePlaceholderVisible();
            ConfigureSingleResultVisible();
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
        if (e.Source is SelectTag tag && tag.Item is ITreeItemNode treeItemNode)
        {
            if (SelectedItems != null)
            {
                var selectedItems = new List<ITreeItemNode>(SelectedItems.Count);
                foreach (var item in SelectedItems)
                {
                    selectedItems.Add(item);
                }
                RemoveItemRecursive(selectedItems, treeItemNode);
                SelectedItems = selectedItems;
            }
        }
        e.Handled = true;
    }

    private void RemoveItemRecursive(List<ITreeItemNode> items, ITreeItemNode item)
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
                                item.IsSelected = true;
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
                                var treeViewItems = BuildTreeNodeList(_treeView.SelectedItems);
                                var treeViewSet   = BuildTreeNodeSet(treeViewItems);
                                var currentSet    = BuildTreeNodeSet(SelectedItems);

                                foreach (var item in treeViewItems)
                                {
                                    if (!currentSet.Contains(item))
                                    {
                                        _treeView.SelectedItems.Remove(item);
                                    }
                                }

                                foreach (var item in SelectedItems)
                                {
                                    if (!treeViewSet.Contains(item))
                                    {
                                        _treeView.SelectedItems.Add(item);
                                    }
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
                            var treeViewItems = BuildTreeNodeList(_treeView.CheckedItems);
                            var treeViewSet   = BuildTreeNodeSet(treeViewItems);
                            var currentSet    = BuildTreeNodeSet(SelectedItems);
                            foreach (var item in treeViewItems)
                            {
                                if (!currentSet.Contains(item))
                                {
                                    _treeView.CheckedItems.Remove(item);
                                }
                            }

                            foreach (var item in SelectedItems)
                            {
                                if (!treeViewSet.Contains(item))
                                {
                                    _treeView.CheckedItems.Add(item);
                                }
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
            if (SelectedItems.Count == 0 || ShowCheckedStrategy == TreeSelectCheckedStrategy.All)
            {
                EffectiveSelectedItems = BuildTreeNodeList(SelectedItems);
            }
            else
            {
                var effectiveSelectedItems = new List<ITreeItemNode>(SelectedItems.Count);
                var showCheckedStrategy    = ShowCheckedStrategy;
                if ((showCheckedStrategy & TreeSelectCheckedStrategy.ShowParent) == TreeSelectCheckedStrategy.ShowParent)
                {
                    var selectedSet          = BuildTreeNodeSet(SelectedItems);
                    var fullySelectedParents = new HashSet<ITreeItemNode>(SelectedItems.Count,
                        ReferenceEqualityComparer.Instance);
                    foreach (var node in SelectedItems)
                    {
                        if (AreAllChildrenSelected(node.Children, selectedSet))
                        {
                            fullySelectedParents.Add(node);
                        }
                    }
                    foreach (var node in SelectedItems)
                    {
                        if (!HasSelectedAncestor(node, fullySelectedParents))
                        {
                            effectiveSelectedItems.Add(node);
                        }
                    }
                }
                if ((showCheckedStrategy & TreeSelectCheckedStrategy.ShowChild) == TreeSelectCheckedStrategy.ShowChild)
                {
                    foreach (var node in SelectedItems)
                    {
                        if (!HasChildren(node.Children))
                        {
                            effectiveSelectedItems.Add(node);
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

    private static List<object?> BuildItemsSourceList(IEnumerable source)
    {
        var items = source switch
        {
            ICollection collection => new List<object?>(collection.Count),
            IReadOnlyCollection<object?> collection => new List<object?>(collection.Count),
            _ => new List<object?>()
        };
        foreach (var item in source)
        {
            items.Add(item);
        }
        return items;
    }

    private static List<ITreeItemNode> BuildTreeNodeList(IEnumerable source)
    {
        var items = source switch
        {
            ICollection collection => new List<ITreeItemNode>(collection.Count),
            IReadOnlyCollection<ITreeItemNode> collection => new List<ITreeItemNode>(collection.Count),
            _ => new List<ITreeItemNode>()
        };
        foreach (var item in source)
        {
            items.Add((ITreeItemNode)item!);
        }
        return items;
    }

    private static HashSet<ITreeItemNode> BuildTreeNodeSet(IEnumerable source)
    {
        var items = source switch
        {
            ICollection collection => new HashSet<ITreeItemNode>(collection.Count, ReferenceEqualityComparer.Instance),
            IReadOnlyCollection<ITreeItemNode> collection =>
                new HashSet<ITreeItemNode>(collection.Count, ReferenceEqualityComparer.Instance),
            _ => new HashSet<ITreeItemNode>(ReferenceEqualityComparer.Instance)
        };
        foreach (var item in source)
        {
            items.Add((ITreeItemNode)item!);
        }
        return items;
    }

    private static HashSet<ITreeItemNode> BuildTreeNodeSet(ICollection<ITreeItemNode> source)
    {
        var items = new HashSet<ITreeItemNode>(source.Count, ReferenceEqualityComparer.Instance);
        foreach (var item in source)
        {
            items.Add(item);
        }
        return items;
    }

    private static List<TreeNodePath>? BuildSelectedNodeIdentityPaths(ICollection<ITreeItemNode>? nodes)
    {
        if (nodes == null)
        {
            return null;
        }

        var paths = new List<TreeNodePath>(nodes.Count);
        foreach (var node in nodes)
        {
            var path = BuildNodeIdentityPath(node);
            if (path != null)
            {
                paths.Add(path);
            }
        }
        return paths;
    }

    private static TreeNodePath? BuildNodeIdentityPath(ITreeItemNode? node)
    {
        if (node == null)
        {
            return null;
        }

        var depth    = CountNodePathDepth(node);
        var segments = new string[depth];
        var current  = node;
        for (var i = segments.Length - 1; current != null; i--)
        {
            var segment = current.ItemKey?.ToString() ?? current.Value?.ToString();
            if (string.IsNullOrEmpty(segment))
            {
                return null;
            }

            segments[i] = segment;
            current     = current.ParentNode as ITreeItemNode;
        }

        return new TreeNodePath(segments);
    }

    private bool TryParseNodePath(TreeNodePath path, out IList<ITreeItemNode> pathNodes)
    {
        var segments     = path.Segments;
        var isPathValid  = true;
        var currentItems = BuildTreeNodeList(Items);
        var nodes        = new List<ITreeItemNode>(segments.Count);
        for (var i = 0; i < segments.Count; i++)
        {
            var segment = segments[i];
            var found   = false;
            foreach (var currentItem in currentItems)
            {
                if (segment == currentItem.ItemKey || segment == currentItem.Value?.ToString())
                {
                    nodes.Add(currentItem);
                    currentItems = BuildTreeNodeList(currentItem.Children);
                    found        = true;
                    break;
                }
            }

            if (!found)
            {
                isPathValid = false;
                break;
            }
        }

        pathNodes = nodes;
        return isPathValid;
    }

    private static int CountNodePathDepth(ITreeItemNode node)
    {
        var count   = 0;
        var current = node;
        while (current != null)
        {
            count++;
            current = current.ParentNode as ITreeItemNode;
        }

        return count;
    }

    private static bool AreAllChildrenSelected(
        IEnumerable<ITreeItemNode> children,
        ISet<ITreeItemNode> selected)
    {
        foreach (var child in children)
        {
            if (!selected.Contains(child))
            {
                return false;
            }
        }
        return true;
    }

    private static bool HasChildren(IEnumerable<ITreeItemNode> children)
    {
        if (children is ICollection<ITreeItemNode> collection)
        {
            return collection.Count > 0;
        }

        if (children is IReadOnlyCollection<ITreeItemNode> readOnlyCollection)
        {
            return readOnlyCollection.Count > 0;
        }

        foreach (var _ in children)
        {
            return true;
        }
        return false;
    }

    private static bool HasSelectedAncestor(ITreeItemNode node, ISet<ITreeItemNode> selectedAncestors)
    {
        var current = node.ParentNode as ITreeItemNode;
        while (current != null)
        {
            if (selectedAncestors.Contains(current))
            {
                return true;
            }
            current = current.ParentNode as ITreeItemNode;
        }
        return false;
    }
}
