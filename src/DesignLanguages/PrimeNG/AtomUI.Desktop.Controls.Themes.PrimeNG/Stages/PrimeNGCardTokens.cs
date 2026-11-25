using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;
using AtomUI.Theme;
using AtomUI.Theme.DesignTokens;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>
/// Card under the PrimeNG presets: the card surface is PrimeUIX <c>content.background</c> (the elevated
/// background: white, dark neutral 900), the radius <c>border.radius.xl</c> and the PrimeUIX <c>card.shadow</c>, plus
/// PrimeUIX 3.1.1 <c>card.body.padding</c> (Aura and Nora 18, Lara 20). In dark the header takes the card surface too. A preset whose card border differs from the global secondary
/// border (Nora dark: neutral 700 where the global one is transparent) sets it for the card only.
/// </summary>
internal static class PrimeNGCardTokens
{
    internal static void EvaluateGlobal(in TokenEvaluationContext<PrimeNGToken> context, DesignToken controlGlobal)
    {
        var card = context.Language.GetVisualRecipes(controlGlobal, context.Variant);
        controlGlobal.ColorBgContainer = controlGlobal.ColorBgElevated;
        controlGlobal.BorderRadiusLG   = card.CardCornerRadius;
        if (card.CardBorderColor is { } border)
        {
            // Deliberately a different stop than the global ColorBorderSecondary (Nora dark: neutral 700, not transparent).
            controlGlobal.ColorBorderSecondary = border;
        }
    }
    internal static void EvaluateOwn(in TokenEvaluationContext<PrimeNGToken> context, DesignToken effectiveGlobal, CardToken token)
    {
        var card = context.Language.GetVisualRecipes(effectiveGlobal, context.Variant);
        token.BodyPadding    = new Thickness(card.CardBodyPadding);
        token.RestingShadows = card.CardShadows;
        if (context.Variant.Appearance == ThemeAppearance.Dark)
        {
            token.HeaderBg = effectiveGlobal.ColorBgContainer;
        }
    }
}
