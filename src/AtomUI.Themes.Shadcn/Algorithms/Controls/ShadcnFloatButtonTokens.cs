using AtomUI.Desktop.Controls;
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
internal sealed class ShadcnFloatButtonTokens : IControlTokenAlgorithm<FloatButtonToken>
{
    public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal)
    {
        var style = ShadcnStyles.Resolve(context);
        controlGlobal.ColorBorder = ShadcnButtonAppearance.ResolveOutline(in context, controlGlobal).Border;
        controlGlobal.BorderRadius = new CornerRadius(Math.Min(style.ButtonRadiusLG, controlGlobal.ControlHeightLG / 3));
        controlGlobal.BorderRadiusLG = new CornerRadius(style.ButtonRadiusLG);
        controlGlobal.PaddingLG = new Thickness(Math.Min(style.PaddingXSM, 10), 2);
        controlGlobal.SpacingXS = style == ShadcnStyles.Mira || style == ShadcnStyles.Sera ? 4 : 6;
    }

    public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, FloatButtonToken token)
    {
        var outline = ShadcnButtonAppearance.ResolveOutline(in context, effectiveGlobal);
        var elevated = effectiveGlobal.ColorBgElevated;
        // Floating actions cover scrolling content: composite the outline paints over an elevated surface.
        token.DefaultBg = outline.Background is { } background
            ? ShadcnBaseColors.CompositeOver(background, background.A / 255d, elevated)
            : elevated;
        token.DefaultHoverBg = ShadcnBaseColors.CompositeOver(
            outline.HoverBackground, outline.HoverBackground.A / 255d, token.DefaultBg);
        token.DefaultActiveBg = ShadcnBaseColors.CompositeOver(effectiveGlobal.ColorText, 0.05, token.DefaultHoverBg);
        token.DefaultIconColor = effectiveGlobal.ColorText;

        var style = ShadcnStyles.Resolve(context);
        var vega = style == ShadcnStyles.Vega;
        token.AppearanceSettings = new FloatButtonOutlineAppearance(
            vega, style == ShadcnStyles.Luma || style == ShadcnStyles.Maia, style == ShadcnStyles.Sera,
            style.ButtonWeight, Math.Ceiling(effectiveGlobal.FontSize * 1.15));
        token.FloatButtonShadows = vega ? ShadcnStyles.Shadow("xs") : default;
    }
}
