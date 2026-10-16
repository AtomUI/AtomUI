using System.Text;

namespace AtomUI.Localization;

internal static class LanguageSnapshotBuilder
{
    private static readonly TranslationSourceKind[] SourcePriorities =
    [
        TranslationSourceKind.ApplicationOverride,
        TranslationSourceKind.StaticLanguagePack,
        TranslationSourceKind.ModuleBuiltIn
    ];

    internal static LanguageSnapshot Build(
        LanguageCatalogRegistry registry,
        LanguageTag requestedLanguage,
        LanguageDefinition definition,
        LanguageSnapshot? previous = null,
        bool deferCatalogs = false)
    {
        ArgumentNullException.ThrowIfNull(registry);
        ArgumentNullException.ThrowIfNull(definition);
        if (requestedLanguage == default)
        {
            throw new ArgumentException("A valid requested language is required.", nameof(requestedLanguage));
        }
        if (definition.Tag != requestedLanguage)
        {
            throw new LanguageConfigurationException(
                $"Language definition '{definition.Tag.Value}' cannot build Snapshot '{requestedLanguage.Value}'.");
        }

        var candidates = LanguageFallbackResolver.Resolve(
            requestedLanguage,
            definition.FormattingCulture);
        var entries = new LanguageSnapshotEntry[registry.Catalogs.Count][];
        var canReuse = previous?.RequestedLanguage == requestedLanguage &&
                       previous.FallbackCandidates is { } previousCandidates &&
                       previousCandidates.SequenceEqual(candidates);

        for (var catalogSlot = 0; catalogSlot < registry.Catalogs.Count; catalogSlot++)
        {
            var catalog = registry.Catalogs[catalogSlot];
            var bundles = registry.GetBundles(catalogSlot);
            if (canReuse && previous!.Registry is { } previousRegistry &&
                previousRegistry.HasSameCatalog(catalog, bundles, out var previousSlot))
            {
                // 行是不可变数组；按身份映射旧 slot，只共享该行，不保留整张旧快照。
                entries[catalogSlot] = deferCatalogs
                    ? previous.GetMaterializedCatalogEntries(previousSlot)!
                    : previous.GetCatalogEntries(previousSlot);
                continue;
            }
            if (!deferCatalogs)
            {
                entries[catalogSlot] = BuildCatalog(registry, catalogSlot, requestedLanguage, candidates);
            }
        }

        return new LanguageSnapshot(requestedLanguage, entries, registry, candidates);
    }

    internal static LanguageSnapshotEntry[] BuildCatalog(LanguageCatalogRegistry registry, int catalogSlot,
        LanguageTag requestedLanguage, IReadOnlyList<LanguageTag> candidates)
    {
        var catalog = registry.Catalogs[catalogSlot];
        var bundles = registry.GetBundles(catalogSlot);
        var requiresTranslatedCoverage = !LanguageFallbackResolver.HasEnglishPrimaryLanguage(requestedLanguage);
        var catalogEntries = new LanguageSnapshotEntry[catalog.Units.Count];
        var resolver = new MatchedBundleResolver(bundles, candidates);
        for (var unitSlot = 0; unitSlot < catalog.Units.Count; unitSlot++)
        {
            if (!resolver.TryResolveValue(
                    unitSlot,
                    out var text,
                    out var resolvedLanguage,
                    out var sourceIdentity))
            {
                var unit = catalog.Units[unitSlot];
                throw new LanguageCatalogException(
                    $"Catalog '{catalog.CatalogId}' unit '{unit.Key}' has no value for " +
                    $"requested language '{requestedLanguage.Value}' or required en-US source.");
            }

            var unitDescriptor = catalog.Units[unitSlot];
            if (requiresTranslatedCoverage && resolvedLanguage == LanguageTags.EnUS)
            {
                throw new LanguageCoverageException(
                    $"Supported language '{requestedLanguage.Value}' resolves Catalog '{catalog.CatalogId}' " +
                    $"unit '{unitDescriptor.Key}' only through final en-US fallback. " +
                    "Add a translation for the exact language or a valid parent candidate.");
            }

            CompositeFormat? compositeFormat = null;
            if (unitDescriptor.IsFormatted)
            {
                try
                {
                    compositeFormat = CompositeFormat.Parse(text);
                }
                catch (FormatException exception)
                {
                    throw new LanguageCatalogException(
                        $"Compiled translation from '{sourceIdentity}' for Catalog '{catalog.CatalogId}' unit " +
                        $"'{unitDescriptor.Key}' and language " +
                        $"'{resolvedLanguage.Value}' has an invalid CompositeFormat.",
                        exception);
                }
            }

            catalogEntries[unitSlot] = new LanguageSnapshotEntry(
                text,
                resolvedLanguage,
                compositeFormat);
        }

        return catalogEntries;
    }

