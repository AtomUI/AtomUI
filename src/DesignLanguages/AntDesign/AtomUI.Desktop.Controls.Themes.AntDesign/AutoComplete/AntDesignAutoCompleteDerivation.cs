using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>The AntDesign view of <see cref="AutoCompleteToken"/>.</summary>
[ControlDerivation]
internal sealed class AntDesignAutoCompleteDerivation : IControlDerivation<AntDesignToken, AutoCompleteToken>
{
    public void Derive(in DerivationContext<AntDesignToken> context, ControlView<AutoCompleteToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        AntDesignAbstractAutoCompleteDerivation.DeriveShared(in context, global, token);
    }
}
