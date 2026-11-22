namespace AtomUI.Desktop.Controls;

/// <summary>
/// Defines how <see cref="Window"/> presents its title bar while the window is in
/// <see cref="Avalonia.Controls.WindowState.FullScreen"/>.
/// </summary>
public enum FullScreenTitleBarMode
{
    /// <summary>
    /// No managed full-screen title bar is presented. The application owns all full-screen chrome and exit entry points.
    /// </summary>
    Hidden,

    /// <summary>
    /// The title bar is closed by default, opens from the top edge of the window, overlays the content and closes
    /// again when nothing keeps it open.
    /// </summary>
    AutoHide,

    /// <summary>
    /// The title bar stays open and overlays the content. The content area keeps the same size as in
    /// <see cref="AutoHide"/>.
    /// </summary>
    Pinned,

    /// <summary>
    /// The title bar stays open above the content, and the content area starts below it.
    /// </summary>
    Docked
}
