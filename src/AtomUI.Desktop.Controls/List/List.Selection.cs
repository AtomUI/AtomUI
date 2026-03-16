using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using AtomUI.Controls.Utils;
using AtomUI.Desktop.Controls.Primitives;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Selection;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Input.Raw;
using Avalonia.Interactivity;
using Avalonia.Metadata;
using Avalonia.Threading;

namespace AtomUI.Desktop.Controls;

public partial class List
{
    #region 公共属性定义
    public static readonly StyledProperty<bool> IsSelectableProperty =
        AvaloniaProperty.Register<List, bool>(nameof(IsSelectable), true);

    public static readonly DirectProperty<List, int> SelectedIndexProperty =
        AvaloniaProperty.RegisterDirect<List, int>(
            nameof(SelectedIndex),
            o => o.SelectedIndex,
            (o, v) => o.SelectedIndex = v,
            unsetValue: -1,
            defaultBindingMode: BindingMode.TwoWay);
    
    public static readonly DirectProperty<List, IList?> SelectedItemsProperty =
        AvaloniaProperty.RegisterDirect<List, IList?>(
            nameof(SelectedItems),
            o => o.SelectedItems,
            (o, v) => o.SelectedItems = v);
    
    public static readonly DirectProperty<List, object?> SelectedItemProperty =
        AvaloniaProperty.RegisterDirect<List, object?>(
            nameof(SelectedItem),
            o => o.SelectedItem,
            (o, v) => o.SelectedItem = v,
            defaultBindingMode: BindingMode.TwoWay, enableDataValidation: true);
    
    public static readonly StyledProperty<object?> SelectedValueProperty =
        AvaloniaProperty.Register<List, object?>(nameof(SelectedValue),
            defaultBindingMode: BindingMode.TwoWay);
    
    public static readonly StyledProperty<IBinding?> SelectedValueBindingProperty =
        AvaloniaProperty.Register<List, IBinding?>(nameof(SelectedValueBinding));
    
    public static readonly StyledProperty<SelectionMode> SelectionModeProperty =
        AvaloniaProperty.Register<List, SelectionMode>(nameof(SelectionMode));
    
    public static readonly StyledProperty<bool> IsShowSelectedIndicatorProperty =
        AvaloniaProperty.Register<List, bool>(nameof(IsShowSelectedIndicator), false);
    
    public int SelectedIndex
    {
        get
        {
            // When a Begin/EndInit/DataContext update is in place we return the value to be
            // updated here, even though it's not yet active and the property changed notification
            // has not yet been raised. If we don't do this then the old value will be written back
            // to the source when two-way bound, and the update value will be lost.
            if (_updateState is not null)
            {
                return _updateState.SelectedIndex.HasValue ?
                    _updateState.SelectedIndex.Value :
                    TryGetExistingSelection()?.SelectedIndex ?? -1;
            }

            return Selection.SelectedIndex;
        }
        set
        {
            if (_updateState != null)
            {
                _updateState.SelectedIndex = value;
            }
            else
            {
                Selection.SelectedIndex = value;
            }
        }
    }

    public IList? SelectedItems
    {
        get
        {
            // See SelectedIndex setter for more information.
            if (_updateState?.SelectedItems.HasValue == true)
            {
                return _updateState.SelectedItems.Value;
            }

            if (Selection is ListViewSelectionModel ism)
            {
                var result = ism.WritableSelectedItems;
                _oldSelectedItems.SetTarget(result);
                return result;
            }

            return null;
        }

        set
        {
            if (_updateState != null)
            {
                _updateState.SelectedItems = new Optional<IList?>(value);
            }
            else if (Selection is ListViewSelectionModel i)
            {
                i.WritableSelectedItems = value;
            }
            else
            {
                throw new InvalidOperationException("Cannot set both Selection and SelectedItems.");
            }
        }
    }

