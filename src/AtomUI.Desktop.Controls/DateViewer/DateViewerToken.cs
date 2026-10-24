using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken(ControlTokenEvaluationVersion.DependencyDefaultsV2, Revision = 1)]
internal sealed partial class DateViewerToken : AbstractControlDesignToken, IControlTokenDefaults
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

    [ControlTokenDefault(nameof(PanelBg))]
    private static Color DefaultForPanelBg(in ControlTokenDefaultContext context) =>
        context.Global.ColorBgContainer;

    [ControlTokenDefault(nameof(CellHoverBg))]
    private static Color DefaultForCellHoverBg(in ControlTokenDefaultContext context) =>
        context.Global.ControlItemBgHover;

    [ControlTokenDefault(nameof(CellRangeBg))]
    private static Color DefaultForCellRangeBg(in ControlTokenDefaultContext context) =>
        context.Global.ControlItemBgActive;

    [ControlTokenDefault(nameof(PanelWidth))]
    private static double DefaultForPanelWidth(in ControlTokenDefaultContext context)
    {
        var dateColumnWidth = context.Global.ControlHeightSM * 1.5;
        var bodyHorizontalPadding = context.Global.UniformlyPadding + context.Global.UniformlyPaddingXXS / 2;
        return dateColumnWidth * 7 + bodyHorizontalPadding * 2;
    }

    [ControlTokenDefault(nameof(CellHeight))]
    private static double DefaultForCellHeight(in ControlTokenDefaultContext context) =>
        context.Global.ControlHeightSM;

    [ControlTokenDefault(nameof(CellWidth))]
    private static double DefaultForCellWidth(in ControlTokenDefaultContext context) =>
        context.Global.ControlHeightSM;

    [ControlTokenDefault(nameof(CellRowHeight), nameof(CellHeight))]
    private static double DefaultForCellRowHeight(in ControlTokenDefaultContext context, double cellHeight) =>
        cellHeight + context.Global.UniformlyPaddingXXS * 3;

    [ControlTokenDefault(nameof(PeriodCellWidth))]
    private static double DefaultForPeriodCellWidth(in ControlTokenDefaultContext context) =>
        context.Global.ControlHeightLG * 1.5;

    [ControlTokenDefault(nameof(PeriodCellPadding))]
    private static Thickness DefaultForPeriodCellPadding(in ControlTokenDefaultContext context) =>
        new Thickness(context.Global.UniformlyPaddingXS, 0);

    [ControlTokenDefault(nameof(ContentCellMinHeight))]
    private static double DefaultForContentCellMinHeight(in ControlTokenDefaultContext context) =>
        context.Global.ControlHeightLG * 2;

    [ControlTokenDefault(nameof(ContentCellSurfaceMargin))]
    private static Thickness DefaultForContentCellSurfaceMargin(in ControlTokenDefaultContext context) =>
        new Thickness(context.Global.UniformlyMarginXXS, 0);

    [ControlTokenDefault(nameof(ContentCellTopBorderThickness))]
    private static Thickness DefaultForContentCellTopBorderThickness(in ControlTokenDefaultContext context) =>
        new Thickness(0, context.Global.LineWidthBold, 0, 0);

    [ControlTokenDefault(nameof(ContentCellValueMargin))]
    private static Thickness DefaultForContentCellValueMargin(in ControlTokenDefaultContext context) =>
        new Thickness(context.Global.UniformlyMarginXXS + context.Global.UniformlyPaddingXS,
            context.Global.LineWidthBold + context.Global.UniformlyPaddingXS / 2,
            context.Global.UniformlyMarginXXS + context.Global.UniformlyPaddingXS, 0);

    [ControlTokenDefault(nameof(ContentCellContentMargin))]
    private static Thickness DefaultForContentCellContentMargin(in ControlTokenDefaultContext context) =>
        new Thickness(context.Global.UniformlyMarginXXS + context.Global.UniformlyPaddingXS, 0);

    [ControlTokenDefault(nameof(PanelPadding))]
    private static Thickness DefaultForPanelPadding(in ControlTokenDefaultContext context) =>
        new Thickness(0);

    [ControlTokenDefault(nameof(HeaderHorizontalPadding))]
    private static Thickness DefaultForHeaderHorizontalPadding(in ControlTokenDefaultContext context) =>
        new Thickness(context.Global.UniformlyPaddingXS, 0);

    [ControlTokenDefault(nameof(DateBodyPadding))]
    private static Thickness DefaultForDateBodyPadding(in ControlTokenDefaultContext context) =>
        new Thickness(context.Global.UniformlyPadding + context.Global.UniformlyPaddingXXS / 2,
            context.Global.UniformlyPaddingXS);

    [ControlTokenDefault(nameof(PanelSpacing))]
    private static double DefaultForPanelSpacing(in ControlTokenDefaultContext context) =>
        context.Global.UniformlyMargin;
}
