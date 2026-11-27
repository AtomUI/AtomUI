using AtomUI.Controls;
using AtomUI.Toolkits.GalleryBase.Navigation;
using AtomUI.Toolkits.GalleryBase.Routing;
using AtomUI.Toolkits.GalleryBase.SourceCode;
using Avalonia;

namespace AtomUI.Toolkits.GalleryBase.Configuration;

/// <summary>
/// The validated configuration of a Gallery application, built from <see cref="GalleryBaseOptions" /> and read
/// by the Gallery controls at run time.
/// </summary>
/// <remarks>
/// Building it throws a <see cref="GalleryConfigurationException" /> when an option value is invalid or the
/// default route is not one of the built navigation nodes. It is a snapshot: navigation nodes and routes
/// added to the options afterwards are not part of it.
/// </remarks>
public sealed class GalleryBaseConfiguration
{
    /// <summary>
    /// The branding of the shell: the brand block in the sidebar header and the links and version tag in the
    /// sidebar footer.
    /// </summary>
    public GalleryBrandingConfiguration Branding { get; }

    /// <summary>
    /// The navigation tree of the sidebar, built by the navigation builder of the options. Every page node in
    /// it, at any depth, has a route in <see cref="Routes" />, or building the configuration fails.
    /// </summary>
    public IReadOnlyList<GalleryNavigationNode> NavigationNodes { get; }

    /// <summary>
    /// The keys of the navigation groups that are open at startup. Each key names a group node of
    /// <see cref="NavigationNodes" /> at any depth.
    /// </summary>
    public IReadOnlyList<EntityKey> DefaultOpenKeys { get; }

    /// <summary>
    /// The route that the navigation view model navigates to when its activator signals activation. It names a
    /// page node of <see cref="NavigationNodes" /> that has a route in <see cref="Routes" />.
    /// </summary>
    public EntityKey DefaultRoute { get; }

    /// <summary>
    /// The routes that the Gallery can navigate to, as a read-only snapshot of the route registry in the options.
    /// Routes added to the registry after the configuration was built are not part of it.
    /// </summary>
    public GalleryRouteRegistry Routes { get; }

    /// <summary>
    /// The shell layout: the width of the sidebar, the spacing of its collapsed header, the desktop window sizes,
    /// the menus and the sidebar footer.
    /// </summary>
    public GalleryShellConfiguration Shell { get; }

    /// <summary>
    /// The settings that apply to only one host platform: the browser shell or the desktop host.
    /// </summary>
    public GalleryPlatformConfiguration Platform { get; }

    /// <summary>
    /// The settings of the source code drawer, which shows the code of a showcase item over the whole page.
    /// <see cref="GallerySourceCodeDisplayConfiguration.CanShowSourceCode" /> tells whether the drawer can open.
    /// </summary>
    public GallerySourceCodeDisplayConfiguration SourceCodeDisplay { get; }

    /// <summary>
    /// The settings of the zoom overlay, which shows a showcase item enlarged over the whole page.
    /// </summary>
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

        var nodesByKey = new Dictionary<EntityKey, GalleryNavigationNode>();
        GalleryNavigationNode? firstUnregisteredRoute = null;
        IndexNavigation(navigationNodes, routes, nodesByKey, ref firstUnregisteredRoute);
        if (!nodesByKey.TryGetValue(navigation.DefaultRoute, out var defaultNode))
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

