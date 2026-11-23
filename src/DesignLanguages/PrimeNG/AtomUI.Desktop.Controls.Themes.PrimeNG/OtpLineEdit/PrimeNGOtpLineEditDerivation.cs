using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG view of <see cref="OtpLineEditToken"/>.</summary>
[ControlDerivation]
internal sealed class PrimeNGOtpLineEditDerivation : IControlDerivation<PrimeNGToken, OtpLineEditToken>
{
    public void Derive(in DerivationContext<PrimeNGToken> context, ControlView<OtpLineEditToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        // PrimeNG: the control-global overrides, read by the formulas below.
        PrimeNGOtpLineEditTokens.EvaluateGlobal(in context, global);

        token.CellWidth               = global.ControlHeight;
        token.CellWidthLG             = global.ControlHeightLG;
        token.CellWidthSM             = global.ControlHeightSM;
        token.CellGap                 = global.UniformlyPaddingXXS;
        // Ant Design draws separate cells.
        token.JoinCells = false;
        // PrimeNG: the preset's control values, over the formulas.
        PrimeNGOtpLineEditTokens.EvaluateOwn(in context, global, token);
    }
}
