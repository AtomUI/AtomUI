using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Derivation;
using AtomUI.Theme.Languages;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>The AntDesign view of <see cref="MenuToken"/>.</summary>
[ControlTokenRule]
internal sealed class MenuTokenRule : IControlTokenRule<AntDesignToken, MenuToken>
{
    public void Apply(in TokenEvaluationContext<AntDesignToken> context, ControlTokenTarget<MenuToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        MenuPopupTokenCalculator.Calculate(in context, global, token);
        token.ItemColor = global.ColorText;
        token.ItemHoverColor = token.ItemColor;

        var colorTextDisabled  = global.ColorTextDisabled;
        var colorError         = global.ColorError;
        var colorTextSecondary = global.ColorTextQuaternary;
        var colorBgElevated    = global.ColorBgElevated;
        var colorBgTextHover   = global.ColorBgTextHover;
        var padding            = global.UniformlyPadding;
        var controlHeight      = global.ControlHeight;
        var controlHeightSM    = global.ControlHeightSM;
        var controlHeightLG    = global.ControlHeightLG;

        var fontSize   = global.FontSize;
        var fontSizeLG = global.FontSizeLG;



        token.TopLevelItemColor         = global.ColorText;
        token.TopLevelItemSelectedColor = global.ColorTextSecondary;
        token.TopLevelItemHoverColor    = global.ColorTextSecondary;

        // The menu bar paints its own background; its items stay transparent until hovered or opened.
        token.TopLevelItemBg         = Colors.Transparent;
        token.TopLevelItemHoverBg    = colorBgTextHover;
        token.TopLevelItemSelectedBg = colorBgTextHover;

        token.TopLevelItemBorderRadiusSM = global.BorderRadiusSM;
        token.TopLevelItemBorderRadius   = global.BorderRadius;
        token.TopLevelItemBorderRadiusLG = global.BorderRadiusLG;
        token.TopLevelItemFontSize = fontSize;
        token.TopLevelItemFontSizeSM = fontSize;
        token.TopLevelItemFontSizeLG = fontSizeLG;
        token.TopLevelItemLineHeight = CalculatorUtils.CalculateLineHeight(token.TopLevelItemFontSize) * token.TopLevelItemFontSize;
        token.TopLevelItemLineHeightSM = CalculatorUtils.CalculateLineHeight(token.TopLevelItemFontSizeSM) * token.TopLevelItemFontSizeSM;
        token.TopLevelItemLineHeightLG = CalculatorUtils.CalculateLineHeight(token.TopLevelItemFontSizeLG) * token.TopLevelItemFontSizeLG;

        token.TopLevelItemPaddingSM = new Thickness(global.PaddingContentHorizontalXS * 0.7,
            Math.Max((controlHeightSM - token.TopLevelItemLineHeightSM) / 2, 0));
        token.TopLevelItemPadding = new Thickness(global.PaddingContentHorizontalXS,
            Math.Max((controlHeight - token.TopLevelItemLineHeight) / 2, 0));
        token.TopLevelItemPaddingLG = new Thickness(global.PaddingContentHorizontalSM,
            Math.Max((controlHeightLG - token.TopLevelItemLineHeightLG) / 2, 0));

        token.TopLevelItemPopupMarginToAnchor = global.UniformlyMarginXXS;
    }
}
