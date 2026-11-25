using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;
using AtomUI.Theme.DesignTokens;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>
/// The keyboard focus outline under the PrimeNG presets: PrimeUIX <c>focus.ring.offset</c> (Aura and Nora 2px; Lara
/// draws its <c>focus.ring.shadow</c> glow on the element edge). The ring width and colour are the global
/// <c>LineWidthFocus</c> / <c>ColorFocusBorder</c> the preset alias derivation writes.
/// </summary>
internal static class PrimeNGAdornerLayerTokens
{
    internal static void EvaluateGlobal(in TokenEvaluationContext<PrimeNGToken> context, DesignToken controlGlobal)
    {
    }
    internal static void EvaluateOwn(in TokenEvaluationContext<PrimeNGToken> context, DesignToken effectiveGlobal, AdornerLayerToken token)
    {
        token.FocusOutlineOffset = context.Language.GetVisualRecipes(effectiveGlobal, context.Variant).FocusOutlineOffset;
    }
}
