namespace AtomUI.Localization;

internal sealed class LocalizationBuilder : ILocalizationBuilder
{
    private readonly List<LanguageCatalogDescriptor> _catalogs = [];
    private readonly List<TranslationBundleDescriptor> _bundles = [];
    private readonly List<LanguageDefinition> _definitions = [];
    private LanguageCatalogRegistry? _registry;
    private LocalizationHost? _host;
    private LanguageContext? _builtContext;
    private LanguageTag _defaultLanguage;
    private LanguageTag[]? _supportedLanguages;
    private bool _isFrozen;

    internal IReadOnlyList<LanguageCatalogDescriptor> Catalogs => _catalogs;

    internal IReadOnlyList<TranslationBundleDescriptor> TranslationBundles => _bundles;

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
        EnsureMutable();
        ArgumentNullException.ThrowIfNull(definition);
        _definitions.Add(definition);
    }

    public void ConfigureLanguages(
        LanguageTag defaultLanguage,
        IEnumerable<LanguageTag> supportedLanguages)
    {
        EnsureMutable();
        if (defaultLanguage == default)
        {
            throw new ArgumentException("A valid default language is required.", nameof(defaultLanguage));
        }

        ArgumentNullException.ThrowIfNull(supportedLanguages);
        _defaultLanguage = defaultLanguage;
        _supportedLanguages = supportedLanguages.ToArray();
    }

    internal LanguageCatalogRegistry FreezeRegistry(bool allowUnresolvedBundles = false)
    {
        if (_registry is not null)
        {
            return _registry;
        }

        _isFrozen = true;
        _registry = LanguageCatalogRegistry.Create(_catalogs, _bundles, allowUnresolvedBundles);
        return _registry;
    }

    // 分级启动：已构建的语言上下文（LanguageContext.Evolve 的调用入口）。
    internal LanguageContext BuiltContext =>
        _builtContext ?? throw new InvalidOperationException("Localization has not been built yet.");

    /// <summary>
    /// 构建本地化宿主。<paramref name="allowUnresolvedBundles"/> 仅在分级启动的 Stage 1 中间态为
    /// true：应用级语言包可能先于其目标包的 catalog 到达（catalog 随包注册被后置），此时未解析的
    /// bundle 不参与 Stage 1 快照（其 catalog 不存在，任何查询都不可达），由 Stage 2 演化以全量
    /// 集合严格重建并解析。
    /// </summary>
    internal LocalizationHost Build(Func<bool>? checkAccess = null, bool allowUnresolvedBundles = false)
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
        var registry = FreezeRegistry(allowUnresolvedBundles);
        var snapshots = new Dictionary<LanguageTag, LanguageSnapshot>(supportedDefinitions.Count);
        foreach (var definition in supportedDefinitions)
        {
            snapshots.Add(
                definition.Tag,
                LanguageSnapshotBuilder.Build(registry, definition.Tag, definition));
        }

        var defaultDefinition = supportedDefinitions.First(definition => definition.Tag == defaultLanguage);
        var initialState = new LanguageState(
            defaultLanguage,
            defaultDefinition.FormattingCulture,
            defaultDefinition.TextDirection,
            revision: 0);
        var context = new LanguageContext(
            registry,
            new LanguageRevision(snapshots[defaultLanguage], initialState));
        _builtContext = context;
        var provider = new LanguageResourceProvider(context);
        var manager = new LanguageManager(
            context,
            snapshots,
            supportedDefinitions,
            provider,
            checkAccess);
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

    // ---- 分级启动：Stage 2 注册窗口 ----
    private bool _stageTwoWindowUsed;

    /// <summary>受控重开（一次性）：Deferred 注册块内追加 catalog/bundle/定义。</summary>
    internal void OpenStageTwoWindow()
    {
        if (_isFrozen && !_stageTwoWindowUsed)
        {
            _stageTwoWindowUsed = true;
            _isFrozen = false;
            return;
        }
        throw new InvalidOperationException(
            "Stage 2 localization window is unavailable (not frozen, or already used).");
    }

    /// <summary>关闭窗口并永久冻结（失败路径也必须调用——由 ApplicationScope 的 finally 保证）。</summary>
    internal void CloseStageTwoWindow()
    {
        _isFrozen = true;
    }

    /// <summary>
    /// Stage 2 演化构建：冻结完整 registry，按不可变来源证据复用旧 Catalog 行。
    /// 新增 Bundle/Override 仍使相关行失效，完成时全支持语言均已验证并就绪。
    /// </summary>
    internal (
        LanguageCatalogRegistry Registry,
        IReadOnlyDictionary<LanguageTag, LanguageSnapshot> Snapshots,
        IReadOnlyList<LanguageDefinition> SupportedLanguages)
        BuildStageTwoEvolution(LanguageCatalogRegistry? previousRegistry = null,
            IReadOnlyDictionary<LanguageTag, LanguageSnapshot>? previousSnapshots = null)
    {
        return CaptureStageTwoEvolution(previousRegistry, previousSnapshots).Build();
    }

    internal LocalizationEvolutionInput CaptureStageTwoEvolution(LanguageCatalogRegistry? previousRegistry,
        IReadOnlyDictionary<LanguageTag, LanguageSnapshot>? previousSnapshots)
    {
        if (!_stageTwoWindowUsed || !_isFrozen)
        {
            throw new InvalidOperationException(
                "Stage 2 localization evolution requires a closed registration window.");
        }
        var (_, supportedLanguages) = NormalizeLanguageConfiguration();
        var definitionsByTag = CollectExplicitDefinitions();
        var supportedDefinitions = ResolveSupportedDefinitions(supportedLanguages, definitionsByTag);
        return new LocalizationEvolutionInput(_catalogs.ToArray(), _bundles.ToArray(), supportedDefinitions,
            previousRegistry, previousSnapshots);
    }
}
