using AtomUI.Desktop.Controls;
using AtomUI.Theme;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.DesignTokens;

namespace AtomUI.Themes.PrimeNG.Algorithms.Controls;

/// <summary>
/// Steps under the dark Nora preset: the finished step's check sits on the solid primary background
/// (<c>FinishIconBgColor</c> is <c>ColorPrimaryBg</c>, the primary 500 stop in Nora dark), so it takes the contrast
/// colour <c>ColorTextLightSolid</c> (PrimeUIX <c>steps.item.number.active.color</c>, <c>primary.contrast.color</c>,
/// <c>surface.950</c>) instead of the primary colour, which it would not be visible on. Under a light appearance (a
/// mixed chain that ends in a light algorithm) it writes nothing and the Ant Design value stays.
/// </summary>
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
        if (context.Appearance != ThemeAppearance.Dark)
        {
            return;
        }
        token.FinishIconColor = effectiveGlobal.ColorTextLightSolid;
    }
}
