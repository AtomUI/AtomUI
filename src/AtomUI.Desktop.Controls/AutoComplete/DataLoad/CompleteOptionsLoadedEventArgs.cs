namespace AtomUI.Desktop.Controls;

public class CompleteOptionsLoadedEventArgs : EventArgs
{
    public string? Predicate { get; }
    public CompleteOptionsLoadResult Result { get; }
    
    public CompleteOptionsLoadedEventArgs(string? predicate, CompleteOptionsLoadResult result)
    {
        Predicate = predicate;
        Result    = result;
    }
}