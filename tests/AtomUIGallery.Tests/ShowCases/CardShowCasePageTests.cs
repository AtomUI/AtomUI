using Shouldly;
using Xunit;

namespace AtomUIGallery.Tests.ShowCases;

public class CardShowCasePageTests
{

    [Fact]
    public void Card_ShowCase_Declares_Deferred_Semantic_Part_Previews_And_Example()
    {
        var source = ReadRepoFile("controlgallery/AtomUIGallery/ShowCases/DataDisplay/Card/Views/CardShowCase.axaml");

        source.ShouldContain("<gallery:GalleryShowCaseHost.SemanticPartsContentTemplate>");
        source.ShouldContain("Name=\"CardSemanticPreview\"");
        source.ShouldContain("SemanticOwnerType=\"{x:Type atom:Card}\"");
        source.ShouldContain("Name=\"CardSemanticOwner\"");
        source.ShouldContain("Name=\"CardMetaSemanticPreview\"");
        source.ShouldContain("SemanticOwnerType=\"{x:Type atom:CardMetaContent}\"");
        source.ShouldContain("SemanticOwner=\"{Binding #CardMetaSemanticOwner}\"");
        source.ShouldContain("<atom:Card Width=\"300\">");
        source.ShouldContain("Name=\"CardMetaSemanticOwner\"");
        CountOccurrences(source, "<gallery:SemanticPartDescription").ShouldBe(12);

        foreach (var path in new[]
                 {
                     "root", "header", "title", "extra", "cover", "body", "actions",
                     "section", "avatar", "description"
                 })
        {
            source.ShouldContain($"Path=\"{path}\"");
        }

        source.ShouldContain("SourceKey=\"card-semantic-part\"");
        source.ShouldContain("<atom:CardHeaderStyle x:SetterTargetType=\"atom:DashedBorder\">");
        source.ShouldContain("<atom:CardTitleStyle x:SetterTargetType=\"ContentPresenter\">");
        source.ShouldContain("<atom:CardMetaContentDescriptionStyle x:SetterTargetType=\"ContentPresenter\">");
    }

    private static string ExtractCardExampleItems(string source)
    {
        const string firstItemMarker  = "<gallery:ShowCaseItem";
        const string panelCloseMarker = "</gallery:ShowCasePanel>";

        var firstItemStart = source.IndexOf(firstItemMarker, StringComparison.Ordinal);
        firstItemStart.ShouldBeGreaterThanOrEqualTo(0);

        var panelCloseStart = source.IndexOf(panelCloseMarker, firstItemStart, StringComparison.Ordinal);
        panelCloseStart.ShouldBeGreaterThan(firstItemStart);

        return source[firstItemStart..panelCloseStart];
    }

    private static string NormalizeMarkup(string source)
    {
        return ShowCaseSnapshotMarkup.Normalize(source);
    }

    private static int CountOccurrences(string source, string value)
    {
        var count      = 0;
        var startIndex = 0;
        while (true)
        {
            var matchIndex = source.IndexOf(value, startIndex, StringComparison.Ordinal);
            if (matchIndex < 0)
            {
                return count;
            }

            count++;
            startIndex = matchIndex + value.Length;
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
