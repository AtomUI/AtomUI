namespace AtomUI.Desktop.Controls;

/// <summary>
/// Controls whether the application logo is shown in the window title bar.
/// </summary>
public enum WindowTitleBarLogoVisibility
{
    /// <summary>
    /// Shows the logo if one is set, except on macOS when the title bar has no visible title.
    /// This is the default value.
    /// </summary>
    Auto,
    /// <summary>
    /// Shows the logo whenever a logo or a logo template is set.
    /// </summary>
    Always,
    /// <summary>
    /// Never shows the logo.
    /// </summary>
    Never
}
