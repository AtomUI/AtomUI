namespace AtomUI.Desktop.Controls;

/// <summary>
/// Shows <see cref="INotification" /> instances as cards in a stack, and closes all of them at once.
/// </summary>
public interface INotificationManager
{
    /// <summary>
    /// Adds a card for the notification to the stack and, when its <see cref="INotification.Expiration" /> is
    /// greater than <see cref="TimeSpan.Zero" />, starts the countdown to its automatic close. May be called
    /// before the manager is attached to a visual tree.
    /// </summary>
    /// <param name="notification">
    /// The notification to display.
    /// </param>
    /// <param name="classes">
    /// Style classes added to the card, or <see langword="null" /> to add none.
    /// </param>
    public void Show(INotification notification, string[]? classes = null);
    /// <summary>
    /// Closes every notification that is shown, running each <see cref="INotification.OnClose" /> callback.
    /// </summary>
    public void DestroyAll();
}
