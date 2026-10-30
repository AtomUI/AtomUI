using AtomUI.Controls;
using AtomUI.Toolkits.GalleryBase.Navigation;
using Shouldly;
using Xunit;

namespace AtomUIGallery.Tests.Workspace;

public class GalleryLandingPagesTests
{
    [Fact]
    public void Navigation_Uses_Overview_Community_And_Components_As_Top_Level_Items()
    {
        var configuration = global::AtomUIGallery.AtomUIGalleryModule.CreateConfiguration();
        var topLevelKeys  = configuration.NavigationNodes.Select(node => node.Key.Value).ToArray();

        topLevelKeys.ShouldBe(["Overview", "Community", "Components"]);
        configuration.DefaultOpenKeys.ShouldContain(new EntityKey("Components"));
        configuration.NavigationNodes[0].IsRoute.ShouldBeTrue();
        configuration.NavigationNodes[1].IsRoute.ShouldBeTrue();
        configuration.NavigationNodes[2].IsRoute.ShouldBeFalse();
        Walk(configuration.NavigationNodes)
            .ShouldContain(node => node.Key == "General" && !node.IsRoute);
        Walk(configuration.NavigationNodes)
            .ShouldNotContain(node => node.Key == "AboutUs" || node.Key == "General_AboutUs");

        var codeBehindSource = ReadRepoFile("controlgallery/AtomUIGallery/Workspace/Views/CaseNavigation.axaml.cs");
        codeBehindSource.ShouldContain("ShowCaseNavMenu.DefaultOpenPaths");
        codeBehindSource.ShouldContain("GalleryNavigationMenuAdapter");
        codeBehindSource.ShouldContain("ViewModel.CanNavigateTo(key.Value)");
        codeBehindSource.ShouldContain("ViewModel.NavigateToCommand.Execute(key.Value)");
    }

    [Fact]
    public void Overview_Page_Uses_Current_Version_In_NuGet_Install_Command()
    {
        var source = ReadRepoFile("controlgallery/AtomUIGallery/ShowCases/General/Overview/Views/OverviewPage.axaml");

        source.ShouldContain("dotnet add package AtomUI.Desktop.Controls --version");
        source.ShouldContain("{x:Static gallery:GalleryVersionInfo.Version}");
    }

    [Fact]
    public void Community_Company_Logo_Uses_Dtd_Free_Vector_Assets_For_Both_Theme_Variants()
    {
        var source    = ReadRepoFile("controlgallery/AtomUIGallery/ShowCases/General/Community/Views/CommunityPage.axaml");
        var lightLogo = ReadRepoFile("controlgallery/AtomUIGallery/Assets/atom-innovation-logo.svg");
        var darkLogo  = ReadRepoFile("controlgallery/AtomUIGallery/Assets/atom-innovation-logo-white.svg");

        source.ShouldContain("/Assets/atom-innovation-logo.svg");

        var darkThemeScopeIndex = source.IndexOf("views|CommunityPage[IsDarkThemeMode=True]", StringComparison.Ordinal);
        darkThemeScopeIndex.ShouldBeGreaterThanOrEqualTo(0);
        source.IndexOf("/Assets/atom-innovation-logo-white.svg", StringComparison.Ordinal)
              .ShouldBeGreaterThan(darkThemeScopeIndex);

        lightLogo.ShouldContain("fill=\"#DE2910\"");
        darkLogo.ShouldContain("fill=\"#ffffff\"");
        foreach (var logo in new[] { lightLogo, darkLogo })
        {
            logo.ShouldContain("xmlns=\"http://www.w3.org/2000/svg\"");
            logo.ShouldNotContain("<!DOCTYPE");
            logo.ShouldNotContain("<!ENTITY");
            logo.ShouldNotContain("SYSTEM");
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

    private static string ReadRepoFile(string relativePath)
    {
        var path = GetRepoFile(relativePath);
        File.Exists(path).ShouldBeTrue($"Expected repository file to exist: {relativePath}");
        return File.ReadAllText(path);
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

        return Path.Combine(AppContext.BaseDirectory, relativePath);
    }
}
