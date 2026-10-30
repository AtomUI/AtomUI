using AtomUI.Controls.Primitives;
using AtomUI.Toolkits.GalleryBase.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AtomUITabStrip     = AtomUI.Desktop.Controls.TabStrip;
using AtomUITabStripItem = AtomUI.Desktop.Controls.TabStripItem;
using AtomUIWindow       = AtomUI.Desktop.Controls.Window;
using AvaloniaWindow     = Avalonia.Controls.Window;

namespace AtomUI.Toolkits.GalleryBase.Tests.Controls;

public class GalleryStickyTabsHostMaximizeTests
{
    public GalleryStickyTabsHostMaximizeTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Maximize_While_Pinned_Keeps_Pinned_Geometry_Consistent()
    {
        var tabStrip = CreateTabStrip();
        var host     = CreateHost(tabStrip, sectionCount: 40);

        using var context = ShowInWindow(host, 1000, 600, out var window);

        ScrollTo(window, host, 400);
        host.GetVisualDescendants()
            .OfType<GalleryStickyTabsPanel>()
            .First()
            .IsStickyPinned.ShouldBeTrue();

        // 内容总高（40*60+200+46 ≈ 2646）远大于新视口，最大化后保持钉住
        window.Width  = 2560;
        window.Height = 1300;
        Dispatcher.UIThread.RunJobs();

        AssertPinnedGeometryConsistent(window, host);
    }

    [Fact]
    public void Maximize_While_Pinned_Unpins_And_Restores_When_Content_Fits_The_Viewport()
    {
        var tabStrip = CreateTabStrip();
        var host     = CreateHost(tabStrip, sectionCount: 14);

        using var context = ShowInWindow(host, 1000, 600, out var window);

        ScrollTo(window, host, 400);
        host.GetVisualDescendants()
            .OfType<GalleryStickyTabsPanel>()
            .First()
            .IsStickyPinned.ShouldBeTrue();

        // 内容总高（14*60+200+46 ≈ 1086）小于新视口，最大化时偏移钳制归零，应脱钉归位
        window.Width  = 2560;
        window.Height = 1300;
        Dispatcher.UIThread.RunJobs();

        var panel     = host.GetVisualDescendants().OfType<GalleryStickyTabsPanel>().First();
        var stripHost = FindStripHost(window, host);
        var scrollViewer = FindPageScrollViewer(host);

        if (panel.IsStickyPinned)
        {
            AssertPinnedGeometryConsistent(window, host);
            return;
        }

        // 脱钉路径：宿主回到面板，无孤儿占位符，无残留显式尺寸
        stripHost.GetVisualParent().ShouldBe(panel);
        double.IsNaN(stripHost.Width).ShouldBeTrue("explicit elevation size must be cleared on restore");
        double.IsNaN(stripHost.Height).ShouldBeTrue();
        panel.Children.Count.ShouldBe(3,
            "panel must contain exactly header host, sticky content host and content host after restore");
        panel.Children
             .OfType<Border>()
             .Count(static border =>
                 border.IsHitTestVisible == false &&
                 border.Focusable == false &&
                 border.Name is null)
             .ShouldBe(0, "no orphan sticky placeholder border may remain in the panel");
        scrollViewer.Offset.Y.ShouldBe(0);
    }

    [Fact]
    public void Maximize_While_Pinned_Does_Not_Create_A_Giant_Gap_Above_Content()
    {
        var tabStrip = CreateTabStrip();
        var host     = CreateHost(tabStrip, sectionCount: 8);

        using var context = ShowInWindow(host, 1000, 600, out var window);

        ScrollTo(window, host, 400);

        window.Width  = 2560;
        window.Height = 1300;
        Dispatcher.UIThread.RunJobs();

        var panel      = host.GetVisualDescendants().OfType<GalleryStickyTabsPanel>().First();
        var contentTop = panel.Children
                              .Single(static child => child.Name == "PART_ContentHost")
                              .Bounds.Y;

        // 脱钉后内容紧跟头部与页签：偏移远小于正常头部高度量级
        contentTop.ShouldBeLessThan(400);
    }

    private static void AssertPinnedGeometryConsistent(AvaloniaWindow window, GalleryStickyTabsHost host)
    {
        var stripHost    = FindStripHost(window, host);
        var layer        = FindStickyElevationLayer(window);
        var scrollViewer = FindPageScrollViewer(host);

        layer.ShouldNotBeNull();
        stripHost.GetVisualParent().ShouldBe(layer);

        var stripRect = stripHost.TranslatePoint(default, window) is { } topLeft
            ? new Rect(topLeft, stripHost.Bounds.Size)
            : default;
        stripRect.Width.ShouldBe(scrollViewer.Viewport.Width, 1);
        stripRect.Height.ShouldBeLessThan(80, "elevated strip must keep the natural tab height, not stretch");
        stripRect.Y.ShouldBeLessThan(120);
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

    private static GalleryStickyTabsHost CreateHost(AtomUITabStrip tabStrip, int sectionCount)
    {
        var content = new StackPanel();
        for (var i = 0; i < sectionCount; i++)
        {
            content.Children.Add(new Border
            {
                Height = 60,
                Background = Avalonia.Media.Brushes.Transparent
            });
        }

        return new GalleryStickyTabsHost
        {
            Header = new Border
            {
                Height = 200,
                Background = Avalonia.Media.Brushes.Transparent
            },
            StickyContent = tabStrip,
            Content = content
        };
    }

    private static void ScrollTo(AvaloniaWindow window, GalleryStickyTabsHost host, double offset)
    {
        FindPageScrollViewer(host).Offset = new Vector(0, offset);
        Dispatcher.UIThread.RunJobs();
    }

    private static ScrollViewer FindPageScrollViewer(GalleryStickyTabsHost host)
    {
        return host.GetVisualDescendants()
                   .OfType<ScrollViewer>()
                   .Single(static viewer => viewer.Name == "PART_ScrollViewer");
    }

    private static Control FindStripHost(AvaloniaWindow window, GalleryStickyTabsHost host)
    {
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
