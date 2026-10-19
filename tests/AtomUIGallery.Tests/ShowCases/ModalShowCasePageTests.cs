using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Shouldly;
using Xunit;

namespace AtomUIGallery.Tests.ShowCases;

public class ModalShowCasePageTests
{

    [Fact]
    public void Modal_ShowCase_Semantic_Preview_Follows_The_Pinned_Overlay_Pattern()
    {
        var source = ReadRepoFile("controlgallery/AtomUIGallery/ShowCases/Feedback/Modal/Views/ModalShowCase.axaml");

        source.ShouldContain("GalleryShowCaseHost.SemanticPartsContentTemplate");

        // 语义页签并列两个预览：Dialog 与 MessageBox。
        var dialogPreview     = ExtractSemanticPreview(source, "ModalSemanticPreview");
        var messageBoxPreview = ExtractSemanticPreview(source, "MessageBoxSemanticPreview");

        AssertPinnedOverlayPreview(dialogPreview, "atom:Dialog", "ModalSemanticStage",
            "ModalSemanticStageLayer", "ModalSemanticOwner");
        AssertPinnedOverlayPreview(messageBoxPreview, "atom:MessageBox", "MessageBoxSemanticStage",
            "MessageBoxSemanticStageLayer", "MessageBoxSemanticOwner");

        // 第三个预览演示原生 Window 宿主（先例：ImagePreviewer 的 native dialog）：跨根经 GetCrossRoots
        // 上报 HostWindow，高亮落在该窗口内。modeless 是必须的——模态窗口会阻断 Gallery 输入。
        // 不默认打开：两个按钮按需触发（第二个打开改了样式的窗口 Dialog——owner 实例级 Semantic Style
        // 经逻辑父链在 Window 宿主同样命中，有控件级回归实证）。
        var windowPreview = ExtractSemanticPreview(source, "WindowDialogSemanticPreview");
        windowPreview.ShouldContain("SemanticOwnerType=\"{x:Type atom:Dialog}\"");
        windowPreview.ShouldContain("SemanticOwner=\"{Binding #WindowDialogSemanticOwner}\"");
        windowPreview.ShouldContain("DialogHostType=\"Window\"");
        windowPreview.ShouldContain("IsModal=\"False\"");
        windowPreview.ShouldContain("IsMotionEnabled=\"False\"");
        windowPreview.ShouldNotContain("IsOpen=\"True\"");
        windowPreview.ShouldContain("IsOpen=\"{Binding IsWindowDialogSemanticOpen}\"");
        // 「打开窗口 Dialog」在舞台内触发（语义预览高亮入口）；样式化窗口的触发在样式卡片。
        windowPreview.ShouldContain("Name=\"WindowDialogSemanticOpenButton\"");
        windowPreview.ShouldNotContain("WindowDialogStyledOpenButton");
        // 显式宿主尺寸：窗口宿主自然测量过小，按 demo 基线固定（样式化窗口 Dialog 在预览内容之外，单独断言）。
        windowPreview.ShouldContain("HostWidth=\"360\"");
        windowPreview.ShouldContain("HostHeight=\"220\"");
        windowPreview.ShouldContain("ModalShowCaseLangResource SemanticWindowStageHint");

        // 样式化窗口 Dialog 必须位于 PreviewContent 之外：语义预览按 owner 类型多实例解析，
        // 同一 Preview 内容内的第二个 Dialog 实例会被一并高亮（回归教训）。
        var styledWindow = ExtractStyledWindowDialog(source);
        styledWindow.ShouldContain("Classes=\"window-semantic-styles-demo\"");
        styledWindow.ShouldContain("IsOpen=\"{Binding IsWindowDialogStyledOpen}\"");
        styledWindow.ShouldContain("DialogHostType=\"Window\"");
        styledWindow.ShouldContain("IsModal=\"False\"");
        styledWindow.ShouldContain("HostWidth=\"360\"");
        styledWindow.ShouldContain("HostHeight=\"220\"");
        // footer 按钮必须显式声明：StandardButtons 默认 NoButton，漏配会让按钮级语义样式无目标
        // 可命中（2026-09-13 真机缺陷回归）。
        styledWindow.ShouldContain("StandardButtons=\"Cancel,Ok\"");
        // 只定制该宿主真实物化的部件（container/body/footer；mask/header/title/close 不存在）。
        styledWindow.ShouldContain("atom:DialogContainerStyle");
        styledWindow.ShouldContain("atom:DialogBodyStyle");
        styledWindow.ShouldContain("atom:DialogFooterStyle");
        styledWindow.ShouldNotContain("atom:DialogMaskStyle");
        styledWindow.ShouldNotContain("atom:DialogHeaderStyle");
        styledWindow.ShouldNotContain("atom:DialogTitleStyle");
        styledWindow.ShouldNotContain("atom:DialogCloseStyle");
        // 按钮与正文文字的定制入口：外层 owner Style 的一级嵌套样式（DialogButton 的 StyleKey 是 atom:Button）。
        styledWindow.ShouldContain("Selector=\"^ atom|TextBlock\"");
        styledWindow.ShouldContain("Selector=\"^ atom|Button\"");
        styledWindow.ShouldNotContain("Selector=\"^ atom|DialogButton\"");
        windowPreview.ShouldNotContain("window-semantic-styles-demo");

        foreach (var preview in new[] { dialogPreview, messageBoxPreview, windowPreview })
        {
            var paths = Regex.Matches(preview, "SemanticPartDescription Path=\"([^\"]+)\"")
                             .Select(static m => m.Groups[1].Value).ToArray();
            paths.ShouldBe(["root", "mask", "container", "wrapper", "header", "title", "body", "footer", "close"]);
        }

        // 跨根经 ISemanticPartCrossRootProvider 上报，不允许出现 code-behind 根注册。
        source.ShouldNotContain("HandleSemanticPreviewLoaded");
        source.ShouldNotContain("HandleSemanticPreviewUnloaded");

        // SemanticStyles 示例必须用生成 Style 类定制（专用 Style 唯一定制入口）。
        var semanticStylesItem = ExtractShowCaseItem(source, "SemanticStylesTitle");
        semanticStylesItem.ShouldContain("atom:DialogMaskStyle");
        semanticStylesItem.ShouldContain("atom:DialogContainerStyle");
        semanticStylesItem.ShouldContain("atom:DialogHeaderStyle");
        semanticStylesItem.ShouldContain("atom:DialogTitleStyle");
        semanticStylesItem.ShouldContain("atom:DialogBodyStyle");
        semanticStylesItem.ShouldContain("atom:DialogFooterStyle");
        semanticStylesItem.ShouldContain("atom:DialogCloseStyle");
        // title 是 Avalonia TextBlock、close 是 Avalonia Button，x:SetterTargetType 不得带 atom: 前缀。
        semanticStylesItem.ShouldContain("x:SetterTargetType=\"TextBlock\"");
        semanticStylesItem.ShouldContain("x:SetterTargetType=\"Button\"");
        // 禁止以事件处理器为特征的代码回退。
        semanticStylesItem.ShouldNotContain(".Loaded=");
        semanticStylesItem.ShouldNotContain(".Unloaded=");
    }

