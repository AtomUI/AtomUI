using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Media;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
internal sealed class ButtonToken : AbstractControlDesignToken
{
    
    /// <summary>
    /// 文字字重
    /// </summary>
    public double FontWeight { get; set; }

    /// <summary>
    /// 默认按钮阴影
    /// </summary>
    public BoxShadows DefaultShadow { get; set; }

    /// <summary>
    /// 主要按钮阴影
    /// </summary>
    public BoxShadows PrimaryShadow { get; set; }

    /// <summary>
    /// 危险按钮阴影
    /// </summary>
    public BoxShadows DangerShadow { get; set; }

    /// <summary>
    /// 主要按钮文本颜色
    /// </summary>
    public Color PrimaryColor { get; set; }

    /// <summary>
    /// 默认按钮文本颜色
    /// </summary>
    public Color DefaultColor { get; set; }

    /// <summary>
    /// 默认按钮背景色
    /// </summary>
    public Color DefaultBg { get; set; }

    /// <summary>
    /// 默认按钮边框颜色
    /// </summary>
    public Color DefaultBorderColor { get; set; }

    /// <summary>
    /// 默认的禁用边框颜色
    /// </summary>
    public Color DefaultBorderColorDisabled { get; set; }

    /// <summary>
    /// 危险按钮文本颜色
    /// </summary>
    public Color DangerColor { get; set; }

    /// <summary>
    /// 默认按钮悬浮态背景色
    /// </summary>
    public Color DefaultHoverBg { get; set; }

    /// <summary>
    /// 默认按钮悬浮态文本颜色
    /// </summary>
    public Color DefaultHoverColor { get; set; }

    /// <summary>
    /// 默认按钮悬浮态边框颜色
    /// </summary>
    public Color DefaultHoverBorderColor { get; set; }

    /// <summary>
    /// 默认按钮激活态背景色
    /// </summary>
    public Color DefaultActiveBg { get; set; }

    /// <summary>
    /// 默认按钮激活态文字颜色
    /// </summary>
    public Color DefaultActiveColor { get; set; }

    /// <summary>
    /// 默认按钮激活态边框颜色
    /// </summary>
    public Color DefaultActiveBorderColor { get; set; }

    /// <summary>
    /// 禁用状态边框颜色
    /// </summary>
    public Color BorderColorDisabled { get; set; }

    /// <summary>
    /// 默认幽灵按钮文本颜色
    /// </summary>
    public Color DefaultGhostColor { get; set; }

    /// <summary>
    /// 幽灵按钮背景色
    /// </summary>
    public Color GhostBg { get; set; }

    /// <summary>
    /// 默认幽灵按钮边框颜色
    /// </summary>
    public Color DefaultGhostBorderColor { get; set; }
    
    // 主要填充按钮的浅色背景颜色
    // Background color of primary filled button
    
    /// <summary>
    /// 默认实心按钮的文本色
    /// Default text color for solid buttons.
    /// </summary>
    public Color SolidTextColor { get; set; }
    
    /// <summary>
    /// 默认文本按钮的文本色
    /// Default text color for text buttons on hover
    /// </summary>
    public Color TextTextColor { get; set; }
    
    /// <summary>
    /// 默认文本按钮悬浮态文本颜色
    /// Default text color for text buttons on hover
    /// </summary>
    public Color TextTextHoverColor { get; set; }
    
    /// <summary>
    /// 默认文本按钮激活态文字颜色
    /// Default text color for text buttons on active
    /// </summary>
    public Color TextTextActiveColor { get; set; }

    /// <summary>
    /// 按钮内间距
    /// </summary>
    public Thickness ContentPadding { get; set; }

    /// <summary>
    /// 大号按钮内间距
    /// </summary>
    public Thickness ContentPaddingLG { get; set; }

    /// <summary>
    /// 小号按钮内间距
    /// </summary>
    public Thickness ContentPaddingSM { get; set; }

    /// <summary>
    /// 按钮右边一个额外的区域内容控件间的间隔
    /// </summary>
    public double ExtraContentItemSpacing { get; set; }
    
    /// <summary>
    /// 按钮右边一个额外的区域对右侧的小号外边距
    /// </summary>
    public Thickness ExtraContentMarginSM { get; set; }

    /// <summary>
    /// 按钮右边一个额外的区域对右侧的外边距
    /// </summary>
    public Thickness ExtraContentMargin { get; set; }

    /// <summary>
    /// 按钮右边一个额外的区域对右侧的大号外边距
    /// </summary>
    public Thickness ExtraContentMarginLG { get; set; }

    /// <summary>
    /// 圆形按钮内间距
    /// </summary>
    public Thickness CirclePadding { get; set; }

