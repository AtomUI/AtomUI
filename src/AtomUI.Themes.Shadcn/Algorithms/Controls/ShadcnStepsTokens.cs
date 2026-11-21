using AtomUI.Desktop.Controls;
using AtomUI.Theme;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.DesignTokens;
using Avalonia;

namespace AtomUI.Themes.Shadcn.Algorithms.Controls;

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
internal sealed class ShadcnStepsTokens : IControlTokenAlgorithm<StepsToken>
{
    private const double MaximumPanelRadius = 8;

    public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal)
    {
    }

    public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, StepsToken token)
    {
        var radius      = Math.Min(effectiveGlobal.BorderRadius.TopLeft, MaximumPanelRadius);
        var smallRadius = Math.Min(effectiveGlobal.BorderRadiusSM.TopLeft, MaximumPanelRadius);

        token.PanelFirstCornerRadius   = new CornerRadius(radius, 0, 0, radius);
        token.PanelLastCornerRadius    = new CornerRadius(0, radius, radius, 0);
        token.PanelFirstCornerRadiusSM = new CornerRadius(smallRadius, 0, 0, smallRadius);
        token.PanelLastCornerRadiusSM  = new CornerRadius(0, smallRadius, smallRadius, 0);

        if (context.Appearance == ThemeAppearance.Dark)
        {
            // A filled panel colors the whole step. Use a tint with its own text color for that larger surface.
            var currentBackground = ShadcnBaseColors.CompositeOver(effectiveGlobal.ColorPrimary, 0.18, token.PanelWaitBackground);
            token.PanelWaitActiveBackground    = currentBackground;
            token.PanelProcessActiveBackground = currentBackground;
            token.PanelFinishActiveBackground  = currentBackground;
            token.PanelActiveTextColor         = effectiveGlobal.ColorText;
            token.PanelErrorActiveBackground   = ShadcnBaseColors.CompositeOver(effectiveGlobal.ColorError, 0.30, token.PanelErrorBackground);
            token.PanelErrorActiveTextColor    = effectiveGlobal.ColorText;
        }
    }
}
