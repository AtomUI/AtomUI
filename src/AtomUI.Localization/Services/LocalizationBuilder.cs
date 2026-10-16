using AtomUI.Localization.Data;

namespace AtomUI.Localization;

internal sealed class LocalizationBuilder : ILocalizationBuilder
{
    private readonly List<LanguageCatalogDescriptor> _catalogs = [];
    private readonly List<TranslationBundleDescriptor> _bundles = [];
    private readonly List<LanguageDefinition> _definitions = [];
    private readonly HashSet<string> _moduleIds = new(StringComparer.Ordinal);
    private readonly Dictionary<string, string> _moduleData = new(StringComparer.Ordinal);
    private IReadOnlySet<string>? _verifiedLanguages;
    private IReadOnlyDictionary<string, string>? _verifiedModules;
    private LanguageCatalogRegistry? _registry;
    private LocalizationHost? _host;
    private LanguageTag _defaultLanguage;
    private LanguageTag[]? _supportedLanguages;
    private bool _isFrozen;
    private readonly bool _appendOnly;

    internal LocalizationBuilder(bool appendOnly = false) => _appendOnly = appendOnly;
    internal IReadOnlyDictionary<string, string> ModuleData => _moduleData;
    internal void Close() => _isFrozen = true;

    private void EnsureInitialConfiguration()
    {
        EnsureMutable();
        if (_appendOnly)
        {
            throw new InvalidOperationException("Supported languages and language definitions cannot change during package loading.");
        }
    }

    internal IReadOnlyList<LanguageCatalogDescriptor> Catalogs => _catalogs;

    internal IReadOnlyList<TranslationBundleDescriptor> TranslationBundles => _bundles;

    public void AddModuleData(string moduleId, string data)
    {
        EnsureMutable();
        if (_moduleIds.Contains(moduleId))
        {
            throw new LanguageCatalogException($"Language module '{moduleId}' is registered more than once.");
        }
        LanguageModuleData module;
        try
        {
            module = LanguageModuleDataCodec.Read(moduleId, data);
        }
        catch (FormatException exception)
        {
            throw new LanguageCatalogException($"Invalid language module '{moduleId}'.", exception);
        }
        var catalogs = module.Catalogs.Select(static catalog => new LanguageDataCatalogDescriptor(catalog)).ToArray();
        var bundles = module.Bundles.Select(static bundle => new TranslationBundleDescriptor(bundle)).ToArray();
        _moduleIds.Add(moduleId);
        _moduleData.Add(moduleId, data);
        _catalogs.AddRange(catalogs);
        _bundles.AddRange(bundles);
    }

    public void AddCatalog(LanguageCatalogDescriptor descriptor)
    {
        EnsureMutable();
        ArgumentNullException.ThrowIfNull(descriptor);
        _catalogs.Add(descriptor);
    }

    public void AddTranslationBundle(TranslationBundleDescriptor descriptor)
    {
        EnsureMutable();
        ArgumentNullException.ThrowIfNull(descriptor);
        _bundles.Add(descriptor);
    }

    public void AddLanguageDefinition(LanguageDefinition definition)
    {
        EnsureInitialConfiguration();
        ArgumentNullException.ThrowIfNull(definition);
        _definitions.Add(definition);
    }

    public void ConfigureLanguages(
        LanguageTag defaultLanguage,
        IEnumerable<LanguageTag> supportedLanguages)
    {
        EnsureInitialConfiguration();
        if (defaultLanguage == default)
        {
            throw new ArgumentException("A valid default language is required.", nameof(defaultLanguage));
        }

        ArgumentNullException.ThrowIfNull(supportedLanguages);
        _defaultLanguage = defaultLanguage;
        _supportedLanguages = supportedLanguages.ToArray();
    }

    internal void ApplyBuildProof(IReadOnlySet<string> languages, IReadOnlyDictionary<string, string> modules)
    {
        EnsureMutable();
        _verifiedLanguages = languages;
        _verifiedModules = modules;
    }

    private bool HasBuildProof(IReadOnlyList<LanguageTag> languages) =>
        _verifiedLanguages is { } verified && _verifiedModules is { } modules &&
        languages.All(language => verified.Contains(language.Value)) && _moduleData.Count == modules.Count &&
        _moduleData.All(pair => modules.TryGetValue(pair.Key, out var data) && string.Equals(pair.Value, data, StringComparison.Ordinal));

    internal LanguageCatalogRegistry FreezeRegistry(bool generatedDataVerified = false)
    {
        if (_registry is not null)
        {
            return _registry;
        }

        _isFrozen = true;
        _registry = LanguageCatalogRegistry.Create(_catalogs, _bundles, generatedDataVerified: generatedDataVerified);
        return _registry;
    }

