using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG view of <see cref="RadioButtonToken"/>.</summary>
[ControlTokenRule]
internal sealed class RadioButtonTokenRule : IControlTokenRule<PrimeNGToken, RadioButtonToken>
{
    public void Apply(in TokenEvaluationContext<PrimeNGToken> context, ControlTokenTarget<RadioButtonToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        // PrimeNG: the control-global overrides, read by the formulas below.
        PrimeNGRadioButtonTokens.EvaluateGlobal(in context, global);
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
        // PrimeNG: the preset's control values, over the formulas.
        PrimeNGRadioButtonTokens.EvaluateOwn(in context, global, token);
    }
}
