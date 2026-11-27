using System.ComponentModel;
using AtomUI.Theme.Languages;
using AtomUI.Theme.Schema;
using Avalonia.Media;

namespace AtomUI.Theme.Definitions.Compiled;

/// <summary>
/// A theme definition compiled into its package (compiled theme ABI 4): the definition's metadata and two
/// generated methods that declare and write every value with its type checked at build time. The runtime never parses
/// the XML.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public abstract class CompiledThemeProgram
{
    /// <summary>
    /// Creates an ABI 4 program (theme definition format v2).
    /// </summary>
    /// <param name="abiVersion">
    /// The compiled theme ABI the program was generated for. It must be 4; any other value throws
    /// <see cref="NotSupportedException" />.
    /// </param>
    /// <param name="sourceIdentity">
    /// The identity of the theme definition source, such as its path. It must be unique in the package, and it names
    /// the source in the diagnostics of a failed binding.
    /// </param>
    /// <param name="sourceRevision">
    /// The revision of the source as it was reported when the program was generated.
    /// </param>
    /// <param name="contentDigest">
    /// The digest of the content of the source document.
    /// </param>
    /// <param name="programDigest">
    /// The digest of everything the generator wrote into the program: its metadata, its declarations and its values.
    /// </param>
    /// <param name="id">
    /// The theme's id, which must be a valid schema identifier and unique in the package.
    /// </param>
    /// <param name="name">
    /// The theme's display name.
    /// </param>
    /// <param name="language">
    /// The id of the design language the theme belongs to. The language must be enabled when the program is bound.
    /// </param>
    /// <param name="style">
    /// The style the theme is fixed to, which the language must declare, or <see langword="null" /> when the theme
    /// follows the selected style.
    /// </param>
    /// <param name="appearance">
    /// The fixed appearance as its numeric value: 0 is <see cref="ThemeAppearance.Light" /> and 1 is
    /// <see cref="ThemeAppearance.Dark" />. Pass -1 when the theme follows the selected appearance.
    /// </param>
    /// <param name="density">
    /// The density the theme is fixed to, which the language must declare, or <see langword="null" /> when the theme
    /// follows the selected density.
    /// </param>
    /// <param name="lockedSeeds">
    /// The adjustable seeds the theme locks, by name. Each name must be an adjustable seed of the language, and the
    /// user's selection cannot replace a locked seed.
    /// </param>
    /// <param name="accentColor">
    /// The theme's accent color: the value of the global <c>ColorPrimary</c> token of the base section when it is a
    /// <see cref="Color" />, or <see langword="null" /> when the base section has none. It is stored as given.
    /// </param>
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

    /// <summary>
    /// Declares the sections, controls and tokens the program writes. It is called once per binding, and the builder
    /// is closed when the call returns, so the builder must not be kept.
    /// </summary>
    /// <param name="builder">
    /// The builder that receives the declarations.
    /// </param>
    /// <remarks>
    /// The declarations must match what <see cref="WriteStaticDefaults" /> writes: a value is accepted only for a
    /// token declared with the same id, kind, control and section.
    /// </remarks>
    protected abstract void Describe(CompiledThemeBindingBuilder builder);
    /// <summary>
    /// Writes the value of every declared token. It runs when the values of the bound definition are read for the
    /// first time, and they are then kept for the lifetime of that definition. The writer is closed when the call
    /// returns, so the writer must not be kept.
    /// </summary>
    /// <param name="writer">
    /// The writer that receives the values. Each section must be opened with its
    /// <c>CompiledThemeValueWriter.BeginSection</c> before the values of that section are written.
    /// </param>
    /// <remarks>
    /// A value without a matching declaration, or a declared token left without a value, fails the completion of the
    /// values with <c>ThemeSchemaException</c>.
    /// </remarks>
    protected abstract void WriteStaticDefaults(CompiledThemeValueWriter writer);

    /// <summary>
    /// Binds the program to <paramref name="registry" />: the language, its axes, every token and control.
    /// </summary>
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
