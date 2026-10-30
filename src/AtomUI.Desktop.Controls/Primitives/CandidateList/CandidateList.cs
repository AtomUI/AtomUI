using AtomUI.Controls;
using AtomUI.Controls.Data;
using AtomUI.Desktop.Controls.Themes;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace AtomUI.Desktop.Controls.Primitives;

public class CandidateList : ListBox, ICandidateList
{
    /// <summary>
    /// 候选项容器的语义标记类，对应 AutoComplete 系列 popup.listItem 语义部件；
    /// 容器运行时创建，标记与 ListBox 的 semantic-item 一样在容器创建时注入。
    /// </summary>
    internal const string PopupListItemClass = "semantic-popup-list-item";

    #region 公共属性定义

    public static readonly StyledProperty<bool> IsCandidateItemNavigationEnabledProperty =
        AvaloniaProperty.Register<CandidateList, bool>(nameof(IsCandidateItemNavigationEnabled), true);

    public static readonly DirectProperty<CandidateList, object?> CandidateSelectedItemProperty =
        AvaloniaProperty.RegisterDirect<CandidateList, object?>(
            nameof(CandidateSelectedItem),
            o => o.CandidateSelectedItem,
            (o, v) => o.CandidateSelectedItem = v,
            defaultBindingMode: BindingMode.TwoWay, enableDataValidation: true);
    
    public static readonly DirectProperty<CandidateList, int> CandidateSelectedIndexProperty =
        AvaloniaProperty.RegisterDirect<CandidateList, int>(
            nameof(CandidateSelectedIndex),
            o => o.CandidateSelectedIndex,
            (o, v) => o.CandidateSelectedIndex = v,
            defaultBindingMode: BindingMode.TwoWay, enableDataValidation: true);
    
    public static readonly StyledProperty<int> MaxCountProperty =
        AvaloniaProperty.Register<CandidateList, int>(nameof(MaxCount), int.MaxValue);
    
    public bool IsCandidateItemNavigationEnabled
    {
        get => GetValue(IsCandidateItemNavigationEnabledProperty);
        set => SetValue(IsCandidateItemNavigationEnabledProperty, value);
    }

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
    
    public int MaxCount
    {
        get => GetValue(MaxCountProperty);
        set => SetValue(MaxCountProperty, value);
    }
    #endregion
    
    #region 公共事件定义
    public static readonly RoutedEvent<RoutedEventArgs> CommitEvent =
        RoutedEvent.Register<CandidateList, RoutedEventArgs>(
            nameof(Commit),
            RoutingStrategies.Bubble);
    
