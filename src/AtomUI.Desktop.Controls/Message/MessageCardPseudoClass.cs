namespace AtomUI.Desktop.Controls;

/// <summary>
/// The pseudo-classes that a <see cref="MessageCard" /> sets for the value of its message type.
/// </summary>
public static class MessageCardPseudoClass
{
    /// <summary>
    /// The pseudo-class that a <see cref="MessageCard" /> carries while its
    /// <see cref="MessageCard.MessageType" /> is <see cref="MessageType.Error" />.
    /// </summary>
    public const string Error = ":error";
    /// <summary>
    /// The pseudo-class that a <see cref="MessageCard" /> carries while its
    /// <see cref="MessageCard.MessageType" /> is <see cref="MessageType.Information" />.
    /// </summary>
    public const string Information = ":information";
    /// <summary>
    /// The pseudo-class that a <see cref="MessageCard" /> carries while its
    /// <see cref="MessageCard.MessageType" /> is <see cref="MessageType.Success" />.
    /// </summary>
    public const string Success = ":success";
    /// <summary>
    /// The pseudo-class that a <see cref="MessageCard" /> carries while its
    /// <see cref="MessageCard.MessageType" /> is <see cref="MessageType.Warning" />.
    /// </summary>
    public const string Warning = ":warning";
    /// <summary>
    /// The pseudo-class that a <see cref="MessageCard" /> carries while its
    /// <see cref="MessageCard.MessageType" /> is <see cref="MessageType.Loading" />.
    /// </summary>
    public const string Loading = ":loading";
}