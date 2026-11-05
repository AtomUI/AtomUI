using AtomUI.Theme.DesignTokens;
using Avalonia;

namespace AtomUI.Toolkits.GalleryBase.Controls;

[ControlDesignToken(ControlTokenEvaluationVersion.DependencyDefaultsV2, Revision = 1)]
internal sealed partial class SemanticPartPreviewToken : AbstractControlDesignToken, IControlTokenDefaults
{
    public Thickness InspectionMargin { get; set; }
    public Thickness CompactInspectionMargin { get; set; }
    public double InspectionMinHeight { get; set; }
    public Thickness StagePadding { get; set; }
    public Thickness CompactStagePadding { get; set; }
    public Thickness TitleMargin { get; set; }
    public Thickness CompactTitleMargin { get; set; }
    public double TitleFontSize { get; set; }
    public Thickness VerticalDividerThickness { get; set; }
    public Thickness HorizontalDividerThickness { get; set; }
    public Thickness InfoDividerThickness { get; set; }
    public Thickness CompactInfoDividerThickness { get; set; }
    public Thickness ItemPadding { get; set; }
    public Thickness InfoPadding { get; set; }
    public double ItemLabelFontSize { get; set; }
    public double ItemOwnerFontSize { get; set; }

    [ControlTokenDefault(nameof(InspectionMargin))]
    private static Thickness DefaultInspectionMargin(in ControlTokenDefaultContext context) =>
        new(context.Global.SizeUnit * 6, context.Global.SizeUnit * 5, context.Global.SizeUnit * 6, context.Global.SizeUnit * 8);

    [ControlTokenDefault(nameof(CompactInspectionMargin))]
    private static Thickness DefaultCompactInspectionMargin(in ControlTokenDefaultContext context) =>
        new(context.Global.SizeUnit * 4, context.Global.SizeUnit * 4, context.Global.SizeUnit * 4, context.Global.SizeUnit * 6);

    [ControlTokenDefault(nameof(InspectionMinHeight))]
    private static double DefaultInspectionMinHeight(in ControlTokenDefaultContext context) => context.Global.SizeUnit * 90;

    [ControlTokenDefault(nameof(StagePadding))]
    private static Thickness DefaultStagePadding(in ControlTokenDefaultContext context) => new(context.Global.SizeUnit * 4);

    [ControlTokenDefault(nameof(CompactStagePadding))]
    private static Thickness DefaultCompactStagePadding(in ControlTokenDefaultContext context) => new(context.Global.SizeUnit * 5);

    [ControlTokenDefault(nameof(TitleMargin))]
    private static Thickness DefaultTitleMargin(in ControlTokenDefaultContext context) => new(0, 0, 0, context.Global.SizeUnit * 3);

    [ControlTokenDefault(nameof(CompactTitleMargin))]
    private static Thickness DefaultCompactTitleMargin(in ControlTokenDefaultContext context) => new(0, 0, 0, context.Global.SizeUnit * 2);

    [ControlTokenDefault(nameof(TitleFontSize))]
    private static double DefaultTitleFontSize(in ControlTokenDefaultContext context) => context.Global.FontSizeLG;

    [ControlTokenDefault(nameof(VerticalDividerThickness))]
    private static Thickness DefaultVerticalDividerThickness(in ControlTokenDefaultContext context) => new(context.Global.LineWidth, 0, 0, 0);

    [ControlTokenDefault(nameof(HorizontalDividerThickness))]
    private static Thickness DefaultHorizontalDividerThickness(in ControlTokenDefaultContext context) => new(0, context.Global.LineWidth, 0, 0);

    [ControlTokenDefault(nameof(InfoDividerThickness))]
    private static Thickness DefaultInfoDividerThickness(in ControlTokenDefaultContext context) => new(0, 0, context.Global.LineWidth, 0);

    [ControlTokenDefault(nameof(CompactInfoDividerThickness))]
    private static Thickness DefaultCompactInfoDividerThickness(in ControlTokenDefaultContext context) => new(0, 0, 0, context.Global.LineWidth);

    [ControlTokenDefault(nameof(ItemPadding))]
    private static Thickness DefaultItemPadding(in ControlTokenDefaultContext context) => new(context.Global.SizeUnit * 3);

    [ControlTokenDefault(nameof(InfoPadding))]
    private static Thickness DefaultInfoPadding(in ControlTokenDefaultContext context) => new(context.Global.SizeUnit * 4);

    [ControlTokenDefault(nameof(ItemLabelFontSize))]
    private static double DefaultItemLabelFontSize(in ControlTokenDefaultContext context) => context.Global.FontSize;

    [ControlTokenDefault(nameof(ItemOwnerFontSize))]
    private static double DefaultItemOwnerFontSize(in ControlTokenDefaultContext context) => 11;
}