    public object? SelectedItem
    {
        get
        {
            // See SelectedIndex getter for more information.
            if (_updateState is not null)
            {
                return _updateState.SelectedItem.HasValue ?
                    _updateState.SelectedItem.Value :
                    TryGetExistingSelection()?.SelectedItem;
            }

            return Selection.SelectedItem;
        }
        set
        {
            if (_updateState != null)
            {
                _updateState.SelectedItem = value;
            }
            else
            {
                Selection.SelectedItem = value;
            }
        }
    }
    
    public object? SelectedValue
    {
        get => GetValue(SelectedValueProperty);
        set => SetValue(SelectedValueProperty, value);
    }
    
    [AssignBinding]
    [InheritDataTypeFromItems(nameof(ItemsSource))]
    public IBinding? SelectedValueBinding
    {
        get => GetValue(SelectedValueBindingProperty);
        set => SetValue(SelectedValueBindingProperty, value);
    }
    
    public SelectionMode SelectionMode
    {
        get => GetValue(SelectionModeProperty);
        set => SetValue(SelectionModeProperty, value);
    }
    
    public bool IsSelectable
    {
        get => GetValue(IsSelectableProperty);
        set => SetValue(IsSelectableProperty, value);
    }
    
    [AllowNull]
    protected ISelectionModel Selection
    {
        get => _updateState?.Selection.HasValue == true ?
            _updateState.Selection.Value :
            GetOrCreateSelectionModel();
        set
        {
            value ??= CreateDefaultSelectionModel();

            if (_updateState != null)
            {
                _updateState.Selection = new Optional<ISelectionModel>(value);
            }
            else if (_selection != value)
            {
                if (value.Source != null && value.Source != _listCollectionView?.SourceCollection)
                {
                    throw new ArgumentException(
                        "The supplied ISelectionModel already has an assigned Source but this " +
                        "collection is different to the Items on the control.");
                }

                var oldSelection = _selection?.SelectedItems.ToArray();
                DeinitializeSelectionModel(_selection);
                _selection = value;

                if (oldSelection?.Length > 0)
                {
                    RaiseEvent(new SelectionChangedEventArgs(
                        SelectionChangedEvent,
                        oldSelection,
                        Array.Empty<object>()));
                }

                InitializeSelectionModel(_selection);
                var selectedItems = SelectedItems;
                _oldSelectedItems.TryGetTarget(out var oldSelectedItems);
                if (oldSelectedItems != selectedItems)
                {
                    RaisePropertyChanged(SelectedItemsProperty, oldSelectedItems, selectedItems);
                    _oldSelectedItems.SetTarget(selectedItems);
                }
            }
        }
    }
    
    #endregion

    #region 公共事件定义

    public static readonly RoutedEvent<RoutedEventArgs> IsSelectedChangedEvent =
        RoutedEvent.Register<SelectingItemsControl, RoutedEventArgs>(
            "IsSelectedChanged",
            RoutingStrategies.Bubble);

    #endregion
    
    private UpdateState? _updateState;
    private ISelectionModel? _selection;
    private int _oldSelectedIndex;
    private WeakReference _oldSelectedItem = new(null);
    private WeakReference<IList?> _oldSelectedItems = new(null);
    private BindingEvaluator<object?>? _selectedValueBindingEvaluator;
    private bool _isSelectionChangeActive;
    private bool _skipSyncSelection;
    private RawKeyEventArgs? _rawKeyEventArgs;
    
    protected bool AlwaysSelected => SelectionMode.HasAllFlags(SelectionMode.AlwaysSelected);
    
    public override void BeginInit()
    {
        base.BeginInit();
        BeginUpdating();
    }
    
    public override void EndInit()
    {
        base.EndInit();
        EndUpdating();
    }
    
    private void BeginUpdating()
    {
        _updateState ??= new UpdateState();
        _updateState.UpdateCount++;
    }

