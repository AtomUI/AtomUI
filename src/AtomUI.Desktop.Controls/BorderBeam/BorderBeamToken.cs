using AtomUI.Theme.DesignTokens;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class BorderBeamToken : AbstractControlDesignToken
{

    /// <summary>Length of the painted beam along the border path.</summary>
    public partial double BeamSize { get; set; }

    /// <summary>Opacity multiplier applied to the beam along the border.</summary>
    public partial double BeamOpacity { get; set; }

    /// <summary>Time the beam takes to complete one trip around the border.</summary>
    public partial TimeSpan MotionDuration { get; set; }

    /// <summary>Upper limit on the gradient stop position used to keep the beam visible along its path.</summary>
    public partial double MaxVisibleStopPercent { get; set; }

}
