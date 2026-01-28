using System.ComponentModel;

namespace AtomUI.Desktop.Controls;

public class MentionsPopulatingEventArgs : CancelEventArgs
{
    public string? Predicate { get; }

    public MentionsPopulatingEventArgs(string? predicate)
    {
        Predicate = predicate;
    }
}