    private void EndUpdating()
    {
        if (_updateState != null && --_updateState.UpdateCount == 0)
        {
            var state = _updateState;
            _updateState = null;

            if (state.Selection.HasValue)
            {
                Selection = state.Selection.Value;
            }

            if (_selection is ListViewSelectionModel s)
            {
                s.Update(_listCollectionView?.SourceCollection, state.SelectedItems);
            }
            else
            {
                if (state.SelectedItems.HasValue)
                {
                    SelectedItems = state.SelectedItems.Value;
                }

                TryInitializeSelectionSource(Selection, false);
            }

            if (state.SelectedValue.HasValue)
            {
                var item = FindItemWithValue(state.SelectedValue.Value);
                if (item != AvaloniaProperty.UnsetValue)
                {
                    state.SelectedItem = item;
                }
            }

            // SelectedIndex vs SelectedItem:
            // - If only one has a value, use it
            // - If both have a value, prefer the one having a "non-empty" value, e.g. not -1 nor null
            // - If both have a "non-empty" value, prefer the index
            if (state.SelectedIndex.HasValue)
            {
                var selectedIndex = state.SelectedIndex.Value;
                if (selectedIndex >= 0 || !state.SelectedItem.HasValue)
                {
                    SelectedIndex = selectedIndex;
                }
                else
                {
                    SelectedItem = state.SelectedItem.Value;
                }
            }
            else if (state.SelectedItem.HasValue)
            {
                SelectedItem = state.SelectedItem.Value;
            }

            if (AlwaysSelected && SelectedIndex == -1 && ItemCount > 0)
            {
                SelectedIndex = 0;
            }
        }
    }
    
    public static bool GetIsSelected(Control control) => control.GetValue(SelectingItemsControl.IsSelectedProperty);
    public static void SetIsSelected(Control control, bool value) => control.SetValue(SelectingItemsControl.IsSelectedProperty, value);
    
    private ISelectionModel? TryGetExistingSelection()
        => _updateState?.Selection.HasValue == true ? _updateState.Selection.Value : _selection;

    private void NotifyItemsSourceChangedForSelection()
    {
        SetCurrentValue(SelectedItemsProperty, null);
        SetCurrentValue(SelectedItemProperty, null);
        SetCurrentValue(SelectedIndexProperty, -1);
        //Do not change SelectedIndex during initialization
        if (_updateState is not null)
        {
            return;
        }
        if (AlwaysSelected && SelectedIndex == -1 && ItemCount > 0)
        {
            SelectedIndex = 0;
        }
        if (_updateState is null)
        {
            TryInitializeSelectionSource(_selection, true);
        }
    }
    
    private ISelectionModel CreateDefaultSelectionModel()
    {
        return new ListViewSelectionModel
        {
            SingleSelect = !SelectionMode.HasAllFlags(SelectionMode.Multiple),
        };
    }
    
    private ISelectionModel GetOrCreateSelectionModel()
    {
        if (_selection is null)
        {
            _selection = CreateDefaultSelectionModel();
            InitializeSelectionModel(_selection);
        }

        return _selection;
    }
    
    private void TryInitializeSelectionSource(ISelectionModel? selection, bool shouldSelectItemFromSelectedValue)
    {
        if (selection is not null && _listCollectionView != null)
        {
            // InternalSelectionModel keeps the SelectedIndex and SelectedItem values before the ItemsSource is set.
            // However, SelectedValue isn't part of that model, so we have to set the SelectedItem from
            // SelectedValue manually now that we have a source.
            //
            // While this works, this is messy: we effectively have "lazy selection initialization" in 3 places:
            //  - UpdateState (all selection properties, for BeginInit/EndInit)
            //  - InternalSelectionModel (SelectedIndex/SelectedItem)
            //  - SelectedItemsControl (SelectedValue)
            //
            // There's the opportunity to have a single place responsible for this logic.
            // TODO12 (or 13): refactor this.
            if (shouldSelectItemFromSelectedValue && selection.SelectedIndex == -1 && selection.SelectedItem is null)
            {
                var item = FindItemWithValue(SelectedValue);
                if (item != AvaloniaProperty.UnsetValue)
                    selection.SelectedItem = item;
            }

            selection.Source = _listCollectionView.SourceCollection;
        }
    }
    
