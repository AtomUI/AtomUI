using AtomUI.Desktop.Controls.Primitives;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;

namespace AtomUI.Desktop.Controls;

internal class SelectCandidateList : List, ICandidateList
{
    #region 公共属性定义

    public static readonly StyledProperty<bool> IsCandidateItemNavigationEnabledProperty =
        AvaloniaProperty.Register<SelectCandidateList, bool>(nameof(IsCandidateItemNavigationEnabled), true);
    
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
    
    public bool IsCandidateItemNavigationEnabled
    {
        get => GetValue(IsCandidateItemNavigationEnabledProperty);
        set => SetValue(IsCandidateItemNavigationEnabledProperty, value);
    }
    
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
    #endregion
    
    #region 公共属性定义

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

    static SelectCandidateList()
    {
        SelectedItemProperty.Changed.AddClassHandler<SelectCandidateList>((list, args) => list.HandleSelectItemChanged(args));
        IsCandidateItemNavigationEnabledProperty.Changed.AddClassHandler<SelectCandidateList>((list, args) => list.HandleIsCandidateItemNavigationEnabled(args));
        CandidateSelectedIndexProperty.Changed.AddClassHandler<SelectCandidateList>((list, args) => list.HandleCandidateSelectedIndexChanged(args));
        CandidateSelectedItemProperty.Changed.AddClassHandler<SelectCandidateList>((list, args) => list.HandleCandidateSelectedItemChanged(args));
    }
    
    internal override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        return new SelectCandidateListItem();
    }

    internal override bool? NeedsContainerOverride(object? item, int index, out object? recycleKey)
    {
        return NeedsContainer<SelectCandidateListItem>(item, out recycleKey);
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
                HandleCommit();
                e.Handled = true;
                break;

            case Key.Up:
                if (IsCandidateItemNavigationEnabled)
                {
                    SelectPreviousCandidateItem();
                }
                else
                {
                    SelectPreviousItem();
                }
                e.Handled = true;
                break;

            case Key.Down:
                if (IsCandidateItemNavigationEnabled)
                {
                    SelectNextCandidateItem();
                }
                else
                {
                    SelectNextItem();
                }
             
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

    private void HandleCommit()
    {
        NotifyCommit();
        ClearState();
    }

    protected virtual void NotifyCommit()
    {
        if (IsCandidateItemNavigationEnabled)
        {
            if (CandidateSelectedItem != null)
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
    
    private void HandleIsCandidateItemNavigationEnabled(AvaloniaPropertyChangedEventArgs e)
    {
        if (ListView != null)
        {
            if (!IsCandidateItemNavigationEnabled)
            {
                for (var i = 0; i < ItemCount; i++)
                {
                    if (ListView.ContainerFromIndex(i) is SelectCandidateListItem childItem)
                    {
                        childItem.SetCurrentValue(SelectCandidateListItem.IsCandidateSelectedProperty, false);
                    }
                }
            }
        }
    }

    private void ClearState()
    {
        SelectedItem           = null;
        SelectedIndex          = -1;
        CandidateSelectedItem  = null;
        CandidateSelectedIndex = -1;
    }
    
    protected virtual void SelectPreviousItem()
    {
        if (SelectedIndex == -1)
        {
            SelectedIndex = ItemCount - 1;
        }
        else
        {
            SelectedIndex = FindNextEnabledIndex(SelectedIndex, -1);
        }
    }
    
    protected virtual void SelectNextItem()
    {
        if (SelectedIndex == -1)
        {
            SelectedIndex = 0;
        }
        else
        {
            SelectedIndex = FindNextEnabledIndex(SelectedIndex, 1);
        }
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
            
            if (ListView.Items[index] is ISelectOption option && option.IsEnabled)
            {
                return index;
            }
        }
    }
    
    protected internal override void NotifyListItemClicked(ListItem item)
    {
        NotifyCommit();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == MaxCountProperty || change.Property == SelectedItemsProperty)
        {
            ConfigureOptionsForMaxCount();
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

        for (var i = 0; i < ItemCount; i++)
        {
            var childItem = ListView.Items[i];
            if (childItem != null)
            {
                if (ListView.ContainerFromItem(childItem) is SelectCandidateListItem childContainer)
                {
                    if (childItem == item)
                    {
                        childContainer.SetCurrentValue(SelectCandidateListItem.IsCandidateSelectedProperty, true);
                        SetCurrentValue(CandidateSelectedItemProperty, childItem);
                        SetCurrentValue(CandidateSelectedIndexProperty, i);
                    }
                    else
                    {
                        childContainer.SetCurrentValue(SelectCandidateListItem.IsCandidateSelectedProperty, false);
                    }
                }
            }
        }
        return true;
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

        var item = ListView.Items[index];
        if (item == null)
        {
            return false;
        }
        return TrySetCandidateItemSelected(item);
    }
}