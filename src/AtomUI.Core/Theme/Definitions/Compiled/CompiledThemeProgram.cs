using System.ComponentModel;
using AtomUI.Theme.Languages;
using AtomUI.Theme.Schema;
using Avalonia.Media;

namespace AtomUI.Theme.Definitions.Compiled;

/// <summary>
/// A theme definition compiled into its package (compiled theme ABI 4, spec 7.10): the definition's metadata and two
/// generated methods that declare and write every value with its type checked at build time. The runtime never parses
/// the XML.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public abstract class CompiledThemeProgram
{
    /// <summary>Creates an ABI 4 program (theme definition format v2).</summary>
    /// <param name="appearance">The fixed appearance as its numeric value, or -1 when open.</param>
    /// <param name="lockedSeeds">The seeds the theme locks.</param>
    protected CompiledThemeProgram(
        int abiVersion, string sourceIdentity, string sourceRevision, string contentDigest,
        string programDigest, string id, string name, string language, string? style, int appearance,
        string? density, string[] lockedSeeds, Color? accentColor)
    {
        if (abiVersion != 4)
        {
            throw new NotSupportedException(
                $"Compiled theme ABI '{abiVersion}' is not supported; rebuild the package against the current AtomUI.");
        }
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceIdentity);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceRevision);
        ArgumentException.ThrowIfNullOrWhiteSpace(contentDigest);
        ArgumentException.ThrowIfNullOrWhiteSpace(programDigest);
        SchemaIdentifier.Validate(id, nameof(id));
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        SchemaIdentifier.Validate(language, nameof(language));
        ArgumentNullException.ThrowIfNull(lockedSeeds);
        if (appearance is < -1 or > 1)
        {
            throw new ArgumentOutOfRangeException(nameof(appearance));
        }
        SourceIdentity = sourceIdentity;
        SourceRevision = sourceRevision;
        ContentDigest = contentDigest;
        ProgramDigest = programDigest;
        Id = id;
        Name = name;
        Language = language;
        Style = style is null ? null : new DesignStyle(style);
        Appearance = appearance < 0 ? null : (ThemeAppearance)appearance;
        Density = density is null ? null : new DesignDensity(density);
        LockedSeeds = lockedSeeds.ToHashSet(StringComparer.Ordinal);
        AccentColor = accentColor;
    }

    internal string SourceIdentity { get; }
    internal string SourceRevision { get; }
    internal string ContentDigest { get; }
    internal string ProgramDigest { get; }
    internal string Id { get; }
    internal string Name { get; }
    internal string Language { get; }
    internal DesignStyle? Style { get; }
    internal ThemeAppearance? Appearance { get; }
    internal DesignDensity? Density { get; }
    internal IReadOnlySet<string> LockedSeeds { get; }
    internal Color? AccentColor { get; }

    /// <summary>
    /// Opt in only when Describe and WriteStaticDefaults are safe on a worker thread and
    /// safe to run concurrently with a UI-thread fallback after a preparation timeout.
    /// Handwritten programs default to UI-thread preparation.
    /// </summary>
    protected virtual bool IsBackgroundPrepareSafe => false;
    internal bool SupportsBackgroundPrepare => IsBackgroundPrepareSafe;

    protected abstract void Describe(CompiledThemeBindingBuilder builder);
    protected abstract void WriteStaticDefaults(CompiledThemeValueWriter writer);

    /// <summary>Binds the program to <paramref name="registry"/>: the language, its axes, every token and control.</summary>
    internal PreparedThemeDefinition Bind(ThemeSchemaRegistry registry)
    {
        if (!registry.TryGetLanguage(Language, out var language))
        {
            throw new ThemeSchemaException($"Compiled theme '{Id}' belongs to design language '{Language}', which is not enabled.");
        }
        if (Style is { } style && !language.Axes.Styles.Contains(style))
        {
            throw new ThemeSchemaException($"Compiled theme '{Id}' fixes the style '{style}', which '{Language}' does not declare.");
        }
        if (Density is { } density && !language.Axes.Densities.Contains(density))
        {
            throw new ThemeSchemaException($"Compiled theme '{Id}' fixes the density '{density}', which '{Language}' does not declare.");
        }
        foreach (var seed in LockedSeeds)
        {
            if (!language.Axes.AdjustableSeeds.Any(adjustable => string.Equals(adjustable.Name, seed, StringComparison.Ordinal)))
            {
                throw new ThemeSchemaException($"Compiled theme '{Id}' locks '{seed}', which is not an adjustable seed of '{Language}'.");
            }
        }

        var builder = new CompiledThemeBindingBuilder(registry, language);
        CompiledThemeBinding binding;
        try
        {
            Describe(builder);
            binding = builder.Complete();
        }
        finally
        {
            builder.Close();
        }

        var location = new ThemeSourceLocation(SourceIdentity, 0, 0, "/Theme");
        return new PreparedThemeDefinition(Id, Name, language, Style, Appearance, Density, LockedSeeds, location,
            () => Write(binding, location));
    }

    private PreparedThemeValues Write(CompiledThemeBinding binding, ThemeSourceLocation location)
    {
        var writer = new CompiledThemeValueWriter(binding, location);
        try
        {
            WriteStaticDefaults(writer);
            return writer.Complete();
        }
        finally { writer.Close(); }
    }
}
