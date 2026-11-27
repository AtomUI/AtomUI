namespace AtomUI.Desktop.Controls;

/// <summary>
/// Data for the <see cref="Mentions.CandidateTriggered" /> event. The control raises the event right before
/// its candidate popup opens for a trigger character typed in the text.
/// </summary>
public class MentionCandidateTriggeredEventArgs : EventArgs
{
    /// <summary>
    /// The trigger character that was typed; it is a single-character entry of
    /// <see cref="Mentions.TriggerPrefix" />.
    /// </summary>
    public string TriggerChar { get; }

    /// <summary>
    /// Creates the arguments for a typed trigger character.
    /// </summary>
    /// <param name="triggerChar">
    /// The typed trigger character; stored as <see cref="TriggerChar" />.
    /// </param>
    public MentionCandidateTriggeredEventArgs(string triggerChar)
    {
        TriggerChar = triggerChar;
    }
}