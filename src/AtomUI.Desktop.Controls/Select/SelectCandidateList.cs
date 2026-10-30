using System.Collections;
using AtomUI.Controls;
using AtomUI.Controls.Data;
using AtomUI.Desktop.Controls.Primitives;
using AtomUI.Desktop.Controls.Themes;
using AtomUI.Generated.AtomUIDesktopControls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace AtomUI.Desktop.Controls;

internal class SelectCandidateList : ListView, ICandidateList
{
    private static readonly FuncTemplate<Panel?> DefaultPanel = new(() => new CandidateVirtualizingStackPanel());

    private ScrollViewer? _scrollViewer;

    private event EventHandler<SelectionChangedEventArgs>? CandidateSelectionChanged;

    event EventHandler<SelectionChangedEventArgs>? ICandidateList.SelectionChanged
    {
        add => CandidateSelectionChanged += value;
        remove => CandidateSelectionChanged -= value;
    }

    object? ICandidateList.SelectedItem
    {
        get => SelectedItem;
        set => SetCandidateSelection(value, null);
    }

    IList? ICandidateList.SelectedItems
    {
        get => SelectedItems.ToList();
        set => SetCandidateSelection(null, value);
    }

    private void SetCandidateSelection(object? item, IList? items)
    {
        using var batch = BeginSelectionBatchUpdate();
        Selection.Clear();
        if (items is not null)
        {
            var remaining = items.Cast<object?>().ToList();
            foreach (var sourceIndex in EnumerateCurrentViewSourceIndexes())
            {
                if (!TryGetSourceItem(sourceIndex, out var sourceItem))
                {
                    continue;
                }

                var matchIndex = remaining.FindIndex(candidate => ReferenceEquals(candidate, sourceItem));
                if (matchIndex >= 0)
                {
                    Selection.Select(sourceIndex);
                    remaining.RemoveAt(matchIndex);
                }
            }
        }
        else if (item is not null)
        {
            foreach (var sourceIndex in EnumerateCurrentViewSourceIndexes())
            {
                if (TryGetSourceItem(sourceIndex, out var sourceItem) && ReferenceEquals(sourceItem, item))
                {
                    Selection.Select(sourceIndex);
                    break;
                }
            }
        }
    }

    #region 公共属性定义

    public static readonly DirectProperty<SelectCandidateList, object?> CandidateSelectedItemProperty =
        AvaloniaProperty.RegisterDirect<SelectCandidateList, object?>(
            nameof(CandidateSelectedItem),
            o => o.CandidateSelectedItem,
            (o, v) => o.CandidateSelectedItem = v,
            defaultBindingMode: BindingMode.TwoWay, enableDataValidation: true);

    public static readonly DirectProperty<SelectCandidateList, int> CandidateSelectedIndexProperty =
        AvaloniaProperty.RegisterDirect<SelectCandidateList, int>(
            nameof(CandidateSelectedIndex),
            o => o.CandidateSelectedIndex,
            (o, v) => o.CandidateSelectedIndex = v,
            defaultBindingMode: BindingMode.TwoWay, enableDataValidation: true);

    public static readonly DirectProperty<SelectCandidateList, bool> IsHideSelectedOptionsProperty =
        AvaloniaProperty.RegisterDirect<SelectCandidateList, bool>(
            nameof(IsHideSelectedOptions),
            o => o.IsHideSelectedOptions,
            (o, v) => o.IsHideSelectedOptions = v);

    private object? _candidateSelectedItem;

    public object? CandidateSelectedItem
    {
        get => _candidateSelectedItem;
        set
        {
            if (value is null)
            {
                ClearActiveCandidate();
            }
            else if (!TrySetCandidateItemSelected(value))
            {
                ClearActiveCandidate();
            }
        }
    }

    private int _candidateSelectedIndex = -1;

    public int CandidateSelectedIndex
    {
        get => _candidateSelectedIndex;
        set
        {
            if (value < 0)
            {
                ClearActiveCandidate();
            }
            else if (!TrySetCandidateItemSelected(value))
            {
                ClearActiveCandidate();
            }
        }
    }

    private bool _isHideSelectedOptions;

