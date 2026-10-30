using AtomUI.Controls.Primitives;
using AtomUI.Toolkits.GalleryBase.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AtomUITabStrip     = AtomUI.Desktop.Controls.TabStrip;
using AtomUITabStripItem = AtomUI.Desktop.Controls.TabStripItem;
using AtomUIWindow       = AtomUI.Desktop.Controls.Window;
using AvaloniaWindow     = Avalonia.Controls.Window;

namespace AtomUI.Toolkits.GalleryBase.Tests.Controls;

public class GalleryStickyTabsHostStickyMirrorTests
{
    public GalleryStickyTabsHostStickyMirrorTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void StickyElevation_Hosts_The_Live_Strip_While_Pinned()
    {
        var tabStrip = CreateTabStrip();
        var host     = CreateHost(tabStrip);

        using var context = ShowInWindow(host, 1000, 600, out var window);

        ScrollTo(window, host, 400);

        var stripHost = FindStripHost(window, host);
        var layer     = FindStickyElevationLayer(window);
        layer.ShouldNotBeNull("pinned strip must be elevated into the scope-aware adorner layer");

        layer.Children.ShouldContain(stripHost,
            "the elevated element must be the live PART_StickyContentHost, not a frozen visual copy");
        layer.Children.OfType<Border>()
             .Count(border => border.Background is VisualBrush)
             .ShouldBe(0,
                 "the sticky strip must not be mirrored through a VisualBrush: Avalonia 12 records the brush content once, so the copy stays stale after selection changes and stretches after window resize");
    }

    [Fact]
    public void StickyElevation_Selection_Is_Live_While_Pinned()
    {
        var tabStrip = CreateTabStrip();
        var host     = CreateHost(tabStrip);

        using var context = ShowInWindow(host, 1000, 600, out var window);

        ScrollTo(window, host, 400);

        ClickTab(window, tabStrip, 1);
        tabStrip.SelectedIndex.ShouldBe(1);

        var stripHost = FindStripHost(window, host);
        var layer     = FindStickyElevationLayer(window);
        layer.ShouldNotBeNull();
        stripHost.GetVisualParent().ShouldBe(layer);

        var selectedInLayer = stripHost.GetVisualDescendants()
                                        .OfType<AtomUITabStripItem>()
                                        .Single(item => item.Classes.Contains(":selected"));
        selectedInLayer.Content.ShouldBe("Semantic Parts");
    }

    [Fact]
    public void StickyElevation_Clicks_Reach_The_Real_Tabs_While_Pinned()
    {
        var tabStrip = CreateTabStrip();
        var host     = CreateHost(tabStrip);

        using var context = ShowInWindow(host, 1000, 600, out var window);

        ScrollTo(window, host, 400);

        var stripHost = FindStripHost(window, host);
        var stripRect = stripHost.TranslatePoint(default, window) is { } topLeft
            ? new Rect(topLeft, stripHost.Bounds.Size)
            : default;

        var mirrorProxy = window.GetVisualDescendants()
                                .OfType<Border>()
                                .FirstOrDefault(b => b.Background is VisualBrush);
        mirrorProxy.ShouldBeNull();

        // 吸顶条就在视口顶部
        stripRect.Y.ShouldBeLessThan(100);

        ClickTab(window, tabStrip, 1);
        tabStrip.SelectedIndex.ShouldBe(1);

        ClickTab(window, tabStrip, 0);
        tabStrip.SelectedIndex.ShouldBe(0);
    }

    [Fact]
    public void StickyElevation_Follows_Window_Resize_While_Pinned()
    {
        var tabStrip = CreateTabStrip();
        var host     = CreateHost(tabStrip);

        using var context = ShowInWindow(host, 1000, 600, out var window);

        var scrollViewer = FindPageScrollViewer(host);
        ScrollTo(window, host, 400);

        window.Width  = 1600;
        window.Height = 1000;
        Dispatcher.UIThread.RunJobs();

        var stripHost = FindStripHost(window, host);
        var layer     = FindStickyElevationLayer(window);
        layer.ShouldNotBeNull();
        stripHost.GetVisualParent().ShouldBe(layer);

        var stripRect = stripHost.TranslatePoint(default, window) is { } topLeft
            ? new Rect(topLeft, stripHost.Bounds.Size)
            : default;
        stripRect.Width.ShouldBe(scrollViewer.Viewport.Width, 1);
        stripRect.Y.ShouldBeLessThan(100);
    }

