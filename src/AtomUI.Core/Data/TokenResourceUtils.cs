using Avalonia;
using Avalonia.Controls;
using Avalonia.Styling;

namespace AtomUI.Data;

/// <summary>
/// One-time lookups of Token resources. Unlike the bindings of <see cref="TokenResourceBinder" />, the
/// result does not follow later resource changes.
/// </summary>
public static class TokenResourceUtils
{
    /// <summary>
    /// Looks the Token <paramref name="resourceKey" /> up in the resources visible from
    /// <paramref name="control" />, for <paramref name="themeVariant" /> or for the control's actual theme
    /// variant when it is <see langword="null" />.
    /// </summary>
    /// <returns>
    /// The resource value, or <see cref="AvaloniaProperty.UnsetValue" /> when no resource is registered under
    /// the key.
    /// </returns>
    public static object? FindTokenResource<TTokenKind>(Control control, TTokenKind resourceKey,
                                                        ThemeVariant? themeVariant = null)
        where TTokenKind : Enum
    {
        if (themeVariant is null)
        {
            themeVariant = control.ActualThemeVariant;
        }

        if (control.TryFindResource(resourceKey, themeVariant, out var value))
        {
            return value;
        }

        return AvaloniaProperty.UnsetValue;
    }

    /// <summary>
    /// Looks the Token <paramref name="resourceKey" /> up in the Application resources only, for
    /// <paramref name="themeVariant" />, or for the application's actual theme variant when it is
    /// <see langword="null" />.
    /// </summary>
    /// <returns>
    /// The resource value, <see cref="AvaloniaProperty.UnsetValue" /> when no resource is registered under
    /// the key, or <see langword="null" /> when there is no Application instance.
    /// </returns>
    public static object? FindGlobalTokenResource<TTokenKind>(TTokenKind resourceKey, ThemeVariant? themeVariant = null)
        where TTokenKind : Enum
    {
        var application = Application.Current;
        if (application is null)
        {
            return null;
        }

        if (themeVariant is null)
        {
            themeVariant = (application as IThemeVariantHost).ActualThemeVariant;
        }

        if (application.TryFindResource(resourceKey, themeVariant, out var value))
        {
            return value;
        }

        return AvaloniaProperty.UnsetValue;
    }
}
