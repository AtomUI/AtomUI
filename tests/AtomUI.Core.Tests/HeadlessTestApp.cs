using Avalonia;
using Avalonia.Headless;

namespace AtomUI.Core.Tests;

internal static class HeadlessTestApp
{
    internal static void Run(Action action)
    {
        ArgumentNullException.ThrowIfNull(action);
        using var session = HeadlessUnitTestSession.StartNew(
            typeof(TestAppBuilder),
            AvaloniaTestIsolationLevel.PerTest);
        session.Dispatch(action, CancellationToken.None).GetAwaiter().GetResult();
    }

    internal static async Task RunAsync(Func<Task> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        var session = HeadlessUnitTestSession.StartNew(
            typeof(TestAppBuilder),
            AvaloniaTestIsolationLevel.PerTest);
        try
        {
            await session.Dispatch(
                async () =>
                {
                    await action();
                    return true;
                },
                CancellationToken.None).ConfigureAwait(false);
        }
        finally
        {
            await Task.Run(session.Dispose).ConfigureAwait(false);
        }
    }
}

public static class TestAppBuilder
{
    public static AppBuilder BuildAvaloniaApp()
    {
        return AppBuilder.Configure<TestApplication>()
                         .UseHeadless(new AvaloniaHeadlessPlatformOptions());
    }
}

// partial: the AtomUI generator adds the RegisterServices hook; the hook does nothing when there is
// no UseAtomUI configuration.
internal sealed partial class TestApplication : Application
{
    /// <summary>
    /// Whether the AtomUI kernel is already committed when Initialize runs. Used by the startup contract
    /// tests.
    /// </summary>
    internal bool ThemeManagerSeenInInitialize { get; private set; }

    public override void Initialize()
    {
        ThemeManagerSeenInInitialize = this.GetThemeManager() is not null;
    }
}
