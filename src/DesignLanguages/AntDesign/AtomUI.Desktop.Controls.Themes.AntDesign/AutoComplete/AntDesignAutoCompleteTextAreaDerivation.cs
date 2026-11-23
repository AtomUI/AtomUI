using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>The AntDesign view of <see cref="AutoCompleteTextAreaToken"/>.</summary>
[ControlDerivation]
internal sealed class AntDesignAutoCompleteTextAreaDerivation : IControlDerivation<AntDesignToken, AutoCompleteTextAreaToken>
{
    public void Derive(in DerivationContext<AntDesignToken> context, ControlView<AutoCompleteTextAreaToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        AntDesignAbstractAutoCompleteDerivation.DeriveShared(in context, global, token);
    }
}
