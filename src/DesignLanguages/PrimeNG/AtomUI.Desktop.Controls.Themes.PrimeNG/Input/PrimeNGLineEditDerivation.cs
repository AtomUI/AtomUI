using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG view of <see cref="LineEditToken"/>.</summary>
[ControlDerivation]
internal sealed class PrimeNGLineEditDerivation : IControlDerivation<PrimeNGToken, LineEditToken>
{
    public void Derive(in DerivationContext<PrimeNGToken> context, ControlView<LineEditToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        // PrimeNG: the control-global overrides, read by the formulas below.
        PrimeNGLineEditTokens.EvaluateGlobal(in context, global);
        token.TransparentOutlineBorder = false;
        token.BottomOutlineBorder      = false;
        token.InputFontSize = global.FontSize;
        token.InputFontSizeLG = global.FontSizeLG;
        token.InputFontSizeSM = token.InputFontSize;
        // PrimeNG: the preset's control values, over the formulas.
        PrimeNGLineEditTokens.EvaluateOwn(in context, global, token);
    }
}
