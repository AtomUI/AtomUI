using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class MenuToken : AbstractMenuPopupToken
{

    public MenuToken()

    {
    }




















    /// <summary>
    /// 顶层菜单项颜色
    /// </summary>
    public partial Color TopLevelItemColor { get; set; }

    /// <summary>
    /// 顶层菜单项选中颜色
    /// </summary>
    public partial Color TopLevelItemSelectedColor { get; set; }

    /// <summary>
    /// 顶层菜单项鼠标放上去的颜色
    /// </summary>
    public partial Color TopLevelItemHoverColor { get; set; }

    /// <summary>
    /// 顶层菜单项背景色
    /// </summary>
    public partial Color TopLevelItemBg { get; set; }

    /// <summary>
    /// 顶层菜单项选中时背景色
    /// </summary>
    public partial Color TopLevelItemSelectedBg { get; set; }

    /// <summary>
    /// 顶层菜单项鼠标放上去背景色
    /// </summary>
    public partial Color TopLevelItemHoverBg { get; set; }

    /// <summary>
    /// 顶层菜单项小号圆角
    /// </summary>
    public partial CornerRadius TopLevelItemBorderRadiusSM { get; set; }

    /// <summary>
    /// 顶层菜单项圆角
    /// </summary>
    public partial CornerRadius TopLevelItemBorderRadius { get; set; }

    /// <summary>
    /// 顶层菜单项大号圆角
    /// </summary>
    public partial CornerRadius TopLevelItemBorderRadiusLG { get; set; }

    /// <summary>
    /// 顶层菜单项小号内间距
    /// </summary>
    public partial Thickness TopLevelItemPaddingSM { get; set; }

    /// <summary>
    /// 顶层菜单项间距
    /// </summary>
    public partial Thickness TopLevelItemPadding { get; set; }

    /// <summary>
    /// 顶层菜单项大号内间距
    /// </summary>
    public partial Thickness TopLevelItemPaddingLG { get; set; }

    /// <summary>
    /// 顶层菜单项小号字体
    /// </summary>
    public partial double TopLevelItemFontSizeSM { get; set; }

    /// <summary>
    /// 顶层菜单项字体
    /// </summary>
    public partial double TopLevelItemFontSize { get; set; }

    /// <summary>
    /// 顶层菜单项大号字体
    /// </summary>
    public partial double TopLevelItemFontSizeLG { get; set; }

    /// <summary>
    /// 顶层菜单项内容字体行高
    /// </summary>
    public partial double TopLevelItemLineHeight { get; set; }

    /// <summary>
    /// 大号顶层菜单项内容字体行高
    /// </summary>
    public partial double TopLevelItemLineHeightLG { get; set; }

    /// <summary>
    /// 小号顶层菜单项内容字体行高
    /// </summary>
    public partial double TopLevelItemLineHeightSM { get; set; }

    /// <summary>
    /// 顶层弹出菜单，距离顶层菜单项的边距
    /// </summary>
    public partial double TopLevelItemPopupMarginToAnchor { get; set; }




    protected override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        if (!IsExplicit(MenuTokenKind.ItemColor))
        {
            ItemColor = EffectiveGlobalToken.ColorText;
        }
        ItemHoverColor = ItemColor;

        var colorTextDisabled  = EffectiveGlobalToken.ColorTextDisabled;
        var colorError         = EffectiveGlobalToken.ColorError;
        var colorTextSecondary = EffectiveGlobalToken.ColorTextQuaternary;
        var colorBgElevated    = EffectiveGlobalToken.ColorBgElevated;
        var colorBgTextHover   = EffectiveGlobalToken.ColorBgTextHover;
        var padding            = EffectiveGlobalToken.UniformlyPadding;
        var controlHeight      = EffectiveGlobalToken.ControlHeight;
        var controlHeightSM    = EffectiveGlobalToken.ControlHeightSM;
        var controlHeightLG    = EffectiveGlobalToken.ControlHeightLG;

        var fontSize   = EffectiveGlobalToken.FontSize;
        var fontSizeLG = EffectiveGlobalToken.FontSizeLG;



        TopLevelItemColor         = EffectiveGlobalToken.ColorText;
        TopLevelItemSelectedColor = EffectiveGlobalToken.ColorTextSecondary;
        TopLevelItemHoverColor    = EffectiveGlobalToken.ColorTextSecondary;

        // The menu bar paints its own background; its items stay transparent until hovered or opened.
        TopLevelItemBg         = Colors.Transparent;
        TopLevelItemHoverBg    = colorBgTextHover;
        TopLevelItemSelectedBg = colorBgTextHover;

        TopLevelItemBorderRadiusSM = EffectiveGlobalToken.BorderRadiusSM;
        TopLevelItemBorderRadius   = EffectiveGlobalToken.BorderRadius;
        TopLevelItemBorderRadiusLG = EffectiveGlobalToken.BorderRadiusLG;

        if (!IsExplicit(MenuTokenKind.TopLevelItemFontSize))
        {
            TopLevelItemFontSize = fontSize;
        }

        if (!IsExplicit(MenuTokenKind.TopLevelItemFontSizeSM))
        {
            TopLevelItemFontSizeSM = fontSize;
        }

        if (!IsExplicit(MenuTokenKind.TopLevelItemFontSizeLG))
        {
            TopLevelItemFontSizeLG = fontSizeLG;
        }

        if (!IsExplicit(MenuTokenKind.TopLevelItemLineHeight))
        {
            TopLevelItemLineHeight = CalculatorUtils.CalculateLineHeight(TopLevelItemFontSize) * TopLevelItemFontSize;
        }

        if (!IsExplicit(MenuTokenKind.TopLevelItemLineHeightSM))
        {
            TopLevelItemLineHeightSM = CalculatorUtils.CalculateLineHeight(TopLevelItemFontSizeSM) * TopLevelItemFontSizeSM;
        }

        if (!IsExplicit(MenuTokenKind.TopLevelItemLineHeightLG))
        {
            TopLevelItemLineHeightLG = CalculatorUtils.CalculateLineHeight(TopLevelItemFontSizeLG) * TopLevelItemFontSizeLG;
        }

        TopLevelItemPaddingSM = new Thickness(EffectiveGlobalToken.PaddingContentHorizontalXS * 0.7,
            Math.Max((controlHeightSM - TopLevelItemLineHeightSM) / 2, 0));
        TopLevelItemPadding = new Thickness(EffectiveGlobalToken.PaddingContentHorizontalXS,
            Math.Max((controlHeight - TopLevelItemLineHeight) / 2, 0));
        TopLevelItemPaddingLG = new Thickness(EffectiveGlobalToken.PaddingContentHorizontalSM,
            Math.Max((controlHeightLG - TopLevelItemLineHeightLG) / 2, 0));

        TopLevelItemPopupMarginToAnchor = EffectiveGlobalToken.UniformlyMarginXXS;

    }

}
