using AtomUI.Theme.DesignTokens;
using Avalonia.Animation.Easings;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken(ControlTokenEvaluationVersion.DependencyDefaultsV2, Revision = 2)]
internal sealed partial class OtpLineEditToken : AbstractControlDesignToken, IControlTokenDefaults
{
    public double CellWidth { get; set; }

    public double CellWidthLG { get; set; }

    public double CellWidthSM { get; set; }

    public double CellGap { get; set; }

    public TimeSpan CaretBlinkDuration { get; set; }

    public Easing CaretBlinkEasing { get; set; } = new LinearEasing();

    [ControlTokenDefault(nameof(CellWidth))]
    private static double DefaultForCellWidth(in ControlTokenDefaultContext context) =>
        context.Global.ControlHeight;

    [ControlTokenDefault(nameof(CellWidthLG))]
    private static double DefaultForCellWidthLG(in ControlTokenDefaultContext context) =>
        context.Global.ControlHeightLG;

    [ControlTokenDefault(nameof(CellWidthSM))]
    private static double DefaultForCellWidthSM(in ControlTokenDefaultContext context) =>
        context.Global.ControlHeightSM;

    [ControlTokenDefault(nameof(CellGap))]
    private static double DefaultForCellGap(in ControlTokenDefaultContext context) =>
        context.Global.UniformlyPaddingXXS;

    [ControlTokenDefault(nameof(CaretBlinkDuration))]
    private static TimeSpan DefaultForCaretBlinkDuration(in ControlTokenDefaultContext context) =>
        TimeSpan.FromSeconds(1);

    [ControlTokenDefault(nameof(CaretBlinkEasing))]
    private static Easing DefaultForCaretBlinkEasing(in ControlTokenDefaultContext context) =>
        new LinearEasing();
}
