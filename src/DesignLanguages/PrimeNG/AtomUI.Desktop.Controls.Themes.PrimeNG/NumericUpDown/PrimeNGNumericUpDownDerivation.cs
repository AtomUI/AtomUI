using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG view of <see cref="NumericUpDownToken"/>.</summary>
[ControlDerivation]
internal sealed class PrimeNGNumericUpDownDerivation : IControlDerivation<PrimeNGToken, NumericUpDownToken>
{
    public void Derive(in DerivationContext<PrimeNGToken> context, ControlView<NumericUpDownToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        // PrimeNG: the control-global overrides, read by the formulas below.
        PrimeNGNumericUpDownTokens.EvaluateGlobal(in context, global);
        PrimeNGAbstractButtonSpinnerDerivation.DeriveShared(in context, global, token);
        token.InputFontSizeSM = global.FontSize;
        token.HandleBg = global.ColorBgContainer;
        token.FilledHandleBg = PrimeNGAbstractButtonSpinnerDerivation.CalculateFilledHandleBackground(token, global, token.HandleBg);
        // PrimeNG: the preset's control values, over the formulas.
        PrimeNGNumericUpDownTokens.EvaluateOwn(in context, global, token);
    }
}
