using AtomUI.Controls.Primitives;
using AtomUI.Toolkits.GalleryBase.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using ReactiveUI;
using Shouldly;
using Xunit;
using AtomUIWindow = AtomUI.Desktop.Controls.Window;
using ModalShowCasePage = AtomUIGallery.ShowCases.Modal.ModalShowCase;
using ModalViewModelPage = AtomUIGallery.ShowCases.Modal.ModalViewModel;

namespace AtomUIGallery.Tests.ShowCases;

public class ModalSemanticPartHighlightTests
{
    [Fact]
    public void Modal_Semantic_Previews_Highlight_All_Parts_In_Stages_And_Window_Host()
    {
        AvaloniaTestApp.EnsureInitialized();

        var page = new ModalShowCasePage { DataContext = new ModalViewModelPage(new ModalTestScreen()) };
        // 三个预览纵向排列，窗口高度必须容纳完整语义内容，HoverCard 的 MouseMove 坐标才能落在窗口内。
        var window = new AtomUIWindow { Width = 1280, Height = 2000, Content = page };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var host = page.GetVisualDescendants().OfType<GalleryShowCaseHost>().Single();
            host.SelectedTab = GalleryShowCaseTab.SemanticParts;
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();

            AssertPreviewHighlightsAllParts(
                window, "ModalSemanticPreview", "ModalSemanticOwner",
                "进入语义页签后，舞台内 Dialog 必须保持常开");
            AssertPreviewHighlightsAllParts(
                window, "MessageBoxSemanticPreview", "MessageBoxSemanticOwner",
                "进入语义页签后，舞台内 MessageBox 必须保持常开");
            AssertWindowHostHighlightsAllMaterializedParts(window);

            // 钉住常开：语义页签内点击遮罩不关闭任何一个预览宿主。OverlayDialogMask 是 internal 类型，
            // 经其模板 Border#Frame（遮罩可视后代，命中判定要求 source 在 mask 子树内）发起按压。
            var maskFrame = window.GetVisualDescendants().OfType<Control>()
                                  .First(m => m.Name == "PART_DialogMask")
                                  .GetVisualDescendants().OfType<Border>()
                                  .Single(b => b.Name == "Frame");
            RaisePointerPressed(maskFrame);
            Dispatcher.UIThread.RunJobs();
            FindNamedOwner(window, "ModalSemanticOwner").IsOpen
                .ShouldBeTrue("钉住的 Dialog 预览必须忽略遮罩点击");
            FindNamedOwner(window, "MessageBoxSemanticOwner").IsOpen
                .ShouldBeTrue("钉住的 MessageBox 预览不得被无关遮罩点击关闭");

            host.SelectedTab = GalleryShowCaseTab.Examples;
            Dispatcher.UIThread.RunJobs();
            GetHighlightCount(window).ShouldBe(0, "切回示例页签后必须清理全部高亮");
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    // 原生 Window 宿主（第三预览，先例：ImagePreviewer 的 native dialog）：跨根是独立 TopLevel 的 DialogWindow，
    // 部件高亮落在该窗口自己的 AdornerLayer；mask/wrapper/header/title/close 在该宿主不物化（Optional），对应卡片无目标。
    // 预览不默认打开（按钮触发），测试经 owner 直接置 IsOpen。
    private static void AssertWindowHostHighlightsAllMaterializedParts(AtomUIWindow window)
    {
        var cards = GetPartCards(window, "WindowDialogSemanticPreview");
        cards.Length.ShouldBe(9);

        var windowOwner = FindNamedOwner(window, "WindowDialogSemanticOwner");
        windowOwner.IsOpen.ShouldBeFalse("Window 宿主预览不得默认打开");
        windowOwner.IsOpen = true;
        PumpUntil(() => windowOwner.GetCrossRoots().Count == 1, "Window 宿主打开后必须上报跨根");
        var dialogWindow = windowOwner.GetCrossRoots()
                                      .ShouldHaveSingleItem("Window 宿主必须把原生窗口上报为跨根")
                                      .ShouldBeAssignableTo<Avalonia.Controls.Window>();
        Dispatcher.UIThread.RunJobs();
        dialogWindow.UpdateLayout();
        Dispatcher.UIThread.RunJobs();

        // 先验证 Optional 语义：从零高亮状态悬停不可物化的部件，不产生任何 Adorner。
        // Window 宿主不物化 mask/wrapper（无遮罩与动效容器），且 WindowDialogPresenter 无条件
        // 隐藏 Surface 标题栏（原生窗口 caption 独占）——header/title/close 的 marker 节点不存在。
        foreach (var path in new[] { "mask", "wrapper", "header", "title", "close" })
        {
            HoverCard(cards, window, path);
            GetHighlightCount(window).ShouldBe(0, $"Window 宿主不物化 '{path}'，不得出现高亮");
            CountAdorners(dialogWindow).ShouldBe(0);
        }

        // root 的目标是舞台内 owner 本身，高亮落在主窗口。样式化窗口 Dialog 位于 PreviewContent 之外
        // （语义预览按 owner 类型多实例解析，混入会被一并高亮），此处只允许一个 root 高亮。
        HoverCard(cards, window, "root");
        GetHighlightCount(window).ShouldBe(1);
        CountAdorners(dialogWindow).ShouldBe(0);

        // container/body/footer 在 Surface 模板内于 Window 宿主正常呈现，高亮落在原生窗口内。
        foreach (var path in new[] { "container", "body", "footer" })
        {
            HoverCard(cards, window, path);
            CountAdorners(dialogWindow).ShouldBe(1, $"部件 '{path}' 必须在原生窗口内高亮");
            GetHighlightCount(window).ShouldBe(0, "主窗口不得出现跨宿主高亮");
        }
    }

    private static void AssertPreviewHighlightsAllParts(
        AtomUIWindow window,
        string previewName,
        string ownerName,
        string because)
    {
        var preview = window.GetVisualDescendants().OfType<SemanticPartPreview>()
                            .Single(p => p.Name == previewName);
        var owner = FindNamedOwner(window, ownerName);
        owner.IsOpen.ShouldBeTrue(because);
        owner.GetCrossRoots().ShouldHaveSingleItem($"{ownerName} 必须把存活 presenter 上报为跨根");

        var cards = GetPartCards(window, previewName);
        cards.Length.ShouldBe(9);

        // 顺序与上游 Modal _semantic.tsx 一致；root 依赖 owner 铺满舞台（Dialog 主题默认零尺寸，
        // resolver 对零尺寸目标不建 Adorner），舞台 owner 已显式 420x320。
        foreach (var path in new[]
                 {
                     "root", "mask", "container", "wrapper",
                     "header", "title", "body", "footer", "close"
                 })
        {
            HoverCard(cards, window, path);
            GetHighlightCount(window).ShouldBe(1, $"部件 '{path}' 在 '{previewName}' 内必须恰好命中一个高亮");
        }
    }

    private static UserControl[] GetPartCards(Visual root, string previewName)
    {
        var preview = root.GetVisualDescendants().OfType<SemanticPartPreview>()
                          .Single(p => p.Name == previewName);
        var partsPane = preview.GetVisualDescendants().OfType<Border>()
                               .First(b => b.Name == "PART_PartsPane");
        return partsPane.GetVisualDescendants().OfType<UserControl>()
                        .Where(c => c.GetType().Name.Contains("SemanticPartPreviewItem"))
                        .ToArray();
    }

    private static AtomUI.Desktop.Controls.Dialog FindNamedOwner(Visual root, string ownerName)
    {
        return root.GetVisualDescendants()
                   .OfType<AtomUI.Desktop.Controls.Dialog>()
                   .Single(d => d.Name == ownerName);
    }

    private static int GetHighlightCount(AtomUIWindow window)
    {
        return CountAdorners(window);
    }

    private static int CountAdorners(Visual root)
    {
        return root.GetVisualDescendants()
                   .Count(v => v.GetType().Name == "SemanticPartAdorner");
    }

    private static void PumpUntil(Func<bool> condition, string? because = null)
    {
        var timeoutAt = DateTimeOffset.UtcNow + TimeSpan.FromSeconds(5);
        while (!condition() && DateTimeOffset.UtcNow < timeoutAt)
        {
            Dispatcher.UIThread.RunJobs();
            System.Threading.Thread.Sleep(1);
        }

        condition().ShouldBeTrue(because);
    }

    private static void HoverCard(UserControl[] cards, AtomUIWindow window, string path)
    {
        var card = cards.Single(c =>
            (string?)c.DataContext?.GetType().GetProperty("Path")!.GetValue(c.DataContext) == path);
        card.BringIntoView();
        Dispatcher.UIThread.RunJobs();
        var center = card.TransformToVisual(window)!.Value
                         .Transform(new Point(card.Bounds.Width / 2, card.Bounds.Height / 2));
        window.MouseMove(new Point(center.X, center.Y));
        Dispatcher.UIThread.RunJobs();
        Dispatcher.UIThread.RunJobs();
        Dispatcher.UIThread.RunJobs();
    }

    private static void RaisePointerPressed(InputElement source)
    {
        source.RaiseEvent(new PointerPressedEventArgs(
            source,
            new Pointer(Pointer.GetNextFreeId(), PointerType.Mouse, true),
            source,
            default,
            0,
            new PointerPointProperties(RawInputModifiers.LeftMouseButton, PointerUpdateKind.LeftButtonPressed),
            KeyModifiers.None));
    }
}

internal sealed class ModalTestScreen : IScreen
{
    public RoutingState Router { get; } = new();
}
