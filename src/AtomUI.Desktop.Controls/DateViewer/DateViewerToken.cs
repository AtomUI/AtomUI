using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
internal sealed class DateViewerToken : AbstractControlDesignToken
{
    public Color PanelBg { get; set; }
    public Color CellHoverBg { get; set; }
    public Color CellRangeBg { get; set; }
    public double PanelWidth { get; set; }
    public double CellHeight { get; set; }
    public double CellWidth { get; set; }
    public double CellRowHeight { get; set; }
    public double PeriodCellWidth { get; set; }
    public Thickness PeriodCellPadding { get; set; }
    public double ContentCellMinHeight { get; set; }
    public Thickness ContentCellSurfaceMargin { get; set; }
    public Thickness ContentCellTopBorderThickness { get; set; }
    public Thickness ContentCellValueMargin { get; set; }
    public Thickness ContentCellContentMargin { get; set; }
    public Thickness PanelPadding { get; set; }
    public Thickness HeaderHorizontalPadding { get; set; }
    public Thickness DateBodyPadding { get; set; }
    public double PanelSpacing { get; set; }

    public override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        PanelBg = EffectiveGlobalToken.ColorBgContainer;
        CellHoverBg = EffectiveGlobalToken.ControlItemBgHover;
        CellRangeBg = EffectiveGlobalToken.ControlItemBgActive;
        var dateColumnWidth = EffectiveGlobalToken.ControlHeightSM * 1.5;
        var bodyHorizontalPadding = EffectiveGlobalToken.UniformlyPadding + EffectiveGlobalToken.UniformlyPaddingXXS / 2;
        PanelWidth = dateColumnWidth * 7 + bodyHorizontalPadding * 2;
        CellHeight = EffectiveGlobalToken.ControlHeightSM;
        CellWidth = EffectiveGlobalToken.ControlHeightSM;
        CellRowHeight = CellHeight + EffectiveGlobalToken.UniformlyPaddingXXS * 3;
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
        PanelSpacing = EffectiveGlobalToken.UniformlyMargin;
    }
}
