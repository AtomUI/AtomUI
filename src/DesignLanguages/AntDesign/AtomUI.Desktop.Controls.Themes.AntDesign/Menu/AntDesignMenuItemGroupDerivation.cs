using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>The AntDesign view of <see cref="MenuItemGroupToken"/>.</summary>
[ControlDerivation]
internal sealed class AntDesignMenuItemGroupDerivation : IControlDerivation<AntDesignToken, MenuItemGroupToken>
{
    public void Derive(in DerivationContext<AntDesignToken> context, ControlView<MenuItemGroupToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        token.TitleFontWeight = FontWeight.Normal;
    }
}
