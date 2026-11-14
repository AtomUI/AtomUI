using AtomUI.Desktop.Controls;
using AtomUI.Theme;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.DesignTokens;
using Avalonia;

namespace AtomUI.Themes.PrimeNG.Algorithms.Controls;

/// <summary>
/// Card under the PrimeNG presets: the card surface is PrimeUIX <c>content.background</c> (the elevated
/// background: white, dark neutral 900), the radius <c>border.radius.xl</c> and the PrimeUIX <c>card.shadow</c>, plus
/// the body padding of the baseline XML (20, the <c>aura-compat</c> value). In dark the header takes the card
/// surface too.
/// </summary>
[ControlTokenAlgorithm("PrimeNG", "Aura", 1)]
[ControlTokenAlgorithm("PrimeNG", "Aura.Dark", 1)]
internal sealed class PrimeNGCardTokens : IControlTokenAlgorithm<CardToken>
{
    /// <inheritdoc />
    public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal)
    {
        var sizes = PrimeNGPresets.Resolve(context).Sizes;
        controlGlobal.ColorBgContainer = controlGlobal.ColorBgElevated;
        controlGlobal.BorderRadiusLG   = PrimeNGOverlayRadius.Of(controlGlobal, sizes);
    }

    /// <inheritdoc />
    public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, CardToken token)
    {
        var controls = PrimeNGPresets.Resolve(context).Controls;
        token.BodyPadding    = new Thickness(controls.CardBodyPadding);
        token.RestingShadows = controls.CardRestingShadows;
        if (context.Appearance == ThemeAppearance.Dark)
        {
            token.HeaderBg = effectiveGlobal.ColorBgContainer;
        }
    }
}
