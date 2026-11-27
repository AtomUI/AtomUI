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

/// <summary>
/// A select control whose drop-down shows a tree. In single selection the selection box shows the header of
/// <see cref="SelectedItem" /> as text; with <see cref="IsMultiple" /> it shows the nodes of
/// <see cref="SelectedItems" /> as tags.
/// </summary>
public partial class TreeSelect : AbstractSelect
{
    #region 公共属性定义

    /// <summary>
    /// Identifies the <see cref="ShowCheckedStrategy" /> property.
    /// </summary>
    public static readonly StyledProperty<TreeSelectCheckedStrategy> ShowCheckedStrategyProperty =
        AvaloniaProperty.Register<TreeSelect, TreeSelectCheckedStrategy>(
            nameof(ShowCheckedStrategy), TreeSelectCheckedStrategy.All);

    /// <summary>
    /// Identifies the <see cref="AutoScrollToSelectedItem" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> AutoScrollToSelectedItemProperty =
        SelectingItemsControl.AutoScrollToSelectedItemProperty.AddOwner<TreeSelect>();

    /// <summary>
    /// Identifies the <see cref="IsShowIcon" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsShowIconProperty =
        TreeView.IsShowIconProperty.AddOwner<TreeSelect>();

    /// <summary>
    /// Identifies the <see cref="IsShowLeafIcon" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsShowLeafIconProperty =
        TreeView.IsShowLeafIconProperty.AddOwner<TreeSelect>();

    /// <summary>
    /// Identifies the <see cref="IsShowLine" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsShowLineProperty =
        TreeView.IsShowLineProperty.AddOwner<TreeSelect>();

    /// <summary>
    /// Identifies the <see cref="IsTreeCheckable" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsTreeCheckableProperty =
        AvaloniaProperty.Register<TreeSelect, bool>(
            nameof(IsTreeCheckable));

    /// <summary>
    /// Identifies the <see cref="IsMultiple" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsMultipleProperty =
        AvaloniaProperty.Register<TreeSelect, bool>(
            nameof(IsMultiple));

    /// <summary>
    /// Identifies the <see cref="IsDefaultExpandAll" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsDefaultExpandAllProperty =
        AvaloniaProperty.Register<TreeSelect, bool>(
            nameof(IsDefaultExpandAll));

    /// <summary>
    /// Identifies the <see cref="IsShowTreeLine" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsShowTreeLineProperty =
        AvaloniaProperty.Register<TreeSelect, bool>(
            nameof(IsShowTreeLine));

    /// <summary>
    /// Identifies the <see cref="IsSwitcherRotation" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsSwitcherRotationProperty =
        TreeView.IsSwitcherRotationProperty.AddOwner<TreeSelect>();

    /// <summary>
    /// Identifies the <see cref="IsTreeCheckStrictly" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsTreeCheckStrictlyProperty =
        AvaloniaProperty.Register<TreeSelect, bool>(nameof(IsTreeCheckStrictly), false);

    /// <summary>
    /// Identifies the <see cref="TreeDefaultExpandedPaths" /> property.
    /// </summary>
    public static readonly DirectProperty<TreeSelect, IList<TreeNodePath>?> TreeDefaultExpandedPathsProperty =
        AvaloniaProperty.RegisterDirect<TreeSelect, IList<TreeNodePath>?>(
            nameof(TreeDefaultExpandedPaths),
            o => o.TreeDefaultExpandedPaths,
            (o, v) => o.TreeDefaultExpandedPaths = v);

    /// <summary>
    /// Identifies the <see cref="ItemsSource" /> property.
    /// </summary>
    public static readonly StyledProperty<IEnumerable<ITreeItemNode>?> ItemsSourceProperty =
        AvaloniaProperty.Register<TreeSelect, IEnumerable<ITreeItemNode>?>(nameof(ItemsSource));

    /// <summary>
    /// Identifies the <see cref="ItemTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> ItemTemplateProperty =
        AvaloniaProperty.Register<TreeSelect, IDataTemplate?>(nameof(ItemTemplate));

    /// <summary>
    /// Identifies the <see cref="DataLoader" /> property.
    /// </summary>
    public static readonly StyledProperty<ITreeItemNodeLoader?> DataLoaderProperty =
        AvaloniaProperty.Register<TreeSelect, ITreeItemNodeLoader?>(
            nameof(DataLoader));

