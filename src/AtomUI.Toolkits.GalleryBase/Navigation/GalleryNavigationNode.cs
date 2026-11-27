using AtomUI.Controls;

namespace AtomUI.Toolkits.GalleryBase.Navigation;

/// <summary>
/// One node of the Gallery navigation tree: a page that has a route, or a group that only contains pages
/// and nested groups. Instances are created by <see cref="GalleryNavigationBuilder" />.
/// </summary>
public sealed class GalleryNavigationNode
{
    /// <summary>
    /// The key of the node, unique across the whole navigation tree. For a page it is the route key that
    /// the route registry must resolve; for a group it is the key that
    /// <see cref="GalleryNavigationBuilder.DefaultOpenKeys" /> accepts.
    /// </summary>
    public EntityKey Key { get; }

    /// <summary>
    /// The label of the node in the navigation menu. A value that implements <c>IGalleryLocalizedText</c>
    /// is resolved to its text when the menu nodes are built; any other value is used as is. Never
    /// <see langword="null" />: the builder rejects a null header.
    /// </summary>
    public object Header { get; }

    /// <summary>
    /// The icon that the navigation menu shows next to the header: a <c>PathIcon</c>, or a
    /// <c>Func&lt;PathIcon&gt;</c> factory that is invoked when the menu nodes are built. Any other value,
    /// including <see langword="null" />, shows no icon.
    /// </summary>
    public object? Icon { get; }

    /// <summary>
    /// <see langword="true" /> when the node is a page that the host screen can navigate to by its
    /// <see cref="Key" />; <see langword="false" /> when it is a group, which only holds children and is
    /// never a navigation target.
    /// </summary>
    public bool IsRoute { get; }

    /// <summary>
    /// The nodes nested below this one, in the order in which they were added. Empty for a page, and for a
    /// group that has no children.
    /// </summary>
    public IReadOnlyList<GalleryNavigationNode> Children { get; }

    internal GalleryNavigationNode(EntityKey key,
                                   object header,
                                   object? icon,
                                   bool isRoute,
                                   IReadOnlyList<GalleryNavigationNode> children)
    {
        Key      = key;
        Header   = header;
        Icon     = icon;
        IsRoute  = isRoute;
        Children = children;
    }
}
