using AtomUI.Controls;
using AtomUI.Controls.Primitives;
using AtomUI.Desktop.Controls;
using AtomUI.Toolkits.GalleryBase.Localization;
using Avalonia.Controls;

namespace AtomUI.Toolkits.GalleryBase.Navigation;

/// <summary>
/// Converts configured Gallery navigation nodes into the node model and the initially open paths of
/// the showcase navigation menu.
/// </summary>
public sealed class GalleryNavigationMenuAdapter
{
    /// <summary>
    /// Builds the menu nodes for the given Gallery nodes, keeping their order and hierarchy. Localized
    /// headers are resolved to text and icon factories are invoked.
    /// </summary>
    /// <param name="nodes">
    /// The Gallery navigation nodes to convert.
    /// </param>
    /// <returns>
    /// One menu node per input node, each carrying its converted children.
    /// </returns>
    public IReadOnlyList<NavMenuNode> BuildNodes(IEnumerable<GalleryNavigationNode> nodes)
    {
        return nodes.Select(BuildNode).ToArray();
    }

    /// <summary>
    /// Builds the paths of the menu nodes that are open initially, one path per key created from the
    /// key's value.
    /// </summary>
    /// <param name="defaultOpenKeys">
    /// The keys of the nodes that are open initially.
    /// </param>
    /// <returns>
    /// The open paths, in the order of the keys.
    /// </returns>
    public IList<TreeNodePath> BuildDefaultOpenPaths(IEnumerable<EntityKey> defaultOpenKeys)
    {
        return defaultOpenKeys.Select(key => new TreeNodePath(key.Value)).ToArray();
    }

    private static NavMenuNode BuildNode(GalleryNavigationNode node)
    {
        var navMenuNode = new NavMenuNode
        {
            Header  = ResolveHeader(node.Header),
            ItemKey = node.Key,
            Icon    = ResolveIcon(node.Icon)
        };

        foreach (var child in node.Children)
        {
            navMenuNode.Children.Add(BuildNode(child));
        }

        return navMenuNode;
    }

    private static object ResolveHeader(object header)
    {
        return header is IGalleryLocalizedText localizedText
            ? localizedText.Resolve()
            : header;
    }

    private static PathIcon? ResolveIcon(object? icon)
    {
        return icon switch
        {
            null              => null,
            PathIcon pathIcon => pathIcon,
            Func<PathIcon> factory => factory(),
            _                 => null
        };
    }
}
