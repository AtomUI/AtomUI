using AtomUI.Theme.Languages;
using Avalonia.Media;

namespace AtomUI.Theme;

/// <summary>
/// A named theme of the catalog (spec 7.8): its language, the axes it fixes, the seeds it locks, its declared primary
/// colour. Every enabled language also has an implicit entry for its default appearance
/// (<see cref="IsLanguageDefault"/>), so a language is selectable without any theme file.
/// </summary>
public sealed record ThemeInfo(string Id, string Name, string Language)
{
    private static readonly IReadOnlyDictionary<string, string> s_noSeedValues =
        new SortedDictionary<string, string>(StringComparer.Ordinal);
    private readonly IReadOnlyList<string> _lockedSeeds = Array.Empty<string>();
    private IReadOnlyDictionary<string, string> _seedValues = s_noSeedValues;

    /// <summary>The style the theme fixes, or <see langword="null"/> when the style is open.</summary>
    public DesignStyle? Style { get; init; }

    /// <summary>The appearance the theme fixes, or <see langword="null"/> when it is open.</summary>
    public ThemeAppearance? Appearance { get; init; }

    /// <summary>The density the theme fixes, or <see langword="null"/> when it is open.</summary>
    public DesignDensity? Density { get; init; }

    /// <summary>The adjustable seeds the theme locks; the end user's selection does not replace them.</summary>
    public IReadOnlyList<string> LockedSeeds
    {
        get => _lockedSeeds;
        init
        {
            ArgumentNullException.ThrowIfNull(value);
            _lockedSeeds = value.Count == 0 ? Array.Empty<string>() : value.Order(StringComparer.Ordinal).ToArray();
        }
    }

    /// <summary>
    /// The theme's own <c>ColorPrimary</c> seed, if it declares one; for the implicit entry of a language, the seed the
    /// language gives in its default variant.
    /// </summary>
    public Color? AccentColor { get; init; }

    /// <summary>
    /// The values the theme gives its language's adjustable seeds (global and language seeds), by seed name and in the
    /// canonical token text; for the implicit entry of a language, the seeds the language gives in its default variant.
    /// A picker uses them to tell the theme's own value from a user's choice.
    /// </summary>
    public IReadOnlyDictionary<string, string> SeedValues
    {
        get => _seedValues;
        init
        {
            ArgumentNullException.ThrowIfNull(value);
            _seedValues = value.Count == 0
                ? s_noSeedValues
                : new SortedDictionary<string, string>(value.ToDictionary(), StringComparer.Ordinal);
        }
    }

    /// <summary>Whether this is the implicit entry of the language's default appearance (no theme file).</summary>
    public bool IsLanguageDefault { get; init; }

    /// <summary>The identity of the implicit entry of <paramref name="language"/>.</summary>
    public static string LanguageDefaultId(string language) => "$" + language;

    public bool Equals(ThemeInfo? other)
    {
        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return other is not null &&
               Id == other.Id &&
               Name == other.Name &&
               Language == other.Language &&
               Style == other.Style &&
               Appearance == other.Appearance &&
               Density == other.Density &&
               AccentColor == other.AccentColor &&
               SeedValues.SequenceEqual(other.SeedValues) &&
               IsLanguageDefault == other.IsLanguageDefault &&
               LockedSeeds.SequenceEqual(other.LockedSeeds);
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Id);
        hash.Add(Name);
        hash.Add(Language);
        hash.Add(Style);
        hash.Add(Appearance);
        hash.Add(Density);
        hash.Add(AccentColor);
        foreach (var (name, value) in SeedValues)
        {
            hash.Add(name);
            hash.Add(value);
        }
        hash.Add(IsLanguageDefault);
        foreach (var seed in LockedSeeds)
        {
            hash.Add(seed);
        }

        return hash.ToHashCode();
    }
}

/// <summary>An enabled design language and what it offers: its axes and the seeds an application may adjust.</summary>
public sealed record DesignLanguageInfo(
    string Id,
    IReadOnlyList<DesignStyle> Styles,
    DesignStyle DefaultStyle,
    IReadOnlyList<DesignDensity> Densities,
    IReadOnlyList<DesignIdiom> Idioms,
    IReadOnlyList<string> AdjustableSeeds);
