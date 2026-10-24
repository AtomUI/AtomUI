using AtomUI.Media;
using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
internal sealed class NavMenuToken : AbstractControlDesignToken
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
    /// </summary>
    public Color MenuSubMenuBg { get; set; }

    #endregion
    
    public NavMenuToken()

    {
    }

    public override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        
        var colorTextLightSolid = EffectiveGlobalToken.ColorTextLightSolid;
        
        var activeBarWidth = !double.IsNaN(ActiveBarScaleX) ? ActiveBarScaleX : 1.0d;
        var activeBarHeight = !double.IsNaN(ActiveBarHeight)
            ? ActiveBarHeight
            : EffectiveGlobalToken.LineWidthBold;
        var itemContentMargin = ItemContentMargin != default
            ? ItemContentMargin
            : new Thickness(
                EffectiveGlobalToken.UniformlyMarginXXS,
                0,
                EffectiveGlobalToken.UniformlyMarginXXS,
                EffectiveGlobalToken.UniformlyMarginXXS);
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
        ItemBackgroundMotionEasing  = Easing.Parse("0.25,0.1,0.25,1");
        HorizontalItemSelectedBg    = Colors.Transparent;
        ActiveBarScaleX              = activeBarWidth;
        ActiveBarHeight             = activeBarHeight;
        
        // Disabled
        ItemDisabledColor = EffectiveGlobalToken.ColorTextDisabled;
        
        // Danger
        DangerItemColor         = EffectiveGlobalToken.ColorError;
        DangerItemHoverColor    = EffectiveGlobalToken.ColorError;
        DangerItemSelectedColor = EffectiveGlobalToken.ColorError;
        DangerItemActiveBg      = EffectiveGlobalToken.ColorError;
        DangerItemSelectedBg    = EffectiveGlobalToken.ColorError;

        KeyGestureColor = EffectiveGlobalToken.ColorTextSecondary;
        
        ItemContentMargin    = itemContentMargin;
        ItemContentPadding   = new Thickness(EffectiveGlobalToken.UniformlyPadding, EffectiveGlobalToken.UniformlyPaddingXXS);
        ItemMargin           = new Thickness(0, 0, EffectiveGlobalToken.UniformlyMarginXS, 0);
        ItemHeight           = EffectiveGlobalToken.ControlHeightLG;
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
        TopLevelItemPopupMarginToAnchor = EffectiveGlobalToken.UniformlyMarginXS;
        
        MenuPopupBg               = EffectiveGlobalToken.ColorBgElevated;
        MenuPopupContentPadding   = new Thickness(0, EffectiveGlobalToken.UniformlyMarginXXS, 0, 0);
        VerticalItemsPanelSpacing = 0;
        VerticalChildItemsMargin  = new Thickness(0, 0, 0, EffectiveGlobalToken.UniformlyMarginXXS);

        InlineItemIndentUnit       = ItemHeight / 2;
        VerticalMenuContentPadding = new Thickness(0, EffectiveGlobalToken.UniformlyMarginXXS, 0, 0);
    }
    
}
