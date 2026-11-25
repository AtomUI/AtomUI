using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG view of <see cref="PopupConfirmToken"/>.</summary>
[ControlTokenRule]
internal sealed class PopupConfirmTokenRule : IControlTokenRule<PrimeNGToken, PopupConfirmToken>
{
    public void Apply(in TokenEvaluationContext<PrimeNGToken> context, ControlTokenTarget<PopupConfirmToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        // PrimeNG: the control-global overrides, read by the formulas below.
        PrimeNGPopupConfirmTokens.EvaluateGlobal(in context, global);
        token.ButtonSpacing          = global.UniformlyMarginXS;
        token.IconMargin             = new Thickness(0, global.UniformlyMarginXS / 2, global.UniformlyMarginXS, 0);
        token.ContentContainerMargin = new Thickness(0, 0, 0, global.UniformlyMarginXS);
        token.TitleMargin            = new Thickness(0, 0, 0, global.UniformlyMarginXS);
        token.ButtonContainerMargin  = new Thickness(0, global.UniformlyMarginXS, 0, 0);
        // PrimeNG: the preset's control values, over the formulas.
        PrimeNGPopupConfirmTokens.EvaluateOwn(in context, global, token);
    }
}
