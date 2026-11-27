namespace AtomUI.Desktop.Controls;

/// <summary>
/// The layout of a <see cref="NavMenu" /> and the way its submenus open.
/// </summary>
public enum NavMenuMode
{
    /// <summary>
    /// Top-level items are stacked vertically; submenus open in a popup.
    /// </summary>
    Vertical,
    /// <summary>
    /// Top-level items are laid out in a row; submenus open in a popup.
    /// </summary>
    Horizontal,
    /// <summary>
    /// Submenus expand inside the menu below their parent item, indented by nesting depth.
    /// </summary>
    Inline
}