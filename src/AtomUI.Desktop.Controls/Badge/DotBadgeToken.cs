using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken(ControlTokenEvaluationVersion.DependencyDefaultsV2, Revision = 1)]
internal sealed partial class DotBadgeToken : AbstractControlDesignToken, IControlTokenDefaults
{
    public double DotSize { get; set; }

    public Color DotColor { get; set; }

    public double ShadowSize { get; set; }

    public Color ShadowColor { get; set; }

    public Thickness LabelMargin { get; set; }

    [ControlTokenDefault(nameof(DotSize))]
    private static double DefaultForDotSize(in ControlTokenDefaultContext context) => context.Global.FontSizeSM / 2;

    [ControlTokenDefault(nameof(DotColor))]
    private static Color DefaultForDotColor(in ControlTokenDefaultContext context) => context.Global.ColorError;

    [ControlTokenDefault(nameof(ShadowSize))]
    private static double DefaultForShadowSize(in ControlTokenDefaultContext context) => context.Global.LineWidth;

    [ControlTokenDefault(nameof(ShadowColor))]
    private static Color DefaultForShadowColor(in ControlTokenDefaultContext context) => context.Global.ColorBorderBg;

    [ControlTokenDefault(nameof(LabelMargin))]
    private static Thickness DefaultForLabelMargin(in ControlTokenDefaultContext context) => new(context.Global.UniformlyMarginXS, 0, 0, 0);
}
