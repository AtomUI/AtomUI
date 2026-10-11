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
        var owners = ControlTokenIdentityCanonicalizer.Canonicalize(requiredTokenOwners);
        if (owners.Any(static identity => identity.OwnerType is null))
        {
            throw new ArgumentException("Required Token owners must carry their actual Control type.", nameof(requiredTokenOwners));
        }
        // Validate owner evidence across bindings before any metadata deduplication.
        if (bindings.Length > 0)
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
        ExportedThemes = Array.AsReadOnly(exports.OrderBy(static export => export.TargetType.AssemblyQualifiedName, StringComparer.Ordinal)
            .ThenBy(static export => export.ResourceKey is Type ? 0 : 1)
            .ThenBy(static export => export.ResourceKey is Type type ? type.AssemblyQualifiedName : (string)export.ResourceKey, StringComparer.Ordinal).ToArray());
        RequiredTokenOwners = Array.AsReadOnly(owners);
        SemanticThemeBindings = Array.AsReadOnly(bindings.Distinct().OrderBy(static binding => binding.OwnerIdentity.Catalog, StringComparer.Ordinal)
            .ThenBy(static binding => binding.OwnerIdentity.Id, StringComparer.Ordinal)
            .ThenBy(static binding => binding.PropertyName, StringComparer.Ordinal)
            .ThenBy(static binding => binding.TargetType.AssemblyQualifiedName, StringComparer.Ordinal).ToArray());
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
        if (ThemeSchemaRegistry.ComputeGeneratedContractFingerprint(this, compiledGlobalTokenNames) != compiledContractFingerprint)
        {
            throw new ThemeSchemaException($"Control theme asset '{AssetUri}' has stale generated contract evidence.");
        }
        // Full runtime Type identities are materialized only for this selected descriptor. The
        // registry later checks this fingerprint independently against its actual global schema.
        ResourceKeySchemaFingerprint = ThemeSchemaRegistry.ComputeGeneratedResourceKeySchemaFingerprint(
            this,
            compiledGlobalTokenNames);
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
