using AtomUI;
using AtomUI.Generated.AtomUIGallery;
using AtomUI.Toolkits.GalleryBase;

namespace AtomUIGallery;

public static class ThemeManagerBuilderExtensions
{
    /// <summary>
    /// Declares the Gallery package, its shell dependency, translations and compiled themes.
    /// </summary>
    public static IAtomUIBuilder UseGalleryControls(this IAtomUIBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return GeneratedControlPackageRegistration.Definition.Register(builder, static package => package
            .DependsOn(static root => root.UseGalleryBase(AtomUIGalleryModule.Configure))
            .DependsOn(static root => GeneratedThemePackageRegistration.Register(root.Theme)));
    }
}
