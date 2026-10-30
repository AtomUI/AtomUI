using System.Security.Cryptography;
using System.Text;
using Shouldly;
using Xunit;

namespace AtomUIGallery.Tests.ShowCases;

public class NumberUpDownShowCasePageTests
{

    [Fact]
    public void NumberUpDown_ShowCase_Declares_The_Semantic_Previews_And_Style_Example()
    {
        var source = ReadRepoFile("controlgallery/AtomUIGallery/ShowCases/DataEntry/NumberUpDown/Views/NumberUpDownShowCase.axaml");
        var english = ReadRepoFile("controlgallery/AtomUIGallery/ShowCases/DataEntry/NumberUpDown/Localization/en-US.xlf");

        source.ShouldContain("<gallery:GalleryShowCaseHost.SemanticPartsContentTemplate>");
        source.ShouldContain("Name=\"NumberUpDownSemanticPreview\"");
        source.ShouldContain("Name=\"SpinnerModeSemanticPreview\"");
        source.ShouldContain("SemanticOwner=\"{Binding #NumberUpDownSemanticOwner}\"");
        source.ShouldContain("SemanticOwner=\"{Binding #SpinnerModeSemanticOwner}\"");
        source.ShouldContain("SemanticOwnerType=\"{x:Type atom:NumericUpDown}\"");
        CountOccurrences(source, "<gallery:SemanticPartDescription").ShouldBe(10);
        foreach (var path in new[] { "root", "prefix", "input", "suffix", "clear" })
        {
            CountOccurrences(source, $"Path=\"{path}\"").ShouldBe(2);
        }

        source.ShouldContain("SourceKey=\"numericupdown-semantic-part\"");
        source.ShouldContain("NumberUpDownShowCaseLangResource SemanticPartStyleTitle");
        source.ShouldContain("NumberUpDownShowCaseLangResource SemanticPartStyleDescription");
        source.ShouldContain("Selector=\"atom|NumericUpDown.semantic-object\"");
        source.ShouldContain("Selector=\"atom|NumericUpDown.semantic-function\"");
        CountOccurrences(source, "Property=\"BorderBrush\"").ShouldBe(2);
        source.ShouldContain("Value=\"#1677FF\"");
        source.ShouldContain("Value=\"#722ED1\"");
        source.ShouldContain("Classes=\"semantic-object\"");
        source.ShouldContain("Classes=\"semantic-function\"");
        source.ShouldContain("PlaceholderText=\"Object\"");
        source.ShouldContain("PlaceholderText=\"Function\"");

        english.ShouldContain("<unit id=\"SemanticRootDescription\">");
        english.ShouldContain("<unit id=\"SemanticPartStyleTitle\">");
    }

    private static string ExtractNumberUpDownExampleItems(string source)
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
