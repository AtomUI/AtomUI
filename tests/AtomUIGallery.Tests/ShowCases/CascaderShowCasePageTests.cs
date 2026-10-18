using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Shouldly;
using Xunit;

namespace AtomUIGallery.Tests.ShowCases;

public class CascaderShowCasePageTests
{
    [Fact]
    public void Cascader_ShowCase_Uses_Document_Layout_With_Examples()
    {
        var source = ReadRepoFile("controlgallery/AtomUIGallery/ShowCases/DataEntry/Cascader/Views/CascaderShowCase.axaml");

        source.ShouldContain("CascaderShowCaseLangResource PageSubtitle");
        source.ShouldContain("CascaderShowCaseLangResource PageDescription");
        source.ShouldNotContain("CascaderShowCaseLangResource InfoNamespaceLabel");
        source.ShouldNotContain("CascaderShowCaseLangResource InfoPackageLabel");
        source.ShouldNotContain("CascaderShowCaseLangResource InfoBaseClassLabel");
        source.ShouldContain("CascaderShowCaseLangResource ComponentCategory");
        source.ShouldContain("CascaderShowCaseLangResource ComponentStatusStable");
        source.ShouldNotContain("CascaderShowCaseLangResource ScenarioExamples");
        source.ShouldNotContain("CascaderShowCaseLangResource ScenarioApi");
        source.ShouldNotContain("CascaderShowCaseLangResource ScenarioDesignToken");
        source.ShouldNotContain("Tag=\"Examples\"");
        source.ShouldNotContain("Tag=\"Api\"");
        source.ShouldNotContain("Tag=\"DesignToken\"");
        source.ShouldContain("<gallery:GalleryShowCaseHost");
        source.ShouldContain("gallery:GalleryShowCaseHost.SemanticPartsContentTemplate");
        source.ShouldContain("StickyContentPadding=\"28,0,28,0\"");
        source.ShouldNotContain("<atom:TabStrip Name=\"ScenarioTabs\"");
        source.ShouldNotContain("<ContentControl Name=\"ScenarioContentHost\">");
        source.ShouldContain("Name=\"ExamplesContent\"");
        source.ShouldContain("IsScrollEnabled=\"False\"");
        source.ShouldContain("IsDeferredLoadingEnabled=\"True\"");
        source.ShouldContain("InitialDeferredLoadItemCount=\"4\"");
        source.ShouldContain("DeferredLoadBatchSize=\"2\"");
        source.ShouldContain("ContentMargin=\"28,10,28,28\"");
        source.ShouldNotContain("Selector=\"atom|TextBlock.info-label\"");
        source.ShouldNotContain("Selector=\"atom|TextBlock.info-value\"");
        CountOccurrences(source, "Classes=\"info-label\"").ShouldBe(0);
        CountOccurrences(source, "Classes=\"info-value\"").ShouldBe(0);
        source.ShouldNotContain("LineHeight=\"22\"");
        source.ShouldContain("Description=\"{gallery:CascaderShowCaseLangResource PageDescription}\"");
        CountShowCaseItemElements(source).ShouldBe(23);
        CountOccurrences(source, "IsDeferredContentEnabled=\"True\"").ShouldBe(23);
        CountOccurrences(source, "<gallery:ShowCaseItem.DeferredContentTemplate>").ShouldBe(23);
        // 23 个示例模板 + 1 个语义部件预览模板
        CountOccurrences(source, "DataTemplate x:DataType=\"vm:CascaderViewModel\"").ShouldBe(24);
        source.ShouldContain("CascaderShowCaseLangResource BasicTitle");
        source.ShouldContain("CascaderShowCaseLangResource MultipleTitle");
        source.ShouldContain("CascaderShowCaseLangResource PrefixAndSuffixTitle");
        source.ShouldContain("CascaderShowCaseLangResource SizeTitle");
        source.ShouldContain("CascaderShowCaseLangResource StyleClassTitle");
        source.ShouldContain("SourceKey=\"cascader-semantic-part\"");
        source.ShouldContain("atom:CascaderPopupListItemStyle");
        source.ShouldContain("PlaceholderText=\"{gallery:CascaderShowCaseLangResource P2PlaceholderSizeTypeLarge}\"");
        source.ShouldContain("PlaceholderText=\"{gallery:CascaderShowCaseLangResource P2PlaceholderSizeTypeMiddle}\"");
        source.ShouldContain("PlaceholderText=\"{gallery:CascaderShowCaseLangResource P2PlaceholderSizeTypeSmall}\"");
        source.ShouldContain("PlaceholderText=\"{gallery:CascaderShowCaseLangResource P2PlaceholderSizeTypeCustom}\"");
        source.ShouldContain("SizeType=\"Large\"");
        source.ShouldContain("SizeType=\"Middle\"");
        source.ShouldContain("SizeType=\"Small\"");
        source.ShouldContain("SizeType=\"Custom\"");
        source.ShouldContain("Height=\"36\"");
        source.ShouldContain("CascaderShowCaseLangResource BasicCascaderViewTitle");
        source.ShouldContain("CascaderShowCaseLangResource EmptyIndicatorTitle");
        CountOccurrences(source, "<atom:CascaderView.EmptyIndicator>").ShouldBe(1);
        source.ShouldContain("OptionCheckedChanged=\"HandlePlacementOptionCheckedChanged\"");
        source.ShouldContain("SearchRequested=\"HandleFilterCascaderViewSearchRequested\"");
        source.ShouldContain("SearchRequested=\"HandleFilterCascaderViewItemsSourceSearchRequested\"");
        source.ShouldNotContain("<atom:TabControl");
        source.ShouldNotContain("<atom:TabItem");
        source.ShouldNotContain(">Gallery<");
    }

