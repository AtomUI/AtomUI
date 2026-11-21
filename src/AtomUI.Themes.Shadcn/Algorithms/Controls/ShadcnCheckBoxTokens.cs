using AtomUI.Desktop.Controls;
using AtomUI.Theme;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.DesignTokens;
using Avalonia;

namespace AtomUI.Themes.Shadcn.Algorithms.Controls;

/// <summary>
/// The checkbox radius of the shadcn styles. The indicator is a 16px square (~16 dome), and the styles give it their
/// own small radius (<c>rounded-[4px]</c> for Vega/Nova/Mira, <c>[5px]</c> for Luma/Rhea, <c>[6px]</c> for Maia,
/// <c>none</c> for Lyra/Sera — appendix C). Without this stage the indicator falls back to the button radius ladder,
/// so a 26px button radius collapses the 16px box into a circle.
/// </summary>
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Nova", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Nova.Dark", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Vega", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Vega.Dark", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Maia", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Maia.Dark", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Lyra", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Lyra.Dark", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Mira", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Mira.Dark", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Luma", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Luma.Dark", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Sera", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Sera.Dark", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Rhea", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Rhea.Dark", 1)]
internal sealed class ShadcnCheckBoxTokens : IControlTokenAlgorithm<CheckBoxToken>
{
    /// <inheritdoc />
    public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal)
    {
        var style = ShadcnStyles.Resolve(context);
        // The indicator's template binds CornerRadius to the control-scope BorderRadiusSM.
        controlGlobal.BorderRadiusSM = new CornerRadius(style.CheckBoxRadius);

        if (context.Appearance == ThemeAppearance.Dark)
        {
            // The indicator uses this control-scoped token for the check mark.
            controlGlobal.ColorWhite = controlGlobal.ColorTextLightSolid;
        }
    }

    /// <inheritdoc />
    public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, CheckBoxToken token)
    {
    }
}
