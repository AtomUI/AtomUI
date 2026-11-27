using Avalonia;
using AtomUI.Media;

namespace AtomUI.Fonts.AlibabaPuHuiTi;

/// <summary>
/// Extension methods that add the <see cref="AlibabaPuHuiTiFontCollection" /> fonts to an Avalonia application
/// through its <see cref="AppBuilder" />.
/// </summary>
public static class AppBuilderExtension
{
    /// <summary>
    /// Registers the Alibaba PuHuiTi font collection with the application's font manager under the key
    /// <c>fonts:AlibabaPuHuiTi</c>, for the lifetime of that font manager.
    /// </summary>
    public static AppBuilder WithAlibabaPuHuiTiFont(this AppBuilder appBuilder)
    {
        return appBuilder.ConfigureFonts(fontManager =>
        {
            FontCollectionLifetime.RegisterPersistent(fontManager, new Uri("fonts:AlibabaPuHuiTi"),
                static () => new AlibabaPuHuiTiFontCollection());
        });
    }
}
