namespace AtomUI.Desktop.Controls;

public class CompleteOptionLoadedEventArgs : EventArgs
{
    public string? Predicate;
    public CompleteOptionLoadResult Result { get; }
    
    public CompleteOptionLoadedEventArgs(string? predicate, CompleteOptionLoadResult result)
    {
        Predicate = predicate;
        Result    = result;
    }
}