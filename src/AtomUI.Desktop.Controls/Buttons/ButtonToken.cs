using AtomUI.Media;
using AtomUI.Theme.Styling;
using AtomUI.Theme.TokenSystem;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
internal class ButtonToken : AbstractControlDesignToken
{
    public const string ID = "Button";

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
    public Thickness Padding { get; set; }

    /// <summary>
    /// 大号按钮内间距
    /// </summary>
    public Thickness PaddingLG { get; set; }

    /// <summary>
    /// 小号按钮内间距
    /// </summary>
    public Thickness PaddingSM { get; set; }

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
    /// 图标的按钮图标尺寸
    /// </summary>
    public double IconSize { get; set; }

    /// <summary>
    /// 只有图标的按钮图标尺寸
    /// </summary>
    public double IconSizeLG { get; set; }

    /// <summary>
    /// 只有图标的按钮图标尺寸
    /// </summary>
    public double IconSizeSM { get; set; }

    /// <summary>
    /// 完成 Icon 外边距
    /// </summary>
    public Thickness IconMargin { get; set; }

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
    public double ContentFontSize { get; set; } = double.NaN;

    /// <summary>
    /// 大号按钮内容字体大小
    /// </summary>
    public double ContentFontSizeLG { get; set; } = double.NaN;

    /// <summary>
    /// 小号按钮内容字体大小
    /// </summary>
    public double ContentFontSizeSM { get; set; } = double.NaN;

    /// <summary>
    /// 按钮内容字体行高
    /// </summary>
    public double ContentLineHeight { get; set; } = double.NaN;

    /// <summary>
    /// 大号按钮内容字体行高
    /// </summary>
    public double ContentLineHeightLG { get; set; } = double.NaN;

    /// <summary>
    /// 小号按钮内容字体行高
    /// </summary>
    public double ContentLineHeightSM { get; set; } = double.NaN;

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
        : base(ID)
    {
    }

    public override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        var fontSize   = SharedToken.FontSize;
        var fontSizeLG = SharedToken.FontSizeLG;

        ContentFontSize   = !double.IsNaN(ContentFontSize) ? ContentFontSize : fontSize;
        ContentFontSizeSM = !double.IsNaN(ContentFontSizeSM) ? ContentFontSizeSM : fontSize;
        ContentFontSizeLG = !double.IsNaN(ContentFontSizeLG) ? ContentFontSizeLG : fontSizeLG;
        ContentLineHeight = !double.IsNaN(ContentLineHeight)
            ? ContentLineHeight
            : CalculatorUtils.CalculateLineHeight(ContentFontSize) * ContentFontSize;
        ContentLineHeightSM = !double.IsNaN(ContentLineHeightSM)
            ? ContentLineHeightSM
            : CalculatorUtils.CalculateLineHeight(ContentFontSizeSM) * ContentFontSizeSM;
        ContentLineHeightLG = !double.IsNaN(ContentLineHeightLG)
            ? ContentLineHeightLG
            : CalculatorUtils.CalculateLineHeight(ContentFontSizeLG) * ContentFontSizeLG;

        var controlOutlineWidth = SharedToken.ControlOutlineWidth;
        FontWeight = 400;
        DefaultShadow = new BoxShadows(new BoxShadow
        {
            OffsetX = 0,
            OffsetY = controlOutlineWidth,
            Blur    = 3,
            Spread  = 0,
            Color   = SharedToken.ColorControlOutline
        });

        PrimaryShadow = new BoxShadows(new BoxShadow
        {
            OffsetX = 0,
            OffsetY = controlOutlineWidth,
            Blur    = 3,
            Spread  = 0,
            Color   = SharedToken.ColorControlOutline
        });

        DangerShadow = new BoxShadows(new BoxShadow
        {
            OffsetX = 0,
            OffsetY = controlOutlineWidth,
            Blur    = 3,
            Spread  = 0,
            Color   = SharedToken.ColorErrorOutline
        });
        
