using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public abstract partial class AbstractDateViewerToken : AbstractControlDesignToken
{
    public partial Color PanelBg { get; set; }
    public partial Color CellHoverBg { get; set; }
    public partial Color CellRangeBg { get; set; }

    /// <summary>
    /// The text colour of today's and the selected cell in the content presentation, drawn on
    /// <c>ControlItemBgActive</c>. Defaults to the primary colour (Ant Design); a theme whose selected background is
    /// the solid primary sets the contrast colour.
    /// </summary>
    public partial Color CellActiveColor { get; set; }
    public partial double PanelWidth { get; set; }
    public partial double CellHeight { get; set; }
    public partial double CellWidth { get; set; }
    public partial double CellRowHeight { get; set; }
    public partial double PeriodCellWidth { get; set; }
    public partial Thickness PeriodCellPadding { get; set; }
    public partial double ContentCellMinHeight { get; set; }
    public partial Thickness ContentCellSurfaceMargin { get; set; }
    public partial Thickness ContentCellTopBorderThickness { get; set; }
    public partial Thickness ContentCellValueMargin { get; set; }
    public partial Thickness ContentCellContentMargin { get; set; }
    public partial Thickness PanelPadding { get; set; }
    public partial Thickness HeaderHorizontalPadding { get; set; }
    public partial Thickness DateBodyPadding { get; set; }

    /// <summary>
    /// Bottom divider of the default panel header (antd picker header borderBottom: lineWidth).
    /// </summary>
    public partial Thickness HeaderBorderThickness { get; set; }

    /// <summary>
    /// Top divider of the standalone footer (antd picker footer borderTop: lineWidth).
    /// </summary>

    /// <summary>
    /// Opacity of the week numbers in week mode (antd dims the selected row's week number to half alpha).
    /// </summary>
    public partial double WeekNumberOpacity { get; set; }

    protected override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        PanelBg = EffectiveGlobalToken.ColorBgContainer;
        CellHoverBg = EffectiveGlobalToken.ControlItemBgHover;
        CellRangeBg = EffectiveGlobalToken.ControlItemBgActive;
        CellActiveColor = EffectiveGlobalToken.ColorPrimary;
        var dateColumnWidth = EffectiveGlobalToken.ControlHeightSM * 1.5;
        var bodyHorizontalPadding = EffectiveGlobalToken.UniformlyPadding + EffectiveGlobalToken.UniformlyPaddingXXS / 2;
        PanelWidth = dateColumnWidth * 7 + bodyHorizontalPadding * 2;

CellWidth = EffectiveGlobalToken.ControlHeightSM;
        PeriodCellWidth = EffectiveGlobalToken.ControlHeightLG * 1.5;
        PeriodCellPadding = new Thickness(EffectiveGlobalToken.UniformlyPaddingXS, 0);
        ContentCellMinHeight = EffectiveGlobalToken.ControlHeightLG * 2;
        var contentSurfaceInset = EffectiveGlobalToken.UniformlyMarginXXS;
        var contentHorizontalInset = contentSurfaceInset + EffectiveGlobalToken.UniformlyPaddingXS;
        var contentTopInset = EffectiveGlobalToken.LineWidthBold + EffectiveGlobalToken.UniformlyPaddingXS / 2;
        ContentCellSurfaceMargin = new Thickness(contentSurfaceInset, 0);
        ContentCellTopBorderThickness = new Thickness(0, EffectiveGlobalToken.LineWidthBold, 0, 0);
        ContentCellValueMargin = new Thickness(contentHorizontalInset, contentTopInset, contentHorizontalInset, 0);
        ContentCellContentMargin = new Thickness(contentHorizontalInset, 0);
        PanelPadding = new Thickness(0);
        HeaderHorizontalPadding = new Thickness(EffectiveGlobalToken.UniformlyPaddingXS, 0);
        DateBodyPadding = new Thickness(bodyHorizontalPadding, EffectiveGlobalToken.UniformlyPaddingXS);
        HeaderBorderThickness = new Thickness(0, 0, 0, EffectiveGlobalToken.LineWidth);
        WeekNumberOpacity = 0.5;
    }
    protected double CalculateCellRowHeight(double cellHeight) =>
        cellHeight + EffectiveGlobalToken.UniformlyPaddingXXS * 3;
}
