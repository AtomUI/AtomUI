using AtomUI.Media;
using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken(ControlTokenEvaluationVersion.DependencyDefaultsV2, Revision = 1)]
internal sealed partial class TabControlToken : AbstractControlDesignToken, IControlTokenDefaults
{
    public TabControlToken()
    {
    }

    /// <summary>
    /// 卡片标签页背景色
    /// </summary>
    public Color CardBg { get; set; }

    /// <summary>
    /// 卡片标签页大小
    /// </summary>
    public double CardSize { get; set; }

    /// <summary>
    /// 卡片标签页内间距
    /// </summary>
    public Thickness CardPadding { get; set; }

    /// <summary>
    /// 小号卡片标签页内间距
    /// </summary>
    public Thickness CardPaddingSM { get; set; }

    /// <summary>
    /// 大号卡片标签页内间距
    /// </summary>
    public Thickness CardPaddingLG { get; set; }

    /// <summary>
    /// 标齐页标题文本大小
    /// </summary>
    public double TitleFontSize { get; set; }

    /// <summary>
    /// 大号标签页标题文本大小
    /// </summary>
    public double TitleFontSizeLG { get; set; }

    /// <summary>
    /// 小号标签页标题文本大小
    /// </summary>
    public double TitleFontSizeSM { get; set; }

    /// <summary>
    /// 指示条颜色
    /// </summary>
    public Color InkBarColor { get; set; }

    /// <summary>
    /// 指示条厚度
    /// </summary>
    public double InkBarThickness { get; set; }

    /// <summary>
    /// 横向标签页外间距
    /// </summary>
    public Thickness HorizontalMargin { get; set; }

    /// <summary>
    /// 横向标签页标签间距
    /// </summary>
    public double HorizontalItemGutter { get; set; }

    /// <summary>
    /// 横向标签页标签外间距
    /// </summary>
    public Thickness HorizontalItemMargin { get; set; }

    /// <summary>
    /// 横向标签页标签内间距
    /// </summary>
    public Thickness HorizontalItemPadding { get; set; }

    /// <summary>
    /// 大号横向标签页标签内间距
    /// </summary>
    public Thickness HorizontalItemPaddingLG { get; set; }

    /// <summary>
    /// 小号横向标签页标签内间距
    /// </summary>
    public Thickness HorizontalItemPaddingSM { get; set; }

    /// <summary>
    /// 纵向标签页标签间距
    /// </summary>
    public double VerticalItemGutter { get; set; }

    /// <summary>
    /// 纵向标签页标签内间距
    /// </summary>
    public Thickness VerticalItemPadding { get; set; }

    /// <summary>
    /// 标签文本颜色
    /// </summary>
    public Color ItemColor { get; set; }

    /// <summary>
    /// 标签悬浮态文本颜色
    /// </summary>
    public Color ItemHoverColor { get; set; }

    /// <summary>
    /// 标签选中态文本颜色
    /// </summary>
    public Color ItemSelectedColor { get; set; }

    /// <summary>
    /// 卡片标签间距
    /// </summary>
    public double CardGutter { get; set; }

    /// <summary>
    /// 标签内容 icon 的外边距
    /// </summary>
    public Thickness ItemIconMargin { get; set; }

    /// <summary>
    /// 水平布局的菜单外边距
    /// </summary>
    public Thickness MenuIndicatorPaddingHorizontal { get; set; }

    /// <summary>
    /// 垂直布局的菜单外边距
    /// </summary>
    public Thickness MenuIndicatorPaddingVertical { get; set; }

    /// <summary>
    /// 滚动边缘的厚度
    /// </summary>
    public double MenuEdgeThickness { get; set; }

    /// <summary>
    /// 左侧滚动溢出阴影
    /// </summary>
    public BoxShadows BoxShadowTabsOverflowLeft { get; set; }

    /// <summary>
    /// 右侧滚动溢出阴影
    /// </summary>
    public BoxShadows BoxShadowTabsOverflowRight { get; set; }

    /// <summary>
    /// 顶部滚动溢出阴影
    /// </summary>
    public BoxShadows BoxShadowTabsOverflowTop { get; set; }

    /// <summary>
    /// 底部滚动溢出阴影
    /// </summary>
    public BoxShadows BoxShadowTabsOverflowBottom { get; set; }

    /// <summary>
    /// 水平添加按钮外边距
    /// </summary>
    public Thickness AddTabButtonMarginHorizontal { get; set; }

