using AtomUI.Toolkits.GalleryBase.Configuration;
using Avalonia;
using ReactiveUI;
using ReactiveUI.Avalonia;
using Shouldly;
using Xunit;

namespace AtomUI.Toolkits.GalleryBase.Tests.Configuration;

public class GalleryBaseRegistrationTests
{
    static GalleryBaseRegistrationTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Duplicate_entry_rejects_before_configuration_callback_or_global_state_change()
    {
        var originalConfiguration = GalleryBaseConfigurationProvider.Current;
        var callbacks = 0;
        GalleryBaseConfiguration? firstConfiguration = null;
        try
        {
            Should.Throw<Exception>(() => AtomUIStartup.AttachWith(new Application(), builder =>
            {
                builder.UseGalleryBase(options =>
                {
                    callbacks++;
                    Configure(options, "first");
                });
                firstConfiguration = GalleryBaseConfigurationProvider.Current;
                builder.UseGalleryBase(options =>
                {
                    callbacks++;
                    Configure(options, "duplicate");
                });
            }));
            firstConfiguration.ShouldBeSameAs(originalConfiguration);
            callbacks.ShouldBe(1);
            GalleryBaseConfigurationProvider.Current.ShouldBeSameAs(originalConfiguration);
        }
        finally
        {
            // The shared fixture may start without product configuration; restore that state too.
            GalleryBaseConfigurationProvider.SetCurrent(originalConfiguration!);
        }
    }

    private static void Configure(GalleryBaseOptions options, string version)
    {
        options.Branding.VersionText = version;
        options.Navigation.DefaultRoute = "Overview";
        options.Navigation.AddPage("Overview", "Overview");
        options.Routes.Map("Overview", screen => new RouteViewModel(screen), () => new RouteView());
    }

    private sealed class RouteViewModel(IScreen hostScreen) : ReactiveObject, IRoutableViewModel
    {
        public string UrlPathSegment => "Overview";
        public IScreen HostScreen { get; } = hostScreen;
    }
    private sealed class RouteView : ReactiveUserControl<RouteViewModel>;
}
