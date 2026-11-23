using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;
using AtomUI.Theme.DesignTokens;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>
/// CheckBox under the PrimeNG presets: PrimeUIX 3.1.1 <c>checkbox.width</c> / <c>checkbox.icon.size</c> (Aura and Nora
/// 18 / 12, Lara 20 / 14). The radius is the global <c>BorderRadiusSM</c> already (PrimeUIX <c>border.radius.sm</c>;
/// Nora <c>border.radius.xs</c>, which its size ladder makes equal). Under a noir primary the check mark is the
/// contrast colour <c>ColorTextLightSolid</c>.
/// </summary>
internal static class PrimeNGCheckBoxTokens
{
    internal static void EvaluateGlobal(in DerivationContext<PrimeNGToken> context, DesignToken controlGlobal)
    {
        // The check mark is the CheckBox's own ColorWhite. PrimeUIX checkbox.icon.checked.color is
        // primary.contrast.color: under a noir primary it takes the contrast colour, so it stays visible on the dark
        // appearance's near-white primary. A coloured primary keeps the white mark.
        if (context.Language.GetVisualRecipes(controlGlobal, context.Variant).IsNoirPrimary)
        {
            controlGlobal.ColorWhite = controlGlobal.ColorTextLightSolid;
        }
    }
    internal static void EvaluateOwn(in DerivationContext<PrimeNGToken> context, DesignToken effectiveGlobal, CheckBoxToken token)
    {
        var recipes = context.Language.GetVisualRecipes(effectiveGlobal, context.Variant);
        token.CheckIndicatorSize = recipes.CheckIndicatorSize;
        token.CheckedMarkSize    = recipes.CheckedMarkSize;
        // root.hover.border.color is form.field.hover.border.color.
        token.IndicatorHoverBorderColor = PrimeNGInputStage.HoverBorder(context, effectiveGlobal);
    }
}
