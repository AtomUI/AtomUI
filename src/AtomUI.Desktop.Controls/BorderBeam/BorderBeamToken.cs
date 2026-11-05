using AtomUI.Theme.DesignTokens;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken(ControlTokenEvaluationVersion.DependencyDefaultsV2, Revision = 1)]
internal sealed partial class BorderBeamToken : AbstractControlDesignToken, IControlTokenDefaults
{

    public double BeamSize { get; set; }

    public double BeamOpacity { get; set; }

    public TimeSpan MotionDuration { get; set; }

    public double MaxVisibleStopPercent { get; set; }

    [ControlTokenDefault(nameof(BeamSize))]
    private static double DefaultForBeamSize(in ControlTokenDefaultContext context) =>
        100d;

    [ControlTokenDefault(nameof(BeamOpacity))]
    private static double DefaultForBeamOpacity(in ControlTokenDefaultContext context) =>
        0.95d;

    [ControlTokenDefault(nameof(MotionDuration))]
    private static TimeSpan DefaultForMotionDuration(in ControlTokenDefaultContext context) =>
        TimeSpan.FromSeconds(6);

    [ControlTokenDefault(nameof(MaxVisibleStopPercent))]
    private static double DefaultForMaxVisibleStopPercent(in ControlTokenDefaultContext context) =>
        70d;

}
