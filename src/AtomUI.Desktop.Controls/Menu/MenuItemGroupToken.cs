using AtomUI.Theme.DesignTokens;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Design tokens of the title shown above the items of a menu item group (<see cref="MenuItemGroup" />).
/// </summary>
[ControlDesignToken]
public sealed partial class MenuItemGroupToken : AbstractControlDesignToken
{
    /// <summary>
    /// Font weight of the heading above a group of menu items.
    /// </summary>
    public partial FontWeight TitleFontWeight { get; set; }
}
