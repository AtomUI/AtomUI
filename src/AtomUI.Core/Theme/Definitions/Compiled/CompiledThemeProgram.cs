using System.ComponentModel;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.Schema;
using Avalonia.Media;

namespace AtomUI.Theme.Definitions.Compiled;

[EditorBrowsable(EditorBrowsableState.Never)]
public abstract class CompiledThemeProgram
{
    /// <summary>
    /// Creates an ABI 1 program. The design language metadata takes its defaults and the algorithm chain is empty.
    /// </summary>
    protected CompiledThemeProgram(
        int abiVersion, string sourceIdentity, string sourceRevision, string contentDigest,
        string programDigest, string id, string name, ThemeAppearance appearance, bool isDefault, Color? accentColor)
    {
        if (abiVersion != 1)
        {
            throw new NotSupportedException($"Compiled theme ABI '{abiVersion}' is not supported.");
        }
        Validate(sourceIdentity, sourceRevision, contentDigest, programDigest, id, name, appearance);
        SourceIdentity = sourceIdentity;
        SourceRevision = sourceRevision;
        ContentDigest = contentDigest;
        ProgramDigest = programDigest;
        Info = new ThemeInfo(id, name, appearance, isDefault, accentColor);
    }

    /// <summary>
    /// Creates an ABI 2 program that also publishes the design language metadata and the algorithm chain.
    /// </summary>
    /// <param name="algorithms">The algorithm chain, each element formatted as <c>Catalog:Id</c>.</param>
    protected CompiledThemeProgram(
        int abiVersion, string sourceIdentity, string sourceRevision, string contentDigest,
        string programDigest, string id, string name, ThemeAppearance appearance, bool isDefault, Color? accentColor,
        string? designLanguage, ThemeDensity density, ThemeBrandColorSupport brandColor, string[] algorithms)
    {
        if (abiVersion != 2)
        {
            throw new NotSupportedException($"Compiled theme ABI '{abiVersion}' is not supported.");
        }
        Validate(sourceIdentity, sourceRevision, contentDigest, programDigest, id, name, appearance);
        ArgumentNullException.ThrowIfNull(algorithms);
        if (designLanguage is not null)
        {
            SchemaIdentifier.Validate(designLanguage, nameof(designLanguage));
        }
        if (!Enum.IsDefined(density))
        {
            throw new ArgumentOutOfRangeException(nameof(density));
        }
        if (!Enum.IsDefined(brandColor))
        {
            throw new ArgumentOutOfRangeException(nameof(brandColor));
        }
        var chain = new ThemeAlgorithm[algorithms.Length];
        for (var i = 0; i < chain.Length; i++)
        {
            var text = algorithms[i] ?? throw new ArgumentNullException(nameof(algorithms), "Algorithm identities must not be null.");
            var separator = text.IndexOf(':');
            if (separator <= 0 || separator == text.Length - 1)
            {
                throw new ArgumentException($"Algorithm identity '{text}' must be formatted as 'Catalog:Id'.", nameof(algorithms));
            }
            chain[i] = new ThemeAlgorithm(text[..separator], text[(separator + 1)..]);
        }
        SourceIdentity = sourceIdentity;
        SourceRevision = sourceRevision;
        ContentDigest = contentDigest;
        ProgramDigest = programDigest;
        Info = new ThemeInfo(id, name, appearance, isDefault, accentColor)
        {
            DesignLanguage = designLanguage,
            Density = density,
            BrandColor = brandColor,
            Algorithms = chain
        };
    }

    private static void Validate(string sourceIdentity, string sourceRevision, string contentDigest, string programDigest,
        string id, string name, ThemeAppearance appearance)
    {
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
