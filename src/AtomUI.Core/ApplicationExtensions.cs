using AtomUI.Localization;
using AtomUI.Theme;
using Avalonia;

namespace AtomUI;

public static class ApplicationExtensions
{
    /// <summary>Gets the initialized application's AtomUI runtime.</summary>
    public static IAtomUI GetAtomUI(this Application application)
    {
        ArgumentNullException.ThrowIfNull(application);
        return ApplicationScopeRegistry.Get(application) ??
            throw new InvalidOperationException("AtomUI is not initialized for this application.");
    }

    public static ILanguageManager? GetLanguageManager(this Application application)
    {
        ArgumentNullException.ThrowIfNull(application);
        return ApplicationScopeRegistry.Get(application)?.LanguageManager;
    }

    public static ILocalizer? GetLocalizer(this Application application)
    {
        ArgumentNullException.ThrowIfNull(application);
        return ApplicationScopeRegistry.Get(application)?.Localizer;
    }

    public static IThemeManager? GetThemeManager(this Application application)
    {
        ArgumentNullException.ThrowIfNull(application);
        return ApplicationScopeRegistry.Get(application)?.ThemeManager;
    }

    /// <summary>获取应用唯一的启动状态与框架就绪结果。须在 UseAtomUI 注册后调用。</summary>
    public static IAtomUIStartup GetStartup(this Application application)
    {
        ArgumentNullException.ThrowIfNull(application);
        return ApplicationScopeRegistry.Get(application)?.Startup ??
            throw new InvalidOperationException("AtomUI startup is unavailable. Call UseAtomUI before GetStartup.");
    }
}
