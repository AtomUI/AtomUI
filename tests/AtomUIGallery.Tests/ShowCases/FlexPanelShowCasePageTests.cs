using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Shouldly;
using Xunit;

namespace AtomUIGallery.Tests.ShowCases;

public class FlexPanelShowCasePageTests
{

    [Fact]
    public void FlexPanel_ShowCase_Initializers_Use_Avalonia_Slider_Type_For_Bare_Slider_Elements()
    {
        var pageSource       = ReadRepoFile("controlgallery/AtomUIGallery/ShowCases/Layout/FlexPanel/Views/FlexPanelShowCase.axaml");
        var codeBehindSource = ReadRepoFile("controlgallery/AtomUIGallery/ShowCases/Layout/FlexPanel/Views/FlexPanelShowCase.axaml.cs");

        var bareSliderNames = Regex.Matches(
                pageSource,
                @"<Slider\s+x:Name=""([^""]+)""",
                RegexOptions.CultureInvariant)
            .Select(match => match.Groups[1].Value)
            .ToArray();

        bareSliderNames.ShouldNotBeEmpty();
        foreach (var sliderName in bareSliderNames)
        {
            codeBehindSource.ShouldContain($"FindRequired<AvaloniaSlider>(root, \"{sliderName}\")");
            codeBehindSource.ShouldNotContain($"FindRequired<AtomSlider>(root, \"{sliderName}\")");
        }
    }

    [Fact]
    public void FlexPanel_ShowCase_Template_Initializers_Search_Visual_And_Logical_Trees()
    {
        var codeBehindSource = ReadRepoFile("controlgallery/AtomUIGallery/ShowCases/Layout/FlexPanel/Views/FlexPanelShowCase.axaml.cs");

        codeBehindSource.ShouldContain("using Avalonia.LogicalTree;");
        codeBehindSource.ShouldContain("root.GetLogicalDescendants()");
        codeBehindSource.ShouldContain("root is T typedRoot && typedRoot.Name == name");
    }

    [Fact]
    public void FlexPanel_ShowCase_PixelAlignedBorder_Containers_Are_Not_Looked_Up_As_Border()
    {
        var pageSource       = ReadRepoFile("controlgallery/AtomUIGallery/ShowCases/Layout/FlexPanel/Views/FlexPanelShowCase.axaml");
        var codeBehindSource = ReadRepoFile("controlgallery/AtomUIGallery/ShowCases/Layout/FlexPanel/Views/FlexPanelShowCase.axaml.cs");

        foreach (var containerName in new[] { "ShrinkContainer", "PlaygroundContainer" })
        {
            pageSource.ShouldContain($"<atom:PixelAlignedBorder x:Name=\"{containerName}\"");
            codeBehindSource.ShouldContain($"FindRequired<Control>(root, \"{containerName}\")");
            codeBehindSource.ShouldNotContain($"FindRequired<Border>(root, \"{containerName}\")");
        }
    }

    private static string ExtractFlexPanelExampleItems(string source)
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
        source = Regex.Replace(
            source,
            @"\s*AttachedToVisualTree=""Initialize[A-Za-z0-9]+Example""",
            string.Empty,
            RegexOptions.CultureInvariant);

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
        var repoRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../"));
        return Path.Combine(repoRoot, relativePath);
    }
}
