using AtomUI.Toolkits.GalleryBase.Localization;
using AtomUI.Toolkits.GalleryBase.Navigation;
using AtomUIGallery.ShowCases.Splash;
using ReactiveUI;
using Shouldly;
using Xunit;

namespace AtomUIGallery.Tests.ShowCases;

public class SplashShowCasePageTests
{
    [Fact]
    public void Splash_ShowCase_Is_Registered_Under_Other_Category()
    {
        var configuration      = global::AtomUIGallery.AtomUIGalleryModule.CreateConfiguration();
        var galleryProject     = ReadRepoFile("controlgallery/AtomUIGallery/AtomUIGallery.csproj");
        var assemblyInfoSource = ReadRepoFile("controlgallery/AtomUIGallery/Properties/AssemblyInfo.cs");
        var navigationEn = XliffTestDocument.Read(
            "controlgallery/AtomUIGallery/Workspace/Localization/CaseNavigationLang/en-US.xlf");
        var navigationZhCn = XliffTestDocument.Read(
            "controlgallery/AtomUIGallery/Workspace/Localization/CaseNavigationLang/zh-CN.xlf");
        var navigationZhTw = XliffTestDocument.Read(
            "controlgallery/AtomUIGallery/Workspace/Localization/CaseNavigationLang/zh-TW.xlf");

        var otherNode = Walk(configuration.NavigationNodes)
            .First(node => node.Key == "Other");
        var splashNode = Walk(configuration.NavigationNodes)
            .First(node => node.Key == SplashViewModel.ID);

        otherNode.IsRoute.ShouldBeFalse();
        otherNode.Header.ShouldBeAssignableTo<IGalleryLocalizedText>();
        otherNode.Icon.ShouldNotBeNull();
        splashNode.IsRoute.ShouldBeTrue();
        splashNode.Header.ShouldBeAssignableTo<IGalleryLocalizedText>();
        configuration.Routes.ContainsRoute(SplashViewModel.ID).ShouldBeTrue();
        // Splash 位于 AtomUI.Controls，经 AtomUI.Desktop.Controls 传递引用；不再依赖 Extras 包。
        galleryProject.ShouldContain("AtomUI.Desktop.Controls.csproj");
        assemblyInfoSource.ShouldContain("AtomUIGallery.ShowCases.Splash");

        foreach (var localization in new[] { navigationEn, navigationZhCn, navigationZhTw })
        {
            localization.ContainsKey("Other_Splash").ShouldBeTrue();
        }
    }

    [Fact]
    public void Splash_ShowCase_Merges_The_default_theme_for_the_in_page_preview()
    {
        var source = ReadRepoFile("controlgallery/AtomUIGallery/ShowCases/Other/Splash/Views/SplashShowCase.axaml");

        // Splash 默认主题不是包主题资产（提交前由 SplashWindow 自带加载），页内预览必须显式合并。
        source.ShouldContain("<ResourceDictionary.MergedDictionaries>");
        source.ShouldContain("<atom:SplashDefaultTheme/>");
        source.ShouldContain("<gallery:ShowCasePanel.Resources>");
    }

    [Fact]
    public void Splash_ShowCase_Demos_Cover_Visual_States()
    {
        var source = ReadRepoFile("controlgallery/AtomUIGallery/ShowCases/Other/Splash/Views/SplashShowCase.axaml");

        var basicDemo = ExtractShowCaseItemMarkup(source, "SplashShowCaseLangResource BasicTitle");
        basicDemo.ShouldContain("<atom:Splash");
        basicDemo.ShouldContain("Title=\"AtomUI\"");
        basicDemo.ShouldContain("IsIndeterminate=\"True\"");
        basicDemo.ShouldContain("Message=\"{gallery:SplashShowCaseLangResource P2MessagePreparingShell}\"");

        var progressDemo = ExtractShowCaseItemMarkup(source, "SplashShowCaseLangResource DeterminateTitle");
        progressDemo.ShouldContain("IsIndeterminate=\"False\"");
        progressDemo.ShouldContain("Progress=\"0.6\"");

        var errorDemo = ExtractShowCaseItemMarkup(source, "SplashShowCaseLangResource ErrorTitle");
        errorDemo.ShouldContain("Status=\"Error\"");
        errorDemo.ShouldContain("Message=\"{gallery:SplashShowCaseLangResource ErrorMessage}\"");
    }

    [Fact]
    public void Splash_ShowCase_Localization_Covers_Every_referenced_key()
    {
        var source = ReadRepoFile("controlgallery/AtomUIGallery/ShowCases/Other/Splash/Views/SplashShowCase.axaml");
        var kindSource = ReadRepoFile(
            "controlgallery/AtomUIGallery/ShowCases/Other/Splash/Localization/SplashShowCaseLangResourceKind.cs");

        var referenced = System.Text.RegularExpressions.Regex
            .Matches(source, @"SplashShowCaseLangResource (\w+)")
            .Select(match => match.Groups[1].Value)
            .ToHashSet();
        referenced.ShouldNotBeEmpty();

        foreach (var key in referenced)
        {
            kindSource.ShouldContain(key, customMessage: $"enum must declare {key}");
        }

        foreach (var language in new[] { "en-US", "zh-CN", "zh-TW", "pt-BR" })
        {
            var document = XliffTestDocument.Read(
                $"controlgallery/AtomUIGallery/ShowCases/Other/Splash/Localization/{language}.xlf");
            foreach (var key in referenced)
            {
                document.ContainsKey(key).ShouldBeTrue($"{language} must translate {key}");
                document[key].ShouldNotBeNullOrWhiteSpace();
            }
        }
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "AtomUI.slnx")))
            {
                return directory.FullName;
            }
            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("AtomUI repository root was not found.");
    }

    private static string ExtractShowCaseItemMarkup(string source, string titleMarker)
    {
        var markerIndex = source.IndexOf(titleMarker, StringComparison.Ordinal);
        markerIndex.ShouldBeGreaterThanOrEqualTo(0, $"{titleMarker} is missing from the page");

        var itemStart = source.LastIndexOf("<gallery:ShowCaseItem", markerIndex, StringComparison.Ordinal);
        itemStart.ShouldBeGreaterThanOrEqualTo(0);
        var itemEnd = source.IndexOf("</gallery:ShowCaseItem>", markerIndex, StringComparison.Ordinal);
        itemEnd.ShouldBeGreaterThan(itemStart);

        return source[itemStart..itemEnd];
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
