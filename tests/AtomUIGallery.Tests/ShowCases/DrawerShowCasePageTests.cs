using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Shouldly;
using Xunit;

namespace AtomUIGallery.Tests.ShowCases;

public class DrawerShowCasePageTests
{

    [Fact]
    public void Drawer_ShowCase_Semantic_Preview_Follows_The_Inline_Pinned_Pattern()
    {
        var source = ReadRepoFile("controlgallery/AtomUIGallery/ShowCases/Feedback/Drawer/Views/DrawerShowCase.axaml");

        source.ShouldContain("GalleryShowCaseHost.SemanticPartsContentTemplate");
        source.ShouldContain("<gallery:SemanticPartPreview Name=\"DrawerSemanticPreview\"");
        source.ShouldContain("SemanticOwnerType=\"{x:Type atom:Drawer}\"");
        source.ShouldContain("SemanticOwner=\"{Binding #DrawerSemanticOwner}\"");
        source.ShouldContain("Name=\"DrawerSemanticStage\"");
        source.ShouldContain("Name=\"DrawerSemanticOwner\"");
        source.ShouldContain("IsOpen=\"True\"");
        source.ShouldContain("IsPinnedOpen=\"True\"");
        source.ShouldContain("IsMotionEnabled=\"False\"");
        source.ShouldContain("DialogSize=\"300\"");
        source.ShouldContain("OpenOn=\"{Binding #DrawerSemanticStage}\"");

        // PartDescriptions 顺序对齐 antd _semantic.tsx（去掉 dragger）。
        var paths = Regex.Matches(source, "SemanticPartDescription Path=\"([^\"]+)\"")
                         .Select(static m => m.Groups[1].Value).ToArray();
        paths.ShouldBe(["root", "mask", "section", "header", "title", "extra", "body", "footer", "close"]);

        // 跨根经 ISemanticPartCrossRootProvider 上报，不允许出现 DropdownButton 式的 code-behind 根注册。
        source.ShouldNotContain("HandleSemanticPreviewLoaded");
        source.ShouldNotContain("HandleSemanticPreviewUnloaded");

        // SemanticStyles 示例必须用生成 Style 类定制（专用 Style 唯一定制入口）。
        var semanticStylesItem = ExtractShowCaseItem(source, "SemanticStylesTitle");
        semanticStylesItem.ShouldContain("atom:DrawerMaskStyle");
        semanticStylesItem.ShouldContain("atom:DrawerSectionStyle");
        semanticStylesItem.ShouldContain("atom:DrawerHeaderStyle");
        semanticStylesItem.ShouldContain("atom:DrawerTitleStyle");
        semanticStylesItem.ShouldContain("atom:DrawerExtraStyle");
        semanticStylesItem.ShouldContain("atom:DrawerBodyStyle");
        semanticStylesItem.ShouldContain("atom:DrawerFooterStyle");
        semanticStylesItem.ShouldContain("atom:DrawerCloseStyle");
        // 禁止以事件处理器为特征的代码回退。
        semanticStylesItem.ShouldNotContain(".Loaded=");
        semanticStylesItem.ShouldNotContain(".Unloaded=");
    }

    private static string ExtractDrawerExampleItems(string source)
    {
        const string firstItemMarker  = "<gallery:ShowCaseItem";
        const string panelCloseMarker = "</gallery:ShowCasePanel>";

        var firstItemStart = source.IndexOf(firstItemMarker, StringComparison.Ordinal);
        firstItemStart.ShouldBeGreaterThanOrEqualTo(0);

        var panelCloseStart = source.IndexOf(panelCloseMarker, firstItemStart, StringComparison.Ordinal);
        panelCloseStart.ShouldBeGreaterThan(firstItemStart);

        return source[firstItemStart..panelCloseStart];
    }

    private static string ExtractShowCaseItem(string source, string titleResourceName)
    {
        var titleMarker = $"DrawerShowCaseLangResource {titleResourceName}";
        var titleIndex  = source.IndexOf(titleMarker, StringComparison.Ordinal);
        titleIndex.ShouldBeGreaterThanOrEqualTo(0);

        var itemStart = source.LastIndexOf("<gallery:ShowCaseItem", titleIndex, StringComparison.Ordinal);
        itemStart.ShouldBeGreaterThanOrEqualTo(0);

        var itemEnd = source.IndexOf("</gallery:ShowCaseItem>", titleIndex, StringComparison.Ordinal);
        itemEnd.ShouldBeGreaterThan(titleIndex);

        return source[itemStart..(itemEnd + "</gallery:ShowCaseItem>".Length)];
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
