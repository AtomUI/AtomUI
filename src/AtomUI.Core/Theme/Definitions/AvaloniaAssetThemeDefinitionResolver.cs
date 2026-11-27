using Avalonia.Platform;

namespace AtomUI.Theme.Definitions;

/// <summary>
/// A theme definition resolver whose documents are Avalonia assets, addressed by absolute <c>avares</c> URIs.
/// </summary>
/// <remarks>
/// Add it to the <c>IAtomUIBuilder</c> with <c>AddThemeDefinitionResolver</c>.
/// </remarks>
public sealed class AvaloniaAssetThemeDefinitionResolver : IThemeDefinitionResolver
{
    private readonly IReadOnlyList<IThemeDefinitionSource> _sources;

    /// <summary>
    /// Creates a resolver that offers one theme definition source per resource URI.
    /// </summary>
    /// <param name="id">
    /// The resolver identity, which must not be blank and must be unique among the registered resolvers.
    /// </param>
    /// <param name="resourceUris">
    /// The absolute <c>avares</c> URIs of the theme definition documents.
    /// </param>
    /// <param name="sourceRevision">
    /// The revision reported for every source.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="resourceUris" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="id" /> or <paramref name="sourceRevision" /> is blank, or a URI is <see langword="null" />,
    /// relative, or not an <c>avares</c> URI.
    /// </exception>
    public AvaloniaAssetThemeDefinitionResolver(
        string id,
        IReadOnlyList<Uri> resourceUris,
        string sourceRevision)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentNullException.ThrowIfNull(resourceUris);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceRevision);

        var sources = new IThemeDefinitionSource[resourceUris.Count];
        for (var index = 0; index < resourceUris.Count; index++)
        {
            var uri = resourceUris[index] ??
                      throw new ArgumentException("Resource URI cannot be null.", nameof(resourceUris));
            if (!uri.IsAbsoluteUri || !string.Equals(uri.Scheme, "avares", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException(
                    $"Theme resource URI '{uri}' must be an absolute avares URI.",
                    nameof(resourceUris));
            }
            sources[index] = new AvaloniaAssetThemeDefinitionSource(uri, sourceRevision);
        }

        Id       = id;
        _sources = Array.AsReadOnly(sources);
    }

    /// <inheritdoc />
    public string Id { get; }
    /// <summary>
    /// Always <see langword="false" />: assets compiled into an assembly do not change, so a catalog reload keeps the
    /// catalog entries that were loaded from them at startup.
    /// </summary>
    public bool SupportsReload => false;

    /// <summary>
    /// Returns one source for every resource URI given to the constructor, without diagnostics.
    /// </summary>
    /// <param name="context">
    /// Not read except to reject <see langword="null" />: the resolver offers the same sources for a startup load
    /// and for a reload.
    /// </param>
    /// <returns>
    /// The sources of this resolver and an empty diagnostic list.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="context" /> is <see langword="null" />.
    /// </exception>
    public ThemeDefinitionResolveResult Resolve(ThemeDefinitionResolveContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return new ThemeDefinitionResolveResult(_sources, Array.Empty<ThemeDiagnostic>());
    }

    private sealed class AvaloniaAssetThemeDefinitionSource(
        Uri uri,
        string revision) : IThemeDefinitionSource
    {
        public string SourceIdentity { get; } = uri.ToString();
        public string SourceRevision { get; } = revision;

        public Stream OpenRead() => AssetLoader.Open(uri);
    }
}
