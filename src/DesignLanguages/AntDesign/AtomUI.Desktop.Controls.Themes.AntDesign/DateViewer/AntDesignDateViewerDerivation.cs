using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>The AntDesign view of <see cref="DateViewerToken"/>.</summary>
[ControlDerivation]
internal sealed class AntDesignDateViewerDerivation : IControlDerivation<AntDesignToken, DateViewerToken>
{
    public void Derive(in DerivationContext<AntDesignToken> context, ControlView<DateViewerToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        AntDesignAbstractDateViewerDerivation.DeriveShared(in context, global, token);
        token.CellHeight = global.ControlHeightSM;
        token.CellRowHeight = AntDesignAbstractDateViewerDerivation.CalculateCellRowHeight(token, global, token.CellHeight);
        token.FooterBorderThickness = new Thickness(0, global.LineWidth, 0, 0);
    }
}
