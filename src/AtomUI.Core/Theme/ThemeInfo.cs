using System.Collections.ObjectModel;
using AtomUI.Theme.Algorithms;
using Avalonia.Media;

namespace AtomUI.Theme;

public sealed record ThemeInfo(
    string Id,
    string Name,
    ThemeAppearance Appearance,
    bool IsDefault)
{
    public ThemeInfo(
        string id,
        string name,
        ThemeAppearance appearance,
        bool isDefault,
        Color? accentColor)
        : this(id, name, appearance, isDefault)
    {
        AccentColor = accentColor;
    }

    public Color? AccentColor { get; }

    /// <summary>
    /// Gets the design language identifier the definition belongs to, or <see langword="null"/> when it declares none.
    /// </summary>
    public string? DesignLanguage { get; init; }

    /// <summary>
    /// Gets the density the definition is authored for.
    /// </summary>
    public ThemeDensity Density { get; init; }

    /// <summary>
    /// Gets whether the brand color of the definition can be replaced.
    /// </summary>
    public ThemeBrandColorSupport BrandColor { get; init; }

    /// <summary>
    /// Gets whether the neutral color family of the definition can be replaced.
    /// </summary>
    public ThemeNeutralColorSupport NeutralColor { get; init; }

    /// <summary>
    /// Gets the definition's own <c>ColorNeutral</c> seed, or <see langword="null"/> when it declares none.
    /// </summary>
    public Color? NeutralAccentColor { get; init; }

    private readonly IReadOnlyList<ThemeAlgorithm> _algorithms = ReadOnlyCollection<ThemeAlgorithm>.Empty;

    /// <summary>
    /// Gets the definition's own algorithm chain. The assigned list is copied into a read-only snapshot.
    /// </summary>
    /// <exception cref="ArgumentNullException">The assigned list is <see langword="null"/>.</exception>
    public IReadOnlyList<ThemeAlgorithm> Algorithms
    {
        get => _algorithms;
        init
        {
            ArgumentNullException.ThrowIfNull(value);
            _algorithms = value.Count == 0
                ? ReadOnlyCollection<ThemeAlgorithm>.Empty
                : Array.AsReadOnly(value.ToArray());
        }
    }

    public bool Equals(ThemeInfo? other)
    {
        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return other is not null &&
               Id == other.Id &&
               Name == other.Name &&
               Appearance == other.Appearance &&
               IsDefault == other.IsDefault &&
               AccentColor == other.AccentColor &&
               DesignLanguage == other.DesignLanguage &&
               Density == other.Density &&
               BrandColor == other.BrandColor &&
               NeutralColor == other.NeutralColor &&
               NeutralAccentColor == other.NeutralAccentColor &&
               Algorithms.SequenceEqual(other.Algorithms);
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Id);
        hash.Add(Name);
        hash.Add(Appearance);
        hash.Add(IsDefault);
        hash.Add(AccentColor);
        hash.Add(DesignLanguage);
        hash.Add(Density);
        hash.Add(BrandColor);
        hash.Add(NeutralColor);
        hash.Add(NeutralAccentColor);
        foreach (var algorithm in Algorithms)
        {
            hash.Add(algorithm);
        }

        return hash.ToHashCode();
    }
}