    /// <summary>
    /// 垂直添加按钮外边距
    /// </summary>
    public Thickness AddTabButtonMarginVertical { get; set; }

    /// <summary>
    /// 关闭按钮外边距
    /// </summary>
    public Thickness CloseIconMargin { get; set; }

    /// <summary>
    /// Tab 标签和内容区域的间距
    /// </summary>
    public double TabAndContentGutter { get; set; }

    [ControlTokenDefault(nameof(CardBg))]
    private static Color DefaultForCardBg(in ControlTokenDefaultContext context) => context.Global.ColorFillAlter;

    [ControlTokenDefault(nameof(CardSize))]
    private static double DefaultForCardSize(in ControlTokenDefaultContext context) => context.Global.ControlHeightLG;

    [ControlTokenDefault(nameof(CardPadding), nameof(CardSize))]
    private static Thickness DefaultForCardPadding(in ControlTokenDefaultContext context, double cardSize) =>
        new(context.Global.UniformlyPadding,
            (cardSize - Math.Round(context.Global.FontSize * context.Global.RelativeLineHeight)) / 2 - context.Global.LineWidth);

    [ControlTokenDefault(nameof(CardPaddingSM))]
    private static Thickness DefaultForCardPaddingSM(in ControlTokenDefaultContext context) =>
        new(context.Global.UniformlyPadding, context.Global.UniformlyPaddingXXS * 1.5);

    [ControlTokenDefault(nameof(CardPaddingLG))]
    private static Thickness DefaultForCardPaddingLG(in ControlTokenDefaultContext context) =>
        new(top: context.Global.UniformlyPaddingXS, bottom: context.Global.UniformlyPaddingXXS * 1.5,
            left: context.Global.UniformlyPadding, right: context.Global.UniformlyPadding);

    [ControlTokenDefault(nameof(TitleFontSize))]
    private static double DefaultForTitleFontSize(in ControlTokenDefaultContext context) => context.Global.FontSize;

    [ControlTokenDefault(nameof(TitleFontSizeLG))]
    private static double DefaultForTitleFontSizeLG(in ControlTokenDefaultContext context) => context.Global.FontSizeLG;

    [ControlTokenDefault(nameof(TitleFontSizeSM))]
    private static double DefaultForTitleFontSizeSM(in ControlTokenDefaultContext context) => context.Global.FontSize;

    [ControlTokenDefault(nameof(InkBarColor))]
    private static Color DefaultForInkBarColor(in ControlTokenDefaultContext context) => context.Global.ColorPrimary;

    [ControlTokenDefault(nameof(InkBarThickness))]
    private static double DefaultForInkBarThickness(in ControlTokenDefaultContext context) => context.Global.LineWidthBold;

    [ControlTokenDefault(nameof(HorizontalMargin))]
    private static Thickness DefaultForHorizontalMargin(in ControlTokenDefaultContext context) =>
        new(0, 0, context.Global.UniformlyMargin, 0);

    [ControlTokenDefault(nameof(HorizontalItemGutter))]
    private static double DefaultForHorizontalItemGutter(in ControlTokenDefaultContext context) => 32;

    [ControlTokenDefault(nameof(HorizontalItemMargin))]
    private static Thickness DefaultForHorizontalItemMargin(in ControlTokenDefaultContext context) => new();

    [ControlTokenDefault(nameof(HorizontalItemPadding))]
    private static Thickness DefaultForHorizontalItemPadding(in ControlTokenDefaultContext context) =>
        new(0, context.Global.UniformlyPaddingSM);

    [ControlTokenDefault(nameof(HorizontalItemPaddingLG))]
    private static Thickness DefaultForHorizontalItemPaddingLG(in ControlTokenDefaultContext context) =>
        new(0, context.Global.UniformlyPadding);

    [ControlTokenDefault(nameof(HorizontalItemPaddingSM))]
    private static Thickness DefaultForHorizontalItemPaddingSM(in ControlTokenDefaultContext context) =>
        new(0, context.Global.UniformlyPaddingXS);

    [ControlTokenDefault(nameof(VerticalItemGutter))]
    private static double DefaultForVerticalItemGutter(in ControlTokenDefaultContext context) => context.Global.SpacingXXS;

