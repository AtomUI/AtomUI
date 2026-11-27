namespace AtomUI.Desktop.Controls;

/// <summary>
/// The side of the trigger character's line at which a <see cref="Mentions" /> control opens its candidate
/// popup.
/// </summary>
public enum MentionsPlacementMode
{
    /// <summary>
    /// The popup opens above the line that holds the trigger character.
    /// </summary>
    Top,
    /// <summary>
    /// The popup opens below the line that holds the trigger character; this is the default of
    /// <see cref="Mentions.Placement" />.
    /// </summary>
    Bottom,
}