using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>
/// The PrimeNG values of the shared <see cref="AbstractDateViewerToken" /> layer.
/// </summary>
public static class DateViewerTokenCalculator
{
    /// <summary>
    /// Writes the PrimeNG values of the date panel into the shared <see cref="AbstractDateViewerToken" /> layer: the
    /// panel width, the cell sizes and colors, the header metrics, the content insets and the week number opacity.
    /// <see cref="CalculateCellRowHeight" /> derives the row height from the cell height.
    /// </summary>
    public static void Calculate(in TokenEvaluationContext<PrimeNGToken> context, DesignToken global, AbstractDateViewerToken token)
    {
        token.PanelBg = global.ColorBgContainer;
        token.CellHoverBg = global.ControlItemBgHover;
        token.CellRangeBg = global.ControlItemBgActive;
        token.CellCornerRadius     = global.BorderRadiusSM;
        // Ant Design formula kept as is: the date panel stages replace the cell size and radius, the row height,
        // the panel width, the today fill, the title weight and the week-day row (PrimeUIX datepicker); the rest
        // keeps the Ant Design formula.
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
    /// Returns <c>CellRowHeight</c>, the height of one row of date cells: <paramref name="cellHeight" /> plus three
    /// times <c>UniformlyPaddingXXS</c>.
    /// </summary>
    public static double CalculateCellRowHeight(AbstractDateViewerToken token, DesignToken global, double cellHeight) =>
        cellHeight + global.UniformlyPaddingXXS * 3;
}
