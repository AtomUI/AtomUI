using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>
/// The PrimeNG view of <see cref="MessageBoxToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class MessageBoxTokenRule : IControlTokenRule<PrimeNGToken, MessageBoxToken>
{
    public void Apply(in TokenEvaluationContext<PrimeNGToken> context, ControlTokenTarget<MessageBoxToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        // PrimeNG: the control-global overrides (MotionDurationMid, BorderRadiusLG, BorderRadiusSM, BoxShadows,
        // FontWeightStrong), read by the MessageBox theme, not by the formulas below; MotionDurationSlow by neither.
        PrimeNGMessageBoxTokens.EvaluateGlobal(in context, global);
        DialogTokenCalculator.Calculate(in context, global, token);
        // Ant Design formula kept as is: the PrimeNG message box stage (PrimeNGMessageBoxTokens.EvaluateOwn)
        // replaces HeaderPadding and StyleIconSize below, so MinHeight is computed again after it. MinWidth
        // 410 is not replaced.
        token.HeaderPadding = DialogTokenCalculator.CalculateHeaderPadding(token, global);
        token.MinHeight = DialogTokenCalculator.CalculateMinimumHeight(token, global, token.HeaderPadding);
        token.MinWidth = 410;
        token.StyleIconSize = global.SizeLG * 1.2;
        // PrimeNG: the preset's control values, over the formulas.
        PrimeNGMessageBoxTokens.EvaluateOwn(in context, global, token);
        // PrimeNG: the formulas that read a value the preset replaced above run again with it, as the previous
        // staged evaluation did (its second round kept the preset's value).
        token.MinHeight = DialogTokenCalculator.CalculateMinimumHeight(token, global, token.HeaderPadding);
    }
}
