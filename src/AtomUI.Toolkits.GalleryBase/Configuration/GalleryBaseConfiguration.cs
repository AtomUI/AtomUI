using AtomUI.Controls;
using AtomUI.Toolkits.GalleryBase.Navigation;
using AtomUI.Toolkits.GalleryBase.Routing;
using AtomUI.Toolkits.GalleryBase.SourceCode;
using Avalonia;

namespace AtomUI.Toolkits.GalleryBase.Configuration;

public sealed class GalleryBaseConfiguration
{
    public GalleryBrandingConfiguration Branding { get; }

    public IReadOnlyList<GalleryNavigationNode> NavigationNodes { get; }

    public IReadOnlyList<EntityKey> DefaultOpenKeys { get; }

    public EntityKey DefaultRoute { get; }

    public GalleryRouteRegistry Routes { get; }

    public GalleryShellConfiguration Shell { get; }

    public GalleryPlatformConfiguration Platform { get; }

    public GallerySourceCodeDisplayConfiguration SourceCodeDisplay { get; }

    public GalleryShowCaseZoomConfiguration ShowCaseZoom { get; }

    private GalleryBaseConfiguration(GalleryBrandingConfiguration branding,
                                     IReadOnlyList<GalleryNavigationNode> navigationNodes,
                                     IReadOnlyList<EntityKey> defaultOpenKeys,
                                     EntityKey defaultRoute,
                                     GalleryRouteRegistry routes,
                                     GalleryShellConfiguration shell,
                                     GalleryPlatformConfiguration platform,
                                     GallerySourceCodeDisplayConfiguration sourceCodeDisplay,
                                     GalleryShowCaseZoomConfiguration showCaseZoom)
    {
        Branding          = branding;
        NavigationNodes   = navigationNodes;
        DefaultOpenKeys   = defaultOpenKeys;
        DefaultRoute      = defaultRoute;
        Routes            = routes;
        Shell             = shell;
        Platform          = platform;
        SourceCodeDisplay = sourceCodeDisplay;
        ShowCaseZoom      = showCaseZoom;
    }

    internal static GalleryBaseConfiguration Create(GalleryBaseOptions options)
    {
        var navigationNodes = options.Navigation.BuildNodes();
        var routes          = options.Routes.ToReadOnlySnapshot();
        ValidateNavigation(options.Navigation, navigationNodes, routes);
        ValidateShell(options.Shell);
        ValidateBranding(options.Branding);
        ValidatePlatform(options.Platform);

        return new GalleryBaseConfiguration(
            GalleryBrandingConfiguration.FromOptions(options.Branding),
            navigationNodes,
            Array.AsReadOnly(options.Navigation.DefaultOpenKeys.ToArray()),
            options.Navigation.DefaultRoute,
            routes,
            GalleryShellConfiguration.FromOptions(options.Shell),
            GalleryPlatformConfiguration.FromOptions(options.Platform),
            GallerySourceCodeDisplayConfiguration.FromOptions(options.SourceCodeDisplay),
            GalleryShowCaseZoomConfiguration.FromOptions(options.ShowCaseZoom));
    }

