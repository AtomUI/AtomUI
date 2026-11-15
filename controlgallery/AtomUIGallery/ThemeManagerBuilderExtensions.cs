using AtomUI;
using AtomUI.Themes.PrimeNG;
using AtomUI.Themes.Shadcn;
using AtomUI.Themes.Bootstrap;
using AtomUI.Themes.Material;
using AtomUI.Generated.AtomUIGallery;
using AtomUI.Toolkits.GalleryBase;

namespace AtomUIGallery;

public static class ThemeManagerBuilderExtensions
{
    /// <summary>
    /// Declares the Gallery package, its shell dependency, translations and theme packages.
    /// </summary>
    public static IAtomUIBuilder UseGalleryControls(this IAtomUIBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return GeneratedControlPackageRegistration.Definition.Register(builder, static package => package
            .DependsOn(static root => root.UseGalleryBase(AtomUIGalleryModule.Configure))
            .DependsOn(static root => root.Theme.UsePrimeNGThemes().UseShadcnThemes().UseBootstrapThemes().UseMaterialThemes()));
    }
}
