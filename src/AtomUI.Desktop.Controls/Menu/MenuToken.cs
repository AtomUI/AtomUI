using AtomUI.Theme.Algorithms;
using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken(ControlTokenEvaluationVersion.DependencyDefaultsV2, Revision = 1)]
internal sealed partial class MenuToken : AbstractControlDesignToken, IControlTokenDefaults
{

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
    /// Historical compatibility field; built-in icon spacing is owned by ItemMargin. No alias or active consumer.
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

    [ControlTokenDefault(nameof(MenuPopupContentPadding))]
    private static Thickness DefaultForMenuPopupContentPadding(in ControlTokenDefaultContext context) =>
        new Thickness(context.Global.UniformlyPaddingXXS, context.Global.BorderRadiusLG.TopLeft / 2);

    [ControlTokenDefault(nameof(MenuPopupMinWidth))]
    private static double DefaultForMenuPopupMinWidth(in ControlTokenDefaultContext context) =>
        120;

    [ControlTokenDefault(nameof(MenuPopupMaxWidth))]
    private static double DefaultForMenuPopupMaxWidth(in ControlTokenDefaultContext context) =>
        800;

    [ControlTokenDefault(nameof(MenuTearOffHeight), nameof(ItemHeight))]
    private static double DefaultForMenuTearOffHeight(in ControlTokenDefaultContext context, double itemHeight) =>
        itemHeight * 1.2;

    [ControlTokenDefault(nameof(MenuPopupBgColor))]
    private static Color DefaultForMenuPopupBgColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorBgElevated;

    [ControlTokenDefault(nameof(ItemColor))]
    private static Color DefaultForItemColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorText;

    [ControlTokenDefault(nameof(KeyGestureColor))]
    private static Color DefaultForKeyGestureColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorTextQuaternary;

    [ControlTokenDefault(nameof(ItemMargin))]
    private static Thickness DefaultForItemMargin(in ControlTokenDefaultContext context) =>
        new Thickness(0, 0, context.Global.UniformlyMarginXXS, 0);

    [ControlTokenDefault(nameof(ItemHoverColor), nameof(ItemColor))]
    private static Color DefaultForItemHoverColor(in ControlTokenDefaultContext context, Color itemColor) =>
        itemColor;

    [ControlTokenDefault(nameof(ItemDisabledColor))]
    private static Color DefaultForItemDisabledColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorTextDisabled;

    [ControlTokenDefault(nameof(DangerItemColor))]
    private static Color DefaultForDangerItemColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorError;

    [ControlTokenDefault(nameof(DangerItemHoverColor))]
    private static Color DefaultForDangerItemHoverColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorError;

    [ControlTokenDefault(nameof(ItemBg))]
    private static Color DefaultForItemBg(in ControlTokenDefaultContext context) =>
        context.Global.ColorBgElevated;

    [ControlTokenDefault(nameof(ItemHoverBg))]
    private static Color DefaultForItemHoverBg(in ControlTokenDefaultContext context) =>
        context.Global.ColorBgTextHover;

    [ControlTokenDefault(nameof(ItemHeight))]
    private static double DefaultForItemHeight(in ControlTokenDefaultContext context) =>
        context.Global.ControlHeight;

    [ControlTokenDefault(nameof(ItemIconSize))]
    private static double DefaultForItemIconSize(in ControlTokenDefaultContext context) =>
        context.Global.IconSize;

    [ControlTokenDefault(nameof(ItemIconMarginInlineEnd))]
    private static double DefaultForItemIconMarginInlineEnd(in ControlTokenDefaultContext context) =>
        context.Global.ControlHeight - context.Global.FontSize;

    [ControlTokenDefault(nameof(ItemBorderRadius))]
    private static CornerRadius DefaultForItemBorderRadius(in ControlTokenDefaultContext context) =>
        context.Global.BorderRadius;

    [ControlTokenDefault(nameof(ItemPaddingInline))]
    private static Thickness DefaultForItemPaddingInline(in ControlTokenDefaultContext context) =>
        new Thickness(context.Global.UniformlyPadding, context.Global.UniformlyPaddingXXS);

    [ControlTokenDefault(nameof(TopLevelItemColor))]
    private static Color DefaultForTopLevelItemColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorText;

    [ControlTokenDefault(nameof(TopLevelItemSelectedColor))]
    private static Color DefaultForTopLevelItemSelectedColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorTextSecondary;

    [ControlTokenDefault(nameof(TopLevelItemHoverColor))]
    private static Color DefaultForTopLevelItemHoverColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorTextSecondary;

    [ControlTokenDefault(nameof(TopLevelItemBg))]
    private static Color DefaultForTopLevelItemBg(in ControlTokenDefaultContext context) =>
        context.Global.ColorBgContainer;

    [ControlTokenDefault(nameof(TopLevelItemSelectedBg))]
    private static Color DefaultForTopLevelItemSelectedBg(in ControlTokenDefaultContext context) =>
        context.Global.ColorBgTextHover;

