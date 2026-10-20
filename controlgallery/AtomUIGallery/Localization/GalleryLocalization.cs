using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Styling;

namespace AtomUIGallery.Localization;

internal static class GalleryLocalization
{
    public static ILanguageManager? GetLanguageManager()
    {
        return Application.Current is { } application
            ? global::AtomUI.ApplicationExtensions.GetLanguageManager(application)
            : null;
    }

    public static string Get<TResourceKind>(TResourceKind resourceKind, string fallback)
        where TResourceKind : struct, Enum
    {
        return Application.Current is { } application &&
               global::AtomUI.ApplicationExtensions.GetLocalizer(application) is { } localizer
            ? localizer.Get(resourceKind)
            : fallback;
    }

    public static string Format<TResourceKind>(
        TResourceKind resourceKind,
        string fallback,
        params object?[] arguments)
        where TResourceKind : struct, Enum
    {
        return Application.Current is { } application &&
               global::AtomUI.ApplicationExtensions.GetLocalizer(application) is { } localizer
            ? localizer.Format(resourceKind, arguments)
            : string.Format(
                GetFormattingCulture(),
                fallback,
                arguments);
    }

    public static CultureInfo GetFormattingCulture()
    {
        return GetLanguageManager()?.Current.FormattingCulture ?? CultureInfo.CurrentCulture;
    }

    public static IDisposable CreateBinding<TResourceKind>(
        AvaloniaObject target,
        AvaloniaProperty targetProperty,
        TResourceKind resourceKind,
        BindingPriority priority = BindingPriority.Template,
        Func<object?, object?>? converter = null)
        where TResourceKind : struct, Enum
    {
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(targetProperty);

        var application = Application.Current ?? throw new ApplicationException(
            "The application instance does not exist.");
        var themeVariant = (application as IThemeVariantHost)?.ActualThemeVariant;
        var observable = application.Styles.GetResourceObservable(resourceKind, themeVariant, converter);
        return target.Bind(targetProperty, observable, priority);
    }
}
