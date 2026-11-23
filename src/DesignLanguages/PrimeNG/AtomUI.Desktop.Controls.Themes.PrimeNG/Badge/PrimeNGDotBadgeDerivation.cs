using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG view of <see cref="DotBadgeToken"/>.</summary>
[ControlDerivation]
internal sealed class PrimeNGDotBadgeDerivation : IControlDerivation<PrimeNGToken, DotBadgeToken>
{
    public void Derive(in DerivationContext<PrimeNGToken> context, ControlView<DotBadgeToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        // PrimeNG: the control-global overrides, read by the formulas below.
        PrimeNGDotBadgeTokens.EvaluateGlobal(in context, global);

        token.DotSize     = global.FontSizeSM / 2;
        token.DotColor    = global.ColorError;
        token.ShadowSize  = global.LineWidth;
        token.ShadowColor = global.ColorBorderBg;
        token.LabelMargin = new Thickness(global.UniformlyMarginXS, 0, 0, 0);
        // PrimeNG: the preset's control values, over the formulas.
        PrimeNGDotBadgeTokens.EvaluateOwn(in context, global, token);
    }
}
