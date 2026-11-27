namespace AtomUI.Theme.Definitions;

/// <summary>
/// The outcome of one theme definition resolve: the sources to read and the diagnostics reported while collecting
/// them.
/// </summary>
public sealed class ThemeDefinitionResolveResult
{
    /// <summary>
    /// Creates a result from copies of the given lists.
    /// </summary>
    /// <param name="sources">
    /// The theme definition sources to read.
    /// </param>
    /// <param name="diagnostics">
    /// The diagnostics reported while collecting the sources.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// An argument is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// A list contains a <see langword="null" /> entry.
    /// </exception>
    public ThemeDefinitionResolveResult(
        IReadOnlyList<IThemeDefinitionSource> sources,
        IReadOnlyList<ThemeDiagnostic> diagnostics)
    {
        ArgumentNullException.ThrowIfNull(sources);
        ArgumentNullException.ThrowIfNull(diagnostics);

        var sourceCopy = sources.ToArray();
        if (sourceCopy.Any(static source => source is null))
        {
            throw new ArgumentException("Theme definition sources cannot contain null entries.", nameof(sources));
        }

        var diagnosticCopy = diagnostics.ToArray();
        if (diagnosticCopy.Any(static diagnostic => diagnostic is null))
        {
            throw new ArgumentException("Theme diagnostics cannot contain null entries.", nameof(diagnostics));
        }

        Sources     = Array.AsReadOnly(sourceCopy);
        Diagnostics = Array.AsReadOnly(diagnosticCopy);
    }

    /// <summary>
    /// The theme definition sources to read, in list order.
    /// </summary>
    public IReadOnlyList<IThemeDefinitionSource> Sources { get; }
    /// <summary>
    /// The diagnostics reported while collecting the sources.
    /// </summary>
    public IReadOnlyList<ThemeDiagnostic> Diagnostics { get; }
    /// <summary>
    /// Whether <see cref="Diagnostics" /> holds no diagnostic of severity
    /// <see cref="ThemeDiagnosticSeverity.Error" />.
    /// </summary>
    /// <remarks>
    /// When <see langword="false" />, the catalog discards <see cref="Sources" /> and fails the load with the
    /// diagnostics. When <see langword="true" />, it reads the sources and carries the diagnostics on.
    /// </remarks>
    public bool Success => Diagnostics.All(static diagnostic =>
        diagnostic.Severity != ThemeDiagnosticSeverity.Error);
}
