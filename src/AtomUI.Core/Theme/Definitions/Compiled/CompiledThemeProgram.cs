using System.ComponentModel;
using AtomUI.Theme.Schema;
using Avalonia.Media;

namespace AtomUI.Theme.Definitions.Compiled;

[EditorBrowsable(EditorBrowsableState.Never)]
public abstract class CompiledThemeProgram
{
    protected CompiledThemeProgram(
        int abiVersion, string sourceIdentity, string sourceRevision, string contentDigest,
        string programDigest, string id, string name, ThemeAppearance appearance, bool isDefault, Color? accentColor)
    {
        if (abiVersion != 1)
        {
            throw new NotSupportedException($"Compiled theme ABI '{abiVersion}' is not supported.");
        }
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceIdentity);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceRevision);
        ArgumentException.ThrowIfNullOrWhiteSpace(contentDigest);
        ArgumentException.ThrowIfNullOrWhiteSpace(programDigest);
        SchemaIdentifier.Validate(id, nameof(id));
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        if (!Enum.IsDefined(appearance))
        {
            throw new ArgumentOutOfRangeException(nameof(appearance));
        }
        SourceIdentity = sourceIdentity;
        SourceRevision = sourceRevision;
        ContentDigest = contentDigest;
        ProgramDigest = programDigest;
        Info = new ThemeInfo(id, name, appearance, isDefault, accentColor);
    }

    internal string SourceIdentity { get; }
    internal string SourceRevision { get; }
    internal string ContentDigest { get; }
    internal string ProgramDigest { get; }
    internal ThemeInfo Info { get; }

    protected abstract void Describe(CompiledThemeBindingBuilder builder);
    protected abstract void WriteStaticDefaults(CompiledThemeValueWriter writer);

    internal PreparedThemeDefinition Bind(ThemeSchemaRegistry registry)
    {
        var builder = new CompiledThemeBindingBuilder(registry);
        CompiledThemeBinding binding;
        try
        {
            Describe(builder);
            binding = builder.Complete(Info.Appearance);
        }
        finally
        {
            builder.Close();
        }
        var location = new ThemeSourceLocation(SourceIdentity, 0, 0, "/Theme");
        return new PreparedThemeDefinition(Info.Id, Info.Name, Info.Appearance, Info.Appearance, Info.IsDefault,
            binding.Algorithms, location, () =>
            {
                var writer = new CompiledThemeValueWriter(binding, location);
                try
                {
                    WriteStaticDefaults(writer);
                    return writer.Complete();
                }
                finally
                {
                    writer.Close();
                }
            });
    }
}
