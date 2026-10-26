using AtomUI.Theme.DesignTokens;
using Avalonia;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken(ControlTokenEvaluationVersion.DependencyDefaultsV2, Revision = 1)]
internal sealed partial class TreeTransferToken : AbstractControlDesignToken, IControlTokenDefaults
{
    public double ListWidth { get; set; }

    public double ListWidthLG { get; set; }

    public double ListHeight { get; set; }

    public double HeaderHeight { get; set; }

    public Thickness HeaderPadding { get; set; }

    [ControlTokenDefault(nameof(ListWidth))]
    private static double DefaultForListWidth(in ControlTokenDefaultContext context) => 180;

    [ControlTokenDefault(nameof(ListWidthLG))]
    private static double DefaultForListWidthLG(in ControlTokenDefaultContext context) => 250;

    [ControlTokenDefault(nameof(ListHeight))]
    private static double DefaultForListHeight(in ControlTokenDefaultContext context) => 200;

    [ControlTokenDefault(nameof(HeaderHeight))]
    private static double DefaultForHeaderHeight(in ControlTokenDefaultContext context) =>
        context.Global.ControlHeightLG;

    [ControlTokenDefault(nameof(HeaderPadding))]
    private static Thickness DefaultForHeaderPadding(in ControlTokenDefaultContext context) =>
        new(context.Global.UniformlyPaddingSM,
            Math.Ceiling((context.Global.ControlHeightLG -
                          context.Global.LineWidth -
                          context.Global.FontHeight) / 2));
}
