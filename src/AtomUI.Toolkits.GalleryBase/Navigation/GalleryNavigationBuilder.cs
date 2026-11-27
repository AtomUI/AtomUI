using AtomUI.Controls;
using AtomUI.Toolkits.GalleryBase.Configuration;

namespace AtomUI.Toolkits.GalleryBase.Navigation;

/// <summary>
/// Builds the Gallery navigation tree for the Gallery options. Pages are added with
/// <see cref="AddPage" />; <see cref="AddGroup" /> adds a group and returns a builder for its children.
/// Keys must be unique across the whole tree, groups included.
/// </summary>
public sealed class GalleryNavigationBuilder
{
    private readonly GalleryNavigationBuilderState _state = new();
    private readonly List<GalleryNavigationNodeBuilder> _nodes = new();

    /// <summary>
    /// The key of the page that the workspace opens when its navigation is activated. Creating the Gallery
    /// configuration fails with <see cref="GalleryConfigurationException" /> unless the key names a page
    /// that has a registered route.
    /// </summary>
    public EntityKey DefaultRoute { get; set; }

    /// <summary>
    /// The keys of the groups that the navigation menu shows expanded at startup. Creating the Gallery
    /// configuration fails with <see cref="GalleryConfigurationException" /> for an empty or repeated key, or
    /// for a key that does not name a group.
    /// </summary>
    public IList<EntityKey> DefaultOpenKeys { get; } = new List<EntityKey>();

    /// <summary>
    /// Adds a page to the top level of the navigation tree.
    /// </summary>
    /// <param name="key">
    /// The key of the page, which is also its route key. The route must be registered, or creating the
    /// Gallery configuration fails.
    /// </param>
    /// <param name="header">
    /// The label of the page in the navigation menu. A value that implements <c>IGalleryLocalizedText</c>
    /// is resolved to its text when the menu nodes are built; any other value is used as is. Must not be
    /// <see langword="null" />.
    /// </param>
    /// <param name="icon">
    /// The icon shown before the label in the navigation menu: a <c>PathIcon</c> or a
    /// <c>Func&lt;PathIcon&gt;</c> factory. Pass <see langword="null" /> to show none.
    /// </param>
    /// <returns>
    /// This builder, to continue adding pages and groups.
    /// </returns>
    /// <exception cref="GalleryConfigurationException">
    /// Thrown when the key is empty or already used by another node of the tree, or when the header is
    /// <see langword="null" />.
    /// </exception>
    public GalleryNavigationBuilder AddPage(EntityKey key, object header, object? icon = null)
    {
        AddNode(_nodes, key, header, icon, isRoute: true);
        return this;
    }

    /// <summary>
    /// Adds a group to the top level of the navigation tree.
    /// </summary>
    /// <param name="key">
    /// The key of the group, unique across the whole navigation tree. It can be listed in
    /// <see cref="DefaultOpenKeys" /> to open the group at startup.
    /// </param>
    /// <param name="header">
    /// The label of the group in the navigation menu. A value that implements <c>IGalleryLocalizedText</c>
    /// is resolved to its text when the menu nodes are built; any other value is used as is. Must not be
    /// <see langword="null" />.
    /// </param>
    /// <param name="icon">
    /// The icon shown before the label in the navigation menu: a <c>PathIcon</c> or a
    /// <c>Func&lt;PathIcon&gt;</c> factory. Pass <see langword="null" /> to show none.
    /// </param>
    /// <returns>
    /// A builder that adds the children of the group.
    /// </returns>
    /// <exception cref="GalleryConfigurationException">
    /// Thrown when the key is empty or already used by another node of the tree, or when the header is
    /// <see langword="null" />.
    /// </exception>
    public GalleryNavigationGroupBuilder AddGroup(EntityKey key, object header, object? icon = null)
    {
        var node = AddNode(_nodes, key, header, icon, isRoute: false);
        return new GalleryNavigationGroupBuilder(_state, node.Children);
    }

    internal IReadOnlyList<GalleryNavigationNode> BuildNodes()
    {
        return Array.AsReadOnly(_nodes.Select(BuildNode).ToArray());
    }

    private GalleryNavigationNodeBuilder AddNode(IList<GalleryNavigationNodeBuilder> nodes,
                                                EntityKey key,
                                                object header,
                                                object? icon,
                                                bool isRoute)
    {
        return _state.AddNode(nodes, key, header, icon, isRoute);
    }

    private static GalleryNavigationNode BuildNode(GalleryNavigationNodeBuilder node)
    {
        return new GalleryNavigationNode(
            node.Key,
            node.Header,
            node.Icon,
            node.IsRoute,
            Array.AsReadOnly(node.Children.Select(BuildNode).ToArray()));
    }
}

