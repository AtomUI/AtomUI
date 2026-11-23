using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>The AntDesign view of <see cref="SimplePaginationToken"/>.</summary>
[ControlDerivation]
internal sealed class AntDesignSimplePaginationDerivation : IControlDerivation<AntDesignToken, SimplePaginationToken>
{
    public void Derive(in DerivationContext<AntDesignToken> context, ControlView<SimplePaginationToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        AntDesignAbstractPaginationDerivation.DeriveShared(in context, global, token);
        token.ItemInputBg = global.ColorBgContainer;
    }
}
