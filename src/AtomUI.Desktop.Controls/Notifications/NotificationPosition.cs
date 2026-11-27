namespace AtomUI.Desktop.Controls;

/// <summary>
/// Corner or edge of the host window where a notification or message stack is anchored.
/// </summary>
public enum NotificationPosition
{
    /// <summary>
    /// Anchors the stack at the top-left corner of the window.
    /// </summary>
    TopLeft,
    /// <summary>
    /// Anchors the stack at the top-right corner of the window. This is the default of
    /// <see cref="WindowNotificationManager.Position" />.
    /// </summary>
    TopRight,
    /// <summary>
    /// Anchors the stack at the bottom-left corner of the window.
    /// </summary>
    BottomLeft,
    /// <summary>
    /// Anchors the stack at the bottom-right corner of the window.
    /// </summary>
    BottomRight,
    /// <summary>
    /// Anchors the stack at the middle of the top edge of the window.
    /// </summary>
    TopCenter,
    /// <summary>
    /// Anchors the stack at the middle of the bottom edge of the window.
    /// </summary>
    BottomCenter
}