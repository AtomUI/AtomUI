using System.Collections.Specialized;
using AtomUI.Controls;
using AtomUI.Controls.Data;
using AtomUI.Controls.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

public partial class ListView
{
    #region 公共属性定义

    /// <summary>
    /// Identifies the <see cref="AutoScrollToSelectedItem" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> AutoScrollToSelectedItemProperty =
        AvaloniaProperty.Register<ListView, bool>(nameof(AutoScrollToSelectedItem), true);

    /// <summary>
    /// Identifies the <see cref="Selection" /> property.
    /// </summary>
    public static readonly DirectProperty<ListView, IListViewSelection> SelectionProperty =
        AvaloniaProperty.RegisterDirect<ListView, IListViewSelection>(nameof(Selection), o => o.Selection);

    /// <summary>
    /// Identifies the <see cref="SelectedIndex" /> property.
    /// </summary>
    public static readonly DirectProperty<ListView, int> SelectedIndexProperty =
        AvaloniaProperty.RegisterDirect<ListView, int>(
            nameof(SelectedIndex), o => o.SelectedIndex, (o, value) => o.SelectedIndex = value,
            unsetValue: -1, defaultBindingMode: BindingMode.TwoWay);

    /// <summary>
    /// Identifies the <see cref="SelectedIndexes" /> property.
    /// </summary>
    public static readonly DirectProperty<ListView, IReadOnlyList<int>> SelectedIndexesProperty =
        AvaloniaProperty.RegisterDirect<ListView, IReadOnlyList<int>>(nameof(SelectedIndexes), o => o.SelectedIndexes);

    /// <summary>
    /// Identifies the <see cref="SelectedItem" /> property.
    /// </summary>
    public static readonly DirectProperty<ListView, object?> SelectedItemProperty =
        AvaloniaProperty.RegisterDirect<ListView, object?>(nameof(SelectedItem), o => o.SelectedItem);

    /// <summary>
    /// Identifies the <see cref="SelectedItems" /> property.
    /// </summary>
    public static readonly DirectProperty<ListView, IReadOnlyList<object?>> SelectedItemsProperty =
        AvaloniaProperty.RegisterDirect<ListView, IReadOnlyList<object?>>(nameof(SelectedItems), o => o.SelectedItems);

    /// <summary>
    /// Identifies the <see cref="SelectedValue" /> property.
    /// </summary>
    public static readonly DirectProperty<ListView, object?> SelectedValueProperty =
        AvaloniaProperty.RegisterDirect<ListView, object?>(nameof(SelectedValue), o => o.SelectedValue);

    /// <summary>
    /// Identifies the <see cref="SelectedValueBinding" /> property.
    /// </summary>
    public static readonly StyledProperty<BindingBase?> SelectedValueBindingProperty =
        AvaloniaProperty.Register<ListView, BindingBase?>(nameof(SelectedValueBinding));

    /// <summary>
    /// Identifies the <see cref="ItemKeySelector" /> property.
    /// </summary>
    public static readonly StyledProperty<ListItemKeySelector?> ItemKeySelectorProperty =
        AvaloniaProperty.Register<ListView, ListItemKeySelector?>(nameof(ItemKeySelector));

    /// <summary>
    /// Identifies the <see cref="SelectionMode" /> property.
    /// </summary>
    public static readonly StyledProperty<SelectionMode> SelectionModeProperty =
        AvaloniaProperty.Register<ListView, SelectionMode>(nameof(SelectionMode));

    /// <summary>
    /// Identifies the IsSelected attached property, which records on an item container whether its item is selected.
    /// </summary>
    /// <remarks>
    /// The value belongs to the single container that carries it and does not propagate.
    /// The list writes it on each item container from its selection model and keeps a binding on the container intact.
    /// A container that raises the IsSelectedChanged routed event makes the list apply its current value to the
    /// selection model.
    /// </remarks>
    public static readonly StyledProperty<bool> IsSelectedProperty =
        AvaloniaProperty.RegisterAttached<ListView, Control, bool>("IsSelected", defaultBindingMode: BindingMode.TwoWay);

    /// <summary>
    /// Identifies the <see cref="IsTextSearchEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsTextSearchEnabledProperty =
        AvaloniaProperty.Register<ListView, bool>(nameof(IsTextSearchEnabled), false);

    /// <summary>
    /// Identifies the <see cref="WrapSelection" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> WrapSelectionProperty =
        AvaloniaProperty.Register<ListView, bool>(nameof(WrapSelection), false);

