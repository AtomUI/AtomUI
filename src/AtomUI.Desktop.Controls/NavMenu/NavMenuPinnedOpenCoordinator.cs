using System.Collections;
using Avalonia.Threading;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Owns the pinned-open request of a <see cref="NavMenu" /> and applies it to the menu item containers that are
/// generated at run time.
///
/// The request is keyed by <see cref="INavMenuNode" />, not by the <see cref="NavMenuItem" /> container.
/// Applying it only to the containers that are realized at a given moment loses requests: property changes,
/// attach and mode switches all happen before containers are generated or while they are rebuilt. A declaration
/// such as <c>IsPopupPinnedOpen="True"</c> in AXAML writes the property before the template is applied and
/// before any container exists to pin. Expressed as a node path, the request outlives the container lifecycle:
/// every trigger only updates the request, and a realized container is caught up from the same request by path.
///
/// Container preparation goes through <see cref="PrepareContainer" />, which follows the same container timing
/// contract as the selection state. Property changes, attach and mode switches evaluate immediately through
/// <see cref="Reconcile" />. Changes of the entry graph go through <see cref="QueueReconcile" />, which coalesces
/// them and evaluates once the ItemsPresenter has settled, because the containers and the node mapping may not
/// have settled inside a collection change callback.
/// </summary>
internal sealed class NavMenuPinnedOpenCoordinator
{
    // The nodes of the sub-menus to keep open, in root -> leaf order. The list may contain nodes whose container
    // is not realized yet.
    private readonly List<INavMenuNode> _pinnedPath = [];

    // The realized containers that are pinned for _pinnedPath. They are tracked so that a release unpins exactly
    // these containers and leaves no pinned state behind.
    private readonly List<NavMenuItem> _pinnedItems = [];

    private int _reconcileGeneration;
    private bool _isApplying;

    /// <summary>
    /// Reconciles the pin-open request with the current state of the menu.
    /// Releases the request when pinning is off or the menu is in <see cref="NavMenuMode.Inline" /> mode; otherwise
    /// resolves the pinned path and applies the pinned state to the realized containers. The method is idempotent
    /// and may be called again on property changes, attach and mode switches.
    /// </summary>
    public void Reconcile(NavMenu menu)
    {
        if (!menu.IsPopupPinnedOpen || menu.EffectiveMode == NavMenuMode.Inline)
        {
            Release();
            return;
        }

        PruneStalePath(menu);
        if (_pinnedPath.Count == 0)
        {
            _pinnedPath.AddRange(ResolvePath(menu));
        }

        ApplyToRealizedContainers(menu);
    }

    /// <summary>
    /// Schedules one reconcile of the menu and discards it when a later request or a release follows.
    /// Call it for item collection changes: at that moment the containers and the node mapping may not have settled,
    /// so an immediate evaluation could resolve a node to the wrong container. The request is kept as state, so the
    /// deferred evaluation loses no request; consecutive changes are reconciled once, at DispatcherPriority.Loaded.
    /// </summary>
    public void QueueReconcile(NavMenu menu)
    {
        var generation = ++_reconcileGeneration;
        Dispatcher.UIThread.Post(
            () =>
            {
                if (generation == _reconcileGeneration)
                {
                    Reconcile(menu);
                }
            },
            DispatcherPriority.Loaded);
    }

    /// <summary>
    /// Applies the pinned state to a menu item container as soon as it is prepared, when the menu requests pinning
    /// and the node of the container lies on the pinned path. An empty pinned path is resolved by a
    /// <see cref="Reconcile" /> first.
    /// The request is settled before any container exists, so each container is caught up when it is realized instead
    /// of waiting for a global rescan that may run later. Because the request is stored by node, a container that is
    /// rebuilt after a mode switch or an item reorder is caught up here again. <see cref="NavMenuItem" /> opens its
    /// sub-menu on attach according to the pinned state, so the container need not be attached yet.
    /// </summary>
    public void PrepareContainer(NavMenu menu, NavMenuItem menuItem)
    {
        if (!menu.IsPopupPinnedOpen || ((INavMenuItem)menuItem).Node is not { } node)
        {
            return;
        }

        if (_pinnedPath.Count == 0)
        {
            Reconcile(menu);
        }

        if (_pinnedPath.Contains(node))
        {
            ApplyPinnedState(menuItem);
        }
    }

