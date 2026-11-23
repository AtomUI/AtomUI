using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG view of <see cref="CountBadgeToken"/>.</summary>
[ControlDerivation]
internal sealed class PrimeNGCountBadgeDerivation : IControlDerivation<PrimeNGToken, CountBadgeToken>
{
    public void Derive(in DerivationContext<PrimeNGToken> context, ControlView<CountBadgeToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        // PrimeNG: the control-global overrides, read by the formulas below.
        PrimeNGCountBadgeTokens.EvaluateGlobal(in context, global);

        var lineWidth = global.LineWidth;
        token.IndicatorHeight = Math.Round(global.FontSize * global.RelativeLineHeight) -
                              2 * lineWidth;
        token.IndicatorHeightSM = global.FontSize;

        token.TextFontSize       = global.FontSizeSM;
        token.TextFontSizeSM     = global.FontSizeSM - 2;
        token.TextFontWeight     = FontWeight.Normal;
        token.TextColor          = global.ColorTextLightSolid;
        token.IndicatorColor     = global.ColorError;
        token.ShadowSize         = lineWidth;
        token.ShadowColor        = global.ColorBorderBg;
        token.TextPadding        = new Thickness(global.UniformlyPaddingXXS, 0);
        token.CornerRadius       = new CornerRadius(token.IndicatorHeight);
        token.CornerRadiusSM     = new CornerRadius(token.IndicatorHeightSM);
        // PrimeNG: the preset's control values, over the formulas.
        PrimeNGCountBadgeTokens.EvaluateOwn(in context, global, token);
    }
}
