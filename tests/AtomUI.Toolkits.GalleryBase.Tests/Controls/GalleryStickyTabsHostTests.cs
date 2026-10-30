using AtomUI.Toolkits.GalleryBase.Controls;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AtomUIWindow = AtomUI.Desktop.Controls.Window;

namespace AtomUI.Toolkits.GalleryBase.Tests.Controls;

public class GalleryStickyTabsHostTests
{
    public GalleryStickyTabsHostTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Bounded_Content_Clamps_The_Content_Host_To_The_Viewport_Remainder()
    {
        var host = CreateHost(isContentHeightBounded: true);

        using var context = ShowInWindow(host);

        var pageScrollViewer = FindPageScrollViewer(host);
        var contentHost = FindContentHost(host);
        var headerHost = host.GetVisualDescendants()
                             .OfType<ContentPresenter>()
                             .Single(static presenter => presenter.Name == "PART_HeaderHost");

        var expectedMaxHeight = pageScrollViewer.Viewport.Height - headerHost.Bounds.Height;
        contentHost.MaxHeight.ShouldBe(expectedMaxHeight, 1);
        contentHost.Bounds.Height.ShouldBe(contentHost.MaxHeight, 1);
        pageScrollViewer.Extent.Height.ShouldBe(pageScrollViewer.Viewport.Height, 1);
    }

    [Fact]
    public void Unbounded_Content_Leaves_The_Content_Host_Unclamped()
    {
        var host = CreateHost(isContentHeightBounded: false);

        using var context = ShowInWindow(host);

        var contentHost = FindContentHost(host);
        double.IsPositiveInfinity(contentHost.MaxHeight).ShouldBeTrue();
    }

    private static GalleryStickyTabsHost CreateHost(bool isContentHeightBounded)
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
            Content = content,
            IsContentHeightBounded = isContentHeightBounded
        };
    }

    private static ScrollViewer FindPageScrollViewer(GalleryStickyTabsHost host)
    {
        return host.GetVisualDescendants()
                   .OfType<ScrollViewer>()
                   .Single(static viewer => viewer.Name == "PART_ScrollViewer");
    }

    private static ContentPresenter FindContentHost(GalleryStickyTabsHost host)
    {
        return host.GetVisualDescendants()
                   .OfType<ContentPresenter>()
                   .Single(static presenter => presenter.Name == "PART_ContentHost");
    }

    private static WindowContext ShowInWindow(GalleryStickyTabsHost host)
    {
        var window = new AtomUIWindow
        {
            Width = 1000,
            Height = 600,
            Content = host
        };

        window.Show();
        Dispatcher.UIThread.RunJobs();
        return new WindowContext(window);
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
}