    private static void ValidateNavigation(GalleryNavigationBuilder navigation,
                                           IReadOnlyList<GalleryNavigationNode> navigationNodes,
                                           GalleryRouteRegistry routes)
    {
        if (string.IsNullOrWhiteSpace(navigation.DefaultRoute.Value))
        {
            throw new GalleryConfigurationException("Gallery navigation DefaultRoute must be configured.");
        }

        var defaultNode = Walk(navigationNodes).FirstOrDefault(node => node.Key == navigation.DefaultRoute);
        if (defaultNode is null)
        {
            throw new GalleryConfigurationException(
                $"Gallery navigation DefaultRoute '{navigation.DefaultRoute}' does not exist in navigation nodes.");
        }

        if (!defaultNode.IsRoute)
        {
            throw new GalleryConfigurationException(
                $"Gallery navigation DefaultRoute '{navigation.DefaultRoute}' must be a page route node.");
        }

        if (!routes.ContainsRoute(navigation.DefaultRoute))
        {
            throw new GalleryConfigurationException(
                $"Gallery navigation DefaultRoute '{navigation.DefaultRoute}' does not have a registered route.");
        }

        var nodesByKey = Walk(navigationNodes).ToDictionary(node => node.Key);
        var defaultOpenKeys = new HashSet<EntityKey>();
        foreach (var defaultOpenKey in navigation.DefaultOpenKeys)
        {
            if (string.IsNullOrWhiteSpace(defaultOpenKey.Value))
            {
                throw new GalleryConfigurationException(
                    "Gallery navigation DefaultOpenKeys must not contain an empty key.");
            }

            if (!defaultOpenKeys.Add(defaultOpenKey))
            {
                throw new GalleryConfigurationException(
                    $"Gallery navigation DefaultOpenKey '{defaultOpenKey}' is configured more than once.");
            }

            if (!nodesByKey.TryGetValue(defaultOpenKey, out var defaultOpenNode))
            {
                throw new GalleryConfigurationException(
                    $"Gallery navigation DefaultOpenKey '{defaultOpenKey}' does not exist in navigation nodes.");
            }

            if (defaultOpenNode.IsRoute)
            {
                throw new GalleryConfigurationException(
                    $"Gallery navigation DefaultOpenKey '{defaultOpenKey}' must be a group navigation node.");
            }
        }

        foreach (var routeNode in Walk(navigationNodes).Where(node => node.IsRoute))
        {
            if (!routes.ContainsRoute(routeNode.Key))
            {
                throw new GalleryConfigurationException(
                    $"Gallery navigation page '{routeNode.Key}' does not have a registered route.");
            }
        }
    }

    private static IEnumerable<GalleryNavigationNode> Walk(IEnumerable<GalleryNavigationNode> nodes)
    {
        foreach (var node in nodes)
        {
            yield return node;
            foreach (var child in Walk(node.Children))
            {
                yield return child;
            }
        }
    }

    private static void ValidateShell(GalleryShellOptions shell)
    {
        if (shell.SidebarWidth <= 0)
        {
            throw new GalleryConfigurationException("Gallery shell SidebarWidth must be greater than 0.");
        }

        if (shell.MinDesktopWindowSize.Width > shell.InitialDesktopWindowSize.Width ||
            shell.MinDesktopWindowSize.Height > shell.InitialDesktopWindowSize.Height)
        {
            throw new GalleryConfigurationException(
                "Gallery shell MinDesktopWindowSize must not exceed InitialDesktopWindowSize.");
        }
    }

    private static void ValidateBranding(GalleryBrandingOptions branding)
    {
        if (string.IsNullOrWhiteSpace(branding.AppName))
        {
            throw new GalleryConfigurationException("Gallery branding AppName must not be empty.");
        }

        foreach (var link in branding.Links)
        {
            if (string.IsNullOrWhiteSpace(link.Key.Value))
            {
                throw new GalleryConfigurationException("Gallery branding link Key must not be empty.");
            }

            if (!Uri.TryCreate(link.Uri, UriKind.Absolute, out _))
            {
                throw new GalleryConfigurationException(
                    $"Gallery branding link '{link.Key}' has an invalid Uri '{link.Uri}'.");
            }
        }
    }

    private static void ValidatePlatform(GalleryPlatformOptions platform)
    {
        if (string.IsNullOrWhiteSpace(platform.CrashLogDirectoryName) ||
            platform.CrashLogDirectoryName.Contains('/') ||
            platform.CrashLogDirectoryName.Contains('\\'))
        {
            throw new GalleryConfigurationException(
                "Gallery platform CrashLogDirectoryName must be a non-empty directory name segment.");
        }
    }
}

public sealed class GalleryBrandingConfiguration
{
    public string AppName { get; }

    public object? Logo { get; }

    public string? VersionText { get; }

    public IReadOnlyList<GalleryLink> Links { get; }

    private GalleryBrandingConfiguration(string appName,
                                         object? logo,
                                         string? versionText,
                                         IReadOnlyList<GalleryLink> links)
    {
        AppName     = appName;
        Logo        = logo;
        VersionText = versionText;
        Links       = links;
    }

    internal static GalleryBrandingConfiguration FromOptions(GalleryBrandingOptions options)
    {
        return new GalleryBrandingConfiguration(
            options.AppName,
            options.Logo,
            options.VersionText,
            Array.AsReadOnly(options.Links.ToArray()));
    }
}

