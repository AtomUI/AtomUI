using System.ComponentModel;

namespace AtomUI.Desktop.Controls;

public class SelectOptionsLoadingEventArgs : CancelEventArgs
{
    public object? Context { get; }
    
    public SelectOptionsLoadingEventArgs(object? context)
    {
        Context = context;
    }
}