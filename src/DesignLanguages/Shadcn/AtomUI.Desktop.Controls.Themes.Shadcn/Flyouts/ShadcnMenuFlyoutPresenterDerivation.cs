using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>The shadcn view of <see cref="MenuFlyoutPresenterToken"/>.</summary>
[ControlDerivation]
internal sealed class ShadcnMenuFlyoutPresenterDerivation : IControlDerivation<ShadcnToken, MenuFlyoutPresenterToken>
{
    public void Derive(in DerivationContext<ShadcnToken> context, ControlView<MenuFlyoutPresenterToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        ShadcnAbstractMenuPopupDerivation.DeriveShared(in context, global, token);
        token.MenuPopupBorderColor = global.ColorBorderSecondary;
        token.MenuPopupBorderThickness = new Thickness();
        token.ItemColor = global.ColorText;
        token.ItemHoverColor = token.ItemColor;
        // The ring plus the menu shadow, and the integrated border in the ring colour.
        token.MenuPopupShadows         = context.Language.GetMenuOverlayRecipe(global).ShadowsWithOutline;
        token.MenuPopupBorderColor     = context.Language.OutlineRing;
        token.MenuPopupBorderThickness = new Thickness(1);
    }
}
