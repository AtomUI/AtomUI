using AtomUI.Controls;
using AtomUI.Toolkits.GalleryBase.Navigation;
using AtomUI.Toolkits.GalleryBase.Routing;
using AtomUI.Toolkits.GalleryBase.SourceCode;
using Avalonia;

namespace AtomUI.Toolkits.GalleryBase.Configuration;

/// <summary>
/// The options of a Gallery application. A <see cref="GalleryBaseConfiguration" /> is built from them, and
/// the Gallery controls read that configuration, not the options.
/// </summary>
/// <remarks>
/// <c>UseGalleryBase</c> creates the options, passes them to its callback and registers the built
/// configuration when the package is committed. An application that assembles the configuration itself
/// calls <see cref="BuildConfiguration" />.
/// </remarks>
public sealed class GalleryBaseOptions
{
    /// <summary>
    /// The brand block in the sidebar header and the links and version tag in the sidebar footer.
    /// </summary>
    public GalleryBrandingOptions Branding { get; } = new();

    /// <summary>
    /// The builder of the sidebar navigation tree. It also holds the default route and the keys of the nodes
    /// that are open by default. Validation requires the default route to be one of the built nodes.
    /// </summary>
    public GalleryNavigationBuilder Navigation { get; } = new();

    /// <summary>
    /// The registry of the routes that the Gallery can navigate to. Building the configuration takes a read-only
    /// snapshot of the registry and validates <see cref="Navigation" /> against it.
    /// </summary>
    public GalleryRouteRegistry Routes { get; } = new();

    /// <summary>
    /// Options of the shell layout: the sidebar, the desktop window sizes, the menus and the sidebar footer.
    /// </summary>
    public GalleryShellOptions Shell { get; } = new();

    /// <summary>
    /// Options that apply to only one host platform: the browser shell or the desktop host.
    /// </summary>
    public GalleryPlatformOptions Platform { get; } = new();

    /// <summary>
    /// Options of the source code drawer, which shows the code of a showcase item over the whole page.
    /// </summary>
    public GallerySourceCodeDisplayOptions SourceCodeDisplay { get; } = new();

    /// <summary>
    /// Options of the zoom overlay, which shows a showcase item enlarged over the whole page.
    /// </summary>
    public GalleryShowCaseZoomOptions ShowCaseZoom { get; } = new();

    /// <summary>
    /// Validates every option group and builds the <see cref="GalleryBaseConfiguration" /> from them.
    /// </summary>
    /// <returns>
    /// The configuration that the Gallery controls read at run time. It is a snapshot: entries added to
    /// <see cref="Navigation" /> or <see cref="Routes" /> afterwards are not part of it.
    /// </returns>
    /// <exception cref="GalleryConfigurationException">
    /// An option value is invalid or the default route is not one of the built navigation nodes.
    /// </exception>
    public GalleryBaseConfiguration BuildConfiguration()
    {
        return GalleryBaseConfiguration.Create(this);
    }
}

/// <summary>
/// Values of the brand block in the sidebar header and of the links and version tag in the sidebar footer.
/// </summary>
public sealed class GalleryBrandingOptions
{
    /// <summary>
    /// The name of the Gallery application, shown as the text of the brand block in the sidebar header when
    /// <see cref="Logo" /> is <see langword="null" />. It must not be empty or whitespace, with or without
    /// a logo.
    /// </summary>
    public string AppName { get; set; } = "Gallery";

    /// <summary>
    /// The logo of the brand block in the sidebar header. A <c>Control</c> is shown as it is. A string is loaded
    /// as an SVG from that resource path and shown 190 by 45 device-independent units in size. Any other object
    /// becomes the content of a <c>ContentControl</c>. When it is <see langword="null" />, the brand block shows
    /// <see cref="AppName" /> as text instead.
    /// </summary>
    public object? Logo { get; set; }

    /// <summary>
    /// The version shown as a tag in the sidebar footer. A <see langword="null" />, empty or whitespace value
    /// shows no tag.
    /// </summary>
    public string? VersionText { get; set; }

    /// <summary>
    /// The links of the sidebar footer, shown in this order as icon-only hyperlink buttons.
    /// Add entries to the list in place. Building the configuration throws when a link has an empty
    /// key or a URI that is not absolute.
    /// </summary>
    public IList<GalleryLink> Links { get; } = new List<GalleryLink>();
}

/// <summary>
/// A link of the sidebar footer, shown as an icon-only hyperlink button that opens the URI.
/// </summary>
/// <param name="Key">
/// Identifies the link; it must not be empty or whitespace.
/// </param>
/// <param name="Uri">
/// The target of the link. It must form an absolute URI; otherwise building the configuration throws.
/// </param>
/// <param name="Icon">
/// The icon of the button: a <c>PathIcon</c> or a function that creates one. <see langword="null" />, the
/// default, and a value of any other type give a button without an icon.
/// </param>
/// <param name="ToolTip">
/// The tooltip text of the link. The button that the shell footer creates does not display it.
/// </param>
public sealed record GalleryLink(
    EntityKey Key,
    string Uri,
    object? Icon = null,
    string? ToolTip = null);

