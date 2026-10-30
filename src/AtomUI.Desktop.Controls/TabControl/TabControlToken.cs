using AtomUI.Media;
using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
internal sealed class TabControlToken : AbstractControlDesignToken
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

    public override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        var lineHeight = EffectiveGlobalToken.RelativeLineHeight;
        var lineWidth  = EffectiveGlobalToken.LineWidth;

        CardBg = EffectiveGlobalToken.ColorFillAlter;

        CardSize = EffectiveGlobalToken.ControlHeightLG;

        CardPadding = new Thickness(EffectiveGlobalToken.UniformlyPadding,
            (CardSize - Math.Round(EffectiveGlobalToken.FontSize * lineHeight)) / 2 - lineWidth);
        CardPaddingSM = new Thickness(EffectiveGlobalToken.UniformlyPadding, EffectiveGlobalToken.UniformlyPaddingXXS * 1.5);
        CardPaddingLG = new Thickness(top: EffectiveGlobalToken.UniformlyPaddingXS,
            bottom: EffectiveGlobalToken.UniformlyPaddingXXS * 1.5,
            left: EffectiveGlobalToken.UniformlyPadding,
            right: EffectiveGlobalToken.UniformlyPadding);

        TitleFontSize   = EffectiveGlobalToken.FontSize;
        TitleFontSizeLG = EffectiveGlobalToken.FontSizeLG;
        TitleFontSizeSM = EffectiveGlobalToken.FontSize;

        InkBarColor = EffectiveGlobalToken.ColorPrimary;

        InkBarThickness = EffectiveGlobalToken.LineWidthBold;

        HorizontalMargin     = new Thickness(0, 0, EffectiveGlobalToken.UniformlyMargin, 0);
        HorizontalItemGutter = 32;
        HorizontalItemMargin = new Thickness();

        HorizontalItemPadding   = new Thickness(0, EffectiveGlobalToken.UniformlyPaddingSM);
        HorizontalItemPaddingSM = new Thickness(0, EffectiveGlobalToken.UniformlyPaddingXS);
        HorizontalItemPaddingLG = new Thickness(0, EffectiveGlobalToken.UniformlyPadding);

        VerticalItemGutter  = EffectiveGlobalToken.SpacingXXS;
        VerticalItemPadding = new Thickness(EffectiveGlobalToken.UniformlyPaddingXS, EffectiveGlobalToken.UniformlyPaddingXS);

        ItemColor         = EffectiveGlobalToken.ColorText;
        ItemSelectedColor = EffectiveGlobalToken.ColorPrimary;
        ItemHoverColor    = EffectiveGlobalToken.ColorPrimaryHover;

        CardGutter                   = EffectiveGlobalToken.UniformlyMarginXXS / 2;
        AddTabButtonMarginHorizontal = new Thickness(EffectiveGlobalToken.UniformlyMarginXXS, 0, 0, 0);
        AddTabButtonMarginVertical   = new Thickness(0, EffectiveGlobalToken.UniformlyMarginXXS, 0, 0);
        ItemIconMargin               = new Thickness(0, 0, EffectiveGlobalToken.UniformlyMarginSM, 0);

        MenuIndicatorPaddingHorizontal = new Thickness(EffectiveGlobalToken.UniformlyPaddingXS, 0, 0, 0);
        MenuIndicatorPaddingVertical   = new Thickness(0, EffectiveGlobalToken.UniformlyPaddingXS, 0, 0);
        CloseIconMargin                = new Thickness(EffectiveGlobalToken.UniformlyMarginXXS, 0, 0, 0);

        var tabsOverflowShadowColor = ColorUtils.FromRgbF(0.08, 0, 0, 0);
        BoxShadowTabsOverflowLeft   = new BoxShadows(new BoxShadow
        {
            OffsetX = 10,
            Blur    = 8,
            Spread  = -8,
            Color   = tabsOverflowShadowColor
        });
        BoxShadowTabsOverflowRight  = new BoxShadows(new BoxShadow
        {
            OffsetX = -10,
            Blur    = 8,
            Spread  = -8,
            Color   = tabsOverflowShadowColor
        });
        BoxShadowTabsOverflowTop    = new BoxShadows(new BoxShadow
        {
            OffsetY = 10,
            Blur    = 8,
            Spread  = -8,
            Color   = tabsOverflowShadowColor
        });
        BoxShadowTabsOverflowBottom = new BoxShadows(new BoxShadow
        {
            OffsetY = -10,
            Blur    = 8,
            Spread  = -8,
            Color   = tabsOverflowShadowColor
        });

        MenuEdgeThickness   = EffectiveGlobalToken.ControlHeight;
        TabAndContentGutter = EffectiveGlobalToken.UniformlyMarginSM;
    }
    
}