    public static readonly RoutedEvent<RoutedEventArgs> CancelEvent =
        RoutedEvent.Register<CandidateList, RoutedEventArgs>(
            nameof(Cancel),
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
    
    private static readonly FuncTemplate<Panel?> DefaultPanel = new(() => new CandidateVirtualizingStackPanel());

    private ScrollViewer? _scrollViewer;

    static CandidateList()
    {
        SelectedItemProperty.Changed.AddClassHandler<CandidateList>((list, args) => list.HandleSelectItemChanged(args));
        SelectionChangedEvent.AddClassHandler<CandidateList>((list, args) => list.HandleSelectionChanged());
        
        ItemsPanelProperty.OverrideDefaultValue<CandidateList>(DefaultPanel);
    }
    
    private void HandleSelectionChanged()
    {
        if (!IsSingleMode())
        {
            ConfigureOptionsForMaxCount();
        }
    }
    
    private void ConfigureOptionsForMaxCount()
    {
        if (SelectedItems?.Count >= MaxCount)
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
                if (item is IListItemData selectOption)
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
    
    public bool TrySetCandidateItemSelected(object item)
        => TrySetCandidateItemSelectedCore(item);

    private bool TrySetCandidateItemSelectedCore(object item)
    {
        var index = Items.IndexOf(item);
        if (index == -1)
        {
            return false;
        }

        return TrySetCandidateItemSelectedCore(index);
    }
    
    public bool TrySetCandidateItemSelected(int index)
        => TrySetCandidateItemSelectedCore(index);

    private bool TrySetCandidateItemSelectedCore(int index)
    {
        if (index < 0 || index > ItemCount - 1)
        {
            return false;
        }

        var candidateItem = Items[index];
        var candidateContainer = ContainerFromIndex(index) as CandidateListItem;
        if (!IsCandidateAvailable(candidateItem, candidateContainer))
        {
            return false;
        }

        var changed = SetActiveCandidate(index, candidateItem, candidateContainer);
        if (changed && ItemsPanelRoot is CandidateVirtualizingStackPanel virtualizingStackPanel)
        {
            if (virtualizingStackPanel.ScrollCandidateItemIntoView(index) is CandidateListItem realizedContainer)
            {
                candidateContainer = realizedContainer;
            }
        }

        if (candidateContainer is CandidateListItem childContainer)
        {
            SetCandidateItemSelectedIfChanged(childContainer, true);
        }
        return true;
    }

    private void ClearCandidateItemSelection(int index)
    {
        if (index >= 0 &&
            index < ItemCount &&
            ContainerFromIndex(index) is CandidateListItem listItem)
        {
            SetCandidateItemSelectedIfChanged(listItem, false);
        }
    }

    private static void SetCandidateItemSelectedIfChanged(CandidateListItem item, bool isSelected)
    {
        if (item.IsCandidateSelected != isSelected)
        {
            item.SetCurrentValue(CandidateListItem.IsCandidateSelectedProperty, isSelected);
        }
    }

    private bool TrySetCandidateFromContainer(CandidateListItem listItem)
    {
        var index = IndexFromContainer(listItem);
        if (index < 0 || index >= ItemCount || !IsCandidateAvailable(Items[index], listItem))
        {
            return false;
        }

        return SetActiveCandidate(index, Items[index], listItem);
    }

    private bool SetActiveCandidate(int index, object? candidateItem, CandidateListItem? candidateContainer)
    {
        if (CandidateSelectedIndex == index && ReferenceEquals(CandidateSelectedItem, candidateItem))
        {
            if (candidateContainer is not null)
            {
                SetCandidateItemSelectedIfChanged(candidateContainer, true);
            }

            return false;
        }

        ClearCandidateItemSelection(CandidateSelectedIndex);
        SetAndRaise(CandidateSelectedItemProperty, ref _candidateSelectedItem, candidateItem);
        SetAndRaise(CandidateSelectedIndexProperty, ref _candidateSelectedIndex, index);
        if (candidateContainer is not null)
        {
            SetCandidateItemSelectedIfChanged(candidateContainer, true);
        }

        return true;
    }

    private bool IsCandidateAvailable(object? item, CandidateListItem? container)
    {
        if (item is IListItemData itemData && !itemData.IsEnabled)
        {
            return false;
        }

        if (!IsSingleMode() && SelectedItems?.Count >= MaxCount && SelectedItems.Contains(item) == false)
        {
            return false;
        }

        return container is null || container.IsEnabled && container.IsVisible;
    }

    private void ClearActiveCandidateIfUnavailable()
    {
        if (CandidateSelectedIndex < 0 ||
            CandidateSelectedIndex >= ItemCount ||
            !IsCandidateAvailable(Items[CandidateSelectedIndex], ContainerFromIndex(CandidateSelectedIndex) as CandidateListItem))
        {
            ClearActiveCandidate();
        }
    }

    private void ClearActiveCandidate()
    {
        if (CandidateSelectedIndex == -1 && CandidateSelectedItem is null)
        {
            return;
        }

        ClearCandidateItemSelection(CandidateSelectedIndex);
        if (CandidateSelectedItem is not null && ContainerFromItem(CandidateSelectedItem) is CandidateListItem itemContainer)
        {
            SetCandidateItemSelectedIfChanged(itemContainer, false);
        }

        SetAndRaise(CandidateSelectedIndexProperty, ref _candidateSelectedIndex, -1);
        SetAndRaise(CandidateSelectedItemProperty, ref _candidateSelectedItem, null);
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
        ClearActiveCandidate();
    }
    
    private void HandleMultiModeCommit()
    {
        if (CandidateSelectedItem != null)
        {
            if (ContainerFromItem(CandidateSelectedItem) is CandidateListItem listItem)
            {
                var index = IndexFromContainer(listItem);
                if (index != -1)
                {
                    if (listItem.IsSelected)
                    {
                        Selection.Deselect(index);
                    }
                    else
                    {
                        Selection.Select(index);
                    }
                }
            }
        }
    }
    
    protected virtual void SelectPreviousCandidateItem()
    {
        if (ItemCount == 0)
        {
            return;
        }

        if (_candidateSelectedIndex == -1)
        {
            CandidateSelectedIndex = FindNextEnabledIndex(
                SelectedIndex != -1 ? SelectedIndex : 0,
                -1);
        }
        else
        {
            CandidateSelectedIndex = FindNextEnabledIndex(CandidateSelectedIndex, -1);
        }
    }
    
    protected virtual void SelectNextCandidateItem()
    {
        if (ItemCount == 0)
        {
            return;
        }

        if (_candidateSelectedIndex == -1)
        {
            CandidateSelectedIndex = FindNextEnabledIndex(
                SelectedIndex != -1 ? SelectedIndex : -1,
                1);
        }
        else
        {
            CandidateSelectedIndex = FindNextEnabledIndex(CandidateSelectedIndex, 1);
        }
    }

    protected virtual void NotifyCommit()
    {
        if (CandidateSelectedItem != null)
        {
            if (!Equals(SelectedItem, CandidateSelectedItem))
            {
                SetCurrentValue(SelectedItemProperty, CandidateSelectedItem);
            }
        }
        RaiseEvent(new RoutedEventArgs(CommitEvent)
        {
            Source = this,
        });
       
    }

    private void HandleCancel()
    {
        NotifyCancel();
        ClearState();
        ClearActiveCandidate();
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
        if (SelectedItems is not null)
        {
            SelectedItems = null;
        }
        if (SelectedItem is not null)
        {
            SelectedItem = null;
        }
        if (SelectedIndex != -1)
        {
            SelectedIndex = -1;
        }
    }
    
    protected override void NotifyListBoxItemClicked(ListBoxItem item)
    {
        if (IsSingleMode())
        {
            if (item is CandidateListItem candidateListItem)
            {
                TrySetCandidateFromContainer(candidateListItem);
            }
            NotifyCommit();
        }
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        if (e.Pointer.Type == PointerType.Mouse &&
            GetContainerFromEventSource(e.Source) is CandidateListItem candidateListItem)
        {
            TrySetCandidateFromContainer(candidateListItem);
        }
    }
    
    private int FindNextEnabledIndex(int startIndex, int delta)
    {
        var index     = startIndex;
        var findCycle = false;
        while (true)
        {
            index += delta;
            if (index >= ItemCount)
            {
                index = 0;
                if (!findCycle)
                {
                    findCycle = true;
                }
                else
                {
                    return -1;
                }
            }
            else if (index < 0)
            {
                index = ItemCount - 1;
                if (!findCycle)
                {
                    findCycle = true;
                }
                else
                {
                    return -1;
                }
            }

            if (IsCandidateAvailable(Items[index], ContainerFromIndex(index) as CandidateListItem))
            {
                return index;
            }
        }
    }
    
    private bool IsSingleMode()
    {
        return (SelectionMode & SelectionMode.Multiple) != SelectionMode.Multiple;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == MaxCountProperty ||
            change.Property == SelectedItemsProperty)
        {
            ConfigureOptionsForMaxCount();
            ClearActiveCandidateIfUnavailable();
        }

        if (change.Property == SelectedItemsProperty)
        {
            ConfigureEmptyIndicator();
        }
        if (change.Property == ItemsSourceProperty || change.Property == ItemCountProperty ||
            change.Property == FilterValueProperty ||
            change.Property == FilterProperty)
        {
            ClearActiveCandidate();
        }
        
    }
    
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        var listItem = new CandidateListItem();
        listItem.Classes.Add(PopupListItemClass);
        NotifyContainerForItemCreated(listItem, item);
        return listItem;
    }
    
    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
    {
        return NeedsContainer<CandidateListItem>(item, out recycleKey);
    }
    
