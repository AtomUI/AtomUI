using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Design tokens of the menu flyout presenter. They add the frame border of the menu flyout popup to the menu
/// popup tokens inherited from the base class.
/// </summary>
[ControlDesignToken]
public sealed partial class MenuFlyoutPresenterToken : AbstractMenuPopupToken
{
    /// <summary>
    /// The border of the menu flyout's arrow box, drawn when the arrow box has a border (<c>ArrowDecoratedBox</c>
    /// <c>ContentBorderThickness</c>). Defaults to <c>ColorBorderSecondary</c>, the ArrowDecoratedBox's own
    /// <c>ContentBorderColor</c> default, so a theme can give menus a border color of their own.
    /// </summary>
    public partial Color MenuPopupBorderColor { get; set; }

    /// <summary>
    /// The thickness of the menu flyout popup's frame border, drawn with <see cref="MenuPopupBorderColor" /> along the
    /// arrow box. Defaults to an empty thickness (no border, the previous rendering); a theme that gives the popup a
    /// border sets it.
    /// </summary>
    public partial Thickness MenuPopupBorderThickness { get; set; }
}
