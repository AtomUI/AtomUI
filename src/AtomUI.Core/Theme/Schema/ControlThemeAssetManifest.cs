namespace AtomUI.Theme.Schema;

internal sealed class ControlThemeAssetManifest
{
    internal ControlThemeAssetManifest(
        ThemeSchemaRegistry registry,
        IEnumerable<ControlThemeAssetDescriptor> descriptors)
    {
        ArgumentNullException.ThrowIfNull(registry);
        ArgumentNullException.ThrowIfNull(descriptors);

        var ordered = descriptors.OrderBy(
            static descriptor => descriptor.AssetUri.ToString(),
            StringComparer.Ordinal).ToArray();
        var uris = new HashSet<string>(StringComparer.Ordinal);
        var ids = new HashSet<string>(StringComparer.Ordinal);
        var exports = new Dictionary<object, string>();
        foreach (var descriptor in ordered)
        {
            ArgumentNullException.ThrowIfNull(descriptor);
            if (!descriptor.AssetUri.IsAbsoluteUri)
            {
                throw new ThemeSchemaException(
                    $"Control theme asset URI '{descriptor.AssetUri}' must be absolute.");
            }
            if (descriptor.ResourceKeySchemaFingerprint == 0)
            {
                throw new ThemeSchemaException(
                    $"Control theme asset '{descriptor.AssetUri}' has an empty resource-key schema fingerprint.");
            }
            var expectedFingerprint = registry.ComputeRegisteredResourceKeySchemaFingerprint(descriptor);
            if (descriptor.ResourceKeySchemaFingerprint != expectedFingerprint)
            {
                throw new ThemeSchemaException(
                    $"Control theme asset '{descriptor.AssetUri}' was compiled against a different resource-key schema.");
            }
            if (!uris.Add(descriptor.AssetUri.ToString()))
            {
                throw new ThemeSchemaException(
                    $"Control theme asset URI '{descriptor.AssetUri}' is registered more than once.");
            }

            if (!ids.Add(descriptor.AssetId))
            {
                throw new ThemeSchemaException($"Control theme asset '{descriptor.AssetId}' is registered more than once.");
            }
            foreach (var export in descriptor.ExportedThemes)
            {
                if (!exports.TryAdd(export.ResourceKey, descriptor.AssetId))
                {
                    throw new ThemeSchemaException($"Theme export '{export.ResourceKey}' conflicts between '{exports[export.ResourceKey]}' and '{descriptor.AssetId}'.");
                }
            }
            var identities = new HashSet<ControlTokenIdentity>();
            foreach (var identity in descriptor.RequiredTokenOwners)
            {
                if (!identities.Add(identity))
                {
                    throw new ThemeSchemaException(
                        $"Control theme asset '{descriptor.AssetUri}' declares duplicate Control identity " +
                        $"'{identity}'.");
                }
                if (!registry.TryGetControl(identity, out _))
                {
                    throw new ThemeSchemaException(
                        $"Control theme asset '{descriptor.AssetUri}' references unregistered identity " +
                        $"'{identity}'.");
                }
            }
        }

        Descriptors = Array.AsReadOnly(ordered);
    }

    internal IReadOnlyList<ControlThemeAssetDescriptor> Descriptors { get; }
}
