using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>The AntDesign view of <see cref="RangeDateViewerToken"/>.</summary>
[ControlDerivation]
internal sealed class AntDesignRangeDateViewerDerivation : IControlDerivation<AntDesignToken, RangeDateViewerToken>
{
    public void Derive(in DerivationContext<AntDesignToken> context, ControlView<RangeDateViewerToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        AntDesignAbstractDateViewerDerivation.DeriveShared(in context, global, token);
        token.CellHeight = global.ControlHeightSM;
        token.CellRowHeight = AntDesignAbstractDateViewerDerivation.CalculateCellRowHeight(token, global, token.CellHeight);
        token.PanelSpacing = global.UniformlyMargin;
    }
}
