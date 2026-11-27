using Avalonia;
using AtomUI.Media;

namespace AtomUI.Fonts.AlibabaSans;

/// <summary>
/// Extension methods that add the <see cref="AlibabaSansFontCollection" /> fonts to an Avalonia application
/// through its <see cref="AppBuilder" />.
/// </summary>
public static class AppBuilderExtension
{
    /// <summary>
    /// Registers the Alibaba Sans font collection with the application's font manager under the key
    /// <c>fonts:AlibabaSans</c>, for the lifetime of that font manager.
    /// </summary>
    public static AppBuilder WithAlibabaSansFont(this AppBuilder appBuilder)
    {
        return appBuilder.ConfigureFonts(fontManager =>
        {
            FontCollectionLifetime.RegisterPersistent(fontManager, new Uri("fonts:AlibabaSans"),
                static () => new AlibabaSansFontCollection());
        });
    }
}