    /// <summary>
    /// Identifies the <see cref="Filter" /> property.
    /// </summary>
    public static readonly StyledProperty<IValueFilter?> FilterProperty =
        AvaloniaProperty.Register<TreeSelect, IValueFilter?>(
            nameof(Filter));

    /// <summary>
    /// Identifies the <see cref="FilterValueSelector" /> property.
    /// </summary>
    public static readonly StyledProperty<DefaultFilterValueSelector?> FilterValueSelectorProperty =
        AvaloniaProperty.Register<TreeSelect, DefaultFilterValueSelector?>(nameof(FilterValueSelector));

    /// <summary>
    /// Identifies the <see cref="FilterStrategy" /> property.
    /// </summary>
    public static readonly DirectProperty<TreeSelect, TreeFilterStrategy> FilterStrategyProperty =
        AvaloniaProperty.RegisterDirect<TreeSelect, TreeFilterStrategy>(
            nameof(FilterStrategy),
            o => o.FilterStrategy,
            (o, v) => o.FilterStrategy = v);

    /// <summary>
    /// Identifies the <see cref="FilterHighlightForeground" /> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> FilterHighlightForegroundProperty =
        AvaloniaProperty.Register<TreeSelect, IBrush?>(nameof(FilterHighlightForeground));

    /// <summary>
    /// Identifies the <see cref="SelectedItem" /> property.
    /// </summary>
    public static readonly DirectProperty<TreeSelect, ITreeItemNode?> SelectedItemProperty =
        AvaloniaProperty.RegisterDirect<TreeSelect, ITreeItemNode?>(
            nameof(SelectedItem),
            o => o.SelectedItem,
            (o, v) => o.SelectedItem = v,
            defaultBindingMode: BindingMode.TwoWay,
            enableDataValidation: true);

    /// <summary>
    /// Identifies the <see cref="SelectedItems" /> property.
    /// </summary>
    public static readonly DirectProperty<TreeSelect, IList<ITreeItemNode>?> SelectedItemsProperty =
        AvaloniaProperty.RegisterDirect<TreeSelect, IList<ITreeItemNode>?>(
            nameof(SelectedItems),
            o => o.SelectedItems,
            (o, v) => o.SelectedItems = v,
            defaultBindingMode: BindingMode.TwoWay,
            enableDataValidation: true);

    /// <summary>
    /// Chooses which of the <see cref="SelectedItems" /> are listed as tags in the selection box while
    /// <see cref="IsMultiple" /> is <see langword="true" />. <see cref="TreeSelectCheckedStrategy.All" /> lists
    /// every selected node, <see cref="TreeSelectCheckedStrategy.ShowParent" /> lists the topmost node of a fully
    /// selected subtree in place of the nodes below it, and
    /// <see cref="TreeSelectCheckedStrategy.ShowChild" /> lists only the selected nodes that have no children.
    /// The flags can be combined; the default is <see cref="TreeSelectCheckedStrategy.All" />.
    /// </summary>
    public TreeSelectCheckedStrategy ShowCheckedStrategy
    {
        get => GetValue(ShowCheckedStrategyProperty);
        set => SetValue(ShowCheckedStrategyProperty, value);
    }

    /// <summary>
    /// Whether the drop-down tree scrolls its selected node into view when the selection changes. The value is
    /// passed on to that tree view only.
    /// </summary>
    public bool AutoScrollToSelectedItem
    {
        get => GetValue(AutoScrollToSelectedItemProperty);
        set => SetValue(AutoScrollToSelectedItemProperty, value);
    }

    /// <summary>
    /// Whether the nodes of the drop-down tree show their icon. The value is passed on to that tree view only.
    /// The default is <see langword="false" />.
    /// </summary>
    public bool IsShowIcon
    {
        get => GetValue(IsShowIconProperty);
        set => SetValue(IsShowIconProperty, value);
    }

    /// <summary>
    /// Whether nodes without children show their icon in the drop-down tree. The value is passed on to that tree
    /// view only. The default is <see langword="false" />.
    /// </summary>
    public bool IsShowLeafIcon
    {
        get => GetValue(IsShowLeafIconProperty);
        set => SetValue(IsShowLeafIconProperty, value);
    }

