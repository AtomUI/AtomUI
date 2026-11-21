using AtomUI.Desktop.Controls;
using AtomUI.Theme;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.DesignTokens;

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
internal sealed class ShadcnToggleSwitchTokens : IControlTokenAlgorithm<ToggleSwitchToken>
{
    public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal)
    {
        if (context.Appearance != ThemeAppearance.Dark)
        {
            return;
        }

        var surface = ShadcnBaseColors.Resolve(controlGlobal.ColorNeutral, dark: true);
        if (controlGlobal.ColorPrimary != surface.Primary)
        {
            return;
        }

        // The switch needs enough contrast for the thumb and optional content on the neutral checked track.
        controlGlobal.ColorPrimary        = ShadcnBaseColors.CompositeOver(surface.Primary, 0.45, surface.Background);
        controlGlobal.ColorPrimaryHover   = ShadcnBaseColors.CompositeOver(surface.Primary, 0.48, surface.Background);
        controlGlobal.ColorTextLightSolid = surface.Foreground;
    }

    public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, ToggleSwitchToken token)
    {
        token.EnableHandlePressStretch = false;

        var mutedText = effectiveGlobal.ColorTextSecondary;
        if (context.Appearance == ThemeAppearance.Dark)
        {
            var surface = effectiveGlobal.ColorBgContainer;
            token.TrackBg      = ShadcnBaseColors.CompositeOver(mutedText, 0.44, surface);
            token.TrackHoverBg = ShadcnBaseColors.CompositeOver(mutedText, 0.50, surface);
        }
        else
        {
            token.TrackBg      = mutedText;
            token.TrackHoverBg = ShadcnBaseColors.CompositeOver(effectiveGlobal.ColorText, 0.08, mutedText);
        }
    }
}
