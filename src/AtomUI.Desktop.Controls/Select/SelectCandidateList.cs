using System.Collections;
using System.Reactive.Disposables;
using AtomUI.Data;
using AtomUI.Desktop.Controls.Primitives;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;

namespace AtomUI.Desktop.Controls;

internal class SelectCandidateList : List, ICandidateList
{
    private static readonly FuncTemplate<Panel?> DefaultPanel = new(() => new CandidateVirtualizingStackPanel());
    
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
        set => SetAndRaise(CandidateSelectedItemProperty, ref _candidateSelectedItem, value);
    }

    private int _candidateSelectedIndex = -1;

    public int CandidateSelectedIndex
    {
        get => _candidateSelectedIndex;
        set => SetAndRaise(CandidateSelectedIndexProperty, ref _candidateSelectedIndex, value);
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
        SelectedItemProperty.Changed.AddClassHandler<SelectCandidateList>((list, args) => list.HandleSelectItemChanged(args));
        CandidateSelectedIndexProperty.Changed.AddClassHandler<SelectCandidateList>((list, args) => list.HandleCandidateSelectedIndexChanged(args));
        CandidateSelectedItemProperty.Changed.AddClassHandler<SelectCandidateList>((list, args) => list.HandleCandidateSelectedItemChanged(args));
        AffectsArrange<SelectCandidateList>(IsHideSelectedOptionsProperty);
        SelectionChangedEvent.AddClassHandler<SelectCandidateList>((list, args) => list.HandleSelectionChanged());
        
        ItemsPanelProperty.OverrideDefaultValue<SelectCandidateList>(DefaultPanel);
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        FilterDescriptions?.CollectionChanged += (sender, args) =>
        {
            CandidateSelectedIndex = -1;
            CandidateSelectedItem  = null;
        };
    }

    private void HandleSelectionChanged()
    {
        if (!IsSingleMode())
        {
            ConfigureOptionsForMaxCount();
        }
    }
    
    internal override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        return new SelectCandidateListItem();
    }

    internal override bool? NeedsContainerOverride(object? item, int index, out object? recycleKey)
    {
        return NeedsContainer<SelectCandidateListItem>(item, out recycleKey);
    }
    
    internal override void PrepareListBoxItem(ListItem listItem, object? item, int index, CompositeDisposable disposables)
    {
        base.PrepareListBoxItem(listItem, item, index, disposables);
        if (listItem is SelectCandidateListItem candidateListItem)
        {
            if (CandidateSelectedIndex != -1)
            {
                candidateListItem.SetCurrentValue(SelectCandidateListItem.IsCandidateSelectedProperty, index == CandidateSelectedIndex);
            }
        }
        disposables.Add(BindUtils.RelayBind(this, IsHideSelectedOptionsProperty, listItem, SelectCandidateListItem.IsHideSelectedOptionsProperty));
    }
    
    private void ResetScrollViewer()
    {
        var sv = this.GetLogicalDescendants().OfType<ScrollViewer>().FirstOrDefault();
        if (sv != null)
        {
            sv.Offset = new Vector(0, 0);
        }
    }

    private void HandleCandidateSelectedIndexChanged(AvaloniaPropertyChangedEventArgs e)
    {
        var oldIndex = e.GetOldValue<int>();
        var newIndex = e.GetNewValue<int>();
        if (newIndex == -1)
        {
            if (ListView?.ContainerFromIndex(oldIndex) is SelectCandidateListItem listItem)
            {
                listItem.SetCurrentValue(SelectCandidateListItem.IsCandidateSelectedProperty, false);
            }
        }
        else
        {
            TrySetCandidateItemSelected(newIndex);
        }
    }
    
    private void HandleCandidateSelectedItemChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (e.NewValue == null)
        {
            if (e.OldValue != null && ListView?.ContainerFromItem(e.OldValue) is SelectCandidateListItem listItem)
            {
                listItem.SetCurrentValue(SelectCandidateListItem.IsCandidateSelectedProperty, false);
            }
        }
        else
        {
            TrySetCandidateItemSelected(e.NewValue);
        }
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
            var container = ListView?.ContainerFromItem(CandidateSelectedItem);
            if (container is ListItem listItem)
            {
                UpdateSelection(listItem, !listItem.IsSelected, false,
                    true);
            }
        }
    }

    protected virtual void NotifyCommit()
    {
        if (CandidateSelectedItem != null)
        {
            SetCurrentValue(SelectedItemProperty, CandidateSelectedItem);
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
        SelectedItems          = null;
        SelectedItem           = null;
        SelectedIndex          = -1;
    }
    
    protected virtual void SelectPreviousCandidateItem()
    {
        if (_candidateSelectedIndex == -1)
        {
            CandidateSelectedIndex = SelectedIndex != -1 ? FindNextEnabledIndex(SelectedIndex, -1) : ItemCount - 1;
        }
        else
        {
            CandidateSelectedIndex = FindNextEnabledIndex(CandidateSelectedIndex, -1);
        }
    }
    
    protected virtual void SelectNextCandidateItem()
    {
        if (_candidateSelectedIndex == -1)
        {
            CandidateSelectedIndex = SelectedIndex != -1 ? FindNextEnabledIndex(SelectedIndex, 1) : 0;
        }
        else
        {
            CandidateSelectedIndex = FindNextEnabledIndex(CandidateSelectedIndex, 1);
        }
    }
    
    private int FindNextEnabledIndex(int startIndex, int delta)
    {
        if (ListView == null)
        {
            return -1;
        }
        
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
            
            var container = ListView.ContainerFromIndex(index);
            if (container == null || container is SelectCandidateListItem listItem && listItem.IsEnabled)
            {
                return index;
            }
        }
    }
    
    protected internal override void NotifyListItemClicked(ListItem item)
    {
        if (IsSingleMode())
        {
            NotifyCommit();
        }
    }

    private bool IsSingleMode()
    {
        return SelectionMode.HasFlag(SelectionMode.Single) && !SelectionMode.HasFlag(SelectionMode.Multiple);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == MaxCountProperty || 
            change.Property == SelectedItemsProperty)
        {
            ConfigureOptionsForMaxCount();
        }

        if (change.Property == SelectedItemsProperty)
        {
            if (IsHideSelectedOptions)
            {
                var oldSelectedCount = 0;
                var newSelectedCount = 0;
                if (change.OldValue is IList oldList)
                {
                    oldSelectedCount = oldList.Count;
                }

                if (change.NewValue is IList newList)
                {
                    newSelectedCount = newList.Count;
                }

                if (newSelectedCount < oldSelectedCount)
                {
                    HasAnyVisibleItem = true;
                }
            }
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
        if (ListView != null)
        {
            if (SelectedItems?.Count >= MaxCount)
            {
                for (var i = 0; i < ItemCount; i++)
                {
                    var item      = ListView.Items[i];
                    var container = ListView.ContainerFromIndex(i);
                    if (!SelectedItems.Contains(item))
                    {
                        container?.SetCurrentValue(IsEnabledProperty, false);
                    }
                    else
                    {
                        container?.SetCurrentValue(IsEnabledProperty, true);
                    }
                }
            }
            else
            {
                for (var i = 0; i < ItemCount; i++)
                {
                    var item      = ListView.Items[i];
                    var container = ListView.ContainerFromIndex(i);
                    if (item is ISelectOption selectOption)
                    {
                        container?.SetCurrentValue(IsEnabledProperty, selectOption.IsEnabled);
                    }
                }
            }
        }
    }

    public bool TrySetCandidateItemSelected(object item)
    {
        if (ListView == null)
        {
            return false;
        }

        if (item is IGroupListItemData groupListItemData && groupListItemData.IsGroupItem)
        {
            return false;
        }
        
        var index = ListView.Items.IndexOf(item);
        if (index == -1)
        {
            return false;
        }

        return TrySetCandidateItemSelected(index);
    }
    
    public bool TrySetCandidateItemSelected(int index)
    {
        if (ListView == null)
        {
            return false;
        }
        if (index < 0 || index > ItemCount - 1)
        {
            return false;
        }
        
        if (ListView.ItemsPanelRoot is CandidateVirtualizingStackPanel virtualizingStackPanel)
        {
            virtualizingStackPanel.ScrollCandidateItemIntoView(index);
        }

        for (var i = 0; i < ListView.ItemCount; i++)
        {
            if (ListView.ContainerFromIndex(i) is SelectCandidateListItem childContainer)
            {
                if (i == index)
                {
                    childContainer.SetCurrentValue(SelectCandidateListItem.IsCandidateSelectedProperty, true);
                    SetCurrentValue(CandidateSelectedItemProperty, ListView.Items[i]);
                    SetCurrentValue(CandidateSelectedIndexProperty, i);
                }
                else
                {
                    childContainer.SetCurrentValue(SelectCandidateListItem.IsCandidateSelectedProperty, false);
                }
            }
        }
        return true;
    }
    
    internal override bool UpdateSelectionFromPointerEvent(ListItem listItem, PointerEventArgs e)
    {
        if (e.Handled)
        {
            return true;
        }

        var select = true;
        if (IsSingleMode())
        {
            if (listItem.IsSelected)
            {
                e.Handled = true;
                return false;
            }
        }
        else
        {
            select = !listItem.IsSelected;
        }
        
        var result = UpdateSelection(listItem, select, false,
            true);
        if (result)
        {
            e.Handled = true;
        }
        return result;
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        if (!IsSingleMode())
        {
            CandidateSelectedIndex = -1;
            CandidateSelectedItem  = null;
        }
    }
    
    protected internal override void ClearContainerForItem(ListItem item)
    {
        if (item is SelectCandidateListItem candidateListItem)
        {
            candidateListItem.ClearValue(SelectCandidateListItem.IsCandidateSelectedProperty);
        }
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
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        if (IsHideSelectedOptions)
        {
            var hasAnyVisibleItem = false;
            var logicalChildren   = ListView?.GetLogicalChildren();
            if (logicalChildren != null)
            {
                foreach (var child in logicalChildren)
                {
                    if (child is Control control && control.IsVisible)
                    {
                        hasAnyVisibleItem = true;
                    }
                }
            }
            HasAnyVisibleItem = hasAnyVisibleItem;
        }
        
        return base.ArrangeOverride(finalSize);
    }
    
}