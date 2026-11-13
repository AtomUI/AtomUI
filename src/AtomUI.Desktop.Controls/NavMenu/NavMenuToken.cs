using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Media;
using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class NavMenuToken : AbstractControlDesignToken
{

    /// <summary>
    /// 菜单内容边距
    /// </summary>
    public partial Thickness MenuPopupContentPadding { get; set; }

    /// <summary>
    /// 菜单 Popup 最小宽度
    /// </summary>
    public partial double MenuPopupMinWidth { get; set; }

    /// <summary>
    /// 菜单 Popup 最大宽度
    /// </summary>
    public partial double MenuPopupMaxWidth { get; set; }

    /// <summary>
    /// 菜单 Popup 最大高度
    /// </summary>
    public partial double MenuPopupMaxHeight { get; set; }
    
    /// <summary>
    /// 顶层弹出菜单，距离顶层菜单项的边距
    /// </summary>
    public partial double TopLevelItemPopupMarginToAnchor { get; set; }

    /// <summary>
    /// 分组标题文字颜色
    /// </summary>
    public partial Color GroupTitleColor { get; set; }
    
    /// <summary>
    /// 分组标题文字高度
    /// </summary>
    public partial double GroupTitleLineHeight { get; set; }
    
    /// <summary>
    /// 
    /// </summary>
    public partial double GroupTitleFontSize { get; set; }
    
    /// <summary>
    /// 菜单项的圆角
    /// </summary>
    public partial CornerRadius ItemBorderRadius { get; set; }
    
    /// <summary>
    /// 子菜单项的圆角
    /// </summary>
    public partial CornerRadius SubMenuItemBorderRadius { get; set; }
    
    /// <summary>
    /// 菜单项文字颜色
    /// </summary>
    public partial Color ItemColor { get; set; }
    
    /// <summary>
    /// 菜单项文字悬浮颜色
    /// </summary>
    public partial Color ItemHoverColor { get; set; }
    
    /// <summary>
    /// 水平菜单项文字悬浮颜色
    /// </summary>
    [ReservedToken("antd Menu horizontalItemHoverColor. antd 6.7 declares it but its menu style never reads it: " +
                   "horizontal hover text uses itemHoverColor (ItemHoverColor here), so wiring it would diverge from " +
                   "antd's rendering.")]
    public partial Color HorizontalItemHoverColor { get; set; }
    
    /// <summary>
    /// 菜单项文字选中颜色
    /// </summary>
    public partial Color ItemSelectedColor { get; set; }
    
    /// <summary>
    /// 水平菜单项文字选中颜色
    /// </summary>
    public partial Color HorizontalItemSelectedColor { get; set; }
    
    /// <summary>
    /// 菜单项文字禁用颜色
    /// </summary>
    public partial Color ItemDisabledColor { get; set; }
    
    /// <summary>
    /// 危险菜单项文字颜色
    /// </summary>
    public partial Color DangerItemColor { get; set; }
    
    /// <summary>
    /// 危险菜单项文字悬浮颜色
    /// </summary>
    public partial Color DangerItemHoverColor { get; set; }
    
    /// <summary>
    /// 危险菜单项文字选中颜色
    /// </summary>
    public partial Color DangerItemSelectedColor { get; set; }
    
    /// <summary>
    /// 危险菜单项文字激活颜色
    /// </summary>
    public partial Color DangerItemActiveBg { get; set; }
    
    /// <summary>
    /// 危险菜单项文字选中颜色
    /// </summary>
    public partial Color DangerItemSelectedBg { get; set; }
    
    /// <summary>
    /// 菜单项背景色
    /// </summary>
    public partial Color ItemBg { get; set; }
    
    /// <summary>
    /// 菜单项悬浮态背景色
    /// </summary>
    public partial Color ItemHoverBg { get; set; }
    
    /// <summary>
    /// 子菜单项背景色
    /// </summary>
    public partial Color SubMenuItemBg { get; set; }
    
    /// <summary>
    /// 菜单项激活态背景色
    /// </summary>
    public partial Color ItemActiveBg { get; set; }
    
    /// <summary>
    /// 菜单项选中态背景色
    /// </summary>
    public partial Color ItemSelectedBg { get; set; }

    /// <summary>
    /// 图标尺寸
    /// </summary>
    public partial double ItemIconSize { get; set; }
    
    /// <summary>
    /// 水平菜单项选中态背景色
    /// </summary>
    public partial Color HorizontalItemSelectedBg { get; set; }

    /// <summary>
    /// 菜单项指示条宽度
    /// </summary>
    public partial double ActiveBarScaleX { get; set; }
    
    /// <summary>
    /// 菜单项指示条高度
    /// </summary>
    public partial double ActiveBarHeight { get; set; }
    
    /// <summary>
    /// 菜单项外间距
    /// </summary>
    public partial Thickness ItemContentMargin { get; set; }
    
    /// <summary>
    /// 菜单项横向内间距
    /// </summary>
    public partial Thickness ItemContentPadding { get; set; }
    
    /// <summary>
    /// 垂直面板的元素间距
    /// </summary>
    public partial double VerticalItemsPanelSpacing { get; set; }
    
    /// <summary>
    /// 垂直面板的外间距
    /// </summary>
    public partial Thickness VerticalChildItemsMargin { get; set; }
    
    /// <summary>
    /// 垂直面板的内容内间距
    /// </summary>
    public partial Thickness VerticalMenuContentPadding { get; set; }
    
    /// <summary>
    /// 菜单项内部元素边距
    /// </summary>
    public partial Thickness ItemMargin { get; set; }
    
    /// <summary>
    /// 水平菜单项外间距
    /// </summary>
    public partial Thickness HorizontalItemMargin { get; set; }
    
    /// <summary>
    /// 横向菜单项横悬浮态背景色
    /// </summary>
    public partial Color HorizontalItemHoverBg { get; set; }
    
    /// <summary>
    /// 横向菜单项圆角
    /// </summary>
    public partial CornerRadius HorizontalItemBorderRadius { get; set; }
    
    /// <summary>
    /// 菜单项高度
    /// </summary>
    public partial double ItemHeight { get; set; }
    
    /// <summary>
    /// 内联菜单项的缩进单位
    /// </summary>
    public partial double InlineItemIndentUnit { get; set; }
    
    /// <summary>
    /// 收起后的宽度
    /// </summary>
    [ReservedToken("antd Menu collapsedWidth (controlHeightLG * 2). The inline collapsed width here is " +
                   "InlineCollapsedWidth (48); CollapsedWidth is kept as the antd-named compatibility token and no " +
                   "element reads it.")]
    public partial double CollapsedWidth { get; set; }

    /// <summary>
    /// 内联折叠后的宽度
    /// </summary>
    public partial double InlineCollapsedWidth { get; set; }
    
    /// <summary>
    /// 弹出框背景色
    /// </summary>
    public partial Color MenuPopupBg { get; set; }
    
    /// <summary>
    /// 横向菜单行高
    /// </summary>
    public partial double HorizontalLineHeight { get; set; }
    
    /// <summary>
    /// 图标与文字间距
    /// </summary>
    public partial Thickness IconMargin { get; set; }
    
    /// <summary>
    /// 收起时图标尺寸
    /// </summary>
    public partial double CollapsedIconSize { get; set; }
    
    // Dark
    /// <summary>
    /// 暗色模式下的菜单的背景颜色
    /// </summary>
    public partial Color DarkMenuBg { get; set; }
    /// <summary>
    /// 暗色模式下的浮层菜单的背景颜色
    /// </summary>
    public partial Color DarkMenuPopupBg { get; set; }
    
    /// <summary>
    /// 暗色模式下的菜单项文字颜色
    /// </summary>
    public partial Color DarkItemColor { get; set; }
    
    /// <summary>
    /// 暗色模式下的危险菜单项文字颜色
    /// </summary>
    public partial Color DarkDangerItemColor { get; set; }
    
    /// <summary>
    /// 暗色模式下的菜单项背景
    /// </summary>
    public partial Color DarkItemBg { get; set; }
    
    /// <summary>
    /// 暗色模式下的子菜单项背景
    /// </summary>
    public partial Color DarkSubMenuItemBg { get; set; }
    
    /// <summary>
    /// 暗色模式下的菜单项选中颜色
    /// </summary>
    public partial Color DarkItemSelectedColor { get; set; }
    
    /// <summary>
    /// 暗色模式下的菜单项选中背景
    /// </summary>
    public partial Color DarkItemSelectedBg { get; set; }
    
    /// <summary>
    /// 暗色模式下的菜单项悬浮背景
    /// </summary>
    public partial Color DarkItemHoverBg { get; set; }
    
    /// <summary>
    /// 暗色模式下的分组标题文字颜色
    /// </summary>
    public partial Color DarkGroupTitleColor { get; set; }
    
    /// <summary>
    /// 暗色模式下的菜单项悬浮颜色
    /// </summary>
    public partial Color DarkItemHoverColor { get; set; }
    
    /// <summary>
    /// 暗色模式下的菜单项禁用颜色
    /// </summary>
    public partial Color DarkItemDisabledColor { get; set; }
    
    /// <summary>
    /// 暗色模式下的危险菜单项选中背景
    /// </summary>
    public partial Color DarkDangerItemSelectedBg { get; set; }
    
    /// <summary>
    /// 暗色模式下的危险菜单项悬浮文字背景
    /// </summary>
    public partial Color DarkDangerItemHoverColor { get; set; }
    
    /// <summary>
    /// 暗色模式下的危险菜单项选中文字颜色
    /// </summary>
    public partial Color DarkDangerItemSelectedColor { get; set; }
    
    /// <summary>
    /// 暗色模式下的危险菜单项激活态背景
    /// </summary>
    public partial Color DarkDangerItemActiveBg { get; set; }

    #region 菜单相关 Token

    /// <summary>
    /// 水平菜单高度
    /// </summary>
    public partial double MenuHorizontalHeight { get; set; }
    
    /// <summary>
    /// 菜单箭头尺寸
    /// </summary>
    public partial double MenuArrowSize { get; set; }

    /// <summary>
    /// 子菜单背景色
    /// </summary>
    [ReservedToken("antd Menu menuSubMenuBg (colorBgElevated). In antd the submenu popup's later popupBg rule paints the " +
                   "same element, so the popup surface is popupBg; here that surface is MenuPopupBg and there is no " +
                   "second submenu layer to paint.")]
    public partial Color MenuSubMenuBg { get; set; }

    #endregion
    
    public NavMenuToken()

    {
    }

    protected override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        
        var colorTextLightSolid = EffectiveGlobalToken.ColorTextLightSolid;
        
        if (!IsExplicit(NavMenuTokenKind.ActiveBarScaleX))
        {
            ActiveBarScaleX = 1.0d;
        }

        if (!IsExplicit(NavMenuTokenKind.ActiveBarHeight))
        {
            ActiveBarHeight = EffectiveGlobalToken.LineWidthBold;
        }

        if (!IsExplicit(NavMenuTokenKind.ItemContentMargin))
        {
            ItemContentMargin = new Thickness(
                EffectiveGlobalToken.UniformlyMarginXXS,
                0,
                EffectiveGlobalToken.UniformlyMarginXXS,
                EffectiveGlobalToken.UniformlyMarginXXS);
        }

        var colorTextDark = ColorUtils.FromRgbF(
            0.65d,
            colorTextLightSolid.GetRedF(),
            colorTextLightSolid.GetGreenF(),
            colorTextLightSolid.GetBlueF());
        
        ItemBorderRadius            = EffectiveGlobalToken.BorderRadiusLG;
        SubMenuItemBorderRadius     = EffectiveGlobalToken.BorderRadiusSM;
        ItemColor                   = EffectiveGlobalToken.ColorText;
        ItemHoverColor              = EffectiveGlobalToken.ColorText;
        HorizontalItemHoverColor    = EffectiveGlobalToken.ColorPrimary;
        GroupTitleColor             = EffectiveGlobalToken.ColorTextDescription;
        ItemSelectedColor           = EffectiveGlobalToken.ColorPrimary;
        HorizontalItemSelectedColor = EffectiveGlobalToken.ColorPrimary;
        ItemBg                      = EffectiveGlobalToken.ColorBgContainer;
        ItemHoverBg                 = EffectiveGlobalToken.ColorBgTextHover;
        ItemActiveBg                = EffectiveGlobalToken.ColorFillContent;
        SubMenuItemBg               = EffectiveGlobalToken.ColorFillAlter;
        ItemSelectedBg              = EffectiveGlobalToken.ControlItemBgActive;
        HorizontalItemSelectedBg    = Colors.Transparent;
        
        // Disabled
        ItemDisabledColor = EffectiveGlobalToken.ColorTextDisabled;
        
        // Danger
        DangerItemColor         = EffectiveGlobalToken.ColorError;
        DangerItemHoverColor    = EffectiveGlobalToken.ColorError;
        DangerItemSelectedColor = EffectiveGlobalToken.ColorError;
        DangerItemActiveBg      = EffectiveGlobalToken.ColorErrorBg;
        DangerItemSelectedBg    = EffectiveGlobalToken.ColorErrorBg;

        ItemContentPadding   = new Thickness(EffectiveGlobalToken.UniformlyPadding, EffectiveGlobalToken.UniformlyPaddingXXS);
        ItemMargin           = new Thickness(0, 0, EffectiveGlobalToken.UniformlyMarginXS, 0);

        if (!IsExplicit(NavMenuTokenKind.ItemHeight))
        {
            ItemHeight           = EffectiveGlobalToken.ControlHeightLG;
        }

        GroupTitleLineHeight = EffectiveGlobalToken.ControlHeight;
        CollapsedWidth       = EffectiveGlobalToken.ControlHeight * 2;
        InlineCollapsedWidth = 48d;
        IconMargin           = new Thickness(0, 0, EffectiveGlobalToken.ControlHeightSM - EffectiveGlobalToken.FontSize, 0);
        CollapsedIconSize    = EffectiveGlobalToken.IconSizeLG;
        GroupTitleFontSize   = EffectiveGlobalToken.FontSize;
        
        // Disabled
        DarkItemDisabledColor = ColorUtils.FromRgbF(0.25d,
            colorTextLightSolid.GetRedF(),
            colorTextLightSolid.GetGreenF(),
            colorTextLightSolid.GetBlueF());
        
        // Dark
        DarkItemColor       = colorTextDark;
        DarkDangerItemColor = EffectiveGlobalToken.ColorError;
        DarkItemBg          = Colors.Transparent;
        DarkMenuBg          = Color.Parse("#001529");
        DarkMenuPopupBg     = Color.Parse("#001529");
        DarkSubMenuItemBg   = Color.Parse("#000c17");

        DarkItemSelectedColor       = colorTextLightSolid;
        DarkItemSelectedBg          = EffectiveGlobalToken.ColorPrimary;
        DarkDangerItemSelectedBg    = EffectiveGlobalToken.ColorError;
        DarkItemHoverBg             = Colors.Transparent;
        DarkGroupTitleColor         = colorTextDark;
        DarkItemHoverColor          = colorTextLightSolid;
        DarkDangerItemHoverColor    = EffectiveGlobalToken.ColorErrorHover;
        DarkDangerItemSelectedColor = colorTextLightSolid;
        DarkDangerItemActiveBg      = EffectiveGlobalToken.ColorError;

        MenuHorizontalHeight       = EffectiveGlobalToken.ControlHeightLG * 1.15;
        HorizontalItemMargin       = new Thickness(EffectiveGlobalToken.UniformlyPadding, 0);
        HorizontalLineHeight       = EffectiveGlobalToken.ControlHeightLG * 1.15;
        HorizontalItemBorderRadius = new CornerRadius(0);
        HorizontalItemHoverBg      = Colors.Transparent;
        
        MenuArrowSize        = EffectiveGlobalToken.FontSize / 7 * 5;
        MenuSubMenuBg        = EffectiveGlobalToken.ColorBgElevated;

        ItemIconSize = EffectiveGlobalToken.IconSize;
        
        MenuPopupMinWidth               = 160d;
        MenuPopupMaxWidth               = 800d;
        MenuPopupMaxHeight              = ItemHeight * 8;
        TopLevelItemPopupMarginToAnchor = EffectiveGlobalToken.UniformlyMarginXXS;
        
        MenuPopupBg               = EffectiveGlobalToken.ColorBgElevated;
        MenuPopupContentPadding   = new Thickness(0, EffectiveGlobalToken.UniformlyMarginXXS, 0, 0);
        VerticalItemsPanelSpacing = 0;
        VerticalChildItemsMargin  = new Thickness(0, 0, 0, EffectiveGlobalToken.UniformlyMarginXXS);

        InlineItemIndentUnit       = ItemHeight / 2;
        VerticalMenuContentPadding = new Thickness(0, EffectiveGlobalToken.UniformlyMarginXXS, 0, 0);
    }
    
}
