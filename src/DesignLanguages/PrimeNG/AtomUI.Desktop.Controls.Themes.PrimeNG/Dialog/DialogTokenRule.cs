using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>
/// The PrimeNG view of <see cref="DialogToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class DialogTokenRule : IControlTokenRule<PrimeNGToken, DialogToken>
{
    public void Apply(in TokenEvaluationContext<PrimeNGToken> context, ControlTokenTarget<DialogToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        // PrimeNG: the control-global overrides (MotionDurationMid, BorderRadiusLG, BorderRadiusSM, BoxShadows,
        // FontWeightStrong), read by the Dialog theme, not by the formulas below; MotionDurationSlow by neither.
        PrimeNGDialogTokens.EvaluateGlobal(in context, global);
        DialogTokenCalculator.Calculate(in context, global, token);
        // Ant Design formula kept as is: the PrimeNG dialog stage (PrimeNGDialogTokens.EvaluateOwn) replaces
        // HeaderPadding below, so MinHeight is computed again after it; MinWidth 200 is not replaced.
        token.HeaderPadding = DialogTokenCalculator.CalculateHeaderPadding(token, global);
        token.MinHeight = DialogTokenCalculator.CalculateMinimumHeight(token, global, token.HeaderPadding);
        token.MinWidth = 200;
        // PrimeNG: the preset's control values, over the formulas.
        PrimeNGDialogTokens.EvaluateOwn(in context, global, token);
        // PrimeNG: the formulas that read a value the preset replaced above run again with it, as the previous
        // staged evaluation did (its second round kept the preset's value).
        token.MinHeight = DialogTokenCalculator.CalculateMinimumHeight(token, global, token.HeaderPadding);
    }
}
