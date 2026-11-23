using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;
using AtomUI.Toolkits.GalleryBase.Controls;
using Avalonia;

namespace AtomUI.Toolkits.GalleryBase.Derivations;

/// <summary>The Generic view of <see cref="ShowCasePanelToken"/>.</summary>
[ControlDerivation]
internal sealed class GenericShowCasePanelDerivation : IControlDerivation<LanguageDesignToken, ShowCasePanelToken>
{
    public void Derive(in DerivationContext<LanguageDesignToken> context, ControlView<ShowCasePanelToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        token.ContentMargin = new Thickness(global.SizeUnit * 6);
        token.MinItemWidth  = global.SizeUnit * 90;
        token.MaxColumns    = 2;
        token.ColumnGap     = global.SizeUnit * 4;
        token.RowGap        = global.SizeUnit * 4;
    }
}
