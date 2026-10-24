using AtomUI.Theme.Algorithms;
using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
internal sealed class MenuToken : AbstractControlDesignToken
{

    public MenuToken()

    {
    }

    /// <summary>
    /// 菜单内容边距
    /// </summary>
    public Thickness MenuPopupContentPadding { get; set; }

    /// <summary>
    /// 菜单 Popup 最小宽度
    /// </summary>
    public double MenuPopupMinWidth { get; set; }

    /// <summary>
    /// 菜单 Popup 最大宽度
    /// </summary>
    public double MenuPopupMaxWidth { get; set; }

    /// <summary>
    /// 历史保留：分离菜单项高度；当前没有公开 tear-off 入口，不参与渲染。
    /// </summary>
    public double MenuTearOffHeight { get; set; }

    /// <summary>
    /// 菜单弹出框背景色
    /// </summary>
    public Color MenuPopupBgColor { get; set; }

    /// <summary>
    /// 菜单项文字颜色
    /// </summary>
    public Color ItemColor { get; set; }

    /// <summary>
    /// 快捷键颜色
    /// </summary>
    public Color KeyGestureColor { get; set; }

    /// <summary>
    /// 菜单项边距
    /// </summary>
    public Thickness ItemMargin { get; set; }

    /// <summary>
    /// 菜单项文字悬浮颜色
    /// </summary>
    public Color ItemHoverColor { get; set; }

    /// <summary>
    /// 菜单项文字禁用颜色
    /// </summary>
    public Color ItemDisabledColor { get; set; }

    /// <summary>
    /// 历史保留：危险菜单项文字颜色；当前 Menu 无危险项入口，不参与渲染。
    /// </summary>
    public Color DangerItemColor { get; set; }

    /// <summary>
    /// 历史保留：危险菜单项悬浮颜色；当前 Menu 无危险项入口，不参与渲染。
    /// </summary>
    public Color DangerItemHoverColor { get; set; }

    /// <summary>
    /// 菜单项背景色
    /// </summary>
    public Color ItemBg { get; set; }

    /// <summary>
    /// 菜单项悬浮态背景色
    /// </summary>
    public Color ItemHoverBg { get; set; }

    /// <summary>
    /// 菜单项高度
    /// </summary>
    public double ItemHeight { get; set; }

    /// <summary>
    /// 图标尺寸
    /// </summary>
    public double ItemIconSize { get; set; }

    /// <summary>
    /// 图标与文字间距
    /// </summary>
    public double ItemIconMarginInlineEnd { get; set; }

    /// <summary>
    /// 菜单项的圆角
    /// </summary>
    public CornerRadius ItemBorderRadius { get; set; }

    /// <summary>
    /// 菜单项横向内间距
    /// </summary>
    public Thickness ItemPaddingInline { get; set; }

    /// <summary>
    /// 顶层菜单项颜色
    /// </summary>
    public Color TopLevelItemColor { get; set; }

    /// <summary>
    /// 顶层菜单项选中颜色
    /// </summary>
    public Color TopLevelItemSelectedColor { get; set; }

    /// <summary>
    /// 顶层菜单项鼠标放上去的颜色
    /// </summary>
    public Color TopLevelItemHoverColor { get; set; }

    /// <summary>
    /// 顶层菜单项背景色
    /// </summary>
    public Color TopLevelItemBg { get; set; }

    /// <summary>
    /// 顶层菜单项选中时背景色
    /// </summary>
    public Color TopLevelItemSelectedBg { get; set; }

    /// <summary>
    /// 顶层菜单项鼠标放上去背景色
    /// </summary>
    public Color TopLevelItemHoverBg { get; set; }

    /// <summary>
    /// 顶层菜单项小号圆角
    /// </summary>
    public CornerRadius TopLevelItemBorderRadiusSM { get; set; }

    /// <summary>
    /// 顶层菜单项圆角
    /// </summary>
    public CornerRadius TopLevelItemBorderRadius { get; set; }

    /// <summary>
    /// 顶层菜单项大号圆角
    /// </summary>
    public CornerRadius TopLevelItemBorderRadiusLG { get; set; }

    /// <summary>
    /// 顶层菜单项小号内间距
    /// </summary>
    public Thickness TopLevelItemPaddingSM { get; set; }

    /// <summary>
    /// 顶层菜单项间距
    /// </summary>
    public Thickness TopLevelItemPadding { get; set; }

    /// <summary>
    /// 顶层菜单项大号内间距
    /// </summary>
    public Thickness TopLevelItemPaddingLG { get; set; }

    /// <summary>
    /// 顶层菜单项小号字体
    /// </summary>
    public double TopLevelItemFontSizeSM { get; set; } = double.NaN;

    /// <summary>
    /// 顶层菜单项字体
    /// </summary>
    public double TopLevelItemFontSize { get; set; } = double.NaN;

    /// <summary>
    /// 顶层菜单项大号字体
    /// </summary>
    public double TopLevelItemFontSizeLG { get; set; } = double.NaN;

    /// <summary>
    /// 顶层菜单项内容字体行高
    /// </summary>
    public double TopLevelItemLineHeight { get; set; } = double.NaN;

    /// <summary>
    /// 大号顶层菜单项内容字体行高
    /// </summary>
    public double TopLevelItemLineHeightLG { get; set; } = double.NaN;

    /// <summary>
    /// 小号顶层菜单项内容字体行高
    /// </summary>
    public double TopLevelItemLineHeightSM { get; set; } = double.NaN;

    /// <summary>
    /// 顶层弹出菜单，距离顶层菜单项的边距
    /// </summary>
    public double TopLevelItemPopupMarginToAnchor { get; set; }