    /// <summary>
    /// Scrolls the selection anchor into view when the list is attached to a visual tree or its template is applied.
    /// The scroll runs at most once per template application, and only when the anchor item is in the current view.
    /// The default is <see langword="true" />.
    /// </summary>
    /// <remarks>
    /// Setting the property to <see langword="true" /> later starts the same scroll attempt.
    /// </remarks>
    public bool AutoScrollToSelectedItem
    {
        get => GetValue(AutoScrollToSelectedItemProperty);
        set => SetValue(AutoScrollToSelectedItemProperty, value);
    }

    /// <summary>
    /// The selection model of the list, through which the selection is queried and changed.
    /// A change made through it updates <see cref="SelectedIndex" />, <see cref="SelectedIndexes" />,
    /// <see cref="SelectedItem" />, <see cref="SelectedItems" /> and <see cref="SelectedValue" />, and raises
    /// <see cref="SelectionChanged" /> when the selected items change.
    /// </summary>
    public IListViewSelection Selection => _selection;

    /// <summary>
    /// The source index of the active item, or -1 when there is no active item.
    /// Setting a non-negative index selects that item through the selection model; setting a negative index clears the
    /// selection.
    /// </summary>
    public int SelectedIndex
    {
        get => _selection.SelectedIndex;
        set
        {
            if (value < 0)
            {
                _selection.Clear();
            }
            else
            {
                _selection.Select(value);
            }
        }
    }

    /// <summary>
    /// The source indexes of the selected items.
    /// </summary>
    public IReadOnlyList<int> SelectedIndexes => _selection.SelectedIndexes;
    /// <summary>
    /// The active item, or <see langword="null" /> when there is no active item.
    /// </summary>
    public object? SelectedItem => _selection.SelectedItem;
    /// <summary>
    /// The selected items, in the order of <see cref="SelectedIndexes" />.
    /// </summary>
    public IReadOnlyList<object?> SelectedItems => _selection.SelectedItems;

    /// <summary>
    /// The binding that is evaluated on the active item to produce <see cref="SelectedValue" />.
    /// The default, <see langword="null" />, makes <see cref="SelectedValue" /> the active item itself.
    /// </summary>
    /// <remarks>
    /// Changing the binding evaluates <see cref="SelectedValue" /> again from the current active item.
    /// </remarks>
    public BindingBase? SelectedValueBinding
    {
        get => GetValue(SelectedValueBindingProperty);
        set => SetValue(SelectedValueBindingProperty, value);
    }

    /// <summary>
    /// A delegate that returns the key which identifies a source item.
    /// The default is <see langword="null" />, which sets no selector.
    /// </summary>
    /// <remarks>
    /// The list does not read this property at present, so setting it has no effect on the selection.
    /// </remarks>
    public ListItemKeySelector? ItemKeySelector
    {
        get => GetValue(ItemKeySelectorProperty);
        set => SetValue(ItemKeySelectorProperty, value);
    }

    /// <summary>
    /// The value of the active item evaluated with <see cref="SelectedValueBinding" />, or the active item itself
    /// when no binding is set.
    /// <see langword="null" /> when there is no active item.
    /// </summary>
    /// <remarks>
    /// Updated after every selection change, before <see cref="SelectionChanged" /> is raised.
    /// </remarks>
    public object? SelectedValue => _selectedValue;

    /// <summary>
    /// Enables selection by typing: text that reaches the list is appended to a search term, and the first item of
    /// the current view whose <c>ToString()</c> result starts with the term, ignoring case, is selected.
    /// The default is <see langword="false" />.
    /// </summary>
    public bool IsTextSearchEnabled
    {
        get => GetValue(IsTextSearchEnabledProperty);
        set => SetValue(IsTextSearchEnabledProperty, value);
    }

    /// <summary>
    /// Makes keyboard navigation of the selection move from the last item to the first one and from the first item
    /// to the last one, instead of stopping at the ends.
    /// The default is <see langword="false" />.
    /// </summary>
    public bool WrapSelection
    {
        get => GetValue(WrapSelectionProperty);
        set => SetValue(WrapSelectionProperty, value);
    }

    /// <summary>
    /// The selection behavior, as a combination of <see cref="Avalonia.Controls.SelectionMode" /> flags.
    /// A plain selection always replaces the current one.
    /// <c>SelectionMode.Multiple</c> lets a range selection extend the selection from the anchor and lets a toggle
    /// add or remove a single item.
    /// <c>SelectionMode.Toggle</c> makes every selection toggle the item without the toggle modifier.
    /// <c>SelectionMode.AlwaysSelected</c> selects the first item of the current view when a collection change
    /// leaves the selection empty.
    /// </summary>
    public SelectionMode SelectionMode
    {
        get => GetValue(SelectionModeProperty);
        set => SetValue(SelectionModeProperty, value);
    }

