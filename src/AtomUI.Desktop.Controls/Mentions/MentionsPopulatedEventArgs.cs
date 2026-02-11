namespace AtomUI.Desktop.Controls;

public class MentionsPopulatedEventArgs
{
    public IList<IMentionOption>? Options { get; init; }
    
    public MentionsPopulatedEventArgs(IList<IMentionOption>? options)
    {
        Options = options;
    }
}