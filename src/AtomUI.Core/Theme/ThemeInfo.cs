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
    /// Gets the definition's own algorithm chain.
    /// </summary>
    public IReadOnlyList<ThemeAlgorithm> Algorithms { get; init; } = [];

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
        foreach (var algorithm in Algorithms)
        {
            hash.Add(algorithm);
        }

        return hash.ToHashCode();
    }
}
