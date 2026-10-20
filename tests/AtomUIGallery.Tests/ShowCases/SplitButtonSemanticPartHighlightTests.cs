using AtomUI.Toolkits.GalleryBase.Controls;
using Avalonia.Controls.Templates;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Threading;
using Avalonia.VisualTree;
using ReactiveUI;
using Shouldly;
using Xunit;
using SplitButtonShowCase = AtomUIGallery.ShowCases.SplitButton.SplitButtonShowCase;
using SplitButtonViewModel = AtomUIGallery.ShowCases.SplitButton.SplitButtonViewModel;
using AtomUIWindow = AtomUI.Desktop.Controls.Window;

namespace AtomUIGallery.Tests.ShowCases;

public class SplitButtonSemanticPartHighlightTests
{
    [Fact]
    public void SplitButton_Semantic_Preview_Highlights_Trigger_And_Popup_Parts()
    {
        AvaloniaTestApp.EnsureInitialized();

        var page = new SplitButtonShowCase
        {
            DataContext = new SplitButtonViewModel(new SplitButtonTestScreen())
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
                              .Single(static candidate => candidate.Name == "SplitButtonSemanticPreview");
            var owner = preview.GetVisualDescendants()
                               .OfType<AtomUI.Desktop.Controls.SplitButton>()
                               .Single(static candidate => candidate.Name == "SplitButtonSemanticOwner");
            owner.Flyout.ShouldNotBeNull().Popup.ShouldNotBeNull().IsOpen.ShouldBeTrue();

            var partsPane = preview.GetVisualDescendants()
                                   .OfType<Border>()
                                   .First(static border => border.Name == "PART_PartsPane");
            var cards = partsPane.GetVisualDescendants()
                                 .OfType<UserControl>()
                                 .Where(static card => card.GetType().Name.Contains("SemanticPartPreviewItem"))
                                 .ToArray();
            // root + primary + secondary + 弹层侧 5 部件。
            cards.Length.ShouldBe(8);

            // 悬停 root（触发器自身）先验证会话管线可用。
            HoverCard(cards, window, "root");
            GetHighlightCount(window).ShouldBe(1);

            // 触发侧部件：模板内 PART_PrimaryButton / PART_SecondaryButton 各一个。
            HoverCard(cards, window, "primary");
            GetHighlightCount(window).ShouldBe(1);
            HoverCard(cards, window, "secondary");
            GetHighlightCount(window).ShouldBe(1);

            // 弹层侧部件必须能解析出目标：popup.root 是 MenuFlyoutPresenter 里的 ArrowDecoratedBox。
            HoverCard(cards, window, "popup.root");
            GetHighlightCount(window).ShouldBe(1);

            // item：主菜单 1st/2nd/SubMenu/Delete + 子菜单 Option 1/Option 2，共 6 个。
            HoverCard(cards, window, "item");
            GetHighlightCount(window).ShouldBe(6);

            // itemTitle：主菜单 Group title + 子菜单 Group title 两个分组标题。
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
    public void SplitButton_Semantic_Preview_Recovers_Popup_Roots_After_Registration_Loss()
    {
        AvaloniaTestApp.EnsureInitialized();

        var page = new SplitButtonShowCase
        {
            DataContext = new SplitButtonViewModel(new SplitButtonTestScreen())
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
                              .Single(static candidate => candidate.Name == "SplitButtonSemanticPreview");

            // 真机时序下弹层由钉住属性异步打开，晚于 Loaded：一次性注册会漏（弹层部件全失效）。
            // 模拟注册丢失后，跟随 LayoutUpdated 的弹性补注册必须把弹层根重新接回高亮会话。
            preview.AdditionalRoots.Clear();
            window.Width = 1200;
            Dispatcher.UIThread.RunJobs();
            window.Width = 1280;
            Dispatcher.UIThread.RunJobs();

            preview.AdditionalRoots.Count.ShouldBeGreaterThanOrEqualTo(1,
                "the popup root must be re-registered after a layout pass once registration was lost");

            var partsPane = preview.GetVisualDescendants()
                                   .OfType<Border>()
                                   .First(static border => border.Name == "PART_PartsPane");
            var cards = partsPane.GetVisualDescendants()
                                 .OfType<UserControl>()
                                 .Where(static card => card.GetType().Name.Contains("SemanticPartPreviewItem"))
                                 .ToArray();

            HoverCard(cards, window, "item");
            GetHighlightCount(window).ShouldBe(6,
                "hovering item must highlight all menu items after the popup roots have recovered");
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Fact]
    public void SplitButton_Semantic_Preview_Restores_Declarative_SubMenu_After_Tab_Switch()
    {
        AvaloniaTestApp.EnsureInitialized();

        var page = new SplitButtonShowCase
        {
            DataContext = new SplitButtonViewModel(new SplitButtonTestScreen())
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
                            .OfType<AtomUI.Desktop.Controls.SplitButton>()
                            .Single(static candidate => candidate.Name == "SplitButtonSemanticOwner");
            var flyout = owner.Flyout.ShouldNotBeNull();
            var popup = flyout.Popup.ShouldNotBeNull();
            popup.IsOpen.ShouldBeTrue();

            var subMenuParent = window.GetVisualDescendants()
                                       .OfType<AtomUI.Desktop.Controls.MenuItem>()
                                       .First(static item => Equals(item.Header, "SubMenu"));
            subMenuParent.IsSubMenuOpen.ShouldBeTrue("the declarative submenu should be open initially");
            var subMenuPopup = subMenuParent.GetTemplateDescendants()
                                            .OfType<Avalonia.Controls.Primitives.Popup>()
                                            .FirstOrDefault();
            subMenuPopup.ShouldNotBeNull();
            subMenuPopup.IsOpen.ShouldBeTrue();

            // 初始对齐基准：子菜单内容顶部应与 SubMenu 触发行顶部对齐（级联菜单规则）。
            var initialDelta = GetTopDelta(window, subMenuParent, subMenuPopup);

            // 切走页签：放置目标不可见触发弹层生命周期关闭（主弹层 + 子菜单全部关闭）。
            host.SelectedTab = GalleryShowCaseTab.Examples;
            Dispatcher.UIThread.RunJobs();
            popup.IsOpen.ShouldBeFalse();

            // 钉住回弹不得把已随宿主关闭的子菜单弹层重新拉起：宿主关闭期间打开的子弹层
            // 会悬挂成空白的孤儿弹层（仅剩卡片阴影），宿主重开后又打开正常的子菜单，
            // 页面上出现第二个带空白阴影的子菜单。
            subMenuPopup.IsOpen.ShouldBeFalse(
                "the submenu popup must close with its host; reopening it while the host popup is closed leaves a blank orphan popup");

            // 切回页签：主弹层被 reconcile 重开，声明式子菜单状态必须一并恢复。
            // 重开由 owner 的恢复轮询（DispatcherTimer）驱动，需要推进真实时间。
            host.SelectedTab = GalleryShowCaseTab.SemanticParts;
            for (var i = 0; i < 10 && !popup.IsOpen; i++)
            {
                Dispatcher.UIThread.RunJobs();
                window.UpdateLayout();
                System.Threading.Thread.Sleep(50);
                Dispatcher.UIThread.RunJobs();
            }
            window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();

            popup.IsOpen.ShouldBeTrue("the pinned popup should reopen when the tab becomes visible again");
            subMenuParent.IsSubMenuOpen.ShouldBeTrue(
                "the declarative submenu state must survive the popup lifecycle close");
            subMenuPopup.IsOpen.ShouldBeTrue(
                "the submenu popup must reopen together with the host popup");

            // 重开后的对齐必须与初始一致：重开链路的延迟打开若跑在宿主布局收敛之前，
            // 子菜单会基于未收敛 Bounds 定位，面板与触发行脱节（阴影压到 Option 1 上方）。
            var reopenedDelta = GetTopDelta(window, subMenuParent, subMenuPopup);
            reopenedDelta.ShouldBe(initialDelta,
                $"submenu popup misplaced after reopen: initial top delta {initialDelta:F1}, reopened {reopenedDelta:F1}");
            subMenuParent.Header.ShouldBe("SubMenu");
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    private static double GetTopDelta(
        AtomUIWindow window,
        AtomUI.Desktop.Controls.MenuItem subMenuParent,
        Avalonia.Controls.Primitives.Popup subMenuPopup)
    {
        var rowTop = subMenuParent.TransformToVisual(window)?.Transform(default).Y ?? double.NaN;
        var contentTop = subMenuPopup.Child?.TransformToVisual(window)?.Transform(default).Y ?? double.NaN;
        return contentTop - rowTop;
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

internal sealed class SplitButtonTestScreen : IScreen
{
    public RoutingState Router { get; } = new();
}
