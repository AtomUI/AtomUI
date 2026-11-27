using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>
/// The shadcn view of <see cref="PopupToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class PopupTokenRule : IControlTokenRule<ShadcnToken, PopupToken>
{
    public void Apply(in TokenEvaluationContext<ShadcnToken> context, ControlTokenTarget<PopupToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        PopupCornerTokenCalculator.Calculate(in context, global, token);

        token.OverlayHostShadow = global.BoxShadowsSecondary;
        // Window-style popups and overlay popups share one shadow. The placement is compensated for the residual
        // shadow, so the position of the content relative to the anchor does not depend on the shadow thickness.
        token.PopupRootShadow = global.BoxShadowsSecondary;
        token.MarginToAnchor = global.UniformlyMarginXXS;
        // Overlays without an arrow (menus, select and the other input panels): the outline ring ahead of the menu shadow,
        // the radius of the overlay table.
        var menu = context.Language.GetMenuOverlayRecipe(global);
        var popupShadows = menu.ShadowsWithOutline;
        token.PopupRootShadow   = popupShadows;
        token.OverlayHostShadow = popupShadows;
        token.PopupCornerRadius = menu.CornerRadius;
    }
}
