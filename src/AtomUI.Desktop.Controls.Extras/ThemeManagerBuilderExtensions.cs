using AtomUI.Generated.AtomUIDesktopControlsExtras;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Extension methods that register the supplemental desktop controls package on an <see cref="IAtomUIBuilder" />.
/// </summary>
public static class ExtrasThemeManagerBuilderExtensions
{
    internal const string PackageId = "AtomUI.Desktop.Controls.Extras";

    /// <summary>
    /// Registers the supplemental desktop controls package on the builder. Its dependency, the desktop control
    /// package, is enabled first when it is not enabled yet, so
    /// <see cref="ThemeManagerBuilderExtensions.UseDesktopControls" /> need not be called separately.
    /// </summary>
    /// <param name="builder">
    /// The builder that assembles the application's packages.
    /// </param>
    /// <returns>
    /// The <paramref name="builder" />, for chaining.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="builder" /> is <see langword="null" />.
    /// </exception>
    public static IAtomUIBuilder UseDesktopExtras(this IAtomUIBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return PackageMetadata.Definition.Register(builder, static package => package
            .DependsOn(static builder => builder.UseDesktopControls()));
    }
}
