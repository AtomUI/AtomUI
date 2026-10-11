using System.Collections.ObjectModel;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.Schema;

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
        IReadOnlyDictionary<ControlTokenIdentity, ControlThemeConfig> controls)
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
    }

    public bool Inherit { get; }

    public IReadOnlyList<ThemeAlgorithm>? Algorithms { get; }

    public IReadOnlyDictionary<string, string> Tokens { get; }

    public IReadOnlyDictionary<ControlTokenIdentity, ControlThemeConfig> Controls { get; }
}
