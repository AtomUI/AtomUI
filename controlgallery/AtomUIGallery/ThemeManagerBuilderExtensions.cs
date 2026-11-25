using AtomUI;
using AtomUI.Generated.AtomUIGallery;
using AtomUI.Toolkits.GalleryBase;

namespace AtomUIGallery;

public static class ThemeManagerBuilderExtensions
{
    /// <summary>
    /// Declares the Gallery package, its shell dependency and translations.
    /// </summary>
    public static IAtomUIBuilder UseGalleryControls(this IAtomUIBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return PackageMetadata.Definition.Register(builder, static package => package
            .DependsOn(static root => root.UseGalleryBase(AtomUIGalleryModule.Configure)));
    }
}