    public bool IsHideSelectedOptions
    {
        get => _isHideSelectedOptions;
        set => SetAndRaise(IsHideSelectedOptionsProperty, ref _isHideSelectedOptions, value);
    }

    public static readonly StyledProperty<int> MaxCountProperty =
        Select.MaxCountProperty.AddOwner<SelectCandidateList>();

    public int MaxCount
    {
        get => GetValue(MaxCountProperty);
        set => SetValue(MaxCountProperty, value);
    }

    #endregion

    #region 公共事件定义
    public static readonly RoutedEvent<RoutedEventArgs> CommitEvent =
        RoutedEvent.Register<SelectCandidateList, RoutedEventArgs>(nameof(Commit),
            RoutingStrategies.Bubble);

    public static readonly RoutedEvent<RoutedEventArgs> CancelEvent =
        RoutedEvent.Register<SelectCandidateList, RoutedEventArgs>(nameof(Cancel),
            RoutingStrategies.Bubble);

    public event EventHandler<RoutedEventArgs>? Commit
    {
        add => AddHandler(CommitEvent, value);
        remove => RemoveHandler(CommitEvent, value);
    }

    public event EventHandler<RoutedEventArgs>? Cancel
    {
        add => AddHandler(CancelEvent, value);
        remove => RemoveHandler(CancelEvent, value);
    }
    #endregion

    #region 内部属性定义

    public static readonly DirectProperty<SelectCandidateList, bool> HasAnyVisibleItemProperty =
        AvaloniaProperty.RegisterDirect<SelectCandidateList, bool>(
            nameof(HasAnyVisibleItem),
            o => o.HasAnyVisibleItem,
            (o, v) => o.HasAnyVisibleItem = v);

    private bool _hasAnyVisibleItem = true;

    public bool HasAnyVisibleItem
    {
        get => _hasAnyVisibleItem;
        set => SetAndRaise(HasAnyVisibleItemProperty, ref _hasAnyVisibleItem, value);
    }
    #endregion

    static SelectCandidateList()
    {
        AffectsArrange<SelectCandidateList>(IsHideSelectedOptionsProperty);
        SelectedItemProperty.Changed.AddClassHandler<SelectCandidateList>((list, args) => list.HandleSelectItemChanged(args));
        SelectionChangedEvent.AddClassHandler<SelectCandidateList>((list, args) => list.HandleSelectionChanged(args));
        ItemsPanelProperty.OverrideDefaultValue<SelectCandidateList>(DefaultPanel);
    }

    protected override void NotifyFilterContextChanged()
    {
        ClearActiveCandidate();
    }

