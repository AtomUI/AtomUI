using AtomUI;
using AtomUI.Fonts.AlibabaSans;
using AtomUI.Generated.AtomUIFontsAlibabaSans;
using AtomUI.Media;
using Avalonia.Media;

/// <summary>
/// Extension methods that add the <see cref="AlibabaSansFontCollection" /> fonts to an
/// <see cref="IAtomUIBuilder" />.
/// </summary>
public static class AlibabaSansThemeManagerBuilderExtensions
{
    /// <summary>
    /// Ensures the Alibaba Sans font package is part of the AtomUI build. When the package is committed, the Alibaba
    /// Sans font collection is acquired for <see cref="FontManager.Current" /> under the key <c>fonts:AlibabaSans</c>,
    /// and the lease on it is transferred to the package's application lifetime.
    /// </summary>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="builder" /> is <see langword="null" />.
    /// </exception>
    public static IAtomUIBuilder UseAlibabaSansFont(this IAtomUIBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return PackageMetadata.Definition.Ensure(builder, static package => package
            .OnCommit(static context => context.Own(FontCollectionLifetime.Acquire(
                FontManager.Current, new Uri("fonts:AlibabaSans"), static () => new AlibabaSansFontCollection()))));
    }
}