    /// <summary>
    /// Makes the pinned path the chain of a newly opened sub-menu node and its ancestors, and releases the pinned
    /// items that are not on that chain. Call it after a sub-menu opens.
    /// Only a real sub-menu replaces the path: a <see cref="NavMenuItem" /> without children raises
    /// <c>SubmenuOpened</c> as well, but it represents no pinnable sub-menu and must not override the pinned path.
    /// </summary>
    public void ExtendToOpenedItem(NavMenu menu, NavMenuItem openedItem)
    {
        if (!openedItem.HasSubMenu ||
            ((INavMenuItem)openedItem).Node is not { } node ||
            !IsNodeInMenu(menu, node))
        {
            return;
        }

        _pinnedPath.Clear();
        _pinnedPath.AddRange(BuildPathToNode(node));

        // Pinning opens the sub-menu, and opening synchronously raises SubmenuOpened, which comes back here.
        // While applying, only the request is updated and the outer apply loop finishes the delivery with the latest
        // path, so a recursive re-entry cannot corrupt the container set being applied.
        if (!_isApplying)
        {
            ApplyToRealizedContainers(menu, releaseObsolete: true);
        }
    }

    /// <summary>
    /// Drops the pinned records of a container that is recycled or removed, together with those of its descendants.
    /// <c>CloseForLifecycle</c> is called only on a container that was pinned and whose node has left the menu entry
    /// graph, that is when it was removed for good and not recycled for reuse.
    /// The request itself is stored by node in <c>_pinnedPath</c> and survives the recycling: the container
    /// properties are cleared by <c>ClearNodeContainerBindings</c> on recycle, and <see cref="Reconcile" /> restores
    /// them from the path once the entry graph has settled.
    /// </summary>
    public void ForgetContainer(NavMenu menu, NavMenuItem menuItem)
    {
        var wasPinned = false;
        for (var i = _pinnedItems.Count - 1; i >= 0; i--)
        {
            if (IsSameOrDescendant(_pinnedItems[i], menuItem))
            {
                _pinnedItems.RemoveAt(i);
                wasPinned = true;
            }
        }

        if (wasPinned &&
            (((INavMenuItem)menuItem).Node is not { } node || !IsNodeInMenu(menu, node)))
        {
            menuItem.CloseForLifecycle();
        }
    }

    /// <summary>
    /// Clears the pinned state of every tracked container and empties the request path, but does not close the
    /// sub-menus that are already open. A reconcile queued by <see cref="QueueReconcile" /> is discarded.
    /// Call it when pinning is canceled, when the menu switches to Inline and on detach: resetting
    /// <c>IsPopupPinnedOpen</c> to false only removes the block that keeps a sub-menu from closing, so an open
    /// sub-menu stays open until the caller decides by lifecycle whether to close it.
    /// </summary>
    public void Release()
    {
        _reconcileGeneration++;
        foreach (var item in _pinnedItems)
        {
            item.IsPopupPinnedOpen = false;
        }

        _pinnedItems.Clear();
        _pinnedPath.Clear();
    }

    private void ApplyToRealizedContainers(NavMenu menu, bool releaseObsolete = false)
    {
        if (_isApplying)
        {
            return;
        }

        _isApplying = true;
        try
        {
            var desired = new List<NavMenuItem>(_pinnedPath.Count);
            foreach (var node in _pinnedPath)
            {
                if (IsNodeInMenu(menu, node) &&
                    menu.FindRealizedMenuItem(node) is { } item &&
                    !desired.Contains(item))
                {
                    desired.Add(item);
                }
            }

            if (releaseObsolete)
            {
                for (var i = _pinnedItems.Count - 1; i >= 0; i--)
                {
                    var item = _pinnedItems[i];
                    if (!desired.Contains(item))
                    {
                        item.IsPopupPinnedOpen = false;
                        _pinnedItems.RemoveAt(i);
                    }
                }
            }

            foreach (var item in desired)
            {
                ApplyPinnedState(item);
            }
        }
        finally
        {
            _isApplying = false;
        }
    }

