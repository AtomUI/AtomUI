using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>
/// The AntDesign view of <see cref="ButtonSpinnerToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class ButtonSpinnerTokenRule : IControlTokenRule<AntDesignToken, ButtonSpinnerToken>
{
    public void Apply(in TokenEvaluationContext<AntDesignToken> context, ControlTokenTarget<ButtonSpinnerToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        ButtonSpinnerTokenCalculator.Calculate(in context, global, token);
        token.ControlWidth = 90;
        token.HandleBg = global.ColorBgContainer;
        token.FilledHandleBg = ButtonSpinnerTokenCalculator.CalculateFilledHandleBackground(token, global, token.HandleBg);
    }
}
