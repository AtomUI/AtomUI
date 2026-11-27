using Avalonia.Controls;

namespace AtomUI;

/// <summary>
/// Provides <see cref="UseMainWindow" />, which lets the application create its main window during the startup.
/// </summary>
public static class StartupBuilderExtensions
{
    /// <summary>
    /// Calls <paramref name="factory" /> on the UI thread once the main loop has started. The factory may await
    /// its data loading and report progress meanwhile. The returned window is assigned to
    /// <c>IClassicDesktopStyleApplicationLifetime.MainWindow</c> and shown. This requires the classic desktop
    /// application lifetime, and application code must not assign MainWindow itself at the same time.
    /// </summary>
    /// <param name="builder">
    /// The builder of the AtomUI setup.
    /// </param>
    /// <param name="factory">
    /// Creates the main window. It receives a <see cref="MainWindowContext" /> for reporting progress.
    /// </param>
    /// <returns>
    /// The same <paramref name="builder" />.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="builder" /> is <see langword="null" />.
    /// </exception>
    public static IAtomUIBuilder UseMainWindow(this IAtomUIBuilder builder,
                                              Func<MainWindowContext, Task<Window>> factory)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ((AtomUIBuilder)builder).SetMainWindowFactory(factory);
        return builder;
    }
}
