using AtomUI.Build.Tasks.LocalizationBuild;
using AtomUI.Generator.Localization;
using AtomUI.Localization.Data;

namespace AtomUI.Generator;

internal static class StartupLanguageLinker
{
    internal static SortedDictionary<string, string> Validate(IEnumerable<StartupReferenceData> references,
        LocalizationGenerationResult localization, IEnumerable<string> supportedLanguages, Action<string>? checkingLanguage = null)
    {
        var encoded = new SortedDictionary<string, string>(StringComparer.Ordinal);
        var origins = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var reference in references)
        {
            foreach (var language in reference.Languages)
            {
                if (language.Version != LanguageModuleDataCodec.Version)
                {
                    throw new InvalidDataException("Unsupported language manifest '" + language.ModuleId + "' from '" + reference.AssemblyName + "'.");
                }
                Add(language.ModuleId, language.Data, reference.AssemblyName);
            }
        }
        var own = localization.Plan.Catalogs.Where(catalog => catalog.OwnsCatalog)
            .OrderBy(catalog => catalog.Catalog.CatalogId, StringComparer.Ordinal).ToArray();
        if (own.Length != 0)
        {
            var moduleId = own[0].Catalog.ModuleId;
            Add(moduleId, LanguageModuleSourceWriter.Encode(localization.AssemblyName, moduleId, own, true), localization.AssemblyName ?? "Application");
        }
        if (localization.Plan.Catalogs.Any(catalog => catalog.Bundles.Any(bundle =>
                bundle.SourceKind != Localization.Xliff.LanguageFileSourceKind.ModuleBuiltIn)))
        {
            var moduleId = localization.AssemblyName + ":ApplicationTranslations";
            Add(moduleId, LanguageModuleSourceWriter.Encode(localization.AssemblyName, moduleId, localization.Plan.Catalogs, false), localization.AssemblyName ?? "Application");
        }
        var modules = encoded.Select(pair => StartupManifestDecoder.Read("Language module", pair.Key, origins[pair.Key],
            pair.Value, data => LanguageModuleDataCodec.Read(pair.Key, data))).ToArray();
        var catalogs = new Dictionary<string, LanguageModuleCatalog>(StringComparer.Ordinal);
        var enums = new HashSet<string>(StringComparer.Ordinal);
        foreach (var catalog in modules.SelectMany(module => module.Catalogs))
        {
            if (catalogs.ContainsKey(catalog.CatalogId) || !enums.Add(catalog.AssemblyName + "|" + catalog.MetadataName))
            {
                throw new InvalidDataException("Conflicting language catalog '" + catalog.CatalogId + "'.");
            }
            catalogs.Add(catalog.CatalogId, catalog);
        }
        var bundles = modules.SelectMany(module => module.Bundles).ToArray();
        var owners = new HashSet<string>(StringComparer.Ordinal);
        foreach (var bundle in bundles)
        {
            if (!Bcp47LanguageTagParser.TryParse(bundle.Language, out var canonical) || canonical != bundle.Language ||
                !catalogs.TryGetValue(bundle.CatalogId, out var catalog) || bundle.Values.Count != catalog.Units.Length)
            {
                throw new InvalidDataException("Unresolved or incompatible translation bundle '" + bundle.CatalogId + "/" + bundle.Language + "' from '" + bundle.SourceIdentity + "'.");
            }
            for (var slot = 0; slot < bundle.Values.Count; slot++)
            {
                if (!bundle.Values.HasValue(slot))
                {
                    continue;
                }
                if (!owners.Add(bundle.CatalogId + "|" + bundle.Language + "|" + bundle.SourceKind + "|" + slot))
                {
                    throw new InvalidDataException("Conflicting translation '" + bundle.CatalogId + "/" + catalog.Units[slot].Key + "/" + bundle.Language + "'.");
                }
                if (catalog.Units[slot].IsFormatted && !StartupCompositeFormat.IsValid(bundle.Values[slot]!))
                {
                    throw new InvalidDataException("Invalid CompositeFormat in '" + bundle.SourceIdentity + "', catalog '" + bundle.CatalogId + "', unit '" + catalog.Units[slot].Key + "', language '" + bundle.Language + "'.");
                }
            }
        }
        foreach (var catalog in catalogs.Values)
        {
            var catalogBundles = bundles.Where(bundle => bundle.CatalogId == catalog.CatalogId).ToArray();
            for (var slot = 0; slot < catalog.Units.Length; slot++)
            {
                if (!catalog.Units[slot].IsFormatted)
                {
                    continue;
                }
                // Built-in English owns the source contract; overrides must preserve its arguments.
                var source = catalogBundles.Where(bundle => bundle.Language == "en-US" && bundle.Values.HasValue(slot))
                    .OrderBy(bundle => bundle.SourceKind).FirstOrDefault();
                if (source is null)
                {
                    continue; // The mandatory en-US coverage pass below reports the missing source.
                }
                if (!CompositeFormatContractParser.TryParse(source.Values[slot]!, out var indexes, out _))
                {
                    throw new InvalidDataException("Invalid placeholder contract for '" + catalog.CatalogId + "/" + catalog.Units[slot].Key + "'.");
                }
                foreach (var bundle in catalogBundles.Where(bundle => bundle.Values.HasValue(slot)))
                {
                    if (!CompositeFormatContractParser.TryParse(bundle.Values[slot]!, out var translated, out _) || !indexes.SequenceEqual(translated))
                    {
                        throw new InvalidDataException("Translation from '" + bundle.SourceIdentity + "' changes the placeholder contract for '" + catalog.CatalogId + "/" + catalog.Units[slot].Key + "/" + bundle.Language + "'.");
                    }
                }
            }
            foreach (var language in supportedLanguages)
            {
                checkingLanguage?.Invoke(language);
                var candidates = LanguageFallbackChain.Resolve(language);
                for (var slot = 0; slot < catalog.Units.Length; slot++)
                {
                    // A non-English supported language needs translated coverage before final en-US fallback.
                    if (!candidates.Where(candidate => candidate != "en-US" || LanguageFallbackChain.IsEnglish(language))
                        .Any(candidate => catalogBundles.Any(bundle => bundle.Language == candidate && bundle.Values.HasValue(slot))))
                    {
                        throw new InvalidDataException("Supported language '" + language + "' has no translation before final en-US fallback for catalog '" + catalog.CatalogId + "', unit '" + catalog.Units[slot].Key + "'.");
                    }
                }
            }
        }
        return encoded;

        void Add(string id, string data, string assemblyName)
        {
            if (encoded.ContainsKey(id))
            {
                throw new InvalidDataException("Duplicate language module '" + id + "'.");
            }
            encoded.Add(id, data);
            origins.Add(id, assemblyName);
        }
    }
}
