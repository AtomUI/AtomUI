using AtomUI.Desktop.Controls;
using AtomUI.Toolkits.GalleryBase.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using ReactiveUI;
using Shouldly;
using Xunit;
using AvaloniaWindow = AtomUI.Desktop.Controls.Window;
using TourShowCase = AtomUIGallery.ShowCases.Tour.TourShowCase;
using TourViewModel = AtomUIGallery.ShowCases.Tour.TourViewModel;

namespace AtomUIGallery.Tests.ShowCases;

public class TourSemanticPartHighlightTests
{
    [Fact]
    public void Tour_Semantic_Preview_Highlights_Parts_With_Pinned_Popup_And_Mask()
    {
        AvaloniaTestApp.EnsureInitialized();

        var page = new TourShowCase
        {
            DataContext = new TourViewModel(new TourTestScreen())
        };

        var window = new AvaloniaWindow
        {
            Width = 1280,
            Height = 900,
            Content = page
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var host = page.GetVisualDescendants().OfType<GalleryShowCaseHost>().Single();
            host.SelectedTab = GalleryShowCaseTab.SemanticParts;
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();

            var preview = page.GetVisualDescendants()
                              .OfType<SemanticPartPreview>()
                              .Single(static candidate => candidate.Name == "TourSemanticPreview");
            var owner = preview.GetVisualDescendants()
                               .OfType<Tour>()
                               .Single(static candidate => candidate.Name == "TourSemanticOwner");

            // 语义预览对齐 antd Tour：默认打开且钉住弹层。
            owner.IsOpen.ShouldBeTrue();

            var partsPane = preview.GetVisualDescendants()
                                   .OfType<Border>()
                                   .First(static border => border.Name == "PART_PartsPane");
            var cards = partsPane.GetVisualDescendants()
                                 .OfType<UserControl>()
                                 .Where(static card => card.GetType().Name.Contains("SemanticPartPreviewItem"))
                                 .ToArray();
            cards.Length.ShouldBe(13);

            // 弹层根：卡片内容承载在 Popup 视觉根中，仍被高亮定位。
            HoverCard(cards, window, "popup.root");
            GetHighlightCount(window).ShouldBe(1);

            // 遮罩：跨根宿主（TourLayer）经 ISemanticPartCrossRootProvider 被会话收集。
            HoverCard(cards, window, "popup.mask");
            GetHighlightCount(window).ShouldBe(1);

            // 卡片内容区：共享 ArrowDecoratedBox 模板的 PART_ContentDecorator。
            HoverCard(cards, window, "popup.section");
            GetHighlightCount(window).ShouldBe(1);

            // 封面：首步设置了封面图。
            HoverCard(cards, window, "popup.cover");
            GetHighlightCount(window).ShouldBeGreaterThanOrEqualTo(1);

            // 关闭按钮：当前步骤可见实例。
            HoverCard(cards, window, "popup.close");
            GetHighlightCount(window).ShouldBeGreaterThanOrEqualTo(1);

            // 头部/标题/描述：当前步骤模板部件。
            HoverCard(cards, window, "popup.header");
            GetHighlightCount(window).ShouldBe(1);
            HoverCard(cards, window, "popup.title");
            GetHighlightCount(window).ShouldBeGreaterThanOrEqualTo(1);
            HoverCard(cards, window, "popup.description");
            GetHighlightCount(window).ShouldBeGreaterThanOrEqualTo(1);

            // 底部/操作组/指示器组：TourStepsView 模板部件。
            HoverCard(cards, window, "popup.footer");
            GetHighlightCount(window).ShouldBe(1);
            HoverCard(cards, window, "popup.actions");
            GetHighlightCount(window).ShouldBe(1);
            HoverCard(cards, window, "popup.indicators");
            GetHighlightCount(window).ShouldBe(1);

            // 圆点：两个步骤 → 两个圆点同时高亮。
            HoverCard(cards, window, "popup.indicator");
            GetHighlightCount(window).ShouldBe(2);

            // 遮罩范围对齐上游 SemanticPreview 舞台（getPopupContainer=false + overflow:hidden）：
            // 共享 TourLayer 宿主在舞台内嵌的 VisualLayerManager 中，只覆盖 600px 舞台区域，
            // 不再覆盖整个窗口。
            var stage = preview.GetVisualDescendants()
                               .OfType<Border>()
                               .Single(static border => border.Name == "TourSemanticStage");
            var stageVlm = stage.GetVisualDescendants()
                                .OfType<Avalonia.Controls.Primitives.VisualLayerManager>()
                                .Single();
            var stageTourLayer = stageVlm.GetVisualDescendants()
                                         .FirstOrDefault(static v => v.GetType().Name == "TourLayer");
            stageTourLayer.ShouldNotBeNull("TourLayer must be hosted by the stage VisualLayerManager");
            stageTourLayer.Bounds.Width.ShouldBe(stage.Bounds.Width, tolerance: 2);
            stageTourLayer.Bounds.Height.ShouldBe(stage.Bounds.Height, tolerance: 2);

            // 挖孔也必须相对舞台层换算：锚点按钮的挖孔（含 Gap 膨胀）落在舞台范围内。
            var anchor = stage.GetVisualDescendants()
                              .OfType<Avalonia.Controls.Button>()
                              .Single(static b => b.Name == "TourSemanticAnchorButton");
            var targetRegion = (Rect)stageTourLayer.GetType()
                                                   .GetProperty("TargetRegion")!
                                                   .GetValue(stageTourLayer)!;
            targetRegion.Width.ShouldBe(anchor.Bounds.Width + 12, tolerance: 2); // GapOffset 默认 6
            targetRegion.Height.ShouldBe(anchor.Bounds.Height + 12, tolerance: 2);
            targetRegion.Left.ShouldBeGreaterThanOrEqualTo(0);
            targetRegion.Top.ShouldBeGreaterThanOrEqualTo(0);
            targetRegion.Right.ShouldBeLessThanOrEqualTo(stage.Bounds.Width + 1);
            targetRegion.Bottom.ShouldBeLessThanOrEqualTo(stage.Bounds.Height + 1);

            // 离开语义部件页签释放高亮会话。
            host.SelectedTab = GalleryShowCaseTab.Examples;
            Dispatcher.UIThread.RunJobs();
            GetHighlightCount(window).ShouldBe(0);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Fact]
    public void Tour_Semantic_Preview_Markers_Hug_The_Popup_Card_And_Stage_Edges()
    {
        AvaloniaTestApp.EnsureInitialized();

        var page = new TourShowCase
        {
            DataContext = new TourViewModel(new TourTestScreen())
        };

        var window = new AvaloniaWindow
        {
            Width = 1280,
            Height = 900,
            Content = page
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var host = page.GetVisualDescendants().OfType<GalleryShowCaseHost>().Single();
            host.SelectedTab = GalleryShowCaseTab.SemanticParts;
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();

            var preview = page.GetVisualDescendants()
                              .OfType<SemanticPartPreview>()
                              .Single(static candidate => candidate.Name == "TourSemanticPreview");
            var owner = preview.GetVisualDescendants()
                               .OfType<Tour>()
                               .Single(static candidate => candidate.Name == "TourSemanticOwner");
            var stage = preview.GetVisualDescendants()
                               .OfType<Border>()
                               .Single(static border => border.Name == "TourSemanticStage");

            var partsPane = preview.GetVisualDescendants()
                                   .OfType<Border>()
                                   .First(static border => border.Name == "PART_PartsPane");
            var cards = partsPane.GetVisualDescendants()
                                 .OfType<UserControl>()
                                 .Where(static card => card.GetType().Name.Contains("SemanticPartPreviewItem"))
                                 .ToArray();

            // root 部件：owner 布局尺寸为零（弹层承载型控件），回退到 popup.root 标记节点，
            // 与 popup.root 部件指向同一张弹层卡片（对齐上游语义：Tour 的 root 即弹层容器）。
            // 几何与目标须在悬停当下捕获——悬停下一张卡片会释放会话并清除 adorner 的
            // AdornedElement，事后持有的引用会退化为 detached 状态。
            var rootMarker = HoverCardAndCaptureMarker(cards, window, "root");
            var popupRootMarker = HoverCardAndCaptureMarker(cards, window, "popup.root");
            rootMarker.Target.ShouldBe(popupRootMarker.Target);
            rootMarker.WindowRect.ShouldBe(popupRootMarker.WindowRect);

            // 弹层卡片标记贴边：adorner 在目标四周各外扩 2px（主标记 layout outset，主笔宽 2px 的一半），
            // 不向内收缩——对齐上游 Marker（描边沿目标边缘、无内收留白），且不绘制白色外环。
            var cardRect = new Rect(
                popupRootMarker.Target.TranslatePoint(new Point(0, 0), window)!.Value,
                popupRootMarker.Target.Bounds.Size);
            popupRootMarker.WindowRect.Left.ShouldBe(cardRect.Left - 2, tolerance: 1);
            popupRootMarker.WindowRect.Top.ShouldBe(cardRect.Top - 2, tolerance: 1);
            popupRootMarker.WindowRect.Right.ShouldBe(cardRect.Right + 2, tolerance: 1);
            popupRootMarker.WindowRect.Bottom.ShouldBe(cardRect.Bottom + 2, tolerance: 1);

            // 遮罩标记贴舞台边缘：同样只外扩 2px，不内收。
            var maskMarker = HoverCardAndCaptureMarker(cards, window, "popup.mask");
            var stageRect = new Rect(
                stage.TranslatePoint(new Point(0, 0), window)!.Value,
                stage.Bounds.Size);
            maskMarker.WindowRect.Left.ShouldBe(stageRect.Left - 2, tolerance: 1);
            maskMarker.WindowRect.Top.ShouldBe(stageRect.Top - 2, tolerance: 1);
            maskMarker.WindowRect.Right.ShouldBe(stageRect.Right + 2, tolerance: 1);
            maskMarker.WindowRect.Bottom.ShouldBe(stageRect.Bottom + 2, tolerance: 1);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    private sealed record MarkerGeometry(Visual Target, Rect WindowRect);

    private static MarkerGeometry HoverCardAndCaptureMarker(
        UserControl[] cards,
        AvaloniaWindow window,
        string path)
    {
        HoverCard(cards, window, path);
        var adorners = window.GetVisualDescendants()
                             .Where(static visual => visual.GetType().Name == "SemanticPartAdorner")
                             .ToArray();
        adorners.Length.ShouldBe(1, $"part '{path}' should highlight exactly one target");
        var adorner = adorners[0];
        var target = AdornerLayer.GetAdornedElement(adorner);
        target.ShouldNotBeNull($"part '{path}' adorner must reference its target");
        return new MarkerGeometry(target!, GetAdornerWindowRect(adorner, window));
    }

    /// <summary>
    /// adorner 的窗口几何：AdornerLayer 按目标尺寸排列 child，负 Margin 外扩后的
    /// Bounds 位置加上层的 RenderTransform 平移，再经层换算到窗口坐标。
    /// </summary>
    private static Rect GetAdornerWindowRect(Visual adorner, AvaloniaWindow window)
    {
        var layer = adorner.GetVisualAncestors()
                           .OfType<AdornerLayer>()
                           .Single();
        var transform = (adorner.RenderTransform as MatrixTransform)?.Matrix
            ?? Matrix.Identity;
        var originInLayer = new Point(
            adorner.Bounds.Position.X + transform.M31,
            adorner.Bounds.Position.Y + transform.M32);
        var originInWindow = layer.TranslatePoint(originInLayer, window)!.Value;
        return new Rect(originInWindow, adorner.Bounds.Size);
    }

    private static int GetHighlightCount(AvaloniaWindow window)
    {
        return window.GetVisualDescendants()
                     .Count(static visual => visual.GetType().Name == "SemanticPartAdorner");
    }

    private static void HoverCard(
        UserControl[] cards,
        AvaloniaWindow window,
        string path)
    {
        var card = cards.Single(candidate =>
            (string?)candidate.DataContext?.GetType().GetProperty("Path")!.GetValue(candidate.DataContext) == path);
        card.BringIntoView();
        Dispatcher.UIThread.RunJobs();
        var center = card.TransformToVisual(window)!.Value
                         .Transform(new Point(card.Bounds.Width / 2, card.Bounds.Height / 2));
        window.MouseMove(new Point(center.X, center.Y));
        Dispatcher.UIThread.RunJobs();
        Dispatcher.UIThread.RunJobs();
        Dispatcher.UIThread.RunJobs();
    }
}

internal sealed class TourTestScreen : IScreen
{
    public RoutingState Router { get; } = new();
}
