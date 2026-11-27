using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Derivation;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>
/// The AntDesign view of <see cref="HyperLinkButtonToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class HyperLinkButtonTokenRule : IControlTokenRule<AntDesignToken, HyperLinkButtonToken>
{
    public void Apply(in TokenEvaluationContext<AntDesignToken> context, ControlTokenTarget<HyperLinkButtonToken> target)
    {
        var token  = target.Own;
        var global = target.Global;

        var fontSize = global.FontSize;
        var fontSizeLG = global.FontSizeLG;
        token.ContentFontSize = fontSize;
        token.ContentFontSizeSM = fontSize;
        token.ContentFontSizeLG = fontSizeLG;
        token.ContentLineHeight = CalculateLineHeight(token.ContentFontSize);
        token.ContentLineHeightSM = CalculateLineHeight(token.ContentFontSizeSM);
        token.ContentLineHeightLG = CalculateLineHeight(token.ContentFontSizeLG);

        var lineWidth = global.LineWidth;
        var controlHeightSM = global.ControlHeightSM;
        var controlHeight = global.ControlHeight;
        var controlHeightLG = global.ControlHeightLG;
        token.ContentPaddingSM = new Thickness(
            8 - lineWidth,
            Math.Max((controlHeightSM - token.ContentLineHeightSM) / 2 - lineWidth, 0));
        token.ContentPadding = new Thickness(
            global.PaddingContentHorizontal - lineWidth,
            Math.Max((controlHeight - token.ContentLineHeight) / 2 - lineWidth, 0));
        token.ContentPaddingLG = new Thickness(
            global.PaddingContentHorizontal - lineWidth,
            Math.Max((controlHeightLG - token.ContentLineHeightLG) / 2 - lineWidth, 0));

        token.OnlyIconSizeSM = global.IconSize;
        token.OnlyIconSize = global.IconSizeLG;
        token.OnlyIconSizeLG = global.IconSizeLG;
        token.IconMargin = new Thickness(0, 0, global.UniformlyPaddingXXS, 0);
        token.IconOnyPadding = UniformIconPadding(controlHeight, token.ContentLineHeight, lineWidth);
        token.IconOnyPaddingLG = UniformIconPadding(controlHeightLG, token.ContentLineHeightLG, lineWidth);
        token.IconOnyPaddingSM = UniformIconPadding(controlHeightSM, token.ContentLineHeightSM, lineWidth);
    }


    private static double CalculateLineHeight(double fontSize)
    {
        return CalculatorUtils.CalculateLineHeight(fontSize) * fontSize;
    }


    private static Thickness UniformIconPadding(double height, double lineHeight, double lineWidth)
    {
        return new Thickness(Math.Max((height - lineHeight) / 2 - lineWidth, 0));
    }
}
