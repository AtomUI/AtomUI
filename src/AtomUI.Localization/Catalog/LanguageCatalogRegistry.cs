using System.Collections.ObjectModel;

namespace AtomUI.Localization;

internal sealed class LanguageCatalogRegistry
{
    private readonly ReadOnlyCollection<LanguageCatalogDescriptor> _catalogs;
    private readonly IReadOnlyList<TranslationBundleDescriptor>[] _bundlesByCatalog;
    private readonly Dictionary<Type, int> _catalogSlotsByResourceType;
    private readonly Dictionary<string, int> _catalogSlotsById;

    private LanguageCatalogRegistry(
        LanguageCatalogDescriptor[] catalogs,
        IReadOnlyList<TranslationBundleDescriptor>[] bundlesByCatalog,
        Dictionary<Type, int> catalogSlotsByResourceType,
        Dictionary<string, int> catalogSlotsById)
    {
        _catalogs = Array.AsReadOnly(catalogs);
        _bundlesByCatalog = bundlesByCatalog;
        _catalogSlotsByResourceType = catalogSlotsByResourceType;
        _catalogSlotsById = catalogSlotsById;
    }

    internal IReadOnlyList<LanguageCatalogDescriptor> Catalogs => _catalogs;

    internal static LanguageCatalogRegistry Create(
        IReadOnlyList<LanguageCatalogDescriptor> catalogInputs,
        IReadOnlyList<TranslationBundleDescriptor> bundleInputs)
    {
        ArgumentNullException.ThrowIfNull(catalogInputs);
        ArgumentNullException.ThrowIfNull(bundleInputs);

        var catalogs = catalogInputs.OrderBy(static catalog => catalog.CatalogId, StringComparer.Ordinal)
                                    .ToArray();
        var slotsByType = new Dictionary<Type, int>();
        var slotsById = new Dictionary<string, int>(StringComparer.Ordinal);
        for (var slot = 0; slot < catalogs.Length; slot++)
        {
            var catalog = catalogs[slot];
            if (!slotsById.TryAdd(catalog.CatalogId, slot))
            {
                throw new LanguageCatalogException(
                    $"Catalog ID '{catalog.CatalogId}' is registered more than once. " +
                    "Each generated Catalog ID must be unique.");
            }
            if (!slotsByType.TryAdd(catalog.ResourceKindType, slot))
            {
                throw new LanguageCatalogException(
                    $"Resource enum type '{catalog.ResourceKindType.FullName}' is registered by more than one Catalog.");
            }
        }

        var bundles = new List<TranslationBundleDescriptor>[catalogs.Length];
        for (var slot = 0; slot < bundles.Length; slot++)
        {
            bundles[slot] = [];
        }

        foreach (var bundle in bundleInputs)
        {
            if (!slotsById.TryGetValue(bundle.CatalogId, out var catalogSlot))
            {
                throw new LanguageCatalogException(
                    $"Translation bundle from '{bundle.SourceIdentity}' targets Catalog " +
                    $"'{bundle.CatalogId}', which is not registered.");
            }

            var catalog = catalogs[catalogSlot];
            if (bundle.Values.Count != catalog.Units.Count)
            {
                throw new LanguageCatalogException(
                    $"Translation bundle from '{bundle.SourceIdentity}' supplies {bundle.Values.Count} value slots " +
                    $"for Catalog '{catalog.CatalogId}', which requires {catalog.Units.Count}.");
            }

            bundles[catalogSlot].Add(bundle);
        }

        var readOnlyBundles = new IReadOnlyList<TranslationBundleDescriptor>[catalogs.Length];
        for (var catalogSlot = 0; catalogSlot < catalogs.Length; catalogSlot++)
        {
            var catalog = catalogs[catalogSlot];
            var orderedBundles = bundles[catalogSlot]
                                 .OrderBy(static bundle => bundle.Language.Value, StringComparer.Ordinal)
                                 .ThenByDescending(static bundle => bundle.SourceKind)
                                 .ThenBy(static bundle => bundle.SourceIdentity, StringComparer.Ordinal)
                                 .ToArray();
            ValidateTranslations(catalog, orderedBundles);
            readOnlyBundles[catalogSlot] = Array.AsReadOnly(orderedBundles);
        }

        return new LanguageCatalogRegistry(catalogs, readOnlyBundles, slotsByType, slotsById);
    }

    internal bool TryGetCatalogSlot(Type resourceKindType, out int catalogSlot)
    {
        ArgumentNullException.ThrowIfNull(resourceKindType);
        return _catalogSlotsByResourceType.TryGetValue(resourceKindType, out catalogSlot);
    }

    internal bool TryGetCatalogSlot(string catalogId, out int catalogSlot)
    {
        ArgumentNullException.ThrowIfNull(catalogId);
        return _catalogSlotsById.TryGetValue(catalogId, out catalogSlot);
    }

    internal IReadOnlyList<TranslationBundleDescriptor> GetBundles(int catalogSlot)
    {
        if ((uint)catalogSlot >= (uint)_bundlesByCatalog.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(catalogSlot));
        }

        return _bundlesByCatalog[catalogSlot];
    }

    private static void ValidateTranslations(
        LanguageCatalogDescriptor catalog,
        IReadOnlyList<TranslationBundleDescriptor> bundles)
    {
        var owners = new Dictionary<(LanguageTag, TranslationSourceKind, int), TranslationBundleDescriptor>();
        var englishCoverage = new bool[catalog.Units.Count];
        foreach (var bundle in bundles)
        {
            for (var unitSlot = 0; unitSlot < bundle.Values.Count; unitSlot++)
            {
                if (bundle.Values[unitSlot] is null)
                {
                    continue;
                }

                var key = (bundle.Language, bundle.SourceKind, unitSlot);
                if (!owners.TryAdd(key, bundle))
                {
                    var existing = owners[key];
                    var unit = catalog.Units[unitSlot];
                    throw new LanguageCatalogException(
                        $"Catalog '{catalog.CatalogId}' unit '{unit.Key}' has conflicting " +
                        $"'{bundle.Language.Value}' translations at priority '{bundle.SourceKind}' from " +
                        $"'{existing.SourceIdentity}' and '{bundle.SourceIdentity}'. Remove one source or use an " +
                        "explicit application override.");
                }

                if (bundle.Language == LanguageTags.EnUS)
                {
                    englishCoverage[unitSlot] = true;
                }
            }
        }
        // Report conflicts before missing source text, preserving deterministic validation order.
        for (var unitSlot = 0; unitSlot < catalog.Units.Count; unitSlot++)
        {
            if (!englishCoverage[unitSlot])
            {
                var unit = catalog.Units[unitSlot];
                throw new LanguageCatalogException(
                    $"Catalog '{catalog.CatalogId}' is missing required en-US source text for unit " +
                    $"'{unit.Key}'. Add the unit to the Catalog's en-US XLIFF source.");
            }
        }
    }
}
