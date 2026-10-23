using System.Diagnostics;
using AtomUI.Theme.Configuration;

namespace AtomUI.Theme;

internal sealed class ThemeScopeGraph
{
    private static readonly ThemeConfig s_fallbackConfig = new ThemeConfigBuilder().Build();
    private readonly ThemeManager _manager;
    private readonly Dictionary<long, ThemeScopeNode> _nodes = new();
    private readonly Dictionary<ThemeConfigProvider, long> _providerRegistrations =
        new(ReferenceEqualityComparer.Instance);
    private long _nextRegistrationId;

    internal ThemeScopeGraph(ThemeManager manager)
    {
        _manager = manager ?? throw new ArgumentNullException(nameof(manager));
    }

    internal long TopologyRevision { get; private set; }

    internal ThemeScopeRegistration Register(
        ThemeConfigProvider provider,
        ThemeContext parentContext,
        ThemeConfig config)
    {
        ArgumentNullException.ThrowIfNull(provider);
        ArgumentNullException.ThrowIfNull(parentContext);
        ArgumentNullException.ThrowIfNull(config);
        EnsureOwnedContext(parentContext);
        if (_providerRegistrations.ContainsKey(provider))
        {
            throw new InvalidOperationException("The ThemeConfigProvider is already registered.");
        }

        var parentRegistrationId = parentContext.RegistrationId;
        if (parentRegistrationId != 0 && !_nodes.ContainsKey(parentRegistrationId))
        {
            throw new InvalidOperationException("The parent ThemeContext is not active in this scope graph.");
        }

        var registrationId = ++_nextRegistrationId;
        var context = new ThemeContext(_manager, parentContext.Snapshot, registrationId);
        context.CommitStyleInputs(parentContext.StyleInputs);
        var node = new ThemeScopeNode(
            registrationId,
            parentRegistrationId,
            provider,
            context,
            config,
            s_fallbackConfig);
        _nodes.Add(registrationId, node);
        _providerRegistrations.Add(provider, registrationId);
        if (parentRegistrationId != 0)
        {
            _nodes[parentRegistrationId].Children.Add(registrationId);
        }

        TopologyRevision++;
        return new ThemeScopeRegistration(this, registrationId, context);
    }

    internal void ReplaceConfig(long registrationId, ThemeConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);
        var node = GetNode(registrationId);
        if (ReferenceEquals(node.Config, config))
        {
            return;
        }

