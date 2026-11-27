namespace AtomUI.Desktop.Controls;

/// <summary>
/// The kind of a message. It selects the built-in status icon that a <see cref="MessageCard" /> shows while its
/// <see cref="MessageCard.Icon" /> is <see langword="null" />, and the colors and the shadow of the card.
/// </summary>
public enum MessageType
{
    /// <summary>
    /// A neutral message. The card shows a filled info-circle icon.
    /// </summary>
    Information,
    /// <summary>
    /// A message about a successful action. The card shows a filled check-circle icon.
    /// </summary>
    Success,
    /// <summary>
    /// A message about a risk. The card shows a filled exclamation-circle icon.
    /// </summary>
    Warning,
    /// <summary>
    /// A message about a failure. The card shows a filled close-circle icon.
    /// </summary>
    Error,
    /// <summary>
    /// A message about an action that is still running. The card shows a spinning loading icon.
    /// </summary>
    Loading
}