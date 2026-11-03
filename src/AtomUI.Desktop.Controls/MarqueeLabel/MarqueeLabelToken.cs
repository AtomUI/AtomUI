using AtomUI.Theme.DesignTokens;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken(ControlTokenEvaluationVersion.DependencyDefaultsV2, Revision = 1)]
internal sealed partial class MarqueeLabelToken : AbstractControlDesignToken, IControlTokenDefaults
{
    
    /// <summary>
    /// 周期这件的间隔
    /// </summary>
    public double CycleSpace { get; set; }

    /// <summary>
    /// 默认速度，像素每秒
    /// </summary>
    public double DefaultSpeed { get; set; }

    [ControlTokenDefault(nameof(CycleSpace))]
    private static double DefaultForCycleSpace(in ControlTokenDefaultContext context) => 200;

    [ControlTokenDefault(nameof(DefaultSpeed))]
    private static double DefaultForDefaultSpeed(in ControlTokenDefaultContext context) => 150;
    
}