    [ControlTokenDefault(nameof(VerticalItemPadding))]
    private static Thickness DefaultForVerticalItemPadding(in ControlTokenDefaultContext context) =>
        new(context.Global.UniformlyPaddingXS, context.Global.UniformlyPaddingXS);

    [ControlTokenDefault(nameof(ItemColor))]
    private static Color DefaultForItemColor(in ControlTokenDefaultContext context) => context.Global.ColorText;

    [ControlTokenDefault(nameof(ItemHoverColor))]
    private static Color DefaultForItemHoverColor(in ControlTokenDefaultContext context) => context.Global.ColorPrimaryHover;

    [ControlTokenDefault(nameof(ItemSelectedColor))]
    private static Color DefaultForItemSelectedColor(in ControlTokenDefaultContext context) => context.Global.ColorPrimary;

    [ControlTokenDefault(nameof(CardGutter))]
    private static double DefaultForCardGutter(in ControlTokenDefaultContext context) => context.Global.UniformlyMarginXXS / 2;

    [ControlTokenDefault(nameof(ItemIconMargin))]
    private static Thickness DefaultForItemIconMargin(in ControlTokenDefaultContext context) =>
        new(0, 0, context.Global.UniformlyMarginSM, 0);

    [ControlTokenDefault(nameof(MenuIndicatorPaddingHorizontal))]
    private static Thickness DefaultForMenuIndicatorPaddingHorizontal(in ControlTokenDefaultContext context) =>
        new(context.Global.UniformlyPaddingXS, 0, 0, 0);

    [ControlTokenDefault(nameof(MenuIndicatorPaddingVertical))]
    private static Thickness DefaultForMenuIndicatorPaddingVertical(in ControlTokenDefaultContext context) =>
        new(0, context.Global.UniformlyPaddingXS, 0, 0);

    [ControlTokenDefault(nameof(MenuEdgeThickness))]
    private static double DefaultForMenuEdgeThickness(in ControlTokenDefaultContext context) => context.Global.ControlHeight;

    [ControlTokenDefault(nameof(BoxShadowTabsOverflowLeft))]
    private static BoxShadows DefaultForBoxShadowTabsOverflowLeft(in ControlTokenDefaultContext context) =>
        new(new BoxShadow { OffsetX = 10, Blur = 8, Spread = -8, Color = ColorUtils.FromRgbF(0.08, 0, 0, 0) });

    [ControlTokenDefault(nameof(BoxShadowTabsOverflowRight))]
    private static BoxShadows DefaultForBoxShadowTabsOverflowRight(in ControlTokenDefaultContext context) =>
        new(new BoxShadow { OffsetX = -10, Blur = 8, Spread = -8, Color = ColorUtils.FromRgbF(0.08, 0, 0, 0) });

    [ControlTokenDefault(nameof(BoxShadowTabsOverflowTop))]
    private static BoxShadows DefaultForBoxShadowTabsOverflowTop(in ControlTokenDefaultContext context) =>
        new(new BoxShadow { OffsetY = 10, Blur = 8, Spread = -8, Color = ColorUtils.FromRgbF(0.08, 0, 0, 0) });

    [ControlTokenDefault(nameof(BoxShadowTabsOverflowBottom))]
    private static BoxShadows DefaultForBoxShadowTabsOverflowBottom(in ControlTokenDefaultContext context) =>
        new(new BoxShadow { OffsetY = -10, Blur = 8, Spread = -8, Color = ColorUtils.FromRgbF(0.08, 0, 0, 0) });

    [ControlTokenDefault(nameof(AddTabButtonMarginHorizontal))]
    private static Thickness DefaultForAddTabButtonMarginHorizontal(in ControlTokenDefaultContext context) =>
        new(context.Global.UniformlyMarginXXS, 0, 0, 0);

    [ControlTokenDefault(nameof(AddTabButtonMarginVertical))]
    private static Thickness DefaultForAddTabButtonMarginVertical(in ControlTokenDefaultContext context) =>
        new(0, context.Global.UniformlyMarginXXS, 0, 0);

    [ControlTokenDefault(nameof(CloseIconMargin))]
    private static Thickness DefaultForCloseIconMargin(in ControlTokenDefaultContext context) =>
        new(context.Global.UniformlyMarginXXS, 0, 0, 0);

    [ControlTokenDefault(nameof(TabAndContentGutter))]
    private static double DefaultForTabAndContentGutter(in ControlTokenDefaultContext context) => context.Global.UniformlyMarginSM;
    
}
