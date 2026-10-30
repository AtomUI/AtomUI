using AtomUI.Generated.AtomUIToolkitsGalleryBase;
using AtomUI.Toolkits.GalleryBase.Configuration;

namespace AtomUI.Toolkits.GalleryBase;

public static class ThemeManagerBuilderExtensions
{
    internal const string PackageId = "AtomUI.Toolkits.GalleryBase";

    public static IAtomUIBuilder UseGalleryBase(this IAtomUIBuilder builder,
                                                Action<GalleryBaseOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return GeneratedControlPackageRegistration.Definition.Register(builder, package =>
        {
            package.DependsOn(static root => Desktop.Controls.ThemeManagerBuilderExtensions.UseDesktopControls(root));
            if (configure is not null)
            {
                var options = new GalleryBaseOptions();
                configure(options);
                var configuration = options.BuildConfiguration();
                package.OnCommit(context => context.Own(
                    GalleryBaseConfigurationProvider.Register(context.Application, configuration)));
            }
        });
    }
}
