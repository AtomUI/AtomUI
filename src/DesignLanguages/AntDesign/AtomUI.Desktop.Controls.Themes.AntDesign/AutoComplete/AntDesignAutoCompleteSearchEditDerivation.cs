using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>The AntDesign view of <see cref="AutoCompleteSearchEditToken"/>.</summary>
[ControlDerivation]
internal sealed class AntDesignAutoCompleteSearchEditDerivation : IControlDerivation<AntDesignToken, AutoCompleteSearchEditToken>
{
    public void Derive(in DerivationContext<AntDesignToken> context, ControlView<AutoCompleteSearchEditToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        AntDesignAbstractAutoCompleteDerivation.DeriveShared(in context, global, token);
    }
}