    [Fact]
    public void Cascader_ShowCase_Clears_External_Search_Input_After_Result_Selection()
    {
        var pageSource = ReadRepoFile("controlgallery/AtomUIGallery/ShowCases/DataEntry/Cascader/Views/CascaderShowCase.axaml");
        var codeBehindSource = ReadRepoFile("controlgallery/AtomUIGallery/ShowCases/DataEntry/Cascader/Views/CascaderShowCase.axaml.cs");

        CountOccurrences(pageSource, "OptionSelected=\"HandleSearchCascaderViewOptionSelected\"").ShouldBe(2);
        CountOccurrences(pageSource, "IsSearchOnEnterEnabled=\"False\"").ShouldBe(2);
        codeBehindSource.ShouldContain("HandleExternalSearchKeyDown");
        codeBehindSource.ShouldContain("HandleExternalSearchKeyUp");
        codeBehindSource.ShouldContain("view.HandleKeyDown(e);");
        codeBehindSource.ShouldContain("HandleSearchCascaderViewOptionSelected");
        codeBehindSource.ShouldContain("searchEdit.Clear();");
    }

    [Fact]
    public void Cascader_ShowCase_Examples_Match_Approved_Control_Demo_Content()
    {
        var source   = ReadRepoFile("controlgallery/AtomUIGallery/ShowCases/DataEntry/Cascader/Views/CascaderShowCase.axaml");
        var approved = ReadRepoFile("tests/AtomUIGallery.Tests/ShowCases/CascaderShowCaseExamples.snapshot");

        var normalized = NormalizeMarkup(ExtractCascaderExampleItems(source));
        CountShowCaseItemElements(normalized).ShouldBe(ReadSnapshotCount(approved));
        ComputeSha256(normalized).ShouldBe(ReadSnapshotHash(approved));
    }

    private static string ExtractCascaderExampleItems(string source)
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

    private static string ComputeSha256(string source)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(source));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private static string ReadSnapshotHash(string source)
    {
        return source
            .Split('\n')
            .First(line => line.StartsWith("sha256:", StringComparison.Ordinal))
            .Split(':', 2)[1]
            .Trim();
    }

    private static int ReadSnapshotCount(string source)
    {
        return int.Parse(source
            .Split('\n')
            .First(line => line.StartsWith("count:", StringComparison.Ordinal))
            .Split(':', 2)[1]
            .Trim());
    }

    private static int CountShowCaseItemElements(string source)
    {
        return Regex.Matches(source, @"<gallery:ShowCaseItem(\s|>)", RegexOptions.CultureInvariant).Count;
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