        node.Config = config;
        node.ConfigRevision++;
    }

    internal void AcceptConfig(long registrationId)
    {
        var node = GetNode(registrationId);
        node.LastValidConfig = node.Config;
    }

    internal void Reparent(long registrationId, ThemeContext parentContext)
    {
        ArgumentNullException.ThrowIfNull(parentContext);
        EnsureOwnedContext(parentContext);
        var node = GetNode(registrationId);
        var nextParentId = parentContext.RegistrationId;
        if (node.ParentRegistrationId == nextParentId)
        {
            return;
        }
        if (nextParentId != 0)
        {
            var nextParent = GetNode(nextParentId);
            if (IsDescendant(nextParent.RegistrationId, registrationId))
            {
                throw new InvalidOperationException("A theme scope cannot be reparented below its own subtree.");
            }
        }

        if (node.ParentRegistrationId != 0 &&
            _nodes.TryGetValue(node.ParentRegistrationId, out var previousParent))
        {
            previousParent.Children.Remove(registrationId);
        }
        if (nextParentId != 0)
        {
            _nodes[nextParentId].Children.Add(registrationId);
        }

        node.ParentRegistrationId = nextParentId;
        TopologyRevision++;
    }

    internal ThemeScopeCapture CaptureAll()
    {
        var roots = _nodes.Values
                          .Where(static node => node.ParentRegistrationId == 0)
                          .Select(static node => node.RegistrationId)
                          .Order()
                          .ToArray();
        var captures = new List<ThemeScopeNodeCapture>(_nodes.Count);
        foreach (var registrationId in roots)
        {
            Capture(registrationId, captures);
        }
        return new ThemeScopeCapture(TopologyRevision, 0, captures.AsReadOnly());
    }

    internal ThemeScopeCapture CaptureSubtree(long registrationId)
    {
        GetNode(registrationId);
        var captures = new List<ThemeScopeNodeCapture>();
        Capture(registrationId, captures);
        return new ThemeScopeCapture(TopologyRevision, registrationId, captures.AsReadOnly());
    }

    internal bool IsCurrent(ThemeScopeCapture capture)
    {
        ArgumentNullException.ThrowIfNull(capture);
        if (capture.TopologyRevision != TopologyRevision)
        {
            return false;
        }

        foreach (var item in capture.Nodes)
        {
            if (!_nodes.TryGetValue(item.Stamp.RegistrationId, out var node) ||
                node.Stamp != item.Stamp)
            {
                return false;
            }
        }
        return true;
    }

    internal bool TryGetNode(long registrationId, out ThemeScopeNode? node)
    {
        return _nodes.TryGetValue(registrationId, out node);
    }

    internal bool TryGetNode(ThemeConfigProvider provider, out ThemeScopeNode? node)
    {
        if (_providerRegistrations.TryGetValue(provider, out var registrationId))
        {
            return _nodes.TryGetValue(registrationId, out node);
        }
        node = null;
        return false;
    }

    internal void Unregister(long registrationId)
    {
        if (!_nodes.TryGetValue(registrationId, out var node))
        {
            return;
        }

        var children = node.Children.ToArray();
        foreach (var child in children)
        {
            Unregister(child);
        }
        if (node.ParentRegistrationId != 0 &&
            _nodes.TryGetValue(node.ParentRegistrationId, out var parent))
        {
            parent.Children.Remove(registrationId);
        }
        _providerRegistrations.Remove(node.Provider);
        _nodes.Remove(registrationId);
        TopologyRevision++;
    }

    internal void DisposeAll()
    {
        var ordered = new List<ThemeScopeNode>(_nodes.Count);
        foreach (var root in _nodes.Values
                                   .Where(static node => node.ParentRegistrationId == 0)
                                   .OrderBy(static node => node.RegistrationId))
        {
            CaptureForDisposal(root.RegistrationId, ordered);
        }

        try
        {
            foreach (var node in ordered)
            {
                try
                {
                    node.Provider.ReleaseManagerRegistration(node.Context);
                }
                catch (Exception exception)
                {
                    Debug.WriteLine(exception);
                }
            }
        }
        finally
        {
            _nodes.Clear();
            _providerRegistrations.Clear();
            TopologyRevision++;
        }
    }

    private ThemeScopeNode GetNode(long registrationId)
    {
        return _nodes.TryGetValue(registrationId, out var node)
            ? node
            : throw new InvalidOperationException($"Theme scope registration '{registrationId}' is not active.");
    }

    private void Capture(long registrationId, List<ThemeScopeNodeCapture> captures)
    {
        var node = _nodes[registrationId];
        captures.Add(new ThemeScopeNodeCapture(
            node.Stamp,
            node.Context,
            node.Config,
            node.LastValidConfig));
        foreach (var child in node.Children)
        {
            Capture(child, captures);
        }
    }

    private void CaptureForDisposal(long registrationId, List<ThemeScopeNode> ordered)
    {
        var node = _nodes[registrationId];
        foreach (var child in node.Children)
        {
            CaptureForDisposal(child, ordered);
        }
        ordered.Add(node);
    }

    private bool IsDescendant(long candidateId, long ancestorId)
    {
        var current = candidateId;
        while (current != 0)
        {
            if (current == ancestorId)
            {
                return true;
            }
            current = _nodes[current].ParentRegistrationId;
        }
        return false;
    }

    private void EnsureOwnedContext(ThemeContext context)
    {
        if (!ReferenceEquals(context.Manager, _manager))
        {
            throw new InvalidOperationException("ThemeContext belongs to another ThemeManager.");
        }
    }
}

internal sealed class ThemeScopeRegistration : IDisposable
{
    private ThemeScopeGraph? _graph;

    internal ThemeScopeRegistration(
        ThemeScopeGraph graph,
        long registrationId,
        ThemeContext context)
    {
        _graph         = graph;
        RegistrationId = registrationId;
        Context        = context;
    }

    internal long RegistrationId { get; }
    internal ThemeContext Context { get; }

    public void Dispose()
    {
        Interlocked.Exchange(ref _graph, null)?.Unregister(RegistrationId);
    }
}
