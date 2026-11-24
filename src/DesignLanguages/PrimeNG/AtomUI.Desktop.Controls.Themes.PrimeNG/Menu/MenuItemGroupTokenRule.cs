using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG view of <see cref="MenuItemGroupToken"/>.</summary>
[ControlTokenRule]
internal sealed class MenuItemGroupTokenRule : IControlTokenRule<PrimeNGToken, MenuItemGroupToken>
{
    public void Apply(in TokenEvaluationContext<PrimeNGToken> context, ControlTokenTarget<MenuItemGroupToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        // PrimeNG: the control-global overrides, read by the formulas below.
        PrimeNGMenuItemGroupTokens.EvaluateGlobal(in context, global);
        token.TitleFontWeight = FontWeight.Normal;
        // PrimeNG: the preset's control values, over the formulas.
        PrimeNGMenuItemGroupTokens.EvaluateOwn(in context, global, token);
    }
}
