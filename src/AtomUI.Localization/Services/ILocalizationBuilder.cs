namespace AtomUI.Localization;

public interface ILocalizationBuilder
{
    void AddModuleData(string moduleId, string data);

    void AddCatalog(LanguageCatalogDescriptor descriptor);

    void AddTranslationBundle(TranslationBundleDescriptor descriptor);

    void AddLanguageDefinition(LanguageDefinition definition);

    void ConfigureLanguages(
        LanguageTag defaultLanguage,
        IEnumerable<LanguageTag> supportedLanguages);
}
