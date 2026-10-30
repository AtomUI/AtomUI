using AtomUI;
using AtomUI.Fonts.AlibabaSans;
using AtomUI.Generated.AtomUIFontsAlibabaSans;
using AtomUI.Media;
using Avalonia.Media;

public static class AlibabaSansThemeManagerBuilderExtensions
{
    public static IAtomUIBuilder UseAlibabaSansFont(this IAtomUIBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return GeneratedControlPackageRegistration.Definition.Ensure(builder, static package => package
            .OnCommit(static context => context.Own(FontCollectionLifetime.Acquire(
                FontManager.Current, new Uri("fonts:AlibabaSans"), static () => new AlibabaSansFontCollection()))));
    }
}
