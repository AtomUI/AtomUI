using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AtomUIDrawer = AtomUI.Desktop.Controls.Drawer;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.Drawer;

public class DrawerScopeLayerTrackingTests
{
    [Fact]
    public void First_Open_Tracks_The_Scrolled_Host_Bounds_Immediately()
    {
        // 回归：首次打开局部宿主抽屉时，层注入会让宿主 ScrollContentPresenter 的
        // 偏移经历「Content 置空 → Offset 钳 0 → 恢复要等下一次布局」的瞬态。
        // 追踪快照若在瞬态里取到 0 偏移，容器会以未补偿滚动的位置落位且不会
        // 自愈（被装饰元素的 Bounds 不随滚动变化），直到下一次开合才纠正——
        // 表现为首次弹出遮罩与面板整体错位。
        AvaloniaTestApp.EnsureInitialized();

        var drawer = new AtomUIDrawer
        {
            Title = "Basic Drawer",
            Content = new TextBlock { Text = "Some contents..." },
            Footer = new Button { ButtonType = ButtonType.Link, Content = "Footer" },
            IsMotionEnabled = false,
        };
        var hostItem = new Border
        {
            Height = 300,
            Child = new StackPanel
            {
                Children =
                {
                    new TextBlock { Text = "Render in this" },
                    drawer
                }
            }
        };
        drawer.OpenOn = hostItem;

        var panel = new StackPanel { Spacing = 20 };
        for (var i = 0; i < 6; i++)
        {
            panel.Children.Add(new Border
            {
                Height = 120,
                Background = Avalonia.Media.Brushes.White,
                Child = new TextBlock { Text = $"Card {i}" }
            });
        }
        panel.Children.Add(hostItem);
        panel.Children.Add(new Border
        {
            Height = 200,
            Background = Avalonia.Media.Brushes.White,
            Child = new TextBlock { Text = "Tail" }
        });

        var window = new AvaloniaWindow
        {
            Width = 1000,
            Height = 700,
            Content = new ScrollViewer { Content = panel }
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            hostItem.BringIntoView();
            window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();
            var scrollPresenter = window.GetVisualDescendants().OfType<ScrollContentPresenter>().First();
            scrollPresenter.Offset.Y.ShouldBeGreaterThan(0, "用例前提：宿主条目必须在滚动偏移之下");

            var itemTopLeftInWindow = hostItem.TranslatePoint(new Point(0, 0), window).ShouldNotBeNull();

            drawer.IsOpen = true;
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();

            var container = window.GetVisualDescendants()
                .Single(v => v.GetType().Name == "DrawerContainer");
            container.Bounds.Width.ShouldBe(hostItem.Bounds.Width, 1,
                "首次打开后容器宽度必须等于宿主条目宽度（不允许整层回退排布）");
            container.Bounds.Height.ShouldBe(hostItem.Bounds.Height, 1,
                "首次打开后容器高度必须等于宿主条目高度（不允许整层回退排布）");
            var containerTopLeft = container.TranslatePoint(new Point(0, 0), window).ShouldNotBeNull();
            containerTopLeft.X.ShouldBe(itemTopLeftInWindow.X, 1);
            containerTopLeft.Y.ShouldBe(itemTopLeftInWindow.Y, 1,
                "首次打开后容器必须即刻贴合宿主条目位置（滚动偏移必须被补偿）");

            // 滚动后容器继续贴合宿主条目（层随内容滚动 + 偏移订阅自愈）。
            scrollPresenter.Offset = new Vector(scrollPresenter.Offset.X, scrollPresenter.Offset.Y - 100);
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();
            var scrolledItemTopLeft = hostItem.TranslatePoint(new Point(0, 0), window).ShouldNotBeNull();
            var scrolledContainerTopLeft = container.TranslatePoint(new Point(0, 0), window).ShouldNotBeNull();
            scrolledContainerTopLeft.Y.ShouldBe(scrolledItemTopLeft.Y, 1,
                "滚动后容器必须继续贴合宿主条目位置");
        }
        finally
        {
            window.Close();
        }
    }
}
