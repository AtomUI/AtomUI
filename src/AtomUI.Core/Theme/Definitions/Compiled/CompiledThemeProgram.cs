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

    /// <summary>
    /// Opt in only when Describe and WriteStaticDefaults are safe on a worker thread and
    /// safe to run concurrently with a UI-thread fallback after a preparation timeout.
    /// Handwritten programs default to UI-thread preparation.
    /// </summary>
    protected virtual bool IsBackgroundPrepareSafe => false;
    internal bool SupportsBackgroundPrepare => IsBackgroundPrepareSafe;

    protected abstract void Describe(CompiledThemeBindingBuilder builder);
    protected abstract void WriteStaticDefaults(CompiledThemeValueWriter writer);

    protected virtual void DescribeControl(int controlId, CompiledThemeBindingBuilder builder) => Describe(builder);
    protected virtual void WriteControlDefaults(int controlId, CompiledThemeValueWriter writer) => WriteStaticDefaults(writer);

    internal PreparedThemeDefinition Bind(ThemeSchemaRegistry registry)
    {
        var builder = new CompiledThemeBindingBuilder(registry, -1);
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
        var ids = binding.DeclaredControls.ToDictionary(pair => pair.Value, pair => pair.Key);
        return new PreparedThemeDefinition(Info.Id, Info.Name, Info.Appearance, Info.Appearance, Info.IsDefault,
            binding.Algorithms, location, () => Write(binding, location, null).Tokens, ids.Keys.ToArray(), identity =>
            {
                var controlBuilder = new CompiledThemeBindingBuilder(registry, ids[identity]);
                try
                {
                    DescribeControl(ids[identity], controlBuilder);
                    return Write(controlBuilder.Complete(Info.Appearance), location, ids[identity]).Controls.Single();
                }
                finally { controlBuilder.Close(); }
            }, ProgramDigest);
    }

    private PreparedThemeValues Write(CompiledThemeBinding binding, ThemeSourceLocation location, int? controlId)
    {
        var writer = new CompiledThemeValueWriter(binding, location);
        try
        {
            if (controlId is { } id) { WriteControlDefaults(id, writer); }
            else { WriteStaticDefaults(writer); }
            return writer.Complete();
        }
        finally { writer.Close(); }
    }
}
