using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>The shadcn view of <see cref="RangeDateViewerToken"/>.</summary>
[ControlDerivation]
internal sealed class ShadcnRangeDateViewerDerivation : IControlDerivation<ShadcnToken, RangeDateViewerToken>
{
    public void Derive(in DerivationContext<ShadcnToken> context, ControlView<RangeDateViewerToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        ShadcnAbstractDateViewerDerivation.DeriveShared(in context, global, token);
        token.CellHeight = global.ControlHeightSM;
        token.CellRowHeight = ShadcnAbstractDateViewerDerivation.CalculateCellRowHeight(token, global, token.CellHeight);
        token.PanelSpacing = global.UniformlyMargin;
    }
}
