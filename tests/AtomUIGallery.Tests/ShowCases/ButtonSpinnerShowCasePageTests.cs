using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Shouldly;
using Xunit;

namespace AtomUIGallery.Tests.ShowCases;

public class ButtonSpinnerShowCasePageTests
{

    [Fact]
    public void ButtonSpinner_ShowCase_Declares_The_Semantic_Previews_And_Style_Example()
    {
        var source  = ReadRepoFile("controlgallery/AtomUIGallery/ShowCases/Navigation/ButtonSpinner/Views/ButtonSpinnerShowCase.axaml");
        var english = ReadRepoFile("controlgallery/AtomUIGallery/ShowCases/Navigation/ButtonSpinner/Localization/en-US.xlf");

        source.ShouldContain("<gallery:GalleryShowCaseHost.SemanticPartsContentTemplate>");
        source.ShouldContain("Name=\"ButtonSpinnerSemanticPreview\"");
        source.ShouldContain("SemanticOwner=\"{Binding #ButtonSpinnerSemanticOwner}\"");
        source.ShouldContain("SemanticOwnerType=\"{x:Type atom:ButtonSpinner}\"");
        source.ShouldContain("SemanticPartStyleTitle");
        source.ShouldContain("SourceKey=\"buttonspinner-semantic-part\"");

        // One preview only: ButtonSpinner has a single built-in template, and
        // IsButtonSpinnerFloatable defaults to false. A second preview that does not change
        // IsButtonSpinnerFloatable renders the identical inline handle and duplicates every
        // part row, so it must not come back.
        CountOccurrences(source, "<gallery:SemanticPartPreview ").ShouldBe(1);
        CountOccurrences(source, "SemanticOwnerType=\"{x:Type atom:ButtonSpinner}\"").ShouldBe(1);

        foreach (var part in new[]
                 {
                     "root", "content", "innerLeftContent", "innerRightContent",
                     "actions", "increaseButton", "decreaseButton"
                 })
        {
            source.ShouldContain($"Path=\"{part}\"");
        }

        // The published parts must be customized through the generated Semantic Part styles,
        // never through a code-behind fallback.
        source.ShouldNotContain("Loaded=\"");
        source.ShouldNotContain("Unloaded=\"");

        foreach (var styleType in new[]
                 {
                     "ButtonSpinnerActionsStyle", "ButtonSpinnerInnerLeftContentStyle",
                     "ButtonSpinnerIncreaseButtonStyle", "ButtonSpinnerDecreaseButtonStyle"
                 })
        {
            source.ShouldContain(styleType);
        }

        // Root border customization is expressed as a plain root Setter inside the owner-scoped
        // Style (root has no generated Style type); it must reach the shared input frame through
        // the owner BorderBrush relay.
        source.ShouldContain("<Setter Property=\"BorderBrush\" Value=\"#1677FF\" />");
        source.ShouldContain("<Setter Property=\"BorderBrush\" Value=\"#722ED1\" />");

        foreach (var key in new[]
                 {
                     "SemanticRootDescription", "SemanticContentDescription",
                     "SemanticInnerLeftContentDescription", "SemanticInnerRightContentDescription",
                     "SemanticActionsDescription", "SemanticIncreaseButtonDescription",
                     "SemanticDecreaseButtonDescription", "SemanticPartStyleTitle",
                     "SemanticPartStyleDescription"
                 })
        {
            english.ShouldContain($"id=\"{key}\"");
        }
    }

    private static string ExtractButtonSpinnerExampleItems(string source)
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
