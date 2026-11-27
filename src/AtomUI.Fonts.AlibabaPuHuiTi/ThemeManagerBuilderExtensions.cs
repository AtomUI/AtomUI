using AtomUI.Generated.AtomUIFontsAlibabaPuHuiTi;
using AtomUI.Media;
using Avalonia.Media;

namespace AtomUI.Fonts.AlibabaPuHuiTi;

/// <summary>
/// Extension methods that add the <see cref="AlibabaPuHuiTiFontCollection" /> fonts to an
/// <see cref="IAtomUIBuilder" />.
/// </summary>
public static class AlibabaPuHuiTiThemeManagerBuilderExtensions
{
    /// <summary>
    /// Ensures the Alibaba PuHuiTi font package is part of the AtomUI build. When the package is committed, the
    /// Alibaba PuHuiTi font collection is acquired for <see cref="FontManager.Current" /> under the key
    /// <c>fonts:AlibabaPuHuiTi</c>, and the lease on it is transferred to the package's application lifetime.
    /// </summary>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="builder" /> is <see langword="null" />.
    /// </exception>
    public static IAtomUIBuilder UseAlibabaPuHuiTiFont(this IAtomUIBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return PackageMetadata.Definition.Ensure(builder, static package => package
            .OnCommit(static context => context.Own(FontCollectionLifetime.Acquire(
                FontManager.Current, new Uri("fonts:AlibabaPuHuiTi"), static () => new AlibabaPuHuiTiFontCollection()))));
    }
}
