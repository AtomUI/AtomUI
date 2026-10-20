using AtomUIGallery.ShowCases.Community;
using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AvaloniaSvg = Avalonia.Svg.Svg;

namespace AtomUIGallery.Tests.Workspace;

// Guards asset resolution and theme swapping only. Visual fidelity of the wordmark is covered by
// manual acceptance on a real machine, not by this headless test.
public class CommunityPageLogoAssetTests
{
    static CommunityPageLogoAssetTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Community_Company_Logo_Resolves_Vector_Asset_For_Both_Theme_Variants()
    {
        var page   = new CommunityPage();
        var window = new Window { Content = page, Width = 1000, Height = 900 };
        window.Show();
        Dispatcher.UIThread.RunJobs();

        var light = FindLogo(page);
        light.Path.ShouldBe("/Assets/atom-innovation-logo.svg");
        light.Model.ShouldNotBeNull();
        light.Bounds.Width.ShouldBe(220);
        (light.Bounds.Width / light.Bounds.Height).ShouldBe(1508d / 415d, 0.001);

        page.SetCurrentValue(CommunityPage.IsDarkThemeModeProperty, true);
        Dispatcher.UIThread.RunJobs();

        var dark = FindLogo(page);
        dark.Path.ShouldBe("/Assets/atom-innovation-logo-white.svg");
        dark.Model.ShouldNotBeNull();
        dark.Bounds.Width.ShouldBe(220);
        (dark.Bounds.Width / dark.Bounds.Height).ShouldBe(1508d / 415d, 0.001);

        window.Close();
    }

    private static AvaloniaSvg FindLogo(CommunityPage page) =>
        page.GetVisualDescendants().OfType<AvaloniaSvg>().Single(svg => svg.Name == "AtomInnovationLogo");
}
