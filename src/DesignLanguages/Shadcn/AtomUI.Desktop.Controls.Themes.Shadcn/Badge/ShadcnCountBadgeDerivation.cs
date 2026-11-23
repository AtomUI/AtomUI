using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>The shadcn view of <see cref="CountBadgeToken"/>.</summary>
[ControlDerivation]
internal sealed class ShadcnCountBadgeDerivation : IControlDerivation<ShadcnToken, CountBadgeToken>
{
    public void Derive(in DerivationContext<ShadcnToken> context, ControlView<CountBadgeToken> view)
    {
        var token  = view.Token;
        var global = view.Global;

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
    }
}
