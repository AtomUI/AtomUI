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
        EvaluateShape(effectiveGlobal, token);
    }

    /// <summary>
    /// Shape values of a progress control beyond the colours.
    /// </summary>
    private protected virtual void EvaluateShape(DesignToken effectiveGlobal, TToken token)
    {
    }
}

/// <summary>
/// The line progress bar: the PrimeUIX colours, <c>progressbar.border.radius</c>, <c>content.border.radius</c>
/// (the global <c>BorderRadius</c>: 6, Nora 2) instead of Ant Design's capsule, and <c>progressbar.label.color</c>,
/// <c>primary.contrast.color</c>, for an inner percentage label: both candidates of the label's readability choice are
/// the contrast colour (<c>InnerLabelColor</c> = <c>ColorTextLightSolid</c>). PrimeUIX's fixed height (1.125rem,
/// Lara 1.25rem) and label font have no token: the bar keeps AtomUI's size types.
/// </summary>
[ControlTokenAlgorithm("PrimeNG", "Aura", 1)]
[ControlTokenAlgorithm("PrimeNG", "Aura.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora.Dark", 1)]
internal sealed class PrimeNGProgressBarTokens : PrimeNGProgressTokens<ProgressBarToken>
{
    private protected override void EvaluateShape(DesignToken effectiveGlobal, ProgressBarToken token)
    {
        token.LineBorderRadius = effectiveGlobal.BorderRadius;
        token.InnerLabelColor  = effectiveGlobal.ColorTextLightSolid;
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
