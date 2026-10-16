using AtomUI;
using AtomUI.Generated.AtomUIFontsAlibabaPuHuiTi;
using AtomUI.Media;
using Avalonia.Media;

namespace AtomUI.Fonts.AlibabaPuHuiTi;

public static class AlibabaPuHuiTiThemeManagerBuilderExtensions
{
    public static IAtomUIBuilder UseAlibabaPuHuiTiFont(this IAtomUIBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return GeneratedControlPackageRegistration.Definition.Ensure(builder, static package => package
            .OnCommit(static context => context.Own(FontCollectionLifetime.Acquire(
                FontManager.Current, new Uri("fonts:AlibabaPuHuiTi"), static () => new AlibabaPuHuiTiFontCollection()))));
    }
}
