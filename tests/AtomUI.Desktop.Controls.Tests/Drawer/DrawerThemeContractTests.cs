using System.Reflection;
using AtomUI.Controls.Primitives;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.Drawer;

public class DrawerThemeContractTests
{
    static DrawerThemeContractTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Drawer_Header_Text_Binds_To_Title()
    {
        var source = File.ReadAllText(GetRepoFile("src/AtomUI.Desktop.Controls/Drawer/Themes/DrawerInfoContainerTheme.axaml"));

        source.ShouldContain("Name=\"HeaderText\"");
        source.ShouldContain("Text=\"{TemplateBinding Title}\"");
    }

    [Fact]
    public void Drawer_ContentPadding_Uses_Public_Property_And_Default_Token_Style()
    {
        var drawerSource        = File.ReadAllText(GetRepoFile("src/AtomUI.Desktop.Controls/Drawer/Drawer.cs"));
        var drawerTheme         = File.ReadAllText(GetRepoFile("src/AtomUI.Desktop.Controls/Drawer/Themes/DrawerTheme.axaml"));
        var containerSource     = File.ReadAllText(GetRepoFile("src/AtomUI.Desktop.Controls/Drawer/DrawerContainer.cs"));
        var containerTheme      = File.ReadAllText(GetRepoFile("src/AtomUI.Desktop.Controls/Drawer/Themes/DrawerContainerTheme.axaml"));
        var infoContainerSource = File.ReadAllText(GetRepoFile("src/AtomUI.Desktop.Controls/Drawer/DrawerInfoContainer.cs"));
        var infoContainerTheme  = File.ReadAllText(GetRepoFile("src/AtomUI.Desktop.Controls/Drawer/Themes/DrawerInfoContainerTheme.axaml"));

        drawerSource.ShouldContain("public static readonly StyledProperty<Thickness> ContentPaddingProperty");
        drawerSource.ShouldContain("public Thickness ContentPadding");
        drawerTheme.ShouldContain("<ControlTheme x:Key=\"{x:Type atom:Drawer}\" TargetType=\"atom:Drawer\">");
        drawerTheme.ShouldContain("<Setter Property=\"ContentPadding\" Value=\"{atom:DrawerTokenResource ContentPadding}\" />");
        containerSource.ShouldContain("ContentPaddingProperty");
        containerTheme.ShouldContain("ContentPadding=\"{TemplateBinding ContentPadding}\"");
        infoContainerSource.ShouldContain("ContentPaddingProperty");
        infoContainerTheme.ShouldContain("<Setter Property=\"Padding\" Value=\"{TemplateBinding ContentPadding}\" />");
        infoContainerTheme.ShouldNotContain("<Setter Property=\"Padding\" Value=\"{atom:DrawerTokenResource ContentPadding}\" />");
    }

    [Fact]
    public void Drawn_Decorations_Overlay_Is_Reserved_For_Window_Chrome()
    {
        var source = File.ReadAllText(GetRepoFile(
            "src/AtomUI.Desktop.Controls/Window/Themes/WindowDrawnDecorationsTheme.axaml"));

        source.ShouldContain("Name=\"PART_OverlayWrapper\"");
        source.ShouldContain("IsDrawnChromeOverlayVisible");
        source.ShouldNotContain("WindowVisualLayerClip");
        source.ShouldNotContain("PART_DrawerOverlayLayerHost");
        source.ShouldNotContain("PART_DialogOverlayLayerHost");
    }

    [Fact]
    public void Drawer_Root_Clips_To_The_Effective_Host_CornerRadius()
    {
        var source = File.ReadAllText(GetRepoFile(
            "src/AtomUI.Desktop.Controls/Drawer/Themes/DrawerContainerTheme.axaml"));

        source.ShouldContain("Name=\"PART_RootClip\"");
        source.ShouldContain("ClipToBounds=\"True\"");
        source.ShouldContain("CornerRadius=\"{TemplateBinding CornerRadius}\"");
    }

    [Fact]
    public void Drawer_ContentPadding_Propagates_To_Body_Presenter()
    {
        var expectedPadding = new Thickness(6, 7, 8, 9);
        var drawer          = new AtomUI.Desktop.Controls.Drawer
        {
            Content         = new TextBlock { Text = "Body" },
            IsMotionEnabled = false,
            Width           = 1,
            Height          = 1
        };

        var contentPaddingProperty = typeof(AtomUI.Desktop.Controls.Drawer).GetProperty(
            "ContentPadding",
            BindingFlags.Instance | BindingFlags.Public);

        contentPaddingProperty.ShouldNotBeNull();
        contentPaddingProperty!.SetValue(drawer, expectedPadding);

        var window = CreateWindow(drawer);
        try
        {
            drawer.IsOpen = true;
            Dispatcher.UIThread.RunJobs();

            var layer = TopLevelPresentationLayer.GetLayer(TopLevel.GetTopLevel(drawer)!);
            layer.ShouldNotBeNull();

            var bodyPresenter = layer.GetVisualDescendants()
                                     .OfType<ContentPresenter>()
                                     .Single(presenter => presenter.Name == "InfoContainer");
            bodyPresenter.Padding.ShouldBe(expectedPadding);
        }
        finally
        {
            window.Close();
        }
    }

    private static AvaloniaWindow CreateWindow(Control content)
    {
        var window = new AvaloniaWindow
        {
            Width   = 480,
            Height  = 360,
            Content = content
        };

        window.Show();
        Dispatcher.UIThread.RunJobs();
        return window;
    }

    private static string GetRepoFile(string relativePath)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, relativePath);
            if (File.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        throw new FileNotFoundException($"Could not find repository file: {relativePath}");
    }
}