    [Fact]
    public void Modal_ShowCase_MessageBox_Semantic_Styling_Is_Merged_Into_The_Styling_Example()
    {
        var source = ReadRepoFile("controlgallery/AtomUIGallery/ShowCases/Feedback/Modal/Views/ModalShowCase.axaml");

        // MessageBox 语义样式示例必须合并进「自定义语义结构的样式」条目，而不是独立成卡。
        source.ShouldNotContain("MessageBoxSemanticStylesTitle");
        var item = ExtractShowCaseItem(source, "SemanticStylesTitle");

        // 同一个条目同时提供 Dialog 与 MessageBox 两个触发按钮。
        item.ShouldContain("Name=\"SemanticStyleDialogOpenButton\"");
        item.ShouldContain("Name=\"MessageBoxSemanticStyleOpenButton\"");
        // 样式化窗口 Dialog 的触发按钮在样式卡片（窗口 Dialog 的触发在语义预览舞台）。
        item.ShouldContain("Name=\"WindowDialogStyledOpenButton\"");
        item.ShouldNotContain("WindowDialogSemanticOpenButton");
        CountOccurrences(item, "<atom:Button").ShouldBe(3);
        item.ShouldNotContain("ToggleSwitch");

        // Dialog 部分用 Dialog<Part>Style。
        item.ShouldContain("atom:DialogMaskStyle");
        item.ShouldContain("atom:DialogCloseStyle");

        // MessageBox 部分必须用 MessageBox<Part>Style 与独立的 owner 选择器。
        item.ShouldContain("Classes=\"messagebox-semantic-styles-demo\"");
        item.ShouldContain("Style Selector=\"atom|MessageBox.messagebox-semantic-styles-demo\"");
        item.ShouldContain("atom:MessageBoxMaskStyle");
        item.ShouldContain("atom:MessageBoxContainerStyle");
        item.ShouldContain("atom:MessageBoxHeaderStyle");
        item.ShouldContain("atom:MessageBoxTitleStyle");
        item.ShouldContain("atom:MessageBoxBodyStyle");
        item.ShouldContain("atom:MessageBoxFooterStyle");
        item.ShouldContain("atom:MessageBoxCloseStyle");

        // 禁止以事件处理器为特征的代码回退。
        item.ShouldNotContain(".Loaded=");
        item.ShouldNotContain(".Unloaded=");
    }

