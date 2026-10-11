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
    private readonly List<ControlTokenDescriptor> _controlTokenDescriptors = new();
    private readonly List<ControlThemeAssetDescriptor> _controlThemeAssetDescriptors = new();
    private readonly List<ControlSemanticDescriptor> _controlSemanticDescriptors = new();
    private readonly List<IControlThemesProvider> _controlThemesProviders = new();
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
    internal int InitializerCount => _initializers.Count;
    internal IReadOnlyList<ControlPackageRegistration> ControlPackages => _controlPackages;
    internal IReadOnlyList<IThemeDefinitionResolver> ThemeDefinitionResolvers => _themeDefinitionResolvers;
    internal string? ApplicationId { get; private set; }
    internal bool UsesUserThemeDirectory => _useUserThemeDirectory;
    internal string? UserThemeDirectory => _userThemeDirectory;

    private readonly List<ControlPackageRegistration> _controlPackages = new();

    public void AddThemeDefinitionResolver(IThemeDefinitionResolver resolver)
    {
        ArgumentNullException.ThrowIfNull(resolver);
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
                _controlTokenDescriptors.Add(descriptor);
            }
            foreach (var descriptor in package.SemanticControls)
            {
                _registeredSemanticControlIdentities.Add(descriptor.Identity);
                _registeredSemanticControlTypes.Add(descriptor.ControlType);
                _controlSemanticDescriptors.Add(descriptor);
            }
            _controlThemeAssetDescriptors.AddRange(package.ThemeAssets);
            _registeredControlThemeProviders.Add(package.ControlThemesProvider.Id);
            _controlThemesProviders.Add(package.ControlThemesProvider);
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
        _initializers.Add(initializer);
    }

    public void WithInitialTheme(string themeId, ThemeConfig? config = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(themeId);
        InitialRequest = new ThemeRequest(themeId, config, ThemeTransitionReason.Startup);
        FollowSystemLightRequest = null;
        FollowSystemDarkRequest = null;
    }

    public void WithFollowSystemThemes(ThemeRequest light, ThemeRequest dark)
    {
        ArgumentNullException.ThrowIfNull(light);
        ArgumentNullException.ThrowIfNull(dark);
        FollowSystemLightRequest = light with { Reason = ThemeTransitionReason.FollowSystem };
        FollowSystemDarkRequest = dark with { Reason = ThemeTransitionReason.FollowSystem };
        InitialRequest = FollowSystemLightRequest;
    }

    public void WithApplicationId(string applicationId)
    {
        ThemeApplicationIdentity.Validate(applicationId, nameof(applicationId));
        ApplicationId = applicationId;
    }

    public void UseUserThemeDirectory()
    {
        _useUserThemeDirectory = true;
        _userThemeDirectory = null;
    }

    public void UseUserThemeDirectory(string directory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        _useUserThemeDirectory = true;
        _userThemeDirectory = Path.GetFullPath(directory);
    }

    public void WithDefaultFontFamily(FontFamily fontFamily)
    {
        FontFamily = fontFamily;
    }

    public void WithDefaultFontFamily(string fontFamily)
    {
        FontFamily = FontFamily.Parse(fontFamily);
    }

    internal ThemeManager Build()
    {
        try
        {
            ControlRegistrationState.Freeze();
            return BuildCore();
        }
        catch (Exception exception)
        {
            ControlRegistrationState.Fail(exception);
            throw;
        }
    }

    private ThemeManager BuildCore()
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

        var themeManager = new ThemeManager
        {
            FontFamily = FontFamily,
            SemanticParts = _controlSemanticDescriptors.Count == 0
                ? SemanticPartRegistry.Empty
                : new SemanticPartRegistry(_controlSemanticDescriptors)
        };
        themeManager.ConfigureStartup(
            InitialRequest,
            FollowSystemLightRequest,
            FollowSystemDarkRequest);
        themeManager.ConfigureThemeDefinitions(
            themeDefinitionResolvers,
            ApplicationId!,
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
        themeManager.EnsureRegistrationCapacity(
            _controlTokenDescriptors.Count,
            _controlThemeAssetDescriptors.Count,
            _controlThemesProviders.Count);

        foreach (var provider in _controlThemesProviders)
        {
            themeManager.RegisterControlThemesProvider(provider);
        }
        foreach (var descriptor in _controlTokenDescriptors)
        {
            themeManager.RegisterControlTokenDescriptor(descriptor);
        }
        var assets = new Dictionary<string, ControlThemeAssetDescriptor>(StringComparer.Ordinal);
        var resources = new Dictionary<string, ControlThemeResourceRegistration>(StringComparer.Ordinal);
        foreach (var package in _controlPackages)
        {
            foreach (var asset in package.ThemeAssets)
            {
                if (assets.TryGetValue(asset.AssetId, out var existing))
                {
                    if (!existing.HasSameMetadata(asset))
                    {
                        throw new ThemeSchemaException($"Theme asset '{asset.AssetId}' has conflicting package definitions.");
                    }
                    continue;
                }
                assets.Add(asset.AssetId, asset);
                themeManager.RegisterControlThemeAssetDescriptor(asset);
            }
            var packageResources = new List<ControlThemeResourceRegistration>();
            foreach (var resource in package.Resources)
            {
                if (resources.TryGetValue(resource.AssetId, out var existing))
                {
                    if (!existing.HasSameMetadata(resource))
                    {
                        throw new ThemeSchemaException($"Theme resource '{resource.AssetId}' has conflicting package definitions.");
                    }
                    continue;
                }
                resources.Add(resource.AssetId, resource);
                packageResources.Add(resource);
            }
            themeManager.StageControlThemeResources(package.ControlThemesProvider, packageResources);
        }
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
        themeManager.PrepareRegistrationSchema();
        return themeManager;
    }
}
