using System.Collections.ObjectModel;
using System.Collections.Concurrent;

namespace AtomUI.Localization;

internal sealed class LanguageCatalogRegistry
{
    private const int NoCatalogSlot = -1;

    private readonly ReadOnlyCollection<LanguageCatalogDescriptor> _catalogs;
    private readonly IReadOnlyList<TranslationBundleDescriptor>[] _bundlesByCatalog;
    private readonly Dictionary<Type, int> _catalogSlotsByResourceType;
    private readonly Dictionary<string, int> _catalogSlotsById;
    private readonly Dictionary<(string Assembly, string Name), int> _dataSlots = new();
    private readonly HashSet<string> _dataMetadataNames = new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<Type, int> _resolvedDataTypes = new();

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
        for (var slot = 0; slot < catalogs.Length; slot++)
        {
            if (catalogs[slot] is LanguageDataCatalogDescriptor data)
            {
                if (!_dataSlots.TryAdd((data.AssemblyName, data.MetadataName), slot))
                {
                    throw new LanguageCatalogException($"Resource enum '{data.MetadataName}' is registered more than once.");
                }
                _dataMetadataNames.Add(data.MetadataName);
            }
        }
        foreach (var resourceType in catalogSlotsByResourceType.Keys)
        {
            if (resourceType.FullName is { } name && resourceType.Assembly.GetName().Name is { } assembly &&
                _dataSlots.ContainsKey((assembly, name)))
            {
                throw new LanguageCatalogException(
                    $"Resource enum type '{name}' is registered by both a manual Catalog and module data.");
            }
        }
    }

    internal IReadOnlyList<LanguageCatalogDescriptor> Catalogs => _catalogs;

    /// <summary>
    /// Builds the registry from catalogs and translation bundles. A bundle must target a registered catalog,
    /// also when the bundle comes from another package; otherwise this throws.
    /// </summary>
    internal static LanguageCatalogRegistry Create(
        IReadOnlyList<LanguageCatalogDescriptor> catalogInputs,
        IReadOnlyList<TranslationBundleDescriptor> bundleInputs,
        LanguageCatalogRegistry? previous = null, bool generatedDataVerified = false)
    {
        ArgumentNullException.ThrowIfNull(catalogInputs);
        ArgumentNullException.ThrowIfNull(bundleInputs);

        var catalogs = previous is null
            ? catalogInputs.OrderBy(static catalog => catalog.CatalogId, StringComparer.Ordinal).ToArray()
            : catalogInputs.OrderBy(catalog => previous.TryGetCatalogSlot(catalog.CatalogId, out var slot) ? slot : int.MaxValue)
                .ThenBy(catalog => catalog.CatalogId, StringComparer.Ordinal).ToArray();
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
            if (catalog is not LanguageDataCatalogDescriptor && !slotsByType.TryAdd(catalog.ResourceKindType, slot))
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
            // Descriptors and bundles are frozen when they are constructed. The same input needs no second per-unit
            // validation of conflicts and en-US coverage.
            if ((!generatedDataVerified || catalog is not LanguageDataCatalogDescriptor || orderedBundles.Any(static bundle => !bundle.IsModuleData)) &&
                (previous is null || !previous.HasSameCatalog(catalog, orderedBundles, out _)))
            {
                ValidateTranslations(catalog, orderedBundles);
            }
            readOnlyBundles[catalogSlot] = Array.AsReadOnly(orderedBundles);
        }

        return new LanguageCatalogRegistry(catalogs, readOnlyBundles, slotsByType, slotsById);
    }

    internal bool TryGetCatalogSlot(Type resourceKindType, out int catalogSlot)
    {
        ArgumentNullException.ThrowIfNull(resourceKindType);
        if (_catalogSlotsByResourceType.TryGetValue(resourceKindType, out catalogSlot))
        {
            return true;
        }
        // Both manual and module data catalogs use enum keys. A key type such as Type or string cannot match, so
        // there is no need to resolve an assembly name.
        if (!resourceKindType.IsEnum)
        {
            catalogSlot = NoCatalogSlot;
            return false;
        }
        // The registry does not change after it is frozen, so hits and misses are both cached by type.
        // Non-language keys such as token enums are resolved only once.
        if (!_resolvedDataTypes.TryGetValue(resourceKindType, out catalogSlot))
        {
            catalogSlot = ResolveDataCatalogSlot(resourceKindType);
            _resolvedDataTypes.TryAdd(resourceKindType, catalogSlot);
        }
        return catalogSlot != NoCatalogSlot;
    }

    private int ResolveDataCatalogSlot(Type resourceKindType)
    {
        // Filter by metadata name first, so only candidate types need to allocate an AssemblyName.
        if (resourceKindType.FullName is { } name && _dataMetadataNames.Contains(name) &&
            resourceKindType.Assembly.GetName().Name is { } assembly &&
            _dataSlots.TryGetValue((assembly, name), out var slot))
        {
            return slot;
        }
        return NoCatalogSlot;
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

    internal bool HasSameCatalog(LanguageCatalogDescriptor catalog,
        IReadOnlyList<TranslationBundleDescriptor> bundles, out int slot)
    {
        return TryGetCatalogSlot(catalog.CatalogId, out slot) &&
               ReferenceEquals(Catalogs[slot], catalog) &&
               _bundlesByCatalog[slot].SequenceEqual(bundles);
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
                if (!bundle.HasValue(unitSlot))
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
