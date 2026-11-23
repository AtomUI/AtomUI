using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG view of <see cref="ButtonSpinnerToken"/>.</summary>
[ControlDerivation]
internal sealed class PrimeNGButtonSpinnerDerivation : IControlDerivation<PrimeNGToken, ButtonSpinnerToken>
{
    public void Derive(in DerivationContext<PrimeNGToken> context, ControlView<ButtonSpinnerToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        // PrimeNG: the control-global overrides, read by the formulas below.
        PrimeNGButtonSpinnerTokens.EvaluateGlobal(in context, global);
        PrimeNGAbstractButtonSpinnerDerivation.DeriveShared(in context, global, token);
        token.ControlWidth = 90;
        token.HandleBg = global.ColorBgContainer;
        token.FilledHandleBg = PrimeNGAbstractButtonSpinnerDerivation.CalculateFilledHandleBackground(token, global, token.HandleBg);
        // PrimeNG: the preset's control values, over the formulas.
        PrimeNGButtonSpinnerTokens.EvaluateOwn(in context, global, token);
    }
}