    protected override void ConfigureEmptyIndicator()
    {
        var isEffectiveEmptyVisible = IsShowEmptyIndicator &&
                                      (ItemCount == 0 || (IsFiltering && FilterResultCount == 0));
        if (IsEffectiveEmptyVisible != isEffectiveEmptyVisible)
        {
            SetCurrentValue(IsEffectiveEmptyVisibleProperty, isEffectiveEmptyVisible);
        }
        SetCurrentValue(IsDefaultEmptyIndicatorVisibleProperty,
            IsEffectiveEmptyVisible && EmptyIndicator is null && EmptyIndicatorTemplate is null);
    }
    
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        ClearActiveCandidate();
    }

    #region 虚拟化上下文管理

    protected override void NotifyRestoreDefaultContext(ListBoxItem item, IListItemData itemData)
    {
        base.NotifyRestoreDefaultContext(item, itemData);
        if (item is CandidateListItem candidateListItem && item is IListItemVirtualizingContextAware virtualListItem)
        {
            if (CandidateSelectedIndex != -1)
            {
                SetCandidateItemSelectedIfChanged(
                    candidateListItem,
                    virtualListItem.VirtualIndex == CandidateSelectedIndex);
            }
            else
            {
                SetCandidateItemSelectedIfChanged(candidateListItem, false);
            }
        }
    }
    
    protected override void NotifyClearContainerForVirtualizingContext(ListBoxItem item)
    {
        base.NotifyClearContainerForVirtualizingContext(item);
        if (item is CandidateListItem listItem)
        {
            if (listItem.IsSet(CandidateListItem.IsCandidateSelectedProperty))
            {
                listItem.ClearValue(CandidateListItem.IsCandidateSelectedProperty);
            }
        }
    }
    #endregion
}