    /// <summary>构建本地化宿主；跨包的 bundle 引用必须闭合，否则抛出。</summary>
    internal LocalizationHost Build(Func<bool>? checkAccess = null)
    {
        if (_host is not null)
        {
            return _host;
        }

        var (defaultLanguage, supportedLanguages) = NormalizeLanguageConfiguration();
        var definitionsByTag = CollectExplicitDefinitions();
        var supportedDefinitions = ResolveSupportedDefinitions(
            supportedLanguages,
            definitionsByTag);
        var generatedDataVerified = HasBuildProof(supportedLanguages);
        var registry = FreezeRegistry(generatedDataVerified);
        var defaultDefinition = supportedDefinitions.First(definition => definition.Tag == defaultLanguage);
        foreach (var definition in supportedDefinitions)
        {
            LanguageSnapshotBuilder.ValidateManualCoverage(registry, definition, generatedDataVerified);
        }
        var snapshots = new Dictionary<LanguageTag, LanguageSnapshot>
        {
            [defaultLanguage] = LanguageSnapshotBuilder.Build(registry, defaultLanguage, defaultDefinition, deferCatalogs: true)
        };
        var initialState = new LanguageState(
            defaultLanguage,
            defaultDefinition.FormattingCulture,
            defaultDefinition.TextDirection,
            revision: 0);
        var context = new LanguageContext(
            registry,
            new LanguageRevision(snapshots[defaultLanguage], initialState));
        var provider = new LanguageResourceProvider(context);
        var manager = new LanguageManager(
            context,
            snapshots,
            supportedDefinitions,
            provider,
            checkAccess,
            _moduleData);
        var localizer = new Localizer(context);
        _host = new LocalizationHost(
            registry,
            snapshots,
            manager,
            localizer,
            provider);
        return _host;
    }

    private (LanguageTag DefaultLanguage, IReadOnlyList<LanguageTag> SupportedLanguages)
        NormalizeLanguageConfiguration()
    {
        var defaultLanguage = _supportedLanguages is null
            ? LanguageTags.EnUS
            : _defaultLanguage;
        var configuredLanguages = _supportedLanguages ?? [LanguageTags.EnUS];
        if (configuredLanguages.Length == 0)
        {
            throw new LanguageConfigurationException("At least one supported language is required.");
        }

        var uniqueLanguages = new List<LanguageTag>(configuredLanguages.Length);
        var seenLanguages = new HashSet<LanguageTag>();
        foreach (var language in configuredLanguages)
        {
            if (language == default)
            {
                throw new LanguageConfigurationException(
                    "Every supported language must be a valid language tag.");
            }
            if (seenLanguages.Add(language))
            {
                uniqueLanguages.Add(language);
            }
        }

        if (!seenLanguages.Contains(defaultLanguage))
        {
            throw new LanguageConfigurationException(
                $"Default language '{defaultLanguage.Value}' must be included in the supported languages.");
        }

        return (defaultLanguage, uniqueLanguages.AsReadOnly());
    }

    private Dictionary<LanguageTag, LanguageDefinition> CollectExplicitDefinitions()
    {
        var definitionsByTag = new Dictionary<LanguageTag, LanguageDefinition>();
        foreach (var definition in _definitions)
        {
            if (!definitionsByTag.TryAdd(definition.Tag, definition))
            {
                throw new LanguageConfigurationException(
                    $"Language definition '{definition.Tag.Value}' is registered more than once.");
            }
        }

        return definitionsByTag;
    }

    private static IReadOnlyList<LanguageDefinition> ResolveSupportedDefinitions(
        IReadOnlyList<LanguageTag> supportedLanguages,
        IReadOnlyDictionary<LanguageTag, LanguageDefinition> explicitDefinitions)
    {
        var definitions = new LanguageDefinition[supportedLanguages.Count];
        for (var index = 0; index < supportedLanguages.Count; index++)
        {
            var language = supportedLanguages[index];
            if (explicitDefinitions.TryGetValue(language, out var explicitDefinition))
            {
                definitions[index] = explicitDefinition;
                continue;
            }
            if (StandardLanguageDefinitions.TryCreate(language, out var standardDefinition))
            {
                definitions[index] = standardDefinition;
                continue;
            }

            throw new LanguageConfigurationException(
                $"Supported language '{language.Value}' has no generated standard metadata. " +
                $"Register an explicit {nameof(LanguageDefinition)} with a formatting Culture and text direction.");
        }

        return Array.AsReadOnly(definitions);
    }

    private void EnsureMutable()
    {
        if (_isFrozen)
        {
            throw new InvalidOperationException("The localization builder has already been frozen.");
        }
    }
}