    [Fact]
    public void StickyElevation_Restores_The_Strip_When_Unpinned()
    {
        var tabStrip = CreateTabStrip();
        var host     = CreateHost(tabStrip);

        using var context = ShowInWindow(host, 1000, 600, out var window);

        ScrollTo(window, host, 400);

        var stripHost = FindStripHost(window, host);
        var layer     = FindStickyElevationLayer(window);
        layer.ShouldNotBeNull();

        ScrollTo(window, host, 0);

        var panel = host.GetVisualDescendants().OfType<GalleryStickyTabsPanel>().First();
        stripHost.GetVisualParent().ShouldBe(panel);
        double.IsNaN(stripHost.Width).ShouldBeTrue("explicit elevation size must be cleared on restore");
        double.IsNaN(stripHost.Height).ShouldBeTrue();
        double.IsNaN(Canvas.GetLeft(stripHost)).ShouldBeTrue();
        double.IsNaN(Canvas.GetTop(stripHost)).ShouldBeTrue();
        panel.Children.OfType<Control>().Count(c => c.Name == "PART_StickyContentHost").ShouldBe(1);

        // 归位后自然位置上的页签仍然可以单击切换
        ClickTab(window, tabStrip, 1);
        tabStrip.SelectedIndex.ShouldBe(1);
    }

    private static void ScrollTo(AvaloniaWindow window, GalleryStickyTabsHost host, double offset)
    {
        FindPageScrollViewer(host).Offset = new Vector(0, offset);
        Dispatcher.UIThread.RunJobs();
        // 让合成器服务端同步 readback，命中测试依赖它
        window.CaptureRenderedFrame();
        Dispatcher.UIThread.RunJobs();
    }

    private static void ClickTab(AvaloniaWindow window, AtomUITabStrip tabStrip, int index)
    {
        var item = tabStrip.Items.OfType<AtomUITabStripItem>().Skip(index).First();
        var container = tabStrip.ContainerFromItem(item) as AtomUITabStripItem ?? item;
        var localCenter = new Point(container.Bounds.Width / 2, container.Bounds.Height / 2);
        var center = container.TranslatePoint(localCenter, window)
                     ?? throw new InvalidOperationException("tab container is not attached to the window");
        Click(window, center);
        Dispatcher.UIThread.RunJobs();
    }

    private static void Click(AvaloniaWindow window, Point point)
    {
        window.MouseMove(point);
        window.MouseDown(point, MouseButton.Left);
        window.MouseUp(point, MouseButton.Left);
    }

    private static AtomUITabStrip CreateTabStrip()
    {
        return new AtomUITabStrip
        {
            Items =
            {
                new AtomUITabStripItem { Content = "Examples" },
                new AtomUITabStripItem { Content = "Semantic Parts" }
            }
        };
    }

    private static GalleryStickyTabsHost CreateHost(AtomUITabStrip tabStrip)
    {
        var content = new StackPanel();
        for (var i = 0; i < 20; i++)
        {
            content.Children.Add(new Border
            {
                Height = 60,
                Background = Brushes.Transparent
            });
        }

        return new GalleryStickyTabsHost
        {
            Header = new Border
            {
                Height = 200,
                Background = Brushes.Transparent
            },
            StickyContent = tabStrip,
            Content = content
        };
    }

    private static ScrollViewer FindPageScrollViewer(GalleryStickyTabsHost host)
    {
        return host.GetVisualDescendants()
                   .OfType<ScrollViewer>()
                   .Single(static viewer => viewer.Name == "PART_ScrollViewer");
    }

    private static Control FindStripHost(AvaloniaWindow window, GalleryStickyTabsHost host)
    {
        // 吸顶期间宿主被提升到窗口级图层，不再位于 host 的视觉子树内。
        return window.GetVisualDescendants()
                     .OfType<Control>()
                     .Single(static control => control.Name == "PART_StickyContentHost");
    }

    private static ScopeAwareAdornerLayer? FindStickyElevationLayer(AvaloniaWindow window)
    {
        return window.GetVisualDescendants()
                     .OfType<ScopeAwareAdornerLayer>()
                     .FirstOrDefault(static layer =>
                         layer.GetVisualParent() is Avalonia.Controls.Primitives.VisualLayerManager &&
                         layer.Children.Any(static child => child.Name == "PART_StickyContentHost"));
    }

    private sealed class WindowContext : IDisposable
    {
        private readonly AtomUIWindow _window;

        public WindowContext(AtomUIWindow window)
        {
            _window = window;
        }

        public void Dispose()
        {
            _window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    private static WindowContext ShowInWindow(GalleryStickyTabsHost host, double width, double height,
        out AtomUIWindow window)
    {
        window = new AtomUIWindow
        {
            Width  = width,
            Height = height,
            Content = host
        };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return new WindowContext(window);
    }
}
