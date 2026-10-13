namespace AtomUI.Localization;

/// <summary>仅含已冻结的声明与读取视图，允许在线程池构建 Catalog 与语言行。</summary>
internal sealed record LocalizationEvolutionInput(
    LanguageCatalogDescriptor[] Catalogs,
    TranslationBundleDescriptor[] Bundles,
    IReadOnlyList<LanguageDefinition> SupportedLanguages,
    LanguageCatalogRegistry? PreviousRegistry,
    IReadOnlyDictionary<LanguageTag, LanguageSnapshot>? PreviousSnapshots)
{
    internal (LanguageCatalogRegistry Registry,
        IReadOnlyDictionary<LanguageTag, LanguageSnapshot> Snapshots,
        IReadOnlyList<LanguageDefinition> SupportedLanguages) Build()
    {
        var registry = LanguageCatalogRegistry.Create(Catalogs, Bundles, previous: PreviousRegistry);
        var snapshots = new Dictionary<LanguageTag, LanguageSnapshot>(SupportedLanguages.Count);
        foreach (var definition in SupportedLanguages)
        {
            LanguageSnapshot? previous = null;
            PreviousSnapshots?.TryGetValue(definition.Tag, out previous);
            snapshots.Add(definition.Tag, LanguageSnapshotBuilder.Build(registry, definition.Tag, definition, previous));
        }
        return (registry, snapshots, SupportedLanguages);
    }
}