    /// <summary>
    /// 只有图标的按钮图标尺寸
    /// </summary>
    public double OnlyIconSize { get; set; }

    /// <summary>
    /// 大号只有图标的按钮图标尺寸
    /// </summary>
    public double OnlyIconSizeLG { get; set; }

    /// <summary>
    /// 小号只有图标的按钮图标尺寸
    /// </summary>
    public double OnlyIconSizeSM { get; set; }

    /// <summary>
    /// 完成 Icon 外边距
    /// </summary>
    public Thickness IconMargin { get; set; }

    /// <summary>
    /// 位于内容右侧的 Icon 外边距
    /// </summary>
    public Thickness IconEndMargin { get; set; }

    /// <summary>
    /// 按钮组边框颜色
    /// </summary>
    public Color GroupBorderColor { get; set; }

    /// <summary>
    /// 链接按钮悬浮态背景色
    /// </summary>
    public Color LinkHoverBg { get; set; }

    /// <summary>
    /// 文本按钮悬浮态背景色
    /// </summary>
    public Color TextHoverBg { get; set; }

    /// <summary>
    /// 按钮内容字体大小
    /// </summary>
    public double ContentFontSize { get; set; }

    /// <summary>
    /// 大号按钮内容字体大小
    /// </summary>
    public double ContentFontSizeLG { get; set; }

    /// <summary>
    /// 小号按钮内容字体大小
    /// </summary>
    public double ContentFontSizeSM { get; set; }

    /// <summary>
    /// 按钮内容字体行高
    /// </summary>
    public double ContentLineHeight { get; set; }

    /// <summary>
    /// 大号按钮内容字体行高
    /// </summary>
    public double ContentLineHeightLG { get; set; }

    /// <summary>
    /// 小号按钮内容字体行高
    /// </summary>
    public double ContentLineHeightSM { get; set; }
    
    /// <summary>
    /// 按钮的下拉弹出菜单跟按钮之间的间距大小
    /// </summary>
    public double GutterToFlyout { get; set; }

    #region 内部 Token 定义

    /// <summary>
    /// IconOnly 按钮内间距
    /// </summary>
    public Thickness IconOnyPadding { get; set; }

    /// <summary>
    /// IconOnly 大号按钮内间距
    /// </summary>
    public Thickness IconOnyPaddingLG { get; set; }

    /// <summary>
    /// IconOnly 小号按钮内间距
    /// </summary>
    public Thickness IconOnyPaddingSM { get; set; }

    #endregion

    public ButtonToken()

    {
    }

    public override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        var fontSize   = EffectiveGlobalToken.FontSize;
        var fontSizeLG = EffectiveGlobalToken.FontSizeLG;

        if (!IsExplicit(ButtonTokenKind.ContentFontSize))
        {
            ContentFontSize = fontSize;
        }

        if (!IsExplicit(ButtonTokenKind.ContentFontSizeSM))
        {
            ContentFontSizeSM = fontSize;
        }

        if (!IsExplicit(ButtonTokenKind.ContentFontSizeLG))
        {
            ContentFontSizeLG = fontSizeLG;
        }

        if (!IsExplicit(ButtonTokenKind.ContentLineHeight))
        {
            ContentLineHeight = CalculatorUtils.CalculateLineHeight(ContentFontSize) * ContentFontSize;
        }

        if (!IsExplicit(ButtonTokenKind.ContentLineHeightSM))
        {
            ContentLineHeightSM = CalculatorUtils.CalculateLineHeight(ContentFontSizeSM) * ContentFontSizeSM;
        }

        if (!IsExplicit(ButtonTokenKind.ContentLineHeightLG))
        {
            ContentLineHeightLG = CalculatorUtils.CalculateLineHeight(ContentFontSizeLG) * ContentFontSizeLG;
        }

        var controlOutlineWidth = EffectiveGlobalToken.ControlOutlineWidth;
        FontWeight = 400;
        DefaultShadow = new BoxShadows(new BoxShadow
        {
            OffsetX = 0,
            OffsetY = controlOutlineWidth,
            Blur    = 3,
            Spread  = 0,
            Color   = EffectiveGlobalToken.ColorControlOutline
        });

        PrimaryShadow = new BoxShadows(new BoxShadow
        {
            OffsetX = 0,
            OffsetY = controlOutlineWidth,
            Blur    = 3,
            Spread  = 0,
            Color   = EffectiveGlobalToken.ColorControlOutline
        });

        DangerShadow = new BoxShadows(new BoxShadow
        {
            OffsetX = 0,
            OffsetY = controlOutlineWidth,
            Blur    = 3,
            Spread  = 0,
            Color   = EffectiveGlobalToken.ColorErrorOutline
        });
        
        var lineWidth         = EffectiveGlobalToken.LineWidth;