/// <summary>
/// Adds pages and nested groups to one group of the Gallery navigation tree. Instances come from
/// <see cref="GalleryNavigationBuilder.AddGroup" /> and from <see cref="AddGroup" /> of this class.
/// </summary>
public sealed class GalleryNavigationGroupBuilder
{
    private readonly GalleryNavigationBuilderState      _state;
    private readonly IList<GalleryNavigationNodeBuilder> _nodes;

    internal GalleryNavigationGroupBuilder(GalleryNavigationBuilderState state,
                                           IList<GalleryNavigationNodeBuilder> nodes)
    {
        _state = state;
        _nodes = nodes;
    }

    /// <summary>
    /// Adds a page to the group.
    /// </summary>
    /// <param name="key">
    /// The key of the page, which is also its route key. The route must be registered, or creating the
    /// Gallery configuration fails.
    /// </param>
    /// <param name="header">
    /// The label of the page in the navigation menu. A value that implements <c>IGalleryLocalizedText</c>
    /// is resolved to its text when the menu nodes are built; any other value is used as is. Must not be
    /// <see langword="null" />.
    /// </param>
    /// <param name="icon">
    /// The icon shown before the label in the navigation menu: a <c>PathIcon</c> or a
    /// <c>Func&lt;PathIcon&gt;</c> factory. Pass <see langword="null" /> to show none.
    /// </param>
    /// <returns>
    /// This builder, to continue adding pages and groups to the group.
    /// </returns>
    /// <exception cref="GalleryConfigurationException">
    /// Thrown when the key is empty or already used by another node of the tree, or when the header is
    /// <see langword="null" />.
    /// </exception>
    public GalleryNavigationGroupBuilder AddPage(EntityKey key, object header, object? icon = null)
    {
        _state.AddNode(_nodes, key, header, icon, isRoute: true);
        return this;
    }

    /// <summary>
    /// Adds a nested group to the group.
    /// </summary>
    /// <param name="key">
    /// The key of the nested group, unique across the whole navigation tree. It can be listed in
    /// <see cref="GalleryNavigationBuilder.DefaultOpenKeys" /> to open the group at startup.
    /// </param>
    /// <param name="header">
    /// The label of the group in the navigation menu. A value that implements <c>IGalleryLocalizedText</c>
    /// is resolved to its text when the menu nodes are built; any other value is used as is. Must not be
    /// <see langword="null" />.
    /// </param>
    /// <param name="icon">
    /// The icon shown before the label in the navigation menu: a <c>PathIcon</c> or a
    /// <c>Func&lt;PathIcon&gt;</c> factory. Pass <see langword="null" /> to show none.
    /// </param>
    /// <returns>
    /// A builder that adds the children of the nested group.
    /// </returns>
    /// <exception cref="GalleryConfigurationException">
    /// Thrown when the key is empty or already used by another node of the tree, or when the header is
    /// <see langword="null" />.
    /// </exception>
    public GalleryNavigationGroupBuilder AddGroup(EntityKey key, object header, object? icon = null)
    {
        var node = _state.AddNode(_nodes, key, header, icon, isRoute: false);
        return new GalleryNavigationGroupBuilder(_state, node.Children);
    }
}

internal sealed class GalleryNavigationBuilderState
{
    private readonly HashSet<EntityKey> _keys = new();

    public GalleryNavigationNodeBuilder AddNode(IList<GalleryNavigationNodeBuilder> nodes,
                                                EntityKey key,
                                                object header,
                                                object? icon,
                                                bool isRoute)
    {
        if (string.IsNullOrWhiteSpace(key.Value))
        {
            throw new GalleryConfigurationException("Gallery navigation node Key must not be empty.");
        }

        if (!_keys.Add(key))
        {
            throw new GalleryConfigurationException(
                $"Gallery navigation node key '{key}' is already registered.");
        }

        if (header is null)
        {
            throw new GalleryConfigurationException(
                $"Gallery navigation node '{key}' Header must not be null.");
        }

        var node = new GalleryNavigationNodeBuilder(key, header, icon, isRoute);
        nodes.Add(node);
        return node;
    }
}

internal sealed class GalleryNavigationNodeBuilder
{
    public EntityKey Key { get; }

    public object Header { get; }

    public object? Icon { get; }

    public bool IsRoute { get; }

    public IList<GalleryNavigationNodeBuilder> Children { get; } = new List<GalleryNavigationNodeBuilder>();

    public GalleryNavigationNodeBuilder(EntityKey key, object header, object? icon, bool isRoute)
    {
        Key     = key;
        Header  = header;
        Icon    = icon;
        IsRoute = isRoute;
    }
}
