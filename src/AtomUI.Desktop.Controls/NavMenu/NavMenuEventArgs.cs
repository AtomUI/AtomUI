using Avalonia.Interactivity;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Carries the menu item that was clicked, for the <see cref="NavMenu.NavMenuItemClick" /> event.
/// </summary>
public class NavMenuItemClickEventArgs : RoutedEventArgs
{
    /// <summary>
    /// Creates the event arguments that report <paramref name="navMenuItem" /> as the clicked item.
    /// </summary>
    public NavMenuItemClickEventArgs(RoutedEvent routedEvent, INavMenuItem navMenuItem)
        : base(routedEvent)
    {
        NavMenuItem = navMenuItem;
    }

    /// <summary>
    /// Gets the menu item that was clicked.
    /// </summary>
    public INavMenuItem NavMenuItem { get; }
}

/// <summary>
/// Carries the node that became selected, for the <see cref="NavMenu.NavMenuNodeSelected" /> event.
/// </summary>
public class NavMenuNodeSelectedEventArgs : RoutedEventArgs
{
    /// <summary>
    /// Creates the event arguments that report <paramref name="menuNode" /> as the selected node.
    /// </summary>
    public NavMenuNodeSelectedEventArgs(RoutedEvent routedEvent, INavMenuNode menuNode)
        : base(routedEvent)
    {
        NavMenuNode = menuNode;
    }

    /// <summary>
    /// Gets the node that became selected.
    /// </summary>
    public INavMenuNode NavMenuNode { get; }
}