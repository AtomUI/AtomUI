using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;
using AtomUI.Theme.DesignTokens;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>
/// Spin under the PrimeNG presets. The indicator cycle derives from <c>MotionDurationSlow</c> (four times it), which a
/// preset with instant state transitions sets to 0 (Nora, see <see cref="PrimeNGMotion"/>); PrimeNG's spinner keeps
/// turning in every preset, so the cycle keeps the Ant Design value (1.2s).
/// </summary>
internal static class PrimeNGSpinTokens
{
    internal static readonly TimeSpan IndicatorCycle = TimeSpan.FromMilliseconds(1200);
    internal static void EvaluateGlobal(in TokenEvaluationContext<PrimeNGToken> context, DesignToken controlGlobal)
    {
    }
    internal static void EvaluateOwn(in TokenEvaluationContext<PrimeNGToken> context, DesignToken effectiveGlobal, SpinToken token)
    {
        if (effectiveGlobal.EnableMotion && PrimeNGMotion.HasInstantTransitions(context))
        {
            token.IndicatorDuration = IndicatorCycle;
        }
    }
}
