using Avalonia;
using AtomUI.Media;

namespace AtomUI.Fonts.AlibabaSans;

public static class AppBuilderExtension
{
    public static AppBuilder WithAlibabaSansFont(this AppBuilder appBuilder)
    {
        return appBuilder.ConfigureFonts(fontManager =>
        {
            FontCollectionLifetime.RegisterPersistent(fontManager, new Uri("fonts:AlibabaSans"),
                static () => new AlibabaSansFontCollection());
        });
    }
}
