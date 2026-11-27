namespace AtomUI.Desktop.Controls;

/// <summary>
/// Selects the host that presents a dialog: a native window or an overlay above its placement target.
/// </summary>
public enum DialogHostType
{
    /// <summary>
    /// Presents the dialog in a separate native window owned by the window that hosts it. The native window
    /// owns the title bar, so the header of the dialog is hidden. Falls back to <see cref="Overlay" /> on
    /// platforms without native window support.
    /// </summary>
    Window,
    /// <summary>
    /// Presents the dialog as an overlay above its placement target, inside the window that contains it.
    /// This is the default host.
    /// </summary>
    Overlay
}