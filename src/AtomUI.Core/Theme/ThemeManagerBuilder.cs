using AtomUI.Registration;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.Definitions;
using AtomUI.Theme.Definitions.Compiled;
using AtomUI.Theme.Resources;
using AtomUI.Theme.Schema;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Theme;

internal sealed class ThemeManagerBuilder : IThemeManagerBuilder
{
    private readonly List<ThemeDefinitionRegistration> _themeDefinitionResolvers = new();
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
        Generated.AtomUICore.GeneratedThemePackageRegistration.Register(this);
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

        _themeDefinitionResolvers.Add(new ThemeDefinitionRegistration(resolver));
    }

    public void AddCompiledThemePackage(CompiledThemePackage package)
    {
        ArgumentNullException.ThrowIfNull(package);
        ControlRegistrationState.ThrowIfUnavailable();
        if (!_registeredThemeDefinitionResolvers.Add(package.Id))
        {
            throw new ThemeResourceRegisterException($"Theme definition registration '{package.Id}' is already registered.");
        }
        _themeDefinitionResolvers.Add(new ThemeDefinitionRegistration(package));
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
            ValidateNewDescriptors(package, null);
            if (_registeredControlThemeProviders.Contains(package.ControlThemesProvider.Id))
            {
                throw new ThemeResourceRegisterException(
                    $"Control theme provider '{package.ControlThemesProvider.Id}' is already registered.");
            }
            _registeredControlPackageIds.Add(package.Id);
            package.PackageCommitOrdinal = _controlPackages.Count;
            _controlPackages.Add(package);
            TrackDescriptors(package);
            _registeredControlThemeProviders.Add(package.ControlThemesProvider.Id);
        }
        catch (Exception exception)
        {
            ControlRegistrationState.Fail(exception);
            DiscardPendingRegistrations();
            throw;
        }
    }

    private void ValidateNewDescriptors(ControlPackageRegistration package, ControlPackageRegistration? previous)
    {
        foreach (var descriptor in package.Controls.Except(previous?.Controls ?? []))
        {
            if (_registeredControlTokenIdentities.Contains(descriptor.Identity))
            {
                throw new ThemeResourceRegisterException($"Control Token descriptor '{descriptor.Identity}' is already registered.");
            }
        }
        foreach (var descriptor in package.SemanticControls.Except(previous?.SemanticControls ?? []))
        {
            if (_registeredSemanticControlIdentities.Contains(descriptor.Identity) ||
                _registeredSemanticControlTypes.Contains(descriptor.ControlType))
            {
                throw new ThemeResourceRegisterException($"Semantic Control descriptor '{descriptor.Identity}' is already registered.");
            }
        }
    }

    private void TrackDescriptors(ControlPackageRegistration package)
    {
        foreach (var descriptor in package.Controls)
        {
            _registeredControlTokenIdentities.Add(descriptor.Identity);
        }
        foreach (var descriptor in package.SemanticControls)
        {
            _registeredSemanticControlIdentities.Add(descriptor.Identity);
            _registeredSemanticControlTypes.Add(descriptor.ControlType);
        }
    }

    internal void CompletePendingRegistrations()
    {
        try
        {
            ControlRegistrationState.ThrowIfUnavailable();
            for (var index = 0; index < _controlPackages.Count; index++)
            {
                var previous = _controlPackages[index];
                if (previous.CompleteRegistration is not { } complete)
                {
                    continue;
                }
                previous.CompleteRegistration = null;
                var package = complete();
                ControlRegistrationState.ThrowIfUnavailable();
                ValidateNewDescriptors(package, previous);
                TrackDescriptors(package);
                package.PackageCommitOrdinal = previous.PackageCommitOrdinal;
                _controlPackages[index] = package;
            }
        }
        catch (Exception exception)
        {
            ControlRegistrationState.Fail(exception);
            DiscardPendingRegistrations();
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

    internal ThemeManager Build() => Build(staged: false);

    internal ThemeManager Build(bool staged)
    {
        try
        {
            if (!staged)
            {
                CompletePendingRegistrations();
            }
            ControlRegistrationState.Freeze();
            return BuildCore();
        }
        catch (Exception exception)
        {
            ControlRegistrationState.Fail(exception);
            DiscardPendingRegistrations();
            throw;
        }
    }

    private readonly List<ControlPackageRegistration> _mergedPackages = [];
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
        DiscardPendingRegistrations();
    }

    internal void DiscardPendingRegistrations()
    {
        foreach (var package in _controlPackages)
        {
            package.CompleteRegistration = null;
        }
    }

    /// <summary>
    /// Stage 2 注册块的完整包并入已构建的 ThemeManager：逐项走与 BuildCore 相同的
    /// 贡献合并路径（provider/描述符/语义/资产/资源），随后由
    /// ThemeManager.ExecuteRegistrationEvolutionAsync 重建 registry 并重应用主题。
    /// </summary>
    internal void MergeStageTwoInto(ThemeManager themeManager) => MergeContributionsInto(themeManager);

    // 初次构建与增量演化共用唯一的贡献累积路径。
    private void MergeContributionsInto(ThemeManager themeManager)
    {
        ArgumentNullException.ThrowIfNull(themeManager);
        var resourcesByProvider = new Dictionary<
            IControlThemesProvider,
            List<ControlThemeResourceRegistration>>(ReferenceEqualityComparer.Instance);
        // Packages are appended only after nested preparation commits, so list order is commit order.
        for (var index = 0; index < _controlPackages.Count; index++)
        {
            var contribution = _controlPackages[index];
            var previous = index < _mergedPackages.Count ? _mergedPackages[index] : null;
            if (ReferenceEquals(previous, contribution))
            {
                continue;
            }
            if (previous is null)
            {
                themeManager.RegisterControlThemesProvider(contribution.ControlThemesProvider);
            }
            MergeContributionMetadata(themeManager, contribution, previous);
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
            resources.Sort(ControlThemeResourceRegistration.CompareOrder);
            themeManager.StageControlThemeResources(
                provider,
                resources.ToArray());
        }

        _mergedPackages.Clear();
        _mergedPackages.AddRange(_controlPackages);
        UpdatePackageSchemaFingerprints(themeManager);
    }

    private void MergeContributionMetadata(
        ThemeManager themeManager,
        ControlPackageRegistration contribution,
        ControlPackageRegistration? previous)
    {
        foreach (var descriptor in contribution.Controls.Except(previous?.Controls ?? []))
        {
            themeManager.RegisterControlTokenDescriptor(descriptor);
        }
        themeManager.StageSemanticControlDescriptors(contribution.SemanticControls.Except(previous?.SemanticControls ?? []).ToArray());
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

    // 两阶段必须使用同一套有效来源，包括 UseUserThemeDirectory 配置生成的 resolver。
    internal IReadOnlyList<ThemeDefinitionRegistration> CreateThemeDefinitionRegistrations()
    {
        var themeDefinitionResolvers = new List<ThemeDefinitionRegistration>(_themeDefinitionResolvers);
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
            themeDefinitionResolvers.Add(new ThemeDefinitionRegistration(new UserDirectoryThemeDefinitionResolver(_userThemeDirectory)));
        }
        return themeDefinitionResolvers.AsReadOnly();
    }

    private ThemeManager BuildCore()
    {
        var themeDefinitionResolvers = CreateThemeDefinitionRegistrations();
        var themeManager = new ThemeManager { FontFamily = FontFamily };
        themeManager.ConfigureStartup(InitialRequest, FollowSystemLightRequest, FollowSystemDarkRequest);
        themeManager.ConfigureThemeDefinitions(themeDefinitionResolvers, ApplicationId!,
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
        themeManager.EnsureRegistrationCapacity(_registeredControlTokenIdentities.Count,
            _controlPackages.Sum(static contribution => contribution.ThemeAssets.Count),
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
            if (package.SchemaFingerprint is not { } schemaFingerprint)
            {
                allFingerprinted = false;
                break;
            }
            packageFingerprints.Add(new PackageSchemaFingerprint(package.Id, schemaFingerprint));
        }
        themeManager.SetPackageSchemaFingerprints(allFingerprinted ? packageFingerprints : null);
    }
}
