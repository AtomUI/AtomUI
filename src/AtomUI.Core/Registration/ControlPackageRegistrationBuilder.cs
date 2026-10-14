using System.ComponentModel;
using AtomUI.Theme;
using AtomUI.Theme.Resources;
using AtomUI.Theme.Schema;

namespace AtomUI.Registration;

[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class ControlPackageRegistrationBuilder
{
    private readonly List<ControlTokenDescriptor> _controls = [];
    private readonly List<ControlSemanticDescriptor> _semantics = [];
    private readonly Dictionary<string, ControlThemeAssetDescriptor> _assets = new(StringComparer.Ordinal);
    private readonly Dictionary<string, ControlThemeResourceRegistration> _resources = new(StringComparer.Ordinal);
    private readonly Dictionary<string, Type> _fragments = new(StringComparer.Ordinal);
    private readonly bool _deferFragments;
    private List<ControlRegistrationFragmentAttribute>? _pendingFragments;
    private Exception? _failure;
    private bool _sealed;
    private ulong? _packageSchemaFingerprint;

    internal ControlPackageRegistrationBuilder() : this(false)
    {
    }

    internal ControlPackageRegistrationBuilder(bool deferFragments)
    {
        _deferFragments = deferFragments;
    }

    /// <summary>
    /// 生成注册 Collect 写入的包级 schema 指纹常量（编译期预计算）。手工收集留空，
    /// 启动 registry 的 Revision 随之回退到全量遍历算法。
    /// </summary>
    public ulong? PackageSchemaFingerprint
    {
        get => _packageSchemaFingerprint;
        set => Mutate(value, static (builder, fingerprint) => builder._packageSchemaFingerprint = fingerprint);
    }

    public void AddControl(ControlTokenDescriptor descriptor) => Mutate(descriptor, static (builder, descriptor) =>
    {
        ArgumentNullException.ThrowIfNull(descriptor);
        builder._controls.Add(descriptor);
    });

    public void AddSemanticControl(ControlSemanticDescriptor descriptor) => Mutate(descriptor, static (builder, descriptor) =>
    {
        ArgumentNullException.ThrowIfNull(descriptor);
        builder._semantics.Add(descriptor);
    });

    public void AddThemeAsset(ControlThemeAssetDescriptor descriptor) => Mutate(descriptor, static (builder, descriptor) => builder.AddThemeAssetCore(descriptor));

    private void AddThemeAssetCore(ControlThemeAssetDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(descriptor);
        if (_assets.TryGetValue(descriptor.AssetId, out var existing))
        {
            if (!existing.HasSameMetadata(descriptor))
            {
                throw new InvalidOperationException($"Theme asset '{descriptor.AssetId}' has conflicting metadata.");
            }
            return;
        }
        _assets.Add(descriptor.AssetId, descriptor);
    }

    public void AddResource(ControlThemeResourceRegistration registration) => Mutate(registration, static (builder, registration) => builder.AddResourceCore(registration));

    private void AddResourceCore(ControlThemeResourceRegistration registration)
    {
        ArgumentNullException.ThrowIfNull(registration);
        if (_resources.TryGetValue(registration.AssetId, out var existing))
        {
            if (!existing.HasSameMetadata(registration))
            {
                throw new InvalidOperationException($"Theme resource '{registration.AssetId}' has conflicting metadata or factory.");
            }
            return;
        }
        _resources.Add(registration.AssetId, registration);
    }

    public void AddThemeAsset(ControlThemeAssetDescriptor descriptor, ControlThemeResourceRegistration registration) => Mutate((descriptor, registration), static (builder, state) =>
    {
        var (descriptor, registration) = state;
        ArgumentNullException.ThrowIfNull(descriptor);
        ArgumentNullException.ThrowIfNull(registration);
        if (descriptor.AssetId != registration.AssetId)
        {
            throw new ArgumentException("Theme asset and resource registration must have the same AssetId.");
        }
        builder.AddThemeAssetCore(descriptor);
        builder.AddResourceCore(registration);
    });

    public void AddFragment(ControlRegistrationFragmentAttribute fragment) => Mutate(fragment, static (builder, fragment) =>
    {
        ArgumentNullException.ThrowIfNull(fragment);
        ArgumentException.ThrowIfNullOrWhiteSpace(fragment.FragmentId);
        var type = fragment.GetType();
        if (builder._fragments.TryGetValue(fragment.FragmentId, out var existing))
        {
            if (existing != type)
            {
                throw new InvalidOperationException($"Registration fragment '{fragment.FragmentId}' has conflicting proxy types.");
            }
            return;
        }
        builder._fragments.Add(fragment.FragmentId, type);
        if (builder._deferFragments && !fragment.IsBootstrap)
        {
            (builder._pendingFragments ??= []).Add(fragment);
            return;
        }
        fragment.Add(builder);
    });

    internal ControlPackageRegistration Build(string id, IControlThemesProvider provider)
    {
        ThrowIfUnavailable();
        _sealed = true;
        foreach (var asset in _assets.Values)
        {
            if (!_resources.ContainsKey(asset.AssetId))
            {
                throw new InvalidOperationException($"Theme asset '{asset.AssetId}' has no resource factory.");
            }
        }
        var pending = _pendingFragments;
        _pendingFragments = null;
        return new(id, _controls, _assets.Values, _semantics, provider, _resources.Values)
        {
            SchemaFingerprint = pending is null ? PackageSchemaFingerprint : null,
            CompleteRegistration = pending is null ? null : () => Complete(id, provider, pending)
        };
    }

    private ControlPackageRegistration Complete(string id, IControlThemesProvider provider,
        IReadOnlyList<ControlRegistrationFragmentAttribute> pending)
    {
        // The published snapshot and original collection stay sealed. Complete in a fresh collection;
        // shared assets are validated against the original contributions before a replacement is published.
        var completion = new ControlPackageRegistrationBuilder
        {
            PackageSchemaFingerprint = PackageSchemaFingerprint
        };
        completion._controls.AddRange(_controls);
        completion._semantics.AddRange(_semantics);
        foreach (var (key, value) in _assets)
        {
            completion._assets.Add(key, value);
        }
        foreach (var (key, value) in _resources)
        {
            completion._resources.Add(key, value);
        }
        foreach (var (key, value) in _fragments)
        {
            completion._fragments.Add(key, value);
        }
        foreach (var fragment in pending)
        {
            completion.Mutate(fragment, static (builder, value) => value.Add(builder));
        }
        return completion.Build(id, provider);
    }

    internal void Fail(Exception exception)
    {
        _failure ??= exception;
        _pendingFragments = null;
    }

    private void Mutate<TState>(TState state, Action<ControlPackageRegistrationBuilder, TState> action)
    {
        ThrowIfUnavailable();
        try
        {
            action(this, state);
        }
        catch (Exception exception)
        {
            Fail(exception);
            throw;
        }
    }

    internal void ThrowIfUnavailable()
    {
        if (_failure is not null || _sealed)
        {
            throw new InvalidOperationException("The package collection is failed or already staged.", _failure);
        }
    }
}
