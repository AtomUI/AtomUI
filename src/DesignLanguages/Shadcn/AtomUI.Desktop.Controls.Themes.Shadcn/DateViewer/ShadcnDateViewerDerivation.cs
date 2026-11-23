using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>The shadcn view of <see cref="DateViewerToken"/>.</summary>
[ControlDerivation]
internal sealed class ShadcnDateViewerDerivation : IControlDerivation<ShadcnToken, DateViewerToken>
{
    public void Derive(in DerivationContext<ShadcnToken> context, ControlView<DateViewerToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        ShadcnAbstractDateViewerDerivation.DeriveShared(in context, global, token);
        token.CellHeight = global.ControlHeightSM;
        token.CellRowHeight = ShadcnAbstractDateViewerDerivation.CalculateCellRowHeight(token, global, token.CellHeight);
        token.FooterBorderThickness = new Thickness(0, global.LineWidth, 0, 0);
    }
}
