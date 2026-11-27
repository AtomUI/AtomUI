namespace AtomUI.Theme.Definitions;

/// <summary>
/// Offers theme definition documents to the theme catalog.
/// </summary>
/// <remarks>
/// Add a resolver to the <c>IAtomUIBuilder</c> with <c>AddThemeDefinitionResolver</c>. The catalog asks it for
/// sources at startup, and again on every catalog reload when
/// <see cref="IThemeDefinitionResolver.SupportsReload" /> is <see langword="true" />.
/// </remarks>
public interface IThemeDefinitionResolver
{
    /// <summary>
    /// The identity of the resolver, which must not be blank and must be unique among the registered resolvers.
    /// </summary>
    string Id { get; }
    /// <summary>
    /// Whether every catalog reload asks this resolver for its sources again.
    /// </summary>
    /// <remarks>
    /// When <see langword="false" />, a reload reuses the part of the catalog that this resolver produced at startup.
    /// </remarks>
    bool SupportsReload { get; }

    /// <summary>
    /// Collects the theme definition sources this resolver currently offers.
    /// </summary>
    /// <param name="context">
    /// Describes the load in progress, including whether it is a reload.
    /// </param>
    /// <returns>
    /// The sources to read and the diagnostics reported while collecting them.
    /// </returns>
    /// <remarks>
    /// The catalog reads the sources only when <see cref="ThemeDefinitionResolveResult.Success" /> is
    /// <see langword="true" />. A failed result, a thrown exception or a <see langword="null" /> result fails the
    /// whole catalog load. A reload calls this method on a worker thread, so a resolver with
    /// <see cref="SupportsReload" /> set to <see langword="true" /> must be safe to call off the UI thread.
    /// </remarks>
    ThemeDefinitionResolveResult Resolve(ThemeDefinitionResolveContext context);
}

/// <summary>
/// Opts a resolver into startup background preparation. Implementations must make Resolve
/// and every returned source's OpenRead safe to call from a worker thread, including a
/// concurrent UI-thread fallback if preparation times out.
/// </summary>
public interface IBackgroundThemeDefinitionResolver : IThemeDefinitionResolver
{
}
