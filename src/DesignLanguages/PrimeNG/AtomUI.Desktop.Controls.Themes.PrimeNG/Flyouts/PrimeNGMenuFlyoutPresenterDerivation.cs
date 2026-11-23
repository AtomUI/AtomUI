using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG view of <see cref="MenuFlyoutPresenterToken"/>.</summary>
[ControlDerivation]
internal sealed class PrimeNGMenuFlyoutPresenterDerivation : IControlDerivation<PrimeNGToken, MenuFlyoutPresenterToken>
{
    public void Derive(in DerivationContext<PrimeNGToken> context, ControlView<MenuFlyoutPresenterToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        // PrimeNG: the control-global overrides, read by the formulas below.
        PrimeNGMenuFlyoutPresenterTokens.EvaluateGlobal(in context, global);
        PrimeNGAbstractMenuPopupDerivation.DeriveShared(in context, global, token);
        token.MenuPopupBorderColor = global.ColorBorderSecondary;
        token.MenuPopupBorderThickness = new Thickness();
        token.ItemColor = global.ColorText;
        token.ItemHoverColor = token.ItemColor;
        // PrimeNG: the preset's control values, over the formulas.
        PrimeNGMenuFlyoutPresenterTokens.EvaluateOwn(in context, global, token);
    }
}
