using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG view of <see cref="MenuItemGroupToken"/>.</summary>
[ControlDerivation]
internal sealed class PrimeNGMenuItemGroupDerivation : IControlDerivation<PrimeNGToken, MenuItemGroupToken>
{
    public void Derive(in DerivationContext<PrimeNGToken> context, ControlView<MenuItemGroupToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        // PrimeNG: the control-global overrides, read by the formulas below.
        PrimeNGMenuItemGroupTokens.EvaluateGlobal(in context, global);
        token.TitleFontWeight = FontWeight.Normal;
        // PrimeNG: the preset's control values, over the formulas.
        PrimeNGMenuItemGroupTokens.EvaluateOwn(in context, global, token);
    }
}
