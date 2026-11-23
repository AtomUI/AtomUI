using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG view of <see cref="SplitButtonToken"/>.</summary>
[ControlDerivation]
internal sealed class PrimeNGSplitButtonDerivation : IControlDerivation<PrimeNGToken, SplitButtonToken>
{
    public void Derive(in DerivationContext<PrimeNGToken> context, ControlView<SplitButtonToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        // PrimeNG: the control-global overrides, read by the formulas below.
        PrimeNGSplitButtonTokens.EvaluateGlobal(in context, global);
        token.InsetSeparator         = false;
        token.GutterToFlyout         = global.UniformlyMarginXXS;
        token.SolidSeparatorColor    = global.ColorPrimaryHover;
        token.DangerSeparatorColor   = global.ColorErrorHover;
        // PrimeNG: the preset's control values, over the formulas.
        PrimeNGSplitButtonTokens.EvaluateOwn(in context, global, token);
    }
}
