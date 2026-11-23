using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>The AntDesign view of <see cref="SearchEditToken"/>.</summary>
[ControlDerivation]
internal sealed class AntDesignSearchEditDerivation : IControlDerivation<AntDesignToken, SearchEditToken>
{
    public void Derive(in DerivationContext<AntDesignToken> context, ControlView<SearchEditToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        token.InputFontSizeSM = global.FontSize;
    }
}
