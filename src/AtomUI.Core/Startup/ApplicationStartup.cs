using System.Runtime.CompilerServices;
using AtomUI.Reflection;
using Avalonia;

namespace AtomUI;

/// <summary>
/// Hooks AtomUI into application startup. The source generator emits the call for a partial application
/// class; when <c>RegisterServices</c> is overridden by hand, call <see cref="Attach" /> after the base call.
/// </summary>
public static class ApplicationStartup
{
    private static readonly ConditionalWeakTable<Application, object> s_attached = new();
    private static readonly object s_marker = new();

    /// <summary>
    /// Runs the AtomUI startup for the application with the configuration that <c>UseAtomUI</c> registered.
    /// The call does nothing when no configuration was registered or when the application is already attached.
    /// </summary>
    public static void Attach(Application application)
    {
        ArgumentNullException.ThrowIfNull(application);
        if (AvaloniaLocator.Current.GetService<StartupConfiguration>() is not { } configuration)
        {
            return;
        }
        if (!s_attached.TryAdd(application, s_marker))
        {
            return;
        }
        GridMeasureCacheCompatibility.EnsureInitialized();
        new StartupRun(application, configuration).Run();
    }

    /// <summary>
    /// Commits the configuration that <paramref name="configure" /> writes to a fresh builder directly: no
    /// early preparation runs in parallel with the creation of the application and the Splash is not shown.
    /// Intended for tests and internal tools.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// The application is already attached.
    /// </exception>
    internal static void AttachWith(Application application, Action<IAtomUIBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(application);
        ArgumentNullException.ThrowIfNull(configure);
        if (!s_attached.TryAdd(application, s_marker))
        {
            throw new InvalidOperationException(
                $"AtomUI is already initialized for Application '{application.GetType().FullName}'.");
        }
        GridMeasureCacheCompatibility.EnsureInitialized();
        var builder = new AtomUIBuilder(application.GetType());
        configure(builder);
        var process = new StartupProcess();
        StartupCommit.Commit(application, builder, process, null, null);
        process.MoveTo(StartupState.Committed);
        process.CompleteReady();
    }

    internal static bool IsAttached(Application application) => s_attached.TryGetValue(application, out _);
}