public sealed class GalleryShellConfiguration
{
    public double SidebarWidth { get; }

    public Size InitialDesktopWindowSize { get; }

    public Size MinDesktopWindowSize { get; }

    public bool IsThemeMenuEnabled { get; }

    public bool IsLanguageMenuEnabled { get; }

    public bool IsWindowOptionsMenuEnabled { get; }

    public bool IsFooterVisibleWhenEmpty { get; }

    private GalleryShellConfiguration(double sidebarWidth,
                                      Size initialDesktopWindowSize,
                                      Size minDesktopWindowSize,
                                      bool isThemeMenuEnabled,
                                      bool isLanguageMenuEnabled,
                                      bool isWindowOptionsMenuEnabled,
                                      bool isFooterVisibleWhenEmpty)
    {
        SidebarWidth               = sidebarWidth;
        InitialDesktopWindowSize   = initialDesktopWindowSize;
        MinDesktopWindowSize       = minDesktopWindowSize;
        IsThemeMenuEnabled         = isThemeMenuEnabled;
        IsLanguageMenuEnabled      = isLanguageMenuEnabled;
        IsWindowOptionsMenuEnabled = isWindowOptionsMenuEnabled;
        IsFooterVisibleWhenEmpty   = isFooterVisibleWhenEmpty;
    }

    internal static GalleryShellConfiguration FromOptions(GalleryShellOptions options)
    {
        return new GalleryShellConfiguration(
            options.SidebarWidth,
            options.InitialDesktopWindowSize,
            options.MinDesktopWindowSize,
            options.IsThemeMenuEnabled,
            options.IsLanguageMenuEnabled,
            options.IsWindowOptionsMenuEnabled,
            options.IsFooterVisibleWhenEmpty);
    }
}

public sealed class GalleryPlatformConfiguration
{
    public bool ConfigureBrowserOverlayLayers { get; }

    public bool EnableBrowserMediaBreakpoints { get; }

    public bool EnableDesktopCrashLog { get; }

    public string CrashLogDirectoryName { get; }

    private GalleryPlatformConfiguration(bool configureBrowserOverlayLayers,
                                         bool enableBrowserMediaBreakpoints,
                                         bool enableDesktopCrashLog,
                                         string crashLogDirectoryName)
    {
        ConfigureBrowserOverlayLayers = configureBrowserOverlayLayers;
        EnableBrowserMediaBreakpoints = enableBrowserMediaBreakpoints;
        EnableDesktopCrashLog         = enableDesktopCrashLog;
        CrashLogDirectoryName         = crashLogDirectoryName;
    }

    internal static GalleryPlatformConfiguration FromOptions(GalleryPlatformOptions options)
    {
        return new GalleryPlatformConfiguration(
            options.ConfigureBrowserOverlayLayers,
            options.EnableBrowserMediaBreakpoints,
            options.EnableDesktopCrashLog,
            options.CrashLogDirectoryName);
    }
}

public sealed class GallerySourceCodeDisplayConfiguration
{
    public bool IsEnabled { get; }

    public IShowCaseCodeSnippetProvider? SnippetProvider { get; }

    public bool CanShowSourceCode => IsEnabled && SnippetProvider is not null;

    private GallerySourceCodeDisplayConfiguration(bool isEnabled,
                                                  IShowCaseCodeSnippetProvider? snippetProvider)
    {
        IsEnabled       = isEnabled;
        SnippetProvider = snippetProvider;
    }

    internal static GallerySourceCodeDisplayConfiguration FromOptions(GallerySourceCodeDisplayOptions options)
    {
        return new GallerySourceCodeDisplayConfiguration(options.IsEnabled, options.SnippetProvider);
    }
}

public sealed class GalleryShowCaseZoomConfiguration
{
    public bool IsEnabled { get; }

    private GalleryShowCaseZoomConfiguration(bool isEnabled)
    {
        IsEnabled = isEnabled;
    }

    internal static GalleryShowCaseZoomConfiguration FromOptions(GalleryShowCaseZoomOptions options)
    {
        return new GalleryShowCaseZoomConfiguration(options.IsEnabled);
    }
}
