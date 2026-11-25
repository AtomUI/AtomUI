using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>The AntDesign values of the shared <see cref="AbstractDateViewerToken"/> layer.</summary>
public static class DateViewerTokenCalculator
{
    public static void Calculate(in TokenEvaluationContext<AntDesignToken> context, DesignToken global, AbstractDateViewerToken token)
    {
        token.PanelBg = global.ColorBgContainer;
        token.CellHoverBg = global.ControlItemBgHover;
        token.CellRangeBg = global.ControlItemBgActive;
        token.CellCornerRadius     = global.BorderRadiusSM;
        token.CellTodayBg          = Colors.Transparent;
        token.CellTodayColor       = global.ColorText;
        token.CellTodayBorderColor = global.ColorPrimary;
        token.HeaderTitleFontWeight = FontWeight.Bold;
        token.WeekHeaderHeight      = 0;
        token.WeekHeaderFontWeight  = FontWeight.Normal;
        token.CellActiveColor = global.ColorPrimary;
        var dateColumnWidth = global.ControlHeightSM * 1.5;
        var bodyHorizontalPadding = global.UniformlyPadding + global.UniformlyPaddingXXS / 2;
        token.PanelWidth = dateColumnWidth * 7 + bodyHorizontalPadding * 2;

        token.CellWidth = global.ControlHeightSM;
        token.PeriodCellWidth = global.ControlHeightLG * 1.5;
        token.PeriodCellPadding = new Thickness(global.UniformlyPaddingXS, 0);
        token.ContentCellMinHeight = global.ControlHeightLG * 2;
        var contentSurfaceInset = global.UniformlyMarginXXS;
        var contentHorizontalInset = contentSurfaceInset + global.UniformlyPaddingXS;
        var contentTopInset = global.LineWidthBold + global.UniformlyPaddingXS / 2;
        token.ContentCellSurfaceMargin = new Thickness(contentSurfaceInset, 0);
        token.ContentCellTopBorderThickness = new Thickness(0, global.LineWidthBold, 0, 0);
        token.ContentCellValueMargin = new Thickness(contentHorizontalInset, contentTopInset, contentHorizontalInset, 0);
        token.ContentCellContentMargin = new Thickness(contentHorizontalInset, 0);
        token.PanelPadding = new Thickness(0);
        token.HeaderHorizontalPadding = new Thickness(global.UniformlyPaddingXS, 0);
        token.DateBodyPadding = new Thickness(bodyHorizontalPadding, global.UniformlyPaddingXS);
        token.HeaderBorderThickness = new Thickness(0, 0, 0, global.LineWidth);
        token.WeekNumberOpacity = 0.5;
    }

    public static double CalculateCellRowHeight(AbstractDateViewerToken token, DesignToken global, double cellHeight) =>
        cellHeight + global.UniformlyPaddingXXS * 3;
}
