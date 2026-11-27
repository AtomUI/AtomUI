using AtomUI.Controls;
using AtomUI.Registration;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.Definitions;
using AtomUI.Theme.Definitions.Compiled;
using AtomUI.Theme.Languages;
using AtomUI.Theme.Resources;
using AtomUI.Theme.Schema;
using Avalonia.Media;

namespace AtomUI.Theme;

internal sealed class ThemeManagerBuilder : IThemeManagerBuilder
{
    private readonly List<ThemeDefinitionRegistration> _themeDefinitionResolvers = new();
    private readonly List<Action<IThemeManager>> _initializers = new();
    private readonly List<EnabledControlPackage> _packageDeclarations = [];
    private readonly List<DesignLanguageDescriptor> _languages = [];
    private readonly List<ControlTokenRuleDescriptor> _tokenRules = [];
    private List<Action<IThemeManagerBuilder>>? _deferredTokenRules;
    private List<ControlBehaviorDescriptor>? _controlBehaviors;
    private readonly Dictionary<Type, string> _defaultThemes = [];
    private readonly Dictionary<(string ControlAssembly, Type LanguageTokenType), (Type Language, ControlValuePackage Package)>
        _valueSelections = [];
    private Type? _initialLanguage;
    private StartupImage? _startupImage;
    private readonly HashSet<ControlTokenIdentity> _registeredControlTokenIdentities = new();
    private readonly HashSet<ControlTokenIdentity> _registeredSemanticControlIdentities = new();
    private readonly HashSet<Type> _registeredSemanticControlTypes = new();
    private readonly HashSet<string> _registeredControlPackageIds = new(StringComparer.Ordinal);
    private readonly HashSet<string> _registeredControlThemeProviders = new(StringComparer.Ordinal);
    private readonly HashSet<string> _registeredThemeDefinitionResolvers = new(StringComparer.Ordinal);
    private bool _useUserThemeDirectory;
    private string? _userThemeDirectory;
    private bool _hasExplicitApplicationId;
    private readonly bool _hasApplicationType;

    internal ThemeManagerBuilder(Type? applicationType = null)
    {
        _hasApplicationType = applicationType is not null;
        ApplicationId = ThemeApplicationIdentity.ResolveDefault(applicationType) ??
                        typeof(ThemeManagerBuilder).Assembly.GetName().Name!;
    }

    internal ControlRegistrationState ControlRegistrationState { get; } = new();
    internal FontFamily? FontFamily { get; private set; }
    /// <summary>
    /// The initial request given by identity, or <see langword="null" />.
    /// </summary>
    internal ThemeRequest? InitialRequest { get; private set; }

    /// <summary>
    /// The initial end-user selection, or <see langword="null" />; resolved when the manager starts.
    /// </summary>
    internal ThemeSelection? InitialSelection { get; private set; }
    internal ThemeRequest? FollowSystemLightRequest { get; private set; }
    internal ThemeRequest? FollowSystemDarkRequest { get; private set; }
    internal IReadOnlyList<Action<IThemeManager>> Initializers => _initializers;
    internal IReadOnlyList<ControlPackageRegistration> ControlPackages => _controlPackages;
    internal IReadOnlyList<EnabledControlPackage> PackageDeclarations => _packageDeclarations;
    internal IReadOnlyList<DesignLanguageDescriptor> Languages => _languages;
    internal IReadOnlyList<ControlTokenRuleDescriptor> TokenRules => _tokenRules;
    internal IReadOnlyList<ControlBehaviorDescriptor>? ControlBehaviors => _controlBehaviors;
    internal bool HasControlContributions => _tokenRules.Count != 0 || _deferredTokenRules is { Count: > 0 } ||
                                             _controlBehaviors is { Count: > 0 };
    internal StartupImage? StartupImage => _startupImage;
    internal void AddPackageDeclaration(EnabledControlPackage declaration)
    {
        if (_registeredControlPackageIds.Contains(declaration.Definition.Id))
        {
            throw new ThemeResourceRegisterException("Control package is already registered: " + declaration.Definition.Id);
        }
        _packageDeclarations.Add(declaration);
    }
    internal void UseStartupImage(StartupImage image)
    {
        ControlRegistrationState.ThrowIfInitialConfigurationUnavailable();
        _startupImage = image;
    }
    internal string? ApplicationId { get; private set; }

