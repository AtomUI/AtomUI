using AtomUI.Toolkits.GalleryBase.Controls;
using AtomUIGallery.ShowCases.Space;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.VisualTree;
using ReactiveUI;
using Shouldly;
using Xunit;
using AtomUIWindow   = AtomUI.Desktop.Controls.Window;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUIGallery.Tests.ShowCases;

public class SpaceShowCaseStickyMaximizeTests
{
    public SpaceShowCaseStickyMaximizeTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Maximize_While_Tabs_Pinned_Keeps_The_Sticky_Geometry_Consistent()
    {
        var page = new SpaceShowCase
        {
            DataContext = new SpaceViewModel(new TestScreen())
        };

        using var context = ShowInWindow(page, 1000, 600, out var window);

        var scrollViewer = FindPageScrollViewer(page);
        scrollViewer.Offset = new Vector(0, 400);
        Dispatcher.UIThread.RunJobs();

        var panel = page.GetVisualDescendants().OfType<GalleryStickyTabsPanel>().Single();
        panel.IsStickyPinned.ShouldBeTrue("precondition: tabs must be pinned after scrolling");

        window.Width  = 2560;
        window.Height = 1300;
        Dispatcher.UIThread.RunJobs();

        AssertStickyAndContentGeometryConsistent(window, page, scrollViewer, panel);
    }

    [Fact]
    public void Maximize_After_Zooming_While_Tabs_Pinned_Keeps_Item_Layout_Consistent()
    {
        var page = new SpaceShowCase
        {
            DataContext = new SpaceViewModel(new TestScreen())
        };

        using var context = ShowInWindow(page, 1000, 600, out var window);

        var scrollViewer = FindPageScrollViewer(page);
        scrollViewer.Offset = new Vector(0, 400);
        Dispatcher.UIThread.RunJobs();

        var panel = page.GetVisualDescendants().OfType<GalleryStickyTabsPanel>().Single();
        panel.IsStickyPinned.ShouldBeTrue("precondition: tabs must be pinned after scrolling");

        ZoomFirstItem(page, window);
        CloseZoom(page, window);

        window.Width  = 2560;
        window.Height = 1300;
        Dispatcher.UIThread.RunJobs();

        AssertStickyAndContentGeometryConsistent(window, page, scrollViewer, panel);
        AssertVisibleItemsHaveNoGiantInnerGap(page);
    }

    [Fact]
    public void Maximize_While_Zoomed_With_Tabs_Pinned_Keeps_Item_Layout_Consistent()
    {
        var page = new SpaceShowCase
        {
            DataContext = new SpaceViewModel(new TestScreen())
        };

        using var context = ShowInWindow(page, 1000, 600, out var window);

        var scrollViewer = FindPageScrollViewer(page);
        scrollViewer.Offset = new Vector(0, 400);
        Dispatcher.UIThread.RunJobs();

        var panel = page.GetVisualDescendants().OfType<GalleryStickyTabsPanel>().Single();
        panel.IsStickyPinned.ShouldBeTrue("precondition: tabs must be pinned after scrolling");

        ZoomFirstItem(page, window);

        window.Width  = 2560;
        window.Height = 1300;
        Dispatcher.UIThread.RunJobs();

        CloseZoom(page, window);
        Dispatcher.UIThread.RunJobs();

        AssertStickyAndContentGeometryConsistent(window, page, scrollViewer, panel);
        AssertVisibleItemsHaveNoGiantInnerGap(page);
    }

