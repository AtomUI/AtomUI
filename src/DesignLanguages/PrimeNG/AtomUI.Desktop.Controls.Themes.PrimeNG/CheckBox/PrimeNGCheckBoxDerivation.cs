using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG view of <see cref="CheckBoxToken"/>.</summary>
[ControlDerivation]
internal sealed class PrimeNGCheckBoxDerivation : IControlDerivation<PrimeNGToken, CheckBoxToken>
{
    public void Derive(in DerivationContext<PrimeNGToken> context, ControlView<CheckBoxToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        // PrimeNG: the control-global overrides, read by the formulas below.
        PrimeNGCheckBoxTokens.EvaluateGlobal(in context, global);
        token.CheckIndicatorSize        = global.ControlInteractiveSize;

        token.CheckedMarkSize           = token.CheckIndicatorSize * 0.6;
        token.IndicatorTristateMarkSize = global.FontSizeLG / 2;
        token.IndicatorHoverBorderColor = global.ColorPrimaryHover;
        token.TextMargin                = new Thickness(global.UniformlyMarginXS, 0, global.UniformlyMarginXS, 0);
        // PrimeNG: the preset's control values, over the formulas.
        PrimeNGCheckBoxTokens.EvaluateOwn(in context, global, token);
    }
}