    [ControlTokenDefault(nameof(TopLevelItemHoverBg))]
    private static Color DefaultForTopLevelItemHoverBg(in ControlTokenDefaultContext context) =>
        context.Global.ColorBgTextHover;

    [ControlTokenDefault(nameof(TopLevelItemBorderRadiusSM))]
    private static CornerRadius DefaultForTopLevelItemBorderRadiusSM(in ControlTokenDefaultContext context) =>
        context.Global.BorderRadiusSM;

    [ControlTokenDefault(nameof(TopLevelItemBorderRadius))]
    private static CornerRadius DefaultForTopLevelItemBorderRadius(in ControlTokenDefaultContext context) =>
        context.Global.BorderRadius;

    [ControlTokenDefault(nameof(TopLevelItemBorderRadiusLG))]
    private static CornerRadius DefaultForTopLevelItemBorderRadiusLG(in ControlTokenDefaultContext context) =>
        context.Global.BorderRadiusLG;

    [ControlTokenDefault(nameof(TopLevelItemPaddingSM), nameof(TopLevelItemLineHeightSM))]
    private static Thickness DefaultForTopLevelItemPaddingSM(in ControlTokenDefaultContext context, double topLevelItemLineHeightSM) =>
        new Thickness(context.Global.PaddingContentHorizontalXS * 0.7, Math.Max((context.Global.ControlHeightSM - topLevelItemLineHeightSM) / 2, 0));

    [ControlTokenDefault(nameof(TopLevelItemPadding), nameof(TopLevelItemLineHeight))]
    private static Thickness DefaultForTopLevelItemPadding(in ControlTokenDefaultContext context, double topLevelItemLineHeight) =>
        new Thickness(context.Global.PaddingContentHorizontalXS, Math.Max((context.Global.ControlHeight - topLevelItemLineHeight) / 2, 0));

    [ControlTokenDefault(nameof(TopLevelItemPaddingLG), nameof(TopLevelItemLineHeightLG))]
    private static Thickness DefaultForTopLevelItemPaddingLG(in ControlTokenDefaultContext context, double topLevelItemLineHeightLG) =>
        new Thickness(context.Global.PaddingContentHorizontalSM, Math.Max((context.Global.ControlHeightLG - topLevelItemLineHeightLG) / 2, 0));

    [ControlTokenDefault(nameof(TopLevelItemFontSizeSM))]
    private static double DefaultForTopLevelItemFontSizeSM(in ControlTokenDefaultContext context) =>
        context.Global.FontSize;

    [ControlTokenDefault(nameof(TopLevelItemFontSize))]
    private static double DefaultForTopLevelItemFontSize(in ControlTokenDefaultContext context) =>
        context.Global.FontSize;

    [ControlTokenDefault(nameof(TopLevelItemFontSizeLG))]
    private static double DefaultForTopLevelItemFontSizeLG(in ControlTokenDefaultContext context) =>
        context.Global.FontSizeLG;

    [ControlTokenDefault(nameof(TopLevelItemLineHeight), nameof(TopLevelItemFontSize))]
    private static double DefaultForTopLevelItemLineHeight(in ControlTokenDefaultContext context, double topLevelItemFontSize) =>
        CalculatorUtils.CalculateLineHeight(topLevelItemFontSize) * topLevelItemFontSize;

    [ControlTokenDefault(nameof(TopLevelItemLineHeightLG), nameof(TopLevelItemFontSizeLG))]
    private static double DefaultForTopLevelItemLineHeightLG(in ControlTokenDefaultContext context, double topLevelItemFontSizeLG) =>
        CalculatorUtils.CalculateLineHeight(topLevelItemFontSizeLG) * topLevelItemFontSizeLG;

    [ControlTokenDefault(nameof(TopLevelItemLineHeightSM), nameof(TopLevelItemFontSizeSM))]
    private static double DefaultForTopLevelItemLineHeightSM(in ControlTokenDefaultContext context, double topLevelItemFontSizeSM) =>
        CalculatorUtils.CalculateLineHeight(topLevelItemFontSizeSM) * topLevelItemFontSizeSM;

    [ControlTokenDefault(nameof(TopLevelItemPopupMarginToAnchor))]
    private static double DefaultForTopLevelItemPopupMarginToAnchor(in ControlTokenDefaultContext context) =>
        context.Global.UniformlyMarginXXS;

    [ControlTokenDefault(nameof(SeparatorItemHeight))]
    private static double DefaultForSeparatorItemHeight(in ControlTokenDefaultContext context) =>
        context.Global.LineWidth * 5;

    [ControlTokenDefault(nameof(ContextMenuOffsetX))]
    private static double DefaultForContextMenuOffsetX(in ControlTokenDefaultContext context) =>
        context.Global.SpacingXXS;

    [ControlTokenDefault(nameof(ContextMenuOffsetY))]
    private static double DefaultForContextMenuOffsetY(in ControlTokenDefaultContext context) =>
        context.Global.SpacingXXS;
}
