namespace AtomUI.Desktop.Controls;

public class SelectSelectionChangedEventArgs : EventArgs
{
    public object? OldValue { get; }
    public object? NewValue { get; }
    
    public SelectMode Mode { get; }
    
    public SelectSelectionChangedEventArgs(SelectMode mode, object? oldValue, object? newValue)
    {
        Mode     = mode;
        OldValue = oldValue;
        NewValue = newValue;
    }
}