using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>
/// The Shadcn values of the shared <see cref="AbstractDateViewerToken" /> layer.
/// </summary>
public static class DateViewerTokenCalculator
{
    /// <summary>
    /// Writes the shared panel background, date cell styling, and layout defaults to <paramref name="token" />.
    /// </summary>
    public static void Calculate(in TokenEvaluationContext<ShadcnToken> context, DesignToken global, AbstractDateViewerToken token)
    {
        // Let the popover or card paint the surface behind the calendar.
        token.PanelBg = Colors.Transparent;
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
        // antd dims the week number of the selected row to half alpha.
        token.WeekNumberOpacity = 0.5;
    }

    /// <summary>
    /// Returns the height of a date cell row: <paramref name="cellHeight" /> plus three times the XXS padding.
    /// </summary>
    /// <param name="token">
    /// The date viewer token that receives the row height. Not read.
    /// </param>
    /// <param name="global">
    /// The global token that supplies the XXS padding.
    /// </param>
    /// <param name="cellHeight">
    /// The height of one date cell.
    /// </param>
    public static double CalculateCellRowHeight(AbstractDateViewerToken token, DesignToken global, double cellHeight) =>
        cellHeight + global.UniformlyPaddingXXS * 3;
}
