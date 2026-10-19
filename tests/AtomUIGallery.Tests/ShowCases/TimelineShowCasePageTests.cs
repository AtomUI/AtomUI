using Avalonia;
using Shouldly;
using Xunit;

namespace AtomUIGallery.Tests.ShowCases;

public class TimelineShowCasePageTests
{

    [Fact]
    public void Timeline_ShowCase_Declares_The_Nine_Semantic_Previews_And_Style_Example()
    {
        var source = ReadRepoFile(
            "controlgallery/AtomUIGallery/ShowCases/DataDisplay/Timeline/Views/TimelineShowCase.axaml");
        var english = ReadRepoFile(
            "controlgallery/AtomUIGallery/ShowCases/DataDisplay/Timeline/Localization/en-US.xlf");
        var semanticSource = ExtractSemanticStyleItem(source);

        source.ShouldContain("<gallery:GalleryShowCaseHost.SemanticPartsContentTemplate>");
        CountOccurrences(source, "<gallery:SemanticPartPreview\n").ShouldBe(2);

        source.ShouldContain("Name=\"TimelineSemanticPreview\"");
        source.ShouldContain("SemanticOwner=\"{Binding #TimelineSemanticOwner}\"");
        source.ShouldContain("SemanticOwnerType=\"{x:Type atom:Timeline}\"");
        source.ShouldContain("Name=\"TimelineItemsSemanticPreview\"");
        source.ShouldContain("SemanticOwner=\"{Binding #TimelineItemsSemanticOwner}\"");
        CountOccurrences(source, "P2ContentCreateAServices}\"").ShouldBe(1);
        source.ShouldContain("Label=\"2015-09-01 11:11:11\"");

        CountOccurrences(source, "<gallery:SemanticPartDescription").ShouldBe(18);
        CountOccurrences(source, "Path=\"root\"").ShouldBe(2);
        CountOccurrences(source, "Path=\"item\"").ShouldBe(2);
        CountOccurrences(source, "Path=\"itemWrapper\"").ShouldBe(2);
        CountOccurrences(source, "Path=\"itemIcon\"").ShouldBe(2);
        CountOccurrences(source, "Path=\"itemSection\"").ShouldBe(2);
        CountOccurrences(source, "Path=\"itemHeader\"").ShouldBe(2);
        CountOccurrences(source, "Path=\"itemTitle\"").ShouldBe(2);
        CountOccurrences(source, "Path=\"itemContent\"").ShouldBe(2);
        CountOccurrences(source, "Path=\"itemRail\"").ShouldBe(2);
        source.ShouldContain("TimelineShowCaseLangResource SemanticTimelineRootDescription");
        source.ShouldContain("TimelineShowCaseLangResource SemanticTimelineItemDescription");
        source.ShouldContain("TimelineShowCaseLangResource SemanticTimelineItemWrapperDescription");
        source.ShouldContain("TimelineShowCaseLangResource SemanticTimelineItemIconDescription");
        source.ShouldContain("TimelineShowCaseLangResource SemanticTimelineItemSectionDescription");
        source.ShouldContain("TimelineShowCaseLangResource SemanticTimelineItemHeaderDescription");
        source.ShouldContain("TimelineShowCaseLangResource SemanticTimelineItemTitleDescription");
        source.ShouldContain("TimelineShowCaseLangResource SemanticTimelineItemContentDescription");
        source.ShouldContain("TimelineShowCaseLangResource SemanticTimelineItemRailDescription");
        source.ShouldContain("TimelineShowCaseLangResource SemanticTimelineItemsRootDescription");
        source.ShouldContain("TimelineShowCaseLangResource SemanticTimelineItemsItemDescription");
        source.ShouldContain("TimelineShowCaseLangResource SemanticTimelineItemsWrapperDescription");
        source.ShouldContain("TimelineShowCaseLangResource SemanticTimelineItemsIconDescription");
        source.ShouldContain("TimelineShowCaseLangResource SemanticTimelineItemsSectionDescription");
        source.ShouldContain("TimelineShowCaseLangResource SemanticTimelineItemsHeaderDescription");
        source.ShouldContain("TimelineShowCaseLangResource SemanticTimelineItemsTitleDescription");
        source.ShouldContain("TimelineShowCaseLangResource SemanticTimelineItemsContentDescription");
        source.ShouldContain("TimelineShowCaseLangResource SemanticTimelineItemsRailDescription");

        semanticSource.ShouldContain("SourceKey=\"timeline-semantic-part\"");
        semanticSource.ShouldContain("TimelineShowCaseLangResource SemanticPartStyleTitle");
        semanticSource.ShouldContain("TimelineShowCaseLangResource SemanticPartStyleDescription");
        semanticSource.ShouldContain("Selector=\"atom|Timeline.semantic-object\"");
        semanticSource.ShouldContain("Selector=\"atom|Timeline.semantic-function\"");
        CountOccurrences(semanticSource, "<atom:TimelineItemIconStyle x:SetterTargetType=\"Border\">")
            .ShouldBe(2);
        CountOccurrences(semanticSource, "<Setter Property=\"BorderBrush\" Value=\"#1890ff\" />").ShouldBe(1);
        CountOccurrences(semanticSource, "<Setter Property=\"BorderBrush\" Value=\"#A294F9\" />").ShouldBe(2);
        CountOccurrences(semanticSource, "<atom:Timeline Classes=\"semantic-object\"").ShouldBe(1);
        CountOccurrences(semanticSource, "<atom:Timeline Classes=\"semantic-function\"").ShouldBe(1);
        semanticSource.ShouldContain("<atom:Timeline Classes=\"semantic-object\" Orientation=\"Horizontal\">");
        CountOccurrences(semanticSource, "<atom:TimelineItem ").ShouldBe(6);
        CountOccurrences(semanticSource, "Label=\"2015-09-01\"").ShouldBe(2);
        CountOccurrences(semanticSource, "Label=\"2015-09-01 09:12:11\"").ShouldBe(2);
        CountOccurrences(semanticSource, "P2ContentCreateAServicesSite").ShouldBe(2);
        CountOccurrences(semanticSource, "P2ContentSolveInitialNetworkProblems").ShouldBe(2);
        CountOccurrences(semanticSource, "P2ContentTechnicalTesting").ShouldBe(2);

        foreach (var key in new[]
                 {
                     "SemanticTimelineRootDescription",
                     "SemanticTimelineItemDescription",
                     "SemanticTimelineItemWrapperDescription",
                     "SemanticTimelineItemIconDescription",
                     "SemanticTimelineItemSectionDescription",
                     "SemanticTimelineItemHeaderDescription",
                     "SemanticTimelineItemTitleDescription",
                     "SemanticTimelineItemContentDescription",
                     "SemanticTimelineItemRailDescription",
                     "SemanticPartStyleTitle",
                     "SemanticPartStyleDescription",
                     "P2ContentSolveInitialNetworkProblems",
                     "P2ContentTechnicalTesting",
                     "P2ContentCreateAServices",
                     "SemanticTimelineItemsRootDescription",
                     "SemanticTimelineItemsItemDescription",
                     "SemanticTimelineItemsWrapperDescription",
                     "SemanticTimelineItemsIconDescription",
                     "SemanticTimelineItemsSectionDescription",
                     "SemanticTimelineItemsHeaderDescription",
                     "SemanticTimelineItemsTitleDescription",
                     "SemanticTimelineItemsContentDescription",
                     "SemanticTimelineItemsRailDescription"
                 })
        {
            english.ShouldContain($"<unit id=\"{key}\">");
        }

        english.ShouldContain("Custom Semantic Part styling");
        english.ShouldContain(
            "Use owner-scoped styles to customize Timeline's published Semantic Parts.");
        semanticSource.ShouldNotContain("semantic dom", Case.Insensitive);
        semanticSource.ShouldNotContain("classNames", Case.Insensitive);
        english.ShouldNotContain("semantic dom", Case.Insensitive);
        english.ShouldNotContain("classNames", Case.Insensitive);
    }

    private static string ExtractSemanticStyleItem(string source)
    {
        const string sourceKeyMarker = "timeline-semantic-part";
        const string panelCloseMarker = "</gallery:ShowCasePanel>";

        var keyIndex = source.IndexOf(sourceKeyMarker, StringComparison.Ordinal);
        keyIndex.ShouldBeGreaterThanOrEqualTo(0);

        var itemStart = source.LastIndexOf("<gallery:ShowCaseItem", keyIndex, StringComparison.Ordinal);
        itemStart.ShouldBeGreaterThanOrEqualTo(0);

        var panelCloseStart = source.IndexOf(panelCloseMarker, keyIndex, StringComparison.Ordinal);
        panelCloseStart.ShouldBeGreaterThan(keyIndex);

        return source[itemStart..panelCloseStart];
    }

    private static string ExtractTimelineExampleItems(string source)
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
