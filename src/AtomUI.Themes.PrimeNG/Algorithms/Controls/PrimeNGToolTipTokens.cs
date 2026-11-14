using AtomUI.Desktop.Controls;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.DesignTokens;

namespace AtomUI.Themes.PrimeNG.Algorithms.Controls;

/// <summary>
/// ToolTip under the PrimeNG presets: PrimeUIX <c>tooltip.background</c> (neutral 700), <c>tooltip.color</c>
/// (<c>surface.0</c>, white in both appearances), <c>overlay.popover.border.radius</c> (the global
/// <c>BorderRadius</c>) and the padding of the baseline XML (12,8, the <c>aura-compat</c> <c>tooltip.padding</c>).
/// </summary>
[ControlTokenAlgorithm("PrimeNG", "Aura", 1)]
[ControlTokenAlgorithm("PrimeNG", "Aura.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara.Dark", 1)]
internal sealed class PrimeNGToolTipTokens : IControlTokenAlgorithm<ToolTipToken>
{
    /// <inheritdoc />
    public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal)
    {
    }

    /// <inheritdoc />
    public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, ToolTipToken token)
    {
        var preset  = PrimeNGPresets.Resolve(context);
        var rules   = preset.For(context.Appearance);
        var surface = rules.Surface.AsSpan();
        token.ToolTipBackground   = rules.Neutral.BgSpotlight.Resolve(surface);
        token.ToolTipColor        = preset.Controls.ToolTipColor.Resolve(surface);
        token.ToolTipCornerRadius = effectiveGlobal.BorderRadius;
        token.ContentPadding      = preset.Controls.ToolTipPadding;
    }
}
