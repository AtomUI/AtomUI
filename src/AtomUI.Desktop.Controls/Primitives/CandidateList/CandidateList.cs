using System.Reactive.Disposables;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;

namespace AtomUI.Desktop.Controls.Primitives;

public class CandidateList : ListBox, ICandidateList
{
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
        set => SetAndRaise(CandidateSelectedItemProperty, ref _candidateSelectedItem, value);
    }

    private int _candidateSelectedIndex = -1;

    public int CandidateSelectedIndex
    {
        get => _candidateSelectedIndex;
        set => SetAndRaise(CandidateSelectedIndexProperty, ref _candidateSelectedIndex, value);
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

    static CandidateList()
    {
        SelectedItemProperty.Changed.AddClassHandler<CandidateList>((list, args) => list.HandleSelectItemChanged(args));
        CandidateSelectedIndexProperty.Changed.AddClassHandler<CandidateList>((list, args) => list.HandleCandidateSelectedIndexChanged(args));
        CandidateSelectedItemProperty.Changed.AddClassHandler<CandidateList>((list, args) => list.HandleCandidateSelectedItemChanged(args));
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
                var item      = Items[i];
                var container = ContainerFromIndex(i);
                if (item is IListBoxItemData selectOption)
                {
                    container?.SetCurrentValue(IsEnabledProperty, selectOption.IsEnabled);
                }
            }
        }
    }
    
    private void HandleCandidateSelectedIndexChanged(AvaloniaPropertyChangedEventArgs e)
    {
        var oldIndex = e.GetOldValue<int>();
        var newIndex = e.GetNewValue<int>();
        if (newIndex == -1)
        {
            if (ContainerFromIndex(oldIndex) is CandidateListItem listItem)
            {
                listItem.SetCurrentValue(CandidateListItem.IsCandidateSelectedProperty, false);
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
            if (e.OldValue != null && ContainerFromItem(e.OldValue) is CandidateListItem listItem)
            {
                listItem.SetCurrentValue(CandidateListItem.IsCandidateSelectedProperty, false);
            }
        }
        else
        {
            TrySetCandidateItemSelected(e.NewValue);
        }
    }
    
    public bool TrySetCandidateItemSelected(object item)
    {
        var index = Items.IndexOf(item);
        if (index == -1)
        {
            return false;
        }

        return TrySetCandidateItemSelected(index);
    }
    
    public bool TrySetCandidateItemSelected(int index)
    {
        if (index < 0 || index > ItemCount - 1)
        {
            return false;
        }
        
        if (ItemsPanelRoot is CandidateVirtualizingStackPanel virtualizingStackPanel)
        {
            virtualizingStackPanel.ScrollCandidateItemIntoView(index);
        }

        for (var i = 0; i < ItemCount; i++)
        {
            if (ContainerFromIndex(i) is CandidateListItem childContainer)
            {
                if (i == index)
                {
                    childContainer.SetCurrentValue(CandidateListItem.IsCandidateSelectedProperty, true);
                    SetCurrentValue(CandidateSelectedItemProperty, Items[i]);
                    SetCurrentValue(CandidateSelectedIndexProperty, i);
                }
                else
                {
                    childContainer.SetCurrentValue(CandidateListItem.IsCandidateSelectedProperty, false);
                }
            }
        }
        return true;
    }
    
    private void ResetScrollViewer()
    {
        var sv = this.GetLogicalDescendants().OfType<ScrollViewer>().FirstOrDefault();
        if (sv != null)
        {
            sv.Offset = new Vector(0, 0);
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
            if (ContainerFromItem(CandidateSelectedItem) is CandidateListItem listItem)
            {
                UpdateSelection(listItem, !listItem.IsSelected, false,
                    true);
            }
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

    private void ClearState()
    {
        SelectedItems = null;
        SelectedItem  = null;
        SelectedIndex = -1;
    }
    
    protected override void NotifyListBoxItemClicked(ListBoxItem item)
    {
        if (IsSingleMode())
        {
            NotifyCommit();
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

            if (ContainerFromIndex(index) is ListBoxItem listBoxItem && listBoxItem.IsEnabled)
            {
                return index;
            }
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
            ConfigureEmptyIndicator();
        }
        if (change.Property == ItemFilterValueProperty ||
            change.Property == ItemFilterProperty)
        {
            CandidateSelectedIndex = -1;
            CandidateSelectedItem  = null;
        }
        
    }
    
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        var listItem = new CandidateListItem();
        NotifyContainerForItemCreated(listItem, item);
        return listItem;
    }
    
    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
    {
        return NeedsContainer<CandidateListItem>(item, out recycleKey);
    }
    
    protected override void PrepareListBoxItem(ListBoxItem listBoxItem, object? item, int index, CompositeDisposable disposables)
    {
        base.PrepareListBoxItem(listBoxItem, item, index, disposables);
        if (listBoxItem is CandidateListItem candidateListItem)
        {
            if (CandidateSelectedIndex != -1)
            {
                candidateListItem.SetCurrentValue(CandidateListItem.IsCandidateSelectedProperty, index == CandidateSelectedIndex);
            }
        }
    }
    
    protected override void ClearContainerForItemOverride(Control element)
    {
        base.ClearContainerForItemOverride(element);
        if (element is CandidateListItem listItem)
        {
            listItem.ClearValue(CandidateListItem.IsCandidateSelectedProperty);
        }
    }
    
    protected override void ConfigureEmptyIndicator()
    {
        SetCurrentValue(IsEffectiveEmptyVisibleProperty, IsShowEmptyIndicator && (ItemCount == 0 || (IsFiltering && FilterResultCount == 0)));
    }
    
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        CandidateSelectedIndex = -1;
        CandidateSelectedItem  = null;
    }
}