    private void HandleSelectionChanged(ListViewSelectionChangedEventArgs args)
    {
        if (!IsSingleMode())
        {
            ConfigureOptionsForMaxCount();
            ConfigureHasAnyVisibleItem();
        }

        CandidateSelectionChanged?.Invoke(
            this,
            new SelectionChangedEventArgs(
                Avalonia.Controls.Primitives.SelectingItemsControl.SelectionChangedEvent,
                args.DeselectedItems.ToArray(),
                args.SelectedItems.ToArray()));
    }

    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        var listItem = new SelectCandidateListItem();
        listItem.Classes.Add(SelectSemanticParts.PopupListItemClass);
        return listItem;
    }

    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
    {
        return NeedsContainer<SelectCandidateListItem>(item, out recycleKey);
    }

    protected override void PrepareListViewItem(ListViewItem listItem, object? item, int index)
    {
        base.PrepareListViewItem(listItem, item, index);
        listItem[!SelectCandidateListItem.IsHideSelectedOptionsProperty] = this[!IsHideSelectedOptionsProperty];
        listItem.SetCurrentValue(
            SelectCandidateListItem.IsCandidateSelectedProperty,
            IsCandidateForViewIndex(index, item));
    }

    private void ResetScrollViewer()
    {
        if (_scrollViewer != null)
        {
            _scrollViewer.Offset = new Vector(0, 0);
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _scrollViewer = e.NameScope.Find<ScrollViewer>(ListViewThemeConstants.ScrollViewerPart);
    }

    public void HandleKeyDown(KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Enter:
                if (IsSingleMode())
                {
                    HandleSingleModeCommit();
                }
                else
                {
                    HandleMultiModeCommit();
                }

                e.Handled = true;
                break;

            case Key.Up:
                SelectPreviousCandidateItem();
                e.Handled = true;
                break;

            case Key.Down:
                SelectNextCandidateItem();

                e.Handled = true;
                break;

            case Key.Escape:
                HandleCancel();
                e.Handled = true;
                break;

            default:
                break;
        }
    }

    private void HandleSingleModeCommit()
    {
        NotifyCommit();
        ClearState();
    }

    private void HandleMultiModeCommit()
    {
        if (CandidateSelectedItem != null)
        {
            ToggleVisibleItemSelection();
        }
    }

    protected virtual void NotifyCommit()
    {
        if (CandidateSelectedIndex >= 0)
        {
            SelectedIndex = CandidateSelectedIndex;
        }

        RaiseEvent(new RoutedEventArgs(CommitEvent)
        {
            Source = this,
        });
    }

    private void HandleCancel()
    {
        NotifyCancel();
        if (IsSingleMode())
        {
            ClearState();
        }
    }

    protected virtual void NotifyCancel()
    {
        RaiseEvent(new RoutedEventArgs(CancelEvent)
        {
            Source = this,
        });
    }

    private void HandleSelectItemChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (e.NewValue == null)
        {
            ResetScrollViewer();
        }
    }

    private void ClearState()
    {
        Selection.Clear();
    }

    protected virtual void SelectPreviousCandidateItem()
    {
        var sourceIndex = FindNextEnabledIndex(
            _candidateSelectedIndex != -1 ? _candidateSelectedIndex : SelectedIndex,
            -1);
        if (sourceIndex == -1)
        {
            ClearActiveCandidate();
            return;
        }

        TrySetCandidateItemSelected(sourceIndex);
    }

    protected virtual void SelectNextCandidateItem()
    {
        var sourceIndex = FindNextEnabledIndex(
            _candidateSelectedIndex != -1 ? _candidateSelectedIndex : SelectedIndex,
            1);
        if (sourceIndex == -1)
        {
            ClearActiveCandidate();
            return;
        }

        TrySetCandidateItemSelected(sourceIndex);
    }

    private int FindNextEnabledIndex(int startSourceIndex, int delta)
    {
        var visibleCandidates = EnumerateCurrentViewSourceIndexes()
                                .Select(sourceIndex =>
                                {
                                    var hasViewIndex = TryGetViewIndexFromSourceIndex(sourceIndex, out var viewIndex);
                                    return (SourceIndex: sourceIndex, ViewIndex: hasViewIndex ? viewIndex : -1);
                                })
                                .Where(candidate => candidate.ViewIndex >= 0)
                                .ToArray();
        if (visibleCandidates.Length == 0)
        {
            return -1;
        }

        var currentCandidateIndex = startSourceIndex >= 0
            ? Array.FindIndex(visibleCandidates, candidate => candidate.SourceIndex == startSourceIndex)
            : -1;
        if (currentCandidateIndex == -1 && delta < 0)
        {
            currentCandidateIndex = visibleCandidates.Length;
        }

        for (var step = 0; step < visibleCandidates.Length; step++)
        {
            var candidateIndex = currentCandidateIndex + delta * (step + 1);
            candidateIndex %= visibleCandidates.Length;
            if (candidateIndex < 0)
            {
                candidateIndex += visibleCandidates.Length;
            }

            var (sourceIndex, viewIndex) = visibleCandidates[candidateIndex];
            if (TryGetSourceItem(sourceIndex, out var candidateItem) &&
                IsCandidateAvailable(sourceIndex, candidateItem, ContainerFromIndex(viewIndex) as SelectCandidateListItem))
            {
                return sourceIndex;
            }
        }

        return -1;
    }

    protected internal override void NotifyItemClicked(ListViewItem item)
    {
        if (IsSingleMode() && !item.IsGroupItem)
        {
            NotifyCommit();
        }
    }

    private bool IsSingleMode()
    {
        return (SelectionMode & SelectionMode.Multiple) != SelectionMode.Multiple;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ItemsSourceProperty)
        {
            ClearActiveCandidate();
        }

        if (change.Property == MaxCountProperty ||
            change.Property == SelectedItemsProperty)
        {
            ConfigureOptionsForMaxCount();
        }

        if (change.Property == SelectedItemsProperty ||
            change.Property == IsHideSelectedOptionsProperty ||
            change.Property == ItemsSourceProperty ||
            change.Property == IsEmptyDataSourceProperty)
        {
            if (IsHideSelectedOptions)
            {
                ConfigureHasAnyVisibleItem();
            }
        }

        if (change.Property == MaxCountProperty ||
            change.Property == SelectedItemsProperty ||
            change.Property == IsHideSelectedOptionsProperty)
        {
            ClearActiveCandidateIfUnavailable();
        }

        if (change.Property == HasAnyVisibleItemProperty ||
            change.Property == IsHideSelectedOptionsProperty ||
            change.Property == SelectedItemsProperty)
        {
            ConfigureEmptyIndicator();
        }
    }

    private void ConfigureOptionsForMaxCount()
    {
        if (SelectedItems.Count >= MaxCount)
        {
            for (var i = 0; i < ItemCount; i++)
            {
                var item      = Items[i];
                var container = ContainerFromIndex(i);
                if (!SelectedItems.Contains(item))
                {
                    SetContainerEnabledIfChanged(container, false);
                }
                else
                {
                    SetContainerEnabledIfChanged(container, true);
                }
            }
        }
        else
        {
            for (var i = 0; i < ItemCount; i++)
            {
                var item      = Items[i];
                var container = ContainerFromIndex(i);
                if (item is ISelectOption selectOption)
                {
                    SetContainerEnabledIfChanged(container, selectOption.IsEnabled);
                }
            }
        }
    }

    private static void SetContainerEnabledIfChanged(Control? container, bool isEnabled)
    {
        if (container is not null && container.IsEnabled != isEnabled)
        {
            container.SetCurrentValue(IsEnabledProperty, isEnabled);
        }
    }

    private void ClearActiveCandidateIfUnavailable()
    {
        if (CandidateSelectedIndex < 0 ||
            !TryGetViewIndexFromSourceIndex(CandidateSelectedIndex, out var viewIndex) ||
            !TryGetSourceItem(CandidateSelectedIndex, out var candidateItem) ||
            !ReferenceEquals(candidateItem, CandidateSelectedItem) ||
            !IsCandidateAvailable(CandidateSelectedIndex, candidateItem, ContainerFromIndex(viewIndex) as SelectCandidateListItem))
        {
            if (CandidateSelectedIndex >= 0 || CandidateSelectedItem is not null)
            {
                ClearActiveCandidate();
            }
        }
    }

    private void ConfigureHasAnyVisibleItem()
    {
        if (!IsHideSelectedOptions)
        {
            return;
        }

        var selectedItems = SelectedItems;
        for (var i = 0; i < ItemCount; i++)
        {
            var item = Items[i];
            if (item is IGroupListItemData groupListItemData && groupListItemData.IsGroupItem)
            {
                continue;
            }

            if (!selectedItems.Contains(item))
            {
                HasAnyVisibleItem = true;
                return;
            }
        }

        HasAnyVisibleItem = false;
    }

    public bool TrySetCandidateItemSelected(object item)
    {
        if (item is IGroupListItemData groupListItemData && groupListItemData.IsGroupItem)
        {
            return false;
        }

        foreach (var sourceIndex in EnumerateCurrentViewSourceIndexes())
        {
            if (TryGetSourceItem(sourceIndex, out var sourceItem) && ReferenceEquals(sourceItem, item))
            {
                return TrySetCandidateItemSelected(sourceIndex);
            }
        }

        return false;
    }

    public bool TrySetCandidateItemSelected(int index)
    {
        if (!TryGetViewIndexFromSourceIndex(index, out var localIndex) ||
            localIndex < 0 || localIndex >= ItemCount ||
            !TryGetSourceItem(index, out var candidateItem))
        {
            return false;
        }

        var candidateContainer = ContainerFromIndex(localIndex) as SelectCandidateListItem;
        if (!IsCandidateAvailable(index, candidateItem, candidateContainer))
        {
            return false;
        }

        var changed = SetActiveCandidate(index, candidateItem!, candidateContainer);
        if (changed && ItemsPanelRoot is CandidateVirtualizingStackPanel virtualizingStackPanel)
        {
            var realizedContainer = virtualizingStackPanel.ScrollCandidateItemIntoView(localIndex);
            if (realizedContainer is SelectCandidateListItem listItem)
            {
                SetCandidateProjectionIfChanged(listItem, true);
            }
        }

        return true;
    }

    private bool TrySetCandidateFromContainer(SelectCandidateListItem listItem, out int sourceIndex)
    {
        sourceIndex = SelectionIndexFromContainer(listItem);
        if (sourceIndex < 0 || !TryGetViewIndexFromSourceIndex(sourceIndex, out _) ||
            !TryGetSourceItem(sourceIndex, out var candidateItem) ||
            !IsCandidateAvailable(sourceIndex, candidateItem, listItem))
        {
            return false;
        }

        SetActiveCandidate(sourceIndex, candidateItem!, listItem);
        return true;
    }

    private bool SetActiveCandidate(int sourceIndex, object candidateItem, SelectCandidateListItem? candidateContainer)
    {
        var isUnchanged = CandidateSelectedIndex == sourceIndex &&
                          ReferenceEquals(CandidateSelectedItem, candidateItem);
        if (isUnchanged)
        {
            if (candidateContainer is not null)
            {
                SetCandidateProjectionIfChanged(candidateContainer, true);
            }
            return false;
        }

        ClearCandidateProjection(CandidateSelectedIndex, CandidateSelectedItem);
        SetAndRaise(CandidateSelectedItemProperty, ref _candidateSelectedItem, candidateItem);
        SetAndRaise(CandidateSelectedIndexProperty, ref _candidateSelectedIndex, sourceIndex);

        if (candidateContainer is not null)
        {
            SetCandidateProjectionIfChanged(candidateContainer, true);
        }

        return true;
    }

    internal void ClearActiveCandidate()
    {
        if (CandidateSelectedIndex == -1 && CandidateSelectedItem is null)
        {
            return;
        }

        ClearCandidateProjection(CandidateSelectedIndex, CandidateSelectedItem);
        SetAndRaise(CandidateSelectedIndexProperty, ref _candidateSelectedIndex, -1);
        SetAndRaise(CandidateSelectedItemProperty, ref _candidateSelectedItem, null);
    }

    private void ClearCandidateProjection(int sourceIndex, object? candidateItem)
    {
        if (sourceIndex >= 0 && TryGetViewIndexFromSourceIndex(sourceIndex, out var viewIndex) &&
            ContainerFromIndex(viewIndex) is SelectCandidateListItem listItem)
        {
            SetCandidateProjectionIfChanged(listItem, false);
            return;
        }

        if (candidateItem is not null && ContainerFromItem(candidateItem) is SelectCandidateListItem itemContainer)
        {
            SetCandidateProjectionIfChanged(itemContainer, false);
        }
    }

    private bool IsCandidateForViewIndex(int viewIndex, object? item)
    {
        return CandidateSelectedIndex >= 0 &&
               TryGetSourceIndexFromViewIndex(viewIndex, out var sourceIndex) &&
               sourceIndex == CandidateSelectedIndex &&
               ReferenceEquals(item, CandidateSelectedItem);
    }

    private bool IsCandidateAvailable(
        int sourceIndex,
        object? candidateItem,
        SelectCandidateListItem? candidateContainer)
    {
        if (candidateItem is IGroupListItemData groupListItemData && groupListItemData.IsGroupItem)
        {
            return false;
        }

        if (candidateItem is not IListItemData itemData || !itemData.IsEnabled)
        {
            return false;
        }

        if (IsHideSelectedOptions && Selection.IsSelected(sourceIndex))
        {
            return false;
        }

        if (!IsSingleMode() && SelectedItems.Count >= MaxCount && !Selection.IsSelected(sourceIndex))
        {
            return false;
        }

        return candidateContainer is null ||
               candidateContainer.IsEnabled && candidateContainer.IsVisible && !candidateContainer.IsGroupItem;
    }

    private static void SetCandidateProjectionIfChanged(SelectCandidateListItem listItem, bool isSelected)
    {
        if (listItem.IsCandidateSelected != isSelected)
        {
            listItem.SetCurrentValue(SelectCandidateListItem.IsCandidateSelectedProperty, isSelected);
        }
    }

    internal void NotifyCandidateContainerAvailabilityChanged(SelectCandidateListItem listItem)
    {
        if (!listItem.IsCandidateSelected)
        {
            return;
        }

        var sourceIndex = SelectionIndexFromContainer(listItem);
        if (sourceIndex < 0 || !TryGetViewIndexFromSourceIndex(sourceIndex, out _) ||
            !TryGetSourceItem(sourceIndex, out var candidateItem) ||
            !IsCandidateAvailable(sourceIndex, candidateItem, listItem))
        {
            ClearActiveCandidate();
        }
    }

    protected internal override bool UpdateSelectionFromPointerEvent(Control source, PointerEventArgs e)
    {
        if (!IsSelectable)
        {
            return false;
        }

        if (GetContainerFromEventSource(source) is SelectCandidateListItem listItem &&
            TrySetCandidateFromContainer(listItem, out var sourceIndex))
        {
            if (IsSingleMode())
            {
                SelectedIndex = sourceIndex;
            }
            else
            {
                ToggleVisibleItemSelection();
            }
        }
        return true;
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        if (e.Pointer.Type == PointerType.Mouse &&
            GetContainerFromEventSource(e.Source) is SelectCandidateListItem listItem)
        {
            TrySetCandidateFromContainer(listItem, out _);
        }
    }

    private void ToggleVisibleItemSelection()
    {
        var sourceIndex = CandidateSelectedIndex;
        if (sourceIndex < 0)
        {
            return;
        }

        if (Selection.IsSelected(sourceIndex))
        {
            Selection.Deselect(sourceIndex);
        }
        else
        {
            Selection.Select(sourceIndex);
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        ClearActiveCandidate();
    }

    protected override void ConfigureEmptyIndicator()
    {
        if (!IsHideSelectedOptions)
        {
            SetCurrentValue(IsEffectiveEmptyVisibleProperty, IsShowEmptyIndicator && IsEmptyDataSource);
        }
        else
        {
            SetCurrentValue(IsEffectiveEmptyVisibleProperty, IsShowEmptyIndicator && (IsEmptyDataSource || !HasAnyVisibleItem));
        }
        SetCurrentValue(IsDefaultEmptyIndicatorVisibleProperty,
            IsEffectiveEmptyVisible && EmptyIndicator is null && EmptyIndicatorTemplate is null);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        if (IsHideSelectedOptions)
        {
            var hasAnyVisibleItem = false;
            foreach (var child in LogicalChildren)
            {
                if (child is Control control && control.IsVisible)
                {
                    hasAnyVisibleItem = true;
                }
            }
            HasAnyVisibleItem = hasAnyVisibleItem;
        }

        return base.ArrangeOverride(finalSize);
    }

    #region 虚拟化上下文管理
    protected override void NotifyClearContainerForVirtualizingContext(ListViewItem item)
    {
        base.NotifyClearContainerForVirtualizingContext(item);
        if (item is SelectCandidateListItem listItem)
        {
            listItem.ClearValue(SelectCandidateListItem.IsCandidateSelectedProperty);
        }
    }

    protected override void NotifyRestoreDefaultContext(ListViewItem item, IListItemData itemData)
    {
        base.NotifyRestoreDefaultContext(item, itemData);
        if (item is SelectCandidateListItem listItem && item is IListItemVirtualizingContextAware virtualListItem)
        {
            SetCandidateProjectionIfChanged(
                listItem,
                CandidateSelectedIndex != -1 &&
                virtualListItem.VirtualIndex == GlobalIndexLocalIndex(CandidateSelectedIndex) &&
                ReferenceEquals(itemData, CandidateSelectedItem));
        }
    }

    protected override void NotifyRestoreVirtualizingContext(ListViewItem item, IDictionary<object, object?> context)
    {
        base.NotifyRestoreVirtualizingContext(item, context);
        if (item is SelectCandidateListItem listItem && item is IListItemVirtualizingContextAware virtualListItem)
        {
            SetCandidateProjectionIfChanged(
                listItem,
                CandidateSelectedIndex != -1 &&
                virtualListItem.VirtualIndex == GlobalIndexLocalIndex(CandidateSelectedIndex) &&
                ReferenceEquals(item.Content, CandidateSelectedItem));
        }
    }
    #endregion
}
