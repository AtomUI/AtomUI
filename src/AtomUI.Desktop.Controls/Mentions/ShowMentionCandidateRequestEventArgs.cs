using Avalonia;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Data for the request that the text area of a <see cref="Mentions" /> control raises when the caret stands
/// in a word that starts with a trigger character and the candidate popup is closed. The control raises
/// <see cref="Mentions.CandidateTriggered" />, places the popup from these values and then opens it.
/// </summary>
public class ShowMentionCandidateRequestEventArgs : EventArgs
{
    /// <summary>
    /// The bounds of the trigger character in the coordinate space of the text presenter; the popup is placed
    /// next to it.
    /// </summary>
    public Rect TriggerBounds { get; }
    /// <summary>
    /// The text typed after the trigger character; it is an empty string right after the trigger character.
    /// </summary>
    public string? Predicate { get; }
    /// <summary>
    /// The trigger character that was typed; it is a single-character entry of
    /// <see cref="Mentions.TriggerPrefix" />.
    /// </summary>
    public string TriggerChar { get; }

    /// <summary>
    /// Creates the arguments for a request that shows the candidates of a trigger character.
    /// </summary>
    /// <param name="triggerBounds">
    /// The bounds of the trigger character; stored as <see cref="TriggerBounds" />.
    /// </param>
    /// <param name="predicate">
    /// The text typed after the trigger character; stored as <see cref="Predicate" />.
    /// </param>
    /// <param name="triggerChar">
    /// The typed trigger character; stored as <see cref="TriggerChar" />.
    /// </param>
    public ShowMentionCandidateRequestEventArgs(Rect triggerBounds, string? predicate, string triggerChar)
    {
        TriggerBounds = triggerBounds;
        Predicate     = predicate;
        TriggerChar   = triggerChar;
    }
}