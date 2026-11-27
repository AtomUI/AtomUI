using Avalonia.Controls;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Describes one message for <see cref="IMessageManager.Show" />. <see cref="Message" /> is an implementation.
/// </summary>
public interface IMessage
{
   /// <summary>
   /// The text that the message card shows.
   /// </summary>
   string Content { get; }

   /// <summary>
   /// A custom icon in front of the text. When it is <see langword="null" />, the card shows the built-in icon
   /// of <see cref="Type" />.
   /// </summary>
   PathIcon? Icon { get; }

   /// <summary>
   /// The type of the message. It selects the built-in status icon and the colors of the card.
   /// </summary>
   MessageType Type { get; }

   /// <summary>
   /// How long the message stays open before it closes by itself. A value of <see cref="TimeSpan.Zero" /> or less
   /// means that it does not expire; it stays until the manager closes it, for example through
   /// <see cref="IMessageManager.DestroyAll" />.
   /// </summary>
   TimeSpan Expiration { get; }

   /// <summary>
   /// Gets an Action to be run when the message is closed.
   /// </summary>
   Action? OnClose { get; }
}