    /// <summary>
    /// Returns whether <c>SelectionMode.AlwaysSelected</c> is set.
    /// </summary>
    protected bool AlwaysSelected => (SelectionMode & SelectionMode.AlwaysSelected) == SelectionMode.AlwaysSelected;

    #endregion

    #region 公共事件定义

    /// <summary>
    /// Identifies the IsSelectedChanged routed event: a bubbling notification that the IsSelected attached
    /// property of an item container changed.
    /// </summary>
    /// <remarks>
    /// The list handles the event with a class handler. For a direct child container that has a source index, the
    /// current IsSelected value is applied to the selection model, and the event is marked handled unless the list
    /// itself is the source.
    /// </remarks>
    public static readonly RoutedEvent<RoutedEventArgs> IsSelectedChangedEvent =
        RoutedEvent.Register<ListView, RoutedEventArgs>("IsSelectedChanged", RoutingStrategies.Bubble);

    /// <summary>
    /// Identifies the <see cref="SelectionChanged" /> routed event.
    /// </summary>
    public static readonly RoutedEvent<ListViewSelectionChangedEventArgs> SelectionChangedEvent =
        RoutedEvent.Register<ListView, ListViewSelectionChangedEventArgs>(nameof(SelectionChanged), RoutingStrategies.Bubble);

    /// <summary>
    /// Raised after the selected items change.
    /// </summary>
    /// <remarks>
    /// The list raises it once the selection model, <see cref="SelectedValue" /> and the item containers are
    /// updated, and only for a change that alters the selected items. The event bubbles.
    /// </remarks>
    public event EventHandler<ListViewSelectionChangedEventArgs>? SelectionChanged
    {
        add => AddHandler(SelectionChangedEvent, value);
        remove => RemoveHandler(SelectionChangedEvent, value);
    }

    #endregion

    private readonly ListViewSelectionModel _selection = new();
    private IListCollectionEntryView? _entryView;
    private bool _entryEventsAttached;
    private bool _ignoreContainerSelectionChanged;
    private bool _hasScrolledToSelectedItem;
    private object? _selectedValue;
    private BindingEvaluator<object?>? _selectedValueBindingEvaluator;
    private string _textSearchTerm = string.Empty;
    private int _lastSelectedIndex = -1;
    private IReadOnlyList<int> _lastSelectedIndexes = Array.Empty<int>();
    private object? _lastSelectedItem;
    private IReadOnlyList<object?> _lastSelectedItems = Array.Empty<object?>();
    private object? _lastSelectedValue;

    static bool HasAllFlags(SelectionMode value, SelectionMode flags) => (value & flags) == flags;

    /// <summary>
    /// Returns whether the container is marked as selected.
    /// </summary>
    public static bool GetIsSelected(Control control) => control.GetValue(IsSelectedProperty);
    /// <summary>
    /// Marks the container as selected or not selected.
    /// </summary>
    /// <remarks>
    /// Only the value on the container changes. The list applies it to its selection model when the container raises
    /// the IsSelectedChanged routed event.
    /// </remarks>
    public static void SetIsSelected(Control control, bool value) => control.SetValue(IsSelectedProperty, value);

