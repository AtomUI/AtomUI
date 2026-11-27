using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia.Controls;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// An <see cref="IMessage" /> with settable properties. It reports changes of <see cref="Content" /> and
/// <see cref="Icon" /> through <see cref="PropertyChanged" />.
/// </summary>
public class Message : IMessage, INotifyPropertyChanged
{
    private string _content;
    private PathIcon? _icon;

    /// <summary>
    /// Creates a message.
    /// </summary>
    /// <param name="content">
    /// The text that the message card shows.
    /// </param>
    /// <param name="type">
    /// The type of the message. It selects the built-in status icon and the colors of the card.
    /// </param>
    /// <param name="icon">
    /// A custom icon, or <see langword="null" /> to show the built-in icon of the type.
    /// </param>
    /// <param name="expiration">
    /// How long the message stays open before it closes by itself, or <see langword="null" /> for three seconds.
    /// A value of <see cref="TimeSpan.Zero" /> or less keeps the message open until the manager closes it.
    /// </param>
    /// <param name="onClose">
    /// An action that runs when the message is closed, or <see langword="null" /> for none.
    /// </param>
    public Message(string content,
                   MessageType type = MessageType.Information,
                   PathIcon? icon = null,
                   TimeSpan? expiration = null,
                   Action? onClose = null)
    {
        _content   = content;
        _icon      = icon;
        Type       = type;
        Expiration = expiration.HasValue ? expiration.Value : TimeSpan.FromSeconds(3);
        OnClose    = onClose;
    }

    /// <summary>
    /// The text that the message card shows. Setting a different value raises <see cref="PropertyChanged" />.
    /// </summary>
    public string Content
    {
        get => _content;

        set
        {
            if (_content != value)
            {
                _content = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// A custom icon in front of the text. When it is <see langword="null" />, the card shows the built-in icon of
    /// <see cref="Type" />. Setting a different instance raises <see cref="PropertyChanged" />.
    /// </summary>
    public PathIcon? Icon
    {
        get => _icon;

        set
        {
            if (!ReferenceEquals(_icon, value))
            {
                _icon = value;
                OnPropertyChanged();
            }
        }
    }

    /// <inheritdoc />
    public MessageType Type { get; set; }

    /// <inheritdoc />
    public TimeSpan Expiration { get; set; }

    /// <inheritdoc />
    public Action? OnClose { get; set; }

    /// <summary>
    /// Raised after <see cref="Content" /> or <see cref="Icon" /> changed to a different value. The other properties
    /// do not raise it.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Raises <see cref="PropertyChanged" /> after a property changed. The setters of <see cref="Content" /> and
    /// <see cref="Icon" /> call it. An override must call the base method for the event to be raised.
    /// </summary>
    /// <param name="propertyName">
    /// The name of the changed property. It defaults to the name of the calling member.
    /// </param>
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