    private readonly List<ControlPackageRegistration> _controlPackages = new();

    public void AddThemeDefinitionResolver(IThemeDefinitionResolver resolver)
    {
        ArgumentNullException.ThrowIfNull(resolver);
        ControlRegistrationState.ThrowIfInitialConfigurationUnavailable();
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
        ControlRegistrationState.ThrowIfInitialConfigurationUnavailable();
        if (!_registeredThemeDefinitionResolvers.Add(package.Id))
        {
            throw new ThemeResourceRegisterException($"Theme definition registration '{package.Id}' is already registered.");
        }
        _themeDefinitionResolvers.Add(new ThemeDefinitionRegistration(package));
    }

    public IThemeManagerBuilder AddDesignLanguage(DesignLanguageDescriptor language)
    {
        ArgumentNullException.ThrowIfNull(language);
        // Package append may bring new languages, so only a frozen or failed builder rejects them.
        ControlRegistrationState.ThrowIfUnavailable();
        // Idempotent for the same registration: a language package's entry point may be reached more than once.
        if (!_languages.Contains(language))
        {
            _languages.Add(language);
        }
        return this;
    }

    public IThemeManagerBuilder AddControlTokenRules(IEnumerable<ControlTokenRuleDescriptor> rules)
    {
        ArgumentNullException.ThrowIfNull(rules);
        ControlRegistrationState.ThrowIfUnavailable();
        foreach (var rule in rules)
        {
            if (rule is null)
            {
                throw new ArgumentException("Control rule descriptors cannot contain null.", nameof(rules));
            }
            // Kept by reference: compilation caches identify a rule by its descriptor instance; a value
            // package's entry point may be reached more than once (a dependent value package registers it too).
            if (!_tokenRules.Contains(rule))
            {
                _tokenRules.Add(rule);
            }
        }
        return this;
    }

    public void AddControlBehavior(ControlBehaviorDescriptor behavior)
    {
        ArgumentNullException.ThrowIfNull(behavior);
        ControlRegistrationState.ThrowIfUnavailable();
        (_controlBehaviors ??= []).Add(behavior);
    }

    public void AddDeferredControlTokenRules(Action<IThemeManagerBuilder> register)
    {
        ArgumentNullException.ThrowIfNull(register);
        ControlRegistrationState.ThrowIfUnavailable();
        (_deferredTokenRules ??= []).Add(register);
    }

