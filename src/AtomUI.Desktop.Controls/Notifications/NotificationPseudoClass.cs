namespace AtomUI.Desktop.Controls;

/// <summary>
/// Pseudo-class names that mark where a notification or message stack is placed.
/// <see cref="WindowNotificationManager" />, <see cref="WindowMessageManager" /> and
/// <see cref="NotificationCard" /> set exactly one of them from their position.
/// </summary>
public static class NotificationPseudoClass
{
    /// <summary>
    /// Set while the stack is anchored at the top-left corner of the window.
    /// </summary>
    public const string TopLeft = ":topleft";
    /// <summary>
    /// Set while the stack is anchored at the top-right corner of the window.
    /// </summary>
    public const string TopRight = ":topright";
    /// <summary>
    /// Set while the stack is anchored at the bottom-left corner of the window.
    /// </summary>
    public const string BottomLeft = ":bottomleft";
    /// <summary>
    /// Set while the stack is anchored at the bottom-right corner of the window.
    /// </summary>
    public const string BottomRight = ":bottomright";
    /// <summary>
    /// Set while the stack is anchored at the middle of the top edge of the window.
    /// </summary>
    public const string TopCenter = ":topcenter";
    /// <summary>
    /// Set while the stack is anchored at the middle of the bottom edge of the window.
    /// </summary>
    public const string BottomCenter = ":bottomcenter";
}