using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>The AntDesign view of <see cref="MenuFlyoutPresenterToken"/>.</summary>
[ControlDerivation]
internal sealed class AntDesignMenuFlyoutPresenterDerivation : IControlDerivation<AntDesignToken, MenuFlyoutPresenterToken>
{
    public void Derive(in DerivationContext<AntDesignToken> context, ControlView<MenuFlyoutPresenterToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        AntDesignAbstractMenuPopupDerivation.DeriveShared(in context, global, token);
        token.MenuPopupBorderColor = global.ColorBorderSecondary;
        token.MenuPopupBorderThickness = new Thickness();
        token.ItemColor = global.ColorText;
        token.ItemHoverColor = token.ItemColor;
    }
}
