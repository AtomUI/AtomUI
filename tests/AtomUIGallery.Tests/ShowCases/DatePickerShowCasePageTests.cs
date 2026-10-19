using Shouldly;
using Xunit;

namespace AtomUIGallery.Tests.ShowCases;

public class DatePickerShowCasePageTests
{

    [Fact]
    public void DatePicker_ShowCase_Exposes_Semantic_Parts_Tab_With_Version_Gated_Styling_Demo()
    {
        var source = ReadRepoFile("controlgallery/AtomUIGallery/ShowCases/DataEntry/DatePicker/Views/DatePickerShowCase.axaml");

        source.ShouldContain("SemanticOwnerType=\"{x:Type atom:RangeDatePicker}\"");
        source.ShouldContain("SemanticOwner=\"{Binding #DatePickerSemanticOwner}\"");
        source.ShouldContain("IsPopupPinnedOpen=\"True\"");
        source.ShouldContain("RangeStartSelectedDate=\"{Binding SemanticPreviewRangeStart}\"");
        source.ShouldContain("RangeEndSelectedDate=\"{Binding SemanticPreviewRangeEnd}\"");
        foreach (var partPath in new[]
                 {
                     "root", "prefix", "input", "secondaryInput", "suffix", "clear",
                     "popup.root", "popup.container", "popup.header", "popup.body",
                     "popup.content", "popup.cell", "popup.footer"
                 })
        {
            source.ShouldContain($"Path=\"{partPath}\"");
        }

        source.ShouldContain("SourceKey=\"datepicker-semantic-part\"");
        source.ShouldContain("atom:DatePickerPrefixStyle");
        source.ShouldContain("atom:DatePickerSuffixStyle");
        source.ShouldContain("atom:DatePickerPopupRootStyle");
        source.ShouldContain("atom:RangeDatePickerInputStyle");
        source.ShouldContain("atom:RangeDatePickerSecondaryInputStyle");
        source.ShouldContain("atom:RangeDatePickerPopupCellStyle");
        source.ShouldContain("atom:RangeDatePickerPopupFooterStyle");

        var lowered = source.ToLowerInvariant();
        lowered.ShouldNotContain("semantic dom");
        lowered.ShouldNotContain("classnames");
        lowered.ShouldNotContain("styles?\"");
    }

    private static string ExtractDatePickerExampleItems(string source)
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

    private static void AssertResourceOrder(string source, params string[] resources)
    {
        var previousIndex = -1;
        foreach (var resource in resources)
        {
            var index = source.IndexOf(resource, StringComparison.Ordinal);
            index.ShouldBeGreaterThan(previousIndex, $"{resource} should appear after the previous resource.");
            previousIndex = index;
        }
    }

    private static string ExtractShowCaseItemByTitle(string source, string titleResource)
    {
        var titleIndex = source.IndexOf(titleResource, StringComparison.Ordinal);
        titleIndex.ShouldBeGreaterThanOrEqualTo(0);

        const string itemStartMarker = "<gallery:ShowCaseItem";
        const string itemEndMarker   = "</gallery:ShowCaseItem>";

        var itemStart = source.LastIndexOf(itemStartMarker, titleIndex, StringComparison.Ordinal);
        itemStart.ShouldBeGreaterThanOrEqualTo(0);

        var itemEnd = source.IndexOf(itemEndMarker, titleIndex, StringComparison.Ordinal);
        itemEnd.ShouldBeGreaterThan(titleIndex);

        return source[itemStart..(itemEnd + itemEndMarker.Length)];
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
