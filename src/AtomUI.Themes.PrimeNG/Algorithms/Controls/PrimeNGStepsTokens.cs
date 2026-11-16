using AtomUI.Desktop.Controls;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.DesignTokens;

namespace AtomUI.Themes.PrimeNG.Algorithms.Controls;

/// <summary>
/// Steps under the Nora presets: the finished step's check sits on the solid primary background
/// (<c>FinishIconBgColor</c> is <c>ColorPrimaryBg</c>, the primary 600 / 500 stop in Nora light / dark), so it takes the
/// contrast colour <c>ColorTextLightSolid</c> (PrimeUIX <c>steps.item.number.active.color</c>,
/// <c>primary.contrast.color</c>: white / <c>surface.950</c>) instead of the primary colour, which it would not be visible on. It writes only while the
/// selected background really is the solid primary (<see cref="PrimeNGPresets.HasSolidHighlight"/>): under a noir
/// primary or a mixed chain whose Map colours come from Ant Design the Ant Design value stays.
/// </summary>
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
        if (!PrimeNGPresets.HasSolidHighlight(effectiveGlobal))
        {
            return;
        }
        token.FinishIconColor = effectiveGlobal.ColorTextLightSolid;
    }
}
