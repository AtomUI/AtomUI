using AtomUI.Registration;
using Avalonia.Controls;
using Avalonia.Styling;

namespace AtomUI.Theme.Resources;

/// <summary>Application-owned resource layer. Manifest lookup does not create package providers.</summary>
internal sealed class StartupResourceProvider : IResourceProvider
{
    private ResourceIndex _resources;
    private ResourceIndex? _stagedIndex;
    private readonly WeakReference<ThemeManager>? _themeManager;
    private readonly Dictionary<StartupAssetEntry, DeferredControlThemeResourceProvider> _materialized = [];
    private readonly ThemeResourceOwnerBridge _bridge = new();
    private IResourceHost? _owner;

    internal StartupResourceProvider(StartupPackageIndex index, ThemeManager? themeManager = null)
    {
        _resources = new ResourceIndex(index);
        _themeManager = themeManager is null ? null : new WeakReference<ThemeManager>(themeManager);
    }

    internal void RunUpdate(Action update) => _bridge.RunUpdate(update);

    internal ResourceIndex CurrentIndex => _resources;
    internal static ResourceIndex PrepareIndex(StartupPackageIndex index) => new(index);
    internal void CommitIndex(ResourceIndex index) => _resources = index;
    internal void StageIndex(ResourceIndex? index) => _stagedIndex = index;

    private ResourceIndex ActiveIndex => _stagedIndex is { } staged &&
        _themeManager?.TryGetTarget(out var manager) == true && manager.PackageReadUpdate?.IsCandidateRead == true
            ? staged : _resources;

    internal void ReleaseUnusedResources()
    {
        var active = _resources.Index.Assets.ToHashSet();
        var failures = new List<Exception>();
        foreach (var pair in _materialized.Where(pair => !active.Contains(pair.Key)).ToArray())
        {
            _materialized.Remove(pair.Key);
            ThemeResourceOwnerBridge.TryCleanup(() => ((IResourceProvider)pair.Value).RemoveOwner(_bridge), failures);
        }
        ThemeResourceOwnerBridge.ThrowFailures(failures);
    }

    public IResourceHost? Owner => _owner;
    public event EventHandler? OwnerChanged;
    public bool HasResources => ActiveIndex.Index.Assets.Count != 0;

    public bool TryGetResource(object key, ThemeVariant? theme, out object? value)
    {
        var resources = ActiveIndex;
        List<int>? known = null;
        if (StartupPackageIndex.ResourceKey(key) is { } identity)
        {
            resources.Known.TryGetValue(identity, out known);
        }
        var knownIndex = 0;
        var unknownIndex = 0;
        while (knownIndex < (known?.Count ?? 0) || unknownIndex < resources.Unknown.Count)
        {
            var knownOrdinal = known is not null && knownIndex < known.Count ? known[knownIndex] : -1;
            var unknownOrdinal = unknownIndex < resources.Unknown.Count ? resources.Unknown[unknownIndex] : -1;
            var ordinal = knownOrdinal > unknownOrdinal ? known![knownIndex++] : resources.Unknown[unknownIndex++];
            var asset = resources.Index.Assets[ordinal];
            if (TryGetAssetResource(asset, key, theme, out value))
            {
                if (value is ControlTheme controlTheme && _themeManager?.TryGetTarget(out var manager) == true)
                {
                    manager.NotifyControlThemeResolved(key, controlTheme);
                }
                return true;
            }
        }
        resources.Index.ThrowIfPackageDisabled(key);
        value = null;
        return false;
    }

    internal int MaterializeAssets()
    {
        foreach (var asset in _resources.Index.Assets)
        {
            GetProvider(asset).EnsureLoaded(asset.GetExportKeys());
        }
        return _resources.Index.Assets.Count;
    }

    private bool TryGetAssetResource(StartupAssetEntry asset, object key, ThemeVariant? theme, out object? value)
    {
        if (_stagedIndex is not null && _themeManager?.TryGetTarget(out var manager) == true &&
            manager.PackageReadUpdate is { IsCandidateRead: true } update && update.OwnsExistingAsset(asset))
        {
            return TryGetCommittedAssetResource(update, asset, key, theme, out value);
        }
        return GetProvider(asset).TryGetResource(key, theme, out value);
    }

