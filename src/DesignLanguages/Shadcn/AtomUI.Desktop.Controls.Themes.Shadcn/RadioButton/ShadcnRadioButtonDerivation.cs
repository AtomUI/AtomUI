using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>The shadcn view of <see cref="RadioButtonToken"/>.</summary>
[ControlDerivation]
internal sealed class ShadcnRadioButtonDerivation : IControlDerivation<ShadcnToken, RadioButtonToken>
{
    public void Derive(in DerivationContext<ShadcnToken> context, ControlView<RadioButtonToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        var lineWidth        = global.LineWidth;
        var fontSizeLG       = global.FontSizeLG;
        var colorPrimary     = global.ColorPrimary;
        var colorWhite       = global.ColorWhite;

        var dotPadding = 4; // 魔术值，需要看有没有好办法消除
        var radioSize  = fontSizeLG;

        var radioDotSize = radioSize - (dotPadding + lineWidth) * 2;

        token.DotPadding       = dotPadding;
        token.RadioSize        = radioSize;
        token.DotSize          = radioDotSize;
        token.DotColorDisabled = global.ColorTextDisabled;

        // internal
        token.RadioColor   = colorWhite;
        token.RadioBgColor = colorPrimary;
        token.IndicatorHoverBorderColor = colorPrimary;
        token.TextMargin   = new Thickness(global.UniformlyMarginXS, 0, global.UniformlyMarginXS, 0);
        if (context.Variant.IsDark)
        {
            // The selected dot uses the dark style's foreground paired with the primary fill.
            token.RadioColor = global.ColorTextLightSolid;
        }
    }
}
