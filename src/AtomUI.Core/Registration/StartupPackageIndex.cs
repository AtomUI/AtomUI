using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using AtomUI.Startup.Data;
using AtomUI.Theme.Schema;

namespace AtomUI.Registration;

/// <summary>Application-owned indexes; metadata queries never activate registration proxies.</summary>
internal sealed class StartupPackageIndex
{
    private readonly Dictionary<string, StartupControlEntry> _byIdentity = new(StringComparer.Ordinal);
    private readonly Dictionary<string, StartupControlEntry> _byType = new(StringComparer.Ordinal);
    private readonly Dictionary<string, StartupControlEntry> _ownKeyTypes = new(StringComparer.Ordinal);
    private readonly StartupControlEntry[] _controls;
    private readonly StartupControlEntry[] _all;
    private readonly ControlThemeAssetDescriptor[] _manualAssets;
    private readonly Dictionary<object, int> _manualOwnKeys = new();
    private readonly StartupImage? _image;
    private readonly HashSet<string> _enabledIds;

    private StartupPackageIndex(IReadOnlyList<EnabledControlPackage> packages, StartupImage? image,
        ThemeSchemaRegistry? manualRegistry, IReadOnlyList<ControlSemanticDescriptor>? manualSemantics,
        IReadOnlyList<ControlThemeAssetDescriptor>? manualAssets, StartupPackageIndex? previous = null)
    {
        _image = previous?._image ?? image;
        _enabledIds = packages.Select(package => package.Definition.Id).ToHashSet(StringComparer.Ordinal);
        Packages = packages.ToArray();
        var data = packages.Select(package => previous is null && image is not null
            ? image.GetPackage(package.Definition) : package.Definition.Data).Select((metadata, ordinal) =>
                new StartupPackageData(metadata.Id, metadata.AssemblyName, metadata.Controls,
                    metadata.Assets.Where(asset => packages[ordinal].Definition.ContainsAsset(asset.Id)),
                    metadata.SchemaFingerprint, metadata.GlobalResourceTokens)).ToArray();
        var manual = (previous?._all.Where(entry => entry.Package is null) ?? [])
            .Concat(CreateManualEntries(manualRegistry, manualSemantics ?? [])).ToArray();
        _manualAssets = (previous?._manualAssets ?? []).Concat(manualAssets ?? []).ToArray();
        var oldEntries = previous?._all.ToDictionary(entry => entry.Data);
        var manualData = new StartupPackageData("$manual", "$manual", manual.Select(entry => entry.Data),
            _manualAssets.Select(asset => new StartupAssetData(asset.AssetId, asset.AssetUri.ToString(), 1, 0, null,
                asset.RequiredTokenOwners.Select(owner => owner.ToString()),
                asset.SemanticThemeBindings.Select(binding => binding.OwnerIdentity.ToString()),
                asset.ExportedThemes.Select(export => ResourceKey(export.ResourceKey)!),
                asset.RequiredTokenOwners.Select(owner => OwnerData(owner, owner.OwnerType!)),
                asset.SemanticThemeBindings.Select(binding => new StartupSemanticBindingData(
                    OwnerData(binding.OwnerIdentity, binding.OwnerIdentity.OwnerType!),
                    new StartupSemanticThemeData(binding.PropertyName, binding.TargetType.Assembly.GetName().Name!, binding.TargetType.FullName!))))),
            manualRegistry?.Revision.Value ?? 0);
        var hasManual = manual.Length != 0 || _manualAssets.Length != 0;
        if (image is null || hasManual || previous is not null)
        {
            StartupManifestLinker.Validate(hasManual ? data.Concat([manualData]) : data,
                Generated.AtomUICore.GeneratedThemeSchema.GetGlobalTokens().Select(token => token.Name));
        }
        CompiledStamp = hasManual || previous is not null ? null : image?.Stamp;
        if (hasManual)
        {
            var fingerprint = new SchemaFingerprintBuilder();
            fingerprint.Add(StartupManifestCodec.Encode(manualData));
            ManualSchemaStamp = fingerprint.Value;
        }
        var all = new List<StartupControlEntry>();
        for (var ordinal = 0; ordinal < packages.Count; ordinal++)
        {
            foreach (var control in data[ordinal].Controls)
            {
                if (!control.Keys.Any(packages[ordinal].Definition.Contains))
                {
                    continue;
                }
                var entry = oldEntries is not null && oldEntries.TryGetValue(control, out var existing)
                    ? existing : new StartupControlEntry(packages[ordinal], control.AssemblyName ?? data[ordinal].AssemblyName, control);
                if (control.HasTokenDescriptor || control.HasSemantic)
                {
                    _byIdentity.Add(control.Identity, entry);
                    _byType.Add(entry.AssemblyName + "|" + control.TypeName, entry);
                }
                all.Add(entry);
            }
        }
        foreach (var entry in manual)
        {
            _byIdentity.Add(entry.Data.Identity, entry);
            _byType.Add(entry.AssemblyName + "|" + entry.Data.TypeName, entry);
            all.Add(entry);
        }
        _all = all.ToArray();
        _controls = previous is not null
            ? previous.Controls.Concat(all.Where(entry => entry.Data.HasTokenDescriptor && entry.Slot < 0)
                .OrderBy(entry => entry.Data.Catalog, StringComparer.Ordinal).ThenBy(entry => entry.Data.Id, StringComparer.Ordinal)).ToArray()
            : image is null
            ? all.Where(entry => entry.Data.HasTokenDescriptor).OrderBy(entry => entry.Data.Catalog, StringComparer.Ordinal)
                .ThenBy(entry => entry.Data.Id, StringComparer.Ordinal).ToArray()
            : image.ControlOrder.Where(identity => _byIdentity.TryGetValue(identity, out var entry) && entry.Data.HasTokenDescriptor)
                .Select(identity => _byIdentity[identity]).Concat(manual.Where(entry => entry.Data.HasTokenDescriptor)
                    .OrderBy(entry => entry.Data.Catalog, StringComparer.Ordinal).ThenBy(entry => entry.Data.Id, StringComparer.Ordinal)).ToArray();
        if (_controls.Length != all.Count(entry => entry.Data.HasTokenDescriptor))
        {
            throw new InvalidOperationException("Startup image is missing enabled Control slots; rebuild the application.");
        }
        for (var slot = 0; slot < _controls.Length; slot++)
        {
            var entry = _controls[slot];
            if (entry.Slot >= 0 && entry.Slot != slot)
            {
                throw new InvalidOperationException("Existing Control slots cannot be reassigned during package append.");
            }
            entry.Slot = slot;
            if (entry.ManualDescriptor is { } descriptor)
            {
                foreach (var token in descriptor.OwnTokens)
                {
                    _manualOwnKeys.Add(token.ResourceKey, slot);
                }
                continue;
            }
            foreach (var keyType in entry.Data.OwnTokens.Select(token => (token.KeyAssembly ?? entry.Package!.Definition.Data.AssemblyName) + "|" + token.KeyType)
                .Distinct(StringComparer.Ordinal))
            {
                _ownKeyTypes.Add(keyType, entry);
            }
        }
        SemanticEntries = all.Where(entry => entry.Data.HasSemantic).ToArray();
        var assets = new List<StartupAssetEntry>();
        var oldAssets = previous?.Assets.ToDictionary(asset => asset.Data);
        for (var ordinal = 0; ordinal < data.Length; ordinal++)
        {
            var orderedAssets = image is not null && previous is null ? data[ordinal].Assets : data[ordinal].Assets.OrderBy(asset => asset.Phase)
                .ThenBy(asset => asset.Order).ThenBy(asset => asset.Id, StringComparer.Ordinal).ToArray();
            foreach (var asset in orderedAssets)
            {
                var candidates = all.Where(entry => ReferenceEquals(entry.Package, packages[ordinal]) &&
                    entry.Data.Assets.Contains(asset.Id, StringComparer.Ordinal)).ToArray();
                // Real TypeMap reachability, including Token-only/Semantic-only triggers, selects
                // the registration proxy. A data string can never make a missing proxy executable.
                if (candidates.Length != 0)
                {
                    foreach (var owner in asset.Owners)
                    {
                        if (!_byIdentity.ContainsKey(owner))
                        {
                            throw new InvalidOperationException("Theme asset '" + asset.Id +
                                "' requires an enabled package for Control '" + owner + "'.");
                        }
                    }
                    assets.Add(oldAssets is not null && oldAssets.TryGetValue(asset, out var existingAsset)
                        ? existingAsset : new StartupAssetEntry(asset, candidates, packages[ordinal]));
                }
            }
        }
        Assets = assets.ToArray();
    }