    private bool TryGetCommittedAssetResource(ThemeManager.PackageUpdate update, StartupAssetEntry asset,
        object key, ThemeVariant? theme, out object? value)
    {
        // Isolate captured parameters here: a lambda in the ordinary lookup method allocates
        // its display class before the staging guard. Protect the full query, including AddDeferred.
        var found = false;
        object? captured = null;
        update.RunCommittedRead(() => found = GetProvider(asset).TryGetResource(key, theme, out captured));
        value = captured;
        return found;
    }

    private DeferredControlThemeResourceProvider GetProvider(StartupAssetEntry asset)
    {
        asset.Package.FirstUse();
        return _materialized.TryGetValue(asset, out var provider) ? provider : CreateProvider(asset);
    }

    private DeferredControlThemeResourceProvider CreateProvider(StartupAssetEntry asset)
    {
        // Owner rollback captures the new provider; keep that closure on the cold creation path.
        var registration = asset.GetRegistration();
        var provider = new DeferredControlThemeResourceProvider(registration.AssetId, registration.Factory,
            registration.ResourceKeys, asset.Package.Definition.Id);
        try
        {
            ((IResourceProvider)provider).AddOwner(_bridge);
        }
        catch (Exception error)
        {
            var failures = new List<Exception> { error };
            ThemeResourceOwnerBridge.TryCleanup(() => ((IResourceProvider)provider).RemoveOwner(_bridge), failures);
            ThemeResourceOwnerBridge.ThrowFailures(failures);
            throw;
        }
        _materialized.Add(asset, provider);
        return provider;
    }

    public void AddOwner(IResourceHost owner)
    {
        ArgumentNullException.ThrowIfNull(owner);
        if (_owner is not null)
        {
            throw new InvalidOperationException("Startup resource layer already has an owner.");
        }
        _owner = owner;
        try
        {
            _bridge.RunUpdate(() => _bridge.SetOwner(owner));
            OwnerChanged?.Invoke(this, EventArgs.Empty);
            if (HasResources)
            {
                owner.NotifyHostedResourcesChanged(ResourcesChangedEventArgs.Create());
            }
        }
        catch (Exception error)
        {
            _owner = null;
            var failures = new List<Exception> { error };
            ThemeResourceOwnerBridge.TryCleanup(DetachResources, failures);
            ThemeResourceOwnerBridge.ThrowFailures(failures);
        }
    }

    public void RemoveOwner(IResourceHost owner)
    {
        ArgumentNullException.ThrowIfNull(owner);
        if (!ReferenceEquals(owner, _owner))
        {
            return;
        }
        _owner = null;
        var failures = new List<Exception>();
        ThemeResourceOwnerBridge.TryCleanup(() => OwnerChanged?.Invoke(this, EventArgs.Empty), failures);
        ThemeResourceOwnerBridge.TryCleanup(DetachResources, failures);
        if (HasResources)
        {
            ThemeResourceOwnerBridge.TryCleanup(() => owner.NotifyHostedResourcesChanged(ResourcesChangedEventArgs.Create()), failures);
        }
        ThemeResourceOwnerBridge.ThrowFailures(failures);
    }

    private void DetachResources()
    {
        _bridge.RunUpdate(() =>
        {
            var failures = new List<Exception>();
            foreach (var provider in _materialized.Values)
            {
                ThemeResourceOwnerBridge.TryCleanup(() => ((IResourceProvider)provider).RemoveOwner(_bridge), failures);
            }
            _materialized.Clear();
            ThemeResourceOwnerBridge.TryCleanup(() => _bridge.SetOwner(null), failures);
            ThemeResourceOwnerBridge.ThrowFailures(failures);
        });
    }
    internal sealed class ResourceIndex
    {
        internal ResourceIndex(StartupPackageIndex index)
        {
            Index = index;
            for (var ordinal = index.Assets.Count - 1; ordinal >= 0; ordinal--)
            {
                if (index.Assets[ordinal].Data.ResourceKeys is not { } keys)
                {
                    Unknown.Add(ordinal);
                    continue;
                }
                foreach (var key in keys)
                {
                    if (!Known.TryGetValue(key, out var matches))
                    {
                        matches = [];
                        Known.Add(key, matches);
                    }
                    matches.Add(ordinal);
                }
            }
        }
        internal StartupPackageIndex Index { get; }
        internal Dictionary<string, List<int>> Known { get; } = new(StringComparer.Ordinal);
        internal List<int> Unknown { get; } = [];
    }

}
