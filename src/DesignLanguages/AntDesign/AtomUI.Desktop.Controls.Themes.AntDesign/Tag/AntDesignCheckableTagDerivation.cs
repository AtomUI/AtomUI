using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>The AntDesign view of <see cref="CheckableTagToken"/>.</summary>
[ControlDerivation]
internal sealed class AntDesignCheckableTagDerivation : IControlDerivation<AntDesignToken, CheckableTagToken>
{
    public void Derive(in DerivationContext<AntDesignToken> context, ControlView<CheckableTagToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        AntDesignAbstractTagDerivation.DeriveShared(in context, global, token);
    }
}
