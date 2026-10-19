using Shouldly;
using Xunit;

namespace AtomUIGallery.Tests.ShowCases;

public class TagShowCasePageTests
{

    [Fact]
    public void Tag_ShowCase_Checkable_Example_Matches_Ant_Design_Demo_State()
    {
        var source = ReadRepoFile("controlgallery/AtomUIGallery/ShowCases/DataDisplay/Tag/Views/TagShowCase.axaml");
        var viewModelSource = ReadRepoFile(
            "controlgallery/AtomUIGallery/ShowCases/DataDisplay/Tag/ViewModels/TagViewModel.cs");

        CountOccurrences(source, "<atom:CheckableTagGroup ").ShouldBe(4);
        source.ShouldContain("<atom:CheckableTag Content=\"{gallery:TagShowCaseLangResource P2ContentYes}\"");
        source.ShouldContain("IsChecked=\"{Binding IsCheckableTagChecked, Mode=TwoWay}\"");
        source.ShouldContain("CheckedItem=\"{Binding SingleCheckedTag, Mode=TwoWay}\"");
        source.ShouldContain("CheckedItems=\"{Binding MultipleCheckedTags, Mode=TwoWay}\"");
        source.ShouldContain("IsMultiple=\"True\"");

        viewModelSource.ShouldContain("IsCheckableTagChecked = true");
        viewModelSource.ShouldContain("SingleCheckedTag = \"Books\"");
        viewModelSource.ShouldContain("new ObservableCollection<object> { \"Movies\", \"Music\" }");
        viewModelSource.ShouldContain("\"Movies\", \"Books\", \"Music\", \"Sports\"");
    }

    [Fact]
    public void Tag_ShowCase_Declares_The_Two_Semantic_Previews_And_Style_Example()
    {
        var source = ReadRepoFile(
            "controlgallery/AtomUIGallery/ShowCases/DataDisplay/Tag/Views/TagShowCase.axaml");
        var english = ReadRepoFile(
            "controlgallery/AtomUIGallery/ShowCases/DataDisplay/Tag/Localization/en-US.xlf");
        var semanticSource = ExtractSemanticStyleItem(source);

        source.ShouldContain("<gallery:GalleryShowCaseHost.SemanticPartsContentTemplate>");
        CountOccurrences(source, "<gallery:SemanticPartPreview\n").ShouldBe(2);

        source.ShouldContain("Name=\"TagSemanticPreview\"");
        source.ShouldContain("SemanticOwner=\"{Binding #TagSemanticOwner}\"");
        source.ShouldContain("SemanticOwnerType=\"{x:Type atom:Tag}\"");
        source.ShouldContain("Name=\"CheckableTagGroupSemanticPreview\"");
        source.ShouldContain("SemanticOwner=\"{Binding #CheckableTagGroupSemanticOwner}\"");
        source.ShouldContain("SemanticOwnerType=\"{x:Type atom:CheckableTagGroup}\"");

        CountOccurrences(source, "<gallery:SemanticPartDescription").ShouldBe(6);
        CountOccurrences(source, "Path=\"root\"").ShouldBe(2);
        CountOccurrences(source, "Path=\"icon\"").ShouldBe(1);
        CountOccurrences(source, "Path=\"content\"").ShouldBe(1);
        CountOccurrences(source, "Path=\"close\"").ShouldBe(1);
        CountOccurrences(source, "Path=\"item\"").ShouldBe(1);
        source.ShouldContain("TagShowCaseLangResource SemanticTagRootDescription");
        source.ShouldContain("TagShowCaseLangResource SemanticTagIconDescription");
        source.ShouldContain("TagShowCaseLangResource SemanticTagContentDescription");
        source.ShouldContain("TagShowCaseLangResource SemanticTagCloseDescription");
        source.ShouldContain("TagShowCaseLangResource SemanticGroupRootDescription");
        source.ShouldContain("TagShowCaseLangResource SemanticGroupItemDescription");

        semanticSource.ShouldContain("SourceKey=\"tag-semantic-part\"");
        semanticSource.ShouldContain("TagShowCaseLangResource SemanticPartStyleTitle");
        semanticSource.ShouldContain("TagShowCaseLangResource SemanticPartStyleDescription");
        semanticSource.ShouldContain("Selector=\"atom|Tag.semantic-object\"");
        semanticSource.ShouldContain("Selector=\"atom|CheckableTagGroup.semantic-object\"");
        semanticSource.ShouldContain("<atom:TagIconStyle x:SetterTargetType=\"atom:IconPresenter\">");
        semanticSource.ShouldContain("<atom:TagContentStyle x:SetterTargetType=\"atom:TextBlock\">");
        semanticSource.ShouldContain("<atom:TagCloseStyle x:SetterTargetType=\"atom:IconButton\">");
        semanticSource.ShouldContain("<atom:CheckableTagGroupItemStyle x:SetterTargetType=\"atom:CheckableTag\">");
        CountOccurrences(semanticSource, "Classes=\"semantic-object\"").ShouldBe(2);

        foreach (var key in new[]
                 {
                     "SemanticTagRootDescription",
                     "SemanticTagIconDescription",
                     "SemanticTagContentDescription",
                     "SemanticTagCloseDescription",
                     "SemanticGroupRootDescription",
                     "SemanticGroupItemDescription",
                     "SemanticPartStyleTitle",
                     "SemanticPartStyleDescription"
                 })
        {
            english.ShouldContain($"<unit id=\"{key}\">");
        }
    }

    private static string ExtractSemanticStyleItem(string source)
    {
        const string sourceKeyMarker = "tag-semantic-part";
        const string panelCloseMarker = "</gallery:ShowCasePanel>";

        var keyIndex = source.IndexOf(sourceKeyMarker, StringComparison.Ordinal);
        keyIndex.ShouldBeGreaterThanOrEqualTo(0);

        var itemStart = source.LastIndexOf("<gallery:ShowCaseItem", keyIndex, StringComparison.Ordinal);
        itemStart.ShouldBeGreaterThanOrEqualTo(0);

        var panelCloseStart = source.IndexOf(panelCloseMarker, keyIndex, StringComparison.Ordinal);
        panelCloseStart.ShouldBeGreaterThan(keyIndex);

        return source[itemStart..panelCloseStart];
    }

    private static string ExtractTagExampleItems(string source)
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
