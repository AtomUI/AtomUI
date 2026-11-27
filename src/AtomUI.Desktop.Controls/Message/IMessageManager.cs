namespace AtomUI.Desktop.Controls;

/// <summary>
/// Shows transient messages of a window. <see cref="WindowMessageManager" /> implements it.
/// </summary>
public interface IMessageManager
{
    /// <summary>
    /// Shows the message as a new card in the stack of the manager. The call must happen on the UI thread.
    /// </summary>
    /// <param name="message">
    /// The message to show. Its values are copied into the card once, so later changes to the message do not
    /// reach the card.
    /// </param>
    /// <param name="classes">
    /// Style classes to add to the message card, or <see langword="null" /> for none.
    /// </param>
    public void Show(IMessage message, string[]? classes = null);
    /// <summary>
    /// Closes every message card of the manager. The close callback of a message runs after its card has closed.
    /// The call must happen on the UI thread.
    /// </summary>
    public void DestroyAll();
}
