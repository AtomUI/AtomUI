using Avalonia;

namespace AtomUI.Desktop.Controls;

public class ShowMentionCandidateRequestEventArgs : EventArgs
{
    public Rect TriggerBounds { get; }
    public string? Predicate { get; }
    public string TriggerChar { get; }

    public ShowMentionCandidateRequestEventArgs(Rect triggerBounds, string? predicate, string triggerChar)
    {
        TriggerBounds = triggerBounds;
        Predicate     = predicate;
        TriggerChar   = triggerChar;
    }
}