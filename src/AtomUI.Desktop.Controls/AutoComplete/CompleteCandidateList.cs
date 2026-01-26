using Avalonia;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;

namespace AtomUI.Desktop.Controls;

internal class CompleteCandidateList : ListBox, ISelectCandidateList
{
    #region 公共事件定义
    public static readonly RoutedEvent<RoutedEventArgs> CommitEvent =
        RoutedEvent.Register<CompleteCandidateList, RoutedEventArgs>(
            nameof(Commit),
            RoutingStrategies.Bubble);
    
    public static readonly RoutedEvent<RoutedEventArgs> CancelEvent =
        RoutedEvent.Register<CompleteCandidateList, RoutedEventArgs>(
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
    
    private bool _ignoringSelectionChanged { get; set; }

    static CompleteCandidateList()
    {
        SelectedItemProperty.Changed.AddClassHandler<CompleteCandidateList>((list, args) => list.HandleSelectItemChanged(args));
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
                if ((e.KeyModifiers & KeyModifiers.Alt) == KeyModifiers.None)
                {
                    SelectedIndexIncrement();
                    e.Handled = true;
                }
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
        try
        {
            _ignoringSelectionChanged = true;
            SelectedItem              = null;
            SelectedIndex             = -1;
        }
        finally
        {
            _ignoringSelectionChanged = false;
        }
    }
    
    protected void SelectedIndexDecrement()
    {
        int index = SelectedIndex;
        if (index >= 0)
        {
            SelectedIndex--;
        }
        else if (index == -1)
        {
            SelectedIndex = ItemCount - 1;
        }
    }
    
    protected void SelectedIndexIncrement()
    {
        SelectedIndex = SelectedIndex + 1 >= ItemCount ? -1 : SelectedIndex + 1;
    }
    
    protected override void NotifyListBoxItemClicked(ListBoxItem item)
    {
        NotifyCommit();
    }
}