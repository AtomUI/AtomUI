using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG view of <see cref="NumericUpDownToken"/>.</summary>
[ControlTokenRule]
internal sealed class NumericUpDownTokenRule : IControlTokenRule<PrimeNGToken, NumericUpDownToken>
{
    public void Apply(in TokenEvaluationContext<PrimeNGToken> context, ControlTokenTarget<NumericUpDownToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        // PrimeNG: the control-global overrides, read by the formulas below.
        PrimeNGNumericUpDownTokens.EvaluateGlobal(in context, global);
        ButtonSpinnerTokenCalculator.Calculate(in context, global, token);
        token.InputFontSizeSM = global.FontSize;
        token.HandleBg = global.ColorBgContainer;
        token.FilledHandleBg = ButtonSpinnerTokenCalculator.CalculateFilledHandleBackground(token, global, token.HandleBg);
        // PrimeNG: the preset's control values, over the formulas.
        PrimeNGNumericUpDownTokens.EvaluateOwn(in context, global, token);
    }
}
