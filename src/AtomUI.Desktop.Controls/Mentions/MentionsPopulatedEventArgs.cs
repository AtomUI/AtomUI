namespace AtomUI.Desktop.Controls;

public class MentionsPopulatedEventArgs
{
    public IReadOnlyList<IMentionOption>? Data { get; init; }
    
    public MentionsPopulatedEventArgs(IReadOnlyList<IMentionOption>? data)
    {
        Data = data;
    }
}