    /// <summary>
    /// Calls the base implementation, then scrolls the selection anchor into view when
    /// <see cref="AutoScrollToSelectedItem" /> is set.
    /// </summary>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        AutoScrollToSelectedItemIfNecessary(GetAnchorIndex());
    }

    internal void AttachSelectionView()
    {
        var view = _collectionView as IListCollectionEntryView;
        if (!ReferenceEquals(_entryView, view))
        {
            if (_entryView is not null && _entryEventsAttached)
            {
                _entryView.EntryChangePrepared -= OnEntryChangePrepared;
                _entryView.EntryChangeCommitted -= OnEntryChangeCommitted;
                _entryView.ProjectionCommitted -= OnProjectionCommitted;
            }

            _entryView = view;
            _selection.AttachView(view);
            if (_entryView is not null)
            {
                _entryView.EntryChangePrepared += OnEntryChangePrepared;
                _entryView.EntryChangeCommitted += OnEntryChangeCommitted;
                _entryView.ProjectionCommitted += OnProjectionCommitted;
                _entryEventsAttached = true;
            }
        }

        _selection.Mode = SelectionMode;
        UpdateContainerSelection();
    }

    private ListViewSelectionModel GetOrCreateSelectionModel()
    {
        AttachSelectionView();
        return _selection;
    }

    /// <summary>
    /// Applies the selection bookkeeping of this part to a property change and ignores every other property.
    /// </summary>
    /// <remarks>
    /// <see cref="AutoScrollToSelectedItem" /> starts the scroll to the selection anchor and
    /// <see cref="WrapSelection" /> is applied to keyboard navigation.
    /// <see cref="SelectionMode" /> is passed to the selection model, and a changed
    /// <see cref="SelectedValueBinding" /> evaluates <see cref="SelectedValue" /> again.
    /// Call it from the <c>OnPropertyChanged</c> override.
    /// </remarks>
    protected void HandlePropertyChangedForSelecting(AvaloniaPropertyChangedEventArgs change)
    {
        if (change.Property == AutoScrollToSelectedItemProperty)
        {
            AutoScrollToSelectedItemIfNecessary(GetAnchorIndex());
        }
        else if (change.Property == SelectionModeProperty)
        {
            _selection.Mode = change.GetNewValue<SelectionMode>();
        }
        else if (change.Property == WrapSelectionProperty)
        {
            this.SetWrapFocus(WrapSelection);
        }
        else if (change.Property == SelectedValueBindingProperty)
        {
            UpdateSelectedValueFromItem();
        }
    }

    /// <summary>
    /// Returns the item container that is, or visually contains, <paramref name="eventSource" />, or
    /// <see langword="null" /> when the source lies outside the item containers.
    /// </summary>
    /// <remarks>
    /// The visual tree is walked upwards; only a direct child of this list with a valid item index counts.
    /// </remarks>
    protected Control? GetContainerFromEventSource(object? eventSource)
    {
        for (var current = eventSource as Visual; current is not null; current = current.GetVisualParent())
        {
            if (current is Control control && control.Parent == this && IndexFromContainer(control) >= 0)
            {
                return control;
            }
        }

        return null;
    }

    /// <summary>
    /// Calls the base implementation, then updates the container for its view index <paramref name="index" />:
    /// the entry it displays, its split-line visibility and its selected state.
    /// </summary>
    /// <remarks>
    /// Only <see cref="ListViewItem" /> containers are affected. The container that displays the selection anchor
    /// becomes the tab-once active element.
    /// </remarks>
    protected override void ContainerForItemPreparedOverride(Control container, object? item, int index)
    {
        base.ContainerForItemPreparedOverride(container, item, index);
        SetContainerEntry(container, index);
        if (container is ListViewItem listItem)
        {
            listItem.IsSplitLineVisible = ShouldShowSplitLine(index == ItemCount - 1);
        }
        MarkContainerSelected(container, container is ListViewItem selectedItem && selectedItem.EntryId is not null && _selection.SelectedEntryIds.Contains(selectedItem.EntryId.Value));
        // Group headers have no source index; with no anchor (-1) they must not become the Tab target.
        if (_selection.AnchorIndex >= 0 && _selection.AnchorIndex == SourceIndexFromViewIndex(index))
        {
            KeyboardNavigation.SetTabOnceActiveElement(this, container);
        }
    }

    /// <summary>
    /// Calls the base implementation, then repeats the per-container update of
    /// <see cref="ContainerForItemPreparedOverride" /> for the new view index <paramref name="newIndex" />.
    /// </summary>
    protected override void ContainerIndexChangedOverride(Control container, int oldIndex, int newIndex)
    {
        base.ContainerIndexChangedOverride(container, oldIndex, newIndex);
        SetContainerEntry(container, newIndex);
        if (container is ListViewItem listItem)
        {
            listItem.IsSplitLineVisible = ShouldShowSplitLine(newIndex == ItemCount - 1);
        }
        MarkContainerSelected(container, container is ListViewItem selectedItem && selectedItem.EntryId is not null && _selection.SelectedEntryIds.Contains(selectedItem.EntryId.Value));
    }

    private void SetContainerEntry(Control container, int viewIndex)
    {
        if (container is not ListViewItem listItem || _entryView?.TryGetViewNode(viewIndex, out var node) != true)
        {
            return;
        }

        listItem.EntryId = node!.Entry?.Id;
        listItem.IsGroupItem = node.IsGroupHeader;
    }

    /// <summary>
    /// Resolves the source index of the item that the container displays.
    /// </summary>
    /// <returns>
    /// <see langword="true" /> when <paramref name="container" /> is a <see cref="ListViewItem" /> that displays an
    /// item with a source index, with <paramref name="sourceIndex" /> set to that index. <see langword="false" /> for a
    /// group header, for any other container or when no collection view is attached, with <paramref name="sourceIndex"
    /// /> set to -1.
    /// </returns>
    protected bool TryGetSourceIndexFromContainer(Control container, out int sourceIndex)
    {
        if (container is ListViewItem listItem && listItem.EntryId is not null &&
            _entryView?.TryGetSourceIndex(listItem.EntryId.Value, out sourceIndex) == true)
        {
            return true;
        }

        sourceIndex = -1;
        return false;
    }

    /// <summary>
    /// Resolves the source item at a source index.
    /// </summary>
    /// <returns>
    /// <see langword="true" /> when the collection view has a source entry at <paramref name="sourceIndex" />, with
    /// <paramref name="item" /> set to that item.
    /// <see langword="false" /> otherwise, with <paramref name="item" /> set to <see langword="null" />.
    /// </returns>
    protected bool TryGetSourceItem(int sourceIndex, out object? item)
    {
        if (_entryView?.TryGetSourceEntry(sourceIndex, out var entry) == true)
        {
            item = entry!.Item;
            return true;
        }

        item = null;
        return false;
    }

    /// <summary>
    /// Resolves the source index of the item at a view index of the current view.
    /// </summary>
    /// <returns>
    /// <see langword="true" /> when the current view has an item at <paramref name="viewIndex" /> that has a
    /// source index, with <paramref name="sourceIndex" /> set to that index.
    /// <see langword="false" /> for a group header, for an index outside the view or when no collection view is
    /// attached, with <paramref name="sourceIndex" /> set to -1.
    /// </returns>
    protected bool TryGetSourceIndexFromViewIndex(int viewIndex, out int sourceIndex)
    {
        if (_entryView?.TryGetViewNode(viewIndex, out var node) == true && node!.Entry is not null)
        {
            sourceIndex = _entryView.TryGetSourceIndex(node.Entry.Id, out var result) ? result : -1;
            return sourceIndex >= 0;
        }

        sourceIndex = -1;
        return false;
    }

    /// <summary>
    /// Resolves the view index in the current view of the item at a source index.
    /// </summary>
    /// <returns>
    /// <see langword="true" /> when the item is in the current view, with <paramref name="viewIndex" /> set to
    /// its view index.
    /// <see langword="false" /> when the item is not in the current view or no collection view is attached,
    /// with <paramref name="viewIndex" /> set to -1.
    /// </returns>
    protected bool TryGetViewIndexFromSourceIndex(int sourceIndex, out int viewIndex)
    {
        if (_entryView?.TryGetSourceEntry(sourceIndex, out var entry) == true &&
            _entryView.TryGetViewIndex(entry!.Id, out viewIndex))
        {
            return true;
        }

        viewIndex = -1;
        return false;
    }

    /// <summary>
    /// Enumerates the source indexes of the items in the current view, in view order.
    /// Group headers are skipped, and the result is empty when no collection view is attached.
    /// </summary>
    protected IEnumerable<int> EnumerateCurrentViewSourceIndexes()
    {
        if (_entryView is null)
        {
            yield break;
        }

        for (var index = 0; _entryView.TryGetViewNode(index, out var node); index++)
        {
            if (node!.Entry is not null && _entryView.TryGetSourceIndex(node.Entry.Id, out var sourceIndex))
            {
                yield return sourceIndex;
            }
        }
    }

    /// <summary>
    /// Enumerates the source indexes of every item of the source collection, including items that the current view
    /// does not show. The result is empty when no collection view is attached.
    /// </summary>
    protected IEnumerable<int> EnumerateSourceIndexes()
    {
        if (_entryView is null)
        {
            yield break;
        }

        for (var index = 0; index < _entryView.SourceEntryCount; index++)
        {
            yield return index;
        }
    }

    /// <summary>
    /// Returns a scope that combines the selection changes made before it is disposed into one change notification.
    /// </summary>
    protected IDisposable BeginSelectionBatchUpdate() => _selection.BeginSelectionBatchUpdate();

    /// <summary>
    /// Returns the source index of the item at the view index <paramref name="index" />, or -1 when the current
    /// view has no item with a source index there.
    /// </summary>
    protected int GlobalIndex(int index) => SourceIndexFromViewIndex(index);

    /// <summary>
    /// Returns the view index in the current view of the item at <paramref name="sourceIndex" />, or -1 when
    /// the item is not in the current view.
    /// </summary>
    protected int GlobalIndexLocalIndex(int sourceIndex)
        => TryGetViewIndexFromSourceIndex(sourceIndex, out var viewIndex) ? viewIndex : -1;

    /// <summary>
    /// Returns the source index of the item that <paramref name="container" /> displays, or -1 when it has none.
    /// </summary>
    protected int SelectionIndexFromContainer(Control container)
        => TryGetSourceIndexFromContainer(container, out var index) ? index : -1;

    /// <summary>
    /// Returns the source index of the item at the view index <paramref name="itemIndex" />, or -1 when the current
    /// view has no item with a source index there.
    /// </summary>
    protected int SelectionIndexFromItemIndex(int itemIndex)
        => SourceIndexFromViewIndex(itemIndex);

    /// <summary>
    /// Returns the view index in the current view of the item at <paramref name="sourceIndex" />, or -1 when
    /// the item is not in the current view.
    /// </summary>
    protected int ItemIndexFromSelectionIndex(int sourceIndex)
        => TryGetViewIndexFromSourceIndex(sourceIndex, out var viewIndex) ? viewIndex : -1;

    private int SourceIndexFromViewIndex(int viewIndex)
        => TryGetSourceIndexFromViewIndex(viewIndex, out var sourceIndex) ? sourceIndex : -1;

    /// <summary>
    /// Applies <c>UpdateSelection</c> to the item of the container that is, or visually contains,
    /// <paramref name="eventSource" />, passing the other arguments on unchanged.
    /// </summary>
    /// <returns>
    /// <see langword="true" /> when the source lies in an item container that has a source index.
    /// <see langword="false" /> for a group header, for any other element or when no collection view is attached.
    /// </returns>
    protected bool UpdateSelectionFromEventSource(object? eventSource, bool select = true, bool rangeModifier = false,
        bool toggleModifier = false, bool rightButton = false, bool fromFocus = false)
    {
        var container = GetContainerFromEventSource(eventSource);
        if (container is not ListViewItem listItem || listItem.IsGroupItem || !TryGetSourceIndexFromContainer(container, out var index))
        {
            return false;
        }

        UpdateSelection(index, select, rangeModifier, toggleModifier, rightButton, fromFocus);
        return true;
    }

    /// <summary>
    /// Moves the selection from the item container of the focused element in <paramref name="direction" /> and focuses
    /// the target container. The other parameters have the same effect as in the overload that takes a start container.
    /// A focused element outside the item containers is treated as no start container.
    /// </summary>
    /// <returns>
    /// <see langword="true" /> when the selection moved.
    /// <see langword="false" /> when no item container is the target.
    /// </returns>
    protected bool MoveSelection(NavigationDirection direction, bool wrap = false, bool rangeModifier = false)
    {
        var focused = TopLevel.GetTopLevel(this)?.FocusManager.GetFocusedElement();
        return MoveSelection(GetContainerFromEventSource(focused), direction, wrap, rangeModifier);
    }

    /// <summary>
    /// Moves the selection from the container <paramref name="from" /> to the next item container in
    /// <paramref name="direction" />, and focuses the target container.
    /// </summary>
    /// <returns>
    /// <see langword="true" /> when the selection moved.
    /// <see langword="false" /> when the presenter panel does not support directional navigation, or when the next
    /// container has no item with a source index, for example a group header.
    /// </returns>
    /// <remarks>
    /// A <see langword="null" /> <paramref name="from" /> starts at an end of the view: moving up or left targets
    /// the last item and any other direction targets the first item.
    /// <paramref name="wrap" /> moves past the last item to the first one and past the first item to the last one.
    /// <paramref name="rangeModifier" /> extends the selection from the anchor to the target item instead of
    /// replacing it.
    /// </remarks>
    protected bool MoveSelection(Control? from, NavigationDirection direction, bool wrap = false, bool rangeModifier = false)
    {
        if (Presenter?.Panel is not INavigableContainer panel)
        {
            return false;
        }

        if (from is null)
        {
            direction = direction switch
            {
                NavigationDirection.Up or NavigationDirection.Left => NavigationDirection.Last,
                _ => NavigationDirection.First,
            };
        }

        if (GetNextControl(panel, direction, from, wrap) is Control next && TryGetSourceIndexFromContainer(next, out var index))
        {
            UpdateSelection(index, true, rangeModifier);
            next.Focus();
            return true;
        }

        return false;
    }

    /// <summary>
    /// Applies an input-driven selection to the item at the source index <paramref name="index" />. The rules
    /// are checked in this order and the first match is applied:
    /// <paramref name="select" /> set to <see langword="false" /> deselects the item;
    /// a right click selects an item that is not selected yet and keeps the other selected items;
    /// with <paramref name="rangeModifier" />, <c>SelectionMode.Multiple</c> and an anchor, the range from the anchor to
    /// the item is selected;
    /// a toggle adds or removes the item;
    /// otherwise the item becomes the only selected item.
    /// </summary>
    /// <param name="index">
    /// A negative index or an index without a source item changes nothing.
    /// </param>
    /// <param name="select">
    /// The other arguments are ignored when it is <see langword="false" />.
    /// </param>
    /// <param name="rangeModifier">
    /// Selects the range from the selection anchor to the item.
    /// </param>
    /// <param name="toggleModifier">
    /// Toggles the item even when <c>SelectionMode.Toggle</c> is not set.
    /// </param>
    /// <param name="rightButton">
    /// The selection comes from a right click.
    /// </param>
    /// <param name="fromFocus">
    /// The selection comes from moving the focus, which never toggles.
    /// </param>
    /// <remarks>
    /// The item toggles when <paramref name="toggleModifier" /> or <c>SelectionMode.Toggle</c> is set and
    /// <paramref name="fromFocus" /> is not.
    /// A range replaces the selection unless it toggles too.
    /// A toggle that selects an item makes it the only selected item when <c>SelectionMode.Multiple</c> is not set.
    /// </remarks>
    protected void UpdateSelection(int index, bool select = true, bool rangeModifier = false, bool toggleModifier = false,
        bool rightButton = false, bool fromFocus = false)
    {
        if (index < 0 || _entryView?.TryGetSourceEntry(index, out var entry) != true || entry is null)
        {
            return;
        }

        var mode = SelectionMode;
        var multi = HasAllFlags(mode, SelectionMode.Multiple);
        var toggle = toggleModifier || HasAllFlags(mode, SelectionMode.Toggle);
        if (!select)
        {
            _selection.Deselect(index);
        }
        else if (rightButton)
        {
            if (!_selection.IsSelected(index))
            {
                _selection.Select(index);
            }
        }
        else if (rangeModifier && multi && _selection.AnchorIndex >= 0 && _entryView.TryGetSourceEntry(_selection.AnchorIndex, out var anchor))
        {
            _selection.SelectRangeToEntry(entry.Id, toggle);
        }
        else if (toggle && multi && !fromFocus)
        {
            if (_selection.IsSelected(index))
            {
                _selection.Deselect(index);
            }
            else
            {
                _selection.Select(index);
            }
        }
        else if (toggle && !multi && !fromFocus)
        {
            if (_selection.IsSelected(index))
            {
                _selection.Clear();
            }
            else
            {
                _selection.Select(index);
            }
        }
        else
        {
            using var batch = BeginSelectionBatchUpdate();
            _selection.Clear();
            _selection.Select(index);
        }
    }

    private void ContainerSelectionChanged(RoutedEventArgs e)
    {
        if (!_ignoreContainerSelectionChanged && e.Source is Control control && control.Parent == this &&
            TryGetSourceIndexFromContainer(control, out var index))
        {
            if (GetIsSelected(control))
            {
                _selection.Select(index);
            }
            else
            {
                _selection.Deselect(index);
            }
        }

        if (e.Source != this)
        {
            e.Handled = true;
        }
    }

    private void MarkContainerSelected(Control container, bool selected)
    {
        _ignoreContainerSelectionChanged = true;
        try { container.SetCurrentValue(IsSelectedProperty, selected); }
        finally { _ignoreContainerSelectionChanged = false; }
    }

    private void UpdateContainerSelection()
    {
        if (Presenter?.Panel is { } panel)
        {
            foreach (var child in panel.Children)
            {
                MarkContainerSelected(child, TryGetSourceIndexFromContainer(child, out var index) && _selection.IsSelected(index));
            }
        }
    }

    internal void HandleItemsViewCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (AlwaysSelected && SelectedIndex < 0)
        {
            var first = EnumerateCurrentViewSourceIndexes().FirstOrDefault(-1);
            if (first >= 0)
            {
                _selection.Select(first);
            }
        }
    }

    private void OnEntryChangePrepared(object? sender, ListCollectionEntryChangeEventArgs e)
    {
        _selection.OnEntryChangePrepared(e.ChangeSet);
    }

    private void OnEntryChangeCommitted(object? sender, EventArgs e)
    {
        _selection.OnEntryChangeCommitted();
        UpdateContainerSelection();
        UpdateSelectedValueFromItem();
        RaiseSelectionProjectionChanges();
    }

    private void OnProjectionCommitted(object? sender, EventArgs e)
    {
        UpdateContainerSelection();
        UpdateSelectedValueFromItem();
        RaiseSelectionProjectionChanges();
    }

    private void HandleSelectionChange(ListViewSelectionChange change)
    {
        UpdateSelectedValueFromItem();
        UpdateContainerSelection();
        RaiseSelectionProjectionChanges(change);
        if (!change.HasBusinessSelectionChange)
        {
            return;
        }

        RaiseEvent(new ListViewSelectionChangedEventArgs(
            SelectionChangedEvent,
            change.DeselectedIndexes,
            change.DeselectedItems,
            change.SelectedIndexes,
            change.SelectedItems));
    }

    private void RaiseSelectionProjectionChanges(ListViewSelectionChange? change = null)
    {
        var selectedIndex  = SelectedIndex;
        var selectedIndexes = SelectedIndexes;
        var selectedItem   = SelectedItem;
        var selectedItems  = SelectedItems;
        var selectedValue  = _selectedValue;

        if (_lastSelectedIndex != selectedIndex)
        {
            RaisePropertyChanged(SelectedIndexProperty, _lastSelectedIndex, selectedIndex);
        }

        if (!_lastSelectedIndexes.SequenceEqual(selectedIndexes))
        {
            RaisePropertyChanged(SelectedIndexesProperty, _lastSelectedIndexes, selectedIndexes);
        }

        if (!ReferenceEquals(_lastSelectedItem, selectedItem))
        {
            RaisePropertyChanged(SelectedItemProperty, _lastSelectedItem, selectedItem);
        }

        if (!_lastSelectedItems.SequenceEqual(selectedItems, ReferenceEqualityComparer.Instance))
        {
            RaisePropertyChanged(SelectedItemsProperty, _lastSelectedItems, selectedItems);
        }

        if (!Equals(_lastSelectedValue, selectedValue))
        {
            RaisePropertyChanged(SelectedValueProperty, _lastSelectedValue, selectedValue);
        }

        _lastSelectedIndex   = selectedIndex;
        _lastSelectedIndexes = selectedIndexes;
        _lastSelectedItem    = selectedItem;
        _lastSelectedItems   = selectedItems;
        _lastSelectedValue   = selectedValue;
    }

    private void UpdateSelectedValueFromItem()
    {
        var item = SelectedItem;
        object? value = item;
        if (item is not null && SelectedValueBinding is not null)
        {
            _selectedValueBindingEvaluator ??= BindingEvaluator<object?>.TryCreate(SelectedValueBinding);
            _selectedValueBindingEvaluator!.UpdateBinding(SelectedValueBinding);
            value = _selectedValueBindingEvaluator.Evaluate(item);
        }

        if (!ReferenceEquals(_selectedValue, value))
        {
            var old = _selectedValue;
            _selectedValue = value;
            RaisePropertyChanged(SelectedValueProperty, old, value);
        }
    }

    private void AutoScrollToSelectedItemIfNecessary(int sourceIndex)
    {
        if (!AutoScrollToSelectedItem || _hasScrolledToSelectedItem || Presenter is null || sourceIndex < 0 || !this.IsAttachedToVisualTree())
        {
            return;
        }

        if (!TryGetViewIndexFromSourceIndex(sourceIndex, out var viewIndex))
        {
            return;
        }

        Dispatcher.Post(() => { ScrollIntoView(viewIndex); _hasScrolledToSelectedItem = true; });
    }

    internal int GetAnchorIndex() => _selection.AnchorIndex;

    internal void NotifyApplyTemplateForSelecting()
    {
        _hasScrolledToSelectedItem = false;
        AutoScrollToSelectedItemIfNecessary(GetAnchorIndex());
    }

    internal void BeginUpdating() { }
    internal void EndUpdating() { AttachSelectionView(); }

    /// <summary>
    /// Handles typed text as an item search when <see cref="IsTextSearchEnabled" /> is set.
    /// The text is appended to the search term and the first item of the current view whose <c>ToString()</c> result
    /// starts with the term, ignoring case, is selected.
    /// Such text input is marked handled; text input that is already handled is ignored.
    /// </summary>
    protected void OnTextInputForSelecting(TextInputEventArgs e)
    {
        if (!e.Handled && IsTextSearchEnabled && !string.IsNullOrEmpty(e.Text))
        {
            _textSearchTerm += e.Text;
            var index = GetIndexFromTextSearch(_textSearchTerm);
            if (index >= 0)
            {
                SelectedIndex = index;
            }

            e.Handled = true;
        }
    }

    private int GetIndexFromTextSearch(string term)
    {
        if (_entryView is null)
        {
            return -1;
        }

        foreach (var sourceIndex in EnumerateCurrentViewSourceIndexes())
        {
            if (_entryView.TryGetSourceEntry(sourceIndex, out var entry) && entry!.Item?.ToString()?.StartsWith(term, StringComparison.CurrentCultureIgnoreCase) == true)
            {
                return sourceIndex;
            }
        }
        return -1;
    }

    /// <summary>
    /// Calls the base implementation. The selection bookkeeping of this part runs in
    /// <see cref="OnDataContextEndUpdate" /> after the update.
    /// </summary>
    protected override void OnDataContextBeginUpdate() { base.OnDataContextBeginUpdate(); BeginUpdating(); }
    /// <summary>
    /// Calls the base implementation, then binds the selection model to the current collection view again and
    /// refreshes the selected state of the item containers.
    /// </summary>
    protected override void OnDataContextEndUpdate() { base.OnDataContextEndUpdate(); EndUpdating(); }

    /// <inheritdoc />
    protected override void UpdateDataValidation(AvaloniaProperty property, BindingValueType state, Exception? error)
    {
        base.UpdateDataValidation(property, state, error);
    }
}
