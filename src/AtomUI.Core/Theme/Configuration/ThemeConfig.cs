using System.Collections.ObjectModel;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.Schema;
using AtomUI.Theme.Styling;
using AtomUI.Theme.Branding;

namespace AtomUI.Theme.Configuration;

public sealed class ThemeConfig
{
    internal static ThemeConfig Empty { get; } = new(
        true,
        null,
        new Dictionary<string, string>(StringComparer.Ordinal),
        new Dictionary<ControlTokenIdentity, ControlThemeConfig>());

    internal ThemeConfig(
        bool inherit,
        IReadOnlyList<ThemeAlgorithm>? algorithms,
        IReadOnlyDictionary<string, string> tokens,
        IReadOnlyDictionary<ControlTokenIdentity, ControlThemeConfig> controls,
        IReadOnlyList<ThemeStyleAsset>? styleAssets = null,
        ThemeBrandSelection? brandSelection = null,
        ThemeBrandRequirements? brandRequirements = null)
    {
        Inherit    = inherit;
        Algorithms = algorithms is null ? null : Array.AsReadOnly(algorithms.ToArray());
        Tokens     = new ReadOnlyDictionary<string, string>(
            new Dictionary<string, string>(tokens, StringComparer.Ordinal));
        // Builder maps already hold canonical owner evidence; copy their values without rebuilding it.
        var canonicalControls = controls as CanonicalControlTokenMap<ControlThemeConfig>;
        if (canonicalControls is null)
        {
            canonicalControls = new CanonicalControlTokenMap<ControlThemeConfig>();
            foreach (var entry in controls)
            {
                canonicalControls.Set(entry.Key, entry.Value);
            }
        }
        Controls = new ReadOnlyDictionary<ControlTokenIdentity, ControlThemeConfig>(
            canonicalControls.ToDictionary(static entry => entry.Key, static entry => entry.Value));
        var assets = styleAssets?.ToArray() ?? [];
        if (assets.Any(static asset => asset is null) ||
            assets.Select(static asset => asset.Id).Distinct(StringComparer.Ordinal).Count() != assets.Length)
        {
            throw new ArgumentException("A configuration requires distinct, non-null style assets.", nameof(styleAssets));
        }
        StyleAssets = Array.AsReadOnly(assets);
        BrandSelection = brandSelection;
        BrandRequirements = brandRequirements;
    }

    public bool Inherit { get; }

    public IReadOnlyList<ThemeAlgorithm>? Algorithms { get; }

    public IReadOnlyDictionary<string, string> Tokens { get; }

    public IReadOnlyDictionary<ControlTokenIdentity, ControlThemeConfig> Controls { get; }

    // Registered compiled assets only. The public registry/manifest boundary is defined in CT-09.
    internal IReadOnlyList<ThemeStyleAsset> StyleAssets { get; }
    internal ThemeBrandSelection? BrandSelection { get; }
    internal ThemeBrandRequirements? BrandRequirements { get; }
}
