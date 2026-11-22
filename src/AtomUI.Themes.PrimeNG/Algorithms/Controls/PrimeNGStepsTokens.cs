using AtomUI.Desktop.Controls;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.DesignTokens;

namespace AtomUI.Themes.PrimeNG.Algorithms.Controls;

/// <summary>
/// Steps under a solid highlight (Nora, or noir in any preset): the finished step's check sits on the solid primary
/// background (<c>FinishIconBgColor</c> is <c>ControlItemBgActive</c>, the primary colour there), so it takes the
/// contrast colour <c>ColorTextLightSolid</c> (PrimeUIX <c>steps.item.number.active.color</c>,
/// <c>primary.contrast.color</c>: white / <c>surface.950</c>) instead of the primary colour, which it would not be visible on. It writes only while the
/// selected background really is the solid primary (<see cref="PrimeNGPresets.HasSolidHighlight"/>: Nora, and every
/// preset under a noir primary); under Aura / Lara's pale highlight or a mixed chain whose Map colours come from Ant
/// Design the Ant Design value stays.
/// </summary>
[ControlTokenAlgorithm("PrimeNG", "Aura", 1)]
[ControlTokenAlgorithm("PrimeNG", "Aura.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora.Dark", 1)]
internal sealed class PrimeNGStepsTokens : IControlTokenAlgorithm<StepsToken>
{
    /// <inheritdoc />
    public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal)
    {
    }

    /// <inheritdoc />
    public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, StepsToken token)
    {
        // PrimeUIX steps / stepper: step.number.size 2rem round, number.font.size 1rem; waiting numbers are
        // number.background (content.background) with a content.border.color border and text.muted text; the
        // separator is separator.background (content.border.color); the active title is the primary colour. Aura's
        // active number is also an outlined disc (number.active.background content.background, primary text); Lara and
        // Nora fill it with the primary colour.
        var border = PrimeNGPresets.ContentBorder(context);
        token.IconContainerSize         = 32;
        token.IconContainerCornerRadius = new Avalonia.CornerRadius(16);
        token.IconFontSize              = 16;
        token.WaitIconBgColor           = effectiveGlobal.ColorBgElevated;
        token.WaitIconBorderColor       = border;
        token.WaitIconColor             = effectiveGlobal.ColorTextSecondary;
        token.WaitTailColor             = border;
        token.ProcessTitleColor         = effectiveGlobal.ColorPrimary;
        // item.label.font.weight 500 (Nora 700).
        token.TitleFontWeight           = PrimeNGPresets.KindOf(context) == PrimeNGPresetKind.Nora
            ? Avalonia.Media.FontWeight.Bold
            : Avalonia.Media.FontWeight.Medium;
        if (PrimeNGPresets.KindOf(context) == PrimeNGPresetKind.Aura)
        {
            token.ProcessIconBgColor     = effectiveGlobal.ColorBgElevated;
            token.ProcessIconBorderColor = border;
            token.ProcessIconColor       = effectiveGlobal.ColorPrimary;
        }
        if (!PrimeNGPresets.HasSolidHighlight(effectiveGlobal))
        {
            return;
        }
        token.FinishIconColor = effectiveGlobal.ColorTextLightSolid;
    }
}
