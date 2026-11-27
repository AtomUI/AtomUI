using AtomUI.Generated.AtomUIDesktopControls;
using Avalonia.Controls;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// The semantic level of a menu item container in the Semantic Part contract. The top-level items and the submenu
/// items of a plain <see cref="Menu" /> use mutually exclusive marker classes, so the owner has to pass this level
/// down when it prepares a container; a container cannot infer it. The same <see cref="MenuItem" /> can sit in the
/// first level of a menu bar, in a submenu at any depth, or in the popup of a <see cref="ContextMenu" />, a
/// <c>MenuFlyout</c> or a <c>DropdownButton</c>.
/// </summary>
internal enum MenuSemanticLevel
{
    /// <summary>
    /// The container is not in the scope of the plain <see cref="Menu" />. Reusers such as the popups of MenuFlyout,
    /// <see cref="ContextMenu" /> and DropdownButton keep the existing <c>.semantic-item</c> behavior and take no part
    /// in the level separation of the plain <see cref="Menu" />.
    /// </summary>
    None = 0,

    /// <summary>
    /// An item in the first level of the menu bar of a plain <see cref="Menu" />.
    /// </summary>
    TopLevel = 1,

    /// <summary>
    /// An item inside a submenu of a plain <see cref="Menu" />, at any nesting depth, including a group in that
    /// submenu.
    /// </summary>
    SubMenu = 2
}

/// <summary>
/// Writes a <see cref="MenuSemanticLevel" /> onto a container as marker classes. The levels are mutually
/// exclusive: the class of the other level is removed before the class of the current level is added, so a
/// container that moves between levels or is recycled keeps no stale level and never carries the class twice.
/// </summary>
internal static class MenuSemanticLevelScope
{
    // A group is only an intermediate hop of the selector routes, not a public Part, so it has no generated constant.
    // This follows the same practice as the group hop of NavMenu.
    private const string TopLevelGroupClass = "semantic-scope-group";
    private const string SubMenuGroupClass  = "semantic-sub-menu-group";

    /// <summary>
    /// Applies the item marker of <paramref name="level" /> to a menu item container and removes the marker of the
    /// other level. <see cref="MenuSemanticLevel.None" /> restores the default <c>semantic-item</c> semantics that
    /// reusers rely on.
    /// </summary>
    public static void ApplyItemLevel(Control container, MenuSemanticLevel level)
    {
        switch (level)
        {
            case MenuSemanticLevel.None:
            case MenuSemanticLevel.TopLevel:
                Apply(container, MenuSemanticParts.ItemClass, MenuSemanticParts.SubMenuItemClass);
                break;

            case MenuSemanticLevel.SubMenu:
                Apply(container, MenuSemanticParts.SubMenuItemClass, MenuSemanticParts.ItemClass);
                break;
        }
    }

    /// <summary>
    /// Applies the group marker of <paramref name="level" /> to a group container and removes the other group marker.
    /// <see cref="MenuSemanticLevel.None" /> removes both group markers of the plain <see cref="Menu" />.
    /// </summary>
    public static void ApplyGroupLevel(Control container, MenuSemanticLevel level)
    {
        switch (level)
        {
            case MenuSemanticLevel.None:
                container.Classes.Remove(TopLevelGroupClass);
                container.Classes.Remove(SubMenuGroupClass);
                break;
            case MenuSemanticLevel.TopLevel:
                Apply(container, TopLevelGroupClass, SubMenuGroupClass);
                break;

            case MenuSemanticLevel.SubMenu:
                Apply(container, SubMenuGroupClass, TopLevelGroupClass);
                break;
        }
    }

    public static void ApplyChildrenLevel(ItemsControl owner, MenuSemanticLevel level)
    {
        foreach (var container in owner.GetRealizedContainers())
        {
            switch (container)
            {
                case MenuItem item:
                    item.SemanticLevel = level;
                    break;
                case MenuItemGroup group:
                    group.SemanticLevel = level;
                    break;
            }
        }
    }

    private static void Apply(Control container, string keep, string drop)
    {
        if (container.Classes.Contains(drop))
        {
            container.Classes.Remove(drop);
        }

        if (!container.Classes.Contains(keep))
        {
            container.Classes.Add(keep);
        }
    }
}