    private void ApplyPinnedState(NavMenuItem menuItem)
    {
        if (!menuItem.IsPopupPinnedOpen)
        {
            menuItem.IsPopupPinnedOpen = true;
        }

        if (!_pinnedItems.Contains(menuItem))
        {
            _pinnedItems.Add(menuItem);
        }
    }

    private void PruneStalePath(NavMenu menu)
    {
        for (var i = _pinnedPath.Count - 1; i >= 0; i--)
        {
            if (!IsNodeInMenu(menu, _pinnedPath[i]))
            {
                _pinnedPath.RemoveAt(i);
            }
        }
    }

    private static List<INavMenuNode> ResolvePath(NavMenu menu)
    {
        // The open chain wins: when a sub-menu is already open at the time of pinning, the intent is to pin the
        // currently open chain, not to force the first item.
        var openPath = ResolveOpenPath(menu);
        if (openPath.Count > 0)
        {
            return openPath;
        }

        foreach (var node in EnumerateNodes(menu.Items))
        {
            if (node.Children.Any())
            {
                return BuildPathToNode(node);
            }
        }

        return [];
    }

    private static List<INavMenuNode> ResolveOpenPath(NavMenu menu)
    {
        foreach (var topLevelItem in NavMenuSemanticNavigator.EnumerateDirectItems(menu))
        {
            if (((INavMenuItem)topLevelItem).Node is not { } node || !IsNodeInMenu(menu, node))
            {
                continue;
            }

            if (FindDeepestOpenSubmenu(topLevelItem) is { } openItem &&
                ((INavMenuItem)openItem).Node is { } openNode)
            {
                return BuildPathToNode(openNode);
            }
        }

        return [];
    }

    private static NavMenuItem? FindDeepestOpenSubmenu(NavMenuItem menuItem)
    {
        if (!menuItem.HasSubMenu || !menuItem.IsSubMenuOpen)
        {
            return null;
        }

        foreach (var child in NavMenuSemanticNavigator.EnumerateDirectItems(menuItem))
        {
            if (FindDeepestOpenSubmenu(child) is { } deeper)
            {
                return deeper;
            }
        }

        return menuItem;
    }

    private static List<INavMenuNode> BuildPathToNode(INavMenuNode node)
    {
        var path = new List<INavMenuNode>();
        for (var current = node; current is not null; current = current.ParentNode as INavMenuNode)
        {
            path.Add(current);
        }

        path.Reverse();
        return path;
    }

    private static bool IsNodeInMenu(NavMenu menu, INavMenuNode node)
    {
        var root = node;
        while (root.ParentNode is INavMenuNode parent)
        {
            root = parent;
        }

        return NavMenuEntryGraph.ContainsDirectNode(menu.Items, root);
    }

    private static IEnumerable<INavMenuNode> EnumerateNodes(IEnumerable entries)
    {
        foreach (var entry in entries)
        {
            switch (entry)
            {
                case INavMenuNode node:
                    yield return node;
                    break;

                case NavMenuGroup group:
                    foreach (var nested in EnumerateNodes(group.Entries))
                    {
                        yield return nested;
                    }

                    break;
            }
        }
    }

    private static bool IsSameOrDescendant(NavMenuItem candidate, NavMenuItem ancestor)
    {
        for (var current = candidate; current is not null; current = current.SemanticParentItem)
        {
            if (ReferenceEquals(current, ancestor))
            {
                return true;
            }
        }

        return false;
    }
}
