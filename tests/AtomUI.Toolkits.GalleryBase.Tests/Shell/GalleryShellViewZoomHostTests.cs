using AtomUI.Toolkits.GalleryBase.Configuration;
using AtomUI.Toolkits.GalleryBase.Controls;
using AtomUI.Toolkits.GalleryBase.Shell;
using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.VisualTree;
using ReactiveUI;
using ReactiveUI.Avalonia;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Toolkits.GalleryBase.Tests.Shell;

public class GalleryShellViewZoomHostTests
{
    [Fact]
    public void Shell_Hosts_Exactly_One_Zoom_Overlay_Host_Wrapping_The_Code_Drawer_Host()
    {
        AvaloniaTestApp.EnsureInitialized();
        using var shell = new GalleryShellView(CreateConfiguration(), new Border(), new RoutingState());
        var window = new AvaloniaWindow { Width = 1000, Height = 700, Content = shell };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var zoomHost = shell.GetVisualDescendants()
                                .OfType<ShowCaseZoomOverlayHost>()
                                .Single();
            zoomHost.PageContent.ShouldBeOfType<GalleryShowCaseCodeDrawerHost>();
        }
        finally
        {
            window.Close();
        }
    }

    private static GalleryBaseConfiguration CreateConfiguration()
    {
        var options = new GalleryBaseOptions();
        options.Branding.VersionText = "v1";
        options.Navigation.DefaultRoute = "Overview";
        options.Navigation.AddPage("Overview", "Overview");
        options.Routes.Map(
            "Overview",
            screen => new TestRouteViewModel(screen),
            () => new TestRouteView());
        return options.BuildConfiguration();
    }

    private sealed class TestRouteViewModel(IScreen hostScreen) : ReactiveObject, IRoutableViewModel
    {
        public string UrlPathSegment => "Overview";

        public IScreen HostScreen { get; } = hostScreen;
    }

    private sealed class TestRouteView : ReactiveUserControl<TestRouteViewModel>
    {
    }
}
