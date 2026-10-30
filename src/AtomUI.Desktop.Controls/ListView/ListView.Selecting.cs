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

    public static readonly StyledProperty<bool> AutoScrollToSelectedItemProperty =
        AvaloniaProperty.Register<ListView, bool>(nameof(AutoScrollToSelectedItem), true);

    public static readonly DirectProperty<ListView, IListViewSelection> SelectionProperty =
        AvaloniaProperty.RegisterDirect<ListView, IListViewSelection>(nameof(Selection), o => o.Selection);

    public static readonly DirectProperty<ListView, int> SelectedIndexProperty =
        AvaloniaProperty.RegisterDirect<ListView, int>(
            nameof(SelectedIndex), o => o.SelectedIndex, (o, value) => o.SelectedIndex = value,
            unsetValue: -1, defaultBindingMode: BindingMode.TwoWay);

    public static readonly DirectProperty<ListView, IReadOnlyList<int>> SelectedIndexesProperty =
        AvaloniaProperty.RegisterDirect<ListView, IReadOnlyList<int>>(nameof(SelectedIndexes), o => o.SelectedIndexes);

    public static readonly DirectProperty<ListView, object?> SelectedItemProperty =
        AvaloniaProperty.RegisterDirect<ListView, object?>(nameof(SelectedItem), o => o.SelectedItem);

    public static readonly DirectProperty<ListView, IReadOnlyList<object?>> SelectedItemsProperty =
        AvaloniaProperty.RegisterDirect<ListView, IReadOnlyList<object?>>(nameof(SelectedItems), o => o.SelectedItems);

    public static readonly DirectProperty<ListView, object?> SelectedValueProperty =
        AvaloniaProperty.RegisterDirect<ListView, object?>(nameof(SelectedValue), o => o.SelectedValue);

    public static readonly StyledProperty<BindingBase?> SelectedValueBindingProperty =
        AvaloniaProperty.Register<ListView, BindingBase?>(nameof(SelectedValueBinding));

    public static readonly StyledProperty<ListItemKeySelector?> ItemKeySelectorProperty =
        AvaloniaProperty.Register<ListView, ListItemKeySelector?>(nameof(ItemKeySelector));

    public static readonly StyledProperty<SelectionMode> SelectionModeProperty =
        AvaloniaProperty.Register<ListView, SelectionMode>(nameof(SelectionMode));

    public static readonly StyledProperty<bool> IsSelectedProperty =
        AvaloniaProperty.RegisterAttached<ListView, Control, bool>("IsSelected", defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<bool> IsTextSearchEnabledProperty =
        AvaloniaProperty.Register<ListView, bool>(nameof(IsTextSearchEnabled), false);

    public static readonly StyledProperty<bool> WrapSelectionProperty =
        AvaloniaProperty.Register<ListView, bool>(nameof(WrapSelection), false);

    public bool AutoScrollToSelectedItem
    {
        get => GetValue(AutoScrollToSelectedItemProperty);
        set => SetValue(AutoScrollToSelectedItemProperty, value);
    }

    public IListViewSelection Selection => _selection;

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

    public IReadOnlyList<int> SelectedIndexes => _selection.SelectedIndexes;
    public object? SelectedItem => _selection.SelectedItem;
    public IReadOnlyList<object?> SelectedItems => _selection.SelectedItems;

    public BindingBase? SelectedValueBinding
    {
        get => GetValue(SelectedValueBindingProperty);
        set => SetValue(SelectedValueBindingProperty, value);
    }

    public ListItemKeySelector? ItemKeySelector
    {
        get => GetValue(ItemKeySelectorProperty);
        set => SetValue(ItemKeySelectorProperty, value);
    }

    public object? SelectedValue => _selectedValue;

    public bool IsTextSearchEnabled
    {
        get => GetValue(IsTextSearchEnabledProperty);
        set => SetValue(IsTextSearchEnabledProperty, value);
    }

    public bool WrapSelection
    {
        get => GetValue(WrapSelectionProperty);
        set => SetValue(WrapSelectionProperty, value);
    }

    public SelectionMode SelectionMode
    {
        get => GetValue(SelectionModeProperty);
        set => SetValue(SelectionModeProperty, value);
    }

    protected bool AlwaysSelected => (SelectionMode & SelectionMode.AlwaysSelected) == SelectionMode.AlwaysSelected;

    #endregion

    #region 公共事件定义

    public static readonly RoutedEvent<RoutedEventArgs> IsSelectedChangedEvent =
        RoutedEvent.Register<ListView, RoutedEventArgs>("IsSelectedChanged", RoutingStrategies.Bubble);

    public static readonly RoutedEvent<ListViewSelectionChangedEventArgs> SelectionChangedEvent =
        RoutedEvent.Register<ListView, ListViewSelectionChangedEventArgs>(nameof(SelectionChanged), RoutingStrategies.Bubble);

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

    public static bool GetIsSelected(Control control) => control.GetValue(IsSelectedProperty);
    public static void SetIsSelected(Control control, bool value) => control.SetValue(IsSelectedProperty, value);

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

    protected override void ContainerForItemPreparedOverride(Control container, object? item, int index)
    {
        base.ContainerForItemPreparedOverride(container, item, index);
        SetContainerEntry(container, index);
        if (container is ListViewItem listItem)
        {
            listItem.IsSplitLineVisible = ShouldShowSplitLine(index == ItemCount - 1);
        }
        MarkContainerSelected(container, container is ListViewItem selectedItem && selectedItem.EntryId is not null && _selection.SelectedEntryIds.Contains(selectedItem.EntryId.Value));
        if (_selection.AnchorIndex == SourceIndexFromViewIndex(index))
        {
            KeyboardNavigation.SetTabOnceActiveElement(this, container);
        }
    }

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

    protected IDisposable BeginSelectionBatchUpdate() => _selection.BeginSelectionBatchUpdate();

    protected int GlobalIndex(int index) => SourceIndexFromViewIndex(index);

    protected int GlobalIndexLocalIndex(int sourceIndex)
        => TryGetViewIndexFromSourceIndex(sourceIndex, out var viewIndex) ? viewIndex : -1;

    protected int SelectionIndexFromContainer(Control container)
        => TryGetSourceIndexFromContainer(container, out var index) ? index : -1;

    protected int SelectionIndexFromItemIndex(int itemIndex)
        => SourceIndexFromViewIndex(itemIndex);

    protected int ItemIndexFromSelectionIndex(int sourceIndex)
        => TryGetViewIndexFromSourceIndex(sourceIndex, out var viewIndex) ? viewIndex : -1;

    private int SourceIndexFromViewIndex(int viewIndex)
        => TryGetSourceIndexFromViewIndex(viewIndex, out var sourceIndex) ? sourceIndex : -1;

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

    protected bool MoveSelection(NavigationDirection direction, bool wrap = false, bool rangeModifier = false)
    {
        var focused = TopLevel.GetTopLevel(this)?.FocusManager.GetFocusedElement();
        return MoveSelection(GetContainerFromEventSource(focused), direction, wrap, rangeModifier);
    }

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

    protected override void OnDataContextBeginUpdate() { base.OnDataContextBeginUpdate(); BeginUpdating(); }
    protected override void OnDataContextEndUpdate() { base.OnDataContextEndUpdate(); EndUpdating(); }

    protected override void UpdateDataValidation(AvaloniaProperty property, BindingValueType state, Exception? error)
    {
        base.UpdateDataValidation(property, state, error);
    }
}