    internal static StartupPackageIndex Create(IReadOnlyList<EnabledControlPackage> packages, StartupImage? image = null,
        ThemeSchemaRegistry? manualRegistry = null, IReadOnlyList<ControlSemanticDescriptor>? manualSemantics = null,
        IReadOnlyList<ControlThemeAssetDescriptor>? manualAssets = null) => new(packages, image, manualRegistry, manualSemantics, manualAssets);
    internal StartupPackageIndex Append(IReadOnlyList<EnabledControlPackage> packages,
        ThemeSchemaRegistry? manualRegistry, IReadOnlyList<ControlSemanticDescriptor> manualSemantics,
        IReadOnlyList<ControlThemeAssetDescriptor> manualAssets) =>
        new(Packages.Concat(packages).ToArray(), null, manualRegistry, manualSemantics, manualAssets, this);

    internal IReadOnlyList<EnabledControlPackage> Packages { get; }
    internal IReadOnlyList<StartupControlEntry> Controls => _controls;
    internal IReadOnlyList<StartupControlEntry> SemanticEntries { get; }
    internal IReadOnlyList<StartupAssetEntry> Assets { get; }
    internal int ControlCount => _controls.Length;
    internal ulong? CompiledStamp { get; }
    internal ulong? ManualSchemaStamp { get; }

