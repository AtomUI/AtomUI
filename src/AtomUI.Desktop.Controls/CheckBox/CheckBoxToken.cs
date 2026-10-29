using AtomUI.Theme.DesignTokens;
using Avalonia;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken(ControlTokenEvaluationVersion.DependencyDefaultsV2, Revision = 1)]
internal sealed partial class CheckBoxToken : AbstractControlDesignToken, IControlTokenDefaults
{
    public double CheckIndicatorSize { get; set; }
    public double CheckedMarkSize { get; set; }
    public double IndicatorTristateMarkSize { get; set; }
    public Thickness TextMargin { get; set; }

    [ControlTokenDefault(nameof(CheckIndicatorSize))]
    private static double DefaultForCheckIndicatorSize(in ControlTokenDefaultContext context) =>
        context.Global.ControlInteractiveSize;

    [ControlTokenDefault(nameof(CheckedMarkSize), nameof(CheckIndicatorSize))]
    private static double DefaultForCheckedMarkSize(in ControlTokenDefaultContext context, double checkIndicatorSize) =>
        checkIndicatorSize * 0.6;

    [ControlTokenDefault(nameof(IndicatorTristateMarkSize))]
    private static double DefaultForIndicatorTristateMarkSize(in ControlTokenDefaultContext context) =>
        context.Global.FontSizeLG / 2;

    [ControlTokenDefault(nameof(TextMargin))]
    private static Thickness DefaultForTextMargin(in ControlTokenDefaultContext context) =>
        new(context.Global.UniformlyMarginXS, 0, context.Global.UniformlyMarginXS, 0);
}
