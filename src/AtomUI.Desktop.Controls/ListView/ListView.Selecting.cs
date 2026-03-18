using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using AtomUI.Controls;
using AtomUI.Controls.Data;
using AtomUI.Controls.Utils;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Selection;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Metadata;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

public partial class ListView
{
    #region 公共属性定义

    public static readonly StyledProperty<bool> AutoScrollToSelectedItemProperty =
        AvaloniaProperty.Register<ListView, bool>(
            nameof(AutoScrollToSelectedItem),
            defaultValue: true);
    
    public static readonly DirectProperty<ListView, int> SelectedIndexProperty =
        AvaloniaProperty.RegisterDirect<ListView, int>(
            nameof(SelectedIndex),
            o => o.SelectedIndex,
            (o, v) => o.SelectedIndex = v,
            unsetValue: -1,
            defaultBindingMode: BindingMode.TwoWay);
    
    public static readonly DirectProperty<ListView, object?> SelectedItemProperty =
        AvaloniaProperty.RegisterDirect<ListView, object?>(
            nameof(SelectedItem),
            o => o.SelectedItem,
            (o, v) => o.SelectedItem = v,
            defaultBindingMode: BindingMode.TwoWay, enableDataValidation: true);
    
    public static readonly StyledProperty<object?> SelectedValueProperty =
        AvaloniaProperty.Register<ListView, object?>(nameof(SelectedValue),
            defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<IBinding?> SelectedValueBindingProperty =
        AvaloniaProperty.Register<ListView, IBinding?>(nameof(SelectedValueBinding));
    
    public static readonly DirectProperty<ListView, IList?> SelectedItemsProperty =
        AvaloniaProperty.RegisterDirect<ListView, IList?>(
            nameof(SelectedItems),
            o => o.SelectedItems,
            (o, v) => o.SelectedItems = v);
    
    public static readonly DirectProperty<ListView, ISelectionModel> SelectionProperty =
        AvaloniaProperty.RegisterDirect<ListView, ISelectionModel>(
            nameof(Selection),
            o => o.Selection,
            (o, v) => o.Selection = v);
    
    public static readonly StyledProperty<SelectionMode> SelectionModeProperty =
        AvaloniaProperty.Register<ListView, SelectionMode>(
            nameof(SelectionMode));
    
    public static readonly StyledProperty<bool> IsSelectedProperty =
        AvaloniaProperty.RegisterAttached<ListView, Control, bool>(
            "IsSelected",
            defaultBindingMode: BindingMode.TwoWay);
    
    public static readonly StyledProperty<bool> IsTextSearchEnabledProperty =
        AvaloniaProperty.Register<ListView, bool>(nameof(IsTextSearchEnabled), false);
    
    public static readonly StyledProperty<bool> WrapSelectionProperty =
        AvaloniaProperty.Register<ListView, bool>(nameof(WrapSelection), defaultValue: false);
    
    public bool AutoScrollToSelectedItem
    {
        get => GetValue(AutoScrollToSelectedItemProperty);
        set => SetValue(AutoScrollToSelectedItemProperty, value);
    }
    
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
    
    [AssignBinding]
    [InheritDataTypeFromItems(nameof(ItemsSource))]
    public IBinding? SelectedValueBinding
    {
        get => GetValue(SelectedValueBindingProperty);
        set => SetValue(SelectedValueBindingProperty, value);
    }
    
    public object? SelectedValue
    {
        get => GetValue(SelectedValueProperty);
        set => SetValue(SelectedValueProperty, value);
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

    [AllowNull]
    public ISelectionModel Selection
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
                if (value.Source != null && ItemsSource is IListCollectionView collectionView && value.Source != collectionView.SourceCollection)
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
    
    protected bool AlwaysSelected => SelectionMode.HasAllFlags(SelectionMode.AlwaysSelected);
    
    #endregion

    #region 公共事件定义

    public static readonly RoutedEvent<RoutedEventArgs> IsSelectedChangedEvent =
        RoutedEvent.Register<ListView, RoutedEventArgs>(
            "IsSelectedChanged",
            RoutingStrategies.Bubble);
    
    public static readonly RoutedEvent<SelectionChangedEventArgs> SelectionChangedEvent =
        RoutedEvent.Register<ListView, SelectionChangedEventArgs>(
            nameof(SelectionChanged),
            RoutingStrategies.Bubble);
    
    public event EventHandler<SelectionChangedEventArgs>? SelectionChanged
    {
        add => AddHandler(SelectionChangedEvent, value);
        remove => RemoveHandler(SelectionChangedEvent, value);
    }

    #endregion
   
    private string _textSearchTerm = string.Empty;
    private DispatcherTimer? _textSearchTimer;
    private ISelectionModel? _selection;
    private int _oldSelectedIndex;
    private WeakReference _oldSelectedItem = new(null);
    private WeakReference<IList?> _oldSelectedItems = new(null);
    private bool _ignoreContainerSelectionChanged;
    private UpdateState? _updateState;
    private bool _hasScrolledToSelectedItem;
    private BindingEvaluator<object?>? _selectedValueBindingEvaluator;
    private bool _isSelectionChangeActive;
    
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

    public static bool GetIsSelected(Control control) => control.GetValue(IsSelectedProperty);
    
    public static void SetIsSelected(Control control, bool value) => control.SetValue(IsSelectedProperty, value);

    protected Control? GetContainerFromEventSource(object? eventSource)
    {
        for (var current = eventSource as Visual; current != null; current = current.GetVisualParent())
        {
            if (current is Control control && control.Parent == this &&
                GlobalIndexFromContainer(control) != -1)
            {
                return control;
            }
        }

        return null;
    }
    
    private void HandleItemsViewCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        //Do not change SelectedIndex during initialization
        if (_updateState is not null)
        {
            return;
        }
    
        if (AlwaysSelected && SelectedIndex == -1 && TotalItemCount > 0)
        {
            SelectedIndex = 0;
        }
    }
    
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        AutoScrollToSelectedItemIfNecessary(GetAnchorIndex());
    }

