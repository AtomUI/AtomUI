using Avalonia.Controls;
using Avalonia.Styling;

namespace AtomUI.Theme.Resources;

/// <summary>
/// 静态主题资源惰性代理；局部 owner 桥保证字典绑定可用，首次装配不广播整棵资源树。
/// 与 Avalonia 资源体系一致，本类型只在 UI 线程访问。
/// </summary>
internal sealed class DeferredControlThemeResourceProvider : IResourceProvider
{
    private readonly string _assetId;
    private readonly string _packageId;
    private readonly IReadOnlyList<object>? _exportedKeys;
    private readonly Func<IResourceProvider> _factory;
    private readonly ThemeResourceOwnerBridge _ownerBridge;
    private IResourceProvider? _inner;
    private IResourceHost? _owner;
    private bool _isLoading;

    internal DeferredControlThemeResourceProvider(string assetId, Func<IResourceProvider> factory,
        IReadOnlySet<object>? resourceKeys = null, string? packageId = null, IReadOnlyList<object>? exportedKeys = null)
    {
        _assetId = assetId;
        _packageId = packageId ?? string.Empty;
        _exportedKeys = exportedKeys;
        _factory = factory;
        ResourceKeys = resourceKeys;
        _ownerBridge = new ThemeResourceOwnerBridge();
    }

    // Explicit validation uses the same attached owner and factory path, including empty-key
    // dictionaries. Export lookups also instantiate typed-theme AddDeferred entries.
    internal void EnsureLoaded(IEnumerable<object>? exportedKeys = null)
    {
        var resources = Load();
        foreach (var key in (ResourceKeys ?? Enumerable.Empty<object>()).Concat(_exportedKeys ?? [])
            .Concat(exportedKeys ?? []).Distinct())
        {
            resources.TryGetResource(key, ThemeVariant.Default, out _);
            resources.TryGetResource(key, ThemeVariant.Light, out _);
            resources.TryGetResource(key, ThemeVariant.Dark, out _);
        }
    }

    internal bool IsLoaded => _inner is not null;
    internal string AssetId => _assetId;
    internal IReadOnlySet<object>? ResourceKeys { get; }
    public IResourceHost? Owner => _owner;
    public bool HasResources => _inner?.HasResources ?? true;
    public event EventHandler? OwnerChanged;

    public bool TryGetResource(object key, ThemeVariant? theme, out object? value)
    {
        // 对齐 ResourceInclude：构造期沿全局资源表重入自身按 miss 处理。
        if (_isLoading)
        {
            value = null;
            return false;
        }
        return Load().TryGetResource(key, theme, out value);
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
            if (_inner is not null)
            {
                AttachInnerOwner();
            }
            OwnerChanged?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception exception)
        {
            // 原始挂载异常与回滚异常都保留，内部宿主状态必须先退出本次挂载。
            var failures = new List<Exception> { exception };
            _owner = null;
            ThemeResourceOwnerBridge.TryCleanup(() => DetachInnerOwner(_inner), failures);
            ThemeResourceOwnerBridge.ThrowFailures(failures);
        }
    }

    void IResourceProvider.RemoveOwner(IResourceHost owner)
    {
        if (!ReferenceEquals(_owner, owner))
        {
            return;
        }
        _owner = null;
        var failures = new List<Exception>();
        ThemeResourceOwnerBridge.TryCleanup(() => DetachInnerOwner(_inner), failures);
        ThemeResourceOwnerBridge.TryCleanup(() => OwnerChanged?.Invoke(this, EventArgs.Empty), failures);
        ThemeResourceOwnerBridge.ThrowFailures(failures);
    }

    private IResourceProvider Load()
    {
        if (_inner is { } loaded)
        {
            return loaded;
        }
        _isLoading = true;
        try
        {
            _ownerBridge.RunUpdate(() =>
            {
                _inner = _factory() ?? throw new InvalidOperationException(
                    $"Theme resource factory '{_assetId}' returned null.");
                if (_owner is not null)
                {
                    AttachInnerOwner();
                }
            });
        }
        catch (Exception exception)
        {
            var failures = new List<Exception> { exception };
            var failed = _inner;
            _inner = null;
            ThemeResourceOwnerBridge.TryCleanup(() => DetachInnerOwner(failed), failures);
            ThemeResourceOwnerBridge.ThrowFailures(failures);
        }
        finally
        {
            _isLoading = false;
        }
        if (AtomUIStartupEventSource.Log.IsEnabled())
        {
            AtomUIStartupEventSource.Log.ThemeAssetMaterialized(_packageId, _assetId);
        }
        return _inner!;
    }

    private void AttachInnerOwner()
    {
        _ownerBridge.RunUpdate(() =>
        {
            _ownerBridge.SetOwner(_owner);
            _inner!.AddOwner(_ownerBridge);
        });
    }

    private void DetachInnerOwner(IResourceProvider? inner)
    {
        _ownerBridge.RunUpdate(() =>
        {
            var failures = new List<Exception>();
            try
            {
                ThemeResourceOwnerBridge.TryCleanup(() => inner?.RemoveOwner(_ownerBridge), failures);
            }
            finally
            {
                ThemeResourceOwnerBridge.TryCleanup(() => _ownerBridge.SetOwner(null), failures);
            }
            ThemeResourceOwnerBridge.ThrowFailures(failures);
        });
    }
}
