namespace AtomUI.Desktop.Controls;

public class MentionCandidateTriggeredEventArgs : EventArgs
{
    public string TriggerChar { get; }

    public MentionCandidateTriggeredEventArgs(string triggerChar)
    {
        TriggerChar = triggerChar;
    }
}