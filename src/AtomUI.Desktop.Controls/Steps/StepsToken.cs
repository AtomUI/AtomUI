using AtomUI.Theme;
using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken(ControlTokenEvaluationVersion.DependencyDefaultsV2, Revision = 1)]
internal sealed partial class StepsToken : AbstractControlDesignToken, IControlTokenDefaults
{

    public StepsToken()
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
    public double IconContainerSize { get; set; }

    /// <summary>
    /// 图标容器圆角（默认全圆）
    /// Corner radius of icon container (full circle by default)
    /// </summary>
    public CornerRadius IconContainerCornerRadius { get; set; }

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
    /// 可跳转步骤条箭头颜色
    /// Color of arrow in nav
    /// </summary>
    public Color NavArrowColor { get; set; }

    /// <summary>
    /// 小号步骤条图标大小
    /// Size of small steps icon
    /// </summary>
    public double IconContainerSizeSM { get; set; }

    /// <summary>
    /// Label 水平排列的时候的外间距
    /// External spacing when Label is arranged horizontally
    /// </summary>
    public Thickness HorizontalHeaderMargin { get; set; }

    /// <summary>
    /// 子标题外间距
    /// External spacing of subtitle
    /// </summary>
    public Thickness SubHeaderMargin { get; set; }

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
    /// 垂直导航类型 item 内边距
    /// </summary>
    public Thickness VerticalNavItemPadding { get; set; }

    /// <summary>
    /// 垂直导航类型内容和指示线的间距
    /// </summary>
    public double NavItemGutter { get; set; }

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
    public double PanelArrowWidth { get; set; }
    public double PanelArrowWidthSM { get; set; }
    public Color PanelWaitBackground { get; set; }
    public Color PanelProcessBackground { get; set; }
    public Color PanelFinishBackground { get; set; }
    public Color PanelErrorBackground { get; set; }
    public Color PanelWaitActiveBackground { get; set; }
    public Color PanelProcessActiveBackground { get; set; }
    public Color PanelFinishActiveBackground { get; set; }
    public Color PanelErrorActiveBackground { get; set; }
    public Color PanelWaitTextColor { get; set; }
    public Color PanelProcessTextColor { get; set; }
    public Color PanelFinishTextColor { get; set; }
    public Color PanelErrorTextColor { get; set; }
    public Color PanelActiveTextColor { get; set; }
    public Color PanelErrorActiveTextColor { get; set; }
    public Thickness PanelFirstBorderThickness { get; set; }
    public Thickness PanelMiddleBorderThickness { get; set; }
    public Thickness PanelLastBorderThickness { get; set; }
    public CornerRadius PanelFirstCornerRadius { get; set; }
    public CornerRadius PanelLastCornerRadius { get; set; }
    public CornerRadius PanelFirstCornerRadiusSM { get; set; }
    public CornerRadius PanelLastCornerRadiusSM { get; set; }

    #endregion

    [ControlTokenDefault(nameof(DescriptionMaxWidth))]
    private static double DefaultForDescriptionMaxWidth(in ControlTokenDefaultContext context) =>
        140;

    [ControlTokenDefault(nameof(CustomIconSize))]
    private static double DefaultForCustomIconSize(in ControlTokenDefaultContext context) =>
        context.Global.ControlHeight;

    [ControlTokenDefault(nameof(CustomIconFontSize))]
    private static double DefaultForCustomIconFontSize(in ControlTokenDefaultContext context) =>
        context.Global.ControlHeightSM;

    [ControlTokenDefault(nameof(IconContainerSize))]
    private static double DefaultForIconContainerSize(in ControlTokenDefaultContext context) =>
        context.Global.ControlHeight;

    [ControlTokenDefault(nameof(IconContainerCornerRadius), nameof(IconContainerSize))]
    private static CornerRadius DefaultForIconContainerCornerRadius(in ControlTokenDefaultContext context, double iconContainerSize) =>
        new CornerRadius(iconContainerSize);

    [ControlTokenDefault(nameof(IconFontSize))]
    private static double DefaultForIconFontSize(in ControlTokenDefaultContext context) =>
        context.Global.FontSize;

    [ControlTokenDefault(nameof(DotSize))]
    private static double DefaultForDotSize(in ControlTokenDefaultContext context) =>
        context.Global.ControlHeight / 4;

    [ControlTokenDefault(nameof(DotCurrentSize))]
    private static double DefaultForDotCurrentSize(in ControlTokenDefaultContext context) =>
        context.Global.ControlHeightLG / 4;

    [ControlTokenDefault(nameof(NavArrowColor))]
    private static Color DefaultForNavArrowColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorTextDisabled;

