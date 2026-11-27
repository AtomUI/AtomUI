using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Styling;

namespace AtomUI.Theme;

/// <summary>
/// Uses the application-local Avalonia theme entry points for participating controls.
/// </summary>
internal static class ScopedControlTheme
{
    private static readonly bool s_enabled =
        AppContext.TryGetSwitch("AtomUI.ScopedControlThemes", out var enabled) && enabled;

    internal static bool IsEnabled => s_enabled;

    internal static void Initialize()
    {
        if (s_enabled)
        {
            ScopedResource(null) = Resolve;
            ScopedTarget(null) = IsTarget;
            ScopedRefresh(null) = Refresh;
        }
    }

    internal static void Refresh(StyledElement element)
    {
        var context = ThemeScope.ResolveContext(element);
        if (context?.IsCompositionSnapshot == true)
        {
            return;
        }
        // Resource notifications reach the committed child scopes as well. A token-only
        // publish within the same schema registry keeps the effective theme instance and its native implicit cache.
        if (ThemeContext.IsPublishing && context is not null &&
            context.LastPublish.Previous is { } previous &&
            previous.Language.Id == context.Snapshot.Language.Id &&
            ReferenceEquals(previous.Registry, context.Snapshot.Registry))
        {
            return;
        }
        if (element.Theme is not null)
        {
            // The explicit Theme remains effective. Clearing it later must consult the current scope.
            ImplicitThemeCache(element) = null;
            return;
        }
        ReevaluateImplicitTheme(element);
    }

    internal static bool HasCachedImplicitTheme(StyledElement element) =>
        s_enabled && element.Theme is null && ImplicitThemeCache(element) is not null;

    private static bool IsTarget(StyledElement element) =>
        Application.Current is { } application &&
        ApplicationScopeRegistry.Get(application)?.ThemeManager.HasScopedControlTheme(element.StyleKey) == true;

    private static object? Resolve(StyledElement element, object key) =>
        Application.Current is { } application &&
        ApplicationScopeRegistry.Get(application)?.ThemeManager is { } manager
            ? manager.ResolveScopedControlTheme(element, key, application)
            : element.TryFindResource(key, out var value) ? value : null;

    [UnsafeAccessor(UnsafeAccessorKind.StaticField, Name = "_atomuiScopedThemeTarget")]
    private static extern ref Func<StyledElement, bool>? ScopedTarget(StyledElement? element);

    [UnsafeAccessor(UnsafeAccessorKind.StaticField, Name = "_atomuiScopedThemeResource")]
    private static extern ref Func<StyledElement, object, object?>? ScopedResource(StyledElement? element);

    [UnsafeAccessor(UnsafeAccessorKind.StaticField, Name = "_atomuiScopedThemeRefresh")]
    private static extern ref Action<StyledElement>? ScopedRefresh(StyledElement? element);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "ReevaluateImplicitTheme")]
    private static extern void ReevaluateImplicitTheme(StyledElement element);

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_implicitTheme")]
    private static extern ref ControlTheme? ImplicitThemeCache(StyledElement element);
}
