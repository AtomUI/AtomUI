using AtomUI.Controls.Primitives;
using AtomUI.Toolkits.GalleryBase.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;
using Button = Avalonia.Controls.Button;

namespace AtomUI.Toolkits.GalleryBase.Tests.Controls;

/// <summary>
/// 钉住的页签被提升到窗口级 adorner 层（位于所有内容之上）。
/// 放大 overlay 是模态覆盖，打开期间必须盖住页面的提升物：
/// 挂起粘滞提升（降级回面板），关闭后自动恢复提升。
/// </summary>
public class ShowCaseZoomSuppressesStickyElevationTests
{
    public ShowCaseZoomSuppressesStickyElevationTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Zooming_While_Pinned_Demotes_The_Elevated_Tabs()
    {
        var item = new ShowCaseItem
        {
            Title = "Zoom Me",
            Content = new Border { Width = 200, Height = 100 }
        };
        var (zoomHost, stickyHost, contentPanel) = CreateTree(item);

        using var context = ShowInWindow(zoomHost, 1000, 600, out var window);

        ScrollToPin(window, stickyHost);
        var panel = stickyHost.GetVisualDescendants().OfType<GalleryStickyTabsPanel>().Single();
        panel.IsStickyPinned.ShouldBeTrue("precondition: tabs must be pinned");
        FindStripHost(window).GetVisualParent().ShouldBeOfType<ScopeAwareAdornerLayer>(
            "precondition: tabs must be elevated before zooming");

        RaiseZoom(window, item);

        var stripHost = FindStripHost(window);
        stripHost.GetVisualParent().ShouldBe(panel,
            "the elevated tabs must be demoted back into the page panel while the zoom overlay is open");
        var overlay = zoomHost.GetVisualDescendants().OfType<ShowCaseZoomOverlay>().Single();
        overlay.IsOpen.ShouldBeTrue();
    }

    [Fact]
    public void Closing_The_Zoom_Restores_The_Elevation_While_Still_Pinned()
    {
        var item = new ShowCaseItem
        {
            Title = "Zoom Me",
            Content = new Border { Width = 200, Height = 100 }
        };
        var (zoomHost, stickyHost, contentPanel) = CreateTree(item);

        using var context = ShowInWindow(zoomHost, 1000, 600, out var window);

        ScrollToPin(window, stickyHost);
        RaiseZoom(window, item);

        var overlay = zoomHost.GetVisualDescendants().OfType<ShowCaseZoomOverlay>().Single();
        overlay.RaiseEvent(new Avalonia.Input.KeyEventArgs
        {
            RoutedEvent = Avalonia.Input.InputElement.KeyDownEvent,
            Key         = Avalonia.Input.Key.Escape,
            Source      = overlay
        });
        PumpRendering(window);

        var stripHost = FindStripHost(window);
        stripHost.GetVisualParent().ShouldBeOfType<ScopeAwareAdornerLayer>(
            "the tabs must be re-elevated after the zoom overlay closes while still scrolled/pinned");
        item.Content.ShouldBeOfType<Border>();
    }

    [Fact]
    public void Zooming_Without_Sticky_Host_Does_Not_Throw()
    {
        var item = new ShowCaseItem
        {
            Title = "Zoom Me",
            Content = new Border { Width = 200, Height = 100 }
        };
        var zoomHost = new ShowCaseZoomOverlayHost
        {
            PageContent = new StackPanel { Children = { item } }
        };

        using var context = ShowInWindow(zoomHost, 500, 500, out var window);

        RaiseZoom(window, item);

        var overlay = zoomHost.GetVisualDescendants().OfType<ShowCaseZoomOverlay>().Single();
        overlay.IsOpen.ShouldBeTrue();
        item.Content.ShouldBeNull();
    }

    private static (ShowCaseZoomOverlayHost zoomHost, GalleryStickyTabsHost stickyHost, StackPanel content)
        CreateTree(ShowCaseItem item)
    {
        var contentPanel = new StackPanel();
        contentPanel.Children.Add(item);
        for (var i = 0; i < 30; i++)
        {
            contentPanel.Children.Add(new Border { Height = 60 });
        }

        var stickyHost = new GalleryStickyTabsHost
        {
            Header = new Border { Height = 120 },
            StickyContent = new Border { Height = 40 },
            Content = contentPanel
        };

        var zoomHost = new ShowCaseZoomOverlayHost { PageContent = stickyHost };
        return (zoomHost, stickyHost, contentPanel);
    }

    private static void ScrollToPin(Avalonia.Controls.Window window, GalleryStickyTabsHost stickyHost)
    {
        var scrollViewer = stickyHost.GetVisualDescendants()
                                     .OfType<ScrollViewer>()
                                     .Single(viewer => viewer.Name == "PART_ScrollViewer");
        scrollViewer.Offset = new Vector(0, 400);
        PumpRendering(window);
    }

    private static void RaiseZoom(Avalonia.Controls.Window window, ShowCaseItem item)
    {
        var zoomButton = item.GetVisualDescendants()
                             .OfType<AtomUI.Desktop.Controls.IconButton>()
                             .Single(button => button.Name == "PART_ZoomButton");
        zoomButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent) { Source = zoomButton });
        PumpRendering(window);
    }

    /// <summary>粘滞提升经 Render 优先级派发，headless 下需要渲染帧驱动（对齐 StickyMirror 测试模式）。</summary>
    private static void PumpRendering(Avalonia.Controls.Window window)
    {
        Dispatcher.UIThread.RunJobs();
        window.CaptureRenderedFrame();
        Dispatcher.UIThread.RunJobs();
    }

    private static Control FindStripHost(AvaloniaWindow window)
    {
        return window.GetVisualDescendants()
                     .OfType<Control>()
                     .Single(static control => control.Name == "PART_StickyContentHost");
    }

    private sealed class WindowContext : IDisposable
    {
        private readonly AtomUI.Desktop.Controls.Window _window;

        public WindowContext(AtomUI.Desktop.Controls.Window window)
        {
            _window = window;
        }

        public void Dispose()
        {
            _window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    private static WindowContext ShowInWindow(Control content, double width, double height,
        out AtomUI.Desktop.Controls.Window window)
    {
        window = new AtomUI.Desktop.Controls.Window { Width = width, Height = height, Content = content };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return new WindowContext(window);
    }
}
