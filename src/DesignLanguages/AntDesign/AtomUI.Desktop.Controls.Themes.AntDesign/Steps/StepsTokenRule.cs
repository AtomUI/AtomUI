using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>The AntDesign view of <see cref="StepsToken"/>.</summary>
[ControlTokenRule]
internal sealed class StepsTokenRule : IControlTokenRule<AntDesignToken, StepsToken>
{
    public void Apply(in TokenEvaluationContext<AntDesignToken> context, ControlTokenTarget<StepsToken> target)
    {
        var token  = target.Own;
        var global = target.Global;

        token.CustomIconSize      = global.ControlHeight;
        token.CustomIconFontSize  = global.ControlHeightSM;
        token.IconContainerSize   = global.ControlHeight;

        token.IconContainerCornerRadius = new CornerRadius(token.IconContainerSize);
        token.IconFontSize        = global.FontSize;
        token.IconContainerSizeSM = global.FontSizeHeading3;
        token.DotSize             = global.ControlHeight / 4;
        token.DotCurrentSize      = global.ControlHeightLG / 4;
        token.NavArrowColor       = global.ColorTextDisabled;
        token.DescriptionMaxWidth = 140;

        token.WaitIconColor         = global.ColorTextLabel;
        token.WaitIconBgColor       = global.ColorFillContent;
        token.WaitIconBorderColor   = Colors.Transparent;
        token.WaitTitleColor        = global.ColorTextDescription;
        token.WaitDescriptionColor  = global.ColorTextDescription;
        token.WaitTailColor         = global.ColorTextDisabled;
        token.WaitDotColor          = global.ColorTextDisabled;

        token.ProcessIconColor        = global.ColorTextLightSolid;
        token.ProcessTitleColor       = global.ColorText;
        token.TitleFontWeight         = FontWeight.Normal;
        token.ProcessDescriptionColor = global.ColorText;
        token.ProcessIconBgColor      = global.ColorPrimary;
        token.ProcessIconBorderColor  = global.ColorPrimary;
        token.ProcessDotColor         = global.ColorPrimary;
        token.ProcessTailColor        = global.ColorPrimary;

        token.ProgressGrooveColor = global.ColorSplit;
        token.ProgressColor       = global.ColorPrimary;

        token.FinishIconBgColor       = global.ControlItemBgActive;
        token.FinishIconBorderColor   = global.ControlItemBgActive;
        token.FinishIconColor         = global.ColorPrimary;
        token.FinishTitleColor        = global.ColorText;
        token.FinishDescriptionColor  = global.ColorTextDescription;
        token.FinishTailColor         = global.ColorPrimary;
        token.FinishDotColor          = global.ColorPrimary;

        token.ErrorIconColor          = global.ColorTextLightSolid;
        token.ErrorTitleColor         = global.ColorError;
        token.ErrorDescriptionColor   = global.ColorError;
        token.ErrorTailColor          = global.ColorError;
        token.ErrorIconBgColor        = global.ColorError;
        token.ErrorIconBorderColor    = global.ColorError;
        token.ErrorDotColor           = global.ColorError;
        token.StepsNavActiveColor     = global.ColorPrimary;
        // Steps inline variable
        token.InlineDotSize       = 6;
        token.InlineHeaderMargin  = new Thickness();
        token.InlineHeaderPadding = new Thickness();
        token.InlineTitleColor    = global.ColorTextSecondary;
        token.InlineTailColor     = global.ColorTextDisabled;
        var inlineItemHorizontalPadding = global.UniformlyPaddingXXS + global.UniformlyMarginXXS / 2;
        token.InlineItemPadding   = new Thickness(
            inlineItemHorizontalPadding,
            global.UniformlyPaddingXS + global.LineWidth,
            inlineItemHorizontalPadding,
            0);

        token.HorizontalHeaderMargin     = new Thickness(global.UniformlyMargin, 0);
        token.SubHeaderMargin            = new Thickness(global.UniformlyMarginXS, 0, 0, 0);
        token.VerticalItemSpacing        = global.UniformlyMarginXXS;
        token.VerticalDescriptionPadding = new Thickness();
        token.DotLineThickness           = global.LineWidthBold; // antd Steps progress-dot rail-size
        token.OutlineDotBorderThickness  = new Thickness(global.LineWidthBold);

        token.VerticalLabelContentMargin = new Thickness(0, global.UniformlyMarginSM, 0, 0);
        token.VerticalNavArrowMargin     = new Thickness();
        token.VerticalNavArrowMarginSM   = new Thickness();
        token.VerticalNavItemPadding     = new Thickness(global.UniformlyPaddingSM, global.UniformlyPadding);
        token.NavItemGutter              = global.Spacing;
        token.ProgressFramePadding       = new Thickness(global.LineWidthBold * 2);
        token.ProgressFramePaddingSM     = new Thickness(global.LineWidthBold * 2);

        var panelTitleHeight = global.FontSizeLG * global.RelativeLineHeightLG;
        var panelItemHeight = global.UniformlyPaddingSM * 2 +
                              global.ControlHeight +
                              panelTitleHeight;
        var panelItemHeightSM = global.UniformlyPaddingXS * 2 +
                                global.ControlHeightSM +
                                global.FontSize * global.RelativeLineHeight;
        token.PanelArrowWidth   = panelItemHeight * 0.7071;
        token.PanelArrowWidthSM = panelItemHeightSM * 0.7071;
        token.PanelWaitBackground          = global.ColorFillTertiary;
        token.PanelProcessBackground       = global.ColorPrimaryBg;
        token.PanelFinishBackground        = global.ColorPrimaryBg;
        token.PanelErrorBackground         = global.ColorErrorBg;
        token.PanelWaitActiveBackground    = global.ColorPrimary;
        token.PanelProcessActiveBackground = global.ColorPrimary;
        token.PanelFinishActiveBackground  = global.ColorPrimary;
        token.PanelErrorActiveBackground   = global.ColorError;
        token.PanelWaitTextColor            = global.ColorTextLabel;
        token.PanelProcessTextColor         = global.ColorPrimary;
        token.PanelFinishTextColor          = global.ColorPrimary;
        token.PanelErrorTextColor           = global.ColorError;
        token.PanelActiveTextColor          = global.ColorTextLightSolid;
        token.PanelErrorActiveTextColor     = global.ColorTextLightSolid;
        token.PanelFirstBorderThickness  = new Thickness(global.LineWidthBold, global.LineWidthBold, 0, global.LineWidthBold);
        token.PanelMiddleBorderThickness = new Thickness(0, global.LineWidthBold, 0, global.LineWidthBold);
        token.PanelLastBorderThickness   = new Thickness(0, global.LineWidthBold, global.LineWidthBold, global.LineWidthBold);
        var panelRadius = global.BorderRadius.TopLeft;
        var panelRadiusSM = global.BorderRadiusSM.TopLeft;
        token.PanelFirstCornerRadius = new CornerRadius(panelRadius, 0, 0, panelRadius);
        token.PanelLastCornerRadius  = new CornerRadius(0, panelRadius, panelRadius, 0);
        token.PanelFirstCornerRadiusSM = new CornerRadius(panelRadiusSM, 0, 0, panelRadiusSM);
        token.PanelLastCornerRadiusSM  = new CornerRadius(0, panelRadiusSM, panelRadiusSM, 0);
    }
}