    private static void ZoomFirstItem(SpaceShowCase page, AvaloniaWindow window)
    {
        var item = FirstZoomableItem(page);
        item.IsZoomEnabled.ShouldBeTrue();
        var zoomButton = item.GetVisualDescendants()
                             .OfType<AtomUI.Desktop.Controls.IconButton>()
                             .Single(static button => button.Name == "PART_ZoomButton");
        zoomButton.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(
            Avalonia.Controls.Button.ClickEvent) { Source = zoomButton });
        Dispatcher.UIThread.RunJobs();
    }

    private static void CloseZoom(SpaceShowCase page, AvaloniaWindow window)
    {
        var overlay = window.GetVisualDescendants()
                            .OfType<AtomUI.Toolkits.GalleryBase.Controls.ShowCaseZoomOverlay>()
                            .Single();
        overlay.IsOpen.ShouldBeTrue();
        overlay.CloseRequested += (_, _) => overlay.IsOpen = false;
        overlay.RaiseEvent(new Avalonia.Input.KeyEventArgs
        {
            RoutedEvent = Avalonia.Input.InputElement.KeyDownEvent,
            Key         = Avalonia.Input.Key.Escape,
            Source      = overlay
        });
        Dispatcher.UIThread.RunJobs();
        overlay.IsOpen.ShouldBeFalse();
    }

    private static ShowCaseItem FirstZoomableItem(SpaceShowCase page)
    {
        return page.GetVisualDescendants()
                   .OfType<ShowCaseItem>()
                   .First(static item => item.IsEffectivelyVisible && item.IsZoomEnabled && item.Content is not null);
    }

    /// <summary>
    /// 可见示例的标题行与内容之间不允许出现巨型空白带（正常间距在百像素量级以内）。
    /// </summary>
    private static void AssertVisibleItemsHaveNoGiantInnerGap(SpaceShowCase page)
    {
        foreach (var item in page.GetVisualDescendants()
                                 .OfType<ShowCaseItem>()
                                 .Where(static candidate =>
                                     candidate.IsEffectivelyVisible &&
                                     !candidate.IsFake &&
                                     candidate.IsDeferredContentMaterialized)
                                 .Take(6))
        {
            item.Bounds.Height.ShouldBeLessThan(900,
                "a visible showcase item must not balloon into a giant blank frame");
        }
    }

    private static void AssertStickyAndContentGeometryConsistent(
        AvaloniaWindow window,
        SpaceShowCase page,
        ScrollViewer scrollViewer,
        GalleryStickyTabsPanel panel)
    {
        var stripHost = window.GetVisualDescendants()
                              .OfType<Control>()
                              .Single(static control => control.Name == "PART_StickyContentHost");

        if (panel.IsStickyPinned)
        {
            var stripRect = stripHost.TranslatePoint(default, window) is { } topLeft
                ? new Rect(topLeft, stripHost.Bounds.Size)
                : default;
            stripHost.Height.ShouldBeLessThan(80,
                "elevated strip must keep the natural tab height instead of stretching into a blank band");
            stripRect.Y.ShouldBeLessThan(140);
            stripRect.Width.ShouldBe(scrollViewer.Viewport.Width, 1);
        }
        else
        {
            stripHost.GetVisualParent().ShouldBe(panel);
            double.IsNaN(stripHost.Height).ShouldBeTrue("elevation height must be cleared on restore");
        }

        var examplesPanel = page.GetVisualDescendants().OfType<ShowCasePanel>().Single();
        var panelTop = examplesPanel.TranslatePoint(default, window) ?? default;
        panelTop.Y.ShouldBeLessThan(420,
            "examples content must start right below the header/tabs, not be pushed down by a giant gap");
    }

    private static ScrollViewer FindPageScrollViewer(SpaceShowCase page)
    {
        return page.GetVisualDescendants()
                   .OfType<ScrollViewer>()
                   .Single(static viewer => viewer.Name == "PART_ScrollViewer");
    }

    private sealed class TestScreen : IScreen
    {
        public RoutingState Router { get; } = new();
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

    private static WindowContext ShowInWindow(Control content, double width, double height,
        out AtomUIWindow window)
    {
        // 与真实 Shell 组合一致：页面由 ShowCaseZoomOverlayHost 包裹
        var zoomHost = new AtomUI.Toolkits.GalleryBase.Controls.ShowCaseZoomOverlayHost
        {
            PageContent = content
        };
        window = new AtomUIWindow
        {
            Width   = width,
            Height  = height,
            Content = zoomHost
        };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return new WindowContext(window);
    }
}
