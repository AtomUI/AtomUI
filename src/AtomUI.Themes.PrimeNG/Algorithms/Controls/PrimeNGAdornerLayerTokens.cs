using AtomUI.Desktop.Controls;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.DesignTokens;

namespace AtomUI.Themes.PrimeNG.Algorithms.Controls;

/// <summary>
/// The keyboard focus outline under the PrimeNG presets: PrimeUIX <c>focus.ring.offset</c> (Aura and Nora 2px; Lara
/// draws its <c>focus.ring.shadow</c> glow on the element edge). The ring width and colour are the global
/// <c>LineWidthFocus</c> / <c>ColorFocusBorder</c> the preset alias stage writes.
/// </summary>
[ControlTokenAlgorithm("PrimeNG", "Aura", 1)]
[ControlTokenAlgorithm("PrimeNG", "Aura.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora.Dark", 1)]
internal sealed class PrimeNGAdornerLayerTokens : IControlTokenAlgorithm<AdornerLayerToken>
{
    /// <inheritdoc />
    public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal)
    {
    }

    /// <inheritdoc />
    public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, AdornerLayerToken token)
    {
        token.FocusOutlineOffset = effectiveGlobal.LineWidth * PrimeNGPresets.Resolve(context).Alias.FocusRingOffsetLineWidths;
    }
}
