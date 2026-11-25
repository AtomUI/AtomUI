using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG view of <see cref="MessageBoxToken"/>.</summary>
[ControlTokenRule]
internal sealed class MessageBoxTokenRule : IControlTokenRule<PrimeNGToken, MessageBoxToken>
{
    public void Apply(in TokenEvaluationContext<PrimeNGToken> context, ControlTokenTarget<MessageBoxToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        // PrimeNG: the control-global overrides, read by the formulas below.
        PrimeNGMessageBoxTokens.EvaluateGlobal(in context, global);
        DialogTokenCalculator.Calculate(in context, global, token);
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
