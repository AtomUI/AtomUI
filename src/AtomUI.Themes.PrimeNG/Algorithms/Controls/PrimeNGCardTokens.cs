using AtomUI.Desktop.Controls;
using AtomUI.Theme;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.DesignTokens;
using Avalonia;

namespace AtomUI.Themes.PrimeNG.Algorithms.Controls;

/// <summary>
/// Card under the PrimeNG presets: the card surface is PrimeUIX <c>content.background</c> (the elevated
/// background: white, dark neutral 900), the radius <c>border.radius.xl</c> and the PrimeUIX <c>card.shadow</c>, plus
/// the body padding of the baseline XML (20 for all three presets; the <c>aura-compat</c> and <c>nora-compat</c>
/// value). In dark the header takes the card surface too. A preset whose card border differs from the global secondary
/// border (Nora dark: neutral 700 where the global one is transparent) sets it for the card only.
/// </summary>
[ControlTokenAlgorithm("PrimeNG", "Aura", 1)]
[ControlTokenAlgorithm("PrimeNG", "Aura.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora.Dark", 1)]
internal sealed class PrimeNGCardTokens : IControlTokenAlgorithm<CardToken>
{
    /// <inheritdoc />
    public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal)
    {
        var preset = PrimeNGPresets.Resolve(context);
        controlGlobal.ColorBgContainer = controlGlobal.ColorBgElevated;
        controlGlobal.BorderRadiusLG   = PrimeNGOverlayRadius.Of(controlGlobal, preset.Sizes);
        var rules = preset.For(context.Appearance);
        if (rules.CardBorderSecondary is { } border)
        {
            controlGlobal.ColorBorderSecondary = border.Resolve(rules.Surface.AsSpan());
        }
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
