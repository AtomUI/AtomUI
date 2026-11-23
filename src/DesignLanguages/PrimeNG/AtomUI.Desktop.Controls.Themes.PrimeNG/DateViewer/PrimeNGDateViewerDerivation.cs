using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG view of <see cref="DateViewerToken"/>.</summary>
[ControlDerivation]
internal sealed class PrimeNGDateViewerDerivation : IControlDerivation<PrimeNGToken, DateViewerToken>
{
    public void Derive(in DerivationContext<PrimeNGToken> context, ControlView<DateViewerToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        // PrimeNG: the control-global overrides, read by the formulas below.
        PrimeNGDateViewerTokens.EvaluateGlobal(in context, global);
        PrimeNGAbstractDateViewerDerivation.DeriveShared(in context, global, token);
        token.CellHeight = global.ControlHeightSM;
        token.CellRowHeight = PrimeNGAbstractDateViewerDerivation.CalculateCellRowHeight(token, global, token.CellHeight);
        token.FooterBorderThickness = new Thickness(0, global.LineWidth, 0, 0);
        // PrimeNG: the preset's control values, over the formulas.
        PrimeNGDateViewerTokens.EvaluateOwn(in context, global, token);
    }
}
