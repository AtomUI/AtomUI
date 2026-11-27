using System.ComponentModel;
using AtomUI.Theme;
using AtomUI.Theme.Resources;
using AtomUI.Theme.Schema;

namespace AtomUI.Registration;

/// <summary>
/// Collects the contributions of one control package while its collect callback runs: control descriptors,
/// theme assets with their resource registrations, and registration fragments.
/// </summary>
/// <remarks>
/// After a mutator throws, or once the collection has been built into the package registration, every
/// further mutator throws an <see cref="InvalidOperationException" />.
/// </remarks>
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

    internal MaterializedControlFragment CompleteFragment()
    {
        _sealed = true;
        return new MaterializedControlFragment(_controls.ToArray(), _semantics.ToArray(),
            _assets.Values.ToArray(), _resources.Values.ToArray());
    }

    internal IReadOnlyList<ControlRegistrationFragmentAttribute> ExtractFragments()
    {
        _sealed = true;
        return _pendingFragments?.ToArray() ?? [];
    }

    /// <summary>
    /// The package-level schema fingerprint precomputed by the source generator and written by the generated
    /// collect callback. Leave it <see langword="null" /> when the collection is written by hand; the Revision
    /// of the startup registry then falls back to the full traversal algorithm.
    /// </summary>
    public ulong? PackageSchemaFingerprint
    {
        get => _packageSchemaFingerprint;
        set => Mutate(value, static (builder, fingerprint) => builder._packageSchemaFingerprint = fingerprint);
    }

    /// <summary>
    /// Adds the descriptor of a control's design tokens to the package.
    /// </summary>
    public void AddControl(ControlTokenDescriptor descriptor) => Mutate(descriptor, static (builder, descriptor) =>
    {
        ArgumentNullException.ThrowIfNull(descriptor);
        builder._controls.Add(descriptor);
    });

    /// <summary>
    /// Adds the descriptor of a control's semantic parts to the package.
    /// </summary>
    public void AddSemanticControl(ControlSemanticDescriptor descriptor) => Mutate(descriptor, static (builder, descriptor) =>
    {
        ArgumentNullException.ThrowIfNull(descriptor);
        builder._semantics.Add(descriptor);
    });

    /// <summary>
    /// Registers a theme asset in the package, keyed by <see cref="ControlThemeAssetDescriptor.AssetId" />.
    /// Registering the same asset again with equal metadata is ignored; different metadata throws an
    /// <see cref="InvalidOperationException" />.
    /// </summary>
    /// <remarks>
    /// Every asset needs a resource registration with the same AssetId, added with <see cref="AddResource" />
    /// or with <see cref="AddThemeAsset(ControlThemeAssetDescriptor, ControlThemeResourceRegistration)" />;
    /// otherwise the package registration cannot be built.
    /// </remarks>
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

    /// <summary>
    /// Registers the resource provider factory of a theme asset, keyed by
    /// <see cref="ControlThemeResourceRegistration.AssetId" />. Registering the same AssetId again with equal
    /// metadata and an equal factory is ignored; any difference throws an
    /// <see cref="InvalidOperationException" />.
    /// </summary>
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

    /// <summary>
    /// Registers a theme asset together with the resource provider factory of the asset.
    /// </summary>
    /// <remarks>
    /// Both arguments must carry the same AssetId, otherwise an <see cref="ArgumentException" /> is thrown.
    /// The effect is the same as calling <see cref="AddThemeAsset(ControlThemeAssetDescriptor)" /> and
    /// <see cref="AddResource(ControlThemeResourceRegistration)" /> in a row.
    /// </remarks>
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

    /// <summary>
    /// Adds a registration fragment to the package and runs
    /// <see cref="ControlRegistrationFragmentAttribute.Add(ControlPackageRegistrationBuilder)" /> on this builder.
    /// </summary>
    /// <remarks>
    /// The <see cref="ControlRegistrationFragmentAttribute.FragmentId" /> must not be null, empty or white space.
    /// The first fragment registered under an id wins: a later fragment with the same id is ignored when it
    /// has the same proxy type, and throws an <see cref="InvalidOperationException" /> when it has another one.
    /// A builder that defers fragments only records the fragment here; its Add runs when the registration is
    /// completed.
    /// </remarks>
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
        if (builder._deferFragments)
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