        if (firstUnregisteredRoute is not null)
        {
            throw new GalleryConfigurationException(
                $"Gallery navigation page '{firstUnregisteredRoute.Key}' does not have a registered route.");
        }
    }

    private static void IndexNavigation(IReadOnlyList<GalleryNavigationNode> nodes,
                                         GalleryRouteRegistry routes,
                                         Dictionary<EntityKey, GalleryNavigationNode> nodesByKey,
                                         ref GalleryNavigationNode? firstUnregisteredRoute)
    {
        foreach (var node in nodes)
        {
            nodesByKey.Add(node.Key, node);
            if (firstUnregisteredRoute is null && node.IsRoute && !routes.ContainsRoute(node.Key))
            {
                firstUnregisteredRoute = node;
            }
            IndexNavigation(node.Children, routes, nodesByKey, ref firstUnregisteredRoute);
        }
    }

    private static void ValidateShell(GalleryShellOptions shell)
    {
        if (shell.SidebarWidth <= 0)
        {
            throw new GalleryConfigurationException("Gallery shell SidebarWidth must be greater than 0.");
        }

        if (shell.CollapsedSidebarHeaderSpacing is { } collapsedSidebarHeaderSpacing &&
            (collapsedSidebarHeaderSpacing < 0 || !double.IsFinite(collapsedSidebarHeaderSpacing)))
        {
            throw new GalleryConfigurationException(
                "Gallery shell CollapsedSidebarHeaderSpacing must be a finite value greater than or equal to 0.");
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

/// <summary>
/// The branding of the shell: the values of the brand block in the sidebar header and of the links and
/// version tag in the sidebar footer.
/// </summary>
public sealed class GalleryBrandingConfiguration
{
    /// <summary>
    /// The name of the Gallery application, shown as the text of the brand block in the sidebar header when
    /// <see cref="Logo" /> is <see langword="null" />. It is never empty or whitespace.
    /// </summary>
    public string AppName { get; }

    /// <summary>
    /// The logo of the brand block in the sidebar header. A <c>Control</c> is shown as it is. A string is loaded
    /// as an SVG from that resource path and shown 190 by 45 device-independent units in size. Any other object
    /// becomes the content of a <c>ContentControl</c>. When it is <see langword="null" />, the brand block shows
    /// <see cref="AppName" /> as text instead.
    /// </summary>
    public object? Logo { get; }

    /// <summary>
    /// The version shown as a tag in the sidebar footer. A <see langword="null" />, empty or whitespace value shows
    /// no tag.
    /// </summary>
    public string? VersionText { get; }

    /// <summary>
    /// The links of the sidebar footer, in display order. Each is shown as a hyperlink button without text that
    /// opens its URI.
    /// </summary>
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

/// <summary>
/// The shell layout of a Gallery application: the width of the sidebar, the spacing of its collapsed header,
/// the desktop window sizes, the menus and the sidebar footer.
/// </summary>
public sealed class GalleryShellConfiguration
{
    /// <summary>
    /// The width of the sidebar when it is expanded, in device-independent units. It is greater than 0.
    /// </summary>
    public double SidebarWidth { get; }

    /// <summary>
    /// A spacing for the header of the collapsed sidebar, in device-independent units. When it has a value, the
    /// shell sets it as its own <c>CollapsedSidebarHeaderSpacing</c>; when it is <see langword="null" />, that
    /// value is left at its default.
    /// </summary>
    public double? CollapsedSidebarHeaderSpacing { get; }

    /// <summary>
    /// The size at which the desktop window is expected to open, in device-independent units. It is at least as
    /// large as <see cref="MinDesktopWindowSize" /> in both dimensions. The Gallery base library does not act on
    /// this value; it only validates it.
    /// </summary>
    public Size InitialDesktopWindowSize { get; }

    /// <summary>
    /// The smallest size, in device-independent units, at which the desktop window is expected to be resizable.
    /// It does not exceed <see cref="InitialDesktopWindowSize" /> in either dimension. The Gallery base library
    /// does not act on this value; it only validates it.
    /// </summary>
    public Size MinDesktopWindowSize { get; }

    /// <summary>
    /// Whether the Gallery application shows its theme menu. The Gallery base library does not act on this value.
    /// </summary>
    public bool IsThemeMenuEnabled { get; }

    /// <summary>
    /// Whether the Gallery application shows its language menu. The Gallery base library does not act on this
    /// value.
    /// </summary>
    public bool IsLanguageMenuEnabled { get; }

    /// <summary>
    /// Whether the Gallery application shows its menu of window options. The Gallery base library does not act on
    /// this value.
    /// </summary>
    public bool IsWindowOptionsMenuEnabled { get; }

    /// <summary>
    /// Whether the shell creates the sidebar footer when it has nothing to show, that is no links and no version
    /// text. When it is <see langword="false" />, the shell creates no footer in that case.
    /// </summary>
    public bool IsFooterVisibleWhenEmpty { get; }

    private GalleryShellConfiguration(double sidebarWidth,
                                      double? collapsedSidebarHeaderSpacing,
                                      Size initialDesktopWindowSize,
                                      Size minDesktopWindowSize,
                                      bool isThemeMenuEnabled,
                                      bool isLanguageMenuEnabled,
                                      bool isWindowOptionsMenuEnabled,
                                      bool isFooterVisibleWhenEmpty)
    {
        SidebarWidth                  = sidebarWidth;
        CollapsedSidebarHeaderSpacing = collapsedSidebarHeaderSpacing;
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
            options.CollapsedSidebarHeaderSpacing,
            options.InitialDesktopWindowSize,
            options.MinDesktopWindowSize,
            options.IsThemeMenuEnabled,
            options.IsLanguageMenuEnabled,
            options.IsWindowOptionsMenuEnabled,
            options.IsFooterVisibleWhenEmpty);
    }
}

/// <summary>
/// The settings that apply to only one host platform: the browser shell or the desktop host.
/// </summary>
public sealed class GalleryPlatformConfiguration
{
    /// <summary>
    /// Whether the browser shell turns on the overlay layer, the popup overlay layer and the light-dismiss overlay
    /// layer of the visual layer manager that wraps the shell, and creates those layers up front.
    /// </summary>
    public bool ConfigureBrowserOverlayLayers { get; }

    /// <summary>
    /// Whether the media breakpoint follows the width of the browser shell view, so that the sidebar collapses on
    /// narrow pages. When it is <see langword="false" />, the breakpoint stays <c>MediaBreakPoint.ExtraLarge</c> at
    /// every width.
    /// </summary>
    public bool EnableBrowserMediaBreakpoints { get; }

    /// <summary>
    /// Whether a desktop host writes a crash log for the Gallery application. The Gallery base library does not
    /// act on this value.
    /// </summary>
    public bool EnableDesktopCrashLog { get; }

    /// <summary>
    /// The name of the directory that holds the desktop crash log. It is a single directory name: not empty and
    /// without a path separator. The Gallery base library does not act on this value; it only validates it.
    /// </summary>
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

/// <summary>
/// The settings of the source code drawer, which shows the code of a showcase item over the whole page.
/// </summary>
public sealed class GallerySourceCodeDisplayConfiguration
{
    /// <summary>
    /// Whether showcase items offer their source code. The source action of a showcase item is visible only when
    /// this is <see langword="true" /> and <see cref="SnippetProvider" /> is assigned.
    /// </summary>
    public bool IsEnabled { get; }

    /// <summary>
    /// The <see cref="IShowCaseCodeSnippetProvider" /> that supplies the code snippets of showcase items. When it
    /// is <see langword="null" />, no code is shown even if <see cref="IsEnabled" /> is <see langword="true" />.
    /// </summary>
    public IShowCaseCodeSnippetProvider? SnippetProvider { get; }

    /// <summary>
    /// Whether the source code drawer can be used: <see cref="IsEnabled" /> is <see langword="true" /> and
    /// <see cref="SnippetProvider" /> is assigned. The source action of a showcase item is visible only then.
    /// </summary>
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

/// <summary>
/// The settings of the zoom overlay, which shows a showcase item enlarged over the whole page.
/// </summary>
public sealed class GalleryShowCaseZoomConfiguration
{
    /// <summary>
    /// Whether showcase items can be enlarged in the zoom overlay. When it is <see langword="false" />, the zoom
    /// action is hidden on every item.
    /// </summary>
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
