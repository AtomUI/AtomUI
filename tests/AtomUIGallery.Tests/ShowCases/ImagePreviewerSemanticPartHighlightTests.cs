using AtomUI.Toolkits.GalleryBase.Controls;
using AtomUI.Theme;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Threading;
using Avalonia.VisualTree;
using ReactiveUI;
using Shouldly;
using Xunit;
using ImagePreviewerShowCase = AtomUIGallery.ShowCases.ImagePreviewer.ImagePreviewerShowCase;
using ImagePreviewerViewModel = AtomUIGallery.ShowCases.ImagePreviewer.ImagePreviewerViewModel;
using AtomUIWindow = AtomUI.Desktop.Controls.Window;

namespace AtomUIGallery.Tests.ShowCases;

public class ImagePreviewerSemanticPartHighlightTests
{
    [Fact]
    public void Cover_Part_Card_Highlight_The_Transparent_Mask_Border()
    {
        AvaloniaTestApp.EnsureInitialized();

        var page = new ImagePreviewerShowCase
        {
            DataContext = new ImagePreviewerViewModel(new ImagePreviewerTestScreen())
        };

        var window = new AtomUIWindow
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

            var preview = page.GetVisualDescendants()
                              .OfType<SemanticPartPreview>()
                              .Single(static candidate => candidate.Name == "ImagePreviewerSemanticPreview");

            var partsPane = preview.GetVisualDescendants()
                                   .OfType<Border>()
                                   .First(static border => border.Name == "PART_PartsPane");
            var cards = partsPane.GetVisualDescendants()
                                 .OfType<UserControl>()
                                 .Where(static card => card.GetType().Name.Contains("SemanticPartPreviewItem"))
                                 .ToArray();
            cards.Length.ShouldBeGreaterThanOrEqualTo(8);

            // cover 部件目标是静止态 Opacity=0 的遮罩 Border：语义预览必须仍能定位并描边。
            HoverCard(cards, window, "cover");
            GetHighlightCount(window).ShouldBeGreaterThanOrEqualTo(1);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Fact]
    public void Cover_Part_Preview_Reveals_The_Mask_Overlay_And_Clears_On_Release()
    {
        AvaloniaTestApp.EnsureInitialized();

        var page = new ImagePreviewerShowCase
        {
            DataContext = new ImagePreviewerViewModel(new ImagePreviewerTestScreen())
        };
        var window = new AtomUIWindow { Width = 1280, Height = 900, Content = page };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var host = page.GetVisualDescendants().OfType<GalleryShowCaseHost>().Single();
            host.SelectedTab = GalleryShowCaseTab.SemanticParts;
            Dispatcher.UIThread.RunJobs();

            var preview = page.GetVisualDescendants()
                              .OfType<SemanticPartPreview>()
                              .Single(static candidate => candidate.Name == "ImagePreviewerSemanticPreview");

            var partsPane = preview.GetVisualDescendants()
                                   .OfType<Border>()
                                   .First(static border => border.Name == "PART_PartsPane");
            var cards = partsPane.GetVisualDescendants()
                                 .OfType<UserControl>()
                                 .Where(static card => card.GetType().Name.Contains("SemanticPartPreviewItem"))
                                 .ToArray();

            var coverMask = preview.GetVisualDescendants()
                                   .OfType<Border>()
                                   .First(static border => border.Classes.Contains("semantic-cover"));
            coverMask.Opacity.ShouldBe(0);

            // hover「cover」行：部件目标（静止态透明的悬停遮罩）被预览显现——遮罩可见 + 预览状态。
            HoverCard(cards, window, "cover");
            SemanticPartPreviewState.GetIsPreviewTarget(coverMask).ShouldBeTrue();
            coverMask.Opacity.ShouldBe(1);

            // hover 移到其他部件行：遮罩回落到静止态。
            HoverCard(cards, window, "root");
            SemanticPartPreviewState.GetIsPreviewTarget(coverMask).ShouldBeFalse();
            coverMask.Opacity.ShouldBe(0);

            // 离开页签释放会话后再次进入，仍可显现。
            host.SelectedTab = GalleryShowCaseTab.Examples;
            Dispatcher.UIThread.RunJobs();
            host.SelectedTab = GalleryShowCaseTab.SemanticParts;
            Dispatcher.UIThread.RunJobs();
            HoverCard(cards, window, "cover");
            coverMask.Opacity.ShouldBe(1);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Fact]
    public void Popup_Parts_Highlight_Inside_The_Native_Preview_Dialog_Window()
    {
        AvaloniaTestApp.EnsureInitialized();

        var page = new ImagePreviewerShowCase
        {
            DataContext = new ImagePreviewerViewModel(new ImagePreviewerTestScreen())
        };
        var window = new AtomUIWindow { Width = 1280, Height = 900, Content = page };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var host = page.GetVisualDescendants().OfType<GalleryShowCaseHost>().Single();
            host.SelectedTab = GalleryShowCaseTab.SemanticParts;
            Dispatcher.UIThread.RunJobs();

            var preview = page.GetVisualDescendants()
                              .OfType<SemanticPartPreview>()
                              .Single(static candidate => candidate.Name == "ImagePreviewerSemanticPreview");
            var owner = preview.GetVisualDescendants()
                               .OfType<AtomUI.Desktop.Controls.ImagePreviewer>()
                               .First(static candidate => candidate.Name == "ImagePreviewerSemanticOwner");

            var partsPane = preview.GetVisualDescendants()
                                   .OfType<Border>()
                                   .First(static border => border.Name == "PART_PartsPane");
            var cards = partsPane.GetVisualDescendants()
                                 .OfType<UserControl>()
                                 .Where(static card => card.GetType().Name.Contains("SemanticPartPreviewItem"))
                                 .ToArray();

            // 打开 native 预览对话框（独立 TopLevel）：跨根宿主契约应上报对话框窗口根，
            // 会话据此发现宿主并刷新高亮。
            owner.OpenDialog();
            Dispatcher.UIThread.RunJobs();
            Dispatcher.UIThread.RunJobs();

            var provider = (ISemanticPartCrossRootProvider)owner;
            var dialogRoot = provider.GetCrossRoots().ShouldHaveSingleItem();

            HoverCard(cards, window, "popup.body");

            // 对话框窗口子树内出现描边（对话框自己的 AdornerLayer），主窗口无泄漏。
            dialogRoot.GetVisualDescendants()
                      .Count(static visual => visual.GetType().Name == "SemanticPartAdorner")
                      .ShouldBeGreaterThanOrEqualTo(1);
            GetHighlightCount(window).ShouldBe(0);

            // 关闭对话框：跨根宿主回收，高亮释放，重复 hover 不再产生目标。
            owner.IsOpen = false;
            Dispatcher.UIThread.RunJobs();
            provider.GetCrossRoots().ShouldBeEmpty();
            HoverCard(cards, window, "popup.body");
            dialogRoot.GetVisualDescendants()
                      .Count(static visual => visual.GetType().Name == "SemanticPartAdorner")
                      .ShouldBe(0);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    private static int GetHighlightCount(AtomUIWindow window)
    {
        return window.GetVisualDescendants()
                     .Count(static visual => visual.GetType().Name == "SemanticPartAdorner");
    }

    private static void HoverCard(
        UserControl[] cards,
        AtomUIWindow window,
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
    }
}

internal sealed class ImagePreviewerTestScreen : IScreen
{
    public RoutingState Router { get; } = new();
}
