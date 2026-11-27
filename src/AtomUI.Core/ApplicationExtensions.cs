using AtomUI.Localization;
using AtomUI.Theme;
using Avalonia;

namespace AtomUI;

/// <summary>
/// Gives an application access to the AtomUI runtime that was initialized for it, to its managers and to its
/// startup state.
/// </summary>
public static class ApplicationExtensions
{
    /// <summary>
    /// Gets the initialized application's AtomUI runtime.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// AtomUI is not initialized for this application.
    /// </exception>
    public static IAtomUI GetAtomUI(this Application application)
    {
        ArgumentNullException.ThrowIfNull(application);
        return ApplicationScopeRegistry.Get(application) ??
            throw new InvalidOperationException("AtomUI is not initialized for this application.");
    }

    /// <summary>
    /// Gets the language manager of the application's AtomUI runtime, or <see langword="null" /> while AtomUI is not
    /// initialized for the application.
    /// </summary>
    public static ILanguageManager? GetLanguageManager(this Application application)
    {
        ArgumentNullException.ThrowIfNull(application);
        return ApplicationScopeRegistry.Get(application)?.LanguageManager;
    }

    /// <summary>
    /// Gets the localizer of the application's AtomUI runtime, or <see langword="null" /> while AtomUI is not
    /// initialized for the application.
    /// </summary>
    public static ILocalizer? GetLocalizer(this Application application)
    {
        ArgumentNullException.ThrowIfNull(application);
        return ApplicationScopeRegistry.Get(application)?.Localizer;
    }

    /// <summary>
    /// Gets the theme manager of the application's AtomUI runtime, or <see langword="null" /> while AtomUI is not
    /// initialized for the application.
    /// </summary>
    public static IThemeManager? GetThemeManager(this Application application)
    {
        ArgumentNullException.ThrowIfNull(application);
        return ApplicationScopeRegistry.Get(application)?.ThemeManager;
    }

    /// <summary>
    /// Gets the startup state and the awaitable framework-ready result of the application's single startup.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// AtomUI is not initialized for this application.
    /// </exception>
    /// <remarks>
    /// Requires that <see cref="AppBuilderExtensions.UseAtomUI(AppBuilder, Action{IAtomUIBuilder})" /> was called
    /// and that the application has since called <see cref="ApplicationStartup.Attach" />.
    /// </remarks>
    public static IStartup GetStartup(this Application application)
    {
        ArgumentNullException.ThrowIfNull(application);
        return ApplicationScopeRegistry.Get(application)?.Startup ??
            throw new InvalidOperationException("AtomUI startup is unavailable. Call UseAtomUI before GetStartup.");
    }
}
