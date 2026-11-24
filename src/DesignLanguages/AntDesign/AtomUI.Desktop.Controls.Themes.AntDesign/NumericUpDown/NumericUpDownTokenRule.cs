using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>The AntDesign view of <see cref="NumericUpDownToken"/>.</summary>
[ControlTokenRule]
internal sealed class NumericUpDownTokenRule : IControlTokenRule<AntDesignToken, NumericUpDownToken>
{
    public void Apply(in TokenEvaluationContext<AntDesignToken> context, ControlTokenTarget<NumericUpDownToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        ButtonSpinnerTokenCalculator.Calculate(in context, global, token);
        token.InputFontSizeSM = global.FontSize;
        token.HandleBg = global.ColorBgContainer;
        token.FilledHandleBg = ButtonSpinnerTokenCalculator.CalculateFilledHandleBackground(token, global, token.HandleBg);
    }
}
