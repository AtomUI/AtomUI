using AtomUI.DesignLanguages.AntDesign;
using Avalonia;
using Avalonia.Headless;
using Shouldly;
using Xunit;

namespace AtomUI.Core.Tests.Startup;

// Contract: the AtomUI kernel is committed before Application.Initialize(). Prism creates and shows the
// shell in Initialize and App.axaml references AtomUI resources; if the commit were deferred until after
// Initialize they would get neither theme nor localization. The other test apps in the repository write
// the hook by hand; this also covers the RegisterServices hook the generator produces for a partial app.
public class StartupContractTests
{
    // The dispatch completion callback of a headless session runs inline, so the dispatch must block;
    // otherwise the session thread would wait for itself in Dispose.
    [Fact]
    public async Task Generated_hook_commits_before_Initialize_and_reaches_Ready()
    {
        var session = HeadlessUnitTestSession.StartNew(typeof(HookedEntry), AvaloniaTestIsolationLevel.PerTest);
        try
        {
            TestApplication? previous = null;
            for (var instance = 0; instance < 2; instance++)
            {
                await session.Dispatch(() =>
                {
                    var application = (TestApplication)Application.Current!;
                    application.ShouldNotBeSameAs(previous);
                    application.ThemeManagerSeenInInitialize.ShouldBeTrue();
                    application.GetStartup().State.ShouldBe(StartupState.Ready);
                    application.GetStartup().Completion.IsCompletedSuccessfully.ShouldBeTrue();
                    previous = application;
                }, TestContext.Current.CancellationToken);
            }
        }
        finally
        {
            await Task.Run(session.Dispose, TestContext.Current.CancellationToken);
        }
    }

    public static class HookedEntry
    {
        public static AppBuilder BuildAvaloniaApp()
        {
            var platformReady = false;
            var configureCount = 0;
            return AppBuilder.Configure(() =>
                             {
                                 platformReady.ShouldBeTrue();
                                 configureCount.ShouldBe(1);
                                 return new TestApplication();
                             })
                             .UseHeadless(new AvaloniaHeadlessPlatformOptions())
                             .AfterPlatformServicesSetup(_ =>
                             {
                                 platformReady = true;
                                 configureCount = 0;
                             })
                             .UseAtomUI(builder =>
                             {
                                 // An ordinary configuration may create AvaloniaObject instances indirectly, so the
                                 // platform must settle the Dispatcher clock first.
                                 platformReady.ShouldBeTrue();
                                 configureCount++;
                                 GC.KeepAlive(new AvaloniaObject());
                                 builder.UseAntDesign();
                                 builder.WithInitialTheme(AntDesignThemes.DaybreakBlue);
                             })
                             .AfterApplicationSetup(_ => configureCount.ShouldBe(1));
        }
    }
}
