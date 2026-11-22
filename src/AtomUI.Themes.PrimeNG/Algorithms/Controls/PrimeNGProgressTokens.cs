using AtomUI.Desktop.Controls;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.DesignTokens;

namespace AtomUI.Themes.PrimeNG.Algorithms.Controls;

/// <summary>
/// The progress controls under the PrimeNG presets: PrimeUIX 3.1.1 <c>progressbar</c>. The value is
/// <c>{primary.color}</c> (<c>ColorPrimary</c>; noir light <c>#020617</c>, dark <c>#FAFAFA</c>) and the track
/// <c>progressbar.background</c> (Aura and Lara <c>content.border.color</c>, surface 200 / 700; Nora surface 300 / 600).
/// PrimeUIX has no success or exception state; those keep the preset's status colours, and the circle text keeps
/// <c>ColorText</c>.
/// </summary>
internal abstract class PrimeNGProgressTokens<TToken> : IControlTokenAlgorithm<TToken>
    where TToken : AbstractProgressToken
{
    /// <inheritdoc />
    public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal)
    {
    }

    /// <inheritdoc />
    public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, TToken token)
    {
        var rules = PrimeNGPresets.Resolve(context).For(context.Appearance);
        token.DefaultColor   = effectiveGlobal.ColorPrimary;
        token.RemainingColor = rules.ProgressTrack.Resolve(rules.Surface.AsSpan());
        EvaluateShape(context, effectiveGlobal, token);
    }

    /// <summary>
    /// Shape values of a progress control beyond the colours.
    /// </summary>
    private protected virtual void EvaluateShape(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, TToken token)
    {
    }
}

/// <summary>
/// The line progress bar: the PrimeUIX colours, <c>progressbar.border.radius</c>, <c>content.border.radius</c>
/// (the global <c>BorderRadius</c>: 6, Nora 2) instead of Ant Design's capsule, and <c>progressbar.label.color</c>,
/// <c>primary.contrast.color</c>, for an inner percentage label: of the label's two candidates (<c>InnerLabelColor</c>
/// = <c>ColorText</c> and <c>ColorTextLightSolid</c>) the contrast colour is the readable one on the primary bar. PrimeNG 22 renders the label inside the
/// value bar and hides it at 0 (<c>labelDisplay</c>: <c>display: none</c> when the value is 0), where it would be the
/// contrast colour on the light track, so <c>InnerLabelEmptyOpacity</c> is 0. The bar is PrimeUIX's fixed height
/// (1.125rem, Lara 1.25rem), so the label fits inside it; the label font has no token.
/// </summary>
[ControlTokenAlgorithm("PrimeNG", "Aura", 1)]
[ControlTokenAlgorithm("PrimeNG", "Aura.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora.Dark", 1)]
internal sealed class PrimeNGProgressBarTokens : PrimeNGProgressTokens<ProgressBarToken>
{
    private protected override void EvaluateShape(in ThemeAlgorithmContext context, DesignToken effectiveGlobal,
                                                  ProgressBarToken token)
    {
        // progressbar.height 1.125rem (Lara 1.25rem), measured 18 on primeng.dev: the label sits inside the value bar.
        // The default ProgressBar is the Large preset (antd's default 8), so that preset is the PrimeUIX height; PrimeUIX
        // has one height, and the smaller presets step down about a third each (antd 8 / 6 / 4): 12 / 8 (Lara 14 / 10).
        var lara = PrimeNGPresets.KindOf(context) == PrimeNGPresetKind.Lara;
        token.LineStrokeThicknessLG  = lara ? 20 : 18;
        token.LineStrokeThickness    = lara ? 14 : 12;
        token.LineStrokeThicknessSM  = lara ? 10 : 8;
        token.LineBorderRadius       = effectiveGlobal.BorderRadius;
        // The label takes the more readable of InnerLabelColor and ColorTextLightSolid on its bar: on the primary that
        // is the contrast colour (progressbar.label.color, primary.contrast.color); a custom bar colour can pick the text.
        token.InnerLabelColor        = effectiveGlobal.ColorText;
        token.InnerLabelEmptyOpacity = 0;
    }
}

/// <summary>
/// The steps progress bar: the PrimeUIX <c>progressbar</c> colours.
/// </summary>
[ControlTokenAlgorithm("PrimeNG", "Aura", 1)]
[ControlTokenAlgorithm("PrimeNG", "Aura.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora.Dark", 1)]
internal sealed class PrimeNGStepsProgressBarTokens : PrimeNGProgressTokens<StepsProgressBarToken>;

/// <summary>
/// The circle progress: the PrimeUIX <c>progressbar</c> colours (PrimeNG has no circle progress).
/// </summary>
[ControlTokenAlgorithm("PrimeNG", "Aura", 1)]
[ControlTokenAlgorithm("PrimeNG", "Aura.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora.Dark", 1)]
internal sealed class PrimeNGCircleProgressTokens : PrimeNGProgressTokens<CircleProgressToken>;

/// <summary>
/// The dashboard progress: the PrimeUIX <c>progressbar</c> colours (PrimeNG has no dashboard progress).
/// </summary>
[ControlTokenAlgorithm("PrimeNG", "Aura", 1)]
[ControlTokenAlgorithm("PrimeNG", "Aura.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora.Dark", 1)]
internal sealed class PrimeNGDashboardProgressTokens : PrimeNGProgressTokens<DashboardProgressToken>;
