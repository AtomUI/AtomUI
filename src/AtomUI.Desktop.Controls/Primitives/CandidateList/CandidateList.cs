using Avalonia;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;

namespace AtomUI.Desktop.Controls.Primitives;

public class CandidateList : ListBox, ICandidateList
{
    #region 公共属性定义

    public static readonly StyledProperty<bool> IsCandidateItemNavigationEnabledProperty =
        AvaloniaProperty.Register<CandidateList, bool>(nameof(IsCandidateItemNavigationEnabled), true);
    
    public bool IsCandidateItemNavigationEnabled
    {
        get => GetValue(IsCandidateItemNavigationEnabledProperty);
        set => SetValue(IsCandidateItemNavigationEnabledProperty, value);
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

    static CandidateList()
    {
        SelectedItemProperty.Changed.AddClassHandler<CandidateList>((list, args) => list.HandleSelectItemChanged(args));
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
                SelectPreviousItem();
                e.Handled = true;
                break;

            case Key.Down:
                SelectNextItem();
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
    
    protected override void NotifyListBoxItemClicked(ListBoxItem item)
    {
        NotifyCommit();
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
}