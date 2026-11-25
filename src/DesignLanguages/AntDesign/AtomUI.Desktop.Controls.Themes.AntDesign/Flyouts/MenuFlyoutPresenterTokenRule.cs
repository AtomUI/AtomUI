using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>The AntDesign view of <see cref="MenuFlyoutPresenterToken"/>.</summary>
[ControlTokenRule]
internal sealed class MenuFlyoutPresenterTokenRule : IControlTokenRule<AntDesignToken, MenuFlyoutPresenterToken>
{
    public void Apply(in TokenEvaluationContext<AntDesignToken> context, ControlTokenTarget<MenuFlyoutPresenterToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        MenuPopupTokenCalculator.Calculate(in context, global, token);
        token.MenuPopupBorderColor = global.ColorBorderSecondary;
        token.MenuPopupBorderThickness = new Thickness();
        token.ItemColor = global.ColorText;
        token.ItemHoverColor = token.ItemColor;
    }
}
