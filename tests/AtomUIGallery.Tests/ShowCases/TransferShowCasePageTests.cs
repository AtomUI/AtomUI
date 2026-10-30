using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Shouldly;
using Xunit;

namespace AtomUIGallery.Tests.ShowCases;

public class TransferShowCasePageTests
{

    [Fact]
    public void Transfer_ShowCase_Declares_The_Semantic_Previews_And_Style_Example()
    {
        var source  = ReadRepoFile("controlgallery/AtomUIGallery/ShowCases/DataEntry/Transfer/Views/TransferShowCase.axaml");
        var english = ReadRepoFile("controlgallery/AtomUIGallery/ShowCases/DataEntry/Transfer/Localization/en-US.xlf");

        source.ShouldContain("<gallery:GalleryShowCaseHost.SemanticPartsContentTemplate>");
        source.ShouldContain("Name=\"ListTransferSemanticPreview\"");
        source.ShouldContain("Name=\"TreeTransferSemanticPreview\"");
        source.ShouldContain("SemanticOwner=\"{Binding #ListTransferSemanticOwner}\"");
        source.ShouldContain("SemanticOwner=\"{Binding #TreeTransferSemanticOwner}\"");
        source.ShouldContain("SemanticOwnerType=\"{x:Type atom:ListTransfer}\"");
        source.ShouldContain("SemanticOwnerType=\"{x:Type atom:TreeTransfer}\"");
        CountOccurrences(source, "<gallery:SemanticPartDescription").ShouldBe(50);
        foreach (var path in new[]
                 {
                     "root", "source.section", "target.section", "actions", "header",
                     "title", "body", "list", "footer"
                 })
        {
            CountOccurrences(source, $"Path=\"{path}\"").ShouldBe(2);
        }
        foreach (var path in new[]
                 {
                     "source.header", "target.header", "source.title", "target.title",
                     "source.body", "target.body", "source.list", "target.list",
                     "source.footer", "target.footer"
                 })
        {
            CountOccurrences(source, $"Path=\"{path}\"").ShouldBe(2);
        }
        foreach (var path in new[] { "item", "source.item", "target.item" })
        {
            CountOccurrences(source, $"Path=\"{path}\"").ShouldBe(2);
        }
        foreach (var path in new[]
                 {
                     "itemIcon", "source.itemIcon", "target.itemIcon",
                     "itemContent", "source.itemContent", "target.itemContent"
                 })
        {
            CountOccurrences(source, $"Path=\"{path}\"").ShouldBe(1);
        }

        source.ShouldContain("SourceKey=\"transfer-semantic-part\"");
        source.ShouldContain("TransferShowCaseLangResource SemanticPartStyleTitle");
        source.ShouldContain("TransferShowCaseLangResource SemanticPartStyleDescription");
        source.ShouldContain("Selector=\"atom|ListTransfer.semantic-classnames-demo\"");
        source.ShouldContain("Selector=\"atom|ListTransfer.semantic-styles-demo\"");
        source.ShouldContain("Selector=\"atom|ListTransfer.semantic-antd-preview\"");
        CountOccurrences(source, "Classes=\"semantic-classnames-demo\"").ShouldBe(1);
        CountOccurrences(source, "Classes=\"semantic-classnames-demo semantic-styles-demo\"").ShouldBe(1);
        source.ShouldContain("Status=\"Error\"");
        source.ShouldContain("Status=\"Warning\"");
        CountOccurrences(source, "<atom:ListTransferSourceSectionStyle").ShouldBe(3);
        CountOccurrences(source, "<atom:ListTransferTargetSectionStyle").ShouldBe(3);
        CountOccurrences(source, "<atom:ListTransferHeaderStyle").ShouldBe(2);
        CountOccurrences(source, "<atom:ListTransferActionsStyle").ShouldBe(1);
        CountOccurrences(source, "<Style Selector=\"^ atom|Button\">").ShouldBe(1);
        source.ShouldContain("x:SetterTargetType=\"TemplatedControl\"");
        source.ShouldContain("x:SetterTargetType=\"atom:PixelAlignedBorder\"");
        source.ShouldContain("x:SetterTargetType=\"StackPanel\"");
        source.ShouldContain("Value=\"#80FAFAFA\"");
        source.ShouldContain("Value=\"#99FFF2E8\"");
        source.ShouldContain("Value=\"#99F6FFED\"");
        source.ShouldContain("Value=\"#B7EB8F\"");
        source.ShouldContain("Value=\"#8DBCC7\"");
        CountOccurrences(source, "TargetKeys=\"{Binding SemanticDemoTargetKeys}\"").ShouldBe(2);
        source.ShouldNotContain("IsMotionEnabled=\"False\"");
        source.ShouldNotContain("SemanticPartSectionStyleTitle");
        source.ShouldNotContain("SemanticPartRegionStyleTitle");
        source.ShouldNotContain("SemanticPartItemStyleTitle");

        source.ShouldContain("Name=\"ListTransferSemanticPreview\"");
        source.ShouldContain("Name=\"TreeTransferSemanticPreview\"");
        CountOccurrences(source, "SemanticPartFooterText").ShouldBe(4);
        CountOccurrences(source, "TargetKeys=\"{Binding SemanticPreviewTargetKeys}\"").ShouldBe(1);
        CountOccurrences(source, "TargetKeys=\"{Binding SemanticTreePreviewTargetKeys}\"").ShouldBe(1);
        source.ShouldContain("Value=\"#FFF7E6\"");
        source.ShouldContain("Value=\"#E6F7FF\"");
        english.ShouldContain("<unit id=\"SemanticPartFooterText\">");
        english.ShouldContain("<source>Custom Footer</source>");

        english.ShouldContain("<unit id=\"SemanticRootDescription\">");
        english.ShouldContain("<unit id=\"SemanticPartStyleTitle\">");
        english.ShouldContain("<source>Custom Semantic Part styling</source>");
        english.ShouldNotContain("React");
        english.ShouldNotContain("DOM");
    }