    /// <summary>
    /// Whether the drop-down tree draws lines that connect a node to its children. The value is passed on to that
    /// tree view only. The default is <see langword="false" />.
    /// </summary>
    public bool IsShowLine
    {
        get => GetValue(IsShowLineProperty);
        set => SetValue(IsShowLineProperty, value);
    }

    /// <summary>
    /// Whether every node of the drop-down tree shows a check box and nodes are selected by checking them instead
    /// of by highlighting them. Changing this property also sets <see cref="IsMultiple" /> to
    /// <see langword="true" />, switches the drop-down tree to check boxes and clears the selection of a tree view
    /// that already exists. The default is <see langword="false" />.
    /// </summary>
    public bool IsTreeCheckable
    {
        get => GetValue(IsTreeCheckableProperty);
        set => SetValue(IsTreeCheckableProperty, value);
    }

    /// <summary>
    /// Whether more than one node can be selected. When <see langword="true" />, the nodes of
    /// <see cref="SelectedItems" /> are shown as tags in the selection box. When <see langword="false" />, the
    /// header of <see cref="SelectedItem" /> is shown there as text, and that text box accepts typed filter text
    /// while the drop-down is open and <c>IsFilterEnabled</c> is set. The default is <see langword="false" />.
    /// </summary>
    public bool IsMultiple
    {
        get => GetValue(IsMultipleProperty);
        set => SetValue(IsMultipleProperty, value);
    }

    /// <summary>
    /// Whether all nodes of the drop-down tree are expanded by default. The value is passed on to that tree view
    /// only. The default is <see langword="false" />.
    /// </summary>
    public bool IsDefaultExpandAll
    {
        get => GetValue(IsDefaultExpandAllProperty);
        set => SetValue(IsDefaultExpandAllProperty, value);
    }

    /// <summary>
    /// Not read by the current implementation: the value is stored and its changes are raised, but neither the
    /// control nor its themes use it. <see cref="IsShowLine" /> is the property that is passed on to the
    /// drop-down tree. The default is <see langword="false" />.
    /// </summary>
    public bool IsShowTreeLine
    {
        get => GetValue(IsShowTreeLineProperty);
        set => SetValue(IsShowTreeLineProperty, value);
    }

    /// <summary>
    /// Whether the switcher of a node rotates when that node is expanded or collapsed in the drop-down tree. The
    /// value is passed on to that tree view only. The default is <see langword="true" />.
    /// </summary>
    public bool IsSwitcherRotation
    {
        get => GetValue(IsSwitcherRotationProperty);
        set => SetValue(IsSwitcherRotationProperty, value);
    }

    /// <summary>
    /// Whether the check box of a node works independently of its parent and children while
    /// <see cref="IsTreeCheckable" /> is set. Otherwise checking a node also changes the check state of its parent
    /// and children. The value is passed on to the drop-down tree only. The default is <see langword="false" />.
    /// </summary>
    public bool IsTreeCheckStrictly
    {
        get => GetValue(IsTreeCheckStrictlyProperty);
        set => SetValue(IsTreeCheckStrictlyProperty, value);
    }

    private IList<TreeNodePath>? _treeDefaultExpandedPaths;

    /// <summary>
    /// Not read by the current implementation: the value is stored and its changes are raised, but neither the
    /// control nor its themes use it, so setting it expands no node. The default is <see langword="null" />.
    /// </summary>
    public IList<TreeNodePath>? TreeDefaultExpandedPaths
    {
        get => _treeDefaultExpandedPaths;
        set => SetAndRaise(TreeDefaultExpandedPathsProperty, ref _treeDefaultExpandedPaths, value);
    }

