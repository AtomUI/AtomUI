namespace AtomUI.Localization;

/// <summary>
/// Collects the catalogs, translation bundles, language modules, language definitions and the language
/// configuration from which the localization host is built.
/// The application reaches the builder through <c>IAtomUIBuilder.Localization</c>; the generated registration code
/// of language modules and of the application's languages fills the same instance before the host is built.
/// </summary>
public interface ILocalizationBuilder
{
    /// <summary>
    /// Decodes the data of one language module and adds the catalogs and translation bundles it carries.
    /// </summary>
    /// <param name="moduleId">
    /// The identifier of the language module. Each module can be added only once.
    /// </param>
    /// <param name="data">
    /// The encoded module data, as the generator emits it for the module.
    /// </param>
    /// <exception cref="InvalidOperationException">
    /// The builder is already frozen, as it is once the localization host has been built.
    /// </exception>
    /// <exception cref="LanguageCatalogException">
    /// The module was added before, or the data is not valid module data.
    /// </exception>
    void AddModuleData(string moduleId, string data);

    /// <summary>
    /// Adds a catalog to the localization host.
    /// </summary>
    /// <param name="descriptor">
    /// The catalog to add.
    /// </param>
    /// <exception cref="InvalidOperationException">
    /// The builder is already frozen, as it is once the localization host has been built.
    /// </exception>
    /// <remarks>
    /// The build throws a <see cref="LanguageCatalogException" /> when another registered catalog has the same
    /// identifier or uses the same resource enum type.
    /// </remarks>
    void AddCatalog(LanguageCatalogDescriptor descriptor);

    /// <summary>
    /// Adds a translation bundle to the localization host.
    /// </summary>
    /// <param name="descriptor">
    /// The bundle to add.
    /// </param>
    /// <exception cref="InvalidOperationException">
    /// The builder is already frozen, as it is once the localization host has been built.
    /// </exception>
    /// <remarks>
    /// The build throws a <see cref="LanguageCatalogException" /> unless the bundle targets a registered catalog
    /// and supplies exactly one value slot per unit of that catalog. Two bundles of the same language and source
    /// kind must not hold a value for the same unit.
    /// </remarks>
    void AddTranslationBundle(TranslationBundleDescriptor descriptor);

    /// <summary>
    /// Registers a language definition. The host uses it for its language in place of the generated standard
    /// metadata.
    /// </summary>
    /// <param name="definition">
    /// The definition to register.
    /// </param>
    /// <exception cref="InvalidOperationException">
    /// The builder is already frozen, or it is loading additional packages, because the set of supported languages
    /// cannot change then.
    /// </exception>
    /// <remarks>
    /// The build throws a <see cref="LanguageConfigurationException" /> when two definitions share a language tag.
    /// </remarks>
    void AddLanguageDefinition(LanguageDefinition definition);

    /// <summary>
    /// Sets the default language and the supported languages. A later call replaces an earlier configuration; without
    /// a call, en-US is the default language and the only supported language.
    /// </summary>
    /// <param name="defaultLanguage">
    /// The language that is current when the localization host is built. It must be one of the supported languages.
    /// </param>
    /// <param name="supportedLanguages">
    /// The languages the application can switch to. Duplicates are ignored, and at least one language is required.
    /// </param>
    /// <exception cref="ArgumentException">
    /// <paramref name="defaultLanguage" /> is the default value.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// The builder is already frozen, or it is loading additional packages, because the set of supported languages
    /// cannot change then.
    /// </exception>
    /// <remarks>
    /// The build throws a <see cref="LanguageConfigurationException" /> when the default language is not among the
    /// supported languages, a supported language is the default value, or the list is empty.
    /// </remarks>
    void ConfigureLanguages(
        LanguageTag defaultLanguage,
        IEnumerable<LanguageTag> supportedLanguages);
}
