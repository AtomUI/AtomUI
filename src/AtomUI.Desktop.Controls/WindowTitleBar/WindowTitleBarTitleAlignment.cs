namespace AtomUI.Desktop.Controls;

/// <summary>
/// Controls the horizontal placement of the window title in the area between the leading and trailing groups of
/// the title bar.
/// </summary>
public enum WindowTitleBarTitleAlignment
{
    /// <summary>
    /// Uses the alignment of the layout strategy of the host operating system: <see cref="WindowCenter" /> on macOS
    /// and <see cref="Left" /> on Windows and Linux. This is the default value.
    /// </summary>
    Auto,
    /// <summary>
    /// Places the title at the left edge of the area between the leading and trailing groups.
    /// </summary>
    Left,
    /// <summary>
    /// Centers the title in the area between the leading and trailing groups.
    /// </summary>
    Center,
    /// <summary>
    /// Centers the title on the window's center line. The title is narrowed when it would extend beyond the area
    /// between the leading and trailing groups.
    /// </summary>
    WindowCenter,
    /// <summary>
    /// Places the title at the right edge of the area between the leading and trailing groups.
    /// </summary>
    Right
}
