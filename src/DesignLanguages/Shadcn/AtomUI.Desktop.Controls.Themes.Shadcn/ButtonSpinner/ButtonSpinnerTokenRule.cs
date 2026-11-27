using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>
/// The shadcn view of <see cref="ButtonSpinnerToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class ButtonSpinnerTokenRule : IControlTokenRule<ShadcnToken, ButtonSpinnerToken>
{
    public void Apply(in TokenEvaluationContext<ShadcnToken> context, ControlTokenTarget<ButtonSpinnerToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        ButtonSpinnerTokenCalculator.Calculate(in context, global, token);
        // Ant Design formula kept as is: shadcn has no button spinner, so the language has no values of its own.
        token.ControlWidth = 90;
        token.HandleBg = global.ColorBgContainer;
        token.FilledHandleBg = ButtonSpinnerTokenCalculator.CalculateFilledHandleBackground(token, global, token.HandleBg);
    }
}
