using System.ComponentModel;

namespace AtomUI.Theme.Definitions.Compiled;

[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class CompiledThemePackage
{
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
            if (!ids.Add(program.Info.Id) || !sources.Add(program.SourceIdentity))
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
