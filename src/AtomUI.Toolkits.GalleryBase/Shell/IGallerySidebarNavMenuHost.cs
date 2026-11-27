using AtomUI.Desktop.Controls;
using Avalonia.Controls;

namespace AtomUI.Toolkits.GalleryBase.Shell;

/// <summary>
/// Exposes the parts of a sidebar navigation view that <see cref="GalleryShellView" /> lays out and resizes.
/// </summary>
/// <remarks>
/// When the navigation view passed to the shell implements this interface, the sidebar column follows the width
/// of <see cref="SidebarNavMenu" />, and the shell collapses and expands that menu from the enclosing media
/// breakpoint. Otherwise the sidebar keeps its configured width.
/// </remarks>
public interface IGallerySidebarNavMenuHost
{
    /// <summary>
    /// The navigation menu of the sidebar. The width of the sidebar follows the width of this menu.
    /// </summary>
    NavMenu SidebarNavMenu { get; }

    /// <summary>
    /// The control shown at the right end of the brand header of the sidebar, such as the button that collapses the
    /// navigation. <see langword="null" /> when the navigation view has no such control.
    /// </summary>
    Control? SidebarHeaderAction { get; }
}
