using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG view of <see cref="StepsProgressBarToken"/>.</summary>
[ControlTokenRule]
internal sealed class StepsProgressBarTokenRule : IControlTokenRule<PrimeNGToken, StepsProgressBarToken>
{
    public void Apply(in TokenEvaluationContext<PrimeNGToken> context, ControlTokenTarget<StepsProgressBarToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        // PrimeNG: the control-global overrides, read by the formulas below.
        PrimeNGProgressTokens<StepsProgressBarToken>.EvaluateGlobal(in context, global);
        LineProgressTokenCalculator.Calculate(in context, global, token);
        token.ProgressStepMarginInlineEnd = global.UniformlyMarginXXS / 2;

        token.ProgressStepMinWidth = token.ProgressStepMarginInlineEnd;
        // PrimeNG: the preset's control values, over the formulas.
        PrimeNGProgressTokens<StepsProgressBarToken>.EvaluateOwn(in context, global, token);
    }
}
