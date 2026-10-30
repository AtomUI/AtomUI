using System.Windows.Input;
using AtomUIGallery.ShowCases.Segmented;
using Shouldly;
using Xunit;

namespace AtomUIGallery.Tests.ShowCases;

public class SegmentedShowCasePageTests
{

    [Fact]
    public void Segmented_ShowCase_Declares_The_Semantic_Previews_And_Style_Example()
    {
        var source = ReadRepoFile(
            "controlgallery/AtomUIGallery/ShowCases/DataDisplay/Segmented/Views/SegmentedShowCase.axaml");
        var english = ReadRepoFile(
            "controlgallery/AtomUIGallery/ShowCases/DataDisplay/Segmented/Localization/en-US.xlf");
        var semanticSource = ExtractSemanticStyleItem(source);

        source.ShouldContain("<gallery:GalleryShowCaseHost.SemanticPartsContentTemplate>");
        source.ShouldContain("Name=\"SegmentedSemanticPreview\"");
        source.ShouldContain("SemanticOwner=\"{Binding #SegmentedSemanticOwner}\"");
        source.ShouldContain("SemanticOwnerType=\"{x:Type atom:Segmented}\"");
        source.ShouldNotContain("Name=\"VerticalSegmentedSemanticPreview\"");
        source.ShouldContain("Name=\"VerticalSegmentedSemanticOwner\"");
        source.ShouldContain("Orientation=\"Vertical\"");
        CountOccurrences(source, "<gallery:SemanticPartDescription").ShouldBe(4);
        CountOccurrences(source, "Path=\"root\"").ShouldBe(1);
        CountOccurrences(source, "Path=\"item\"").ShouldBe(1);
        CountOccurrences(source, "Path=\"icon\"").ShouldBe(1);
        CountOccurrences(source, "Path=\"label\"").ShouldBe(1);
        source.ShouldContain("SegmentedShowCaseLangResource SemanticRootDescription");
        source.ShouldContain("SegmentedShowCaseLangResource SemanticItemDescription");
        source.ShouldContain("SegmentedShowCaseLangResource SemanticIconDescription");
        source.ShouldContain("SegmentedShowCaseLangResource SemanticLabelDescription");

        semanticSource.ShouldContain("SourceKey=\"segmented-semantic-part\"");
        semanticSource.ShouldContain("SegmentedShowCaseLangResource SemanticPartStyleTitle");
        semanticSource.ShouldContain("SegmentedShowCaseLangResource SemanticPartStyleDescription");
        semanticSource.ShouldContain("Selector=\"atom|Segmented.semantic-object\"");
        semanticSource.ShouldContain("<atom:SegmentedItemStyle x:SetterTargetType=\"atom:SegmentedItem\">");
        semanticSource.ShouldContain("<atom:SegmentedIconStyle x:SetterTargetType=\"atom:IconPresenter\">");
        semanticSource.ShouldContain("<atom:SegmentedLabelStyle x:SetterTargetType=\"ContentPresenter\">");
        semanticSource.ShouldContain("Classes=\"semantic-object\"");
        CountOccurrences(semanticSource, "Classes=\"semantic-object\"").ShouldBe(2);
        semanticSource.ShouldContain("Orientation=\"Vertical\"");

        foreach (var key in new[]
                 {
                     "SemanticRootDescription",
                     "SemanticItemDescription",
                     "SemanticIconDescription",
                     "SemanticLabelDescription",
                     "SemanticPartStyleTitle",
                     "SemanticPartStyleDescription"
                 })
        {
            english.ShouldContain($"<unit id=\"{key}\">");
        }
    }

    private static string ExtractSemanticStyleItem(string source)
    {
        const string sourceKeyMarker = "segmented-semantic-part";
        const string panelCloseMarker = "</gallery:ShowCasePanel>";

        var keyIndex = source.IndexOf(sourceKeyMarker, StringComparison.Ordinal);
        keyIndex.ShouldBeGreaterThanOrEqualTo(0);

        var itemStart = source.LastIndexOf("<gallery:ShowCaseItem", keyIndex, StringComparison.Ordinal);
        itemStart.ShouldBeGreaterThanOrEqualTo(0);

        var panelCloseStart = source.IndexOf(panelCloseMarker, keyIndex, StringComparison.Ordinal);
        panelCloseStart.ShouldBeGreaterThan(keyIndex);

        return source[itemStart..panelCloseStart];
    }

    [Fact]
    public void Segmented_ShowCase_Dynamic_Demo_Loads_More_Options_Once()
    {
        var viewModel = new SegmentedViewModel(null!);
        var command   = (ICommand)viewModel.LoadMoreOptionsCommand;

        viewModel.DynamicOptions.ShouldBe(new[] { "Daily", "Weekly", "Monthly" });
        viewModel.IsDynamicOptionsLoaded.ShouldBeFalse();

        command.Execute(null);

        viewModel.DynamicOptions.ShouldBe(new[] { "Daily", "Weekly", "Monthly", "Quarterly", "Yearly" });
        viewModel.IsDynamicOptionsLoaded.ShouldBeTrue();

        command.Execute(null);
        viewModel.DynamicOptions.ShouldBe(new[] { "Daily", "Weekly", "Monthly", "Quarterly", "Yearly" });
    }

    private static string ExtractSegmentedExampleItems(string source)
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
