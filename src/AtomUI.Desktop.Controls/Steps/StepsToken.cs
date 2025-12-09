using AtomUI.Theme.TokenSystem;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
internal class StepsToken : AbstractControlDesignToken
{
    public const string ID = "Steps";
    
    public StepsToken()
        : base(ID)
    {
    }
    
    /// <summary>
    /// 描述区域最大宽度
    /// Max width of description area
    /// </summary>
    public double DescriptionMaxWidth { get; set; }
    
    /// <summary>
    /// 自定义图标容器尺寸
    /// Size of custom icon container
    /// </summary>
    public double CustomIconSize { get; set; }
    
    /// <summary>
    /// 自定义图标大小
    /// Font size of custom icon
    /// </summary>
    public double CustomIconFontSize { get; set; }
    
    /// <summary>
    /// 图标容器尺寸
    /// Size of icon container
    /// </summary>
    public double IconSize { get; set; }
    
    /// <summary>
    /// 图标大小
    /// Size of icon
    /// </summary>
    public double IconFontSize { get; set; }
    
    /// <summary>
    /// 点状步骤点大小
    /// Size of dot
    /// </summary>
    public double DotSize { get; set; }
    
    /// <summary>
    /// 点状步骤点当前大小
    /// Current size of dot
    /// </summary>
    public double DotCurrentSize { get; set; }
    
    /// <summary>
    /// 水平点状步骤点外间距
    /// </summary>
    public Thickness HorizontalDotMargin { get; set; }
    
    /// <summary>
    /// 垂直点状步骤点外间距
    /// </summary>
    public Thickness VerticalDotMargin { get; set; }
    
    /// <summary>
    /// 可跳转步骤条箭头颜色
    /// Color of arrow in nav
    /// </summary>
    public Color NavArrowColor { get; set; }
    
    /// <summary>
    /// 小号步骤条图标大小
    /// Size of small steps icon
    /// </summary>
    public double IconSizeSM { get; set; }
    
    /// <summary>
    /// Label 水平排列的时候的外间距
    /// External spacing when Label is arranged horizontally
    /// </summary>
    public Thickness HorizontalHeaderMargin { get; set; }
    
    /// <summary>
    /// 垂直排列的时候 item 的间距
    /// </summary>
    public double VerticalItemSpacing { get; set; }
    
    /// <summary>
    /// 垂直排列描述的内间距
    /// </summary>
    public Thickness VerticalDescriptionPadding { get; set; }
    
    /// <summary>
    /// 垂直标签排列时候内容跟图标之间的外间距
    /// </summary>
    public Thickness VerticalLabelContentMargin { get; set; }
    
    /// <summary>
    /// 垂直导航类型箭头的外间距
    /// </summary>
    public Thickness VerticalNavArrowMargin { get; set; }
    
    /// <summary>
    /// 垂直导航类型箭头的外间距，小尺寸
    /// </summary>
    public Thickness VerticalNavArrowMarginSM { get; set; }
    
    /// <summary>
    /// 垂直导航类型内容和指示线的间距
    /// </summary>
    public double NavItemGutter { get; set; }
    
    /// <summary>
    /// 垂直导航类型内容和指示线的间距，小尺寸
    /// </summary>
    public double NavItemGutterSM { get; set; }

    #region 内部 Token

    public Color WaitIconColor { get; set; }
    public Color WaitIconBgColor { get; set; }
    public Color WaitIconBorderColor { get; set; }
    public Color FinishIconBgColor { get; set; }
    public Color FinishIconBorderColor { get; set; }
    
    public Color ProcessTailColor { get; set; }
    public Color ProcessIconColor { get; set; }
    public Color ProcessTitleColor { get; set; }
    public Color ProcessDescriptionColor { get; set; }
    public Color ProcessIconBgColor { get; set; }
    public Color ProcessIconBorderColor { get; set; }
    public Color ProcessDotColor { get; set; }
    public Color WaitTitleColor { get; set; }
    public Color WaitDescriptionColor { get; set; }
    public Color WaitTailColor { get; set; }
    public Color WaitDotColor { get; set; }
    public Color FinishIconColor { get; set; }
    public Color FinishTitleColor { get; set; }
    public Color FinishDescriptionColor { get; set; }
    public Color FinishTailColor { get; set; }
    public Color FinishDotColor { get; set; }
    public Color ErrorIconColor { get; set; }
    public Color ErrorTitleColor { get; set; }
    public Color ErrorDescriptionColor { get; set; }
    public Color ErrorTailColor { get; set; }
    public Color ErrorIconBgColor { get; set; }
    public Color ErrorIconBorderColor { get; set; }
    public Color ErrorDotColor { get; set; }
    public Color StepsNavActiveColor { get; set; }
    public double StepsProgressSize { get; set; }
    // Steps inline variable
    public double InlineDotSize { get; set; }
    public Thickness InlineHeaderMargin { get; set; }
    public Thickness InlineHeaderPadding { get; set; }
    public Thickness InlineItemPadding { get; set; }
    public Color InlineTitleColor { get; set; }
    public Color InlineTailColor { get; set; }
    public double DotLineThickness { get; set; }
    public Thickness ProgressFramePadding { get; set; }
    public Thickness ProgressFramePaddingSM { get; set; }
    public Color ProgressGrooveColor { get; set; }
    public Color ProgressColor { get; set; }
    