    private void UpdateSelectedValueFromItem()
    {
        if (_isSelectionChangeActive)
        {
            return;
        }

        var binding = SelectedValueBinding;
        var item    = SelectedItem;

        if (binding is null || item is null)
        {
            // No SelectedValueBinding, SelectedValue is Item itself
            try
            {
                _isSelectionChangeActive = true;
                SetCurrentValue(SelectedValueProperty, item);
            }
            finally
            {
                _isSelectionChangeActive = false;
            }
            return;
        }

        var bindingEvaluator = GetSelectedValueBindingEvaluator(binding);

        try
        {
            _isSelectionChangeActive = true;
            SetCurrentValue(SelectedValueProperty, bindingEvaluator.Evaluate(item));
        }
        finally
        {
            _isSelectionChangeActive = false;
        }
    }
    
    private void SelectItemWithValue(object? value)
    {
        if (ItemCount == 0 || _isSelectionChangeActive)
        {
            return;
        }

        try
        {
            _isSelectionChangeActive = true;
            var si = FindItemWithValue(value);
            if (si != AvaloniaProperty.UnsetValue)
            {
                SelectedItem = si;
            }
            else
            {
                SelectedItem = null;
            }
        }
        finally
        {
            _isSelectionChangeActive = false;
        }
    }
    
