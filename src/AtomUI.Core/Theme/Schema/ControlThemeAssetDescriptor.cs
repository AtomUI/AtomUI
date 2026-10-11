using System.ComponentModel;

namespace AtomUI.Theme.Schema;

public sealed class ControlThemeAssetDescriptor
{
    public ControlThemeAssetDescriptor(
        string assetId,
        Uri assetUri,
        IEnumerable<ControlThemeExportDescriptor> exportedThemes,
        IEnumerable<ControlTokenIdentity> requiredTokenOwners,
        IEnumerable<ControlThemeBindingDescriptor> semanticThemeBindings,
        ulong resourceKeySchemaFingerprint)
        : this(assetId, assetUri, exportedThemes, requiredTokenOwners, semanticThemeBindings,
            resourceKeySchemaFingerprint, presorted: false)
    {
    }

    private ControlThemeAssetDescriptor(
        string assetId,
        Uri assetUri,
        IEnumerable<ControlThemeExportDescriptor> exportedThemes,
        IEnumerable<ControlTokenIdentity> requiredTokenOwners,
        IEnumerable<ControlThemeBindingDescriptor> semanticThemeBindings,
        ulong resourceKeySchemaFingerprint,
        bool presorted)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(assetId);
        ArgumentNullException.ThrowIfNull(assetUri);
        ArgumentNullException.ThrowIfNull(exportedThemes);
        ArgumentNullException.ThrowIfNull(requiredTokenOwners);
        ArgumentNullException.ThrowIfNull(semanticThemeBindings);
        AssetId = assetId;
        AssetUri = assetUri;
        var exports = exportedThemes.ToArray();
        var bindings = semanticThemeBindings.ToArray();
        if (exports.Any(static export => export is null) || bindings.Any(static binding => binding is null))
        {
            throw new ArgumentException("Theme exports and bindings cannot contain null.");
        }
        var keys = new HashSet<object>();
        foreach (var export in exports)
        {
            if (!keys.Add(export.ResourceKey))
            {
                throw new ArgumentException($"Duplicate theme export key '{export.ResourceKey}'.", nameof(exportedThemes));
            }
        }
        // The presorted path is the generated-registration contract: inputs arrive in the exact order
        // the sorting path below would produce, so ordering is asserted O(n) instead of re-sorted.
        // Unsorted evidence throws immediately (a generator bug must not become a silent wrong result).
        var owners = presorted
            ? CanonicalizePresortedOwners(requiredTokenOwners)
            : ControlTokenIdentityCanonicalizer.Canonicalize(requiredTokenOwners);
        if (owners.Any(static identity => identity.OwnerType is null))
        {
            throw new ArgumentException("Required Token owners must carry their actual Control type.", nameof(requiredTokenOwners));
        }
        if (presorted)
        {
            // Establish the order contract before the binding-owner walk relies on it.
            EnsurePresorted(exports, CompareExports, "exports", assetId);
            EnsurePresorted(bindings, CompareBindings, "semantic theme bindings", assetId);
        }
        // Validate owner evidence across bindings before any metadata deduplication.
        if (bindings.Length > 0)
        {
            if (presorted)
            {
                ValidatePresortedBindingOwners(owners, bindings);
            }
            else
            {
                var bindingOwners = new Dictionary<ControlTokenIdentity, ControlTokenIdentity>();
                foreach (var owner in owners)
                {
                    bindingOwners.Add(owner, owner);
                }
                foreach (var binding in bindings)
                {
                    var owner = binding.OwnerIdentity;
                    bindingOwners[owner] = bindingOwners.TryGetValue(owner, out var existing)
                        ? ControlTokenIdentityCanonicalizer.Merge(existing, owner)
                        : owner;
                }
            }
        }
        if (presorted)
        {
            ExportedThemes       = Array.AsReadOnly(exports);
            SemanticThemeBindings = Array.AsReadOnly(bindings);
        }
        else
        {
            ExportedThemes = Array.AsReadOnly(exports.OrderBy(static export => export.TargetType.AssemblyQualifiedName, StringComparer.Ordinal)
                .ThenBy(static export => export.ResourceKey is Type ? 0 : 1)
                .ThenBy(static export => export.ResourceKey is Type type ? type.AssemblyQualifiedName : (string)export.ResourceKey, StringComparer.Ordinal).ToArray());
            SemanticThemeBindings = Array.AsReadOnly(bindings.Distinct().OrderBy(static binding => binding.OwnerIdentity.Catalog, StringComparer.Ordinal)
                .ThenBy(static binding => binding.OwnerIdentity.Id, StringComparer.Ordinal)
                .ThenBy(static binding => binding.PropertyName, StringComparer.Ordinal)
                .ThenBy(static binding => binding.TargetType.AssemblyQualifiedName, StringComparer.Ordinal).ToArray());
        }
        RequiredTokenOwners = Array.AsReadOnly(owners);
        ResourceKeySchemaFingerprint = resourceKeySchemaFingerprint;
    }

    /// <summary>Materializes a generated asset using compile-time schema evidence and actual runtime Types.</summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static ControlThemeAssetDescriptor CreateGenerated(
        string assetId,
        Uri assetUri,
        IEnumerable<ControlThemeExportDescriptor> exportedThemes,
        IEnumerable<ControlTokenIdentity> requiredTokenOwners,
        IEnumerable<ControlThemeBindingDescriptor> semanticThemeBindings,
        IReadOnlyList<string> compiledGlobalTokenNames,
        ulong compiledContractFingerprint) => new(assetId, assetUri, exportedThemes, requiredTokenOwners,
            semanticThemeBindings, compiledGlobalTokenNames, compiledContractFingerprint);

    /// <summary>
    /// Materializes a generated asset by continuing from the package-level fingerprint prefixes
    /// (compile-time constants over the compiled global token names), skipping the per-asset
    /// re-hash of the shared global segment. Bit-identical to the names-based overload.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static ControlThemeAssetDescriptor CreateGenerated(
        string assetId,
        Uri assetUri,
        IEnumerable<ControlThemeExportDescriptor> exportedThemes,
        IEnumerable<ControlTokenIdentity> requiredTokenOwners,
        IEnumerable<ControlThemeBindingDescriptor> semanticThemeBindings,
        in SchemaPrefixState contractFingerprintPrefix,
        in SchemaPrefixState resourceKeyFingerprintPrefix,
        ulong compiledContractFingerprint) => new(assetId, assetUri, exportedThemes, requiredTokenOwners,
            semanticThemeBindings, contractFingerprintPrefix, resourceKeyFingerprintPrefix, compiledContractFingerprint);

    private ControlThemeAssetDescriptor(
        string assetId,
        Uri assetUri,
        IEnumerable<ControlThemeExportDescriptor> exportedThemes,
        IEnumerable<ControlTokenIdentity> requiredTokenOwners,
        IEnumerable<ControlThemeBindingDescriptor> semanticThemeBindings,
        IReadOnlyList<string> compiledGlobalTokenNames,
        ulong compiledContractFingerprint)
        : this(assetId, assetUri, exportedThemes, requiredTokenOwners, semanticThemeBindings, 0)
    {
        ArgumentNullException.ThrowIfNull(compiledGlobalTokenNames);
        var (contractPrefix, resourceKeyPrefix) = ThemeSchemaRegistry.HashGlobalTokenNames(compiledGlobalTokenNames);
        ValidateGeneratedContract(this, contractPrefix, compiledContractFingerprint);
        // Full runtime Type identities are materialized only for this selected descriptor. The
        // registry later checks this fingerprint independently against its actual global schema.
        ResourceKeySchemaFingerprint = ComputeResourceKeyFingerprint(this, resourceKeyPrefix);
    }

    private ControlThemeAssetDescriptor(
        string assetId,
        Uri assetUri,
        IEnumerable<ControlThemeExportDescriptor> exportedThemes,
        IEnumerable<ControlTokenIdentity> requiredTokenOwners,
        IEnumerable<ControlThemeBindingDescriptor> semanticThemeBindings,
        in SchemaPrefixState contractFingerprintPrefix,
        in SchemaPrefixState resourceKeyFingerprintPrefix,
        ulong compiledContractFingerprint)
        : this(assetId, assetUri, exportedThemes, requiredTokenOwners, semanticThemeBindings, 0)
    {
        ValidateGeneratedContract(this, contractFingerprintPrefix, compiledContractFingerprint);
        // Full runtime Type identities are materialized only for this selected descriptor. The
        // registry later checks this fingerprint independently against its actual global schema.
        ResourceKeySchemaFingerprint = ComputeResourceKeyFingerprint(this, resourceKeyFingerprintPrefix);
    }

    /// <summary>
    /// Materializes a generated asset whose exports/owners/bindings are already emitted in the exact
    /// order the public constructor's canonical sort would produce. Ordering is asserted in O(n) and
    /// violations throw, turning a generator ordering bug into an explicit startup failure instead of
    /// a silently wrong schema. Bit-identical output to the sorting <c>CreateGenerated</c> overloads.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static ControlThemeAssetDescriptor CreateGeneratedPreSorted(
        string assetId,
        Uri assetUri,
        IEnumerable<ControlThemeExportDescriptor> exportedThemes,
        IEnumerable<ControlTokenIdentity> requiredTokenOwners,
        IEnumerable<ControlThemeBindingDescriptor> semanticThemeBindings,
        in SchemaPrefixState contractFingerprintPrefix,
        in SchemaPrefixState resourceKeyFingerprintPrefix,
        ulong compiledContractFingerprint) => new(assetId, assetUri, exportedThemes, requiredTokenOwners,
            semanticThemeBindings, contractFingerprintPrefix, resourceKeyFingerprintPrefix, compiledContractFingerprint,
            presorted: true);

    private ControlThemeAssetDescriptor(
        string assetId,
        Uri assetUri,
        IEnumerable<ControlThemeExportDescriptor> exportedThemes,
        IEnumerable<ControlTokenIdentity> requiredTokenOwners,
        IEnumerable<ControlThemeBindingDescriptor> semanticThemeBindings,
        in SchemaPrefixState contractFingerprintPrefix,
        in SchemaPrefixState resourceKeyFingerprintPrefix,
        ulong compiledContractFingerprint,
        bool presorted)
        : this(assetId, assetUri, exportedThemes, requiredTokenOwners, semanticThemeBindings, 0, presorted)
    {
        ValidateGeneratedContract(this, contractFingerprintPrefix, compiledContractFingerprint);
        ResourceKeySchemaFingerprint = ComputeResourceKeyFingerprint(this, resourceKeyFingerprintPrefix);
    }

    private static void ValidateGeneratedContract(
        ControlThemeAssetDescriptor descriptor,
        in SchemaPrefixState contractFingerprintPrefix,
        ulong compiledContractFingerprint)
    {
        var fingerprint = SchemaFingerprintBuilder.Restore(contractFingerprintPrefix);
        ThemeSchemaRegistry.AddGeneratedContractAssetSchema(ref fingerprint, descriptor);
        if (fingerprint.Value != compiledContractFingerprint)
        {
            throw new ThemeSchemaException(
                $"Control theme asset '{descriptor.AssetUri}' has stale generated contract evidence. " +
                "The containing package was likely compiled with an incompatible AtomUI generator version; " +
                "rebuild it against the current AtomUI.Core.");
        }
    }

    private static ulong ComputeResourceKeyFingerprint(
        ControlThemeAssetDescriptor descriptor,
        in SchemaPrefixState resourceKeyFingerprintPrefix)
    {
        var fingerprint = SchemaFingerprintBuilder.Restore(resourceKeyFingerprintPrefix);
        ThemeSchemaRegistry.AddAssetSchemaCore(ref fingerprint, descriptor);
        return fingerprint.Value;
    }

    // Presorted helpers mirror the public constructor's canonical comparers exactly; they assert the
    // generated emission order instead of re-establishing it.
    private static int CompareExports(ControlThemeExportDescriptor left, ControlThemeExportDescriptor right)
    {
        var result = string.CompareOrdinal(left.TargetType.AssemblyQualifiedName, right.TargetType.AssemblyQualifiedName);
        if (result != 0)
        {
            return result;
        }
        result = (left.ResourceKey is Type ? 0 : 1).CompareTo(right.ResourceKey is Type ? 0 : 1);
        if (result != 0)
        {
            return result;
        }
        return string.CompareOrdinal(
            left.ResourceKey is Type leftType ? leftType.AssemblyQualifiedName : (string)left.ResourceKey,
            right.ResourceKey is Type rightType ? rightType.AssemblyQualifiedName : (string)right.ResourceKey);
    }

    private static int CompareBindings(ControlThemeBindingDescriptor left, ControlThemeBindingDescriptor right)
    {
        var result = CompareIdentityKeys(left.OwnerIdentity, right.OwnerIdentity);
        if (result != 0)
        {
            return result;
        }
        result = string.CompareOrdinal(left.PropertyName, right.PropertyName);
        if (result != 0)
        {
            return result;
        }
        return string.CompareOrdinal(left.TargetType.AssemblyQualifiedName, right.TargetType.AssemblyQualifiedName);
    }

    private static int CompareIdentityKeys(ControlTokenIdentity left, ControlTokenIdentity right)
    {
        var result = string.CompareOrdinal(left.Catalog, right.Catalog);
        return result != 0 ? result : string.CompareOrdinal(left.Id, right.Id);
    }

    private static void EnsurePresorted<T>(T[] items, Func<T, T, int> compare, string kind, string assetId)
    {
        for (var index = 1; index < items.Length; index++)
        {
            if (compare(items[index - 1], items[index]) > 0)
            {
                throw new ThemeSchemaException(
                    $"Generated theme asset '{assetId}' has {kind} outside the compiled schema order.");
            }
        }
    }

    private static ControlTokenIdentity[] CanonicalizePresortedOwners(IEnumerable<ControlTokenIdentity> requiredTokenOwners)
    {
        var owners = new List<ControlTokenIdentity>();
        foreach (var identity in requiredTokenOwners)
        {
            if (owners.Count > 0)
            {
                var compare = CompareIdentityKeys(owners[^1], identity);
                if (compare == 0)
                {
                    owners[^1] = ControlTokenIdentityCanonicalizer.Merge(owners[^1], identity);
                    continue;
                }
                if (compare > 0)
                {
                    throw new ThemeSchemaException(
                        $"Generated theme asset has required Token owners outside the compiled schema order near '{identity}'.");
                }
            }
            owners.Add(identity);
        }
        return owners.ToArray();
    }

    // Owners and bindings are both ascending by owner identity here, so the merge validation of the
    // sorting path (dictionary accumulation) becomes a two-pointer walk plus adjacent-pair merges.
    private static void ValidatePresortedBindingOwners(
        ControlTokenIdentity[] owners,
        ControlThemeBindingDescriptor[] bindings)
    {
        var ownerIndex = 0;
        var previous = default(ControlTokenIdentity);
        var hasPrevious = false;
        foreach (var binding in bindings)
        {
            var owner = binding.OwnerIdentity;
            if (hasPrevious && CompareIdentityKeys(previous, owner) == 0)
            {
                ControlTokenIdentityCanonicalizer.Merge(previous, owner);
            }
            previous = owner;
            hasPrevious = true;
            while (ownerIndex < owners.Length && CompareIdentityKeys(owners[ownerIndex], owner) < 0)
            {
                ownerIndex++;
            }
            if (ownerIndex < owners.Length && CompareIdentityKeys(owners[ownerIndex], owner) == 0)
            {
                ControlTokenIdentityCanonicalizer.Merge(owners[ownerIndex], owner);
            }
        }
    }

    internal bool HasSameMetadata(ControlThemeAssetDescriptor other) =>
        AssetId == other.AssetId && AssetUri == other.AssetUri &&
        ResourceKeySchemaFingerprint == other.ResourceKeySchemaFingerprint &&
        ExportedThemes.SequenceEqual(other.ExportedThemes) &&
        RequiredTokenOwners.Select(static identity => (identity, identity.OwnerType))
            .SequenceEqual(other.RequiredTokenOwners.Select(static identity => (identity, identity.OwnerType))) &&
        SemanticThemeBindings.SequenceEqual(other.SemanticThemeBindings) &&
        SemanticThemeBindings.Select(static binding => binding.OwnerIdentity.OwnerType)
            .SequenceEqual(other.SemanticThemeBindings.Select(static binding => binding.OwnerIdentity.OwnerType));

    public string AssetId { get; }
    public IReadOnlyList<ControlThemeExportDescriptor> ExportedThemes { get; }
    public IReadOnlyList<ControlTokenIdentity> RequiredTokenOwners { get; }
    public IReadOnlyList<ControlThemeBindingDescriptor> SemanticThemeBindings { get; }

    public Uri AssetUri { get; }
    public ulong ResourceKeySchemaFingerprint { get; }
}