    #endregion

    public override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        
        CustomIconSize      = SharedToken.ControlHeight;
        CustomIconFontSize  = SharedToken.ControlHeightSM;
        IconSize            = SharedToken.ControlHeight;
        IconFontSize        = SharedToken.FontSize;
        IconSizeSM          = SharedToken.FontSizeHeading3;
        DotSize             = SharedToken.ControlHeight / 4;
        DotCurrentSize      = SharedToken.ControlHeightLG / 4;
        NavArrowColor       = SharedToken.ColorTextDisabled;
        DescriptionMaxWidth = 140;

        WaitIconColor         = SharedToken.ColorTextLabel;
        WaitIconBgColor       = SharedToken.ColorFillContent;
        WaitIconBorderColor   = Colors.Transparent;
        WaitTitleColor        = SharedToken.ColorTextDescription;
        WaitDescriptionColor  = SharedToken.ColorTextDescription;
        WaitTailColor         = SharedToken.ColorSplit;
        WaitDotColor          = SharedToken.ColorTextDisabled;
        
        ProcessIconColor        = SharedToken.ColorTextLightSolid;
        ProcessTitleColor       = SharedToken.ColorText;
        ProcessDescriptionColor = SharedToken.ColorText;
        ProcessIconBgColor      = SharedToken.ColorPrimary;
        ProcessIconBorderColor  = SharedToken.ColorPrimary;
        ProcessDotColor         = SharedToken.ColorPrimary;
        ProcessTailColor        = SharedToken.ColorSplit;

        ProgressGrooveColor = SharedToken.ColorSplit;
        ProgressColor       = SharedToken.ColorPrimary;
        
        FinishIconBgColor       = SharedToken.ControlItemBgActive;
        FinishIconBorderColor   = SharedToken.ControlItemBgActive;
        FinishIconColor         = SharedToken.ColorPrimary;
        FinishTitleColor        = SharedToken.ColorText;
        FinishDescriptionColor  = SharedToken.ColorTextDescription;
        FinishTailColor         = SharedToken.ColorPrimary;
        FinishDotColor          = SharedToken.ColorPrimary;
        
        ErrorIconColor          = SharedToken.ColorTextLightSolid;
        ErrorTitleColor         = SharedToken.ColorError;
        ErrorDescriptionColor   = SharedToken.ColorError;
        ErrorTailColor          = SharedToken.ColorSplit;
        ErrorIconBgColor        = SharedToken.ColorError;
        ErrorIconBorderColor    = SharedToken.ColorError;
        ErrorDotColor           = SharedToken.ColorError;
        StepsNavActiveColor     = SharedToken.ColorPrimary;
        StepsProgressSize       = SharedToken.ControlHeightLG;
        // Steps inline variable
        InlineDotSize       = 6;
        InlineHeaderMargin  = new Thickness(0, SharedToken.UniformlyMarginXS - SharedToken.LineWidth, 0, 0);
        InlineHeaderPadding = new Thickness(SharedToken.UniformlyMarginXS, 0);
        InlineTitleColor    = SharedToken.ColorTextQuaternary;
        InlineTailColor     = SharedToken.ColorBorderSecondary;
        InlineItemPadding   = new Thickness(0, SharedToken.UniformlyMarginXS);
        
        HorizontalHeaderMargin     = new Thickness(0, 0, SharedToken.UniformlyMargin, 0);
        HorizontalHeaderMargin     = new Thickness(0, 0, SharedToken.UniformlyMargin, 0);
        VerticalItemSpacing        = SharedToken.SpacingXXS * 1.5;
        VerticalDescriptionPadding = new Thickness(0, 0, 0, SharedToken.UniformlyPaddingXS);
        DotLineThickness           = SharedToken.LineWidth * 3;

        HorizontalDotMargin        = new Thickness(SharedToken.UniformlyMarginXS, 0);
        VerticalDotMargin          = new Thickness(0, SharedToken.UniformlyMarginXS);
        VerticalLabelContentMargin = new Thickness(0, SharedToken.UniformlyMarginSM, 0, 0);
        VerticalNavArrowMargin     = new Thickness(0, SharedToken.UniformlyMarginSM);
        VerticalNavArrowMarginSM   = new Thickness(0, SharedToken.UniformlyMarginXS);
        NavItemGutter              = SharedToken.Spacing;
        NavItemGutterSM            = SharedToken.SpacingXS;
        ProgressFramePadding       = new Thickness(SharedToken.LineWidthBold * 2);
        ProgressFramePaddingSM     = new Thickness(SharedToken.LineWidthBold);
    }
}