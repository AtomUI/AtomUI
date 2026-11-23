using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>The AntDesign view of <see cref="TabStripToken"/>.</summary>
[ControlDerivation]
internal sealed class AntDesignTabStripDerivation : IControlDerivation<AntDesignToken, TabStripToken>
{
    public void Derive(in DerivationContext<AntDesignToken> context, ControlView<TabStripToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        AntDesignAbstractLineTabDerivation.DeriveShared(in context, global, token);
    }
}
