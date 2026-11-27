namespace AtomUI.Desktop.Controls;

/// <summary>
/// Edge of the area a drawer overlays that the drawer panel is docked to.
/// </summary>
public enum DrawerPlacement
{
    /// <summary>
    /// Docked to the left edge; the drawer slides in from the left.
    /// </summary>
    Left = 0,
    /// <summary>
    /// Docked to the top edge; the drawer slides in from the top.
    /// </summary>
    Top = 1,
    /// <summary>
    /// Docked to the right edge; the drawer slides in from the right. This is the default placement.
    /// </summary>
    Right = 2,
    /// <summary>
    /// Docked to the bottom edge; the drawer slides in from the bottom.
    /// </summary>
    Bottom = 3
}