    private void HandleSelectionModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        _skipSyncSelection = true;
        if (e.PropertyName == nameof(ISelectionModel.SelectedIndex))
        {
            var selectedIndex    = SelectedIndex;
            var oldSelectedIndex = _oldSelectedIndex;
            if (_oldSelectedIndex != selectedIndex)
            {
                RaisePropertyChanged(SelectedIndexProperty, oldSelectedIndex, selectedIndex);
                _oldSelectedIndex = selectedIndex;
            }
        }
        else if (e.PropertyName == nameof(ISelectionModel.SelectedItem))
        {
            var selectedItem    = SelectedItem;
            var oldSelectedItem = _oldSelectedItem.Target;
            if (selectedItem != oldSelectedItem)
            {
                RaisePropertyChanged(SelectedItemProperty, oldSelectedItem, selectedItem);
                _oldSelectedItem.Target = selectedItem;
            }
        }
        else 
        if (e.PropertyName == nameof(ListViewSelectionModel.WritableSelectedItems))
        {
            _oldSelectedItems.TryGetTarget(out var oldSelectedItems);
            if (oldSelectedItems != (Selection as ListViewSelectionModel)?.SelectedItems)
            {
                var selectedItems = SelectedItems;
                RaisePropertyChanged(SelectedItemsProperty, oldSelectedItems, selectedItems);
                _oldSelectedItems.SetTarget(selectedItems);
            }
        }
        else if (e.PropertyName == nameof(ISelectionModel.Source))
        {
            ClearValue(SelectedValueProperty);
        }
    }
    
    private void HandleSelectionModelSelectionChanged(object? sender, SelectionModelSelectionChangedEventArgs e)
    {
        if (!_isSelectionChangeActive)
        {
            UpdateSelectedValueFromItem();
        }
        
        var route = BuildEventRoute(SelectionChangedEvent);
        
        if (route.HasHandlers)
        {
            var ev = new SelectionChangedEventArgs(
                SelectionChangedEvent,
                e.DeselectedItems.ToArray(),
                e.SelectedItems.ToArray());
            RaiseEvent(ev);
        }
    }

    private int GetAbsoluteIndex(int index)
    {
        Debug.Assert(_listCollectionView != null);
        if (_listCollectionView.PageSize > 0)
        {
            return index + _listCollectionView.PageSize * _listCollectionView.PageIndex;
        }

        return index;
    }

    private int GetLogicalIndex(int index)
    {
        Debug.Assert(_listCollectionView != null);
        if (_listCollectionView.PageSize > 0)
        {
            return index % _listCollectionView.PageSize;
        }
        return index;
    }
    
    private void HandleSelectionModelLostSelection(object? sender, EventArgs e)
    {
        if (AlwaysSelected && _listCollectionView?.TotalItemCount > 0)
        {
            SelectedIndex = 0;
        }
    }
    
    private object? FindItemWithValue(object? value)
    {
        if (ItemCount == 0 || value is null)
        {
            return AvaloniaProperty.UnsetValue;
        }

        var items   = _listCollectionView;
        var binding = SelectedValueBinding;

        if (binding is null)
        {
            // No SelectedValueBinding set, SelectedValue is the item itself
            // Still verify the value passed in is in the Items list
            var index = items!.IndexOf(value);

            if (index >= 0)
            {
                return value;
            }
            return AvaloniaProperty.UnsetValue;
        }

        var bindingEvaluator = GetSelectedValueBindingEvaluator(binding);

        // Matching UWP behavior, if duplicates are present, return the first item matching
        // the SelectedValue provided
        foreach (var item in items!)
        {
            var itemValue = bindingEvaluator.Evaluate(item);

            if (Equals(itemValue, value))
            {
                bindingEvaluator.ClearDataContext();
                return item;
            }
        }

        bindingEvaluator.ClearDataContext();

        return AvaloniaProperty.UnsetValue;
    }
    
    private void InitializeSelectionModel(ISelectionModel model)
    {
        if (_updateState is null)
        {
            TryInitializeSelectionSource(model, false);
        }

        model.PropertyChanged  += HandleSelectionModelPropertyChanged;
        model.SelectionChanged += HandleSelectionModelSelectionChanged;
        model.LostSelection    += HandleSelectionModelLostSelection;

        if (model.SingleSelect)
        {
            SelectionMode &= ~SelectionMode.Multiple;
        }
        else
        {
            SelectionMode |= SelectionMode.Multiple;
        }
        
        _oldSelectedIndex       = model.SelectedIndex;
        _oldSelectedItem.Target = model.SelectedItem;

        if (_updateState is null && AlwaysSelected && model.Count == 0)
        {
            model.SelectedIndex = 0;
        }

        if (SelectedIndex != -1)
        {
            RaiseEvent(new SelectionChangedEventArgs(
                SelectionChangedEvent,
                Array.Empty<object>(),
                Selection.SelectedItems.ToArray()));
        }
    }
    
    private void DeinitializeSelectionModel(ISelectionModel? model)
    {
        if (model != null)
        {
            model.PropertyChanged  -= HandleSelectionModelPropertyChanged;
            model.SelectionChanged -= HandleSelectionModelSelectionChanged;
        }
    }
    
    private BindingEvaluator<object?> GetSelectedValueBindingEvaluator(IBinding binding)
    {
        _selectedValueBindingEvaluator ??= new();
        _selectedValueBindingEvaluator.UpdateBinding(binding);
        return _selectedValueBindingEvaluator;
    }

    private void NotifyPropertyChangedForSelection(AvaloniaPropertyChangedEventArgs change)
    {
        if (change.Property == SelectedValueProperty)
        {
            if (_isSelectionChangeActive)
            {
                return;
            }

            if (_updateState is not null)
            {
                _updateState.SelectedValue = change.NewValue;
                return;
            }

            SelectItemWithValue(change.NewValue);
        }
        else if (change.Property == SelectedValueBindingProperty)
        {
            var idx = SelectedIndex;

            // If no selection is active, don't do anything as SelectedValue is already null
            if (idx == -1)
            {
                return;
            }

            var value = change.GetNewValue<IBinding?>();
            if (value is null)
            {
                // Clearing SelectedValueBinding makes the SelectedValue the item itself
                SetCurrentValue(SelectedValueProperty, SelectedItem);
                return;
            }

            var selectedItem = SelectedItem;

            try
            {
                _isSelectionChangeActive = true;

                var bindingEvaluator = GetSelectedValueBindingEvaluator(value);

                // Re-evaluate SelectedValue with the new binding
                SetCurrentValue(SelectedValueProperty, bindingEvaluator.Evaluate(selectedItem));
            }
            finally
            {
                _isSelectionChangeActive = false;
            }
        }
        else if (change.Property == SelectedItemProperty)
        {
            SyncSelectedItemToListView();
        }
        else if (change.Property == SelectedIndexProperty)
        {
            SyncSelectedIndexToListView();
        }
    }

    private void SyncSelectedIndexToListView()
    {
        if (_skipSyncSelection)
        {
            _skipSyncSelection = false;
            return;
        }
        
        if (ListView != null && _listCollectionView != null)
        {
            if (PageSize > 0)
            {
                var index = SelectedIndex % PageSize;
                ListView.SelectedIndex = index;
            }
            else
            {
                ListView.SelectedIndex = SelectedIndex;
            }
        }
    }

    private void SyncSelectedItemToListView()
    {
        if (_skipSyncSelection)
        {
            _skipSyncSelection = false;
            return;
        }

        if (ListView != null && _listCollectionView != null)
        {
            if (SelectedItem == null)
            {
                ListView.SelectedItem = null;
            }
            else
            {
                if (PageSize > 0)
                {
                    var pageIndex = GetItemPageIndex(SelectedItem);
                    if (_listCollectionView.PageIndex == pageIndex)
                    {
                        ListView.SelectedItem = SelectedItem;
                    }
                    else
                    {
                        ListView.SelectedItem = null;
                    }
                }
                else
                {
                    ListView.SelectedItem = SelectedItem;
                }
            }
        }
    }

    private void SyncSelectionToListView()
    {
        if (ListView != null && _listCollectionView != null)
        {
            if (SelectedItems?.Count > 0)
            {
                if (PageSize <= 0)
                {
                    ListView.SelectedItems = SelectedItems;
                }
                else
                {
                    var effectiveSelectedItems = new List<object>();
                    foreach (var item in SelectedItems)
                    {
                        var pageIndex = GetItemPageIndex(item);
                        if (pageIndex == _listCollectionView.PageIndex)
                        {
                            effectiveSelectedItems.Add(item);
                        }
                    }
                    ListView.SelectedItems = effectiveSelectedItems.Count > 0 ? effectiveSelectedItems : null;
                    Dispatcher.UIThread.Post(() =>
                    {
                        if (Selection.SelectedIndexes.Count > 0)
                        {
                            var currentPageStart = _listCollectionView.PageIndex * _listCollectionView.PageSize;
                            var currentPageEnd   = currentPageStart + _listCollectionView.PageSize - 1;
                            var selectedStart    = Selection.SelectedIndexes.First();
                            var selectedEnd      = Selection.SelectedIndexes.Last();
                            if (selectedEnd <= currentPageStart)
                            {
                                ListView.Selection.AnchorIndex = 0;
                            }
                            else if (selectedStart >= currentPageEnd)
                            {
                                ListView.Selection.AnchorIndex = _listCollectionView.PageSize - 1;
                            }
                            else if (selectedEnd > currentPageStart && selectedEnd < currentPageEnd)
                            {
                                ListView.Selection.AnchorIndex = GetLogicalIndex(selectedEnd);
                            }
                            else if (selectedStart > currentPageStart && selectedStart < currentPageEnd)
                            {
                                ListView.Selection.AnchorIndex = GetLogicalIndex(selectedStart);
                            }
                        }
                    });
                }
                
            }
        }
    }
    
    private int GetItemPageIndex(object item)
    {
        return _listCollectionView?.PageIndexOf(item) ?? -1;
    }
    
    internal void MarkContainerSelected(Control container, bool selected)
    {
        ListView?.MarkContainerSelected(container, selected);
    }
    
    private void HandleSyncSelectionRequest(object? sender, ListSyncSelectionEventArgs args)
    {
        if (ListView == null || _listCollectionView == null)
        {
            return;
        }
        
        _skipSyncSelection = true;
        if (args.RemovedItems.Count > 0)
        {
            Selection.BeginBatchUpdate();
            foreach (var item in args.RemovedItems)
            {
                var index = GetAbsoluteIndex(ListView.Items.IndexOf(item));
                Selection.Deselect(index);
            }
            Selection.EndBatchUpdate();
        }
        if (args.AddedItems.Count > 0)
        {
            Selection.BeginBatchUpdate();
            var isMeta = _rawKeyEventArgs?.Modifiers.HasFlag(RawInputModifiers.Meta) ?? false;
            var isCtrl = _rawKeyEventArgs?.Modifiers.HasFlag(RawInputModifiers.Control) ?? false;
            var isShift = _rawKeyEventArgs?.Modifiers.HasFlag(RawInputModifiers.Shift) ?? false;
            
            if (args.AddedItems.Count == 1 && (_rawKeyEventArgs == null || (!isMeta && !isCtrl && !isShift)))
            {
                Selection.Clear();
            }

            if (!isShift)
            {
                foreach (var item in args.AddedItems)
                {
                    var index = GetAbsoluteIndex(ListView.Items.IndexOf(item));
                    Selection.Select(index);
                }
            }
            else
            {
                // 计算新的范围
                var start      = args.AddedItems[0];
                var last        = args.AddedItems[^1];
                var startIndex = GetAbsoluteIndex(ListView.Items.IndexOf(start));
                var endIndex = GetAbsoluteIndex(ListView.Items.IndexOf(last));
                if (Selection.SelectedIndexes.Count > 0)
                {
                    startIndex = Math.Min(startIndex, Selection.SelectedIndexes.First());
                    endIndex = Math.Max(endIndex, Selection.SelectedIndexes.Last());
                }

                var currentStartIndex = _listCollectionView.PageIndex * _listCollectionView.PageSize;
                var currentEndIndex   = currentStartIndex + _listCollectionView.PageSize - 1;
                
                if (startIndex < currentStartIndex)
                {
                    ListView.Selection.SelectRange(0, ListView.Selection.AnchorIndex);
                }
                else if (endIndex > currentEndIndex)
                {
                    ListView.Selection.SelectRange(ListView.Selection.AnchorIndex, _listCollectionView.PageSize - 1);
                }
                Selection.SelectRange(startIndex, endIndex);
            }
            Selection.EndBatchUpdate();
        }
        else if (args.AddedItems.Count == 0 && args.RemovedItems.Count > 0)
        {
            // 删除其他页的选中, 此时应该只有本页的有一条选中
            var selectedIndex = GetAbsoluteIndex(ListView.Selection.SelectedIndex);
            Selection.BeginBatchUpdate();
            foreach (var index in Selection.SelectedIndexes)
            {
                if (index != selectedIndex)
                {
                    Selection.Deselect(index);
                }
            }
            Selection.EndBatchUpdate();
        }
        Console.WriteLine(Selection.Count);
    }

    // When in a BeginInit..EndInit block, or when the DataContext is updating, we need to
    // defer changes to the selection model because we have no idea in which order properties
    // will be set. Consider:
    //
    // - Both Items and SelectedItem are bound
    // - The DataContext changes
    // - The binding for SelectedItem updates first, producing an item
    // - Items is searched to find the index of the new selected item
    // - However Items isn't yet updated; the item is not found
    // - SelectedIndex is incorrectly set to -1
    //
    // This logic cannot be encapsulated in SelectionModel because the selection model can also
    // be bound, consider:
    //
    // - Both Items and Selection are bound
    // - The DataContext changes
    // - The binding for Items updates first
    // - The new items are assigned to Selection.Source
    // - The binding for Selection updates, producing a new SelectionModel
    // - Both the old and new SelectionModels have the incorrect Source
    private class UpdateState
    {
        public int UpdateCount { get; set; }
        public Optional<ISelectionModel> Selection { get; set; }
        public Optional<IList?> SelectedItems { get; set; }
        public Optional<int> SelectedIndex { get; set; }
        public Optional<object?> SelectedItem { get; set; }
        public Optional<object?> SelectedValue { get; set; }
    }
}