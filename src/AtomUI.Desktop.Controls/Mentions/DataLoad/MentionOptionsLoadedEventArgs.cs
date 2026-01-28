namespace AtomUI.Desktop.Controls;

public class MentionOptionsLoadedEventArgs : EventArgs
{
    public string? Predicate;
    public MentionOptionsLoadResult Result { get; }
    
    public MentionOptionsLoadedEventArgs(string? predicate, MentionOptionsLoadResult result)
    {
        Predicate = predicate;
        Result    = result;
    }
}