using AtomUI.Theme.DesignTokens;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Design tokens of the <see cref="BorderBeam" /> control. They set the length and the opacity of the beam,
/// the time of one loop around the border and the colored share of its gradient.
/// </summary>
[ControlDesignToken]
public sealed partial class BorderBeamToken : AbstractControlDesignToken
{

    /// <summary>
    /// Length of the painted beam along the border path.
    /// </summary>
    public partial double BeamSize { get; set; }

    /// <summary>
    /// Opacity multiplier applied to the beam along the border.
    /// </summary>
    public partial double BeamOpacity { get; set; }

    /// <summary>
    /// Time the beam takes to complete one trip around the border.
    /// </summary>
    public partial TimeSpan MotionDuration { get; set; }

    /// <summary>
    /// Share of the beam length, from 0 to 100 percent, that is colored; the rest of the gradient fades to
    /// transparent.
    /// </summary>
    public partial double MaxVisibleStopPercent { get; set; }

}
