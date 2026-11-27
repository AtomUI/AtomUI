using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>
/// The shadcn view of <see cref="ContextMenuToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class ContextMenuTokenRule : IControlTokenRule<ShadcnToken, ContextMenuToken>
{
    public void Apply(in TokenEvaluationContext<ShadcnToken> context, ControlTokenTarget<ContextMenuToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        MenuPopupTokenCalculator.Calculate(in context, global, token);
        token.ItemColor = global.ColorText;
        token.ItemHoverColor = token.ItemColor;
        token.ContextMenuOffsetX = global.SpacingXXS;
        token.ContextMenuOffsetY = global.SpacingXXS;
        // registry context-menu: the outline ring ahead of the menu shadow of the style.
        token.MenuPopupShadows = context.Language.GetMenuOverlayRecipe(global).ShadowsWithOutline;
    }
}