/// <summary>
/// Options of the shell layout: the width of the sidebar, the spacing of its collapsed header, the desktop
/// window sizes, the menus and the sidebar footer.
/// </summary>
public sealed class GalleryShellOptions
{
    /// <summary>
    /// The width of the sidebar when it is expanded, in device-independent units. It must be greater than 0.
    /// </summary>
    public double SidebarWidth { get; set; } = 280;

    /// <summary>
    /// A spacing for the header of the collapsed sidebar, in device-independent units.
    /// When it has a value, the shell sets it as its own <c>CollapsedSidebarHeaderSpacing</c>; when it is
    /// <see langword="null" />, that value is left at its default. The value must be finite and not negative.
    /// </summary>
    public double? CollapsedSidebarHeaderSpacing { get; set; }

    /// <summary>
    /// The size at which the desktop window is expected to open, in device-independent units.
    /// It must be at least as large as <see cref="MinDesktopWindowSize" /> in both dimensions.
    /// The Gallery base library does not act on this value; it only validates it.
    /// </summary>
    public Size InitialDesktopWindowSize { get; set; } = new(1300, 900);

    /// <summary>
    /// The smallest size, in device-independent units, at which the desktop window is expected to be
    /// resizable. It must not exceed <see cref="InitialDesktopWindowSize" /> in either dimension.
    /// The Gallery base library does not act on this value; it only validates it.
    /// </summary>
    public Size MinDesktopWindowSize { get; set; } = new(1040, 720);

    /// <summary>
    /// Whether the Gallery application shows its theme menu. The Gallery base library does not act on this value;
    /// it only forwards it to <see cref="GalleryShellConfiguration" />, where an application that shows a theme
    /// menu reads it.
    /// </summary>
    public bool IsThemeMenuEnabled { get; set; } = true;

    /// <summary>
    /// Whether the Gallery application shows its language menu. The Gallery base library does not act on this
    /// value; it only forwards it to <see cref="GalleryShellConfiguration" />, where an application that shows a
    /// language menu reads it.
    /// </summary>
    public bool IsLanguageMenuEnabled { get; set; } = true;

    /// <summary>
    /// Whether the Gallery application shows its menu of window options. The Gallery base library does not
    /// act on this value; it only forwards it to <see cref="GalleryShellConfiguration" />, where an application
    /// that shows such a menu reads it.
    /// </summary>
    public bool IsWindowOptionsMenuEnabled { get; set; } = true;

    /// <summary>
    /// Whether the sidebar footer is created when it has nothing to show, that is no links and no version text.
    /// By default the footer is left out in that case.
    /// </summary>
    public bool IsFooterVisibleWhenEmpty { get; set; }
}

/// <summary>
/// Options that apply to only one host platform: the browser shell or the desktop host.
/// </summary>
public sealed class GalleryPlatformOptions
{
    /// <summary>
    /// Whether the browser shell turns on the overlay layer, the popup overlay layer and the light-dismiss
    /// overlay layer of the visual layer manager that wraps the shell, and creates those layers up front.
    /// </summary>
    public bool ConfigureBrowserOverlayLayers { get; set; } = true;

    /// <summary>
    /// Whether the media breakpoint follows the width of the browser shell view, so that the sidebar collapses
    /// on narrow pages. When it is <see langword="false" />, the breakpoint stays
    /// <c>MediaBreakPoint.ExtraLarge</c> at every width.
    /// </summary>
    public bool EnableBrowserMediaBreakpoints { get; set; } = true;

    /// <summary>
    /// Whether a desktop host writes a crash log for the Gallery application. The Gallery base library does not
    /// act on this value; it only forwards it to <see cref="GalleryPlatformConfiguration" />.
    /// </summary>
    public bool EnableDesktopCrashLog { get; set; } = true;

    /// <summary>
    /// The name of the directory that holds the desktop crash log. It must be a single directory name: not empty
    /// and without a path separator. The Gallery base library does not act on this value; it only validates it.
    /// </summary>
    public string CrashLogDirectoryName { get; set; } = "Gallery";
}

/// <summary>
/// Options of the source code drawer, which shows the code of a showcase item over the whole page.
/// </summary>
public sealed class GallerySourceCodeDisplayOptions
{
    /// <summary>
    /// Whether showcase items offer their source code. The source action of a showcase item is visible only
    /// when this is <see langword="true" /> and <see cref="SnippetProvider" /> is assigned.
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// The provider that supplies the code snippets of showcase items. When it is <see langword="null" />, no
    /// code is shown even if <see cref="IsEnabled" /> is <see langword="true" />.
    /// </summary>
    public IShowCaseCodeSnippetProvider? SnippetProvider { get; set; }
}

/// <summary>
/// Options of the zoom overlay, which shows a showcase item enlarged over the whole page.
/// </summary>
public sealed class GalleryShowCaseZoomOptions
{
    /// <summary>
    /// Whether showcase items can be enlarged in the zoom overlay. When it is <see langword="false" />, the
    /// zoom action is hidden on every item.
    /// </summary>
    public bool IsEnabled { get; set; } = true;
}
