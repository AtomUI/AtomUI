using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>The shadcn view of <see cref="PopupToken"/>.</summary>
[ControlTokenRule]
internal sealed class PopupTokenRule : IControlTokenRule<ShadcnToken, PopupToken>
{
    public void Apply(in TokenEvaluationContext<ShadcnToken> context, ControlTokenTarget<PopupToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        PopupCornerTokenCalculator.Calculate(in context, global, token);

        token.OverlayHostShadow = global.BoxShadowsSecondary;
        // 窗口型与 Overlay Popup 使用同一阴影；定位按残余阴影补偿，内容相对 anchor 的位置与阴影厚度无关
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
