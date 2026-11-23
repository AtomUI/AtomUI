using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>The shadcn view of <see cref="SearchEditToken"/>.</summary>
[ControlDerivation]
internal sealed class ShadcnSearchEditDerivation : IControlDerivation<ShadcnToken, SearchEditToken>
{
    public void Derive(in DerivationContext<ShadcnToken> context, ControlView<SearchEditToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        token.InputFontSizeSM = global.FontSize;
    }
}
