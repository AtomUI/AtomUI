using AtomUI.Desktop.Controls;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.DesignTokens;

namespace AtomUI.Themes.PrimeNG.Algorithms.Controls;

/// <summary>
/// RadioButton under the PrimeNG presets with a noir primary: the checked dot (<c>RadioColor</c>, white in Ant
/// Design) takes the contrast colour <c>ColorTextLightSolid</c>, PrimeUIX <c>radiobutton.icon.checked.color</c>
/// (<c>primary.contrast.color</c>), so it stays visible on the dark appearance's near-white primary. A coloured
/// primary writes nothing and keeps the white dot.
/// </summary>
[ControlTokenAlgorithm("PrimeNG", "Aura", 1)]
[ControlTokenAlgorithm("PrimeNG", "Aura.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora.Dark", 1)]
internal sealed class PrimeNGRadioButtonTokens : IControlTokenAlgorithm<RadioButtonToken>
{
    /// <inheritdoc />
    public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal)
    {
    }

    /// <inheritdoc />
    public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, RadioButtonToken token)
    {
        // PrimeUIX radiobutton.width 1.125rem with icon.size 0.625rem (Lara 1.25rem / 0.875rem). Nora keeps the field
        // background when checked and draws the dot in the primary colour (checked.background form.field.background,
        // icon.checked.color primary.color).
        var kind = PrimeNGPresets.KindOf(context);
        token.RadioSize  = kind == PrimeNGPresetKind.Lara ? 20 : 18;
        token.DotSize    = kind == PrimeNGPresetKind.Lara ? 14 : 10;
        token.DotPadding = (token.RadioSize - token.DotSize) / 2;
        token.IndicatorHoverBorderColor = PrimeNGInputStage.HoverBorder(context, effectiveGlobal);
        if (kind == PrimeNGPresetKind.Nora)
        {
            token.RadioBgColor = effectiveGlobal.ColorBgContainer;
            token.RadioColor   = effectiveGlobal.ColorPrimary;
            return;
        }
        if (PrimeNGNoir.IsActive(context, effectiveGlobal))
        {
            token.RadioColor = effectiveGlobal.ColorTextLightSolid;
        }
    }
}