    /// <summary>
    /// The root nodes of the drop-down tree. When it is replaced, the selection is carried over to the nodes of the
    /// new source that have the same <c>ItemKey</c> or <c>Value</c> along their ancestor path; a selected node that
    /// cannot be matched this way is dropped from the selection. <see langword="null" /> leaves only the nodes of
    /// <see cref="Items" />.
    /// </summary>
    public IEnumerable<ITreeItemNode>? ItemsSource
    {
        get => GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    /// <summary>
    /// The template that renders a node of the drop-down tree; its data context is the node. The value is passed on
    /// to that tree view. When it is not set, the theme supplies a tree template that shows the <c>Header</c> of a
    /// node as text and takes its children from its <c>Children</c> collection.
    /// </summary>
    [InheritDataTypeFromItems(nameof(ItemsSource))]
    public IDataTemplate? ItemTemplate
    {
        get => GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }

    /// <summary>
    /// The loader that supplies the children of a node of the drop-down tree when that node is expanded, for trees
    /// whose nodes are loaded on demand. The value is passed on to that tree view. When it is
    /// <see langword="null" />, nothing is loaded and a node shows only the children it already holds.
    /// </summary>
    public ITreeItemNodeLoader? DataLoader
    {
        get => GetValue(DataLoaderProperty);
        set => SetValue(DataLoaderProperty, value);
    }

    /// <summary>
    /// The filter that decides which nodes of the drop-down tree match the filter text typed into the selection
    /// box. The value is passed on to that tree view. When it is still <see langword="null" /> when the control is
    /// initialized, a <see cref="ValueFilterMode.Contains" /> filter is set as the current value. While it is
    /// <see langword="null" />, typed text does not filter the tree.
    /// </summary>
    public IValueFilter? Filter
    {
        get => GetValue(FilterProperty);
        set => SetValue(FilterProperty, value);
    }

    /// <summary>
    /// Not read by the current implementation: the value is stored and its changes are raised, but the control does
    /// not pass it on to the drop-down tree, so it has no effect on filtering.
    /// </summary>
    public DefaultFilterValueSelector? FilterValueSelector
    {
        get => GetValue(FilterValueSelectorProperty);
        set => SetValue(FilterValueSelectorProperty, value);
    }

    private TreeFilterStrategy _filterStrategy = TreeFilterStrategy.HighlightedWhole |
                                                 TreeFilterStrategy.BoldedMatch |
                                                 TreeFilterStrategy.ExpandPath |
                                                 TreeFilterStrategy.HideUnMatched;

    /// <summary>
    /// How the drop-down tree presents the result of filtering. The flags choose whether a matching node is
    /// highlighted as a whole (<see cref="TreeFilterStrategy.HighlightedWhole" />), whether the match is shown bold
    /// (<see cref="TreeFilterStrategy.BoldedMatch" />), whether the ancestors of a matching node are expanded
    /// (<see cref="TreeFilterStrategy.ExpandPath" />) and whether nodes without a match are hidden
    /// (<see cref="TreeFilterStrategy.HideUnMatched" />). The value is passed on to that tree view. The default
    /// combines all four flags.
    /// </summary>
    public TreeFilterStrategy FilterStrategy
    {
        get => _filterStrategy;
        set => SetAndRaise(FilterStrategyProperty, ref _filterStrategy, value);
    }

    /// <summary>
    /// The foreground of the highlighted match in the header of a node of the drop-down tree. The value is passed on
    /// to that tree view. When it is not set, the theme supplies <c>TreeSelectToken.FilterHighlightColor</c>.
    /// </summary>
    public IBrush? FilterHighlightForeground
    {
        get => GetValue(FilterHighlightForegroundProperty);
        set => SetValue(FilterHighlightForegroundProperty, value);
    }

    private ITreeItemNode? _selectedItem;

    /// <summary>
    /// The selected node while <see cref="IsMultiple" /> is <see langword="false" />; its header is shown in the
    /// selection box. Setting it selects the node in the drop-down tree, and selecting a node there updates this
    /// property. The binding is two-way by default. The drop-down tree does not update it in multiple selection.
    /// </summary>
    public ITreeItemNode? SelectedItem
    {
        get => _selectedItem;
        set => SetAndRaise(SelectedItemProperty, ref _selectedItem, value);
    }

    private IList<ITreeItemNode>? _selectedItems;

    /// <summary>
    /// The selected nodes while <see cref="IsMultiple" /> is <see langword="true" />; they are shown as tags in the
    /// selection box, limited to the nodes chosen by <see cref="ShowCheckedStrategy" />. Selecting or checking
    /// nodes in the drop-down tree replaces the list with a new one, and closing a tag removes that node and its
    /// descendants. When the assigned list notifies collection changes, adding or removing a node in place is
    /// applied to the tags and the drop-down tree as well. The binding is two-way by default.
    /// <see langword="null" /> means that nothing is selected.
    /// </summary>
    public IList<ITreeItemNode>? SelectedItems
    {
        get => _selectedItems;
        set => SetAndRaise(SelectedItemsProperty, ref _selectedItems, value);
    }

    /// <summary>
    /// A view of <see cref="Items" /> for enumerating the root nodes of the drop-down tree and for observing changes
    /// of that collection.
    /// </summary>
    public ItemsSourceView ItemsView => ItemsSourceView.GetOrCreate(_items);

    /// <summary>
    /// The root nodes of the drop-down tree. Nodes declared as XAML content are collected here, and the collection
    /// assigned to <see cref="ItemsSource" /> is merged into it while one is assigned.
    /// </summary>
    [Content]
    public ItemCollection Items => _items;
    #endregion

    #region 内部属性定义

    /// <summary>
    /// Lower bound of the drop-down width, resolved from <c>TreeSelectToken.MinPopupWidth</c> by the theme; the
    /// effective popup width is the larger of it and the matched select width.
    /// </summary>
    internal static readonly StyledProperty<double> MinPopupWidthProperty =
        AvaloniaProperty.Register<TreeSelect, double>(nameof(MinPopupWidth));

    internal double MinPopupWidth
    {
        get => GetValue(MinPopupWidthProperty);
        set => SetValue(MinPopupWidthProperty, value);
    }

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
        ConfigureKeyboardNavigation();
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

    /// <summary>
    /// Clears the selection by setting <see cref="SelectedItem" /> and <see cref="SelectedItems" /> to
    /// <see langword="null" />. The nodes of <see cref="Items" /> are not affected. The clear button of the
    /// selection box calls this method.
    /// </summary>
    public void Clear()
    {
        SelectedItems = null;
        SelectedItem  = null;
    }

    /// <summary>
    /// Calls the base method, then sets a <see cref="ValueFilterMode.Contains" /> filter as the current value of
    /// <see cref="Filter" /> when no filter is set, and recomputes whether <c>MaxCount</c> is reached.
    /// </summary>
    protected override void OnInitialized()
    {
        base.OnInitialized();
        if (Filter == null)
        {
            SetCurrentValue(FilterProperty, ValueFilterFactory.BuildFilter(ValueFilterMode.Contains));
        }
        ConfigureMaxSelectReached();
    }

    /// <summary>
    /// Calls the base method, then starts observing collection changes of <see cref="SelectedItems" /> and refreshes
    /// the state that depends on the selection: placeholder, tags, selected count and the selection of the
    /// drop-down tree.
    /// </summary>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        ConfigureSelectedItemsCollectionChangedSource(SelectedItems);
        ConfigureSelectedItemsValueState();
    }

