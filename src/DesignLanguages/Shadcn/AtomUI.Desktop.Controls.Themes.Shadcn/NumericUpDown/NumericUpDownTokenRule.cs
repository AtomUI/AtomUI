using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>The shadcn view of <see cref="NumericUpDownToken"/>.</summary>
[ControlTokenRule]
internal sealed class NumericUpDownTokenRule : IControlTokenRule<ShadcnToken, NumericUpDownToken>
{
    public void Apply(in TokenEvaluationContext<ShadcnToken> context, ControlTokenTarget<NumericUpDownToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        ButtonSpinnerTokenCalculator.Calculate(in context, global, token);
        token.InputFontSizeSM = global.FontSize;
        token.HandleBg = global.ColorBgContainer;
        token.FilledHandleBg = ButtonSpinnerTokenCalculator.CalculateFilledHandleBackground(token, global, token.HandleBg);
    }
}