    [ControlTokenDefault(nameof(IconContainerSizeSM))]
    private static double DefaultForIconContainerSizeSM(in ControlTokenDefaultContext context) =>
        context.Global.FontSizeHeading3;

    [ControlTokenDefault(nameof(HorizontalHeaderMargin))]
    private static Thickness DefaultForHorizontalHeaderMargin(in ControlTokenDefaultContext context) =>
        new Thickness(context.Global.UniformlyMargin, 0);

    [ControlTokenDefault(nameof(SubHeaderMargin))]
    private static Thickness DefaultForSubHeaderMargin(in ControlTokenDefaultContext context) =>
        new Thickness(context.Global.UniformlyMarginXS, 0, 0, 0);

    [ControlTokenDefault(nameof(VerticalItemSpacing))]
    private static double DefaultForVerticalItemSpacing(in ControlTokenDefaultContext context) =>
        context.Global.UniformlyMarginXXS;

    [ControlTokenDefault(nameof(VerticalDescriptionPadding))]
    private static Thickness DefaultForVerticalDescriptionPadding(in ControlTokenDefaultContext context) =>
        new Thickness();

    [ControlTokenDefault(nameof(VerticalLabelContentMargin))]
    private static Thickness DefaultForVerticalLabelContentMargin(in ControlTokenDefaultContext context) =>
        new Thickness(0, context.Global.UniformlyMarginSM, 0, 0);

    [ControlTokenDefault(nameof(VerticalNavArrowMargin))]
    private static Thickness DefaultForVerticalNavArrowMargin(in ControlTokenDefaultContext context) =>
        new Thickness();

    [ControlTokenDefault(nameof(VerticalNavArrowMarginSM))]
    private static Thickness DefaultForVerticalNavArrowMarginSM(in ControlTokenDefaultContext context) =>
        new Thickness();

    [ControlTokenDefault(nameof(VerticalNavItemPadding))]
    private static Thickness DefaultForVerticalNavItemPadding(in ControlTokenDefaultContext context) =>
        new Thickness(context.Global.UniformlyPaddingSM, context.Global.UniformlyPadding);

    [ControlTokenDefault(nameof(NavItemGutter))]
    private static double DefaultForNavItemGutter(in ControlTokenDefaultContext context) =>
        context.Global.Spacing;

    [ControlTokenDefault(nameof(WaitIconColor))]
    private static Color DefaultForWaitIconColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorTextLabel;

    [ControlTokenDefault(nameof(WaitIconBgColor))]
    private static Color DefaultForWaitIconBgColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorFillContent;

    [ControlTokenDefault(nameof(WaitIconBorderColor))]
    private static Color DefaultForWaitIconBorderColor(in ControlTokenDefaultContext context) =>
        Colors.Transparent;

    [ControlTokenDefault(nameof(FinishIconBgColor))]
    private static Color DefaultForFinishIconBgColor(in ControlTokenDefaultContext context) =>
        context.Global.ControlItemBgActive;

    [ControlTokenDefault(nameof(FinishIconBorderColor))]
    private static Color DefaultForFinishIconBorderColor(in ControlTokenDefaultContext context) =>
        context.Global.ControlItemBgActive;

    [ControlTokenDefault(nameof(ProcessTailColor))]
    private static Color DefaultForProcessTailColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorPrimary;

    [ControlTokenDefault(nameof(ProcessIconColor))]
    private static Color DefaultForProcessIconColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorTextLightSolid;

    [ControlTokenDefault(nameof(ProcessTitleColor))]
    private static Color DefaultForProcessTitleColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorText;

    [ControlTokenDefault(nameof(ProcessDescriptionColor))]
    private static Color DefaultForProcessDescriptionColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorText;

    [ControlTokenDefault(nameof(ProcessIconBgColor))]
    private static Color DefaultForProcessIconBgColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorPrimary;

    [ControlTokenDefault(nameof(ProcessIconBorderColor))]
    private static Color DefaultForProcessIconBorderColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorPrimary;

    [ControlTokenDefault(nameof(ProcessDotColor))]
    private static Color DefaultForProcessDotColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorPrimary;

    [ControlTokenDefault(nameof(WaitTitleColor))]
    private static Color DefaultForWaitTitleColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorTextDescription;

    [ControlTokenDefault(nameof(WaitDescriptionColor))]
    private static Color DefaultForWaitDescriptionColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorTextDescription;

    [ControlTokenDefault(nameof(WaitTailColor))]
    private static Color DefaultForWaitTailColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorTextDisabled;

    [ControlTokenDefault(nameof(WaitDotColor))]
    private static Color DefaultForWaitDotColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorTextDisabled;

    [ControlTokenDefault(nameof(FinishIconColor))]
    private static Color DefaultForFinishIconColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorPrimary;

