using AtomUI.Desktop.Controls;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.DesignTokens;

namespace AtomUI.Themes.PrimeNG.Algorithms.Controls;

/// <summary>
/// Spin under the PrimeNG presets. The indicator cycle derives from <c>MotionDurationSlow</c> (four times it), which a
/// preset with instant state transitions sets to 0 (Nora, see <see cref="PrimeNGMotion"/>); PrimeNG's spinner keeps
/// turning in every preset, so the cycle keeps the Ant Design value (1.2s).
/// </summary>
[ControlTokenAlgorithm("PrimeNG", "Aura", 1)]
[ControlTokenAlgorithm("PrimeNG", "Aura.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora.Dark", 1)]
internal sealed class PrimeNGSpinTokens : IControlTokenAlgorithm<SpinToken>
{
    internal static readonly TimeSpan IndicatorCycle = TimeSpan.FromMilliseconds(1200);

    /// <inheritdoc />
    public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal)
    {
    }

    /// <inheritdoc />
    public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, SpinToken token)
    {
        if (effectiveGlobal.EnableMotion && PrimeNGMotion.HasInstantTransitions(context))
        {
            token.IndicatorDuration = IndicatorCycle;
        }
    }
}
