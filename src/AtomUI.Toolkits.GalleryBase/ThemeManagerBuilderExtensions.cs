using AtomUI.Generated.AtomUIToolkitsGalleryBase;
using AtomUI.Toolkits.GalleryBase.Configuration;

namespace AtomUI.Toolkits.GalleryBase;

/// <summary>
/// Provides the entry point that adds the Gallery base package to an AtomUI builder.
/// </summary>
public static class ThemeManagerBuilderExtensions
{
    internal const string PackageId = "AtomUI.Toolkits.GalleryBase";

    /// <summary>
    /// Adds the Gallery base package to the builder: the token rules of its controls and a dependency on the
    /// desktop controls package.
    /// </summary>
    /// <param name="builder">
    /// The builder that assembles the application.
    /// </param>
    /// <param name="configure">
    /// Configures the product options of the Gallery. The resulting configuration is registered when the package
    /// is committed. When <see langword="null" />, no configuration is registered and
    /// <see cref="GalleryBaseConfigurationProvider.GetRequired" /> throws.
    /// </param>
    /// <returns>
    /// The builder, for chaining.
    /// </returns>
    public static IAtomUIBuilder UseGalleryBase(this IAtomUIBuilder builder,
                                                Action<GalleryBaseOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(builder);
        // The Gallery's own controls take language-independent values (derived from the global tokens only).
        GeneratedControlTokenRuleRegistration.Register(builder.Theme);
        return PackageMetadata.Definition.Register(builder, package =>
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