        PrimaryColor            = EffectiveGlobalToken.ColorTextLightSolid;
        DangerColor             = EffectiveGlobalToken.ColorTextLightSolid;
        BorderColorDisabled     = EffectiveGlobalToken.ColorBorder;
        DefaultGhostColor       = EffectiveGlobalToken.ColorBgContainer;
        GhostBg                 = Colors.Transparent;
        DefaultGhostBorderColor = EffectiveGlobalToken.ColorBgContainer;

        GroupBorderColor           = EffectiveGlobalToken.ColorPrimaryHover;
        LinkHoverBg                = Colors.Transparent;
        TextHoverBg                = EffectiveGlobalToken.ColorFillTertiary;
        DefaultColor               = EffectiveGlobalToken.ColorText;
        DefaultBg                  = EffectiveGlobalToken.ColorBgContainer;
        DefaultBorderColor         = EffectiveGlobalToken.ColorBorder;
        DefaultBorderColorDisabled = EffectiveGlobalToken.ColorBorder;
        DefaultHoverBg             = EffectiveGlobalToken.ColorBgContainer;
        DefaultHoverColor          = EffectiveGlobalToken.ColorPrimaryHover;
        DefaultHoverBorderColor    = EffectiveGlobalToken.ColorPrimaryHover;
        DefaultActiveBg            = EffectiveGlobalToken.ColorBgContainer;
        DefaultActiveColor         = EffectiveGlobalToken.ColorPrimaryActive;
        DefaultActiveBorderColor   = EffectiveGlobalToken.ColorPrimaryActive;

        var isBright = ColorUtils.IsBright(EffectiveGlobalToken.ColorBgSolid, Colors.White);
        if (isBright)
        {
            SolidTextColor = Colors.Black;
        }
        else
        {
            SolidTextColor = Colors.White;
        }

        TextTextColor       = EffectiveGlobalToken.ColorText;
        TextTextHoverColor  = EffectiveGlobalToken.ColorText;
        TextTextActiveColor = EffectiveGlobalToken.ColorText;

        var controlHeightSM = EffectiveGlobalToken.ControlHeightSM;
        var controlHeight   = EffectiveGlobalToken.ControlHeight;
        var controlHeightLG = EffectiveGlobalToken.ControlHeightLG;

        if (!IsExplicit(ButtonTokenKind.ContentPaddingSM))
        {
            ContentPaddingSM = new Thickness(8 - EffectiveGlobalToken.LineWidth,
                Math.Max((controlHeightSM - ContentLineHeightSM) / 2 - lineWidth, 0));
        }

        if (!IsExplicit(ButtonTokenKind.ContentPadding))
        {
            ContentPadding = new Thickness(EffectiveGlobalToken.PaddingContentHorizontal - lineWidth,
                Math.Max((controlHeight - ContentLineHeight) / 2 - lineWidth, 0));
        }

        if (!IsExplicit(ButtonTokenKind.ContentPaddingLG))
        {
            ContentPaddingLG = new Thickness(EffectiveGlobalToken.PaddingContentHorizontal - lineWidth,
                Math.Max((controlHeightLG - ContentLineHeightLG) / 2 - lineWidth, 0));
        }

        ExtraContentMarginSM    = new Thickness(ContentPaddingSM.Left / 2, 0, 0, 0);
        ExtraContentMargin      = new Thickness(ContentPadding.Left / 2, 0, 0, 0);
        ExtraContentMarginLG    = new Thickness(ContentPaddingLG.Left / 2, 0, 0, 0);
        ExtraContentItemSpacing = EffectiveGlobalToken.UniformlyMarginXXS / 2;

        CirclePadding  = new Thickness(ContentPaddingSM.Left / 2);
        OnlyIconSizeSM = EffectiveGlobalToken.IconSize;
        OnlyIconSize   = EffectiveGlobalToken.IconSizeLG;
        OnlyIconSizeLG = EffectiveGlobalToken.IconSizeLG;

        IconMargin    = new Thickness(0, 0, EffectiveGlobalToken.UniformlyPaddingXXS, 0);
        IconEndMargin = new Thickness(EffectiveGlobalToken.UniformlyPaddingXXS, 0, 0, 0);

        IconOnyPadding   = new Thickness(Math.Max((controlHeight - ContentLineHeight) / 2 - lineWidth, 0));
        IconOnyPaddingLG = new Thickness(Math.Max((controlHeightLG - ContentLineHeightLG) / 2 - lineWidth, 0));
        IconOnyPaddingSM = new Thickness(Math.Max((controlHeightSM - ContentLineHeightSM) / 2 - lineWidth, 0));
        
        GutterToFlyout = EffectiveGlobalToken.UniformlyMarginXXS;
    }
    
}
