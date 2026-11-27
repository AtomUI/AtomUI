using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;
using AtomUI.Theme.DesignTokens;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>
/// RadioButton under the PrimeNG presets. Nora keeps the field background when checked and draws the checked dot
/// (<c>RadioColor</c>, white in Ant Design) in the primary color. Under a noir primary Aura and Lara give the
/// checked dot the contrast color <c>ColorTextLightSolid</c>, PrimeUIX <c>radiobutton.icon.checked.color</c>
/// (<c>primary.contrast.color</c>), so it stays visible on the dark appearance's near-white primary; with a coloured
/// primary they write no color and keep the white dot.
/// </summary>
internal static class PrimeNGRadioButtonTokens
{
    internal static void EvaluateGlobal(in TokenEvaluationContext<PrimeNGToken> context, DesignToken controlGlobal)
    {
    }
    internal static void EvaluateOwn(in TokenEvaluationContext<PrimeNGToken> context, DesignToken effectiveGlobal, RadioButtonToken token)
    {
        // PrimeUIX radiobutton.width 1.125rem with icon.size 0.625rem (Lara 1.25rem / 0.875rem). Nora keeps the field
        // background when checked and draws the dot in the primary color (checked.background form.field.background,
        // icon.checked.color primary.color).
        var kind = context.Variant.Style;
        token.RadioSize  = kind == PrimeNGLanguage.Lara ? 20 : 18;
        token.DotSize    = kind == PrimeNGLanguage.Lara ? 14 : 10;
        token.DotPadding = (token.RadioSize - token.DotSize) / 2;
        token.IndicatorHoverBorderColor = PrimeNGInputStage.HoverBorder(context, effectiveGlobal);
        if (kind == PrimeNGLanguage.Nora)
        {
            token.RadioBgColor = effectiveGlobal.ColorBgContainer;
            token.RadioColor   = effectiveGlobal.ColorPrimary;
            return;
        }
        // Noir in Aura and Lara: radiobutton.icon.checked.color is primary.contrast.color, which stays visible on the
        // dark appearance's near-white primary.
        if (context.Language.GetVisualRecipes(effectiveGlobal, context.Variant).IsNoirPrimary)
        {
            token.RadioColor = effectiveGlobal.ColorTextLightSolid;
        }
    }
}
