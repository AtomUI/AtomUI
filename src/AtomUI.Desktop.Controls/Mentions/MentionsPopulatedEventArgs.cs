namespace AtomUI.Desktop.Controls;

public class MentionsPopulatedEventArgs
{
    public IList<IMentionOption>? Data { get; init; }
    
    public MentionsPopulatedEventArgs(IList<IMentionOption>? data)
    {
        Data = data;
    }
}