    internal static void ValidateManualCoverage(LanguageCatalogRegistry registry, LanguageDefinition definition, bool generatedDataVerified = false)
    {
        var requiresTranslatedCoverage = !LanguageFallbackResolver.HasEnglishPrimaryLanguage(definition.Tag);
        var candidates = LanguageFallbackResolver.Resolve(definition.Tag, definition.FormattingCulture);
        for (var catalogSlot = 0; catalogSlot < registry.Catalogs.Count; catalogSlot++)
        {
            var catalog = registry.Catalogs[catalogSlot];
            var bundles = registry.GetBundles(catalogSlot);
            // Skip only with a proof for this exact module set and supported languages. A referenced
            // but disabled language pack must not make missing runtime translations appear covered.
            if (generatedDataVerified && catalog is LanguageDataCatalogDescriptor && bundles.All(static bundle => bundle.IsModuleData))
            {
                continue;
            }
            var resolver = new MatchedBundleResolver(bundles, candidates);
            for (var slot = 0; slot < catalog.Units.Count; slot++)
            {
                if (!resolver.TryResolveValue(slot, out var text, out var language, out var sourceIdentity) ||
                    (requiresTranslatedCoverage && language == LanguageTags.EnUS))
                {
                    throw new LanguageCoverageException(
                        $"Supported language '{definition.Tag.Value}' resolves Catalog '{catalog.CatalogId}' " +
                        $"unit '{catalog.Units[slot].Key}' only through final en-US fallback.");
                }
                if (catalog.Units[slot].IsFormatted)
                {
                    try
                    {
                        CompositeFormat.Parse(text);
                    }
                    catch (FormatException exception)
                    {
                        throw new LanguageCatalogException(
                            $"Compiled translation from '{sourceIdentity}' for Catalog '{catalog.CatalogId}' unit " +
                            $"'{catalog.Units[slot].Key}' and language '{language.Value}' has an invalid CompositeFormat.",
                            exception);
                    }
                }
            }
        }
    }

    // Expand the ordered match prefix only when a unit cannot use a previously matched bundle.
    // The first match stays inline: immediate hits need neither a list nor scans of later bundles.
    private struct MatchedBundleResolver(
        IReadOnlyList<TranslationBundleDescriptor> bundles,
        IReadOnlyList<LanguageTag> candidates)
    {
        private readonly IReadOnlyList<TranslationBundleDescriptor> _bundles = bundles;
        private readonly IReadOnlyList<LanguageTag> _candidates = candidates;
        private TranslationBundleDescriptor? _firstMatch;
        private List<TranslationBundleDescriptor>? _additionalMatches;
        private int _sourceIndex;
        private int _candidateIndex;
        private int _bundleIndex;
        private bool _isEnglishFallback;

        internal bool TryResolveValue(
            int unitSlot,
            out string text,
            out LanguageTag resolvedLanguage,
            out string sourceIdentity)
        {
            if (_firstMatch is { } first && first.Values[unitSlot] is { } firstValue)
            {
                text = firstValue;
                resolvedLanguage = first.Language;
                sourceIdentity = first.SourceIdentity;
                return true;
            }

            if (_additionalMatches is { } matches)
            {
                for (var index = 0; index < matches.Count; index++)
                {
                    var bundle = matches[index];
                    if (bundle.Values[unitSlot] is { } value)
                    {
                        text = value;
                        resolvedLanguage = bundle.Language;
                        sourceIdentity = bundle.SourceIdentity;
                        return true;
                    }
                }
            }

            while (TryGetNextMatch() is { } next)
            {
                if (_firstMatch is null)
                {
                    _firstMatch = next;
                }
                else
                {
                    (_additionalMatches ??= []).Add(next);
                }

                if (next.Values[unitSlot] is { } value)
                {
                    text = value;
                    resolvedLanguage = next.Language;
                    sourceIdentity = next.SourceIdentity;
                    return true;
                }
            }

            text = string.Empty;
            resolvedLanguage = default;
            sourceIdentity = string.Empty;
            return false;
        }

        private TranslationBundleDescriptor? TryGetNextMatch()
        {
            while (true)
            {
                if (_sourceIndex == SourcePriorities.Length)
                {
                    if (_isEnglishFallback)
                    {
                        return null;
                    }
                    _isEnglishFallback = true;
                    _sourceIndex = 0;
                    _candidateIndex = 0;
                }

                if (!_isEnglishFallback && _candidateIndex == _candidates.Count)
                {
                    _sourceIndex++;
                    _candidateIndex = 0;
                    continue;
                }

                var language = _isEnglishFallback ? LanguageTags.EnUS : _candidates[_candidateIndex];
                if (!_isEnglishFallback && language == LanguageTags.EnUS)
                {
                    _candidateIndex++;
                    continue;
                }

                var sourceKind = SourcePriorities[_sourceIndex];
                while (_bundleIndex < _bundles.Count)
                {
                    var bundle = _bundles[_bundleIndex++];
                    if (bundle.Language == language && bundle.SourceKind == sourceKind)
                    {
                        return bundle;
                    }
                }

                _bundleIndex = 0;
                if (_isEnglishFallback)
                {
                    _sourceIndex++;
                }
                else
                {
                    _candidateIndex++;
                }
            }
        }
    }
}
