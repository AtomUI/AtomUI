namespace AtomUI.Theme.Definitions;

/// <summary>
/// Describes the theme catalog load that asks a resolver for theme definition sources.
/// </summary>
public sealed class ThemeDefinitionResolveContext
{
    /// <summary>
    /// Creates the context of one theme catalog load.
    /// </summary>
    /// <param name="applicationId">
    /// The application identity, which names the folder of the application under
    /// <paramref name="applicationDataRoot" />.
    /// </param>
    /// <param name="applicationDataRoot">
    /// The root folder of per-application data. It may be blank when the platform has no such folder.
    /// </param>
    /// <param name="isReload">
    /// Whether the load is a catalog reload rather than the startup load.
    /// </param>
    /// <param name="generation">
    /// The generation of the reload, or <c>0</c> for the startup load.
    /// </param>
    /// <exception cref="ArgumentException">
    /// <paramref name="applicationId" /> is blank.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="applicationDataRoot" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="generation" /> is negative.
    /// </exception>
    public ThemeDefinitionResolveContext(
        string applicationId,
        string applicationDataRoot,
        bool isReload,
        long generation)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(applicationId);
        ArgumentNullException.ThrowIfNull(applicationDataRoot);
        ArgumentOutOfRangeException.ThrowIfNegative(generation);

        ApplicationId       = applicationId;
        ApplicationDataRoot = applicationDataRoot;
        IsReload            = isReload;
        Generation          = generation;
    }

    /// <summary>
    /// The application identity, which names the folder of the application under <see cref="ApplicationDataRoot" />.
    /// </summary>
    public string ApplicationId { get; }
    /// <summary>
    /// The root folder of per-application data. It may be blank when the platform has no such folder.
    /// </summary>
    public string ApplicationDataRoot { get; }
    /// <summary>
    /// Whether the load is a catalog reload rather than the startup load.
    /// </summary>
    public bool IsReload { get; }
    /// <summary>
    /// The generation of the reload, or <c>0</c> for the startup load. The generation of a reload is the one reported
    /// by <see cref="ThemeCatalogChangedEventArgs" /> and <see cref="ThemeCatalogReloadResult" />.
    /// </summary>
    public long Generation { get; }
}
