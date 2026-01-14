namespace AtomUI.Desktop.Controls;

public class MentionOptionLoadedEventArgs : EventArgs
{
    public string? Predicate;
    public MentionOptionLoadResult Result { get; }
    
    public MentionOptionLoadedEventArgs(string? predicate, MentionOptionLoadResult result)
    {
        Predicate = predicate;
        Result    = result;
    }
}