    private void NotifyApplyTemplateForSelecting()
    {
        void ExecuteScrollWhenLayoutUpdated(object? sender, EventArgs e)
        {
            LayoutUpdated -= ExecuteScrollWhenLayoutUpdated;

            AutoScrollToSelectedItemIfNecessary(GetAnchorIndex());
        }

        if (AutoScrollToSelectedItem)
        {
            LayoutUpdated += ExecuteScrollWhenLayoutUpdated;
        }
    }
    
    internal int GetAnchorIndex()
    {
        var selection = _updateState is not null ? TryGetExistingSelection() : Selection;
        return selection?.AnchorIndex ?? -1;
    }
    
    private ISelectionModel? TryGetExistingSelection()
        => _updateState?.Selection.HasValue == true ? _updateState.Selection.Value : _selection;
    
    protected override void ContainerForItemPreparedOverride(Control container, object? item, int index)
    {
        base.ContainerForItemPreparedOverride(container, item, index);

        // Once the container has been full prepared and added to the tree, any bindings from
        // styles or item container themes are guaranteed to be applied. 
        if (!container.IsSet(IsSelectedProperty))
        {
            // The IsSelected property is not set on the container: update the container
            // selection based on the current selection as understood by this control.
            MarkContainerSelected(container, Selection.IsSelected(GlobalIndex(index)));
        }
        else
        {
            // The IsSelected property is set on the container: there is a style or item
            // container theme which has bound the IsSelected property. Update our selection
            // based on the selection state of the container.
            var containerIsSelected = GetIsSelected(container);
            UpdateSelection(GlobalIndex(index), containerIsSelected, toggleModifier: true);
        }

        if (Selection.AnchorIndex == index)
        {
            KeyboardNavigation.SetTabOnceActiveElement(this, container);
        }
    }
    
    protected override void ContainerIndexChangedOverride(Control container, int oldIndex, int newIndex)
    {
        base.ContainerIndexChangedOverride(container, oldIndex, newIndex);
        
        MarkContainerSelected(container, Selection.IsSelected(GlobalIndex(newIndex)));
    }
    
    protected override void OnDataContextBeginUpdate()
    {
        base.OnDataContextBeginUpdate();
        BeginUpdating();
    }
    
    protected override void OnDataContextEndUpdate()
    {
        base.OnDataContextEndUpdate();
        EndUpdating();
    }
    
