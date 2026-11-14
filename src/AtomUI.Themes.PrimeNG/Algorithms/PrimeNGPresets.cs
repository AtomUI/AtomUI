using AtomUI.Theme.Algorithms;

namespace AtomUI.Themes.PrimeNG.Algorithms;

/// <summary>
/// Finds the rule table of the PrimeNG preset a chain runs, for the control stages, which are shared by the light
/// and dark algorithm of a preset and learn which one runs only from the context.
/// </summary>
internal static class PrimeNGPresets
{
    internal const string Catalog = "PrimeNG";

    /// <summary>
    /// Returns the rule table of the last PrimeNG algorithm in the chain. A control stage is only called for a chain
    /// that contains one of the algorithms it is declared for.
    /// </summary>
    internal static PrimeNGPreset Resolve(in ThemeAlgorithmContext context)
    {
        var algorithms = context.Algorithms;
        for (var index = algorithms.Count - 1; index >= 0; index--)
        {
            var algorithm = algorithms[index];
            if (!string.Equals(algorithm.Catalog, Catalog, StringComparison.Ordinal))
            {
                continue;
            }
            switch (algorithm.Id)
            {
                case "Aura":
                case "Aura.Dark":
                    return PrimeNGAuraPreset.Table;
            }
        }

        throw new InvalidOperationException("The algorithm chain contains no PrimeNG preset algorithm.");
    }
}
