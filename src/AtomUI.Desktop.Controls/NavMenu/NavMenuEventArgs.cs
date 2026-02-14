using Avalonia.Interactivity;

namespace AtomUI.Desktop.Controls;

public class NavMenuItemClickEventArgs : RoutedEventArgs
{
    public NavMenuItemClickEventArgs(RoutedEvent routedEvent, INavMenuItem navMenuItem)
        : base(routedEvent)
    {
        NavMenuItem = navMenuItem;
    }

    public INavMenuItem NavMenuItem { get; }
}

public class NavMenuNodeSelectedEventArgs : RoutedEventArgs
{
    public NavMenuNodeSelectedEventArgs(RoutedEvent routedEvent, INavMenuNode menuNode)
        : base(routedEvent)
    {
        NavMenuNode = menuNode;
    }

    public INavMenuNode NavMenuNode { get; }
}