        var lineWidth         = SharedToken.LineWidth;

        PrimaryColor            = SharedToken.ColorTextLightSolid;
        DangerColor             = SharedToken.ColorTextLightSolid;
        BorderColorDisabled     = SharedToken.ColorBorder;
        DefaultGhostColor       = SharedToken.ColorBgContainer;
        GhostBg                 = Colors.Transparent;
        DefaultGhostBorderColor = SharedToken.ColorBgContainer;

        GroupBorderColor           = SharedToken.ColorPrimaryHover;
        LinkHoverBg                = Colors.Transparent;
        TextHoverBg                = SharedToken.ColorBgTextHover;
        DefaultColor               = SharedToken.ColorText;
        DefaultBg                  = SharedToken.ColorBgContainer;
        DefaultBorderColor         = SharedToken.ColorBorder;
        DefaultBorderColorDisabled = SharedToken.ColorBorder;
        DefaultHoverBg             = SharedToken.ColorBgContainer;
        DefaultHoverColor          = SharedToken.ColorPrimaryHover;
        DefaultHoverBorderColor    = SharedToken.ColorPrimaryHover;
        DefaultActiveBg            = SharedToken.ColorBgContainer;
        DefaultActiveColor         = SharedToken.ColorPrimaryActive;
        DefaultActiveBorderColor   = SharedToken.ColorPrimaryActive;

        var isBright = ColorUtils.IsBright(SharedToken.ColorBgSolid, Colors.White);
        if (isBright)
        {
            SolidTextColor = Colors.Black;
        }
        else
        {
            SolidTextColor = Colors.White;
        }

        TextTextColor       = SharedToken.ColorText;
        TextTextHoverColor  = SharedToken.ColorText;
        TextTextActiveColor = SharedToken.ColorText;

        var controlHeightSM = SharedToken.ControlHeightSM;
        var controlHeight   = SharedToken.ControlHeight;
        var controlHeightLG = SharedToken.ControlHeightLG;

        PaddingSM = new Thickness(8 - SharedToken.LineWidth,
            Math.Max((controlHeightSM - ContentLineHeightSM) / 2 - lineWidth, 0));
        Padding = new Thickness(SharedToken.PaddingContentHorizontal - lineWidth,
            Math.Max((controlHeight - ContentLineHeight) / 2 - lineWidth, 0));
        PaddingLG = new Thickness(SharedToken.PaddingContentHorizontal - lineWidth,
            Math.Max((controlHeightLG - ContentLineHeightLG) / 2 - lineWidth, 0));

        ExtraContentMarginSM    = new Thickness(PaddingSM.Left / 2, 0, 0, 0);
        ExtraContentMargin      = new Thickness(Padding.Left / 2, 0, 0, 0);
        ExtraContentMarginLG    = new Thickness(PaddingLG.Left / 2, 0, 0, 0);
        ExtraContentItemSpacing = SharedToken.UniformlyMarginXXS / 2;

        CirclePadding  = new Thickness(PaddingSM.Left / 2);
        OnlyIconSizeSM = SharedToken.IconSize;
        OnlyIconSize   = SharedToken.IconSizeLG;
        OnlyIconSizeLG = SharedToken.IconSizeLG;

        IconSizeSM = SharedToken.IconSizeSM;
        IconSize   = SharedToken.IconSize;
        IconSizeLG = SharedToken.IconSize;

        IconMargin = new Thickness(0, 0, SharedToken.UniformlyPaddingXXS, 0);

        IconOnyPadding   = new Thickness(Math.Max((controlHeight - ContentLineHeight) / 2 - lineWidth, 0));
        IconOnyPaddingLG = new Thickness(Math.Max((controlHeightLG - ContentLineHeightLG) / 2 - lineWidth, 0));
        IconOnyPaddingSM = new Thickness(Math.Max((controlHeightSM - ContentLineHeightSM) / 2 - lineWidth, 0));
    }
}