    [ControlTokenDefault(nameof(FinishTitleColor))]
    private static Color DefaultForFinishTitleColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorText;

    [ControlTokenDefault(nameof(FinishDescriptionColor))]
    private static Color DefaultForFinishDescriptionColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorTextDescription;

    [ControlTokenDefault(nameof(FinishTailColor))]
    private static Color DefaultForFinishTailColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorPrimary;

    [ControlTokenDefault(nameof(FinishDotColor))]
    private static Color DefaultForFinishDotColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorPrimary;

    [ControlTokenDefault(nameof(ErrorIconColor))]
    private static Color DefaultForErrorIconColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorTextLightSolid;

    [ControlTokenDefault(nameof(ErrorTitleColor))]
    private static Color DefaultForErrorTitleColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorError;

    [ControlTokenDefault(nameof(ErrorDescriptionColor))]
    private static Color DefaultForErrorDescriptionColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorError;

    [ControlTokenDefault(nameof(ErrorTailColor))]
    private static Color DefaultForErrorTailColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorError;

    [ControlTokenDefault(nameof(ErrorIconBgColor))]
    private static Color DefaultForErrorIconBgColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorError;

    [ControlTokenDefault(nameof(ErrorIconBorderColor))]
    private static Color DefaultForErrorIconBorderColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorError;

    [ControlTokenDefault(nameof(ErrorDotColor))]
    private static Color DefaultForErrorDotColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorError;

    [ControlTokenDefault(nameof(StepsNavActiveColor))]
    private static Color DefaultForStepsNavActiveColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorPrimary;

    [ControlTokenDefault(nameof(InlineDotSize))]
    private static double DefaultForInlineDotSize(in ControlTokenDefaultContext context) =>
        6;

    [ControlTokenDefault(nameof(InlineHeaderMargin))]
    private static Thickness DefaultForInlineHeaderMargin(in ControlTokenDefaultContext context) =>
        new Thickness();

    [ControlTokenDefault(nameof(InlineHeaderPadding))]
    private static Thickness DefaultForInlineHeaderPadding(in ControlTokenDefaultContext context) =>
        new Thickness();

    [ControlTokenDefault(nameof(InlineItemPadding))]
    private static Thickness DefaultForInlineItemPadding(in ControlTokenDefaultContext context) =>
        new Thickness( (context.Global.UniformlyPaddingXXS + context.Global.UniformlyMarginXXS / 2), context.Global.UniformlyPaddingXS + context.Global.LineWidth, (context.Global.UniformlyPaddingXXS + context.Global.UniformlyMarginXXS / 2), 0);

    [ControlTokenDefault(nameof(InlineTitleColor))]
    private static Color DefaultForInlineTitleColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorTextSecondary;

    [ControlTokenDefault(nameof(InlineTailColor))]
    private static Color DefaultForInlineTailColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorTextDisabled;

    [ControlTokenDefault(nameof(DotLineThickness))]
    private static double DefaultForDotLineThickness(in ControlTokenDefaultContext context) =>
        context.Global.LineWidth * 3;

    [ControlTokenDefault(nameof(ProgressFramePadding))]
    private static Thickness DefaultForProgressFramePadding(in ControlTokenDefaultContext context) =>
        new Thickness(context.Global.LineWidthBold * 2);

    [ControlTokenDefault(nameof(ProgressFramePaddingSM))]
    private static Thickness DefaultForProgressFramePaddingSM(in ControlTokenDefaultContext context) =>
        new Thickness(context.Global.LineWidthBold * 2);

    [ControlTokenDefault(nameof(ProgressGrooveColor))]
    private static Color DefaultForProgressGrooveColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorSplit;

    [ControlTokenDefault(nameof(ProgressColor))]
    private static Color DefaultForProgressColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorPrimary;

    [ControlTokenDefault(nameof(PanelArrowWidth))]
    private static double DefaultForPanelArrowWidth(in ControlTokenDefaultContext context) =>
        (context.Global.UniformlyPaddingSM * 2 + context.Global.ControlHeight + (context.Global.FontSizeLG * context.Global.RelativeLineHeightLG)) * 0.7071;

    [ControlTokenDefault(nameof(PanelArrowWidthSM))]
    private static double DefaultForPanelArrowWidthSM(in ControlTokenDefaultContext context) =>
        (context.Global.UniformlyPaddingXS * 2 + context.Global.ControlHeightSM + context.Global.FontSize * context.Global.RelativeLineHeight) * 0.7071;

    [ControlTokenDefault(nameof(PanelWaitBackground))]
    private static Color DefaultForPanelWaitBackground(in ControlTokenDefaultContext context) =>
        context.Global.ColorFillTertiary;

