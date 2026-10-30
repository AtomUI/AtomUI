using AtomUI.Controls;
using AtomUI.Toolkits.GalleryBase.Navigation;
using AtomUI.Toolkits.GalleryBase.Routing;
using AtomUI.Toolkits.GalleryBase.SourceCode;
using Avalonia;

namespace AtomUI.Toolkits.GalleryBase.Configuration;

public sealed class GalleryBaseOptions
{
    public GalleryBrandingOptions Branding { get; } = new();

    public GalleryNavigationBuilder Navigation { get; } = new();

    public GalleryRouteRegistry Routes { get; } = new();

    public GalleryShellOptions Shell { get; } = new();

    public GalleryPlatformOptions Platform { get; } = new();

    public GallerySourceCodeDisplayOptions SourceCodeDisplay { get; } = new();

    public GalleryShowCaseZoomOptions ShowCaseZoom { get; } = new();

    public GalleryBaseConfiguration BuildConfiguration()
    {
        return GalleryBaseConfiguration.Create(this);
    }
}

public sealed class GalleryBrandingOptions
{
    public string AppName { get; set; } = "Gallery";

    public object? Logo { get; set; }

    public string? VersionText { get; set; }

    public IList<GalleryLink> Links { get; } = new List<GalleryLink>();
}

public sealed record GalleryLink(
    EntityKey Key,
    string Uri,
    object? Icon = null,
    string? ToolTip = null);

public sealed class GalleryShellOptions
{
    public double SidebarWidth { get; set; } = 280;

    public Size InitialDesktopWindowSize { get; set; } = new(1300, 900);

    public Size MinDesktopWindowSize { get; set; } = new(1040, 720);

    public bool IsThemeMenuEnabled { get; set; } = true;

    public bool IsLanguageMenuEnabled { get; set; } = true;

    public bool IsWindowOptionsMenuEnabled { get; set; } = true;

    public bool IsFooterVisibleWhenEmpty { get; set; }
}

public sealed class GalleryPlatformOptions
{
    public bool ConfigureBrowserOverlayLayers { get; set; } = true;

    public bool EnableBrowserMediaBreakpoints { get; set; } = true;

    public bool EnableDesktopCrashLog { get; set; } = true;

    public string CrashLogDirectoryName { get; set; } = "Gallery";
}

public sealed class GallerySourceCodeDisplayOptions
{
    public bool IsEnabled { get; set; }

    public IShowCaseCodeSnippetProvider? SnippetProvider { get; set; }
}

public sealed class GalleryShowCaseZoomOptions
{
    public bool IsEnabled { get; set; } = true;
}