    [Fact]
    public void Transfer_ShowCase_Preview_Descriptions_Are_Unique_Per_Preview()
    {
        var source = ReadRepoFile("controlgallery/AtomUIGallery/ShowCases/DataEntry/Transfer/Views/TransferShowCase.axaml");

        var previewRegions = Regex.Matches(
            source,
            @"<gallery:SemanticPartPreview\s[^>]*Name=""(?<name>[^""]+)"".*?</gallery:SemanticPartPreview>",
            RegexOptions.Singleline | RegexOptions.CultureInvariant);
        previewRegions.Count.ShouldBe(2);

        foreach (Match region in previewRegions)
        {
            var paths = Regex.Matches(region.Value, @"Path=""([^""]+)""", RegexOptions.CultureInvariant)
                             .Select(static match => match.Groups[1].Value)
                             .ToArray();
            paths.Length.ShouldBe(paths.Distinct().Count(),
                $"duplicate Semantic Part description paths in preview '{region.Groups[1].Value}'");
        }
    }

    private static string ExtractTransferExampleItems(string source)
    {
        const string firstItemMarker  = "<gallery:ShowCaseItem";
        const string panelCloseMarker = "</gallery:ShowCasePanel>";

        var firstItemStart = source.IndexOf(firstItemMarker, StringComparison.Ordinal);
        firstItemStart.ShouldBeGreaterThanOrEqualTo(0);

        var panelCloseStart = source.IndexOf(panelCloseMarker, firstItemStart, StringComparison.Ordinal);
        panelCloseStart.ShouldBeGreaterThan(firstItemStart);

        return source[firstItemStart..panelCloseStart];
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
        var repoRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../"));
        return Path.Combine(repoRoot, relativePath);
    }
}
