using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>
/// The AntDesign view of <see cref="RadioButtonToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class RadioButtonTokenRule : IControlTokenRule<AntDesignToken, RadioButtonToken>
{
    public void Apply(in TokenEvaluationContext<AntDesignToken> context, ControlTokenTarget<RadioButtonToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        var lineWidth        = global.LineWidth;
        var fontSizeLG       = global.FontSizeLG;
        var colorPrimary     = global.ColorPrimary;
        var colorWhite       = global.ColorWhite;

        var dotPadding = 4; // Magic value; see whether there is a good way to remove it.
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
    }
}
