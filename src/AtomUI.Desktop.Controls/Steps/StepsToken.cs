using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class StepsToken : AbstractControlDesignToken
{

    public StepsToken()

    {
    }

    /// <summary>
    /// 描述区域最大宽度
    /// Max width of description area
    /// </summary>
    public partial double DescriptionMaxWidth { get; set; }

    /// <summary>
    /// 自定义图标容器尺寸
    /// Size of custom icon container
    /// </summary>
    public partial double CustomIconSize { get; set; }

    /// <summary>
    /// 自定义图标大小
    /// Font size of custom icon
    /// </summary>
    public partial double CustomIconFontSize { get; set; }

    /// <summary>
    /// 图标容器尺寸
    /// Size of icon container
    /// </summary>
    public partial double IconContainerSize { get; set; }

    /// <summary>
    /// 图标容器圆角（默认全圆）
    /// Corner radius of icon container (full circle by default)
    /// </summary>
    public partial CornerRadius IconContainerCornerRadius { get; set; }

    /// <summary>
    /// 图标大小
    /// Size of icon
    /// </summary>
    public partial double IconFontSize { get; set; }

    /// <summary>
    /// 点状步骤点大小
    /// Size of dot
    /// </summary>
    public partial double DotSize { get; set; }

    /// <summary>
    /// 点状步骤点当前大小
    /// Current size of dot
    /// </summary>
    public partial double DotCurrentSize { get; set; }

    /// <summary>
    /// 可跳转步骤条箭头颜色
    /// Color of arrow in nav
    /// </summary>
    public partial Color NavArrowColor { get; set; }

    /// <summary>
    /// 小号步骤条图标大小
    /// Size of small steps icon
    /// </summary>
    public partial double IconContainerSizeSM { get; set; }

    /// <summary>
    /// Label 水平排列的时候的外间距
    /// External spacing when Label is arranged horizontally
    /// </summary>
    public partial Thickness HorizontalHeaderMargin { get; set; }

    /// <summary>
    /// 子标题外间距
    /// External spacing of subtitle
    /// </summary>
    public partial Thickness SubHeaderMargin { get; set; }

    /// <summary>
    /// 垂直排列的时候 item 的间距
    /// </summary>
    public partial double VerticalItemSpacing { get; set; }

    /// <summary>
    /// 垂直排列描述的内间距
    /// </summary>
    public partial Thickness VerticalDescriptionPadding { get; set; }

    /// <summary>
    /// 垂直标签排列时候内容跟图标之间的外间距
    /// </summary>
    public partial Thickness VerticalLabelContentMargin { get; set; }

    /// <summary>
    /// 垂直导航类型箭头的外间距
    /// </summary>
    public partial Thickness VerticalNavArrowMargin { get; set; }

    /// <summary>
    /// 垂直导航类型箭头的外间距，小尺寸
    /// </summary>
    public partial Thickness VerticalNavArrowMarginSM { get; set; }

    /// <summary>
    /// 垂直导航类型 item 内边距
    /// </summary>
    public partial Thickness VerticalNavItemPadding { get; set; }

    /// <summary>
    /// 垂直导航类型内容和指示线的间距
    /// </summary>
    public partial double NavItemGutter { get; set; }

    #region 内部 Token

    public partial Color WaitIconColor { get; set; }
    public partial Color WaitIconBgColor { get; set; }
    public partial Color WaitIconBorderColor { get; set; }
    public partial Color FinishIconBgColor { get; set; }
    public partial Color FinishIconBorderColor { get; set; }

    public partial Color ProcessTailColor { get; set; }
    public partial Color ProcessIconColor { get; set; }
    public partial Color ProcessTitleColor { get; set; }
    public partial Color ProcessDescriptionColor { get; set; }
    public partial Color ProcessIconBgColor { get; set; }
    public partial Color ProcessIconBorderColor { get; set; }
    public partial Color ProcessDotColor { get; set; }
    public partial Color WaitTitleColor { get; set; }
    public partial Color WaitDescriptionColor { get; set; }
    public partial Color WaitTailColor { get; set; }
    public partial Color WaitDotColor { get; set; }
    public partial Color FinishIconColor { get; set; }
    public partial Color FinishTitleColor { get; set; }
    public partial Color FinishDescriptionColor { get; set; }
    public partial Color FinishTailColor { get; set; }
    public partial Color FinishDotColor { get; set; }
    public partial Color ErrorIconColor { get; set; }
    public partial Color ErrorTitleColor { get; set; }
    public partial Color ErrorDescriptionColor { get; set; }
    public partial Color ErrorTailColor { get; set; }
    public partial Color ErrorIconBgColor { get; set; }
    public partial Color ErrorIconBorderColor { get; set; }
    public partial Color ErrorDotColor { get; set; }
    public partial Color StepsNavActiveColor { get; set; }
    // Steps inline variable
    public partial double InlineDotSize { get; set; }
    public partial Thickness InlineHeaderMargin { get; set; }
    public partial Thickness InlineHeaderPadding { get; set; }
    public partial Thickness InlineItemPadding { get; set; }
    public partial Color InlineTitleColor { get; set; }
    public partial Color InlineTailColor { get; set; }
    public partial double DotLineThickness { get; set; }

    /// <summary>
    /// 空心点状步骤点的边框宽度
    /// Border width of the outline dot (antd Steps dot icon border width)
    /// </summary>
    public partial Thickness OutlineDotBorderThickness { get; set; }
    public partial Thickness ProgressFramePadding { get; set; }
    public partial Thickness ProgressFramePaddingSM { get; set; }
    public partial Color ProgressGrooveColor { get; set; }
    public partial Color ProgressColor { get; set; }
    public partial double PanelArrowWidth { get; set; }
    public partial double PanelArrowWidthSM { get; set; }
    public partial Color PanelWaitBackground { get; set; }
    public partial Color PanelProcessBackground { get; set; }
    public partial Color PanelFinishBackground { get; set; }
    public partial Color PanelErrorBackground { get; set; }
    public partial Color PanelWaitActiveBackground { get; set; }
    public partial Color PanelProcessActiveBackground { get; set; }
    public partial Color PanelFinishActiveBackground { get; set; }
    public partial Color PanelErrorActiveBackground { get; set; }
    public partial Color PanelWaitTextColor { get; set; }
    public partial Color PanelProcessTextColor { get; set; }
    public partial Color PanelFinishTextColor { get; set; }
    public partial Color PanelErrorTextColor { get; set; }
    public partial Color PanelActiveTextColor { get; set; }
    public partial Color PanelErrorActiveTextColor { get; set; }
    public partial Thickness PanelFirstBorderThickness { get; set; }
    public partial Thickness PanelMiddleBorderThickness { get; set; }
    public partial Thickness PanelLastBorderThickness { get; set; }
    public partial CornerRadius PanelFirstCornerRadius { get; set; }
    public partial CornerRadius PanelLastCornerRadius { get; set; }
    public partial CornerRadius PanelFirstCornerRadiusSM { get; set; }
    public partial CornerRadius PanelLastCornerRadiusSM { get; set; }

    #endregion

    protected override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);

        CustomIconSize      = EffectiveGlobalToken.ControlHeight;
        CustomIconFontSize  = EffectiveGlobalToken.ControlHeightSM;

        if (!IsExplicit(StepsTokenKind.IconContainerSize))
        {
            IconContainerSize   = EffectiveGlobalToken.ControlHeight;
        }

        IconContainerCornerRadius = new CornerRadius(IconContainerSize);
        IconFontSize        = EffectiveGlobalToken.FontSize;
        IconContainerSizeSM = EffectiveGlobalToken.FontSizeHeading3;
        DotSize             = EffectiveGlobalToken.ControlHeight / 4;
        DotCurrentSize      = EffectiveGlobalToken.ControlHeightLG / 4;
        NavArrowColor       = EffectiveGlobalToken.ColorTextDisabled;
        DescriptionMaxWidth = 140;

        WaitIconColor         = EffectiveGlobalToken.ColorTextLabel;
        WaitIconBgColor       = EffectiveGlobalToken.ColorFillContent;
        WaitIconBorderColor   = Colors.Transparent;
        WaitTitleColor        = EffectiveGlobalToken.ColorTextDescription;
        WaitDescriptionColor  = EffectiveGlobalToken.ColorTextDescription;
        WaitTailColor         = EffectiveGlobalToken.ColorTextDisabled;
        WaitDotColor          = EffectiveGlobalToken.ColorTextDisabled;

        ProcessIconColor        = EffectiveGlobalToken.ColorTextLightSolid;
        ProcessTitleColor       = EffectiveGlobalToken.ColorText;
        ProcessDescriptionColor = EffectiveGlobalToken.ColorText;
        ProcessIconBgColor      = EffectiveGlobalToken.ColorPrimary;
        ProcessIconBorderColor  = EffectiveGlobalToken.ColorPrimary;
        ProcessDotColor         = EffectiveGlobalToken.ColorPrimary;
        ProcessTailColor        = EffectiveGlobalToken.ColorPrimary;

        ProgressGrooveColor = EffectiveGlobalToken.ColorSplit;
        ProgressColor       = EffectiveGlobalToken.ColorPrimary;

        FinishIconBgColor       = EffectiveGlobalToken.ControlItemBgActive;
        FinishIconBorderColor   = EffectiveGlobalToken.ControlItemBgActive;
        FinishIconColor         = EffectiveGlobalToken.ColorPrimary;
        FinishTitleColor        = EffectiveGlobalToken.ColorText;
        FinishDescriptionColor  = EffectiveGlobalToken.ColorTextDescription;
        FinishTailColor         = EffectiveGlobalToken.ColorPrimary;
        FinishDotColor          = EffectiveGlobalToken.ColorPrimary;

        ErrorIconColor          = EffectiveGlobalToken.ColorTextLightSolid;
        ErrorTitleColor         = EffectiveGlobalToken.ColorError;
        ErrorDescriptionColor   = EffectiveGlobalToken.ColorError;
        ErrorTailColor          = EffectiveGlobalToken.ColorError;
        ErrorIconBgColor        = EffectiveGlobalToken.ColorError;
        ErrorIconBorderColor    = EffectiveGlobalToken.ColorError;
        ErrorDotColor           = EffectiveGlobalToken.ColorError;
        StepsNavActiveColor     = EffectiveGlobalToken.ColorPrimary;
        // Steps inline variable
        InlineDotSize       = 6;
        InlineHeaderMargin  = new Thickness();
        InlineHeaderPadding = new Thickness();
        InlineTitleColor    = EffectiveGlobalToken.ColorTextSecondary;
        InlineTailColor     = EffectiveGlobalToken.ColorTextDisabled;
        var inlineItemHorizontalPadding = EffectiveGlobalToken.UniformlyPaddingXXS + EffectiveGlobalToken.UniformlyMarginXXS / 2;
        InlineItemPadding   = new Thickness(
            inlineItemHorizontalPadding,
            EffectiveGlobalToken.UniformlyPaddingXS + EffectiveGlobalToken.LineWidth,
            inlineItemHorizontalPadding,
            0);

        HorizontalHeaderMargin     = new Thickness(EffectiveGlobalToken.UniformlyMargin, 0);
        SubHeaderMargin            = new Thickness(EffectiveGlobalToken.UniformlyMarginXS, 0, 0, 0);
        VerticalItemSpacing        = EffectiveGlobalToken.UniformlyMarginXXS;
        VerticalDescriptionPadding = new Thickness();
        DotLineThickness           = EffectiveGlobalToken.LineWidthBold; // antd Steps progress-dot rail-size
        OutlineDotBorderThickness  = new Thickness(EffectiveGlobalToken.LineWidthBold);

        VerticalLabelContentMargin = new Thickness(0, EffectiveGlobalToken.UniformlyMarginSM, 0, 0);
        VerticalNavArrowMargin     = new Thickness();
        VerticalNavArrowMarginSM   = new Thickness();
        VerticalNavItemPadding     = new Thickness(EffectiveGlobalToken.UniformlyPaddingSM, EffectiveGlobalToken.UniformlyPadding);
        NavItemGutter              = EffectiveGlobalToken.Spacing;
        ProgressFramePadding       = new Thickness(EffectiveGlobalToken.LineWidthBold * 2);
        ProgressFramePaddingSM     = new Thickness(EffectiveGlobalToken.LineWidthBold * 2);

        var panelTitleHeight = EffectiveGlobalToken.FontSizeLG * EffectiveGlobalToken.RelativeLineHeightLG;
        var panelItemHeight = EffectiveGlobalToken.UniformlyPaddingSM * 2 +
                              EffectiveGlobalToken.ControlHeight +
                              panelTitleHeight;
        var panelItemHeightSM = EffectiveGlobalToken.UniformlyPaddingXS * 2 +
                                EffectiveGlobalToken.ControlHeightSM +
                                EffectiveGlobalToken.FontSize * EffectiveGlobalToken.RelativeLineHeight;
        PanelArrowWidth   = panelItemHeight * 0.7071;
        PanelArrowWidthSM = panelItemHeightSM * 0.7071;
        PanelWaitBackground          = EffectiveGlobalToken.ColorFillTertiary;
        PanelProcessBackground       = EffectiveGlobalToken.ColorPrimaryBg;
        PanelFinishBackground        = EffectiveGlobalToken.ColorPrimaryBg;
        PanelErrorBackground         = EffectiveGlobalToken.ColorErrorBg;
        PanelWaitActiveBackground    = EffectiveGlobalToken.ColorPrimary;
        PanelProcessActiveBackground = EffectiveGlobalToken.ColorPrimary;
        PanelFinishActiveBackground  = EffectiveGlobalToken.ColorPrimary;
        PanelErrorActiveBackground   = EffectiveGlobalToken.ColorError;
        PanelWaitTextColor            = EffectiveGlobalToken.ColorTextLabel;
        PanelProcessTextColor         = EffectiveGlobalToken.ColorPrimary;
        PanelFinishTextColor          = EffectiveGlobalToken.ColorPrimary;
        PanelErrorTextColor           = EffectiveGlobalToken.ColorError;
        PanelActiveTextColor          = EffectiveGlobalToken.ColorTextLightSolid;
        PanelErrorActiveTextColor     = EffectiveGlobalToken.ColorTextLightSolid;
        PanelFirstBorderThickness  = new Thickness(EffectiveGlobalToken.LineWidthBold, EffectiveGlobalToken.LineWidthBold, 0, EffectiveGlobalToken.LineWidthBold);
        PanelMiddleBorderThickness = new Thickness(0, EffectiveGlobalToken.LineWidthBold, 0, EffectiveGlobalToken.LineWidthBold);
        PanelLastBorderThickness   = new Thickness(0, EffectiveGlobalToken.LineWidthBold, EffectiveGlobalToken.LineWidthBold, EffectiveGlobalToken.LineWidthBold);
        var panelRadius = EffectiveGlobalToken.BorderRadius.TopLeft;
        var panelRadiusSM = EffectiveGlobalToken.BorderRadiusSM.TopLeft;
        PanelFirstCornerRadius = new CornerRadius(panelRadius, 0, 0, panelRadius);
        PanelLastCornerRadius  = new CornerRadius(0, panelRadius, panelRadius, 0);
        PanelFirstCornerRadiusSM = new CornerRadius(panelRadiusSM, 0, 0, panelRadiusSM);
        PanelLastCornerRadiusSM  = new CornerRadius(0, panelRadiusSM, panelRadiusSM, 0);
    }

}
