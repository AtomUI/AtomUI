using AtomUI.Desktop.Controls;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Themes.Shadcn.Algorithms.Controls;

/// <summary>
/// Button under the shadcn styles: the style's type weight (design 5.5); the colours come from the global brand
/// family (the 80% hover composite).
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
internal sealed class ShadcnButtonTokens : IControlTokenAlgorithm<ButtonToken>
{
    /// <inheritdoc />
    public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal)
    {
        var style = ShadcnStyles.Resolve(context);
        // The button sizes read the BorderRadius / LG / SM ladder. The capsule styles (rounded-full) draw half
        // the control height at every size -- the --radius value itself would exceed it and mangle the shape.
        if (ShadcnStyles.IsButtonCapsule(style.Id))
        {
            controlGlobal.BorderRadius   = new CornerRadius(controlGlobal.ControlHeight / 2);
            controlGlobal.BorderRadiusLG = new CornerRadius(controlGlobal.ControlHeightLG / 2);
            controlGlobal.BorderRadiusSM = new CornerRadius(controlGlobal.ControlHeightSM / 2);
        }
        else
        {
            controlGlobal.BorderRadius   = new CornerRadius(style.ButtonRadius);
            controlGlobal.BorderRadiusLG = new CornerRadius(style.ButtonRadius);
            controlGlobal.BorderRadiusSM = new CornerRadius(style.ButtonRadiusSM);
        }
    }

    /// <inheritdoc />
    public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, ButtonToken token)
    {
        token.FontWeight = ShadcnStyles.Resolve(context).ButtonWeight;
    }
}
