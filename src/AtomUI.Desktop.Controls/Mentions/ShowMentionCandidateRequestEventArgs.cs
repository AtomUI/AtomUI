using Avalonia;

namespace AtomUI.Desktop.Controls;

public class ShowMentionCandidateRequestEventArgs : EventArgs
{
    public Rect TriggerBounds { get; }
    public string? Predicate { get; }

    public ShowMentionCandidateRequestEventArgs(Rect triggerBounds, string? predicate)
    {
        TriggerBounds = triggerBounds;
        Predicate     = predicate;
    }
}