    public void AddControlPackage(ControlPackageRegistration package)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(package);
            ControlRegistrationState.ThrowIfUnavailable();
            if (ControlRegistrationState.IsStaged(package.Id) || _registeredControlPackageIds.Contains(package.Id) || _packageDeclarations.Any(declaration => declaration.Definition.Id == package.Id))
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
            CompleteDeferredTokenRules();
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
        CheckInitialLanguageNotSet();
        ArgumentException.ThrowIfNullOrWhiteSpace(themeId);
        InitialRequest = new ThemeRequest(themeId, config, ThemeTransitionReason.Startup);
        InitialSelection = null;
        FollowSystemLightRequest = null;
        FollowSystemDarkRequest = null;
    }

    public void WithInitialTheme(ThemeSelection selection)
    {
        ControlRegistrationState.ThrowIfInitialConfigurationUnavailable();
        CheckInitialLanguageNotSet();
        ArgumentNullException.ThrowIfNull(selection);
        InitialSelection = selection;
        InitialRequest = null;
        FollowSystemLightRequest = null;
        FollowSystemDarkRequest = null;
    }

    public void WithFollowSystemThemes(ThemeRequest light, ThemeRequest dark)
    {
        ControlRegistrationState.ThrowIfInitialConfigurationUnavailable();
        CheckInitialLanguageNotSet();
        ArgumentNullException.ThrowIfNull(light);
        ArgumentNullException.ThrowIfNull(dark);
        FollowSystemLightRequest = light with { Reason = ThemeTransitionReason.FollowSystem };
        FollowSystemDarkRequest = dark with { Reason = ThemeTransitionReason.FollowSystem };
        InitialRequest = FollowSystemLightRequest;
        InitialSelection = null;
    }

    public void WithInitialDesignLanguage<TLanguage>() where TLanguage : DesignLanguage
    {
        ControlRegistrationState.ThrowIfInitialConfigurationUnavailable();
        if (_initialLanguage is not null || InitialRequest is not null || InitialSelection is not null)
        {
            throw new InvalidOperationException("The initial language or theme has already been configured.");
        }
        _initialLanguage = typeof(TLanguage);
    }

    public void SetDefaultTheme<TLanguage>(string themeId) where TLanguage : DesignLanguage
    {
        ControlRegistrationState.ThrowIfInitialConfigurationUnavailable();
        ArgumentException.ThrowIfNullOrWhiteSpace(themeId);
        if (!_defaultThemes.TryAdd(typeof(TLanguage), themeId) && _defaultThemes[typeof(TLanguage)] != themeId)
        {
            throw new InvalidOperationException($"A default theme is already configured for '{typeof(TLanguage).Name}'.");
        }
    }

    public void SelectValuePackage<TLanguage>(ControlValuePackage package) where TLanguage : DesignLanguage
    {
        ControlRegistrationState.ThrowIfInitialConfigurationUnavailable();
        ArgumentNullException.ThrowIfNull(package);
        var pair = (package.ControlAssembly, package.LanguageTokenType);
        if (!_valueSelections.TryAdd(pair, (typeof(TLanguage), package)) &&
            _valueSelections[pair] != (typeof(TLanguage), package))
        {
            throw new InvalidOperationException($"Multiple value packages were selected for '{package.ControlAssembly}'.");
        }
    }

    private void CheckInitialLanguageNotSet()
    {
        if (_initialLanguage is not null)
        {
            throw new InvalidOperationException("The initial language or theme has already been configured.");
        }
    }

    private DesignLanguageDescriptor GetEnabledLanguage(Type type) =>
        _languages.FirstOrDefault(language => language.Language.GetType() == type) ??
        throw new ThemeSelectionException($"Design language '{type.Name}' is not enabled.");

    private ThemeSelection? ResolveInitialSelection() =>
        _initialLanguage is { } type ? ThemeSelection.Of(GetEnabledLanguage(type).Id) : InitialSelection;

    private IReadOnlyDictionary<string, string>? ResolveDefaultThemes() =>
        _defaultThemes.Count == 0 ? null : _defaultThemes
            .ToDictionary(entry => GetEnabledLanguage(entry.Key).Id, static entry => entry.Value, StringComparer.Ordinal);

    public void WithApplicationId(string applicationId)
    {
        ControlRegistrationState.ThrowIfInitialConfigurationUnavailable();
        ThemeApplicationIdentity.Validate(applicationId, nameof(applicationId));
        ApplicationId = applicationId;
        _hasExplicitApplicationId = true;
    }

    /// <summary>
    /// Whether the application identity is known, from the application type or an explicit application id, which
    /// early preparation requires. When an application already exists, the synchronous Splash preparation is gated
    /// by it as well.
    /// </summary>
    internal bool CanPrepareBeforeApplication => _hasApplicationType || _hasExplicitApplicationId;

    /// <summary>
    /// Whether preparation can run on a background thread. It does so only with theme definition resolvers that
    /// declare themselves safe for the background and with the pure-data programs of generated theme packages.
    /// </summary>
    internal bool CanPrepareInBackground => CanPrepareBeforeApplication &&
        _themeDefinitionResolvers.All(static registration =>
            registration.Resolver is IBackgroundThemeDefinitionResolver ||
            registration.Package is { } package && package.Programs.All(static program => program.SupportsBackgroundPrepare));

    internal ThemeStartupInputs CaptureStartupInputs() =>
        new(InitialRequest,
            ResolveInitialSelection(),
            FollowSystemLightRequest,
            FollowSystemDarkRequest,
            CreateThemeDefinitionRegistrations(),
            ApplicationId!,
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            FontFamily,
            _languages.ToArray(),
            ResolveDefaultThemes());

    public void UseUserThemeDirectory()
    {
        ControlRegistrationState.ThrowIfInitialConfigurationUnavailable();
        _useUserThemeDirectory = true;
        _userThemeDirectory = null;
    }

    public void UseUserThemeDirectory(string directory)
    {
        ControlRegistrationState.ThrowIfInitialConfigurationUnavailable();
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
        try
        {
            CompletePendingRegistrations();
            RegisterLinkedValues();
            CompleteDeferredTokenRules();
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

    private void CompleteDeferredTokenRules()
    {
        while (_deferredTokenRules is { } registrations)
        {
            _deferredTokenRules = null;
            foreach (var register in registrations)
            {
                register(this);
            }
        }
    }

    private void RegisterLinkedValues()
    {
        if ((_startupImage?.ControlValuePackages.Count ?? 0) == 0 && _valueSelections.Count == 0)
        {
            return;
        }

        var controlAssemblies = _packageDeclarations.Select(static package => package.Definition.Data.AssemblyName)
            .Concat(_controlPackages.SelectMany(static package => package.Controls)
                .Select(static control => control.ControlType.Assembly.GetName().Name))
            .ToHashSet(StringComparer.Ordinal);
        var tokenTypes = new HashSet<Type>();
        foreach (var language in _languages)
        {
            for (var type = language.LanguageTokenType; type is not null && tokenTypes.Add(type); type = type.BaseType)
            {
            }
        }

        var manual = _tokenRules.Select(static descriptor =>
            (descriptor.TokenType.Assembly.GetName().Name, descriptor.LanguageTokenType)).ToHashSet();
        foreach (var (pair, selection) in _valueSelections)
        {
            var language = GetEnabledLanguage(selection.Language);
            if (language.LanguageTokenType != pair.LanguageTokenType || !controlAssemblies.Contains(pair.ControlAssembly))
            {
                throw new ThemeSchemaException(
                    $"Value package '{selection.Package.Id}' does not match an enabled control package and language '{language.Id}'.");
            }
            if (manual.Contains(pair))
            {
                throw new ThemeSchemaException(
                    $"Value package '{selection.Package.Id}' also has an explicit token rule registration for the same pair.");
            }
            selection.Package.Register(this);
        }

        foreach (var candidates in (_startupImage?.ControlValuePackages ?? [])
                     .Where(package => controlAssemblies.Contains(package.ControlAssembly) &&
                                       tokenTypes.Contains(package.LanguageTokenType))
                     .GroupBy(static package => (package.ControlAssembly, package.LanguageTokenType)))
        {
            if (_valueSelections.ContainsKey(candidates.Key) || manual.Contains(candidates.Key))
            {
                continue;
            }
            if (candidates.Count() != 1)
            {
                var language = _languages.FirstOrDefault(value => value.LanguageTokenType == candidates.Key.LanguageTokenType)?.Id ??
                               candidates.Key.LanguageTokenType.Name;
                throw new ThemeSchemaException(
                    $"Multiple value packages for '{candidates.Key.ControlAssembly}' under '{language}': " +
                    string.Join(", ", candidates.Select(static package => package.Id).Order(StringComparer.Ordinal)) +
                    ". Select one with UseValues.");
            }
            candidates.First().Register(this);
        }
    }

    private readonly List<ControlPackageRegistration> _mergedPackages = [];
    private readonly Dictionary<string, ControlThemeAssetDescriptor> _mergedThemeAssets =
        new(StringComparer.Ordinal);
    private readonly Dictionary<string, ControlThemeResourceRegistration> _mergedThemeResources =
        new(StringComparer.Ordinal);

    internal void DiscardPendingRegistrations()
    {
        _deferredTokenRules = null;
        foreach (var package in _controlPackages)
        {
            package.CompleteRegistration = null;
        }
    }

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

    // The effective sources include the resolver that the UseUserThemeDirectory configuration adds.
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
        themeManager.SetLanguages(_languages.ToArray(), _tokenRules.ToArray());
        themeManager.SetStartupBehaviors(_controlBehaviors);
        themeManager.ConfigureStartup(InitialRequest, ResolveInitialSelection(), FollowSystemLightRequest,
            FollowSystemDarkRequest, ResolveDefaultThemes());
        themeManager.ConfigureThemeDefinitions(themeDefinitionResolvers, ApplicationId!,
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
        themeManager.EnsureRegistrationCapacity(_registeredControlTokenIdentities.Count,
            _controlPackages.Sum(static contribution => contribution.ThemeAssets.Count),
            _registeredControlThemeProviders.Count);
        MergeContributionsInto(themeManager);
        if (_packageDeclarations.Count != 0 || _startupImage is not null)
        {
            themeManager.ConfigureStartupPackages(_packageDeclarations, _startupImage);
        }
        themeManager.SetEnabledPackageIds(_controlPackages.Select(package => package.Id)
            .Concat(_packageDeclarations.Select(package => package.Definition.Id)));
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