    internal void NotifyFirstUse(int slot) => _controls[slot].Package?.FirstUse();

    internal ControlSemanticDescriptor GetSemanticAt(int slot)
    {
        var entry = SemanticEntries[slot];
        var descriptor = entry.GetSemantic();
        entry.Package?.FirstUse();
        return descriptor;
    }

    internal void ThrowIfPackageDisabled(object key)
    {
        if (ResourceKey(key) is { } resourceKey)
        {
            ThrowIfPackageDisabledKey(resourceKey);
        }
    }

    private void ThrowIfPackageDisabledKey(string resourceKey)
    {
        if (_image is not null &&
            _image.TryGetResourceOwner(resourceKey, out var packageId) && !_enabledIds.Contains(packageId!))
        {
            throw new InvalidOperationException("Control package '" + packageId +
                "' is referenced but not enabled. Call its UseXxx() package registration entry before using this resource.");
        }
    }
    internal void Release()
    {
        List<Exception>? failures = null;
        for (var index = Packages.Count - 1; index >= 0; index--)
        {
            try
            {
                Packages[index].Release();
            }
            catch (Exception error)
            {
                (failures ??= []).Add(error);
            }
        }
        if (failures is not null)
        {
            throw new AggregateException("Control package disposal failed.", failures);
        }
    }

    internal bool TryGetControl(Type type, [NotNullWhen(true)] out ControlTokenDescriptor? descriptor)
    {
        if (_byType.TryGetValue(TypeKey(type), out var entry) && entry.Slot >= 0)
        {
            var candidate = entry.GetDescriptor();
            if (candidate.ControlType == type)
            {
                descriptor = candidate;
                return true;
            }
        }
        ThrowIfPackageDisabled(type);
        descriptor = null;
        return false;
    }

    internal bool TryGetControl(ControlTokenIdentity identity, [NotNullWhen(true)] out ControlTokenDescriptor? descriptor)
    {
        if (_byIdentity.TryGetValue(identity.ToString(), out var entry) && entry.Slot >= 0)
        {
            var candidate = entry.GetDescriptor();
            if (ControlTokenIdentityCanonicalizer.Matches(identity, candidate.ControlType))
            {
                descriptor = candidate;
                return true;
            }
        }
        ThrowIfPackageDisabledKey("i:" + identity);
        descriptor = null;
        return false;
    }

    internal bool MatchesControlRequirement(ControlTokenIdentity identity, Type ownerType,
        string ownerAssemblyName, string ownerMetadataName)
    {
        if (!_byIdentity.TryGetValue(identity.ToString(), out var entry) || entry.Slot < 0 ||
            !entry.Data.HasTokenDescriptor || !ControlTokenIdentityCanonicalizer.Matches(identity, ownerType))
        {
            return false;
        }
        if (entry.ManualDescriptor is { } manual)
        {
            return manual.ControlType == ownerType &&
                   string.Equals(manual.ControlType.FullName, ownerMetadataName, StringComparison.Ordinal) &&
                   string.Equals(manual.ControlType.Assembly.GetName().Name, ownerAssemblyName, StringComparison.Ordinal);
        }
        return string.Equals(entry.AssemblyName, ownerAssemblyName, StringComparison.Ordinal) &&
               string.Equals(entry.Data.TypeName, ownerMetadataName, StringComparison.Ordinal) &&
               string.Equals(ownerType.Assembly.GetName().Name, ownerAssemblyName, StringComparison.Ordinal);
    }

