using Avalonia.Controls;
using Avalonia.Styling;

namespace AtomUI.Theme.Resources;

/// <summary>
/// 静态资源固定层：Prepare 处理新资源的 owner，Commit 只交换查询表，通知由主题 Publish 统一发出。
/// </summary>
internal sealed class StaticThemeResourceProvider : IResourceProvider
{
    private readonly ThemeResourceOwnerBridge _ownerBridge;
    private readonly HashSet<IResourceProvider> _attached = new(ReferenceEqualityComparer.Instance);
    private IResourceHost? _owner;
    private ResourceTable _table = new([]);
    private ResourceTable? _stagedTable;
    private readonly WeakReference<ThemeManager>? _themeManager;

    internal StaticThemeResourceProvider(ThemeManager? themeManager = null)
    {
        _ownerBridge = new ThemeResourceOwnerBridge();
        _themeManager = themeManager is null ? null : new WeakReference<ThemeManager>(themeManager);
    }

    internal int MaterializeAssets()
    {
        var resources = _table.Resources.OfType<DeferredControlThemeResourceProvider>().Distinct().ToArray();
        foreach (var resource in resources)
        {
            resource.EnsureLoaded();
        }
        return resources.Length;
    }

    internal void RunUpdate(Action update) => _ownerBridge.RunUpdate(update);

    internal ResourceTable CurrentTable => _table;
    public IResourceHost? Owner => _owner;
    public event EventHandler? OwnerChanged;
    internal void StageTable(ResourceTable? table) => _stagedTable = table;
    private ThemeManager.PackageUpdate? ReadUpdate => _stagedTable is not null &&
        _themeManager?.TryGetTarget(out var manager) == true ? manager.PackageReadUpdate : null;
    private ResourceTable ActiveTable => ReadUpdate?.IsCandidateRead == true ? _stagedTable! : _table;
    public bool HasResources => ActiveTable.Resources.Length != 0;

    public bool TryGetResource(object key, ThemeVariant? theme, out object? value)
    {
        var found = ActiveTable.TryGetResource(key, theme, out value, ReadUpdate);
        if (found && value is ControlTheme controlTheme && _themeManager?.TryGetTarget(out var manager) == true)
        {
            manager.NotifyControlThemeResolved(key, controlTheme);
        }
        return found;
    }

    internal ResourceTable PrepareTable(IEnumerable<IResourceProvider> resources)
    {
        var table = new ResourceTable(resources.ToArray());
        try
        {
            _ownerBridge.RunUpdate(() =>
            {
                foreach (var resource in table.Resources)
                {
                    // AddOwner 可以先赋 Owner 再在回调中抛错；尝试挂载者也必须参加回滚。
                    if (_attached.Add(resource))
                    {
                        resource.AddOwner(_ownerBridge);
                    }
                }
            });
        }
        catch (Exception exception)
        {
            var failures = new List<Exception> { exception };
            ThemeResourceOwnerBridge.TryCleanup(ReleaseUnusedResources, failures);
            ThemeResourceOwnerBridge.ThrowFailures(failures);
        }
        return table;
    }

    internal void CommitTable(ResourceTable table) => _table = table;

    internal void ReleaseUnusedResources()
    {
        var active = new HashSet<IResourceProvider>(_table.Resources, ReferenceEqualityComparer.Instance);
        ReleaseResources(_attached.Where(resource => !active.Contains(resource)).ToArray());
    }

    private void ReleaseResources(IResourceProvider[] resources)
    {
        _ownerBridge.RunUpdate(() =>
        {
            var failures = new List<Exception>();
            foreach (var resource in resources)
            {
                try
                {
                    ThemeResourceOwnerBridge.TryCleanup(() => resource.RemoveOwner(_ownerBridge), failures);
                }
                finally
                {
                    _attached.Remove(resource);
                }
            }
            ThemeResourceOwnerBridge.ThrowFailures(failures);
        });
    }

