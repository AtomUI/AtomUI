using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>
/// The shadcn view of <see cref="MenuFlyoutPresenterToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class MenuFlyoutPresenterTokenRule : IControlTokenRule<ShadcnToken, MenuFlyoutPresenterToken>
{
    public void Apply(in TokenEvaluationContext<ShadcnToken> context, ControlTokenTarget<MenuFlyoutPresenterToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        MenuPopupTokenCalculator.Calculate(in context, global, token);
        token.MenuPopupBorderColor = global.ColorBorderSecondary;
        token.MenuPopupBorderThickness = new Thickness();
        token.ItemColor = global.ColorText;
        token.ItemHoverColor = token.ItemColor;
        // The ring plus the menu shadow, and the integrated border in the ring color.
        token.MenuPopupShadows         = context.Language.GetMenuOverlayRecipe(global).ShadowsWithOutline;
        token.MenuPopupBorderColor     = context.Language.OutlineRing;
        token.MenuPopupBorderThickness = new Thickness(1);
    }
}
