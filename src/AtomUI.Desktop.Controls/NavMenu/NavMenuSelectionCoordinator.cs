namespace AtomUI.Desktop.Controls;

internal sealed class NavMenuSelectionCoordinator
{
    private INavMenuNode? _appliedSelectedNode;
    private NavMenuItem? _appliedSelectedItem;
    // This owns only the selected-path projection on generated containers. ParentNode can
    // already be cleared when an entry is removed; Forget releases recycled containers.
    private HashSet<NavMenuItem> _appliedSelectedPathItems = new();

    public bool Select(NavMenu menu, NavMenuItem menuItem)
    {
        var selectedNode = ((INavMenuItem)menuItem).Node;
        if (selectedNode is null)
        {
            return false;
        }

        if (ReferenceEquals(_appliedSelectedNode, selectedNode) && menuItem.IsSelected)
        {
            _appliedSelectedItem = menuItem;
            return ReferenceEquals(menu.SelectedItem, selectedNode);
        }

        var newItems          = NavMenu.CollectSelectPathItems(menuItem);
        var newSelectedPaths  = NavMenu.BuildSelectPathSet(newItems);
        var oldSelectedNode   = _appliedSelectedNode ?? menu.SelectedItem;
        var oldSelectedItem   = ResolveLatestSelectedItem(menu, oldSelectedNode);
        var oldSelectedPaths  = _appliedSelectedPathItems;
        var requestedNode     = menu.SelectedItem;
        var appliedPaths      = new HashSet<NavMenuItem>(oldSelectedPaths);
        _appliedSelectedPathItems = appliedPaths;

        // Property callbacks can recycle containers or enter another selection. Each
        // transition owns its projection set; the previous set is a stable snapshot.
        bool IsCurrentSelection() => ReferenceEquals(_appliedSelectedPathItems, appliedPaths) &&
                                     ReferenceEquals(menu.SelectedItem, requestedNode);

        foreach (var oldInSelectPathItem in oldSelectedPaths)
        {
            if (!newSelectedPaths.Contains(oldInSelectPathItem))
            {
                appliedPaths.Remove(oldInSelectPathItem);
                oldInSelectPathItem.SetCurrentValue(NavMenuItem.IsInSelectedPathProperty, false);
                if (!IsCurrentSelection())
                {
                    return false;
                }
            }
        }

        _appliedSelectedNode = null;
        _appliedSelectedItem = null;
        if (oldSelectedItem != null)
        {
            var oldParentItem = ResolveSelectionOwner(menu, oldSelectedItem);
            oldParentItem.SelectChildItem(oldSelectedItem, false);
            if (!IsCurrentSelection())
            {
                return false;
            }
        }

        foreach (var newInSelectPathItem in newItems)
        {
            // A preceding property callback may have removed another path container.
            if (!ReferenceEquals(newInSelectPathItem.OwnerMenu, menu))
            {
                return false;
            }

            appliedPaths.Add(newInSelectPathItem);
            newInSelectPathItem.SetCurrentValue(NavMenuItem.IsInSelectedPathProperty, true);
            if (!IsCurrentSelection())
            {
                return false;
            }
        }

        if (!ReferenceEquals(menuItem.OwnerMenu, menu))
        {
            return false;
        }

        var parentItem = ResolveSelectionOwner(menu, menuItem);
        _appliedSelectedNode = selectedNode;
        _appliedSelectedItem = menuItem;
        parentItem.SelectChildItem(menuItem, true);
        return IsCurrentSelection() &&
               ReferenceEquals(_appliedSelectedItem, menuItem) &&
               ReferenceEquals(menuItem.OwnerMenu, menu) &&
               menu.TryPublishNavMenuItemSelection(menuItem);
    }

    public void ClearSelection(NavMenu menu)
    {
        var oldSelectedNode = _appliedSelectedNode ?? menu.SelectedItem;
        var oldSelectedItem = ResolveLatestSelectedItem(menu, oldSelectedNode);
        var oldSelectedPaths = _appliedSelectedPathItems;
        var appliedPaths = new HashSet<NavMenuItem>(oldSelectedPaths);
        _appliedSelectedPathItems = appliedPaths;
        foreach (var oldInSelectPathItem in oldSelectedPaths)
        {
            appliedPaths.Remove(oldInSelectPathItem);
            oldInSelectPathItem.SetCurrentValue(NavMenuItem.IsInSelectedPathProperty, false);
            if (!ReferenceEquals(_appliedSelectedPathItems, appliedPaths))
            {
                return;
            }
        }

        _appliedSelectedNode = null;
        _appliedSelectedItem = null;
        if (oldSelectedItem is not null)
        {
            var oldParentItem = ResolveSelectionOwner(menu, oldSelectedItem);
            oldParentItem.SelectChildItem(oldSelectedItem, false);
        }
    }

    public void PrepareContainer(NavMenu menu, NavMenuItem menuItem)
    {
        var node = ((INavMenuItem)menuItem).Node;
        if (node is null)
        {
            return;
        }

        var selectedNode = _appliedSelectedNode ?? menu.SelectedItem;
        var isSelected = ReferenceEquals(node, selectedNode);
        var isInSelectedPath = !isSelected && IsAncestorOf(node, selectedNode);
        var appliedPaths = _appliedSelectedPathItems;
        if (isInSelectedPath)
        {
            appliedPaths.Add(menuItem);
        }
        else
        {
            appliedPaths.Remove(menuItem);
        }

        if (isSelected && ReferenceEquals(node, _appliedSelectedNode))
        {
            _appliedSelectedItem = menuItem;
        }

        menuItem.SetCurrentValue(NavMenuItem.IsSelectedProperty, isSelected);
        if (ReferenceEquals(_appliedSelectedPathItems, appliedPaths) &&
            ReferenceEquals(((INavMenuItem)menuItem).Node, node))
        {
            menuItem.SetCurrentValue(NavMenuItem.IsInSelectedPathProperty, isInSelectedPath);
        }
    }

    public void Forget(NavMenuItem menuItem)
    {
        _appliedSelectedPathItems.Remove(menuItem);
        if (ReferenceEquals(_appliedSelectedItem, menuItem))
        {
            _appliedSelectedItem = null;
        }
    }

    private NavMenuItem? ResolveLatestSelectedItem(NavMenu menu, INavMenuNode? selectedNode)
    {
        if (_appliedSelectedItem is not null)
        {
            return _appliedSelectedItem;
        }

        return selectedNode is not null && BelongsToMenu(menu, selectedNode)
            ? menu.FindRealizedMenuItem(selectedNode)
            : null;
    }

    private static bool BelongsToMenu(NavMenu menu, INavMenuNode node)
    {
        var rootNode = node;
        while (rootNode.ParentNode is INavMenuNode parentNode)
        {
            rootNode = parentNode;
        }

        return NavMenuEntryGraph.ContainsDirectNode(menu.Items, rootNode);
    }

    private static bool IsAncestorOf(INavMenuNode candidate, INavMenuNode? selectedNode)
    {
        var current = selectedNode?.ParentNode as INavMenuNode;
        while (current is not null)
        {
            if (ReferenceEquals(current, candidate))
            {
                return true;
            }

            current = current.ParentNode as INavMenuNode;
        }

        return false;
    }

    private static IMenuChildSelectable ResolveSelectionOwner(NavMenu menu, NavMenuItem menuItem)
    {
        return menuItem.SemanticParentItem is not null
            ? menuItem.SemanticParentItem
            : menu;
    }
}
