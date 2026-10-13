using AtomUI.Registration;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.Definitions;
using AtomUI.Theme.Resources;
using AtomUI.Theme.Schema;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Theme;

internal sealed class ThemeManagerBuilder : IThemeManagerBuilder
{
    private readonly List<IThemeDefinitionResolver> _themeDefinitionResolvers = new();
    private readonly List<Action<IThemeManager>> _initializers = new();
    private readonly HashSet<ControlTokenIdentity> _registeredControlTokenIdentities = new();
    private readonly HashSet<ControlTokenIdentity> _registeredSemanticControlIdentities = new();
    private readonly HashSet<Type> _registeredSemanticControlTypes = new();
    private readonly HashSet<string> _registeredControlPackageIds = new(StringComparer.Ordinal);
    private readonly HashSet<string> _registeredControlThemeProviders = new(StringComparer.Ordinal);
    private readonly HashSet<string> _registeredThemeDefinitionResolvers = new(StringComparer.Ordinal);
    private bool _useUserThemeDirectory;
    private string? _userThemeDirectory;

    internal ThemeManagerBuilder(Application? application = null)
    {
        ApplicationId = ThemeApplicationIdentity.ResolveDefault(application?.GetType()) ??
                        typeof(ThemeManagerBuilder).Assembly.GetName().Name!;
        InitialRequest = new ThemeRequest(
            IThemeManager.DEFAULT_THEME_ID,
            null,
            ThemeTransitionReason.Startup);
        AddThemeDefinitionResolver(CoreThemeDefinitionResolver.Create());
    }

    internal ControlRegistrationState ControlRegistrationState { get; } = new();
    internal FontFamily? FontFamily { get; private set; }
    internal ThemeRequest InitialRequest { get; private set; }
    internal ThemeRequest? FollowSystemLightRequest { get; private set; }
    internal ThemeRequest? FollowSystemDarkRequest { get; private set; }
    internal IReadOnlyList<Action<IThemeManager>> Initializers => _initializers;
    internal IReadOnlyList<ControlPackageRegistration> ControlPackages => _controlPackages;
    internal string? ApplicationId { get; private set; }

    private readonly List<ControlPackageRegistration> _controlPackages = new();

    public void AddThemeDefinitionResolver(IThemeDefinitionResolver resolver)
    {
        ArgumentNullException.ThrowIfNull(resolver);
        ControlRegistrationState.ThrowIfUnavailable();
        if (string.IsNullOrWhiteSpace(resolver.Id))
        {
            throw new ArgumentException("Theme definition resolver id cannot be empty.", nameof(resolver));
        }
        if (!_registeredThemeDefinitionResolvers.Add(resolver.Id))
        {
            throw new ThemeResourceRegisterException(
                $"Theme definition resolver '{resolver.Id}' is already registered.");
        }

        _themeDefinitionResolvers.Add(resolver);
    }

