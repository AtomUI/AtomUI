using Shouldly;
using Xunit;

namespace AtomUIGallery.Tests.ShowCases;

public class TooltipShowCasePageTests
{

    [Fact]
    public void Tooltip_ShowCase_Exposes_Semantic_Parts_Tab()
    {
        var source = ReadRepoFile("controlgallery/AtomUIGallery/ShowCases/DataDisplay/Tooltip/Views/TooltipShowCase.axaml");

        source.ShouldContain("SemanticOwnerType=\"{x:Type atom:ToolTip}\"");
        source.ShouldContain("SemanticOwner=\"{Binding #TooltipSemanticOwner}\"");
        foreach (var partPath in new[]
                 {
                     "root", "container", "arrow"
                 })
        {
            source.ShouldContain($"Path=\"{partPath}\"");
        }

        source.ShouldContain("SemanticRootDescription");
        source.ShouldContain("SemanticContainerDescription");
        source.ShouldContain("SemanticArrowDescription");
        source.ShouldContain("SemanticPartStyleTitle");
        source.ShouldContain("SemanticPartStyleDescription");

        var lowered = source.ToLowerInvariant();
        lowered.ShouldNotContain("semantic dom");
        lowered.ShouldNotContain("classnames");
    }

    [Fact]
    public void Tooltip_ShowCase_Semantic_Preview_Content_Hugs_Its_Content_Width()
    {
        var source = ReadRepoFile("controlgallery/AtomUIGallery/ShowCases/DataDisplay/Tooltip/Views/TooltipShowCase.axaml");

        // 预览舞台以 Stretch 测量内容，ToolTip 若不声明非 Stretch 的对齐会被
        // 撑到 ToolTipMaxWidth(250) 满宽，胶囊宽度远超 "prompt text" 所需。
        const string ownerStart = "<atom:ToolTip Name=\"TooltipSemanticOwner\"";
        var ownerStartIndex = source.IndexOf(ownerStart, StringComparison.Ordinal);
        ownerStartIndex.ShouldBeGreaterThanOrEqualTo(0);

        var ownerEndIndex = source.IndexOf("/>", ownerStartIndex, StringComparison.Ordinal);
        ownerEndIndex.ShouldBeGreaterThan(ownerStartIndex);

        var ownerElement = source[ownerStartIndex..ownerEndIndex];
        ownerElement.ShouldContain("HorizontalAlignment=\"Center\"");
    }

    private static string ExtractTooltipExampleItems(string source)
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
