namespace AtomUI.Desktop.Controls;

/// <summary>
/// Severity of a notification. It selects the status icon, the frame colors and the shadows of the
/// <see cref="NotificationCard" />.
/// </summary>
public enum NotificationType
{
    /// <summary>
    /// No severity. The card shows no status icon and keeps the default frame colors and shadows.
    /// </summary>
    Default = -1,
    /// <summary>
    /// Shows the information icon and paints the card with the information colors.
    /// </summary>
    Information,
    /// <summary>
    /// Shows the success icon and paints the card with the success colors.
    /// </summary>
    Success,
    /// <summary>
    /// Shows the warning icon and paints the card with the warning colors.
    /// </summary>
    Warning,
    /// <summary>
    /// Shows the error icon and paints the card with the error colors.
    /// </summary>
    Error
}
