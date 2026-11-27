using AtomUI.Theme.Schema;

namespace AtomUI.Theme.Languages;

/// <summary>
/// The values a design language offers on each axis, and the seeds an application may adjust in it.
/// Requesting a value the language does not declare fails when the configuration is committed.
/// </summary>
public sealed record DesignLanguageAxes(
    IReadOnlyList<DesignStyle> Styles,
    DesignStyle DefaultStyle,
    IReadOnlyList<DesignDensity> Densities,
    IReadOnlyList<DesignIdiom> Idioms,
    IReadOnlyList<TokenDescriptor> AdjustableSeeds)
{
    /// <summary>
    /// Optional: for an adjustable seed, the tokens derived from it. A theme that writes one of them without locking the
    /// seed gets a warning, because the value no longer follows the seed.
    /// </summary>
    public IReadOnlyDictionary<string, IReadOnlySet<string>> SeedDerivations { get; init; } =
        new Dictionary<string, IReadOnlySet<string>>(StringComparer.Ordinal);

    /// <summary>
    /// Whether <paramref name="variant" /> uses only values this language declares.
    /// </summary>
    public bool Supports(in DesignVariant variant) =>
        Styles.Contains(variant.Style) && Densities.Contains(variant.Density) && Idioms.Contains(variant.Idiom);
}
