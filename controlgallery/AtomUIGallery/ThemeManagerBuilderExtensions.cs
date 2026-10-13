using AtomUI;
using AtomUI.Generated.AtomUIGallery;

namespace AtomUIGallery;

public static class ThemeManagerBuilderExtensions
{
    /// <summary>
    /// Registers Gallery-level services shared by all hosts. The GalleryBase control package
    /// entry (UseGalleryBase) must be invoked directly by the application project so the
    /// linked-registration plan can see it.
    /// </summary>
    public static IAtomUIBuilder UseGalleryControls(this IAtomUIBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        GeneratedLanguageModuleRegistration.Register(builder.Localization);
        GeneratedThemePackageRegistration.Register(builder.Theme);

        return builder;
    }
}
