using System.ComponentModel;

namespace AtomUI.Theme.Definitions.Compiled;

/// <summary>
/// The compiled theme definitions of one package, registered together under one id through
/// <see cref="IThemeManagerBuilder.AddCompiledThemePackage" />. The generated <c>CreatePackage</c> method of a
/// package creates it.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class CompiledThemePackage
{
    /// <summary>
    /// Creates a package that holds a copy of <paramref name="programs" />.
    /// </summary>
    /// <param name="id">
    /// The package's registration id. Registering a second package or a resolver with the same id throws
    /// <see cref="ThemeResourceRegisterException" />.
    /// </param>
    /// <param name="programs">
    /// The theme definitions of the package. Their theme ids and source identities must be unique within the package.
    /// </param>
    public CompiledThemePackage(string id, IReadOnlyList<CompiledThemeProgram> programs)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentNullException.ThrowIfNull(programs);
        if (programs.Count > ThemeDocumentReaderOptions.DefaultMaxElements)
        {
            throw new ArgumentOutOfRangeException(nameof(programs));
        }
        var copy = programs.ToArray();
        var ids = new HashSet<string>(StringComparer.Ordinal);
        var sources = new HashSet<string>(StringComparer.Ordinal);
        foreach (var program in copy)
        {
            ArgumentNullException.ThrowIfNull(program);
            if (!ids.Add(program.Id) || !sources.Add(program.SourceIdentity))
            {
                throw new ArgumentException("Compiled package theme and source identities must be unique.", nameof(programs));
            }
        }
        Id = id;
        Programs = Array.AsReadOnly(copy);
    }

    internal string Id { get; }
    internal IReadOnlyList<CompiledThemeProgram> Programs { get; }
}
