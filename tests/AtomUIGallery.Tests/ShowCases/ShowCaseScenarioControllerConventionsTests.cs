using Shouldly;
using Xunit;

namespace AtomUIGallery.Tests.ShowCases;

public class ShowCaseScenarioControllerConventionsTests
{
    [Fact]
    public void Standard_ShowCases_Render_Examples_Directly_Without_Api_Or_Token_Tabs()
    {
        var standardShowCases = GetStandardDocumentedShowCaseFiles();
        standardShowCases.Count.ShouldBeGreaterThan(0);

        var failures = new List<string>();
        foreach (var pagePath in standardShowCases)
        {
            var relativePagePath = Path.GetRelativePath(GetRepoRoot(), pagePath);
            var source           = File.ReadAllText(pagePath);

            var usesExamplesOnlyHost = source.Contains(
                "<gallery:GalleryStickyTabsHost",
                StringComparison.Ordinal);
            var usesSemanticPartsHost = source.Contains(
                "<gallery:GalleryShowCaseHost",
                StringComparison.Ordinal);
            var declaresSemanticParts = source.Contains(
                "<gallery:GalleryShowCaseHost.SemanticPartsContentTemplate>",
                StringComparison.Ordinal);

            if (!usesExamplesOnlyHost && !usesSemanticPartsHost)
            {
                failures.Add(
                    $"{relativePagePath}: must use GalleryStickyTabsHost or the Semantic-aware GalleryShowCaseHost shell.");
            }

            if (usesSemanticPartsHost != declaresSemanticParts)
            {
                failures.Add(
                    $"{relativePagePath}: GalleryShowCaseHost and SemanticPartsContentTemplate must be declared together.");
            }

            if (!source.Contains("<gallery:ShowCasePanel Name=\"ExamplesContent\"", StringComparison.Ordinal))
            {
                failures.Add($"{relativePagePath}: must render ExamplesContent as the direct showcase body.");
            }

            foreach (var removedMarker in new[]
                     {
                         "<gallery:GalleryStickyTabsHost.StickyContent>",
                         "Name=\"ScenarioTabs\"",
                         "Name=\"ScenarioContentHost\"",
                         "Tag=\"Api\"",
                         "Tag=\"DesignToken\"",
                         "ScenarioApi",
                         "ScenarioDesignToken"
                     })
            {
                if (source.Contains(removedMarker, StringComparison.Ordinal))
                {
                    failures.Add($"{relativePagePath}: must not declare obsolete scenario marker {removedMarker}.");
                }
            }

            if (source.Contains("ItemsSource=\"{Binding ApiRows}\"", StringComparison.Ordinal) ||
                source.Contains("ItemsSource=\"{Binding DesignTokenRows}\"", StringComparison.Ordinal))
            {
                failures.Add($"{relativePagePath}: must not bind removed API or token metadata rows.");
            }
        }

        failures.ShouldBeEmpty();
    }

    private static IReadOnlyList<string> GetStandardDocumentedShowCaseFiles()
    {
        return Directory
               .EnumerateFiles(Path.Combine(GetRepoRoot(), "controlgallery/AtomUIGallery/ShowCases"),
                                "*ShowCase.axaml",
                                SearchOption.AllDirectories)
               .Where(IsMainShowCasePage)
               .Where(path =>
               {
                   var source = File.ReadAllText(path);
                   return !source.Contains("Name=\"ScenarioTabs\"", StringComparison.Ordinal) &&
                          !source.Contains("Name=\"ScenarioContentHost\"", StringComparison.Ordinal);
               })
               .Order(StringComparer.Ordinal)
               .ToArray();
    }

    private static bool IsMainShowCasePage(string path)
    {
        var fileName    = Path.GetFileNameWithoutExtension(path);
        var controlName = Directory.GetParent(path)?.Parent?.Name;
        return string.Equals(fileName, $"{controlName}ShowCase", StringComparison.Ordinal);
    }

    private static string GetRepoRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, "controlgallery/AtomUIGallery/ShowCases");
            if (Directory.Exists(candidate))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        return AppContext.BaseDirectory;
    }
}