    internal bool TryGetSemantic(Type type, [NotNullWhen(true)] out ControlSemanticDescriptor? descriptor, bool notifyFirstUse = true)
    {
        if (_byType.TryGetValue(TypeKey(type), out var entry) && entry.Data.HasSemantic)
        {
            var candidate = entry.GetSemantic();
            if (candidate.ControlType == type)
            {
                if (notifyFirstUse)
                {
                    entry.Package?.FirstUse();
                }
                descriptor = candidate;
                return true;
            }
        }
        ThrowIfPackageDisabled(type);
        descriptor = null;
        return false;
    }

    internal bool TryGetSemantic(ControlTokenIdentity identity, [NotNullWhen(true)] out ControlSemanticDescriptor? descriptor, bool notifyFirstUse = true)
    {
        if (_byIdentity.TryGetValue(identity.ToString(), out var entry) && entry.Data.HasSemantic)
        {
            var candidate = entry.GetSemantic();
            if (ControlTokenIdentityCanonicalizer.Matches(identity, candidate.ControlType))
            {
                if (notifyFirstUse)
                {
                    entry.Package?.FirstUse();
                }
                descriptor = candidate;
                return true;
            }
        }
        ThrowIfPackageDisabledKey("i:" + identity);
        descriptor = null;
        return false;
    }

    internal bool TryGetOwnResourceSlot(object key, out int slot)
    {
        if (_manualOwnKeys.TryGetValue(key, out slot))
        {
            return true;
        }
        if (key is Enum value && _ownKeyTypes.TryGetValue(TypeKey(value.GetType()), out var entry))
        {
            var ordinal = Convert.ToInt64(value, CultureInfo.InvariantCulture);
            if (ordinal >= 0 && ordinal < entry.Data.OwnTokens.Count)
            {
                slot = entry.Slot;
                return true;
            }
        }
        slot = -1;
        return false;
    }

    internal static string TypeKey(Type type) => type.Assembly.GetName().Name + "|" + type.FullName;
    internal static string? ResourceKey(object key) => key switch
    {
        Type type => "t:" + TypeKey(type),
        string name => "s:" + name,
        _ => null
    };

    private static StartupOwnerData OwnerData(ControlTokenIdentity identity, Type type) =>
        new(identity.ToString(), type.Assembly.GetName().Name!, type.FullName!);

    private static StartupControlEntry[] CreateManualEntries(ThemeSchemaRegistry? registry,
        IReadOnlyList<ControlSemanticDescriptor> semantics)
    {
        _ = new SemanticPartRegistry(semantics);
        var owners = new Dictionary<Type, (ControlTokenDescriptor? Token, ControlSemanticDescriptor? Semantic)>();
        foreach (var descriptor in registry?.Controls ?? [])
        {
            owners.Add(descriptor.ControlType, (descriptor, null));
        }
        foreach (var semantic in semantics)
        {
            owners.TryGetValue(semantic.ControlType, out var owner);
            if (owner.Token is { } token && token.Identity != semantic.Identity)
            {
                throw new ThemeSchemaException("Manual semantic and Token owners have different identities: " + semantic.Identity);
            }
            owners[semantic.ControlType] = (owner.Token, semantic);
        }
        return owners.Select(pair =>
        {
            var identity = pair.Value.Token?.Identity ?? pair.Value.Semantic!.Identity;
            var assembly = pair.Key.Assembly.GetName().Name!;
            var tokens = pair.Value.Token?.OwnTokens.Select(token => new StartupTokenData(token.Name,
                token.ValueType.FullName!, token.ResourceKey.GetType().FullName!, token.ResourceKey.ToString()!, token.ResourceKey.GetType().Assembly.GetName().Name!)) ?? [];
            var data = new StartupControlData(identity.Catalog, identity.Id, pair.Key.FullName!, ["manual:" + identity],
                tokens, pair.Value.Semantic is not null, pair.Value.Token is not null, assemblyName: assembly,
                semanticThemes: pair.Value.Semantic?.Parts.Where(part => part.Theme is not null).Select(part =>
                    new StartupSemanticThemeData(part.Theme!.PropertyName, part.Theme.TargetType.Assembly.GetName().Name!, part.Theme.TargetType.FullName!)));
            return new StartupControlEntry(null, assembly, data, pair.Value.Token, pair.Value.Semantic);
        }).ToArray();
    }
}

internal sealed class StartupControlEntry
{
    private readonly object _gate = new();
    private MaterializedControlFragment? _fragment;
    private ControlTokenDescriptor? _descriptor;
    private ControlSemanticDescriptor? _semantic;
    private bool _materializing;

