using AtomUI.Theme.Algorithms;
using AtomUI.Theme.DesignTokens;

namespace AtomUI.Themes.PrimeNG.Algorithms;

/// <summary>
/// Finds the rule table of the PrimeNG preset a chain runs, for the control stages, which are shared by the light
/// and dark algorithm of a preset and learn which one runs only from the context.
/// </summary>
internal enum PrimeNGPresetKind
{
    Aura,
    Lara,
    Nora
}

internal static class PrimeNGPresets
{
    internal const string Catalog = "PrimeNG";

    /// <summary>
    /// Returns the rule table of the last PrimeNG algorithm in the chain. A control stage is only called for a chain
    /// that contains one of the algorithms it is declared for. A PrimeNG identity without a rule table here fails
    /// instead of falling back to another preset.
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
            return algorithm.Id switch
            {
                "Aura" or "Aura.Dark" => PrimeNGAuraPreset.Table,
                "Lara" or "Lara.Dark" => PrimeNGLaraPreset.Table,
                "Nora" or "Nora.Dark" => PrimeNGNoraPreset.Table,
                _ => throw new InvalidOperationException(
                    $"The PrimeNG algorithm '{algorithm.Id}' has no rule table for the control stages.")
            };
        }

        throw new InvalidOperationException("The algorithm chain contains no PrimeNG preset algorithm.");
    }

    /// <summary>
    /// Whether the selected background (<c>ControlItemBgActive</c>) is the solid primary colour, as in the Nora
    /// presets (PrimeUIX <c>highlight.background</c>). A control that draws the primary colour on the selected
    /// background compensates only then: a noir primary, or a hybrid chain whose Map colours come from Ant Design,
    /// has a tinted selected background.
    /// </summary>
    /// <summary>Which of the three preset families the chain's PrimeNG algorithm belongs to.</summary>
    internal static PrimeNGPresetKind KindOf(in ThemeAlgorithmContext context)
    {
        var preset = Resolve(context);
        if (ReferenceEquals(preset, PrimeNGLaraPreset.Table))
        {
            return PrimeNGPresetKind.Lara;
        }
        return ReferenceEquals(preset, PrimeNGNoraPreset.Table) ? PrimeNGPresetKind.Nora : PrimeNGPresetKind.Aura;
    }

    /// <summary>A surface stop (slate in light, zinc in dark) of the active appearance; 0 is white.</summary>
    internal static Avalonia.Media.Color Surface(in ThemeAlgorithmContext context, int stop)
    {
        var rules = Resolve(context).For(context.Appearance);
        return new PrimeNGShade(stop).Resolve(rules.Surface.AsSpan());
    }

    /// <summary>PrimeUIX <c>content.border.color</c>: Aura and Lara surface 200 / 700, Nora surface 400 / 500.</summary>
    internal static Avalonia.Media.Color ContentBorder(in ThemeAlgorithmContext context)
    {
        var rules = Resolve(context).For(context.Appearance);
        return rules.MenuBorder.Resolve(rules.Surface.AsSpan());
    }

    internal static bool HasSolidHighlight(DesignToken token) => token.ControlItemBgActive == token.ColorPrimary;

    /// <summary>
    /// The rules a control stage applies: the table of the chain's preset for the resolved appearance, with the noir
    /// rows when the control's primary colour is noir.
    /// </summary>
    internal static PrimeNGAppearanceRules Rules(in ThemeAlgorithmContext context, DesignToken token) =>
        PrimeNGNoir.Resolve(Resolve(context), context.Appearance, token);
}
