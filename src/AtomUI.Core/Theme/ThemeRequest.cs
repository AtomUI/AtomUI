using AtomUI.Theme.Configuration;

namespace AtomUI.Theme;

/// <summary>
/// What triggered a theme change; every <see cref="ThemeRequest" /> carries one.
/// </summary>
public enum ThemeTransitionReason : byte
{
    /// <summary>
    /// The theme is applied when the application starts.
    /// </summary>
    Startup,
    /// <summary>
    /// The theme was requested explicitly, for example through a selection the user made.
    /// </summary>
    UserRequest,
    /// <summary>
    /// The theme follows the platform appearance: the request is one of the pair prepared for the light and dark
    /// appearances, and it is applied at startup or when the platform changes.
    /// </summary>
    FollowSystem,
    /// <summary>
    /// The theme catalog was reloaded and the theme was applied again from it.
    /// </summary>
    CatalogReload,
    /// <summary>
    /// The local config of a scope changed, and the theme was derived again for it.
    /// </summary>
    LocalConfigChanged,
    /// <summary>
    /// The tree of theme scopes changed, for example because a scope was attached or detached.
    /// </summary>
    ScopeTopologyChanged
}

/// <summary>
/// A request to show one theme: the theme, the configuration layered over its definition defaults, and what
/// triggered the change.
/// </summary>
/// <param name="ThemeId">
/// The identity of the theme to show.
/// </param>
/// <param name="Config">
/// The configuration to layer over the theme definition defaults, or <see langword="null" /> when the request
/// adds none.
/// </param>
/// <param name="Reason">
/// What triggered the change.
/// </param>
public sealed record ThemeRequest(
    string ThemeId,
    ThemeConfig? Config,
    ThemeTransitionReason Reason);
