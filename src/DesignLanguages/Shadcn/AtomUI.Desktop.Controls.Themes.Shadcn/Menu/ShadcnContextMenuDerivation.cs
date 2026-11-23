using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>The shadcn view of <see cref="ContextMenuToken"/>.</summary>
[ControlDerivation]
internal sealed class ShadcnContextMenuDerivation : IControlDerivation<ShadcnToken, ContextMenuToken>
{
    public void Derive(in DerivationContext<ShadcnToken> context, ControlView<ContextMenuToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        ShadcnAbstractMenuPopupDerivation.DeriveShared(in context, global, token);
        token.ItemColor = global.ColorText;
        token.ItemHoverColor = token.ItemColor;
        token.ContextMenuOffsetX = global.SpacingXXS;
        token.ContextMenuOffsetY = global.SpacingXXS;
        token.MenuPopupShadows = context.Language.GetMenuOverlayRecipe(global).ShadowsWithOutline;
    }
}