    private static string ExtractModalExampleItems(string source)
    {
        const string firstItemMarker  = "<gallery:ShowCaseItem";
        const string panelCloseMarker = "</gallery:ShowCasePanel>";

        var firstItemStart = source.IndexOf(firstItemMarker, StringComparison.Ordinal);
        firstItemStart.ShouldBeGreaterThanOrEqualTo(0);

        var panelCloseStart = source.IndexOf(panelCloseMarker, firstItemStart, StringComparison.Ordinal);
        panelCloseStart.ShouldBeGreaterThan(firstItemStart);

        return source[firstItemStart..panelCloseStart];
    }

    // 对齐上游 getContainer={false} 的内联模态：舞台自带 ScopeAwareOverlayLayerPanel，
    // owner 经 OverlayScope 把 overlay 宿主限定在舞台作用域内，mask 随舞台而不是铺满窗口；
    // 常开由 IsPinnedOpen 钉住，StandardButtons 提供真实按钮序列以呈现 footer。
    private static void AssertPinnedOverlayPreview(
        string preview,
        string ownerType,
        string stageName,
        string stageLayerName,
        string ownerName)
    {
        preview.ShouldContain($"SemanticOwnerType=\"{{x:Type {ownerType}}}\"");
        preview.ShouldContain($"SemanticOwner=\"{{Binding #{ownerName}}}\"");
        preview.ShouldContain($"Name=\"{stageName}\"");
        preview.ShouldContain($"Name=\"{stageLayerName}\"");
        preview.ShouldContain($"<atom:ScopeAwareOverlayLayerPanel Name=\"{stageLayerName}\"");
        preview.ShouldContain($"OverlayScope=\"{{Binding #{stageLayerName}}}\"");
        preview.ShouldContain($"PlacementTarget=\"{{Binding #{stageLayerName}}}\"");
        preview.ShouldContain("IsOpen=\"True\"");
        preview.ShouldContain("IsPinnedOpen=\"True\"");
        preview.ShouldContain("IsMotionEnabled=\"False\"");
        preview.ShouldContain("IsModal=\"True\"");
        preview.ShouldContain("DialogHostType=\"Overlay\"");
        preview.ShouldContain("StandardButtons=\"Cancel,Ok\"");
    }

    private static string ExtractStyledWindowDialog(string source)
    {
        const string startMarker = "<atom:Dialog Classes=\"window-semantic-styles-demo\"";
        const string endMarker   = "</atom:Dialog>";

        var start = source.IndexOf(startMarker, StringComparison.Ordinal);
        start.ShouldBeGreaterThanOrEqualTo(0, "missing styled window dialog");
        var end = source.IndexOf(endMarker, start, StringComparison.Ordinal);
        end.ShouldBeGreaterThan(start);

        return source[start..(end + endMarker.Length)];
    }

    private static string ExtractSemanticPreview(string source, string previewName)
    {
        const string closingMarker = "</gallery:SemanticPartPreview>";

        var startMarker = $"<gallery:SemanticPartPreview Name=\"{previewName}\"";
        var start       = source.IndexOf(startMarker, StringComparison.Ordinal);
        start.ShouldBeGreaterThanOrEqualTo(0, $"missing SemanticPartPreview '{previewName}'");

        var end = source.IndexOf(closingMarker, start, StringComparison.Ordinal);
        end.ShouldBeGreaterThan(start);

        return source[start..(end + closingMarker.Length)];
    }

    private static string ExtractShowCaseItem(string source, string titleResourceName)
    {
        var titleMarker = $"ModalShowCaseLangResource {titleResourceName}";
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
