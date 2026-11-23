using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG view of <see cref="RangeDateViewerToken"/>.</summary>
[ControlDerivation]
internal sealed class PrimeNGRangeDateViewerDerivation : IControlDerivation<PrimeNGToken, RangeDateViewerToken>
{
    public void Derive(in DerivationContext<PrimeNGToken> context, ControlView<RangeDateViewerToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        // PrimeNG: the control-global overrides, read by the formulas below.
        PrimeNGRangeDateViewerTokens.EvaluateGlobal(in context, global);
        PrimeNGAbstractDateViewerDerivation.DeriveShared(in context, global, token);
        token.CellHeight = global.ControlHeightSM;
        token.CellRowHeight = PrimeNGAbstractDateViewerDerivation.CalculateCellRowHeight(token, global, token.CellHeight);
        token.PanelSpacing = global.UniformlyMargin;
        // PrimeNG: the preset's control values, over the formulas.
        PrimeNGRangeDateViewerTokens.EvaluateOwn(in context, global, token);
    }
}
