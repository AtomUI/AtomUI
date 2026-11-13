using AtomUI.Theme.DesignTokens;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class BorderBeamToken : AbstractControlDesignToken
{

    public partial double BeamSize { get; set; }

    public partial double BeamOpacity { get; set; }

    public partial TimeSpan MotionDuration { get; set; }

    public partial double MaxVisibleStopPercent { get; set; }

    public BorderBeamToken()

    {
    }

    protected override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        BeamSize              = 100d;
        BeamOpacity           = 0.95d;
        MotionDuration        = TimeSpan.FromSeconds(6);
        MaxVisibleStopPercent = 70d;
    }

}
