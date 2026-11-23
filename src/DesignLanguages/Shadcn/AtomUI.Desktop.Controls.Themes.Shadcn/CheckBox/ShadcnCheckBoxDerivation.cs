using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>The shadcn view of <see cref="CheckBoxToken"/>.</summary>
[ControlDerivation]
internal sealed class ShadcnCheckBoxDerivation : IControlDerivation<ShadcnToken, CheckBoxToken>
{
    public void Derive(in DerivationContext<ShadcnToken> context, ControlView<CheckBoxToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        // The indicator is a 16px square with the style's own small radius (rounded-[4px] Vega/Nova/Mira, [5px] Luma/Rhea,
        // [6px] Maia, none Lyra/Sera); the template binds it to the control-scope BorderRadiusSM. The button radius ladder
        // would collapse a 26px radius into a circle.
        global.BorderRadiusSM = new CornerRadius(context.Language.CheckBoxRadius);
        if (context.Variant.IsDark)
        {
            // The indicator uses this control-scoped token for the check mark.
            global.ColorWhite = global.ColorTextLightSolid;
        }
        token.CheckIndicatorSize        = global.ControlInteractiveSize;

        token.CheckedMarkSize           = token.CheckIndicatorSize * 0.6;
        token.IndicatorTristateMarkSize = global.FontSizeLG / 2;
        token.IndicatorHoverBorderColor = global.ColorPrimaryHover;
        token.TextMargin                = new Thickness(global.UniformlyMarginXS, 0, global.UniformlyMarginXS, 0);
    }
}
