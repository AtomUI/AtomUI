using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>
/// The PrimeNG view of <see cref="TabStripToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class TabStripTokenRule : IControlTokenRule<PrimeNGToken, TabStripToken>
{
    public void Apply(in TokenEvaluationContext<PrimeNGToken> context, ControlTokenTarget<TabStripToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        // PrimeNG: the control-global overrides (MotionDurationSlow, ColorBorderSecondary), read by the TabStrip theme
        // (the ink bar transition, BorderBrush) and not by the formulas below.
        PrimeNGTabStripTokens.EvaluateGlobal(in context, global);
        LineTabTokenCalculator.Calculate(in context, global, token);
        // PrimeNG: the preset's control values, over the formulas.
        PrimeNGTabStripTokens.EvaluateOwn(in context, global, token);
    }
}