    public void AddControlPackage(ControlPackageRegistration package)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(package);
            ControlRegistrationState.ThrowIfUnavailable();
            if (_registeredControlPackageIds.Contains(package.Id))
            {
                throw new ThemeResourceRegisterException(
                    $"Control package '{package.Id}' is already registered.");
            }
            foreach (var descriptor in package.Controls)
            {
                if (_registeredControlTokenIdentities.Contains(descriptor.Identity))
                {
                    throw new ThemeResourceRegisterException(
                        $"Control Token descriptor '{descriptor.Identity}' is already registered.");
                }
            }
            foreach (var descriptor in package.SemanticControls)
            {
                if (_registeredSemanticControlIdentities.Contains(descriptor.Identity))
                {
                    throw new ThemeResourceRegisterException(
                        $"Semantic Control descriptor '{descriptor.Identity}' is already registered.");
                }
                if (_registeredSemanticControlTypes.Contains(descriptor.ControlType))
                {
                    throw new ThemeResourceRegisterException(
                        $"Semantic Control type '{descriptor.ControlType.FullName}' is already registered.");
                }
            }
            if (_registeredControlThemeProviders.Contains(package.ControlThemesProvider.Id))
            {
                throw new ThemeResourceRegisterException(
                    $"Control theme provider '{package.ControlThemesProvider.Id}' is already registered.");
            }
            _registeredControlPackageIds.Add(package.Id);
            package.PackageCommitOrdinal = _controlPackages.Count;
            _controlPackages.Add(package);
            foreach (var descriptor in package.Controls)
            {
                _registeredControlTokenIdentities.Add(descriptor.Identity);
            }
            foreach (var descriptor in package.SemanticControls)
            {
                _registeredSemanticControlIdentities.Add(descriptor.Identity);
                _registeredSemanticControlTypes.Add(descriptor.ControlType);
            }
            _registeredControlThemeProviders.Add(package.ControlThemesProvider.Id);
        }
        catch (Exception exception)
        {
            ControlRegistrationState.Fail(exception);
            throw;
        }
    }

    public void AddInitializer(Action<IThemeManager> initializer)
    {
        ArgumentNullException.ThrowIfNull(initializer);
        ControlRegistrationState.ThrowIfInitialConfigurationUnavailable();
        _initializers.Add(initializer);
    }

    public void WithInitialTheme(string themeId, ThemeConfig? config = null)
    {
        ControlRegistrationState.ThrowIfInitialConfigurationUnavailable();
        ArgumentException.ThrowIfNullOrWhiteSpace(themeId);
        InitialRequest = new ThemeRequest(themeId, config, ThemeTransitionReason.Startup);
        FollowSystemLightRequest = null;
        FollowSystemDarkRequest = null;
    }

    public void WithFollowSystemThemes(ThemeRequest light, ThemeRequest dark)
    {
        ControlRegistrationState.ThrowIfInitialConfigurationUnavailable();
        ArgumentNullException.ThrowIfNull(light);
        ArgumentNullException.ThrowIfNull(dark);
        FollowSystemLightRequest = light with { Reason = ThemeTransitionReason.FollowSystem };
        FollowSystemDarkRequest = dark with { Reason = ThemeTransitionReason.FollowSystem };
        InitialRequest = FollowSystemLightRequest;
    }

    public void WithApplicationId(string applicationId)
    {
        ControlRegistrationState.ThrowIfInitialConfigurationUnavailable();
        ThemeApplicationIdentity.Validate(applicationId, nameof(applicationId));
        ApplicationId = applicationId;
    }

    public void UseUserThemeDirectory()
    {
        ControlRegistrationState.ThrowIfUnavailable();
        _useUserThemeDirectory = true;
        _userThemeDirectory = null;
    }

    public void UseUserThemeDirectory(string directory)
    {
        ControlRegistrationState.ThrowIfUnavailable();
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        _useUserThemeDirectory = true;
        _userThemeDirectory = Path.GetFullPath(directory);
    }

    public void WithDefaultFontFamily(FontFamily fontFamily)
    {
        ControlRegistrationState.ThrowIfInitialConfigurationUnavailable();
        FontFamily = fontFamily;
    }

    public void WithDefaultFontFamily(string fontFamily)
    {
        ControlRegistrationState.ThrowIfInitialConfigurationUnavailable();
        FontFamily = FontFamily.Parse(fontFamily);
    }

    internal ThemeManager Build()
    {
        return Build(deferPackageCollects: false);
    }

    internal ThemeManager Build(bool deferPackageCollects)
    {
        try
        {
            // 非 staged 应用：分区包的余量收集在此内联执行，终态与旧全量 Collect 等价
            //（关键集先行 + 余量追补，registry 构建按 identity 排序，顺序无关）。
            // staged 应用（deferPackageCollects）由 Stage 2 注册窗口执行（分级启动）。
            if (!deferPackageCollects)
            {
                RunDeferredCollects();
            }
            ControlRegistrationState.Freeze();
            return BuildCore();
        }
        catch (Exception exception)
        {
            ControlRegistrationState.Fail(exception);
            throw;
        }
    }

    private int _mergedPackageCount;
    private int _mergedSegmentCount;
    private readonly Dictionary<string, ControlThemeAssetDescriptor> _mergedThemeAssets =
        new(StringComparer.Ordinal);
    private readonly Dictionary<string, ControlThemeResourceRegistration> _mergedThemeResources =
        new(StringComparer.Ordinal);

    // ---- 分级启动：Stage 2 注册窗口 ----

    /// <summary>受控重开 staging（ApplicationScope 在执行 Deferred 注册块前调用）。</summary>
    internal void OpenStageTwoWindow()
    {
        ControlRegistrationState.ReopenForStageTwo();
    }

    /// <summary>关闭窗口并永久冻结（失败路径也必须调用——由 ApplicationScope 的 finally 保证）。</summary>
    internal void CloseStageTwoWindow()
    {
        ControlRegistrationState.CloseStageTwoWindow();
    }

    /// <summary>
    /// Stage 2 注册块的包与分区包余量段并入已构建的 ThemeManager：逐项走与 BuildCore 相同的
    /// 贡献合并路径（provider/描述符/语义/资产/资源），随后由
    /// ThemeManager.ExecuteRegistrationEvolutionAsync 重建 registry 并重应用主题。
    /// </summary>
    internal void MergeStageTwoInto(ThemeManager themeManager) => MergeContributionsInto(themeManager);

    // 初次构建与增量演化共用唯一的贡献累积路径。
    private void MergeContributionsInto(ThemeManager themeManager)
    {
        ArgumentNullException.ThrowIfNull(themeManager);
        var contributions = new List<(ControlPackageRegistration Registration, bool IsSegment)>();
        for (var index = _mergedPackageCount; index < _controlPackages.Count; index++)
        {
            contributions.Add((_controlPackages[index], false));
        }
        for (var index = _mergedSegmentCount; index < _deferredPackageSegments.Count; index++)
        {
            contributions.Add((_deferredPackageSegments[index], true));
        }

        var resourcesByProvider = new Dictionary<
            IControlThemesProvider,
            List<ControlThemeResourceRegistration>>(ReferenceEqualityComparer.Instance);
        var registeredProviders = new HashSet<IControlThemesProvider>(ReferenceEqualityComparer.Instance);
        foreach (var (contribution, _) in contributions
                     .OrderBy(static item => item.Registration.PackageCommitOrdinal)
                     .ThenBy(static item => item.IsSegment))
        {
            if (registeredProviders.Add(contribution.ControlThemesProvider))
            {
                // Stage 1 provider 需要重新进入挂载队列以追加余量资源；Stage 2 分区包的包主体与
                // 余量段共享 provider，只登记一次，避免同一批 staged resources 被重复挂载。
                themeManager.RegisterControlThemesProvider(contribution.ControlThemesProvider);
            }
            MergeContributionMetadata(themeManager, contribution);
            foreach (var resource in contribution.Resources)
            {
                if (_mergedThemeResources.TryGetValue(resource.AssetId, out var existing))
                {
                    if (!existing.HasSameMetadata(resource))
                    {
                        throw new ThemeSchemaException(
                            $"Theme resource '{resource.AssetId}' has conflicting package definitions.");
                    }
                    continue;
                }
                _mergedThemeResources.Add(resource.AssetId, resource);
                if (!resourcesByProvider.TryGetValue(contribution.ControlThemesProvider, out var providerResources))
                {
                    providerResources = [];
                    resourcesByProvider.Add(contribution.ControlThemesProvider, providerResources);
                }
                providerResources.Add(resource);
            }
        }
        foreach (var (provider, resources) in resourcesByProvider)
        {
            themeManager.StageControlThemeResources(
                provider,
                resources.OrderBy(static resource => resource.Phase)
                         .ThenBy(static resource => resource.Order)
                         .ThenBy(static resource => resource.AssetId, StringComparer.Ordinal)
                         .ToArray());
        }

        _mergedPackageCount = _controlPackages.Count;
        _mergedSegmentCount = _deferredPackageSegments.Count;
        UpdatePackageSchemaFingerprints(themeManager);
    }

    private void MergeContributionMetadata(
        ThemeManager themeManager,
        ControlPackageRegistration contribution)
    {
        foreach (var descriptor in contribution.Controls)
        {
            themeManager.RegisterControlTokenDescriptor(descriptor);
        }
        themeManager.StageSemanticControlDescriptors(contribution.SemanticControls);
        foreach (var asset in contribution.ThemeAssets)
        {
            if (_mergedThemeAssets.TryGetValue(asset.AssetId, out var existing))
            {
                if (!existing.HasSameMetadata(asset))
                {
                    throw new ThemeSchemaException(
                        $"Theme asset '{asset.AssetId}' has conflicting package definitions.");
                }
                continue;
            }
            _mergedThemeAssets.Add(asset.AssetId, asset);
            themeManager.RegisterControlThemeAssetDescriptor(asset);
        }
    }

    // 分区收集的余量执行：fresh builder 收集增量，产出一个与包同 id、同 provider 的「余量段」。
    // 段与包主体共同构成该包的完整贡献，BuildCore（Stage 1）与 MergeStageTwoInto（Stage 2）消费
    // 同一贡献集合——余量段的描述符/语义/资产/资源对两条路径同等可见。
    internal void RunDeferredCollects()
    {
        foreach (var package in _controlPackages)
        {
            if (package.DeferredCollect is not { } deferredCollect || package.DeferredCollectConsumed)
            {
                continue;
            }
            var delta = new ControlPackageRegistrationBuilder();
            deferredCollect(delta);
            var segment = delta.Build(package.Id, package.ControlThemesProvider);
            segment.PackageCommitOrdinal = package.PackageCommitOrdinal;
            // 增量复用 AddControlPackage 的校验语义，跳过包 id / provider 重复检查（同一包的补充段）。
            foreach (var descriptor in segment.Controls)
            {
                if (_registeredControlTokenIdentities.Contains(descriptor.Identity))
                {
                    throw new ThemeResourceRegisterException(
                        $"Control Token descriptor '{descriptor.Identity}' is already registered.");
                }
            }
            foreach (var descriptor in segment.SemanticControls)
            {
                if (_registeredSemanticControlIdentities.Contains(descriptor.Identity))
                {
                    throw new ThemeResourceRegisterException(
                        $"Semantic Control descriptor '{descriptor.Identity}' is already registered.");
                }
                if (_registeredSemanticControlTypes.Contains(descriptor.ControlType))
                {
                    throw new ThemeResourceRegisterException(
                        $"Semantic Control type '{descriptor.ControlType.FullName}' is already registered.");
                }
            }
            foreach (var descriptor in segment.Controls)
            {
                _registeredControlTokenIdentities.Add(descriptor.Identity);
            }
            foreach (var descriptor in segment.SemanticControls)
            {
                _registeredSemanticControlIdentities.Add(descriptor.Identity);
                _registeredSemanticControlTypes.Add(descriptor.ControlType);
            }
            _deferredPackageSegments.Add(segment);
            package.DeferredCollectConsumed = true;
        }
    }

    // 分区包的余量段：与包同 id、同 provider 的补充贡献（RunDeferredCollects 产出）。
    private readonly List<ControlPackageRegistration> _deferredPackageSegments = new();

    /// <summary>
    /// 包的完整贡献集合：包主体（Stage 1 关键集）+ 分区包余量段（<see cref="RunDeferredCollects"/> 产出）。
    /// BuildCore（Stage 1 构建）与 MergeStageTwoInto（Stage 2 并入）都按本集合逐项消费——审查与
    /// 契约测试须用本视图，直接用 ControlPackages 只能看到包主体的关键集部分。
    /// </summary>
    internal IEnumerable<ControlPackageRegistration> EnumerateContributions()
    {
        foreach (var package in _controlPackages)
        {
            yield return package;
        }
        foreach (var segment in _deferredPackageSegments)
        {
            yield return segment;
        }
    }

    // 两阶段必须使用同一套有效来源，包括 UseUserThemeDirectory 配置生成的 resolver。
    internal IReadOnlyList<IThemeDefinitionResolver> CreateThemeDefinitionResolvers()
    {
        var themeDefinitionResolvers = new List<IThemeDefinitionResolver>(_themeDefinitionResolvers);
        if (_useUserThemeDirectory)
        {
            if (themeDefinitionResolvers.Any(static resolver =>
                    string.Equals(
                        resolver.Id,
                        UserDirectoryThemeDefinitionResolver.ResolverId,
                        StringComparison.Ordinal)))
            {
                throw new ThemeResourceRegisterException(
                    $"Theme definition resolver '{UserDirectoryThemeDefinitionResolver.ResolverId}' is already registered.");
            }
            themeDefinitionResolvers.Add(new UserDirectoryThemeDefinitionResolver(_userThemeDirectory));
        }
        return themeDefinitionResolvers.AsReadOnly();
    }

    private ThemeManager BuildCore()
    {
        var themeDefinitionResolvers = CreateThemeDefinitionResolvers();
        var themeManager = new ThemeManager { FontFamily = FontFamily };
        themeManager.ConfigureStartup(InitialRequest, FollowSystemLightRequest, FollowSystemDarkRequest);
        themeManager.ConfigureThemeDefinitions(themeDefinitionResolvers, ApplicationId!,
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
        themeManager.EnsureRegistrationCapacity(_registeredControlTokenIdentities.Count,
            EnumerateContributions().Sum(static contribution => contribution.ThemeAssets.Count),
            _registeredControlThemeProviders.Count);
        MergeContributionsInto(themeManager);
        themeManager.PrepareRegistrationSchema();
        return themeManager;
    }

    private void UpdatePackageSchemaFingerprints(ThemeManager themeManager)
    {
        // All-fingerprinted package sets let the startup registry compose its Revision from the
        // compile-time constants; any hand-registered package falls the whole set back to the
        // full-walk algorithm.
        var packageFingerprints = new List<PackageSchemaFingerprint>(_controlPackages.Count);
        var allFingerprinted = true;
        foreach (var package in _controlPackages)
        {
            // 余量未消费 = 包内容不完整，全量指纹不可用（见 ControlPackageRegistration 注释）；
            // 仅约束分区包，普通包（DeferredCollect == null）不受影响。
            if (package.SchemaFingerprint is not { } schemaFingerprint ||
                (package.DeferredCollect is not null && !package.DeferredCollectConsumed))
            {
                allFingerprinted = false;
                break;
            }
            packageFingerprints.Add(new PackageSchemaFingerprint(package.Id, schemaFingerprint));
        }
        themeManager.SetPackageSchemaFingerprints(allFingerprinted ? packageFingerprints : null);
    }
}
