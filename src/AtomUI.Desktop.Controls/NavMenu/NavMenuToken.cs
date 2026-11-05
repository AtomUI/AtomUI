using AtomUI.Media;
using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken(ControlTokenEvaluationVersion.DependencyDefaultsV2, Revision = 1)]
internal sealed partial class NavMenuToken : AbstractControlDesignToken, IControlTokenDefaults
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
    /// 菜单 Popup 最大高度
    /// </summary>
    public double MenuPopupMaxHeight { get; set; }

    /// <summary>
    /// 顶层弹出菜单，距离顶层菜单项的边距
    /// </summary>
    public double TopLevelItemPopupMarginToAnchor { get; set; }

    /// <summary>
    /// 分组标题文字颜色
    /// </summary>
    public Color GroupTitleColor { get; set; }

    /// <summary>
    /// 分组标题文字高度
    /// </summary>
    public double GroupTitleLineHeight { get; set; }

    /// <summary>
    ///
    /// </summary>
    public double GroupTitleFontSize { get; set; }

    /// <summary>
    /// 菜单项的圆角
    /// </summary>
    public CornerRadius ItemBorderRadius { get; set; }

    /// <summary>
    /// 子菜单项的圆角
    /// </summary>
    public CornerRadius SubMenuItemBorderRadius { get; set; }

    /// <summary>
    /// 历史保留：快捷键颜色；当前 NavMenu 无快捷键文字节点，不参与渲染。
    /// </summary>
    public Color KeyGestureColor { get; set; }

    /// <summary>
    /// 菜单项文字颜色
    /// </summary>
    public Color ItemColor { get; set; }

    /// <summary>
    /// 菜单项文字悬浮颜色
    /// </summary>
    public Color ItemHoverColor { get; set; }

    /// <summary>
    /// 水平菜单项文字悬浮颜色
    /// </summary>
    public Color HorizontalItemHoverColor { get; set; }

    /// <summary>
    /// 菜单项文字选中颜色
    /// </summary>
    public Color ItemSelectedColor { get; set; }

    /// <summary>
    /// 水平菜单项文字选中颜色
    /// </summary>
    public Color HorizontalItemSelectedColor { get; set; }

    /// <summary>
    /// 菜单项文字禁用颜色
    /// </summary>
    public Color ItemDisabledColor { get; set; }

    /// <summary>
    /// 危险菜单项文字颜色
    /// 历史保留：当前 NavMenu 无公开危险项入口，不参与渲染。
    /// </summary>
    public Color DangerItemColor { get; set; }

    /// <summary>
    /// 危险菜单项文字悬浮颜色
    /// 历史保留：当前 NavMenu 无公开危险项入口，不参与渲染。
    /// </summary>
    public Color DangerItemHoverColor { get; set; }

    /// <summary>
    /// 危险菜单项文字选中颜色
    /// 历史保留：当前 NavMenu 无公开危险项入口，不参与渲染。
    /// </summary>
    public Color DangerItemSelectedColor { get; set; }

    /// <summary>
    /// 危险菜单项文字激活颜色
    /// 历史保留：当前 NavMenu 无公开危险项入口，不参与渲染。
    /// </summary>
    public Color DangerItemActiveBg { get; set; }

    /// <summary>
    /// 危险菜单项文字选中颜色
    /// 历史保留：当前 NavMenu 无公开危险项入口，不参与渲染。
    /// </summary>
    public Color DangerItemSelectedBg { get; set; }

    /// <summary>
    /// 菜单项背景色
    /// </summary>
    public Color ItemBg { get; set; }

    /// <summary>
    /// 菜单项悬浮态背景色
    /// </summary>
    public Color ItemHoverBg { get; set; }

    /// <summary>
    /// 子菜单项背景色
    /// </summary>
    public Color SubMenuItemBg { get; set; }

    /// <summary>
    /// 菜单项激活态背景色
    /// </summary>
    public Color ItemActiveBg { get; set; }

    /// <summary>
    /// 菜单项选中态背景色
    /// </summary>
    public Color ItemSelectedBg { get; set; }

    /// <summary>
    /// 菜单项背景过渡统一缓动曲线
    /// </summary>
    public Easing? ItemBackgroundMotionEasing { get; set; }

    /// <summary>
    /// 图标尺寸
    /// </summary>
    public double ItemIconSize { get; set; }

    /// <summary>
    /// 水平菜单项选中态背景色
    /// </summary>
    public Color HorizontalItemSelectedBg { get; set; }

    /// <summary>
    /// 菜单项指示条宽度
    /// </summary>
    public double ActiveBarScaleX { get; set; } = double.NaN;

    /// <summary>
    /// 菜单项指示条高度
    /// </summary>
    public double ActiveBarHeight { get; set; } = double.NaN;

    /// <summary>
    /// 菜单项外间距
    /// </summary>
    public Thickness ItemContentMargin { get; set; }

    /// <summary>
    /// 菜单项横向内间距
    /// </summary>
    public Thickness ItemContentPadding { get; set; }

    /// <summary>
    /// 垂直面板的元素间距
    /// </summary>
    public double VerticalItemsPanelSpacing { get; set; }

    /// <summary>
    /// 垂直面板的外间距
    /// </summary>
    public Thickness VerticalChildItemsMargin { get; set; }

    /// <summary>
    /// 垂直面板的内容内间距
    /// </summary>
    public Thickness VerticalMenuContentPadding { get; set; }

    /// <summary>
    /// 菜单项内部元素边距
    /// </summary>
    public Thickness ItemMargin { get; set; }

    /// <summary>
    /// 水平菜单项外间距
    /// </summary>
    public Thickness HorizontalItemMargin { get; set; }

    /// <summary>
    /// 横向菜单项横悬浮态背景色
    /// </summary>
    public Color HorizontalItemHoverBg { get; set; }

    /// <summary>
    /// 横向菜单项圆角
    /// </summary>
    public CornerRadius HorizontalItemBorderRadius { get; set; }

    /// <summary>
    /// 菜单项高度
    /// </summary>
    public double ItemHeight { get; set; }

    /// <summary>
    /// 内联菜单项的缩进单位
    /// </summary>
    public double InlineItemIndentUnit { get; set; }

    /// <summary>
    /// 收起后的宽度
    /// Historical compatibility field; inline collapsed layout uses InlineCollapsedWidth. No alias.
    /// </summary>
    public double CollapsedWidth { get; set; }

    /// <summary>
    /// 内联折叠后的宽度
    /// </summary>
    public double InlineCollapsedWidth { get; set; }

    /// <summary>
    /// 弹出框背景色
    /// </summary>
    public Color MenuPopupBg { get; set; }

    /// <summary>
    /// 横向菜单行高
    /// </summary>
    public double HorizontalLineHeight { get; set; }

    /// <summary>
    /// 图标与文字间距
    /// </summary>
    public Thickness IconMargin { get; set; }

    /// <summary>
    /// 收起时图标尺寸
    /// </summary>
    public double CollapsedIconSize { get; set; }

    // Dark
    /// <summary>
    /// 暗色模式下的菜单的背景颜色
    /// </summary>
    public Color DarkMenuBg { get; set; }
    /// <summary>
    /// 暗色模式下的浮层菜单的背景颜色
    /// </summary>
    public Color DarkMenuPopupBg { get; set; }

    /// <summary>
    /// 暗色模式下的菜单项文字颜色
    /// </summary>
    public Color DarkItemColor { get; set; }

    /// <summary>
    /// 暗色模式下的危险菜单项文字颜色
    /// 历史保留：当前 NavMenu 无公开危险项入口，不参与渲染。
    /// </summary>
    public Color DarkDangerItemColor { get; set; }

    /// <summary>
    /// 暗色模式下的菜单项背景
    /// </summary>
    public Color DarkItemBg { get; set; }

    /// <summary>
    /// 暗色模式下的子菜单项背景
    /// </summary>
    public Color DarkSubMenuItemBg { get; set; }

    /// <summary>
    /// 暗色模式下的菜单项选中颜色
    /// </summary>
    public Color DarkItemSelectedColor { get; set; }

    /// <summary>
    /// 暗色模式下的菜单项选中背景
    /// </summary>
    public Color DarkItemSelectedBg { get; set; }

    /// <summary>
    /// 暗色模式下的菜单项悬浮背景
    /// </summary>
    public Color DarkItemHoverBg { get; set; }

    /// <summary>
    /// 暗色模式下的分组标题文字颜色
    /// </summary>
    public Color DarkGroupTitleColor { get; set; }

    /// <summary>
    /// 暗色模式下的菜单项悬浮颜色
    /// </summary>
    public Color DarkItemHoverColor { get; set; }

    /// <summary>
    /// 暗色模式下的菜单项禁用颜色
    /// </summary>
    public Color DarkItemDisabledColor { get; set; }

    /// <summary>
    /// 暗色模式下的危险菜单项选中背景
    /// 历史保留：当前 NavMenu 无公开危险项入口，不参与渲染。
    /// </summary>
    public Color DarkDangerItemSelectedBg { get; set; }

    /// <summary>
    /// 暗色模式下的危险菜单项悬浮文字背景
    /// 历史保留：当前 NavMenu 无公开危险项入口，不参与渲染。
    /// </summary>
    public Color DarkDangerItemHoverColor { get; set; }

    /// <summary>
    /// 暗色模式下的危险菜单项选中文字颜色
    /// 历史保留：当前 NavMenu 无公开危险项入口，不参与渲染。
    /// </summary>
    public Color DarkDangerItemSelectedColor { get; set; }

    /// <summary>
    /// 暗色模式下的危险菜单项激活态背景
    /// 历史保留：当前 NavMenu 无公开危险项入口，不参与渲染。
    /// </summary>
    public Color DarkDangerItemActiveBg { get; set; }

    #region 菜单相关 Token

    /// <summary>
    /// 水平菜单高度
    /// </summary>
    public double MenuHorizontalHeight { get; set; }

    /// <summary>
    /// 菜单箭头尺寸
    /// </summary>
    public double MenuArrowSize { get; set; }

    /// <summary>
    /// 子菜单背景色
    /// Historical compatibility field; popup and inline surfaces use MenuPopupBg and SubMenuItemBg respectively. No alias.
    /// </summary>
    public Color MenuSubMenuBg { get; set; }

    #endregion

    [ControlTokenDefault(nameof(MenuPopupContentPadding))]
    private static Thickness DefaultForMenuPopupContentPadding(in ControlTokenDefaultContext context) =>
        new Thickness(0, context.Global.UniformlyMarginXXS, 0, 0);

    [ControlTokenDefault(nameof(MenuPopupMinWidth))]
    private static double DefaultForMenuPopupMinWidth(in ControlTokenDefaultContext context) =>
        160d;

    [ControlTokenDefault(nameof(MenuPopupMaxWidth))]
    private static double DefaultForMenuPopupMaxWidth(in ControlTokenDefaultContext context) =>
        800d;

    [ControlTokenDefault(nameof(MenuPopupMaxHeight), nameof(ItemHeight))]
    private static double DefaultForMenuPopupMaxHeight(in ControlTokenDefaultContext context, double itemHeight) =>
        itemHeight * 8;

    [ControlTokenDefault(nameof(TopLevelItemPopupMarginToAnchor))]
    private static double DefaultForTopLevelItemPopupMarginToAnchor(in ControlTokenDefaultContext context) =>
        context.Global.UniformlyMarginXS;

    [ControlTokenDefault(nameof(GroupTitleColor))]
    private static Color DefaultForGroupTitleColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorTextDescription;

    [ControlTokenDefault(nameof(GroupTitleLineHeight))]
    private static double DefaultForGroupTitleLineHeight(in ControlTokenDefaultContext context) =>
        context.Global.ControlHeight;

    [ControlTokenDefault(nameof(GroupTitleFontSize))]
    private static double DefaultForGroupTitleFontSize(in ControlTokenDefaultContext context) =>
        context.Global.FontSize;

    [ControlTokenDefault(nameof(ItemBorderRadius))]
    private static CornerRadius DefaultForItemBorderRadius(in ControlTokenDefaultContext context) =>
        context.Global.BorderRadiusLG;

    [ControlTokenDefault(nameof(SubMenuItemBorderRadius))]
    private static CornerRadius DefaultForSubMenuItemBorderRadius(in ControlTokenDefaultContext context) =>
        context.Global.BorderRadiusSM;

    [ControlTokenDefault(nameof(KeyGestureColor))]
    private static Color DefaultForKeyGestureColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorTextSecondary;

    [ControlTokenDefault(nameof(ItemColor))]
    private static Color DefaultForItemColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorText;

    [ControlTokenDefault(nameof(ItemHoverColor))]
    private static Color DefaultForItemHoverColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorText;

    [ControlTokenDefault(nameof(HorizontalItemHoverColor))]
    private static Color DefaultForHorizontalItemHoverColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorPrimary;

    [ControlTokenDefault(nameof(ItemSelectedColor))]
    private static Color DefaultForItemSelectedColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorPrimary;

    [ControlTokenDefault(nameof(HorizontalItemSelectedColor))]
    private static Color DefaultForHorizontalItemSelectedColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorPrimary;

    [ControlTokenDefault(nameof(ItemDisabledColor))]
    private static Color DefaultForItemDisabledColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorTextDisabled;

    [ControlTokenDefault(nameof(DangerItemColor))]
    private static Color DefaultForDangerItemColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorError;

    [ControlTokenDefault(nameof(DangerItemHoverColor))]
    private static Color DefaultForDangerItemHoverColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorError;

    [ControlTokenDefault(nameof(DangerItemSelectedColor))]
    private static Color DefaultForDangerItemSelectedColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorError;

    [ControlTokenDefault(nameof(DangerItemActiveBg))]
    private static Color DefaultForDangerItemActiveBg(in ControlTokenDefaultContext context) =>
        context.Global.ColorError;

    [ControlTokenDefault(nameof(DangerItemSelectedBg))]
    private static Color DefaultForDangerItemSelectedBg(in ControlTokenDefaultContext context) =>
        context.Global.ColorError;

    [ControlTokenDefault(nameof(ItemBg))]
    private static Color DefaultForItemBg(in ControlTokenDefaultContext context) =>
        context.Global.ColorBgContainer;

    [ControlTokenDefault(nameof(ItemHoverBg))]
    private static Color DefaultForItemHoverBg(in ControlTokenDefaultContext context) =>
        context.Global.ColorBgTextHover;

    [ControlTokenDefault(nameof(SubMenuItemBg))]
    private static Color DefaultForSubMenuItemBg(in ControlTokenDefaultContext context) =>
        context.Global.ColorFillAlter;

    [ControlTokenDefault(nameof(ItemActiveBg))]
    private static Color DefaultForItemActiveBg(in ControlTokenDefaultContext context) =>
        context.Global.ColorFillContent;

    [ControlTokenDefault(nameof(ItemSelectedBg))]
    private static Color DefaultForItemSelectedBg(in ControlTokenDefaultContext context) =>
        context.Global.ControlItemBgActive;

    [ControlTokenDefault(nameof(ItemBackgroundMotionEasing))]
    private static Easing? DefaultForItemBackgroundMotionEasing(in ControlTokenDefaultContext context) =>
        Easing.Parse("0.25,0.1,0.25,1");

    [ControlTokenDefault(nameof(ItemIconSize))]
    private static double DefaultForItemIconSize(in ControlTokenDefaultContext context) =>
        context.Global.IconSize;

    [ControlTokenDefault(nameof(HorizontalItemSelectedBg))]
    private static Color DefaultForHorizontalItemSelectedBg(in ControlTokenDefaultContext context) =>
        Colors.Transparent;

    [ControlTokenDefault(nameof(ActiveBarScaleX))]
    private static double DefaultForActiveBarScaleX(in ControlTokenDefaultContext context) =>
        1.0d;

    [ControlTokenDefault(nameof(ActiveBarHeight))]
    private static double DefaultForActiveBarHeight(in ControlTokenDefaultContext context) =>
        context.Global.LineWidthBold;

    [ControlTokenDefault(nameof(ItemContentMargin))]
    private static Thickness DefaultForItemContentMargin(in ControlTokenDefaultContext context) =>
        new Thickness(context.Global.UniformlyMarginXXS, 0, context.Global.UniformlyMarginXXS, context.Global.UniformlyMarginXXS);

    [ControlTokenDefault(nameof(ItemContentPadding))]
    private static Thickness DefaultForItemContentPadding(in ControlTokenDefaultContext context) =>
        new Thickness(context.Global.UniformlyPadding, context.Global.UniformlyPaddingXXS);

    [ControlTokenDefault(nameof(VerticalItemsPanelSpacing))]
    private static double DefaultForVerticalItemsPanelSpacing(in ControlTokenDefaultContext context) =>
        0;

    [ControlTokenDefault(nameof(VerticalChildItemsMargin))]
    private static Thickness DefaultForVerticalChildItemsMargin(in ControlTokenDefaultContext context) =>
        new Thickness(0, 0, 0, context.Global.UniformlyMarginXXS);

    [ControlTokenDefault(nameof(VerticalMenuContentPadding))]
    private static Thickness DefaultForVerticalMenuContentPadding(in ControlTokenDefaultContext context) =>
        new Thickness(0, context.Global.UniformlyMarginXXS, 0, 0);

    [ControlTokenDefault(nameof(ItemMargin))]
    private static Thickness DefaultForItemMargin(in ControlTokenDefaultContext context) =>
        new Thickness(0, 0, context.Global.UniformlyMarginXS, 0);

    [ControlTokenDefault(nameof(HorizontalItemMargin))]
    private static Thickness DefaultForHorizontalItemMargin(in ControlTokenDefaultContext context) =>
        new Thickness(context.Global.UniformlyPadding, 0);

    [ControlTokenDefault(nameof(HorizontalItemHoverBg))]
    private static Color DefaultForHorizontalItemHoverBg(in ControlTokenDefaultContext context) =>
        Colors.Transparent;

    [ControlTokenDefault(nameof(HorizontalItemBorderRadius))]
    private static CornerRadius DefaultForHorizontalItemBorderRadius(in ControlTokenDefaultContext context) =>
        new CornerRadius(0);

    [ControlTokenDefault(nameof(ItemHeight))]
    private static double DefaultForItemHeight(in ControlTokenDefaultContext context) =>
        context.Global.ControlHeightLG;

    [ControlTokenDefault(nameof(InlineItemIndentUnit), nameof(ItemHeight))]
    private static double DefaultForInlineItemIndentUnit(in ControlTokenDefaultContext context, double itemHeight) =>
        itemHeight / 2;

    [ControlTokenDefault(nameof(CollapsedWidth))]
    private static double DefaultForCollapsedWidth(in ControlTokenDefaultContext context) =>
        context.Global.ControlHeight * 2;

    [ControlTokenDefault(nameof(InlineCollapsedWidth))]
    private static double DefaultForInlineCollapsedWidth(in ControlTokenDefaultContext context) =>
        48d;

    [ControlTokenDefault(nameof(MenuPopupBg))]
    private static Color DefaultForMenuPopupBg(in ControlTokenDefaultContext context) =>
        context.Global.ColorBgElevated;

    [ControlTokenDefault(nameof(HorizontalLineHeight))]
    private static double DefaultForHorizontalLineHeight(in ControlTokenDefaultContext context) =>
        context.Global.ControlHeightLG * 1.15;

    [ControlTokenDefault(nameof(IconMargin))]
    private static Thickness DefaultForIconMargin(in ControlTokenDefaultContext context) =>
        new Thickness(0, 0, context.Global.ControlHeightSM - context.Global.FontSize, 0);

    [ControlTokenDefault(nameof(CollapsedIconSize))]
    private static double DefaultForCollapsedIconSize(in ControlTokenDefaultContext context) =>
        context.Global.IconSizeLG;

    [ControlTokenDefault(nameof(DarkMenuBg))]
    private static Color DefaultForDarkMenuBg(in ControlTokenDefaultContext context) =>
        Color.Parse("#001529");

    [ControlTokenDefault(nameof(DarkMenuPopupBg))]
    private static Color DefaultForDarkMenuPopupBg(in ControlTokenDefaultContext context) =>
        Color.Parse("#001529");

    [ControlTokenDefault(nameof(DarkItemColor))]
    private static Color DefaultForDarkItemColor(in ControlTokenDefaultContext context) =>
        ColorUtils.FromRgbF( 0.65d, context.Global.ColorTextLightSolid.GetRedF(), context.Global.ColorTextLightSolid.GetGreenF(), context.Global.ColorTextLightSolid.GetBlueF());

    [ControlTokenDefault(nameof(DarkDangerItemColor))]
    private static Color DefaultForDarkDangerItemColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorError;

    [ControlTokenDefault(nameof(DarkItemBg))]
    private static Color DefaultForDarkItemBg(in ControlTokenDefaultContext context) =>
        Colors.Transparent;

    [ControlTokenDefault(nameof(DarkSubMenuItemBg))]
    private static Color DefaultForDarkSubMenuItemBg(in ControlTokenDefaultContext context) =>
        Color.Parse("#000c17");

    [ControlTokenDefault(nameof(DarkItemSelectedColor))]
    private static Color DefaultForDarkItemSelectedColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorTextLightSolid;

    [ControlTokenDefault(nameof(DarkItemSelectedBg))]
    private static Color DefaultForDarkItemSelectedBg(in ControlTokenDefaultContext context) =>
        context.Global.ColorPrimary;

    [ControlTokenDefault(nameof(DarkItemHoverBg))]
    private static Color DefaultForDarkItemHoverBg(in ControlTokenDefaultContext context) =>
        Colors.Transparent;

    [ControlTokenDefault(nameof(DarkGroupTitleColor))]
    private static Color DefaultForDarkGroupTitleColor(in ControlTokenDefaultContext context) =>
        ColorUtils.FromRgbF( 0.65d, context.Global.ColorTextLightSolid.GetRedF(), context.Global.ColorTextLightSolid.GetGreenF(), context.Global.ColorTextLightSolid.GetBlueF());

    [ControlTokenDefault(nameof(DarkItemHoverColor))]
    private static Color DefaultForDarkItemHoverColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorTextLightSolid;

    [ControlTokenDefault(nameof(DarkItemDisabledColor))]
    private static Color DefaultForDarkItemDisabledColor(in ControlTokenDefaultContext context) =>
        ColorUtils.FromRgbF(0.25d, context.Global.ColorTextLightSolid.GetRedF(), context.Global.ColorTextLightSolid.GetGreenF(), context.Global.ColorTextLightSolid.GetBlueF());

    [ControlTokenDefault(nameof(DarkDangerItemSelectedBg))]
    private static Color DefaultForDarkDangerItemSelectedBg(in ControlTokenDefaultContext context) =>
        context.Global.ColorError;

    [ControlTokenDefault(nameof(DarkDangerItemHoverColor))]
    private static Color DefaultForDarkDangerItemHoverColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorErrorHover;

    [ControlTokenDefault(nameof(DarkDangerItemSelectedColor))]
    private static Color DefaultForDarkDangerItemSelectedColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorTextLightSolid;

    [ControlTokenDefault(nameof(DarkDangerItemActiveBg))]
    private static Color DefaultForDarkDangerItemActiveBg(in ControlTokenDefaultContext context) =>
        context.Global.ColorError;

    [ControlTokenDefault(nameof(MenuHorizontalHeight))]
    private static double DefaultForMenuHorizontalHeight(in ControlTokenDefaultContext context) =>
        context.Global.ControlHeightLG * 1.15;

    [ControlTokenDefault(nameof(MenuArrowSize))]
    private static double DefaultForMenuArrowSize(in ControlTokenDefaultContext context) =>
        context.Global.FontSize / 7 * 5;

    [ControlTokenDefault(nameof(MenuSubMenuBg))]
    private static Color DefaultForMenuSubMenuBg(in ControlTokenDefaultContext context) =>
        context.Global.ColorBgElevated;
}
