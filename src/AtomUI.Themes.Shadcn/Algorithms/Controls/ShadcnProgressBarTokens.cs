using AtomUI.Desktop.Controls;
using AtomUI.Theme;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.DesignTokens;
using Avalonia;

namespace AtomUI.Themes.Shadcn.Algorithms.Controls;

/// <summary>
/// The line progress of the shadcn styles (appendix C: one bar height and radius per style, indicator
/// <c>bg-primary</c>, track <c>bg-muted</c>). The antd defaults (<c>ColorInfo</c> indicator, fill-secondary track,
/// 8/6/4 size ladder) do not follow the style's primary, and the heights have no ladder in the registry - every
/// style draws one height, so the whole thickness ladder carries the style value like the pill-button radius
/// ladder does (Luma sets ButtonRadiusSM = ButtonRadiusLG = 26).
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
internal sealed class ShadcnProgressBarTokens : IControlTokenAlgorithm<ProgressBarToken>
{
    /// <inheritdoc />
    public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal)
    {
    }

    /// <inheritdoc />
    public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, ProgressBarToken token)
    {
        var style  = ShadcnStyles.Resolve(context);
        var dark   = context.Appearance == ThemeAppearance.Dark;
        var surface = ShadcnBaseColors.Resolve(effectiveGlobal.ColorNeutral, dark);

        // bg-primary / bg-muted over the whole line family.
        token.DefaultColor   = surface.Primary;
        token.RemainingColor = surface.Muted;

        // One bar height per style: the registry has no size variants, so the ladder carries the style
        // height at every size (the same way the pill-button styles carry 26 through their radius ladder).
        token.LineStrokeThicknessLG = style.ProgressHeight;
        token.LineStrokeThickness   = style.ProgressHeight;
        token.LineStrokeThicknessSM = style.ProgressHeight;

        // rounded-full keeps the capsule sentinel (the drawing path clamps it to half the height);
        // literal radii (4xl 26 / 2xl 18 / md 8) clamp the same way; rounded-none is square.
        token.LineBorderRadius = style.ProgressRadius >= 100
            ? new CornerRadius(100)
            : new CornerRadius(style.ProgressRadius);
    }
}