    void IResourceProvider.AddOwner(IResourceHost owner)
    {
        ArgumentNullException.ThrowIfNull(owner);
        if (_owner is not null)
        {
            throw new InvalidOperationException("The resource provider already has a parent.");
        }
        _owner = owner;
        try
        {
            OwnerChanged?.Invoke(this, EventArgs.Empty);
            _ownerBridge.RunUpdate(() =>
            {
                _ownerBridge.SetOwner(owner);
                PrepareTable(_table.Resources);
            });
            if (HasResources)
            {
                owner.NotifyHostedResourcesChanged(ResourcesChangedEventArgs.Create());
            }
        }
        catch (Exception exception)
        {
            var failures = new List<Exception> { exception };
            _owner = null;
            ThemeResourceOwnerBridge.TryCleanup(DetachResources, failures);
            ThemeResourceOwnerBridge.ThrowFailures(failures);
        }
    }

    void IResourceProvider.RemoveOwner(IResourceHost owner)
    {
        ArgumentNullException.ThrowIfNull(owner);
        if (!ReferenceEquals(_owner, owner))
        {
            return;
        }
        _owner = null;
        var failures = new List<Exception>();
        ThemeResourceOwnerBridge.TryCleanup(() => OwnerChanged?.Invoke(this, EventArgs.Empty), failures);
        ThemeResourceOwnerBridge.TryCleanup(DetachResources, failures);
        if (HasResources)
        {
            ThemeResourceOwnerBridge.TryCleanup(
                () => owner.NotifyHostedResourcesChanged(ResourcesChangedEventArgs.Create()), failures);
        }
        ThemeResourceOwnerBridge.ThrowFailures(failures);
    }

    private void DetachResources()
    {
        _ownerBridge.RunUpdate(() =>
        {
            var failures = new List<Exception>();
            try
            {
                ThemeResourceOwnerBridge.TryCleanup(() => ReleaseResources(_attached.ToArray()), failures);
            }
            finally
            {
                ThemeResourceOwnerBridge.TryCleanup(() => _ownerBridge.SetOwner(null), failures);
            }
            ThemeResourceOwnerBridge.ThrowFailures(failures);
        });
    }

    internal sealed class ResourceTable
    {
        private readonly Dictionary<object, List<int>> _candidates = new();
        private readonly List<int> _unknown = [];

        internal ResourceTable(IResourceProvider[] resources)
        {
            Resources = resources;
            for (var index = resources.Length - 1; index >= 0; index--)
            {
                if (resources[index] is not DeferredControlThemeResourceProvider { ResourceKeys: { } keys })
                {
                    _unknown.Add(index);
                    continue;
                }
                foreach (var key in keys)
                {
                    if (!_candidates.TryGetValue(key, out var matches))
                    {
                        matches = [];
                        _candidates.Add(key, matches);
                    }
                    matches.Add(index);
                }
            }
        }

        internal IResourceProvider[] Resources { get; }

        internal bool TryGetResource(object key, ThemeVariant? theme, out object? value, ThemeManager.PackageUpdate? update = null)
        {
            _candidates.TryGetValue(key, out var candidates);
            var knownIndex = 0;
            var unknownIndex = 0;
            while (knownIndex < (candidates?.Count ?? 0) || unknownIndex < _unknown.Count)
            {
                var known = candidates is not null && knownIndex < candidates.Count ? candidates[knownIndex] : -1;
                var unknown = unknownIndex < _unknown.Count ? _unknown[unknownIndex] : -1;
                // 已知候选和开放资产按同一个全局倒序归并，不能越过未知高优先级覆盖。
                var index = known > unknown ? candidates![knownIndex++] : _unknown[unknownIndex++];
                var resource = Resources[index];
                bool found;
                if (update is { IsCandidateRead: true } && update.OwnsExistingResource(resource))
                {
                    found = TryGetCommittedResource(update, resource, key, theme, out value);
                }
                else
                {
                    found = resource.TryGetResource(key, theme, out value);
                }
                if (found)
                {
                    return true;
                }
            }
            value = null;
            return false;
        }

        private static bool TryGetCommittedResource(ThemeManager.PackageUpdate update, IResourceProvider resource,
            object key, ThemeVariant? theme, out object? value)
        {
            // The transaction callback must stay outside the ordinary candidate loop so its
            // captured parameters do not allocate on every nontransaction hit or miss.
            object? captured = null;
            var found = false;
            update.RunCommittedRead(() => found = resource.TryGetResource(key, theme, out captured));
            value = captured;
            return found;
        }
    }
}
