using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>
/// The PrimeNG view of <see cref="MenuFlyoutPresenterToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class MenuFlyoutPresenterTokenRule : IControlTokenRule<PrimeNGToken, MenuFlyoutPresenterToken>
{
    public void Apply(in TokenEvaluationContext<PrimeNGToken> context, ControlTokenTarget<MenuFlyoutPresenterToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        // PrimeNG: no control-global overrides; EvaluateGlobal writes nothing.
        PrimeNGMenuFlyoutPresenterTokens.EvaluateGlobal(in context, global);
        MenuPopupTokenCalculator.Calculate(in context, global, token);
        token.MenuPopupBorderColor = global.ColorBorderSecondary;
        token.MenuPopupBorderThickness = new Thickness();
        token.ItemColor = global.ColorText;
        token.ItemHoverColor = token.ItemColor;
        // PrimeNG: the preset's control values, over the formulas.
        PrimeNGMenuFlyoutPresenterTokens.EvaluateOwn(in context, global, token);
    }
}
