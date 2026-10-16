using Avalonia;
using AtomUI.Media;

namespace AtomUI.Fonts.AlibabaPuHuiTi;

public static class AppBuilderExtension
{
    public static AppBuilder WithAlibabaPuHuiTiFont(this AppBuilder appBuilder)
    {
        return appBuilder.ConfigureFonts(fontManager =>
        {
            FontCollectionLifetime.RegisterPersistent(fontManager, new Uri("fonts:AlibabaPuHuiTi"),
                static () => new AlibabaPuHuiTiFontCollection());
        });
    }
}