    internal StartupControlEntry(EnabledControlPackage? package, string assemblyName, StartupControlData data,
        ControlTokenDescriptor? manualDescriptor = null, ControlSemanticDescriptor? manualSemantic = null)
    {
        Package = package;
        AssemblyName = assemblyName;
        Data = data;
        ManualDescriptor = manualDescriptor;
        _semantic = manualSemantic;
    }

    internal EnabledControlPackage? Package { get; }
    internal ControlTokenDescriptor? ManualDescriptor { get; }
    internal string AssemblyName { get; }
    internal StartupControlData Data { get; }
    internal int Slot { get; set; } = -1;
    internal ControlTokenIdentity Identity => new(Data.Catalog, Data.Id);

    internal ControlTokenDescriptor GetDescriptor()
    {
        lock (_gate)
        {
            if (_descriptor is not null)
            {
                return _descriptor;
            }
            var descriptor = ManualDescriptor ?? Materialize().Controls.SingleOrDefault(control =>
                control.Identity == Identity && control.ControlType.FullName == Data.TypeName &&
                control.ControlType.Assembly.GetName().Name == AssemblyName);
            if (descriptor is null || Slot < 0)
            {
                throw new InvalidOperationException("Registration proxy does not satisfy its Control manifest: " + Data.Identity);
            }
            return _descriptor = descriptor.Slot == Slot ? descriptor : descriptor.Bind(Slot);
        }
    }

    internal ControlSemanticDescriptor GetSemantic()
    {
        lock (_gate)
        {
            return _semantic ??= Materialize().Semantics.SingleOrDefault(semantic =>
                semantic.Identity == Identity && semantic.ControlType.FullName == Data.TypeName &&
                semantic.ControlType.Assembly.GetName().Name == AssemblyName) ??
                throw new InvalidOperationException("Registration proxy does not satisfy its semantic manifest: " + Data.Identity);
        }
    }

    internal MaterializedControlFragment Materialize()
    {
        lock (_gate)
        {
            if (_fragment is not null)
            {
                return _fragment;
            }
            if (_materializing)
            {
                throw new InvalidOperationException("Recursive Control registration materialization: " + Data.Identity);
            }
            _materializing = true;
            try
            {
                var package = Package ?? throw new InvalidOperationException("Manual descriptors have no registration proxy.");
                ControlRegistrationFragmentAttribute? fragment = null;
                foreach (var key in Data.Keys)
                {
                    if (package.Definition.Contains(key) && (fragment = package.Definition.Resolve(key)) is not null)
                    {
                        break;
                    }
                }
                if (fragment is null)
                {
                    throw new InvalidOperationException("Control package proxy was not selected: " + Data.Identity);
                }
                var builder = new ControlPackageRegistrationBuilder();
                fragment.Add(builder);
                _fragment = builder.CompleteFragment();
                if (AtomUIStartupEventSource.Log.IsEnabled())
                {
                    AtomUIStartupEventSource.Log.FragmentMaterialized(package.Definition.Id, Data.TypeName);
                }
                return _fragment;
            }
            finally
            {
                _materializing = false;
            }
        }
    }
}

internal sealed class StartupAssetEntry(StartupAssetData data, StartupControlEntry[] candidates, EnabledControlPackage package)
{
    internal StartupAssetData Data { get; } = data;
    internal EnabledControlPackage Package { get; } = package;

    internal IEnumerable<object> GetExportKeys()
    {
        foreach (var candidate in candidates)
        {
            var descriptor = candidate.Materialize().Assets.FirstOrDefault(asset => asset.AssetId == Data.Id);
            if (descriptor is not null)
            {
                return descriptor.ExportedThemes.Select(export => export.ResourceKey);
            }
        }
        return [];
    }

    internal ControlThemeResourceRegistration GetRegistration()
    {
        foreach (var candidate in candidates)
        {
            var registration = candidate.Materialize().Resources.FirstOrDefault(resource => resource.AssetId == Data.Id);
            if (registration is not null)
            {
                return registration;
            }
        }
        throw new InvalidOperationException("Selected package proxy did not provide theme asset: " + Data.Id);
    }
}

internal sealed record MaterializedControlFragment(
    IReadOnlyList<ControlTokenDescriptor> Controls,
    IReadOnlyList<ControlSemanticDescriptor> Semantics,
    IReadOnlyList<ControlThemeAssetDescriptor> Assets,
    IReadOnlyList<ControlThemeResourceRegistration> Resources);
