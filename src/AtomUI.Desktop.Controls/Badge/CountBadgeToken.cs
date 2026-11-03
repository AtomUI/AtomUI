using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken(ControlTokenEvaluationVersion.DependencyDefaultsV2, Revision = 1)]
internal sealed partial class CountBadgeToken : AbstractControlDesignToken, IControlTokenDefaults
{
    public double IndicatorHeight { get; set; }

    public double IndicatorHeightSM { get; set; }

    public double TextFontSize { get; set; }

    public double TextFontSizeSM { get; set; }

    public Color TextColor { get; set; }

    public Color IndicatorColor { get; set; }

    public double ShadowSize { get; set; }

    public Color ShadowColor { get; set; }

    public Thickness TextPadding { get; set; }

    public CornerRadius CornerRadius { get; set; }

    public CornerRadius CornerRadiusSM { get; set; }

    [ControlTokenDefault(nameof(IndicatorHeight))]
    private static double DefaultForIndicatorHeight(in ControlTokenDefaultContext context) =>
        Math.Round(context.Global.FontSize * context.Global.RelativeLineHeight) - 2 * context.Global.LineWidth;

    [ControlTokenDefault(nameof(IndicatorHeightSM))]
    private static double DefaultForIndicatorHeightSM(in ControlTokenDefaultContext context) => context.Global.FontSize;

    [ControlTokenDefault(nameof(TextFontSize))]
    private static double DefaultForTextFontSize(in ControlTokenDefaultContext context) => context.Global.FontSizeSM;

    [ControlTokenDefault(nameof(TextFontSizeSM))]
    private static double DefaultForTextFontSizeSM(in ControlTokenDefaultContext context) => context.Global.FontSizeSM - 2;

    [ControlTokenDefault(nameof(TextColor))]
    private static Color DefaultForTextColor(in ControlTokenDefaultContext context) => context.Global.ColorTextLightSolid;

    [ControlTokenDefault(nameof(IndicatorColor))]
    private static Color DefaultForIndicatorColor(in ControlTokenDefaultContext context) => context.Global.ColorError;

    [ControlTokenDefault(nameof(ShadowSize))]
    private static double DefaultForShadowSize(in ControlTokenDefaultContext context) => context.Global.LineWidth;

    [ControlTokenDefault(nameof(ShadowColor))]
    private static Color DefaultForShadowColor(in ControlTokenDefaultContext context) => context.Global.ColorBorderBg;

    [ControlTokenDefault(nameof(TextPadding))]
    private static Thickness DefaultForTextPadding(in ControlTokenDefaultContext context) => new(context.Global.UniformlyPaddingXXS, 0);

    [ControlTokenDefault(nameof(CornerRadius), nameof(IndicatorHeight))]
    private static CornerRadius DefaultForCornerRadius(in ControlTokenDefaultContext context, double indicatorHeight) => new(indicatorHeight);

    [ControlTokenDefault(nameof(CornerRadiusSM), nameof(IndicatorHeightSM))]
    private static CornerRadius DefaultForCornerRadiusSM(in ControlTokenDefaultContext context, double indicatorHeightSM) => new(indicatorHeightSM);
}