    /// <summary>
    /// 菜单分割项的高度
    /// </summary>
    public double SeparatorItemHeight { get; set; }

    /// <summary>
    /// 上下文菜单水平位移
    /// </summary>
    public double ContextMenuOffsetX { get; set; }

    /// <summary>
    /// 上下文菜单垂直位移
    /// </summary>
    public double ContextMenuOffsetY { get; set; }

    public override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);

        var colorTextDisabled  = EffectiveGlobalToken.ColorTextDisabled;
        var colorError         = EffectiveGlobalToken.ColorError;
        var colorTextSecondary = EffectiveGlobalToken.ColorTextQuaternary;
        var colorBgContainer   = EffectiveGlobalToken.ColorBgContainer;
        var colorBgElevated    = EffectiveGlobalToken.ColorBgElevated;
        var colorBgTextHover   = EffectiveGlobalToken.ColorBgTextHover;
        var padding            = EffectiveGlobalToken.UniformlyPadding;
        var controlHeight      = EffectiveGlobalToken.ControlHeight;
        var controlHeightSM    = EffectiveGlobalToken.ControlHeightSM;
        var controlHeightLG    = EffectiveGlobalToken.ControlHeightLG;

        var fontSize   = EffectiveGlobalToken.FontSize;
        var fontSizeLG = EffectiveGlobalToken.FontSizeLG;

        KeyGestureColor  = colorTextSecondary;
        ItemBorderRadius = EffectiveGlobalToken.BorderRadius;
        ItemColor        = EffectiveGlobalToken.ColorText;
        ItemHoverColor   = ItemColor;
        ItemBg           = colorBgElevated;
        ItemHoverBg      = colorBgTextHover;
        ItemMargin       = new Thickness(0, 0, EffectiveGlobalToken.UniformlyMarginXXS, 0);

        ItemDisabledColor = colorTextDisabled;

        DangerItemColor      = colorError;
        DangerItemHoverColor = colorError;

        ItemHeight       = controlHeight;
        MenuPopupBgColor = EffectiveGlobalToken.ColorBgElevated;

        ItemPaddingInline       = new Thickness(padding, EffectiveGlobalToken.UniformlyPaddingXXS);
        ItemIconSize            = EffectiveGlobalToken.IconSize;
        ItemIconMarginInlineEnd = controlHeight - fontSize;

        TopLevelItemColor         = EffectiveGlobalToken.ColorText;
        TopLevelItemSelectedColor = EffectiveGlobalToken.ColorTextSecondary;
        TopLevelItemHoverColor    = EffectiveGlobalToken.ColorTextSecondary;

        TopLevelItemBg         = colorBgContainer;
        TopLevelItemHoverBg    = colorBgTextHover;
        TopLevelItemSelectedBg = colorBgTextHover;

        TopLevelItemBorderRadiusSM = EffectiveGlobalToken.BorderRadiusSM;
        TopLevelItemBorderRadius   = EffectiveGlobalToken.BorderRadius;
        TopLevelItemBorderRadiusLG = EffectiveGlobalToken.BorderRadiusLG;

        TopLevelItemFontSize   = !double.IsNaN(TopLevelItemFontSize) ? TopLevelItemFontSize : fontSize;
        TopLevelItemFontSizeSM = !double.IsNaN(TopLevelItemFontSizeSM) ? TopLevelItemFontSizeSM : fontSize;
        TopLevelItemFontSizeLG = !double.IsNaN(TopLevelItemFontSizeLG) ? TopLevelItemFontSizeLG : fontSizeLG;

        TopLevelItemLineHeight = !double.IsNaN(TopLevelItemLineHeight)
            ? TopLevelItemLineHeight
            : CalculatorUtils.CalculateLineHeight(TopLevelItemFontSize) * TopLevelItemFontSize;
        TopLevelItemLineHeightSM = !double.IsNaN(TopLevelItemLineHeightSM)
            ? TopLevelItemLineHeightSM
            : CalculatorUtils.CalculateLineHeight(TopLevelItemFontSizeSM) * TopLevelItemFontSizeSM;
        TopLevelItemLineHeightLG = !double.IsNaN(TopLevelItemLineHeightLG)
            ? TopLevelItemLineHeightLG
            : CalculatorUtils.CalculateLineHeight(TopLevelItemFontSizeLG) * TopLevelItemFontSizeLG;

        TopLevelItemPaddingSM = new Thickness(EffectiveGlobalToken.PaddingContentHorizontalXS * 0.7,
            Math.Max((controlHeightSM - TopLevelItemLineHeightSM) / 2, 0));
        TopLevelItemPadding = new Thickness(EffectiveGlobalToken.PaddingContentHorizontalXS,
            Math.Max((controlHeight - TopLevelItemLineHeight) / 2, 0));
        TopLevelItemPaddingLG = new Thickness(EffectiveGlobalToken.PaddingContentHorizontalSM,
            Math.Max((controlHeightLG - TopLevelItemLineHeightLG) / 2, 0));

        TopLevelItemPopupMarginToAnchor = EffectiveGlobalToken.UniformlyMarginXXS;

        MenuPopupMinWidth = 120;
        MenuPopupMaxWidth = 800;

        SeparatorItemHeight = EffectiveGlobalToken.LineWidth * 5;
        MenuTearOffHeight   = ItemHeight * 1.2;

        MenuPopupContentPadding =
            new Thickness(EffectiveGlobalToken.UniformlyPaddingXXS, EffectiveGlobalToken.BorderRadiusLG.TopLeft / 2);

        ContextMenuOffsetX = EffectiveGlobalToken.SpacingXXS;
        ContextMenuOffsetY = EffectiveGlobalToken.SpacingXXS;
    }

}
