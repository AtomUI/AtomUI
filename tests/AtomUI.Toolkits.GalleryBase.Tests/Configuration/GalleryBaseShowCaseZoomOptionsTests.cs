using AtomUI.Toolkits.GalleryBase.Configuration;
using ReactiveUI;
using ReactiveUI.Avalonia;
using Shouldly;
using Xunit;

namespace AtomUI.Toolkits.GalleryBase.Tests.Configuration;

public class GalleryBaseShowCaseZoomOptionsTests
{
    [Fact]
    public void ShowCase_Zoom_Is_Enabled_By_Default()
    {
        var configuration = CreateConfiguration();

        configuration.ShowCaseZoom.IsEnabled.ShouldBeTrue();
    }

    [Fact]
    public void ShowCase_Zoom_Can_Be_Disabled_Via_Options()
    {
        var options = CreateOptions();
        options.ShowCaseZoom.IsEnabled = false;

        var configuration = options.BuildConfiguration();

        configuration.ShowCaseZoom.IsEnabled.ShouldBeFalse();
    }

    private static GalleryBaseOptions CreateOptions()
    {
        var options = new GalleryBaseOptions();
        options.Branding.VersionText = "v1";
        options.Navigation.DefaultRoute = "Overview";
        options.Navigation.AddPage("Overview", "Overview");
        options.Routes.Map(
            "Overview",
            screen => new TestRouteViewModel(screen),
            () => new TestRouteView());
        return options;
    }

    private static GalleryBaseConfiguration CreateConfiguration()
    {
        return CreateOptions().BuildConfiguration();
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
