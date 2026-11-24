using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG view of <see cref="ButtonSpinnerToken"/>.</summary>
[ControlTokenRule]
internal sealed class ButtonSpinnerTokenRule : IControlTokenRule<PrimeNGToken, ButtonSpinnerToken>
{
    public void Apply(in TokenEvaluationContext<PrimeNGToken> context, ControlTokenTarget<ButtonSpinnerToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        // PrimeNG: the control-global overrides, read by the formulas below.
        PrimeNGButtonSpinnerTokens.EvaluateGlobal(in context, global);
        ButtonSpinnerTokenCalculator.Calculate(in context, global, token);
        token.ControlWidth = 90;
        token.HandleBg = global.ColorBgContainer;
        token.FilledHandleBg = ButtonSpinnerTokenCalculator.CalculateFilledHandleBackground(token, global, token.HandleBg);
        // PrimeNG: the preset's control values, over the formulas.
        PrimeNGButtonSpinnerTokens.EvaluateOwn(in context, global, token);
    }
}