    protected override void UpdateDataValidation(
        AvaloniaProperty property,
        BindingValueType state,
        Exception? error)
    {
        if (property == SelectedItemProperty)
        {
            DataValidationErrors.SetError(this, error);
        }
    }
    
    protected override void OnTextInput(TextInputEventArgs e)
    {
        if (!e.Handled)
        {
            if (!IsTextSearchEnabled)
            {
                return;
            }

            StopTextSearchTimer();

            _textSearchTerm += e.Text;

            var newIndex = GetIndexFromTextSearch(_textSearchTerm);
            if (newIndex >= 0)
            {
                SelectedIndex = newIndex;
            }
                
            StartTextSearchTimer();

            e.Handled = true;
        }

        base.OnTextInput(e);
    }

    protected void HandlePropertyChangedForSelecting(AvaloniaPropertyChangedEventArgs change)
    {
        if (change.Property == AutoScrollToSelectedItemProperty)
        {
            AutoScrollToSelectedItemIfNecessary(GetAnchorIndex());
        }
        else if (change.Property == SelectionModeProperty && _selection != null)
        {
            var newValue = change.GetNewValue<SelectionMode>();
            _selection.SingleSelect = !newValue.HasAllFlags(SelectionMode.Multiple);
        }
        else if (change.Property == WrapSelectionProperty)
        {
            this.SetWrapFocus(WrapSelection);
        }
        else if (change.Property == SelectedValueProperty)
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
    }
     
    protected bool MoveSelection(
        NavigationDirection direction,
        bool wrap = false,
        bool rangeModifier = false)
    {
        var focused =  FocusUtils.GetFocusManager(this)?.GetFocusedElement();
        var from    = GetContainerFromEventSource(focused) ?? ContainerFromIndex(Selection.AnchorIndex);
        return MoveSelection(from, direction, wrap, rangeModifier);
    }
        
    protected bool MoveSelection(
        Control? from,
        NavigationDirection direction,
        bool wrap = false,
        bool rangeModifier = false)
    {
        if (Presenter?.Panel is not INavigableContainer container)
        {
            return false;
        }

        if (from is null)
        {
            direction = direction switch
            {
                NavigationDirection.Down => NavigationDirection.First,
                NavigationDirection.Up => NavigationDirection.Last,
                NavigationDirection.Right => NavigationDirection.First,
                NavigationDirection.Left => NavigationDirection.Last,
                _ => direction,
            };
        }

        if (GetNextControl(container, direction, from, wrap) is Control next)
        {
            var index = GlobalIndexFromContainer(next);

            if (index != -1)
            {
                UpdateSelection(index, true, rangeModifier);
                next.Focus();
                return true;
            }
        }

        return false;
    }
        
    protected void UpdateSelection(
        int index,
        bool select = true,
        bool rangeModifier = false,
        bool toggleModifier = false,
        bool rightButton = false,
        bool fromFocus = false)
    {
        if (index < 0 || index >= TotalItemCount)
        {
            return;
        }

        var mode   = SelectionMode;
        var multi  = mode.HasAllFlags(SelectionMode.Multiple);
        var toggle = toggleModifier || mode.HasAllFlags(SelectionMode.Toggle);
        var range  = multi && rangeModifier;

        if (!select)
        {
            Selection.Deselect(index);
        }
        else if (rightButton)
        {
            if (Selection.IsSelected(index) == false)
            {
                SelectedIndex = index;
            }
        }
        else if (range)
        {
            using var operation = Selection.BatchUpdate();
            if (!toggleModifier)
            {
                Selection.Clear();
            }
            Selection.SelectRange(Selection.AnchorIndex, index);
        }
        else if (!fromFocus && toggle)
        {
            if (multi)
            {
                if (Selection.IsSelected(index))
                {
                    Selection.Deselect(index);
                }
                else
                {
                    Selection.Select(index);
                }
            }
            else
            {
                SelectedIndex = (SelectedIndex == index) ? -1 : index;
            }
        }
        else if (!toggle)
        {
            using var operation = Selection.BatchUpdate();
            Selection.Clear();
            Selection.Select(index);
        }
    }
    
