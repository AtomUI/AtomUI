using AtomUI.Theme.DesignTokens;
using Avalonia;

namespace AtomUI.Toolkits.GalleryBase.Shell;

[ControlDesignToken(ControlTokenEvaluationVersion.DependencyDefaultsV2, Revision = 1)]
internal sealed partial class GalleryShellViewToken : AbstractControlDesignToken, IControlTokenDefaults
{
    public Thickness FooterPadding { get; set; }
    public Thickness FooterBorderThickness { get; set; }
    public Thickness BrandMargin { get; set; }
    public Thickness BrandHeaderMargin { get; set; }
    public double BrandFontSize { get; set; }

    [ControlTokenDefault(nameof(FooterPadding))]
    private static Thickness DefaultFooterPadding(in ControlTokenDefaultContext context) => new(context.Global.SizeUnit * 4, context.Global.SizeUnit * 3);

    [ControlTokenDefault(nameof(FooterBorderThickness))]
    private static Thickness DefaultFooterBorderThickness(in ControlTokenDefaultContext context) => new(0, context.Global.LineWidth, 0, 0);

    [ControlTokenDefault(nameof(BrandMargin))]
    private static Thickness DefaultBrandMargin(in ControlTokenDefaultContext context) => new(context.Global.SizeUnit * 6, 0, 0, 0);

    [ControlTokenDefault(nameof(BrandHeaderMargin))]
    private static Thickness DefaultBrandHeaderMargin(in ControlTokenDefaultContext context) => new(0, context.Global.SizeUnit * 5, 0, context.Global.SizeUnit * 4);

    [ControlTokenDefault(nameof(BrandFontSize))]
    private static double DefaultBrandFontSize(in ControlTokenDefaultContext context) => context.Global.FontSizeHeading4;
}
