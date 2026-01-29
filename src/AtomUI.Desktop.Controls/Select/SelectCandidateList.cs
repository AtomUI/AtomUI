using AtomUI.Desktop.Controls.Primitives;
using Avalonia;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;

namespace AtomUI.Desktop.Controls;

internal class SelectCandidateList : List, ICandidateList
{
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
                HandleCommit();
                e.Handled = true;
                break;

            case Key.Up:
                SelectedIndexDecrement();
                e.Handled = true;
                break;

            case Key.Down:
                SelectedIndexIncrement();
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
        SelectedItem  = null;
        SelectedIndex = -1;
    }
    
    protected void SelectedIndexDecrement()
    {
        int index = SelectedIndex;
        --index;
        if (index == -1)
        {
            index = ItemCount - 1;
        }
        SelectedIndex = index;
    }
    
    protected void SelectedIndexIncrement()
    {
        SelectedIndex = SelectedIndex + 1 >= ItemCount ? 0 : SelectedIndex + 1;
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
}