    protected void UpdateSelection(
        Control container,
        bool select = true,
        bool rangeModifier = false,
        bool toggleModifier = false,
        bool rightButton = false,
        bool fromFocus = false)
    {
        var index = GlobalIndexFromContainer(container);

        if (index != -1)
        {
            UpdateSelection(index, select, rangeModifier, toggleModifier, rightButton, fromFocus);
        }
    }

    protected int GlobalIndexFromContainer(Control container)
    {
        var index = IndexFromContainer(container);
        return GlobalIndex(index);
    }

    protected int GlobalIndex(int index)
    {
        if (PageSize <= 0)
        {
            return index;
        }
        return PageIndex * PageSize + index;
    }

    protected int GlobalIndexLocalIndex(int globalIndex)
    {
        if (PageSize <= 0)
        {
            return globalIndex;
        }

        return globalIndex % PageSize;
    }

    protected bool UpdateSelectionFromEventSource(
        object? eventSource,
        bool select = true,
        bool rangeModifier = false,
        bool toggleModifier = false,
        bool rightButton = false,
        bool fromFocus = false)
    {
        var container = GetContainerFromEventSource(eventSource);

        if (container is ListViewItem listViewItem && (!IsGroupEnabled || !listViewItem.IsGroupItem))
        {
            UpdateSelection(container, select, rangeModifier, toggleModifier, rightButton, fromFocus);
            return true;
        }

        return false;
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

    private void OnItemsViewSourceChanged(object? sender, EventArgs e)
    {
        if (_updateState is null)
        {
            TryInitializeSelectionSource(_selection, true);
        }
    }
        
    private void OnSelectionModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ISelectionModel.AnchorIndex))
        {
            _hasScrolledToSelectedItem = false;

            var anchorIndex = GetAnchorIndex();
            KeyboardNavigation.SetTabOnceActiveElement(this, ContainerFromIndex(anchorIndex));
            AutoScrollToSelectedItemIfNecessary(anchorIndex);
        }
        else if (e.PropertyName == nameof(ISelectionModel.SelectedIndex))
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
        else if (e.PropertyName == nameof(ListViewSelectionModel.WritableSelectedItems))
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
         
    private void OnSelectionModelSelectionChanged(object? sender, SelectionModelSelectionChangedEventArgs e)
    {
        void Mark(int index, bool selected)
        {
            var container = ContainerFromIndex(GlobalIndexLocalIndex(index));

            if (container != null)
            {
                MarkContainerSelected(container, selected);
            }
        }

        if (PageSize <= 0)
        {
            foreach (var i in e.SelectedIndexes)
            {
                Mark(i, true);
            }

            foreach (var i in e.DeselectedIndexes)
            {
                Mark(i, false);
            }

            if (!_isSelectionChangeActive)
            {
                UpdateSelectedValueFromItem();
            }
        }
        else
        {
            for (var i = 0; i < ItemCount; i++)
            {
                var globalIndex = GlobalIndex(i);
                Mark(i, Selection.IsSelected(globalIndex));
            }
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
        
    private void OnSelectionModelLostSelection(object? sender, EventArgs e)
    {
        if (AlwaysSelected && ItemsView.Count > 0)
        {
            SelectedIndex = 0;
        }
    }
        
    private void SelectItemWithValue(object? value)
    {
        if (TotalItemCount == 0 || _isSelectionChangeActive)
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
        
    private object? FindItemWithValue(object? value)
    {
        if (TotalItemCount == 0 || value is null)
        {
            return AvaloniaProperty.UnsetValue;
        }

        var items   = ItemsView;
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
            else
            {
                return AvaloniaProperty.UnsetValue;
            }
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
        
    private void AutoScrollToSelectedItemIfNecessary(int anchorIndex)
    {
        if (AutoScrollToSelectedItem &&
            !_hasScrolledToSelectedItem &&
            Presenter != null &&
            anchorIndex >= 0 &&
            this.IsAttachedToVisualTree())
        {
            Dispatcher.UIThread.Post(state =>
            {
                ScrollIntoView((int)state!);
                _hasScrolledToSelectedItem = true;
            }, anchorIndex);
        }
    }
        
    private void ContainerSelectionChanged(RoutedEventArgs e)
    {
        if (!_ignoreContainerSelectionChanged &&
            e.Source is Control control &&
            control.Parent == this &&
            GlobalIndexFromContainer(control) is var index &&
            index >= 0)
        {
            if (GetIsSelected(control))
            {
                Selection.Select(index);
            }
            else
            {
                Selection.Deselect(index);
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

        try
        {
            container.SetCurrentValue(IsSelectedProperty, selected);
        }
        finally
        {
            _ignoreContainerSelectionChanged = false;
        }
    }
        
    private void UpdateContainerSelection()
    {
        if (Presenter?.Panel is { } panel)
        {
            foreach (var container in panel.Children)
            {
                MarkContainerSelected(
                    container,
                    Selection.IsSelected(GlobalIndexFromContainer(container)));
            }
        }
    }

    private ISelectionModel CreateDefaultSelectionModel()
    {
        return new ListViewSelectionModel
        {
            SingleSelect = !SelectionMode.HasAllFlags(SelectionMode.Multiple),
        };
    }

    private void InitializeSelectionModel(ISelectionModel model)
    {
        if (_updateState is null)
        {
            TryInitializeSelectionSource(model, false);
        }

        model.PropertyChanged  += OnSelectionModelPropertyChanged;
        model.SelectionChanged += OnSelectionModelSelectionChanged;
        model.LostSelection    += OnSelectionModelLostSelection;

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

        UpdateContainerSelection();

        if (SelectedIndex != -1)
        {
            RaiseEvent(new SelectionChangedEventArgs(
                SelectionChangedEvent,
                Array.Empty<object>(),
                Selection.SelectedItems.ToArray()));
        }
    }

    private void TryInitializeSelectionSource(ISelectionModel? selection, bool shouldSelectItemFromSelectedValue)
    {
        if (selection is not null && ItemsSource is IListCollectionView listCollectionView)
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
                {
                    selection.SelectedItem = item;
                }
            }
            if (IsGroupEnabled)
            {
                selection.Source = listCollectionView;
            }
            else
            {
                selection.Source = listCollectionView.SourceCollection;
            }
        }
    }

    private void DeinitializeSelectionModel(ISelectionModel? model)
    {
        if (model != null)
        {
            model.PropertyChanged  -= OnSelectionModelPropertyChanged;
            model.SelectionChanged -= OnSelectionModelSelectionChanged;
        }
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
                s.Update(ItemsView.TryGetInitializedSource(), state.SelectedItems);
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
                    SelectedIndex = selectedIndex;
                else
                    SelectedItem = state.SelectedItem.Value;
            }
            else if (state.SelectedItem.HasValue)
            {
                SelectedItem = state.SelectedItem.Value;
            }

            if (AlwaysSelected && SelectedIndex == -1 && TotalItemCount > 0)
            {
                SelectedIndex = 0;
            }
        }
    }

    private void StartTextSearchTimer()
    {
        _textSearchTimer      =  new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _textSearchTimer.Tick += TextSearchTimer_Tick;
        _textSearchTimer.Start();
    }

    private void StopTextSearchTimer()
    {
        if (_textSearchTimer == null)
        {
            return;
        }

        _textSearchTimer.Tick -= TextSearchTimer_Tick;
        _textSearchTimer.Stop();

        _textSearchTimer = null;
    }

    private void TextSearchTimer_Tick(object? sender, EventArgs e)
    {
        _textSearchTerm = string.Empty;
        StopTextSearchTimer();
    }

    private int GetIndexFromTextSearch(string textSearchTerm)
    {
        if (string.IsNullOrEmpty(textSearchTerm))
        {
            return -1;
        }

        var count = Items.Count;
        if (count == 0)
        {
            return -1;
        }

        var       textBinding          = TextSearch.GetTextBinding(this) ?? DisplayMemberBinding;
        using var textBindingEvaluator = BindingEvaluator<string?>.TryCreate(textBinding);

        for (var i = 0; i < count; i++)
        {
            var text = TextSearchUtils.GetEffectiveText(Items[i], textBindingEvaluator);
            if (text.StartsWith(textSearchTerm, StringComparison.OrdinalIgnoreCase))
            {
                return i;
            }
        }

        return -1;
    }

    private BindingEvaluator<object?> GetSelectedValueBindingEvaluator(IBinding binding)
    {
        _selectedValueBindingEvaluator ??= new();
        _selectedValueBindingEvaluator.UpdateBinding(binding);
        return _selectedValueBindingEvaluator;
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