    [ControlTokenDefault(nameof(PanelProcessBackground))]
    private static Color DefaultForPanelProcessBackground(in ControlTokenDefaultContext context) =>
        context.Global.ColorPrimaryBg;

    [ControlTokenDefault(nameof(PanelFinishBackground))]
    private static Color DefaultForPanelFinishBackground(in ControlTokenDefaultContext context) =>
        context.Global.ColorPrimaryBg;

    [ControlTokenDefault(nameof(PanelErrorBackground))]
    private static Color DefaultForPanelErrorBackground(in ControlTokenDefaultContext context) =>
        context.Global.ColorErrorBg;

    [ControlTokenDefault(nameof(PanelWaitActiveBackground))]
    private static Color DefaultForPanelWaitActiveBackground(in ControlTokenDefaultContext context) =>
        context.Global.ColorPrimary;

    [ControlTokenDefault(nameof(PanelProcessActiveBackground))]
    private static Color DefaultForPanelProcessActiveBackground(in ControlTokenDefaultContext context) =>
        context.Global.ColorPrimary;

    [ControlTokenDefault(nameof(PanelFinishActiveBackground))]
    private static Color DefaultForPanelFinishActiveBackground(in ControlTokenDefaultContext context) =>
        context.Global.ColorPrimary;

    [ControlTokenDefault(nameof(PanelErrorActiveBackground))]
    private static Color DefaultForPanelErrorActiveBackground(in ControlTokenDefaultContext context) =>
        context.Global.ColorError;

    [ControlTokenDefault(nameof(PanelWaitTextColor))]
    private static Color DefaultForPanelWaitTextColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorTextLabel;

    [ControlTokenDefault(nameof(PanelProcessTextColor))]
    private static Color DefaultForPanelProcessTextColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorPrimary;

    [ControlTokenDefault(nameof(PanelFinishTextColor))]
    private static Color DefaultForPanelFinishTextColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorPrimary;

    [ControlTokenDefault(nameof(PanelErrorTextColor))]
    private static Color DefaultForPanelErrorTextColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorError;

    [ControlTokenDefault(nameof(PanelActiveTextColor))]
    private static Color DefaultForPanelActiveTextColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorTextLightSolid;

    [ControlTokenDefault(nameof(PanelErrorActiveTextColor))]
    private static Color DefaultForPanelErrorActiveTextColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorTextLightSolid;

    [ControlTokenDefault(nameof(PanelFirstBorderThickness))]
    private static Thickness DefaultForPanelFirstBorderThickness(in ControlTokenDefaultContext context) =>
        new Thickness(context.Global.LineWidthBold, context.Global.LineWidthBold, 0, context.Global.LineWidthBold);

    [ControlTokenDefault(nameof(PanelMiddleBorderThickness))]
    private static Thickness DefaultForPanelMiddleBorderThickness(in ControlTokenDefaultContext context) =>
        new Thickness(0, context.Global.LineWidthBold, 0, context.Global.LineWidthBold);

    [ControlTokenDefault(nameof(PanelLastBorderThickness))]
    private static Thickness DefaultForPanelLastBorderThickness(in ControlTokenDefaultContext context) =>
        new Thickness(0, context.Global.LineWidthBold, context.Global.LineWidthBold, context.Global.LineWidthBold);

    [ControlTokenDefault(nameof(PanelFirstCornerRadius))]
    private static CornerRadius DefaultForPanelFirstCornerRadius(in ControlTokenDefaultContext context) =>
        new CornerRadius((context.Global.BorderRadius.TopLeft), 0, 0, (context.Global.BorderRadius.TopLeft));

    [ControlTokenDefault(nameof(PanelLastCornerRadius))]
    private static CornerRadius DefaultForPanelLastCornerRadius(in ControlTokenDefaultContext context) =>
        new CornerRadius(0, (context.Global.BorderRadius.TopLeft), (context.Global.BorderRadius.TopLeft), 0);

    [ControlTokenDefault(nameof(PanelFirstCornerRadiusSM))]
    private static CornerRadius DefaultForPanelFirstCornerRadiusSM(in ControlTokenDefaultContext context) =>
        new CornerRadius((context.Global.BorderRadiusSM.TopLeft), 0, 0, (context.Global.BorderRadiusSM.TopLeft));

    [ControlTokenDefault(nameof(PanelLastCornerRadiusSM))]
    private static CornerRadius DefaultForPanelLastCornerRadiusSM(in ControlTokenDefaultContext context) =>
        new CornerRadius(0, (context.Global.BorderRadiusSM.TopLeft), (context.Global.BorderRadiusSM.TopLeft), 0);

}
