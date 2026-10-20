using AtomUI.Toolkits.GalleryBase.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Threading;
using Avalonia.VisualTree;
using ReactiveUI;
using Shouldly;
using Xunit;
using DropdownButtonShowCase = AtomUIGallery.ShowCases.DropdownButton.DropdownButtonShowCase;
using DropdownButtonViewModel = AtomUIGallery.ShowCases.DropdownButton.DropdownButtonViewModel;
using AtomUIWindow = AtomUI.Desktop.Controls.Window;

namespace AtomUIGallery.Tests.ShowCases;

public class DropdownButtonSemanticPartHighlightTests
{
    [Fact]
    public void DropdownButton_Semantic_Preview_Highlights_Popup_Parts()
    {
        AvaloniaTestApp.EnsureInitialized();

        var page = new DropdownButtonShowCase
        {
            DataContext = new DropdownButtonViewModel(new DropdownButtonTestScreen())
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
                              .Single(static candidate => candidate.Name == "DropdownButtonSemanticPreview");
            var owner = preview.GetVisualDescendants()
                               .OfType<AtomUI.Desktop.Controls.DropdownButton>()
                               .Single(static candidate => candidate.Name == "DropdownButtonSemanticOwner");
            owner.DropdownFlyout.ShouldNotBeNull().Popup.ShouldNotBeNull().IsOpen.ShouldBeTrue();

            var partsPane = preview.GetVisualDescendants()
                                   .OfType<Border>()
                                   .First(static border => border.Name == "PART_PartsPane");
            var cards = partsPane.GetVisualDescendants()
                                 .OfType<UserControl>()
                                 .Where(static card => card.GetType().Name.Contains("SemanticPartPreviewItem"))
                                 .ToArray();
            cards.Length.ShouldBe(6);

            // 悬停 root（触发器自身）先验证会话管线可用。
            HoverCard(cards, window, "root");
            GetHighlightCount(window).ShouldBe(1);

            // 弹层侧部件必须能解析出目标：popup.root 是 MenuFlyoutPresenter 里的 ArrowDecoratedBox。
            HoverCard(cards, window, "popup.root");
            GetHighlightCount(window).ShouldBe(1);

            // item：主菜单 1st/2nd/SubMenu/Delete + 子菜单 Option 1/Option 2，共 6 个。
            HoverCard(cards, window, "item");
            GetHighlightCount(window).ShouldBe(6);

            // itemTitle：主菜单 Group title + 子菜单 Item 1 两个分组标题。
            HoverCard(cards, window, "itemTitle");
            GetHighlightCount(window).ShouldBe(2);

            // itemContent：6 个 MenuItem 的文本 presenter。
            HoverCard(cards, window, "itemContent");
            GetHighlightCount(window).ShouldBe(6);

            // itemIcon：Save / Edit / Delete 三个带图标的菜单项。
            HoverCard(cards, window, "itemIcon");
            GetHighlightCount(window).ShouldBe(3);

            // 离开 Semantic Parts 页签释放高亮会话。
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
    public void DropdownButton_Semantic_Preview_Trigger_Hugs_Content_Like_Upstream()
    {
        AvaloniaTestApp.EnsureInitialized();

        var page = new DropdownButtonShowCase
        {
            DataContext = new DropdownButtonViewModel(new DropdownButtonTestScreen())
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

            var owner = page.GetVisualDescendants()
                            .OfType<AtomUI.Desktop.Controls.DropdownButton>()
                            .Single(static candidate => candidate.Name == "DropdownButtonSemanticOwner");

            // antd 的 _semantic.tsx 只给弹层 root 设 width:200，触发器保持内容宽度；
            // 固定 Width 会让 "Hover me" 出现大量左右留白。断言布局宽度严格等于
            // 测量期望宽度（无拉伸、无固定宽、无额外留白）。
            owner.Width.ShouldBe(double.NaN);
            owner.Bounds.Width.ShouldBe(owner.DesiredSize.Width, 0.5);
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
        Dispatcher.UIThread.RunJobs();
    }
}

internal sealed class DropdownButtonTestScreen : IScreen
{
    public RoutingState Router { get; } = new();
}