    /// <summary>
    /// Calls the base method, then updates the text, placeholder and read-only state of the text box in the
    /// selection box from the selection mode, the drop-down state and the filter settings.
    /// </summary>
    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        ConfigureSingleFilterTextBox();
    }

    /// <summary>
    /// Stops observing collection changes of <see cref="SelectedItems" />, discards the tree view created for
    /// the drop-down, and then calls the base method.
    /// </summary>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        ReleaseSelectedItemsCollectionChangedSource();
        ClearPopupContent();
        base.OnDetachedFromVisualTree(e);
    }

    /// <summary>
    /// Sets the effective width of the drop-down to the larger of <paramref name="selectWidth" /> and
    /// <c>MinPopupWidth</c>, or to <c>MinPopupWidth</c> alone when <c>IsPopupMatchSelectWidth</c> is
    /// <see langword="false" />. The base class calls it with the width of the control; this class calls it
    /// when <c>IsPopupMatchSelectWidth</c> or <c>MinPopupWidth</c> changes. An override that does not call this
    /// method must set <c>EffectivePopupWidth</c> itself.
    /// </summary>
    /// <param name="selectWidth">
    /// The width of the select control.
    /// </param>
    protected override void ConfigurePopupMinWith(double selectWidth)
    {
        var matchedWidth = IsPopupMatchSelectWidth ? selectWidth : 0.0;
        SetCurrentValue(EffectivePopupWidthProperty, Math.Max(matchedWidth, MinPopupWidth));
    }

    /// <summary>
    /// Calls the base method, then keeps the selection box and the drop-down tree in step with the changed property.
    /// A change of the drop-down state, the selection, <see cref="IsMultiple" />, <c>FilterValue</c>,
    /// <c>PlaceholderText</c> or <c>IsFilterEnabled</c> updates the text, placeholder, tags and read-only state of
    /// the selection box; <see cref="IsMultiple" /> and <see cref="IsTreeCheckable" /> also set the selection mode
    /// of that tree. <c>StyleVariant</c> and <c>Status</c> update the pseudo classes, and the popup width settings
    /// update the effective popup width. An override must call this method for this state to stay in step.
    /// </summary>
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
        else if (change.Property == IsPopupMatchSelectWidthProperty ||
                 change.Property == MinPopupWidthProperty)
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

    /// <summary>
    /// Discards the tree view of the previous template, calls the base method, then finds the filter text box and
    /// the box that receives the pointer input passed through the drop-down's overlay, brings the selection box in
    /// line with the current state, relays the hover and pressed state of that box to the select handle, and creates
    /// the content of the drop-down when it is open.
    /// </summary>
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

    /// <summary>
    /// Creates the tree view of the drop-down on the first call and puts it into the frame of the popup; later calls
    /// only make sure that the frame holds that tree view. The tree view reads the selection, filter, node and
    /// loading settings of this control through bindings, so it is created once. Nothing happens when the template
    /// has no popup part.
    /// </summary>
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
                ItemsSource         = BuildItemsSourceList(Items),
                // Focus stays in TreeSelect; keys are forwarded and only Enter / Space commit.
                Focusable                      = false,
                IsKeyboardMoveSelectionEnabled = false
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

        // The popup frame is a static Border in the TreeSelectTheme template (Popup.Child). It is taken from
        // Popup.Child on every call, so that no cached field points at a stale instance after a template re-apply.
        if (Popup.Child is Border popupFrame && !ReferenceEquals(popupFrame.Child, _treeView))
        {
            popupFrame.Child = _treeView;
        }
    }

    /// <summary>
    /// Calls the base method, then clears the keyboard-active node of the drop-down tree and, in single selection,
    /// clears the filter text and brings the text box of the selection box in line with that.
    /// </summary>
    protected override void PopupClosed(object? sender, EventArgs e)
    {
        base.PopupClosed(sender, e);
        _treeView?.ClearKeyboardActiveItem();
        if (!IsMultiple)
        {
            FilterValue = null;
            ConfigureSingleFilterTextBox();
        }
    }

    /// <summary>
    /// Calls the base method, then, in single selection, passes <see cref="SelectedItem" /> to the drop-down tree and
    /// moves the focus to the filter text box when filtering is enabled.
    /// </summary>
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

    /// <summary>
    /// Calls the base method, then keeps the drop-down open for a press inside the drop-down. While the drop-down is
    /// open, a press on this control outside the filter text box and outside the close button of a tag closes it.
    /// While it is closed, the press marks the control as pressed so that the matching release opens it.
    /// </summary>
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

    /// <summary>
    /// Marks a release inside the drop-down as handled so that it does not toggle the drop-down. A release on this
    /// control that follows a press on it toggles the drop-down, unless it lands on the close button of a tag; that
    /// release is marked as handled as well. The pressed state is cleared and the base method is called last.
    /// </summary>
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

    /// <summary>
    /// Assigns the value that a form pushes to this control: an <c>ITreeItemNode</c> becomes
    /// <see cref="SelectedItem" /> in single selection, an <c>IList&lt;ITreeItemNode&gt;</c> becomes
    /// <see cref="SelectedItems" /> in multiple selection, and a value of any other type clears the selection.
    /// </summary>
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

    /// <summary>
    /// Returns the value that a form reads from this control: <see cref="SelectedItem" /> in single selection and
    /// <see cref="SelectedItems" /> in multiple selection, either of which is <see langword="null" /> when
    /// nothing is selected.
    /// </summary>
    protected override object? NotifyGetFormValue()
    {
        if (!IsMultiple)
        {
            return SelectedItem;
        }
        return SelectedItems;
    }

    /// <summary>
    /// Clears the selection when a form resets this control: <see cref="SelectedItem" /> is set to
    /// <see langword="null" /> in single selection and <see cref="SelectedItems" /> in multiple selection.
    /// </summary>
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

        // The popup frame is a static Border in the TreeSelectTheme template (Popup.Child). Only the tree view
        // inside it is removed; the Border itself stays as the root of the popup layer.
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
